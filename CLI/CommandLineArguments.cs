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
    /// <summary>
    /// Gets or sets the query.
    /// </summary>
    public string? Query { get; set; }
    /// <summary>
    /// Gets or sets the query file.
    /// </summary>
    public string? QueryFile { get; set; }
    /// <summary>
    /// Gets or sets the output format.
    /// </summary>
    public string? OutputFormat { get; set; } = "json";
    /// <summary>
    /// Gets or sets the output path.
    /// </summary>
    public string? OutputPath { get; set; }
    /// <summary>
    /// Gets or sets the database connection.
    /// </summary>
    public string? DatabaseConnection { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether verbose.
    /// </summary>
    public bool Verbose { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether generate report.
    /// </summary>
    public bool GenerateReport { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether batch mode.
    /// </summary>
    public bool BatchMode { get; set; }
    /// <summary>
    /// Gets or sets the config file.
    /// </summary>
    public string? ConfigFile { get; set; }
    /// <summary>
    /// Gets or sets the thread count.
    /// </summary>
    public int ThreadCount { get; set; } = Environment.ProcessorCount;
    /// <summary>
    /// Gets or sets a value indicating whether show execution plan.
    /// </summary>
    public bool ShowExecutionPlan { get; set; }
    /// <summary>
    /// Gets or sets the SQL server version.
    /// </summary>
    public string? SqlServerVersion { get; set; } = "2019";
    /// <summary>
    /// Gets or sets a value indicating whether dry run.
    /// </summary>
    public bool DryRun { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether export suggestions.
    /// </summary>
    public bool ExportSuggestions { get; set; }
    /// <summary>
    /// Gets or sets the filter by severity.
    /// </summary>
    public string? FilterBySeverity { get; set; } // "Critical", "Warning", "Info"
    /// <summary>
    /// Gets or sets the max results.
    /// </summary>
    public int? MaxResults { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether enable cache.
    /// </summary>
    public bool EnableCache { get; set; } = true;
    /// <summary>
    /// Gets or sets the cache path.
    /// </summary>
    public string? CachePath { get; set; }
    /// <summary>
    /// Gets or sets the slow log file.
    /// </summary>
    public string? SlowLogFile { get; set; }
    /// <summary>
    /// Gets or sets the slow log format.
    /// </summary>
    public string SlowLogFormat { get; set; } = "mysql";
    /// <summary>
    /// Gets or sets a value indicating whether parse slow log.
    /// </summary>
    public bool ParseSlowLog { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether show help.
    /// </summary>
    public bool ShowHelp { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether show version.
    /// </summary>
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

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    public override string ToString() => $"CommandLineArguments {{ Query = {Query}, QueryFile = {QueryFile}, OutputFormat = {OutputFormat}, OutputPath = {OutputPath}, DatabaseConnection = {DatabaseConnection}, Verbose = {Verbose} }}";
}
