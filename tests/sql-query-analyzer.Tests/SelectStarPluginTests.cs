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
/// Unit tests for the <see cref="SelectStarPlugin"/> class to verify SELECT * detection functionality.
/// </summary>
public class SelectStarPluginTests
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<SelectStarPluginTests> _logger;

    public SelectStarPluginTests()
    {
        // Create a minimal logger factory for testing
        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning); // Reduce noise in tests
        });

        _logger = _loggerFactory.CreateLogger<SelectStarPluginTests>();
    }

    /// <summary>
    /// Tests that the plugin detects basic SELECT * pattern.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_QueryWithSelectStar_AddsIssue()
    {
        _logger.LogInformation("Starting test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithSelectStar_AddsIssue), "test-1");
        try
        {
            // Arrange
            var plugin = new SelectStarPlugin(_loggerFactory.CreateLogger<SelectStarPlugin>());
            var result = new QueryAnalysisResult
            {
                Query = "SELECT * FROM Orders",
                QueryId = "test-1"
            };

            // Act
            var processedResult = await plugin.ProcessAsync(result);

            // Assert
            processedResult.Should().NotBeNull();
            processedResult.Issues.Should().HaveCount(1);
            processedResult.Issues[0].IssueType.Should().Be(Constants.IssueType.SelectStar);
            processedResult.Issues[0].Severity.Should().Be(Constants.IssueSeverity.Warning);
            processedResult.Issues[0].Description.Should().Contain("SELECT * detected");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithSelectStar_AddsIssue), "test-1");
            throw;
        }
        finally
        {
            _logger.LogInformation("Finished test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithSelectStar_AddsIssue), "test-1");
        }
    }

    /// <summary>
    /// Tests that the plugin detects SELECT table.* pattern.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_QueryWithTableStar_AddsIssue()
    {
        _logger.LogInformation("Starting test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithTableStar_AddsIssue), "test-2");
        try
        {
            // Arrange
            var plugin = new SelectStarPlugin(_loggerFactory.CreateLogger<SelectStarPlugin>());
            var result = new QueryAnalysisResult
            {
                Query = "SELECT o.* FROM Orders o",
                QueryId = "test-2"
            };

            // Act
            var processedResult = await plugin.ProcessAsync(result);

            // Assert
            processedResult.Issues.Should().HaveCount(1);
            processedResult.Issues[0].IssueType.Should().Be(Constants.IssueType.SelectStar);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithTableStar_AddsIssue), "test-2");
            throw;
        }
        finally
        {
            _logger.LogInformation("Finished test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithTableStar_AddsIssue), "test-2");
        }
    }

    /// <summary>
    /// Tests that the plugin detects SELECT * in multi-column SELECT.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_QueryWithStarInMultiColumnSelect_AddsIssue()
    {
        _logger.LogInformation("Starting test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithStarInMultiColumnSelect_AddsIssue), "test-3");
        try
        {
            // Arrange
            var plugin = new SelectStarPlugin(_loggerFactory.CreateLogger<SelectStarPlugin>());
            var result = new QueryAnalysisResult
            {
                Query = "SELECT id, name, * FROM Orders",
                QueryId = "test-3"
            };

            // Act
            var processedResult = await plugin.ProcessAsync(result);

            // Assert
            processedResult.Issues.Should().HaveCount(1);
            processedResult.Issues[0].IssueType.Should().Be(Constants.IssueType.SelectStar);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithStarInMultiColumnSelect_AddsIssue), "test-3");
            throw;
        }
        finally
        {
            _logger.LogInformation("Finished test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithStarInMultiColumnSelect_AddsIssue), "test-3");
        }
    }

    /// <summary>
    /// Tests that the plugin ignores COUNT(*) function.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_QueryWithCountStar_DoesNotAddIssue()
    {
        _logger.LogInformation("Starting test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithCountStar_DoesNotAddIssue), "test-4");
        try
        {
            // Arrange
            var plugin = new SelectStarPlugin(_loggerFactory.CreateLogger<SelectStarPlugin>());
            var result = new QueryAnalysisResult
            {
                Query = "SELECT COUNT(*) FROM Orders",
                QueryId = "test-4"
            };

            // Act
            var processedResult = await plugin.ProcessAsync(result);

            // Assert
            processedResult.Issues.Should().BeEmpty();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithCountStar_DoesNotAddIssue), "test-4");
            throw;
        }
        finally
        {
            _logger.LogInformation("Finished test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithCountStar_DoesNotAddIssue), "test-4");
        }
    }

    /// <summary>
    /// Tests that the plugin ignores queries without SELECT *.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_QueryWithoutStar_DoesNotAddIssue()
    {
        _logger.LogInformation("Starting test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithoutStar_DoesNotAddIssue), "test-5");
        try
        {
            // Arrange
            var plugin = new SelectStarPlugin(_loggerFactory.CreateLogger<SelectStarPlugin>());
            var result = new QueryAnalysisResult
            {
                Query = "SELECT id, name, price FROM Products WHERE category = 'Electronics'",
                QueryId = "test-5"
            };

            // Act
            var processedResult = await plugin.ProcessAsync(result);

            // Assert
            processedResult.Issues.Should().BeEmpty();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithoutStar_DoesNotAddIssue), "test-5");
            throw;
        }
        finally
        {
            _logger.LogInformation("Finished test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithoutStar_DoesNotAddIssue), "test-5");
        }
    }

    /// <summary>
    /// Tests that the plugin ignores SELECT * in comments.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_QueryWithStarInComment_DoesNotAddIssue()
    {
        _logger.LogInformation("Starting test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithStarInComment_DoesNotAddIssue), "test-6");
        try
        {
            // Arrange
            var plugin = new SelectStarPlugin(_loggerFactory.CreateLogger<SelectStarPlugin>());
            var result = new QueryAnalysisResult
            {
                Query = "SELECT id, name FROM Orders -- This query uses SELECT * in comments\nWHERE status = 'active'",
                QueryId = "test-6"
            };

            // Act
            var processedResult = await plugin.ProcessAsync(result);

            // Assert
            processedResult.Issues.Should().BeEmpty();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithStarInComment_DoesNotAddIssue), "test-6");
            throw;
        }
        finally
        {
            _logger.LogInformation("Finished test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithStarInComment_DoesNotAddIssue), "test-6");
        }
    }

    /// <summary>
    /// Tests that the plugin ignores SELECT * in multi-line comments.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_QueryWithStarInMultiLineComment_DoesNotAddIssue()
    {
        _logger.LogInformation("Starting test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithStarInMultiLineComment_DoesNotAddIssue), "test-7");
        try
        {
            // Arrange
            var plugin = new SelectStarPlugin(_loggerFactory.CreateLogger<SelectStarPlugin>());
            var result = new QueryAnalysisResult
            {
                Query = "/* This is a comment with SELECT * pattern */ SELECT id, name FROM Orders",
                QueryId = "test-7"
            };

            // Act
            var processedResult = await plugin.ProcessAsync(result);

            // Assert
            processedResult.Issues.Should().BeEmpty();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithStarInMultiLineComment_DoesNotAddIssue), "test-7");
            throw;
        }
        finally
        {
            _logger.LogInformation("Finished test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithStarInMultiLineComment_DoesNotAddIssue), "test-7");
        }
    }

    /// <summary>
    /// Tests that the plugin handles case-insensitive SELECT * patterns.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_QueryWithLowerCaseSelectStar_AddsIssue()
    {
        _logger.LogInformation("Starting test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithLowerCaseSelectStar_AddsIssue), "test-8");
        try
        {
            // Arrange
            var plugin = new SelectStarPlugin(_loggerFactory.CreateLogger<SelectStarPlugin>());
            var result = new QueryAnalysisResult
            {
                Query = "select * from orders",
                QueryId = "test-8"
            };

            // Act
            var processedResult = await plugin.ProcessAsync(result);

            // Assert
            processedResult.Issues.Should().HaveCount(1);
            processedResult.Issues[0].IssueType.Should().Be(Constants.IssueType.SelectStar);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithLowerCaseSelectStar_AddsIssue), "test-8");
            throw;
        }
        finally
        {
            _logger.LogInformation("Finished test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_QueryWithLowerCaseSelectStar_AddsIssue), "test-8");
        }
    }

    /// <summary>
    /// Tests that the plugin is disabled when IsEnabled is false.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_PluginDisabled_DoesNotAddIssue()
    {
        _logger.LogInformation("Starting test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_PluginDisabled_DoesNotAddIssue), "test-9");
        try
        {
            // Arrange
            var plugin = new SelectStarPlugin(_loggerFactory.CreateLogger<SelectStarPlugin>());
            plugin.IsEnabled = false;

            var result = new QueryAnalysisResult
            {
                Query = "SELECT * FROM Orders",
                QueryId = "test-9"
            };

            // Act
            var processedResult = await plugin.ProcessAsync(result);

            // Assert
            processedResult.Issues.Should().BeEmpty();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_PluginDisabled_DoesNotAddIssue), "test-9");
            throw;
        }
        finally
        {
            _logger.LogInformation("Finished test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_PluginDisabled_DoesNotAddIssue), "test-9");
        }
    }

    /// <summary>
    /// Tests that the plugin handles null query gracefully.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_NullQuery_DoesNotThrow()
    {
        _logger.LogInformation("Starting test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_NullQuery_DoesNotThrow), "test-10");
        try
        {
            // Arrange
            var plugin = new SelectStarPlugin(_loggerFactory.CreateLogger<SelectStarPlugin>());
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_NullQuery_DoesNotThrow), "test-10");
            throw;
        }
        finally
        {
            _logger.LogInformation("Finished test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_NullQuery_DoesNotThrow), "test-10");
        }
    }

    /// <summary>
    /// Tests that the plugin handles empty query gracefully.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_EmptyQuery_DoesNotAddIssue()
    {
        _logger.LogInformation("Starting test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_EmptyQuery_DoesNotAddIssue), "test-11");
        try
        {
            // Arrange
            var plugin = new SelectStarPlugin(_loggerFactory.CreateLogger<SelectStarPlugin>());
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_EmptyQuery_DoesNotAddIssue), "test-11");
            throw;
        }
        finally
        {
            _logger.LogInformation("Finished test {TestName} for QueryId {QueryId}", nameof(ProcessAsync_EmptyQuery_DoesNotAddIssue), "test-11");
        }
    }

    /// <summary>
    /// Tests that the plugin can be initialized and shutdown without errors.
    /// </summary>
    [Fact]
    public async Task InitializeAsync_And_ShutdownAsync_ShouldNotThrow()
    {
        _logger.LogInformation("Starting test {TestName}", nameof(InitializeAsync_And_ShutdownAsync_ShouldNotThrow));
        try
        {
            // Arrange
            var plugin = new SelectStarPlugin(_loggerFactory.CreateLogger<SelectStarPlugin>());

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
        _logger.LogInformation("Starting test {TestName}", nameof(PluginMetadata_IsCorrect));
        try
        {
            // Arrange
            var plugin = new SelectStarPlugin();

            // Act & Assert
            plugin.PluginId.Should().Be("select-star-detection");
            plugin.Name.Should().Be("SELECT * Detection Plugin");
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
}
