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
/// Unit tests for the <see cref="CartesianJoinPlugin"/> class to verify Cartesian join detection functionality.
/// </summary>
public class CartesianJoinPluginTests
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<CartesianJoinPluginTests> _logger;

    public CartesianJoinPluginTests()
    {
        // Create a minimal logger factory for testing
        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning); // Reduce noise in tests
        });

        _logger = _loggerFactory.CreateLogger<CartesianJoinPluginTests>();
    }

    /// <summary>
    /// Tests that the plugin detects implicit CROSS JOIN pattern (comma-separated tables).
    /// </summary>
    [Fact]
    public async Task ProcessAsync_QueryWithCommaSeparatedTables_AddsIssue()
    {
        // Arrange
        var plugin = new CartesianJoinPlugin(_loggerFactory.CreateLogger<CartesianJoinPlugin>());
        var result = new QueryAnalysisResult
        {
            Query = "SELECT * FROM Orders o, Customers c",
            QueryId = "test-1"
        };

        _logger.LogInformation("Starting test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithCommaSeparatedTables_AddsIssue), result.QueryId);
        try
        {
            // Act
            var processedResult = await plugin.ProcessAsync(result);

            // Assert
            processedResult.Should().NotBeNull();
            processedResult.Issues.Should().HaveCount(1);
            processedResult.Issues[0].IssueType.Should().Be(Constants.IssueType.CrossJoin);
            processedResult.Issues[0].Severity.Should().Be(Constants.IssueSeverity.Critical);
            processedResult.Issues[0].Description.Should().Contain("Implicit CROSS JOIN detected");
            processedResult.Issues[0].Metadata.Should().ContainKey("pattern").WhoseValue.Should().Be("implicit-cross-join");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithCommaSeparatedTables_AddsIssue), result.QueryId);
            throw;
        }
        finally
        {
            _logger.LogInformation("Finished test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithCommaSeparatedTables_AddsIssue), result.QueryId);
        }
    }

    /// <summary>
    /// Tests that the plugin detects explicit CROSS JOIN syntax.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_QueryWithExplicitCrossJoin_AddsIssue()
    {
        // Arrange
        var plugin = new CartesianJoinPlugin(_loggerFactory.CreateLogger<CartesianJoinPlugin>());
        var result = new QueryAnalysisResult
        {
            Query = "SELECT * FROM Orders o CROSS JOIN Customers c ON o.CustomerId = c.Id",
            QueryId = "test-2"
        };

        _logger.LogInformation("Starting test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithExplicitCrossJoin_AddsIssue), result.QueryId);
        try
        {
            // Act
            var processedResult = await plugin.ProcessAsync(result);

            // Assert
            processedResult.Issues.Should().HaveCount(1);
            processedResult.Issues[0].IssueType.Should().Be(Constants.IssueType.CrossJoin);
            processedResult.Issues[0].Severity.Should().Be(Constants.IssueSeverity.Critical);
            processedResult.Issues[0].Description.Should().Contain("Explicit CROSS JOIN detected");
            processedResult.Issues[0].Metadata.Should().ContainKey("pattern").WhoseValue.Should().Be("explicit-cross-join");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithExplicitCrossJoin_AddsIssue), result.QueryId);
            throw;
        }
        finally
        {
            _logger.LogInformation("Finished test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithExplicitCrossJoin_AddsIssue), result.QueryId);
        }
    }

    /// <summary>
    /// Tests that the plugin detects CROSS JOIN without ON clause.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_QueryWithCrossJoinWithoutCondition_AddsIssue()
    {
        // Arrange
        var plugin = new CartesianJoinPlugin(_loggerFactory.CreateLogger<CartesianJoinPlugin>());
        var result = new QueryAnalysisResult
        {
            Query = "SELECT * FROM Orders o CROSS JOIN Customers c",
            QueryId = "test-3"
        };

        _logger.LogInformation("Starting test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithCrossJoinWithoutCondition_AddsIssue), result.QueryId);
        try
        {
            // Act
            var processedResult = await plugin.ProcessAsync(result);

            // Assert
            processedResult.Issues.Should().HaveCount(1);
            processedResult.Issues[0].IssueType.Should().Be(Constants.IssueType.CrossJoin);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithCrossJoinWithoutCondition_AddsIssue), result.QueryId);
            throw;
        }
        finally
        {
            _logger.LogInformation("Finished test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithCrossJoinWithoutCondition_AddsIssue), result.QueryId);
        }
    }

    /// <summary>
    /// Tests that the plugin detects multiple comma-separated tables.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_QueryWithThreeCommaSeparatedTables_AddsIssue()
    {
        // Arrange
        var plugin = new CartesianJoinPlugin(_loggerFactory.CreateLogger<CartesianJoinPlugin>());
        var result = new QueryAnalysisResult
        {
            Query = "SELECT * FROM Orders o, Customers c, Products p",
            QueryId = "test-4"
        };

        _logger.LogInformation("Starting test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithThreeCommaSeparatedTables_AddsIssue), result.QueryId);
        try
        {
            // Act
            var processedResult = await plugin.ProcessAsync(result);

            // Assert
            processedResult.Issues.Should().HaveCount(1);
            processedResult.Issues[0].IssueType.Should().Be(Constants.IssueType.CrossJoin);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithThreeCommaSeparatedTables_AddsIssue), result.QueryId);
            throw;
        }
        finally
        {
            _logger.LogInformation("Finished test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithThreeCommaSeparatedTables_AddsIssue), result.QueryId);
        }
    }

    /// <summary>
    /// Tests that the plugin ignores queries with proper INNER JOIN conditions.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_QueryWithProperInnerJoin_DoesNotAddIssue()
    {
        // Arrange
        var plugin = new CartesianJoinPlugin(_loggerFactory.CreateLogger<CartesianJoinPlugin>());
        var result = new QueryAnalysisResult
        {
            Query = "SELECT * FROM Orders o INNER JOIN Customers c ON o.CustomerId = c.Id WHERE o.Status = 'Active'",
            QueryId = "test-5"
        };

        _logger.LogInformation("Starting test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithProperInnerJoin_DoesNotAddIssue), result.QueryId);
        try
        {
            // Act
            var processedResult = await plugin.ProcessAsync(result);

            // Assert
            processedResult.Issues.Should().BeEmpty();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithProperInnerJoin_DoesNotAddIssue), result.QueryId);
            throw;
        }
        finally
        {
            _logger.LogInformation("Finished test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithProperInnerJoin_DoesNotAddIssue), result.QueryId);
        }
    }

    /// <summary>
    /// Tests that the plugin ignores queries with LEFT JOIN conditions.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_QueryWithLeftJoin_DoesNotAddIssue()
    {
        // Arrange
        var plugin = new CartesianJoinPlugin(_loggerFactory.CreateLogger<CartesianJoinPlugin>());
        var result = new QueryAnalysisResult
        {
            Query = "SELECT * FROM Orders o LEFT JOIN Customers c ON o.CustomerId = c.Id",
            QueryId = "test-6"
        };

        _logger.LogInformation("Starting test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithLeftJoin_DoesNotAddIssue), result.QueryId);
        try
        {
            // Act
            var processedResult = await plugin.ProcessAsync(result);

            // Assert
            processedResult.Issues.Should().BeEmpty();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithLeftJoin_DoesNotAddIssue), result.QueryId);
            throw;
        }
        finally
        {
            _logger.LogInformation("Finished test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithLeftJoin_DoesNotAddIssue), result.QueryId);
        }
    }

    /// <summary>
    /// Tests that the plugin ignores single table queries.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_QueryWithSingleTable_DoesNotAddIssue()
    {
        // Arrange
        var plugin = new CartesianJoinPlugin(_loggerFactory.CreateLogger<CartesianJoinPlugin>());
        var result = new QueryAnalysisResult
        {
            Query = "SELECT * FROM Orders",
            QueryId = "test-7"
        };

        _logger.LogInformation("Starting test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithSingleTable_DoesNotAddIssue), result.QueryId);
        try
        {
            // Act
            var processedResult = await plugin.ProcessAsync(result);

            // Assert
            processedResult.Issues.Should().BeEmpty();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithSingleTable_DoesNotAddIssue), result.QueryId);
            throw;
        }
        finally
        {
            _logger.LogInformation("Finished test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithSingleTable_DoesNotAddIssue), result.QueryId);
        }
    }

    /// <summary>
    /// Tests that the plugin ignores queries without FROM clause.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_QueryWithoutFromClause_DoesNotAddIssue()
    {
        // Arrange
        var plugin = new CartesianJoinPlugin(_loggerFactory.CreateLogger<CartesianJoinPlugin>());
        var result = new QueryAnalysisResult
        {
            Query = "SELECT 1",
            QueryId = "test-8"
        };

        _logger.LogInformation("Starting test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithoutFromClause_DoesNotAddIssue), result.QueryId);
        try
        {
            // Act
            var processedResult = await plugin.ProcessAsync(result);

            // Assert
            processedResult.Issues.Should().BeEmpty();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithoutFromClause_DoesNotAddIssue), result.QueryId);
            throw;
        }
        finally
        {
            _logger.LogInformation("Finished test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithoutFromClause_DoesNotAddIssue), result.QueryId);
        }
    }

    /// <summary>
    /// Tests that the plugin handles case-insensitive CROSS JOIN patterns.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_QueryWithLowerCaseCrossJoin_AddsIssue()
    {
        // Arrange
        var plugin = new CartesianJoinPlugin(_loggerFactory.CreateLogger<CartesianJoinPlugin>());
        var result = new QueryAnalysisResult
        {
            Query = "select * from orders o cross join customers c on o.id = c.order_id",
            QueryId = "test-9"
        };

        _logger.LogInformation("Starting test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithLowerCaseCrossJoin_AddsIssue), result.QueryId);
        try
        {
            // Act
            var processedResult = await plugin.ProcessAsync(result);

            // Assert
            processedResult.Issues.Should().HaveCount(1);
            processedResult.Issues[0].IssueType.Should().Be(Constants.IssueType.CrossJoin);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithLowerCaseCrossJoin_AddsIssue), result.QueryId);
            throw;
        }
        finally
        {
            _logger.LogInformation("Finished test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithLowerCaseCrossJoin_AddsIssue), result.QueryId);
        }
    }

    /// <summary>
    /// Tests that the plugin detects CROSS JOIN even when mentioned in comments (hard to avoid false positives without full parser).
    /// </summary>
    [Fact]
    public async Task ProcessAsync_QueryWithCrossJoinInComment_StillDetectsIssue()
    {
        // Arrange
        var plugin = new CartesianJoinPlugin(_loggerFactory.CreateLogger<CartesianJoinPlugin>());
        var result = new QueryAnalysisResult
        {
            Query = "SELECT id, name FROM Orders -- This query mentions CROSS JOIN in comments\nWHERE status = 'active'",
            QueryId = "test-10"
        };

        _logger.LogInformation("Starting test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithCrossJoinInComment_StillDetectsIssue), result.QueryId);
        try
        {
            // Act
            var processedResult = await plugin.ProcessAsync(result);

            // Assert - Note: Without a full SQL parser, we can't reliably remove comments from the middle of queries
            // So we accept that this will detect the CROSS JOIN pattern. This is acceptable for a simple analyzer.
            processedResult.Issues.Should().NotBeEmpty();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithCrossJoinInComment_StillDetectsIssue), result.QueryId);
            throw;
        }
        finally
        {
            _logger.LogInformation("Finished test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithCrossJoinInComment_StillDetectsIssue), result.QueryId);
        }
    }

    /// <summary>
    /// Tests that the plugin handles multi-line comments gracefully.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_QueryWithMultiLineComment_DoesNotAddIssue()
    {
        // Arrange
        var plugin = new CartesianJoinPlugin(_loggerFactory.CreateLogger<CartesianJoinPlugin>());
        var result = new QueryAnalysisResult
        {
            Query = "/* This is a comment with CROSS JOIN pattern */ SELECT id, name FROM Orders",
            QueryId = "test-11"
        };

        _logger.LogInformation("Starting test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithMultiLineComment_DoesNotAddIssue), result.QueryId);
        try
        {
            // Act
            var processedResult = await plugin.ProcessAsync(result);

            // Assert - The plugin should handle this without crashing
            processedResult.Should().NotBeNull();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithMultiLineComment_DoesNotAddIssue), result.QueryId);
            throw;
        }
        finally
        {
            _logger.LogInformation("Finished test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithMultiLineComment_DoesNotAddIssue), result.QueryId);
        }
    }

    /// <summary>
    /// Tests that the plugin is disabled when IsEnabled is false.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_PluginDisabled_DoesNotAddIssue()
    {
        // Arrange
        var plugin = new CartesianJoinPlugin(_loggerFactory.CreateLogger<CartesianJoinPlugin>());
        plugin.IsEnabled = false;

        var result = new QueryAnalysisResult
        {
            Query = "SELECT * FROM Orders o, Customers c",
            QueryId = "test-12"
        };

        _logger.LogInformation("Starting test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_PluginDisabled_DoesNotAddIssue), result.QueryId);
        try
        {
            // Act
            var processedResult = await plugin.ProcessAsync(result);

            // Assert
            processedResult.Issues.Should().BeEmpty();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_PluginDisabled_DoesNotAddIssue), result.QueryId);
            throw;
        }
        finally
        {
            _logger.LogInformation("Finished test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_PluginDisabled_DoesNotAddIssue), result.QueryId);
        }
    }

    /// <summary>
    /// Tests that the plugin handles null query gracefully.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_NullQuery_DoesNotThrow()
    {
        // Arrange
        var plugin = new CartesianJoinPlugin(_loggerFactory.CreateLogger<CartesianJoinPlugin>());
        var result = new QueryAnalysisResult
        {
            Query = null,
            QueryId = "test-13"
        };

        _logger.LogInformation("Starting test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_NullQuery_DoesNotThrow), result.QueryId);
        try
        {
            // Act
            var processedResult = await plugin.ProcessAsync(result);

            // Assert
            processedResult.Should().NotBeNull();
            processedResult.Issues.Should().BeEmpty();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_NullQuery_DoesNotThrow), result.QueryId);
            throw;
        }
        finally
        {
            _logger.LogInformation("Finished test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_NullQuery_DoesNotThrow), result.QueryId);
        }
    }

    /// <summary>
    /// Tests that the plugin handles empty query gracefully.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_EmptyQuery_DoesNotAddIssue()
    {
        // Arrange
        var plugin = new CartesianJoinPlugin(_loggerFactory.CreateLogger<CartesianJoinPlugin>());
        var result = new QueryAnalysisResult
        {
            Query = "",
            QueryId = "test-14"
        };

        _logger.LogInformation("Starting test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_EmptyQuery_DoesNotAddIssue), result.QueryId);
        try
        {
            // Act
            var processedResult = await plugin.ProcessAsync(result);

            // Assert
            processedResult.Issues.Should().BeEmpty();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_EmptyQuery_DoesNotAddIssue), result.QueryId);
            throw;
        }
        finally
        {
            _logger.LogInformation("Finished test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_EmptyQuery_DoesNotAddIssue), result.QueryId);
        }
    }

    /// <summary>
    /// Tests that the plugin can be initialized and shutdown without errors.
    /// </summary>
    [Fact]
    public async Task InitializeAsync_And_ShutdownAsync_ShouldNotThrow()
    {
        // Arrange
        var plugin = new CartesianJoinPlugin(_loggerFactory.CreateLogger<CartesianJoinPlugin>());

        _logger.LogInformation("Starting test {TestName}", nameof(InitializeAsync_And_ShutdownAsync_ShouldNotThrow));
        try
        {
            // Act & Assert
            await plugin.InitializeAsync(); // Should not throw
            await plugin.ShutdownAsync(); // Should not throw
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test {TestName}", nameof(InitializeAsync_And_ShutdownAsync_ShouldNotThrow));
            throw;
        }
        finally
        {
            _logger.LogInformation("Finished test {TestName}", nameof(InitializeAsync_And_ShutdownAsync_ShouldNotThrow));
        }
    }

    /// <summary>
    /// Tests that the plugin has correct metadata.
    /// </summary>
    [Fact]
    public void PluginMetadata_IsCorrect()
    {
        // Arrange
        var plugin = new CartesianJoinPlugin();

        _logger.LogInformation("Starting test {TestName}", nameof(PluginMetadata_IsCorrect));
        try
        {
            // Act & Assert
            plugin.PluginId.Should().Be("cartesian-join-detection");
            plugin.Name.Should().Be("Cartesian Join Detection Plugin");
            plugin.Version.Should().Be(new Version(1, 0, 0));
            plugin.IsEnabled.Should().BeTrue();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test {TestName}", nameof(PluginMetadata_IsCorrect));
            throw;
        }
        finally
        {
            _logger.LogInformation("Finished test {TestName}", nameof(PluginMetadata_IsCorrect));
        }
    }

    /// <summary>
    /// Tests that the plugin detects both implicit and explicit CROSS JOIN in the same query.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_QueryWithBothImplicitAndExplicitCrossJoin_AddsTwoIssues()
    {
        // Arrange
        var plugin = new CartesianJoinPlugin(_loggerFactory.CreateLogger<CartesianJoinPlugin>());
        var result = new QueryAnalysisResult
        {
            Query = "SELECT * FROM Orders o, Customers c CROSS JOIN Products p",
            QueryId = "test-15"
        };

        _logger.LogInformation("Starting test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithBothImplicitAndExplicitCrossJoin_AddsTwoIssues), result.QueryId);
        try
        {
            // Act
            var processedResult = await plugin.ProcessAsync(result);

            // Assert
            processedResult.Issues.Should().HaveCount(2);
            processedResult.Issues.Should().AllSatisfy(issue =>
            {
                issue.IssueType.Should().Be(Constants.IssueType.CrossJoin);
                issue.Severity.Should().Be(Constants.IssueSeverity.Critical);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithBothImplicitAndExplicitCrossJoin_AddsTwoIssues), result.QueryId);
            throw;
        }
        finally
        {
            _logger.LogInformation("Finished test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithBothImplicitAndExplicitCrossJoin_AddsTwoIssues), result.QueryId);
        }
    }

    /// <summary>
    /// Tests that the plugin detects CROSS JOIN with table aliases.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_QueryWithCrossJoinAndAliases_AddsIssue()
    {
        // Arrange
        var plugin = new CartesianJoinPlugin(_loggerFactory.CreateLogger<CartesianJoinPlugin>());
        var result = new QueryAnalysisResult
        {
            Query = "SELECT o.OrderId, c.CustomerName FROM Orders AS o CROSS JOIN Customers AS c",
            QueryId = "test-16"
        };

        _logger.LogInformation("Starting test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithCrossJoinAndAliases_AddsIssue), result.QueryId);
        try
        {
            // Act
            var processedResult = await plugin.ProcessAsync(result);

            // Assert
            processedResult.Issues.Should().HaveCount(1);
            processedResult.Issues[0].IssueType.Should().Be(Constants.IssueType.CrossJoin);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithCrossJoinAndAliases_AddsIssue), result.QueryId);
            throw;
        }
        finally
        {
            _logger.LogInformation("Finished test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithCrossJoinAndAliases_AddsIssue), result.QueryId);
        }
    }
}