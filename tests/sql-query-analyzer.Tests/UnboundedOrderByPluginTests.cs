#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Models;
using SqlQueryAnalyzer.Plugins;
using Xunit;

namespace SqlQueryAnalyzer.Tests;

/// <summary>
/// Unit tests for the <see cref="UnboundedOrderByPlugin"/> class to verify ORDER BY without pagination detection functionality.
/// </summary>
public class UnboundedOrderByPluginTests
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<UnboundedOrderByPluginTests> _logger;

    public UnboundedOrderByPluginTests()
    {
        // Create a minimal logger factory for testing
        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning); // Reduce noise in tests
        });
        _logger = _loggerFactory.CreateLogger<UnboundedOrderByPluginTests>();
    }

    /// <summary>
    /// Tests that the plugin detects ORDER BY without pagination.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_QueryWithOrderByWithoutPagination_AddsIssue()
    {
        _logger.LogInformation("ProcessAsync_QueryWithOrderByWithoutPagination_AddsIssue started with {QueryId}", "test-1");
        // Arrange
        var plugin = new UnboundedOrderByPlugin(_loggerFactory.CreateLogger<UnboundedOrderByPlugin>());
        var result = new QueryAnalysisResult
        {
            Query = "SELECT * FROM Orders ORDER BY order_date",
            QueryId = "test-1"
        };

        // Act
        var processedResult = await plugin.ProcessAsync(result);

        // Assert
        processedResult.Should().NotBeNull();
        processedResult.Issues.Should().HaveCount(1);
        processedResult.Issues[0].IssueType.Should().Be(Constants.IssueType.LargeResultSet);
        processedResult.Issues[0].Severity.Should().Be(Constants.IssueSeverity.Info);
        processedResult.Issues[0].Description.Should().Contain("ORDER BY without pagination");
        _logger.LogInformation("ProcessAsync_QueryWithOrderByWithoutPagination_AddsIssue completed with {IssueCount} issue(s)", processedResult.Issues.Count);
    }

    /// <summary>
    /// Tests that the plugin detects ORDER BY with TOP clause (should not flag).
    /// </summary>
    [Fact]
    public async Task ProcessAsync_QueryWithOrderByAndTop_DoesNotAddIssue()
    {
        _logger.LogInformation("ProcessAsync_QueryWithOrderByAndTop_DoesNotAddIssue started with {QueryId}", "test-2");
        // Arrange
        var plugin = new UnboundedOrderByPlugin(_loggerFactory.CreateLogger<UnboundedOrderByPlugin>());
        var result = new QueryAnalysisResult
        {
            Query = "SELECT TOP 100 * FROM Orders ORDER BY order_date",
            QueryId = "test-2"
        };

        // Act
        var processedResult = await plugin.ProcessAsync(result);

        // Assert - TOP clause should be detected and no issue should be added
        processedResult.Issues.Should().BeEmpty();
        _logger.LogInformation("ProcessAsync_QueryWithOrderByAndTop_DoesNotAddIssue completed with {IssueCount} issue(s)", processedResult.Issues.Count);
    }

    /// <summary>
    /// Tests that the plugin detects ORDER BY with LIMIT clause (should not flag).
    /// </summary>
    [Fact]
    public async Task ProcessAsync_QueryWithOrderByAndLimit_DoesNotAddIssue()
    {
        _logger.LogInformation("ProcessAsync_QueryWithOrderByAndLimit_DoesNotAddIssue started with {QueryId}", "test-3");
        // Arrange
        var plugin = new UnboundedOrderByPlugin(_loggerFactory.CreateLogger<UnboundedOrderByPlugin>());
        var result = new QueryAnalysisResult
        {
            Query = "SELECT * FROM Orders ORDER BY order_date LIMIT 100",
            QueryId = "test-3"
        };

        // Act
        var processedResult = await plugin.ProcessAsync(result);

        // Assert
        processedResult.Issues.Should().BeEmpty();
        _logger.LogInformation("ProcessAsync_QueryWithOrderByAndLimit_DoesNotAddIssue completed with {IssueCount} issue(s)", processedResult.Issues.Count);
    }

    /// <summary>
    /// Tests that the plugin detects ORDER BY with OFFSET-FETCH clause (should not flag).
    /// </summary>
    [Fact]
    public async Task ProcessAsync_QueryWithOrderByAndOffsetFetch_DoesNotAddIssue()
    {
        _logger.LogInformation("ProcessAsync_QueryWithOrderByAndOffsetFetch_DoesNotAddIssue started with {QueryId}", "test-4");
        // Arrange
        var plugin = new UnboundedOrderByPlugin(_loggerFactory.CreateLogger<UnboundedOrderByPlugin>());
        var result = new QueryAnalysisResult
        {
            Query = "SELECT * FROM Orders ORDER BY order_date OFFSET 0 ROWS FETCH NEXT 100 ROWS ONLY",
            QueryId = "test-4"
        };

        // Act
        var processedResult = await plugin.ProcessAsync(result);

        // Assert
        processedResult.Issues.Should().BeEmpty();
        _logger.LogInformation("ProcessAsync_QueryWithOrderByAndOffsetFetch_DoesNotAddIssue completed with {IssueCount} issue(s)", processedResult.Issues.Count);
    }

    /// <summary>
    /// Tests that the plugin detects ORDER BY with FETCH NEXT clause (should not flag).
    /// </summary>
    [Fact]
    public async Task ProcessAsync_QueryWithOrderByAndFetchNext_DoesNotAddIssue()
    {
        // Arrange
        var plugin = new UnboundedOrderByPlugin(_loggerFactory.CreateLogger<UnboundedOrderByPlugin>());
        var result = new QueryAnalysisResult
        {
            Query = "SELECT * FROM Orders ORDER BY order_date FETCH NEXT 100 ROWS ONLY",
            QueryId = "test-5"
        };

        // Act
        var processedResult = await plugin.ProcessAsync(result);

        // Assert
        processedResult.Issues.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that the plugin detects ORDER BY with ROW_NUMBER pagination (should not flag).
    /// </summary>
    [Fact]
    public async Task ProcessAsync_QueryWithOrderByAndRowNumber_DoesNotAddIssue()
    {
        // Arrange
        var plugin = new UnboundedOrderByPlugin(_loggerFactory.CreateLogger<UnboundedOrderByPlugin>());
        var result = new QueryAnalysisResult
        {
            Query = "SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY order_date) as rn FROM Orders) t WHERE rn BETWEEN 1 AND 100",
            QueryId = "test-6"
        };

        // Act
        var processedResult = await plugin.ProcessAsync(result);

        // Assert
        processedResult.Issues.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that the plugin ignores queries without ORDER BY.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_QueryWithoutOrderBy_DoesNotAddIssue()
    {
        // Arrange
        var plugin = new UnboundedOrderByPlugin(_loggerFactory.CreateLogger<UnboundedOrderByPlugin>());
        var result = new QueryAnalysisResult
        {
            Query = "SELECT id, name FROM Products WHERE category = 'Electronics'",
            QueryId = "test-7"
        };

        // Act
        var processedResult = await plugin.ProcessAsync(result);

        // Assert
        processedResult.Issues.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that the plugin handles case-insensitive ORDER BY patterns.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_QueryWithLowerCaseOrderBy_AddsIssue()
    {
        // Arrange
        var plugin = new UnboundedOrderByPlugin(_loggerFactory.CreateLogger<UnboundedOrderByPlugin>());
        var result = new QueryAnalysisResult
        {
            Query = "select * from orders order by order_date",
            QueryId = "test-8"
        };

        // Act
        var processedResult = await plugin.ProcessAsync(result);

        // Assert
        processedResult.Issues.Should().HaveCount(1);
        processedResult.Issues[0].IssueType.Should().Be(Constants.IssueType.LargeResultSet);
    }

    /// <summary>
    /// Tests that the plugin is disabled when IsEnabled is false.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_PluginDisabled_DoesNotAddIssue()
    {
        // Arrange
        var plugin = new UnboundedOrderByPlugin(_loggerFactory.CreateLogger<UnboundedOrderByPlugin>());
        plugin.IsEnabled = false;

        var result = new QueryAnalysisResult
        {
            Query = "SELECT * FROM Orders ORDER BY order_date",
            QueryId = "test-9"
        };

        // Act
        var processedResult = await plugin.ProcessAsync(result);

        // Assert
        processedResult.Issues.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that the plugin handles null query gracefully.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_NullQuery_DoesNotThrow()
    {
        // Arrange
        var plugin = new UnboundedOrderByPlugin(_loggerFactory.CreateLogger<UnboundedOrderByPlugin>());
        var result = new QueryAnalysisResult
        {
            Query = null,
            QueryId = "test-10"
        };

        // Act
        var processedResult = await plugin.ProcessAsync(result);

        // Assert
        processedResult.Should().NotBeNull();
        processedResult.Issues.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that the plugin handles empty query gracefully.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_EmptyQuery_DoesNotAddIssue()
    {
        // Arrange
        var plugin = new UnboundedOrderByPlugin(_loggerFactory.CreateLogger<UnboundedOrderByPlugin>());
        var result = new QueryAnalysisResult
        {
            Query = "",
            QueryId = "test-11"
        };

        // Act
        var processedResult = await plugin.ProcessAsync(result);

        // Assert
        processedResult.Issues.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that the plugin can be initialized and shutdown without errors.
    /// </summary>
    [Fact]
    public async Task InitializeAsync_And_ShutdownAsync_ShouldNotThrow()
    {
        // Arrange
        var plugin = new UnboundedOrderByPlugin(_loggerFactory.CreateLogger<UnboundedOrderByPlugin>());

        // Act & Assert
        await plugin.InitializeAsync(); // Should not throw
        await plugin.ShutdownAsync(); // Should not throw
    }

    /// <summary>
    /// Tests that the plugin has correct metadata.
    /// </summary>
    [Fact]
    public void PluginMetadata_IsCorrect()
    {
        // Arrange
        var plugin = new UnboundedOrderByPlugin();

        // Act & Assert
        plugin.PluginId.Should().Be("unbounded-orderby-detection");
        plugin.Name.Should().Be("Unbounded ORDER BY Detection Plugin");
        plugin.Version.Should().Be(new Version(1, 0, 0));
        plugin.IsEnabled.Should().BeTrue();
    }

    /// <summary>
    /// Tests multiple ORDER BY clauses in a single query.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_QueryWithMultipleOrderByClauses_AddsIssueForEach()
    {
        // Arrange
        var plugin = new UnboundedOrderByPlugin(_loggerFactory.CreateLogger<UnboundedOrderByPlugin>());
        var result = new QueryAnalysisResult
        {
            Query = "SELECT * FROM Orders ORDER BY order_date; SELECT * FROM Products ORDER BY price",
            QueryId = "test-12"
        };

        // Act
        var processedResult = await plugin.ProcessAsync(result);

        // Assert
        processedResult.Should().NotBeNull();
        processedResult.Issues.Should().HaveCount(2);
    }

    /// <summary>
    /// Tests ORDER BY with multiple columns.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_QueryWithMultipleColumnOrderBy_AddsIssue()
    {
        // Arrange
        var plugin = new UnboundedOrderByPlugin(_loggerFactory.CreateLogger<UnboundedOrderByPlugin>());
        var result = new QueryAnalysisResult
        {
            Query = "SELECT * FROM Orders ORDER BY customer_id, order_date DESC, status",
            QueryId = "test-13"
        };

        // Act
        var processedResult = await plugin.ProcessAsync(result);

        // Assert
        processedResult.Issues.Should().HaveCount(1);
        processedResult.Issues[0].IssueType.Should().Be(Constants.IssueType.LargeResultSet);
    }
}
