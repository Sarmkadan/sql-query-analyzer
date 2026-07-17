#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;

namespace SqlQueryAnalyzer.CLI;

/// <summary>
/// Provides extension methods for <see cref="CommandLineArguments"/> to simplify
/// common operations and enhance type safety when working with command-line arguments.
/// </summary>
public static class CommandLineArgumentsExtensions
{
    /// <summary>
    /// Determines if the analysis should output to a file based on the provided arguments.
    /// </summary>
    /// <param name="args">The command line arguments to check.</param>
    /// <returns><see langword="true"/> if output should be written to a file; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="args"/> is <see langword="null"/>.</exception>
    public static bool ShouldWriteToFile(this CommandLineArguments args)
    {
        ArgumentNullException.ThrowIfNull(args);
        return !string.IsNullOrEmpty(args.OutputPath) && args.OutputFormat is not (null or "console");
    }

    /// <summary>
    /// Gets the effective output file path, combining OutputPath with default extensions
    /// based on the output format if no extension is provided.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    /// <returns>The resolved output file path with appropriate extension, or <see langword="null"/> if no output path is specified.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="args"/> is <see langword="null"/>.</exception>
    public static string? GetOutputFilePathWithExtension(this CommandLineArguments args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (string.IsNullOrEmpty(args.OutputPath))
        {
            return null;
        }

        var outputPath = args.OutputPath.Trim();

        // If path already has an extension, return as-is
        if (Path.HasExtension(outputPath))
        {
            return outputPath;
        }

        // Append appropriate extension based on format
        var format = args.OutputFormat?.ToLowerInvariant() ?? "json";
        return format switch
        {
            "json" => $"{outputPath}.json",
            "csv" => $"{outputPath}.csv",
            "xml" => $"{outputPath}.xml",
            "html" => $"{outputPath}.html",
            "text" => $"{outputPath}.txt",
            _ => $"{outputPath}.{format}"
        };
    }

    /// <summary>
    /// Determines if verbose logging should be enabled based on the arguments.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    /// <returns><see langword="true"/> if verbose logging is enabled; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="args"/> is <see langword="null"/>.</exception>
    public static bool IsVerboseEnabled(this CommandLineArguments args)
    {
        ArgumentNullException.ThrowIfNull(args);
        return args.Verbose || args.GenerateReport;
    }

    /// <summary>
    /// Gets the effective database connection string, prioritizing explicit connection
    /// over config file if both are specified.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    /// <returns>The effective database connection string, or <see langword="null"/> if neither is specified.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="args"/> is <see langword="null"/>.</exception>
    public static string? GetEffectiveConnectionString(this CommandLineArguments args)
    {
        ArgumentNullException.ThrowIfNull(args);
        return args.DatabaseConnection ?? args.ConfigFile;
    }

    /// <summary>
    /// Determines if caching should be enabled based on the arguments.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    /// <returns><see langword="true"/> if caching is enabled; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="args"/> is <see langword="null"/>.</exception>
    public static bool IsCacheEnabled(this CommandLineArguments args)
    {
        ArgumentNullException.ThrowIfNull(args);
        return args.EnableCache && !string.IsNullOrEmpty(args.CachePath);
    }

    /// <summary>
    /// Gets the effective SQL Server version as a normalized version string.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    /// <returns>The normalized SQL Server version string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="args"/> is <see langword="null"/>.</exception>
    public static string GetNormalizedSqlServerVersion(this CommandLineArguments args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (string.IsNullOrEmpty(args.SqlServerVersion))
        {
            return "2019";
        }

        var version = args.SqlServerVersion.Trim();

        // Normalize common version formats
        return version switch
        {
            "2017" or "2019" or "2022" => version,
            "14" => "2017",
            "15" => "2019",
            "16" => "2022",
            _ when version.StartsWith("20", StringComparison.Ordinal) => version,
            _ => "2019" // Default fallback
        };
    }

    /// <summary>
    /// Gets the effective severity filter as a normalized collection of severity levels.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    /// <returns>A read-only list of normalized severity levels to filter by.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="args"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> GetNormalizedSeverityFilter(this CommandLineArguments args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (string.IsNullOrEmpty(args.FilterBySeverity))
        {
            return Array.Empty<string>();
        }

        var severity = args.FilterBySeverity.Trim();
        return severity switch
        {
            "Critical" => new[] { "Critical" },
            "Warning" => new[] { "Warning", "Critical" },
            "Info" => new[] { "Info", "Warning", "Critical" },
            _ => Array.Empty<string>()
        };
    }

    /// <summary>
    /// Determines if the analysis should include execution plan analysis.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    /// <returns><see langword="true"/> if execution plan analysis should be performed; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="args"/> is <see langword="null"/>.</exception>
    public static bool ShouldAnalyzeExecutionPlan(this CommandLineArguments args)
    {
        ArgumentNullException.ThrowIfNull(args);
        return args.ShowExecutionPlan || args.GenerateReport;
    }

    /// <summary>
    /// Gets the effective maximum results limit, ensuring it's within valid bounds.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    /// <returns>The effective maximum results limit, or <see langword="null"/> if not specified or unlimited.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="args"/> is <see langword="null"/>.</exception>
    public static int? GetEffectiveMaxResults(this CommandLineArguments args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (!args.MaxResults.HasValue || args.MaxResults <= 0)
        {
            return null;
        }

        // Cap at a reasonable maximum to prevent resource exhaustion
        const int maxAllowed = 100000;
        return Math.Min(args.MaxResults.Value, maxAllowed);
    }

    /// <summary>
    /// Determines if suggestions should be exported based on the arguments.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    /// <returns><see langword="true"/> if suggestions should be exported; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="args"/> is <see langword="null"/>.</exception>
    public static bool ShouldExportSuggestions(this CommandLineArguments args)
    {
        ArgumentNullException.ThrowIfNull(args);
        return args.ExportSuggestions && !args.DryRun;
    }
}