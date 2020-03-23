#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Plugins;

/// <summary>
/// Interface for extending analyzer with custom analysis logic.
/// Plugins can add new issue detection or post-processing logic.
/// Enables third-party extensions without modifying core code.
/// </summary>
public interface IAnalysisPlugin
{
    /// <summary>
    /// Gets unique identifier for this plugin.
    /// </summary>
    string PluginId { get; }

    /// <summary>
    /// Gets human-readable plugin name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets plugin version.
    /// </summary>
    Version Version { get; }

    /// <summary>
    /// Called when plugin is initialized.
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Called to process analysis result.
    /// Plugin may add issues or modify result.
    /// </summary>
    Task<QueryAnalysisResult> ProcessAsync(QueryAnalysisResult result);

    /// <summary>
    /// Called when plugin is unloaded.
    /// </summary>
    Task ShutdownAsync();

    /// <summary>
    /// Checks if plugin is enabled.
    /// </summary>
    bool IsEnabled { get; set; }
}

/// <summary>
/// Plugin manager for loading and executing plugins.
/// Handles plugin lifecycle and error recovery.
/// </summary>
public sealed class PluginManager
{
    private readonly List<IAnalysisPlugin> _plugins = new();
    private readonly ILogger<PluginManager> _logger;

    public PluginManager(ILogger<PluginManager> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Registers a plugin.
    /// </summary>
    public async Task RegisterPluginAsync(IAnalysisPlugin plugin)
    {
        try
        {
            await plugin.InitializeAsync().ConfigureAwait(false);
            _plugins.Add(plugin);
            _logger.LogInformation("Plugin registered: {Name} v{Version}", plugin.Name, plugin.Version);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to register plugin: {plugin.Name}");
            throw;
        }
    }

    /// <summary>
    /// Unregisters a plugin.
    /// </summary>
    public async Task UnregisterPluginAsync(string pluginId)
    {
        var plugin = _plugins.FirstOrDefault(p => p.PluginId == pluginId);
        if (plugin == null)
        {
            _logger.LogWarning("Plugin not found: {PluginId}", pluginId);
            return;
        }

        try
        {
            await plugin.ShutdownAsync().ConfigureAwait(false);
            _plugins.Remove(plugin);
            _logger.LogInformation("Plugin unregistered: {Name}", plugin.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error unregistering plugin: {plugin.Name}");
        }
    }

    /// <summary>
    /// Processes analysis result through all enabled plugins.
    /// </summary>
    public async Task<QueryAnalysisResult> ProcessThroughPluginsAsync(QueryAnalysisResult result)
    {
        var current = result;

        foreach (var plugin in _plugins.Where(p => p.IsEnabled))
        {
            try
            {
                _logger.LogDebug("Processing through plugin: {Name}", plugin.Name);
                current = await plugin.ProcessAsync(current).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Plugin error: {plugin.Name}");
                // Continue with other plugins even if one fails
            }
        }

        return current;
    }

    /// <summary>
    /// Gets count of registered plugins.
    /// </summary>
    public int GetPluginCount() => _plugins.Count;

    /// <summary>
    /// Gets all registered plugins.
    /// </summary>
    public List<IAnalysisPlugin> GetPlugins() => _plugins.ToList();

    /// <summary>
    /// Gets plugin by ID.
    /// </summary>
    public IAnalysisPlugin? GetPlugin(string pluginId) =>
        _plugins.FirstOrDefault(p => p.PluginId == pluginId);
}

/// <summary>
/// Base class for creating plugins.
/// Provides common functionality.
/// </summary>
public abstract class AnalysisPluginBase : IAnalysisPlugin
{
    protected ILogger? Logger { get; set; }

    public abstract string PluginId { get; }
    public abstract string Name { get; }
    public abstract Version Version { get; }
    public bool IsEnabled { get; set; } = true;

    public virtual Task InitializeAsync() => Task.CompletedTask;
    public abstract Task<QueryAnalysisResult> ProcessAsync(QueryAnalysisResult result);
    public virtual Task ShutdownAsync() => Task.CompletedTask;
}

/// <summary>
/// Example plugin that adds custom issue detection.
/// </summary>
public class CustomIssueDetectionPlugin : AnalysisPluginBase
{
    public override string PluginId => "custom-issues";
    public override string Name => "Custom Issue Detection";
    public override Version Version => new(1, 0, 0);

    public override Task<QueryAnalysisResult> ProcessAsync(QueryAnalysisResult result)
    {
        // Add custom issue detection logic here
        // Example: detect specific pattern in query

        if (result.Query.Contains("DELETE", StringComparison.OrdinalIgnoreCase) &&
            !result.Query.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
        {
            result.Issues.Add(new PerformanceIssue
            {
                IssueType = Constants.IssueType.TableScan,
                Severity = Constants.IssueSeverity.Critical,
                Description = "DELETE without WHERE clause detected - would delete all rows!",
                EstimatedPerformanceImpact = 100.0
            });
        }

        return Task.FromResult(result);
    }
}

/// <summary>
/// Example plugin that enhances results with additional context.
/// </summary>
public class ResultEnhancementPlugin : AnalysisPluginBase
{
    public override string PluginId => "enhancement";
    public override string Name => "Result Enhancement";
    public override Version Version => new(1, 0, 0);

    public override Task<QueryAnalysisResult> ProcessAsync(QueryAnalysisResult result)
    {
        // Enhance result with additional metadata
        if (result.Metadata == null)
        {
            result.Metadata = new();
        }

        result.Metadata["plugin-enhanced"] = DateTime.UtcNow;
        result.Metadata["plugin-version"] = Version.ToString();

        return Task.FromResult(result);
    }
}
