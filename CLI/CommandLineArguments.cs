#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SqlQueryAnalyzer.CLI;

/// <summary>
/// Represents parsed command-line arguments for the analyzer.
/// Supports query analysis, batch processing, and configuration overrides.
/// </summary>
public class CommandLineArguments
{
    public string? Query { get; set; }
    public string? QueryFile { get; set; }
    public string? OutputFormat { get; set; } = "json";
    public string? OutputPath { get; set; }
    public string? DatabaseConnection { get; set; }
    public bool Verbose { get; set; }
    public bool GenerateReport { get; set; }
    public bool BatchMode { get; set; }
    public string? ConfigFile { get; set; }
    public int ThreadCount { get; set; } = Environment.ProcessorCount;
    public bool ShowExecutionPlan { get; set; }
    public string? SqlServerVersion { get; set; } = "2019";
    public bool DryRun { get; set; }
    public bool ExportSuggestions { get; set; }
    public string? FilterBySeverity { get; set; } // "Critical", "Warning", "Info"
    public int? MaxResults { get; set; }
    public bool EnableCache { get; set; } = true;
    public string? CachePath { get; set; }
    public bool ShowHelp { get; set; }
    public bool ShowVersion { get; set; }

    /// <summary>
    /// Validates that all required arguments are present and valid.
    /// Throws ArgumentException if validation fails.
    /// </summary>
    public void Validate()
    {
        if (!ShowHelp && !ShowVersion && string.IsNullOrEmpty(Query) && string.IsNullOrEmpty(QueryFile))
        {
            throw new ArgumentException("Either --query or --query-file must be provided");
        }

        if (!string.IsNullOrEmpty(OutputFormat) &&
            !new[] { "json", "csv", "xml", "html", "text" }.Contains(OutputFormat.ToLower()))
        {
            throw new ArgumentException($"Invalid output format: {OutputFormat}. Supported: json, csv, xml, html, text");
        }

        if (ThreadCount < 1 || ThreadCount > Environment.ProcessorCount * 2)
        {
            throw new ArgumentException($"Thread count must be between 1 and {Environment.ProcessorCount * 2}");
        }

        if (!string.IsNullOrEmpty(FilterBySeverity) &&
            !new[] { "Critical", "Warning", "Info" }.Contains(FilterBySeverity))
        {
            throw new ArgumentException($"Invalid severity filter: {FilterBySeverity}");
        }
    }

    /// <summary>
    /// Determines if the analysis should run synchronously (dry-run or single query).
    /// </summary>
    public bool ShouldRunSync => DryRun || (Query != null && !BatchMode);

    /// <summary>
    /// Gets the effective number of parallel tasks based on mode and available resources.
    /// </summary>
    public int GetEffectiveThreadCount() => BatchMode ? ThreadCount : 1;
}
