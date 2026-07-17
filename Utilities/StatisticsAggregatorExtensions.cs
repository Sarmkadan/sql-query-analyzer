using System;
using System.Globalization;

namespace SqlQueryAnalyzer.Utilities;

/// <summary>
/// Provides extension methods for the <see cref="StatisticsAggregator"/> class
/// to format and calculate derived statistics from aggregated query analysis results.
/// </summary>
public static class StatisticsAggregatorExtensions
{
    /// <summary>
    /// Gets the average performance score formatted as a string using the specified format provider.
    /// </summary>
    /// <param name="aggregator">The statistics aggregator instance. Cannot be null.</param>
    /// <param name="format">The numeric format string. Defaults to "F2". Cannot be null or empty.</param>
    /// <returns>The formatted average performance score using invariant culture.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="aggregator"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="format"/> is null or empty.</exception>
    public static string GetAverageScoreFormatted(this StatisticsAggregator aggregator, string format = "F2")
    {
        ArgumentNullException.ThrowIfNull(aggregator);
        ArgumentException.ThrowIfNullOrEmpty(format);

        return aggregator.GetAveragePerformanceScore().ToString(format, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Calculates the ratio of total issues found per total queries analyzed.
    /// Returns 0.0 when no queries are present to avoid division by zero.
    /// </summary>
    /// <param name="aggregator">The statistics aggregator instance. Cannot be null.</param>
    /// <returns>The density of issues found per query (issues/query).</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="aggregator"/> is null.</exception>
    public static double GetIssueDensity(this StatisticsAggregator aggregator)
    {
        ArgumentNullException.ThrowIfNull(aggregator);

        var summary = aggregator.GetSummary();
        return summary.TotalQueries == 0
            ? 0.0
            : (double)summary.TotalIssuesFound / summary.TotalQueries;
    }
}
