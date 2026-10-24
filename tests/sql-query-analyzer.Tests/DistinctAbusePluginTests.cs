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
/// Unit tests for the <see cref="DistinctAbusePlugin"/> class to verify SELECT DISTINCT with JOIN detection thresholds and edge cases.
/// </summary>
public class DistinctAbusePluginTests
{
    private readonly ILoggerFactory _loggerFactory;

    public DistinctAbusePluginTests()
    {
        // Create a minimal logger factory for testing
        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning); // Reduce noise in tests
        });
    }

    /// <summary>
    /// Tests that a query without DISTINCT is not flagged even when it contains a JOIN.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_QueryWithoutDistinct_DoesNotAddIssue()
    {
        // Arrange
        var plugin = new DistinctAbusePlugin(_loggerFactory.CreateLogger<DistinctAbusePlugin>());
        var result = new QueryAnalysisResult
        {
            Query = "SELECT a.Name, b.Title FROM Authors a INNER JOIN Books b ON b.AuthorId = a.Id",
            QueryId = "test-1"
        };

        // Act
        var processedResult = await plugin.ProcessAsync(result);

        // Assert
        processedResult.Issues.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that COUNT(DISTINCT ...) is not treated as a SELECT DISTINCT statement.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_CountDistinctWithJoin_DoesNotAddIssue()
    {
        // Arrange
        var plugin = new DistinctAbusePlugin(_loggerFactory.CreateLogger<DistinctAbusePlugin>());
        var result = new QueryAnalysisResult
        {
            Query = "SELECT COUNT(DISTINCT b.AuthorId) FROM Books b INNER JOIN Authors a ON a.Id = b.AuthorId",
            QueryId = "test-2"
        };

        // Act
        var processedResult = await plugin.ProcessAsync(result);

        // Assert
        processedResult.Issues.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that DISTINCT on a single column without any JOIN is not flagged.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_SingleColumnDistinctWithoutJoin_DoesNotAddIssue()
    {
        // Arrange
        var plugin = new DistinctAbusePlugin(_loggerFactory.CreateLogger<DistinctAbusePlugin>());
        var result = new QueryAnalysisResult
        {
            Query = "SELECT DISTINCT Status FROM Orders WHERE Total > 0",
            QueryId = "test-3"
        };

        // Act
        var processedResult = await plugin.ProcessAsync(result);

        // Assert
        processedResult.Issues.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that DISTINCT on a single column is flagged when a JOIN is present; the plugin has no index awareness,
    /// so the column count does not suppress the warning.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_SingleColumnDistinctWithInnerJoin_AddsWarning()
    {
        // Arrange
        var plugin = new DistinctAbusePlugin(_loggerFactory.CreateLogger<DistinctAbusePlugin>());
        var result = new QueryAnalysisResult
        {
            Query = "SELECT DISTINCT a.Name FROM Authors a INNER JOIN Books b ON b.AuthorId = a.Id",
            QueryId = "test-4"
        };

        // Act
        var processedResult = await plugin.ProcessAsync(result);

        // Assert
        processedResult.Issues.Should().ContainSingle();
        var issue = processedResult.Issues[0];
        issue.IssueType.Should().Be(Constants.IssueType.LargeResultSet);
        issue.Severity.Should().Be(Constants.IssueSeverity.Warning);
        issue.AffectedClause.Should().Be("SELECT DISTINCT");
        issue.Metadata.Should().ContainKey("pattern").WhoseValue.Should().Be("distinct-with-joins");
        issue.Metadata.Should().ContainKey("select_clause").WhoseValue.Should().Be("a.Name");
    }

    /// <summary>
    /// Tests that each explicit JOIN type supported by the plugin is detected.
    /// </summary>
    [Theory]
    [InlineData("INNER JOIN")]
    [InlineData("LEFT JOIN")]
    [InlineData("RIGHT JOIN")]
    [InlineData("FULL JOIN")]
    [InlineData("CROSS JOIN")]
    public async Task ProcessAsync_DistinctWithExplicitJoinType_AddsIssue(string joinKeyword)
    {
        // Arrange
        var plugin = new DistinctAbusePlugin(_loggerFactory.CreateLogger<DistinctAbusePlugin>());
        var result = new QueryAnalysisResult
        {
            Query = $"SELECT DISTINCT a.Name FROM Authors a {joinKeyword} Books b ON b.AuthorId = a.Id",
            QueryId = "test-5"
        };

        // Act
        var processedResult = await plugin.ProcessAsync(result);

        // Assert
        processedResult.Issues.Should().ContainSingle();
    }

    /// <summary>
    /// Tests that an implicit (comma-separated) join with DISTINCT is detected.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_DistinctWithCommaSeparatedTables_AddsIssue()
    {
        // Arrange
        var plugin = new DistinctAbusePlugin(_loggerFactory.CreateLogger<DistinctAbusePlugin>());
        var result = new QueryAnalysisResult
        {
            Query = "SELECT DISTINCT c.Name FROM Customers c, Orders o WHERE c.Id = o.CustomerId",
            QueryId = "test-6"
        };

        // Act
        var processedResult = await plugin.ProcessAsync(result);

        // Assert
        processedResult.Issues.Should().ContainSingle();
    }

    /// <summary>
    /// Tests that DISTINCT combined with GROUP BY is not flagged when no JOIN is present.
    /// GROUP BY alone does not indicate join fan-out.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_DistinctWithGroupByWithoutJoin_DoesNotAddIssue()
    {
        // Arrange
        var plugin = new DistinctAbusePlugin(_loggerFactory.CreateLogger<DistinctAbusePlugin>());
        var result = new QueryAnalysisResult
        {
            Query = "SELECT DISTINCT Status FROM Orders GROUP BY Status",
            QueryId = "test-7"
        };

        // Act
        var processedResult = await plugin.ProcessAsync(result);

        // Assert
        processedResult.Issues.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that multiple DISTINCT statements are each evaluated, so a DISTINCT inside a subquery
    /// that joins tables is reported alongside the outer DISTINCT.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_DistinctInOuterAndSubqueryWithJoin_AddsIssuePerDistinct()
    {
        // Arrange
        var plugin = new DistinctAbusePlugin(_loggerFactory.CreateLogger<DistinctAbusePlugin>());
        var result = new QueryAnalysisResult
        {
            Query = "SELECT DISTINCT o.CustomerId FROM Orders o " +
                    "WHERE o.Id IN (SELECT DISTINCT oi.OrderId FROM OrderItems oi INNER JOIN Products p ON p.Id = oi.ProductId)",
            QueryId = "test-8"
        };

        // Act
        var processedResult = await plugin.ProcessAsync(result);

        // Assert
        // The outer DISTINCT is also flagged because the plugin scans all text after its FROM, including the subquery join.
        processedResult.Issues.Should().HaveCount(2);
        processedResult.Issues.Select(i => i.Metadata["select_clause"])
            .Should().BeEquivalentTo(new[] { "o.CustomerId", "oi.OrderId" });
    }

    /// <summary>
    /// Tests that a query with DISTINCT but no FROM clause is not matched.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_DistinctWithoutFromClause_DoesNotAddIssue()
    {
        // Arrange
        var plugin = new DistinctAbusePlugin(_loggerFactory.CreateLogger<DistinctAbusePlugin>());
        var result = new QueryAnalysisResult
        {
            Query = "SELECT DISTINCT 1",
            QueryId = "test-10"
        };

        // Act
        var processedResult = await plugin.ProcessAsync(result);

        // Assert
        processedResult.Issues.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that lowercase SQL keywords are matched case-insensitively.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_LowerCaseDistinctWithJoin_AddsIssue()
    {
        // Arrange
        var plugin = new DistinctAbusePlugin(_loggerFactory.CreateLogger<DistinctAbusePlugin>());
        var result = new QueryAnalysisResult
        {
            Query = "select distinct a.name from authors a inner join books b on b.author_id = a.id",
            QueryId = "test-11"
        };

        // Act
        var processedResult = await plugin.ProcessAsync(result);

        // Assert
        processedResult.Issues.Should().ContainSingle();
    }

    /// <summary>
    /// Tests that a null query is skipped without throwing.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_NullQuery_DoesNotThrow()
    {
        // Arrange
        var plugin = new DistinctAbusePlugin(_loggerFactory.CreateLogger<DistinctAbusePlugin>());
        var result = new QueryAnalysisResult
        {
            Query = null!,
            QueryId = "test-12"
        };

        // Act
        var processedResult = await plugin.ProcessAsync(result);

        // Assert
        processedResult.Should().BeSameAs(result);
        processedResult.Issues.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that a whitespace-only query is skipped.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WhitespaceQuery_DoesNotAddIssue()
    {
        // Arrange
        var plugin = new DistinctAbusePlugin(_loggerFactory.CreateLogger<DistinctAbusePlugin>());
        var result = new QueryAnalysisResult
        {
            Query = "   \n\t  ",
            QueryId = "test-13"
        };

        // Act
        var processedResult = await plugin.ProcessAsync(result);

        // Assert
        processedResult.Issues.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that the plugin does nothing when it is disabled, even for a clearly flagged query.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_PluginDisabled_DoesNotAddIssue()
    {
        // Arrange
        var plugin = new DistinctAbusePlugin(_loggerFactory.CreateLogger<DistinctAbusePlugin>())
        {
            IsEnabled = false
        };
        var result = new QueryAnalysisResult
        {
            Query = "SELECT DISTINCT a.Name FROM Authors a INNER JOIN Books b ON b.AuthorId = a.Id",
            QueryId = "test-14"
        };

        // Act
        var processedResult = await plugin.ProcessAsync(result);

        // Assert
        processedResult.Issues.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that the plugin exposes the expected identifier, name and version.
    /// </summary>
    [Fact]
    public void PluginMetadata_IsCorrect()
    {
        // Arrange & Act
        var plugin = new DistinctAbusePlugin();

        // Assert
        plugin.PluginId.Should().Be("distinct-abuse-detection");
        plugin.Name.Should().Be("DISTINCT Abuse Detection Plugin");
        plugin.Version.Should().Be(new Version(1, 0, 0));
        plugin.IsEnabled.Should().BeTrue();
    }
}
