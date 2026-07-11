#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace SqlQueryAnalyzer.CLI;

/// <summary>
/// Provides validation helpers for <see cref="CliApplicationHost"/> instances.
/// Validates the public members: Query, Arguments, Result, ShouldContinue, and Metadata.
/// </summary>
public static class CliApplicationHostValidation
{
    /// <summary>
    /// Validates the public members of a <see cref="CliApplicationHost"/> instance.
    /// Returns a list of human-readable problems found during validation.
    /// </summary>
    /// <param name="value">The CliApplicationHost instance to validate.</param>
    /// <returns>An enumerable of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this CliApplicationHost value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate Query (from AnalysisContext)
        if (value.Query is null or { Length: 0 })
        {
            problems.Add("Query is null or empty");
        }

        // Validate Arguments (from AnalysisContext)
        if (value.Arguments is null)
        {
            problems.Add("Arguments is null");
        }
        else
        {
            // Validate CommandLineArguments properties that affect execution
            if (value.Arguments.ShowHelp || value.Arguments.ShowVersion)
            {
                // These are valid states, no additional validation needed
            }
            else if (string.IsNullOrEmpty(value.Arguments.Query) && string.IsNullOrEmpty(value.Arguments.QueryFile))
            {
                problems.Add("Either Query or QueryFile must be provided in Arguments");
            }

            if (!string.IsNullOrEmpty(value.Arguments.OutputFormat) &&
                !new[] { "json", "csv", "xml", "html", "text" }.Contains(value.Arguments.OutputFormat.ToLowerInvariant()))
            {
                problems.Add($"Invalid output format: {value.Arguments.OutputFormat}. Supported: json, csv, xml, html, text");
            }

            if (value.Arguments.ThreadCount < 1 || value.Arguments.ThreadCount > Environment.ProcessorCount * 2)
            {
                problems.Add($"Thread count must be between 1 and {Environment.ProcessorCount * 2}, but was {value.Arguments.ThreadCount}");
            }

            if (!string.IsNullOrEmpty(value.Arguments.FilterBySeverity) &&
                !new[] { "Critical", "Warning", "Info" }.Contains(value.Arguments.FilterBySeverity))
            {
                problems.Add($"Invalid severity filter: {value.Arguments.FilterBySeverity}");
            }

            if (value.Arguments.SqlServerVersion is not null)
            {
                // Basic SQL Server version format validation (major.minor.build.revision)
                var versionParts = value.Arguments.SqlServerVersion.Split('.', StringSplitOptions.RemoveEmptyEntries);
                if (versionParts.Length == 0 || !int.TryParse(versionParts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var major) || major < 2000 || major > 2030)
                {
                    problems.Add($"Invalid SQL Server version format or range: {value.Arguments.SqlServerVersion}");
                }
            }
        }

        // Validate Result (from AnalysisContext)
        if (value.Result is null)
        {
            problems.Add("Result is null");
        }
        else
        {
            // Validate QueryAnalysisResult properties
            if (value.Result.PerformanceScore < 0 || value.Result.PerformanceScore > 100)
            {
                problems.Add($"PerformanceScore must be between 0 and 100, but was {value.Result.PerformanceScore}");
            }

            if (value.Result.AnalyzedAt == default)
            {
                problems.Add("AnalyzedAt date is default (uninitialized)");
            }

            if (value.Result.Complexity <= 0)
            {
                problems.Add($"Complexity must be positive, but was {value.Result.Complexity}");
            }

            if (value.Result.Issues is null)
            {
                problems.Add("Issues collection is null");
            }
        }

        // Validate ShouldContinue (from AnalysisContext)
        // ShouldContinue is a boolean flag, so it's always valid

        // Validate Metadata (from AnalysisContext)
        if (value.Metadata is null)
        {
            problems.Add("Metadata dictionary is null");
        }
        else
        {
            // Check for null keys or values in metadata
            foreach (var kvp in value.Metadata)
            {
                if (kvp.Key is null)
                {
                    problems.Add("Metadata contains a null key");
                    break;
                }

                if (kvp.Value is null)
                {
                    problems.Add($"Metadata contains null value for key: {kvp.Key}");
                    break;
                }
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="CliApplicationHost"/> instance is valid.
    /// </summary>
    /// <param name="value">The CliApplicationHost instance to check.</param>
    /// <returns>True if the instance is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this CliApplicationHost value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="CliApplicationHost"/> instance is valid.
    /// Throws an <see cref="ArgumentException"/> with a detailed message listing all validation problems.
    /// </summary>
    /// <param name="value">The CliApplicationHost instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of problems.</exception>
    public static void EnsureValid(this CliApplicationHost value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException("CliApplicationHost validation failed:\n" + string.Join("\n", problems));
        }
    }
}