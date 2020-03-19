#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Extensions;
using SqlQueryAnalyzer.Formatters;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Export;

/// <summary>
/// Service for exporting analysis results to various file formats.
/// Supports JSON, CSV, XML, HTML, and custom formats.
/// Handles batch exports and compression.
/// </summary>
public sealed class ExportService
{
    private readonly ILogger<ExportService> _logger;
    private readonly Dictionary<string, IResultFormatter> _formatters = new();

    public ExportService(ILogger<ExportService> logger)
    {
        _logger = logger;

        // Register default formatters
        RegisterFormatter("json", new JsonResultFormatter());
        RegisterFormatter("csv", new CsvResultFormatter());
        RegisterFormatter("xml", new XmlResultFormatter());
        RegisterFormatter("html", new HtmlResultFormatter());
        RegisterFormatter("text", new TextResultFormatter());
    }

    /// <summary>
    /// Registers a custom formatter.
    /// </summary>
    public void RegisterFormatter(string format, IResultFormatter formatter)
    {
        _formatters[format.ToLower()] = formatter;
        _logger.LogDebug($"Registered formatter: {format}");
    }

    /// <summary>
    /// Exports single analysis result to file.
    /// </summary>
    public async Task ExportAsync(
        QueryAnalysisResult result,
        string filePath,
        string format = "json")
    {
        try
        {
            if (!_formatters.TryGetValue(format.ToLower(), out var formatter))
            {
                throw new ArgumentException($"Unsupported format: {format}");
            }

            var content = formatter.Format(result);

            // Ensure directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllTextAsync(filePath, content);
            _logger.LogInformation($"Exported analysis to {filePath} ({format})");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Export failed to {filePath}");
            throw;
        }
    }

    /// <summary>
    /// Exports batch of results to file.
    /// </summary>
    public async Task ExportBatchAsync(
        List<QueryAnalysisResult> results,
        string filePath,
        string format = "json")
    {
        // Fix: Prevent ArgumentNullException on results count logging and formatting
        if (results == null)
            throw new ArgumentNullException(nameof(results), "Cannot export a null list of analysis results.");

        try
        {
            if (!_formatters.TryGetValue(format.ToLower(), out var formatter))
            {
                throw new ArgumentException($"Unsupported format: {format}");
            }

            var content = formatter.FormatBatch(results);

            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllTextAsync(filePath, content);
            _logger.LogInformation($"Exported {results.Count} results to {filePath} ({format})");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Batch export failed to {filePath}");
            throw;
        }
    }

    /// <summary>
    /// Exports results to multiple formats simultaneously.
    /// </summary>
    public async Task ExportMultipleFormatsAsync(
        QueryAnalysisResult result,
        string outputDirectory,
        params string[] formats)
    {
        var tasks = formats.Select(format =>
            ExportAsync(result, Path.Combine(outputDirectory, $"analysis.{format}"), format));

        await Task.WhenAll(tasks);
        _logger.LogInformation($"Exported to {formats.Length} formats");
    }

    /// <summary>
    /// Exports analysis with summary report.
    /// </summary>
    public async Task ExportWithReportAsync(
        QueryAnalysisResult result,
        string outputDirectory)
    {
        try
        {
            // Ensure output directory exists
            Directory.CreateDirectory(outputDirectory);

            // Export main analysis
            await ExportAsync(result, Path.Combine(outputDirectory, "analysis.json"), "json");

            // Export HTML report
            var htmlPath = Path.Combine(outputDirectory, "report.html");
            await ExportAsync(result, htmlPath, "html");

            // Export summary
            var summaryPath = Path.Combine(outputDirectory, "summary.txt");
            var summary = GenerateSummary(result);
            await File.WriteAllTextAsync(summaryPath, summary);

            // Export recommendations
            var recommendationsPath = Path.Combine(outputDirectory, "recommendations.txt");
            var recommendations = GenerateRecommendations(result);
            await File.WriteAllTextAsync(recommendationsPath, recommendations);

            _logger.LogInformation($"Complete export package created in {outputDirectory}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Export with report failed");
            throw;
        }
    }

    /// <summary>
    /// Generates text summary of analysis.
    /// </summary>
    private string GenerateSummary(QueryAnalysisResult result)
    {
        var stats = new[] { result }.GetBatchStatistics();

        return $@"
SQL QUERY ANALYSIS SUMMARY
==========================

Performance Score: {result.PerformanceScore:F1}/100
Query Complexity: {result.Complexity}
Analysis Time: {result.AnalyzedAt:yyyy-MM-dd HH:mm:ss}

ISSUES FOUND:
  Critical: {result.GetIssuesBySeverity(Constants.IssueSeverity.Critical).Count}
  Warnings: {result.GetIssuesBySeverity(Constants.IssueSeverity.Warning).Count}
  Info: {result.GetIssuesBySeverity(Constants.IssueSeverity.Info).Count}

OPTIMIZATION POTENTIAL: {result.TotalOptimizationPotential:F1}%

RECOMMENDATION: {result.GetRecommendation()}
";
    }

    /// <summary>
    /// Generates detailed recommendations based on analysis.
    /// </summary>
    private string GenerateRecommendations(QueryAnalysisResult result)
    {
        var recommendations = new List<string>();

        foreach (var issue in result.GetTopIssuesByImpact(5))
        {
            recommendations.Add($"• {issue.IssueType}: {issue.Description}");
            recommendations.Add($"  Impact: {issue.EstimatedPerformanceImpact:F1}%");
        }

        recommendations.Add("\nSUGGESTED INDEXES:");
        foreach (var suggestion in result.GetTopSuggestions(3))
        {
            recommendations.Add($"• CREATE INDEX ON {suggestion.TableName}({suggestion.ColumnName})");
            recommendations.Add($"  Expected gain: {suggestion.EstimatedPerformanceGain:F1}%");
        }

        return string.Join("\n", recommendations);
    }

    /// <summary>
    /// Gets list of supported export formats.
    /// </summary>
    public List<string> GetSupportedFormats()
    {
        return _formatters.Keys.ToList();
    }

    /// <summary>
    /// Checks if format is supported.
    /// </summary>
    public bool IsFormatSupported(string format)
    {
        return _formatters.ContainsKey(format.ToLower());
    }
}

/// <summary>
/// Export configuration options.
/// </summary>
public sealed class ExportOptions
{
    public List<string> Formats { get; set; } = new() { "json" };
    public string OutputDirectory { get; set; } = "./exports";
    public bool IncludeReport { get; set; } = true;
    public bool Compress { get; set; } = false;
    public string? CompressionPassword { get; set; }
}
