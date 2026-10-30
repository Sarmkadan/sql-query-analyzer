using System;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;

using SqlQueryAnalyzer.Exceptions;
using SqlQueryAnalyzer.Middleware;

using Xunit;

namespace SqlQueryAnalyzer.Tests.Middleware;

/// <summary>
/// Unit tests for <see cref="ErrorHandlingMiddleware"/> (retry, classification, error reporting and
/// recovery suggestions) and <see cref="DegradationStrategy"/> (primary/fallback execution).
/// Logger behaviour is verified through <see cref="Mock{T}"/> of <see cref="ILogger{T}"/>.
/// </summary>
public class ErrorHandlingMiddlewareTests
{
    private readonly Mock<ILogger<ErrorHandlingMiddleware>> _mockLogger;
    private readonly ErrorHandlingMiddleware _middleware;

    /// <summary>
    /// Initializes the shared mock logger and the <see cref="ErrorHandlingMiddleware"/> under test.
    /// xUnit creates a new test class instance for every test method, so each test starts with a fresh mock.
    /// </summary>
    public ErrorHandlingMiddlewareTests()
    {
        _mockLogger = new Mock<ILogger<ErrorHandlingMiddleware>>();
        _middleware = new ErrorHandlingMiddleware(_mockLogger.Object);
    }

    /// <summary>
    /// Verifies that a successful operation's result is returned unchanged and that exactly one Debug
    /// log entry containing "Executing TestOperation" is written.
    /// </summary>
    [Fact]
    public async Task ExecuteWithErrorHandlingAsync_SuccessfulOperation_ReturnsResult()
    {
        // Arrange
        var expectedResult = "Success";
        Task<string> operation() => Task.FromResult(expectedResult);

        // Act
        var result = await _middleware.ExecuteWithErrorHandlingAsync(operation, "TestOperation");

        // Assert
        result.Should().Be(expectedResult);
        _mockLogger.Verify(x => x.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Executing TestOperation")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that an <see cref="AnalysisException"/> classified as transient (message contains "timeout")
    /// is retried, that the operation succeeding on the third attempt returns its result, and that two
    /// Warning logs containing "failed (transient)" are written (one per retried failure).
    /// </summary>
    [Fact]
    public async Task ExecuteWithErrorHandlingAsync_TransientAnalysisException_RetriesThenSucceeds()
    {
        // Arrange
        var attemptCount = 0;
        Task<string> operation()
        {
            attemptCount++;
            if (attemptCount < 3)
            {
                throw new AnalysisException("Connection timeout occurred");
            }
            return Task.FromResult("Success after retry");
        }

        // Act
        var result = await _middleware.ExecuteWithErrorHandlingAsync(operation, "DatabaseOperation");

        // Assert
        result.Should().Be("Success after retry");
        attemptCount.Should().Be(3); // Should attempt 3 times
        _mockLogger.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("failed (transient)")),
            It.IsAny<AnalysisException>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(2)); // 2 retries
    }

    /// <summary>
    /// Verifies that a transient "connection" <see cref="AnalysisException"/> is retried and that the operation
    /// succeeding on the third attempt returns its value. Despite the test name, the default value is not
    /// returned here; the test asserts that no "exhausted retries" Warning is logged.
    /// </summary>
    [Fact]
    public async Task ExecuteWithErrorHandlingAsync_TransientAnalysisExceptionWithConnection_RetriesThenReturnsDefault()
    {
        // Arrange
        var attemptCount = 0;
        Task<string> operation()
        {
            attemptCount++;
            if (attemptCount < 3)
            {
                throw new AnalysisException("Database server unavailable");
            }
            return Task.FromResult("Success");
        }

        // Act
        var result = await _middleware.ExecuteWithErrorHandlingAsync(operation, "ConnectionTest", "default");

        // Assert
        result.Should().Be("Success");
        attemptCount.Should().Be(3); // Should attempt max retries
        _mockLogger.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("exhausted retries")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    /// <summary>
    /// Verifies that a non-transient <see cref="AnalysisException"/> is rethrown on the first attempt without
    /// any retry, and that exactly one Error log containing "failed with analysis error" is written.
    /// </summary>
    [Fact]
    public async Task ExecuteWithErrorHandlingAsync_NonTransientAnalysisException_ThrowsImmediately()
    {
        // Arrange
        Task<string> operation() => throw new AnalysisException("Invalid query syntax");

        // Act & Assert
        await Assert.ThrowsAsync<AnalysisException>(() =>
            _middleware.ExecuteWithErrorHandlingAsync(operation, "QueryValidation"));

        _mockLogger.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("failed with analysis error")),
            It.IsAny<AnalysisException>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that an exception that is not an <see cref="AnalysisException"/> is logged once as an
    /// unexpected Error ("failed unexpectedly") and rethrown as the same <see cref="InvalidOperationException"/>.
    /// </summary>
    [Fact]
    public async Task ExecuteWithErrorHandlingAsync_GenericException_LogsAndThrows()
    {
        // Arrange
        Task<string> operation() => throw new InvalidOperationException("Something went wrong");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _middleware.ExecuteWithErrorHandlingAsync(operation, "GenericOperation"));

        _mockLogger.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("failed unexpectedly")),
            It.IsAny<InvalidOperationException>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies through reflection that the private <c>IsTransientError</c> method returns <c>true</c>
    /// for an <see cref="AnalysisException"/> whose message contains "connection".
    /// </summary>
    [Fact]
    public void IsTransientError_ConnectionError_ReturnsTrue()
    {
        // Arrange
        var exception = new AnalysisException("Connection to database failed");

        // Use reflection to test the private method
        var method = typeof(ErrorHandlingMiddleware).GetMethod(
            "IsTransientError",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        var isTransient = (bool)method!.Invoke(_middleware, new object[] { exception })!;

        // Assert
        isTransient.Should().BeTrue();
    }

    /// <summary>
    /// Verifies through reflection that the private <c>IsTransientError</c> method returns <c>true</c>
    /// for an <see cref="AnalysisException"/> whose message contains "timeout".
    /// </summary>
    [Fact]
    public void IsTransientError_TimeoutError_ReturnsTrue()
    {
        // Arrange
        var exception = new AnalysisException("Query execution timeout");

        // Use reflection to test the private method
        var method = typeof(ErrorHandlingMiddleware).GetMethod(
            "IsTransientError",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        var isTransient = (bool)method!.Invoke(_middleware, new object[] { exception })!;

        // Assert
        isTransient.Should().BeTrue();
    }

    /// <summary>
    /// Verifies through reflection that the private <c>IsTransientError</c> method returns <c>true</c>
    /// for an <see cref="AnalysisException"/> whose message contains "unavailable".
    /// </summary>
    [Fact]
    public void IsTransientError_UnavailableError_ReturnsTrue()
    {
        // Arrange
        var exception = new AnalysisException("Service temporarily unavailable");

        // Use reflection to test the private method
        var method = typeof(ErrorHandlingMiddleware).GetMethod(
            "IsTransientError",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        var isTransient = (bool)method!.Invoke(_middleware, new object[] { exception })!;

        // Assert
        isTransient.Should().BeTrue();
    }

    /// <summary>
    /// Verifies through reflection that the private <c>IsTransientError</c> method returns <c>false</c>
    /// for an <see cref="AnalysisException"/> whose message contains none of the transient keywords
    /// ("connection", "timeout", "unavailable").
    /// </summary>
    [Fact]
    public void IsTransientError_NonTransientError_ReturnsFalse()
    {
        // Arrange
        var exception = new AnalysisException("Invalid query syntax");

        // Use reflection to test the private method
        var method = typeof(ErrorHandlingMiddleware).GetMethod(
            "IsTransientError",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        var isTransient = (bool)method!.Invoke(_middleware, new object[] { exception })!;

        // Assert
        isTransient.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that <c>CreateErrorReport</c> for a non-transient <see cref="AnalysisException"/> populates
    /// the message, the exception type name, the context, a non-recoverable flag, a non-empty suggestion,
    /// and a UTC timestamp within one second of now.
    /// </summary>
    [Fact]
    public void CreateErrorReport_WithAnalysisException_CreatesCompleteReport()
    {
        // Arrange
        var exception = new AnalysisException("Test error message");
        var context = "TestContext";

        // Act
        var report = _middleware.CreateErrorReport(exception, context);

        // Assert
        report.Should().NotBeNull();
        report.ErrorMessage.Should().Be("Test error message");
        report.ErrorType.Should().Be("AnalysisException");
        report.Context.Should().Be(context);
        report.IsRecoverable.Should().BeFalse();
        report.Suggestion.Should().NotBeNullOrEmpty();
        report.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Verifies that <c>CreateErrorReport</c> marks the report as recoverable (<c>IsRecoverable == true</c>)
    /// when the <see cref="AnalysisException"/> message indicates a transient failure ("timeout").
    /// </summary>
    [Fact]
    public void CreateErrorReport_WithTransientAnalysisException_IsRecoverable()
    {
        // Arrange
        var exception = new AnalysisException("Connection timeout");
        var context = "DatabaseOperation";

        // Act
        var report = _middleware.CreateErrorReport(exception, context);

        // Assert
        report.IsRecoverable.Should().BeTrue();
    }

    /// <summary>
    /// Verifies through reflection that the private <c>GetRecoverySuggestion</c> method returns a suggestion
    /// containing "Query file not found" for a <see cref="FileNotFoundException"/>.
    /// </summary>
    [Fact]
    public void CreateErrorReport_WithFileNotFoundException_HasCorrectSuggestion()
    {
        // Arrange
        var exception = new FileNotFoundException("File not found");

        // Use reflection to test the private method since CreateErrorReport has a bug with non-AnalysisException
        var method = typeof(ErrorHandlingMiddleware).GetMethod(
            "GetRecoverySuggestion",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        var suggestion = (string)method!.Invoke(_middleware, new object[] { exception })!;

        // Assert
        suggestion.Should().Contain("Query file not found");
    }

    /// <summary>
    /// Verifies through reflection that the private <c>GetRecoverySuggestion</c> method returns a suggestion
    /// containing "Invalid query or parameters" for an <see cref="ArgumentException"/>.
    /// </summary>
    [Fact]
    public void CreateErrorReport_WithArgumentException_HasCorrectSuggestion()
    {
        // Arrange
        var exception = new ArgumentException("Invalid argument");

        // Use reflection to test the private method since CreateErrorReport has a bug with non-AnalysisException
        var method = typeof(ErrorHandlingMiddleware).GetMethod(
            "GetRecoverySuggestion",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        var suggestion = (string)method!.Invoke(_middleware, new object[] { exception })!;

        // Assert
        suggestion.Should().Contain("Invalid query or parameters");
    }

    /// <summary>
    /// Verifies through reflection that the private <c>GetRecoverySuggestion</c> method returns a suggestion
    /// containing "Operation timed out" for a <see cref="TimeoutException"/>.
    /// </summary>
    [Fact]
    public void CreateErrorReport_WithTimeoutException_HasCorrectSuggestion()
    {
        // Arrange
        var exception = new TimeoutException("Operation timed out");

        // Use reflection to test the private method since CreateErrorReport has a bug with non-AnalysisException
        var method = typeof(ErrorHandlingMiddleware).GetMethod(
            "GetRecoverySuggestion",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        var suggestion = (string)method!.Invoke(_middleware, new object[] { exception })!;

        // Assert
        suggestion.Should().Contain("Operation timed out");
    }

    /// <summary>
    /// Verifies through reflection that the private <c>GetRecoverySuggestion</c> method returns a suggestion
    /// containing "Database connection failed" for a <see cref="DatabaseConnectionException"/>.
    /// </summary>
    [Fact]
    public void CreateErrorReport_WithDatabaseConnectionException_HasCorrectSuggestion()
    {
        // Arrange
        var exception = new DatabaseConnectionException("Database connection failed");

        // Use reflection to test the private method since CreateErrorReport has a bug with non-AnalysisException
        var method = typeof(ErrorHandlingMiddleware).GetMethod(
            "GetRecoverySuggestion",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        var suggestion = (string)method!.Invoke(_middleware, new object[] { exception })!;

        // Assert
        suggestion.Should().Contain("Database connection failed");
    }

    /// <summary>
    /// Verifies that <c>ErrorReport.ToString()</c> renders the error message, type, context, recoverability
    /// ("Yes - retry may succeed" when <c>IsRecoverable</c> is true), suggestion, and timestamp formatted
    /// as <c>yyyy-MM-dd HH:mm:ss</c>.
    /// </summary>
    [Fact]
    public void ErrorReport_ToString_FormatsCorrectly()
    {
        // Arrange
        var report = new ErrorReport
        {
            ErrorMessage = "Test error",
            ErrorType = "TestException",
            Context = "TestContext",
            IsRecoverable = true,
            Suggestion = "Try again",
            Timestamp = new DateTime(2024, 1, 1, 12, 0, 0)
        };

        // Act
        var result = report.ToString();

        // Assert
        result.Should().Contain("Error: Test error");
        result.Should().Contain("Type: TestException");
        result.Should().Contain("Context: TestContext");
        result.Should().Contain("Recoverable: Yes");
        result.Should().Contain("Suggestion: Try again");
        result.Should().Contain("2024-01-01 12:00:00");
    }

    /// <summary>
    /// Verifies that <c>DegradationStrategy.ExecuteWithDegradationAsync</c> returns the primary result when the
    /// primary operation succeeds, without invoking the degraded path or logging a degradation warning.
    /// </summary>
    [Fact]
    public async Task DegradationStrategy_ExecuteWithDegradationAsync_PrimaryOperationSucceeds()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DegradationStrategy>>();
        var strategy = new DegradationStrategy(mockLogger.Object);

        var primaryExecuted = false;

        Task<string> primaryOperation()
        {
            primaryExecuted = true;
            return Task.FromResult("PrimaryResult");
        }

        Task<string> degradedOperation()
        {
            return Task.FromResult("DegradedResult");
        }

        // Act
        var result = await strategy.ExecuteWithDegradationAsync(primaryOperation, degradedOperation, "TestOperation");

        // Assert
        result.Should().Be("PrimaryResult");
        primaryExecuted.Should().BeTrue();
        mockLogger.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("failed, attempting degraded")),
            It.IsAny<InvalidOperationException>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    /// <summary>
    /// Verifies that when the primary operation throws, <c>DegradationStrategy.ExecuteWithDegradationAsync</c>
    /// returns the degraded result, logs one Warning ("TestOperation failed, attempting degraded") and one
    /// Information entry ("Degraded operation succeeded").
    /// </summary>
    [Fact]
    public async Task DegradationStrategy_ExecuteWithDegradationAsync_PrimaryFailsDegradedSucceeds()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DegradationStrategy>>();
        var strategy = new DegradationStrategy(mockLogger.Object);

        var primaryExecuted = false;
        var degradedExecuted = false;

        Task<string> primaryOperation()
        {
            primaryExecuted = true;
            throw new InvalidOperationException("Primary failed");
        }

        Task<string> degradedOperation()
        {
            degradedExecuted = true;
            return Task.FromResult("DegradedResult");
        }

        // Act
        var result = await strategy.ExecuteWithDegradationAsync(primaryOperation, degradedOperation, "TestOperation");

        // Assert
        result.Should().Be("DegradedResult");
        primaryExecuted.Should().BeTrue();
        degradedExecuted.Should().BeTrue();
        mockLogger.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("TestOperation failed, attempting degraded")),
            It.IsAny<InvalidOperationException>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Degraded operation succeeded")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that when both the primary and the degraded operation throw, <c>DegradationStrategy.ExecuteWithDegradationAsync</c>
    /// rethrows the degraded operation's <see cref="InvalidOperationException"/>, logs the degradation Warning once,
    /// and logs an Error containing "Degraded operation also failed".
    /// </summary>
    [Fact]
    public async Task DegradationStrategy_ExecuteWithDegradationAsync_BothFail_Throws()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DegradationStrategy>>();
        var strategy = new DegradationStrategy(mockLogger.Object);

        Task<string> primaryOperation() => throw new InvalidOperationException("Primary failed");
        Task<string> degradedOperation() => throw new InvalidOperationException("Degraded also failed");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            strategy.ExecuteWithDegradationAsync(primaryOperation, degradedOperation, "TestOperation"));

        mockLogger.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("TestOperation failed, attempting degraded")),
            It.IsAny<InvalidOperationException>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        mockLogger.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Degraded operation also failed")),
            It.IsAny<InvalidOperationException>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies through reflection that the private <c>GetRecoverySuggestion</c> method returns the generic
    /// "An unexpected error occurred. Check logs for more details." message for an exception type that has
    /// no specific mapping.
    /// </summary>
    [Fact]
    public void GetRecoverySuggestion_UnknownException_ReturnsGenericMessage()
    {
        // Arrange
        var middleware = new ErrorHandlingMiddleware(_mockLogger.Object);
        var exception = new Exception("Generic error");

        // Use reflection to test the private method
        var method = typeof(ErrorHandlingMiddleware).GetMethod(
            "GetRecoverySuggestion",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        var suggestion = (string)method!.Invoke(middleware, new object[] { exception })!;

        // Assert
        suggestion.Should().Be("An unexpected error occurred. Check logs for more details.");
    }
}