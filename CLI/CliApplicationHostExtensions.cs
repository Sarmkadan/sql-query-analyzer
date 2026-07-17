#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;
using SqlQueryAnalyzer.Models;
using SqlQueryAnalyzer.Constants;

namespace SqlQueryAnalyzer.CLI;

/// <summary>
/// Provides extension methods for <see cref="CliApplicationHost"/> to enhance CLI functionality
/// with common operations like result validation, metadata management, and query analysis utilities.
/// </summary>
public static class CliApplicationHostExtensions
{
    /// <summary>
    /// Validates that the analysis result contains issues and returns an enumerable of issues.
    /// </summary>
    /// <param name="host">The CLI application host instance.</param>
    /// <returns>An enumerable of performance issues; empty if no issues found or result is null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="host"/> is null.</exception>
    public static IEnumerable<PerformanceIssue> GetIssues(this CliApplicationHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        return host.Result?.Issues ?? Enumerable.Empty<PerformanceIssue>();
    }

    /// <summary>
    /// Determines whether the analysis result has any critical issues.
    /// </summary>
    /// <param name="host">The CLI application host instance.</param>
    /// <returns>True if critical issues exist; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="host"/> is null.</exception>
    public static bool HasCriticalIssues(this CliApplicationHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        return host.GetIssues()
            .Any(issue => issue.Severity == IssueSeverity.Critical);
    }

    /// <summary>
    /// Adds or updates metadata with the specified key and value.
    /// </summary>
    /// <param name="host">The CLI application host instance.</param>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="host"/> is null.
    /// Thrown when <paramref name="key"/> is null or empty.
    /// Thrown when <paramref name="value"/> is null.
    /// </exception>
    public static void SetMetadata(this CliApplicationHost host, string key, object value)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(value);

        host.Metadata[key] = value;
    }

    /// <summary>
    /// Gets metadata value by key or returns the default value if key doesn't exist.
    /// </summary>
    /// <typeparam name="T">The type of the metadata value.</typeparam>
    /// <param name="host">The CLI application host instance.</param>
    /// <param name="key">The metadata key.</param>
    /// <returns>The metadata value if found; otherwise the default value for type T.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="host"/> is null.
    /// Thrown when <paramref name="key"/> is null.
    /// </exception>
    public static T? GetMetadata<T>(this CliApplicationHost host, string key)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(key);

        if (host.Metadata.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }

        return default;
    }

    /// <summary>
    /// Gets the performance score as a formatted string with invariant culture.
    /// </summary>
    /// <param name="host">The CLI application host instance.</param>
    /// <returns>A formatted string representation of the performance score; "N/A" if result is null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="host"/> is null.</exception>
    public static string GetPerformanceScoreString(this CliApplicationHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        return host.Result?.PerformanceScore switch
        {
            null => "N/A",
            var score => score.Value.ToString("F1", CultureInfo.InvariantCulture)
        };
    }

    /// <summary>
    /// Gets the total number of issues grouped by severity level.
    /// </summary>
    /// <param name="host">The CLI application host instance.</param>
    /// <returns>A read-only dictionary mapping severity levels to issue counts.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="host"/> is null.</exception>
    public static IReadOnlyDictionary<IssueSeverity, int> GetIssueCountsBySeverity(this CliApplicationHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        return host.GetIssues()
            .GroupBy(issue => issue.Severity)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    /// <summary>
    /// Determines whether the analysis should continue based on the ShouldContinue flag.
    /// </summary>
    /// <param name="host">The CLI application host instance.</param>
    /// <returns>True if analysis should continue; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="host"/> is null.</exception>
    public static bool ShouldContinueAnalysis(this CliApplicationHost host)
    {
        ArgumentNullException.ThrowIfNull(host);
        return host.ShouldContinue;
    }

    /// <summary>
    /// Gets the query text from the host's Query property.
    /// </summary>
    /// <param name="host">The CLI application host instance.</param>
    /// <returns>The query text; <see cref="string.Empty"/> if null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="host"/> is null.</exception>
    public static string GetQueryText(this CliApplicationHost host)
    {
        ArgumentNullException.ThrowIfNull(host);
        return host.Query ?? string.Empty;
    }

    /// <summary>
    /// Gets the command line arguments from the host's Arguments property.
    /// </summary>
    /// <param name="host">The CLI application host instance.</param>
    /// <returns>The command line arguments instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="host"/> is null.</exception>
    public static CommandLineArguments GetCommandLineArguments(this CliApplicationHost host)
    {
        ArgumentNullException.ThrowIfNull(host);
        return host.Arguments;
    }
}