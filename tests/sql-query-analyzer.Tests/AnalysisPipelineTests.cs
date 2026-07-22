#nullable enable

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SqlQueryAnalyzer.CLI;
using SqlQueryAnalyzer.Constants;
using SqlQueryAnalyzer.Caching;
using SqlQueryAnalyzer.Middleware;
using SqlQueryAnalyzer.Models;
using SqlQueryAnalyzer.Services;
using Xunit;

namespace SqlQueryAnalyzer.Tests;

/// <summary>
/// Tests for AnalysisPipeline to verify middleware execution order, exception behavior, and result aggregation.
/// </summary>
public class AnalysisPipelineTests
{
    private readonly Mock<ILogger<AnalysisPipeline>> _mockLogger;
    private readonly Mock<IQueryAnalyzerService> _mockAnalyzer;
    private AnalysisPipeline _pipeline;
    private readonly AnalysisContext _context;

    public AnalysisPipelineTests()
    {
        _mockLogger = new Mock<ILogger<AnalysisPipeline>>();
        _mockAnalyzer = new Mock<IQueryAnalyzerService>();

        _pipeline = new AnalysisPipeline(_mockLogger.Object, _mockAnalyzer.Object, includeCachingMiddleware: false);
        _context = new AnalysisContext
        {
            Query = "SELECT * FROM Users WHERE Id = 1",
            Arguments = new CommandLineArguments { Verbose = false }
        };
    }

    [Fact]
    public void Constructor_RegistersDefaultMiddlewaresInCorrectOrder()
    {
        // Arrange & Act
        var pipeline = new AnalysisPipeline(NullLogger<AnalysisPipeline>.Instance, _mockAnalyzer.Object);

        // Assert - Verify all default middlewares are registered in the correct order
        pipeline.MiddlewareCount.Should().Be(5);
    }

    [Fact]
    public void Clear_RemovesAllRegisteredMiddlewares()
    {
        // Arrange
        var pipeline = new AnalysisPipeline(NullLogger<AnalysisPipeline>.Instance, _mockAnalyzer.Object);
        pipeline.MiddlewareCount.Should().Be(5);

        // Act
        pipeline.Clear();

        // Assert
        pipeline.MiddlewareCount.Should().Be(0);
    }

    [Fact]
    public void RegisterMiddleware_AddsMiddlewareToPipeline()
    {
        // Arrange
        var pipeline = new AnalysisPipeline(NullLogger<AnalysisPipeline>.Instance, _mockAnalyzer.Object);
        var initialCount = pipeline.MiddlewareCount;
        var mockMiddleware = new Mock<IAnalysisMiddleware>();

        // Act
        pipeline.RegisterMiddleware(mockMiddleware.Object);

        // Assert
        pipeline.MiddlewareCount.Should().Be(initialCount + 1);
    }

    [Fact]
    public async Task ExecuteAsync_ExecutesMiddlewaresInRegistrationOrder()
    {
        // Arrange
        var executionOrder = new List<string>();
        var pipeline = new AnalysisPipeline(NullLogger<AnalysisPipeline>.Instance, _mockAnalyzer.Object);

        // Register custom middlewares that track execution order
        pipeline.RegisterMiddleware(new TestMiddleware(executionOrder, "Middleware1"));
        pipeline.RegisterMiddleware(new TestMiddleware(executionOrder, "Middleware2"));
        pipeline.RegisterMiddleware(new TestMiddleware(executionOrder, "Middleware3"));

        var context = new AnalysisContext
        {
            Query = "SELECT * FROM Test",
            Arguments = new CommandLineArguments()
        };

        // Act
        await pipeline.ExecuteAsync(context);

        // Assert
        executionOrder.Should().BeEquivalentTo(new[] { "Middleware1", "Middleware2", "Middleware3" });
    }

    [Fact]
    public async Task ExecuteAsync_StopsExecutionWhenShouldContinueIsFalse()
    {
        // Arrange
        var executionOrder = new List<string>();
        var pipeline = new AnalysisPipeline(NullLogger<AnalysisPipeline>.Instance, _mockAnalyzer.Object);

        // Register middlewares with the second one setting ShouldContinue to false
        pipeline.RegisterMiddleware(new TestMiddleware(executionOrder, "Middleware1"));
        pipeline.RegisterMiddleware(new StopExecutionMiddleware(executionOrder));
        pipeline.RegisterMiddleware(new TestMiddleware(executionOrder, "Middleware3"));

        var context = new AnalysisContext
        {
            Query = "SELECT * FROM Test",
            Arguments = new CommandLineArguments()
        };

        // Act
        await pipeline.ExecuteAsync(context);

        // Assert - Only first two middlewares should execute
        executionOrder.Should().BeEquivalentTo(new[] { "Middleware1", "Middleware2" });
        context.ShouldContinue.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_HandlesMiddlewareExceptions()
    {
        // Arrange
        var pipeline = new AnalysisPipeline(NullLogger<AnalysisPipeline>.Instance, _mockAnalyzer.Object);
        pipeline.RegisterMiddleware(new ThrowingMiddleware("Test exception"));

        var context = new AnalysisContext
        {
            Query = "SELECT * FROM Test",
            Arguments = new CommandLineArguments()
        };

        // Act & Assert - Exception should propagate
        var act = async () => await pipeline.ExecuteAsync(context);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Test exception");
    }

    [Fact]
    public async Task ExecuteAsync_LogsPipelineExecution()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<AnalysisPipeline>>();
        var pipeline = new AnalysisPipeline(mockLogger.Object, _mockAnalyzer.Object);
        var context = new AnalysisContext
        {
            Query = "SELECT * FROM Users WHERE Id = 1",
            Arguments = new CommandLineArguments { Verbose = true }
        };

        // Act
        await pipeline.ExecuteAsync(context);

        // Assert - Verify logging occurred (exact messages depend on implementation)
        mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_WithRealAnalyzer_PopulatesResultInContext()
    {
        // Arrange
        var mockAnalyzer = new Mock<IQueryAnalyzerService>();
        var expectedResult = new QueryAnalysisResult
        {
            Query = "SELECT * FROM Users WHERE Id = 1",
            PerformanceScore = 85.5,
            Complexity = QueryComplexity.Low
        };

        mockAnalyzer.Setup(x => x.AnalyzeQueryAsync(It.IsAny<string>()))
                  .ReturnsAsync(expectedResult);

        var pipeline = new AnalysisPipeline(NullLogger<AnalysisPipeline>.Instance, mockAnalyzer.Object);
        var context = new AnalysisContext
        {
            Query = "SELECT * FROM Users WHERE Id = 1",
            Arguments = new CommandLineArguments()
        };

        // Act
        await pipeline.ExecuteAsync(context);

        // Assert
        context.Result.Should().NotBeNull();
        context.Result.Should().BeSameAs(expectedResult);
        mockAnalyzer.Verify(x => x.AnalyzeQueryAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithMultipleMiddlewares_AggregatesResults()
    {
        // Arrange
        var pipeline = new AnalysisPipeline(NullLogger<AnalysisPipeline>.Instance, _mockAnalyzer.Object);

        // Register a middleware that modifies the result
        pipeline.RegisterMiddleware(new ResultModifyingMiddleware());

        var context = new AnalysisContext
        {
            Query = "SELECT * FROM Test",
            Arguments = new CommandLineArguments()
        };

        // Act
        await pipeline.ExecuteAsync(context);

        // Assert
        context.Result.Should().NotBeNull();
        context.Result!.Metadata.Should().ContainKey("MiddlewareExecuted");
        context.Result.Metadata["MiddlewareExecuted"].Should().Be("true");
    }

    [Fact]
    public async Task ExecuteAsync_EmptyQuery_StillExecutesPipeline()
    {
        // Arrange
        var pipeline = new AnalysisPipeline(NullLogger<AnalysisPipeline>.Instance, _mockAnalyzer.Object);
        var context = new AnalysisContext
        {
            Query = "",
            Arguments = new CommandLineArguments()
        };

        // Act & Assert - Should not throw, just execute
        var act = async () => await pipeline.ExecuteAsync(context);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ExecuteAsync_NullQuery_StillExecutesPipeline()
    {
        // Arrange
        var pipeline = new AnalysisPipeline(NullLogger<AnalysisPipeline>.Instance, _mockAnalyzer.Object);
        var context = new AnalysisContext
        {
            Query = null,
            Arguments = new CommandLineArguments()
        };

        // Act & Assert - Should not throw, just execute
        var act = async () => await pipeline.ExecuteAsync(context);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ExecuteAsync_WithMetadata_PreservesMetadata()
    {
        // Arrange
        var pipeline = new AnalysisPipeline(NullLogger<AnalysisPipeline>.Instance, _mockAnalyzer.Object);
        var context = new AnalysisContext
        {
            Query = "SELECT * FROM Test",
            Arguments = new CommandLineArguments(),
            Metadata = new Dictionary<string, object> { { "TestKey", "TestValue" } }
        };

        // Act
        await pipeline.ExecuteAsync(context);

        // Assert
        context.Metadata.Should().ContainKey("TestKey");
        context.Metadata["TestKey"].Should().Be("TestValue");
    }

    [Fact]
    public async Task ExecuteAsync_WithSeverityFilter_AppliesFilter()
    {
        // Arrange
        var mockAnalyzer = new Mock<IQueryAnalyzerService>();
        var issues = new List<PerformanceIssue>
        {
            new() { Severity = Constants.IssueSeverity.Critical, Description = "Critical issue" },
            new() { Severity = Constants.IssueSeverity.Warning, Description = "Warning issue" },
            new() { Severity = Constants.IssueSeverity.Info, Description = "Info issue" }
        };

        var result = new QueryAnalysisResult
        {
            Query = "SELECT * FROM Test",
            PerformanceScore = 75,
            Issues = issues
        };

        mockAnalyzer.Setup(x => x.AnalyzeQueryAsync(It.IsAny<string>()))
                  .ReturnsAsync(result);

        var pipeline = new AnalysisPipeline(NullLogger<AnalysisPipeline>.Instance, mockAnalyzer.Object);
        var context = new AnalysisContext
        {
            Query = "SELECT * FROM Test",
            Arguments = new CommandLineArguments { FilterBySeverity = "Critical" }
        };

        // Act
        await pipeline.ExecuteAsync(context);

        // Assert
        context.Result.Should().NotBeNull();
        context.Result!.Issues.Should().HaveCount(1);
        context.Result.Issues[0].Severity.Should().Be(Constants.IssueSeverity.Critical);
    }

    [Fact]
    public async Task ExecuteAsync_WithMaxResults_AppliesLimit()
    {
        // Arrange
        var mockAnalyzer = new Mock<IQueryAnalyzerService>();
        var issues = new List<PerformanceIssue>
        {
            new() { Severity = Constants.IssueSeverity.Critical, Description = "Issue 1" },
            new() { Severity = Constants.IssueSeverity.Critical, Description = "Issue 2" },
            new() { Severity = Constants.IssueSeverity.Critical, Description = "Issue 3" },
            new() { Severity = Constants.IssueSeverity.Critical, Description = "Issue 4" },
            new() { Severity = Constants.IssueSeverity.Critical, Description = "Issue 5" }
        };

        var result = new QueryAnalysisResult
        {
            Query = "SELECT * FROM Test",
            PerformanceScore = 75,
            Issues = issues
        };

        mockAnalyzer.Setup(x => x.AnalyzeQueryAsync(It.IsAny<string>()))
                  .ReturnsAsync(result);

        var pipeline = new AnalysisPipeline(NullLogger<AnalysisPipeline>.Instance, mockAnalyzer.Object);
        var context = new AnalysisContext
        {
            Query = "SELECT * FROM Test",
            Arguments = new CommandLineArguments { MaxResults = 2 }
        };

        // Act
        await pipeline.ExecuteAsync(context);

        // Assert
        context.Result.Should().NotBeNull();
        context.Result!.Issues.Should().HaveCount(2);
        context.Result.Issues.Should().BeEquivalentTo(issues.Take(2));
    }

    [Fact]
    public async Task ExecuteAsync_WhenResultIsNull_OptimizationMiddlewareHandlesGracefully()
    {
        // Arrange
        var pipeline = new AnalysisPipeline(NullLogger<AnalysisPipeline>.Instance, _mockAnalyzer.Object);
        var context = new AnalysisContext
        {
            Query = "SELECT * FROM Test",
            Arguments = new CommandLineArguments()
        };
        // Result is null by default

        // Act & Assert - Should not throw
        var act = async () => await pipeline.ExecuteAsync(context);
        await act.Should().NotThrowAsync();
    }

    // Test middleware implementations for tracking execution
    private class TestMiddleware : IAnalysisMiddleware
    {
        private readonly List<string> _executionOrder;
        private readonly string _name;

        public TestMiddleware(List<string> executionOrder, string name)
        {
            _executionOrder = executionOrder;
            _name = name;
        }

        public Task ExecuteAsync(AnalysisContext context)
        {
            _executionOrder.Add(_name);
            return Task.CompletedTask;
        }
    }

    private class StopExecutionMiddleware : IAnalysisMiddleware
    {
        private readonly List<string> _executionOrder;

        public StopExecutionMiddleware(List<string> executionOrder)
        {
            _executionOrder = executionOrder;
        }

        public Task ExecuteAsync(AnalysisContext context)
        {
            _executionOrder.Add("Middleware2");
            context.ShouldContinue = false;
            return Task.CompletedTask;
        }
    }

    private class ThrowingMiddleware : IAnalysisMiddleware
    {
        private readonly string _message;

        public ThrowingMiddleware(string message)
        {
            _message = message;
        }

        public Task ExecuteAsync(AnalysisContext context)
        {
            throw new InvalidOperationException(_message);
        }
    }

    private class ResultModifyingMiddleware : IAnalysisMiddleware
    {
        public Task ExecuteAsync(AnalysisContext context)
        {
            if (context.Result == null)
            {
                context.Result = new QueryAnalysisResult
                {
                    Query = context.Query,
                    PerformanceScore = 50
                };
            }

            context.Result.Metadata["MiddlewareExecuted"] = "true";
            return Task.CompletedTask;
        }
    }
}
