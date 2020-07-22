#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Provides extension methods for <see cref="QueryStatistics"/> to enhance query analysis capabilities
/// </summary>
public static class QueryStatisticsExtensions
{
    /// <summary>
    /// Calculates the average logical reads per execution as a formatted string with thousands separator
    /// </summary>
    /// <param name="statistics">The query statistics to analyze</param>
    /// <returns>Formatted string with thousands separator (e.g., "12,345")</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="statistics"/> is null</exception>
    public static string GetAverageLogicalReadsFormatted(this QueryStatistics statistics)
    {
        ArgumentNullException.ThrowIfNull(statistics);

        return statistics.AverageLogicalReads.ToString("N0", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Calculates the total logical reads per second across all executions
    /// </summary>
    /// <param name="statistics">The query statistics to analyze</param>
    /// <returns>Logical reads per second, or 0 if no executions</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="statistics"/> is null</exception>
    public static double GetLogicalReadsPerSecond(this QueryStatistics statistics)
    {
        ArgumentNullException.ThrowIfNull(statistics);

        if (statistics.TotalExecutionTime.TotalSeconds <= 0)
            return 0;

        return statistics.TotalLogicalReads / statistics.TotalExecutionTime.TotalSeconds;
    }

    /// <summary>
    /// Calculates the total CPU time per logical read ratio
    /// </summary>
    /// <param name="statistics">The query statistics to analyze</param>
    /// <returns>CPU time per logical read in milliseconds, or 0 if no logical reads</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="statistics"/> is null</exception>
    public static double GetCpuTimePerLogicalRead(this QueryStatistics statistics)
    {
        ArgumentNullException.ThrowIfNull(statistics);

        if (statistics.TotalLogicalReads <= 0)
            return 0;

        return statistics.TotalCpuTime.TotalMilliseconds / statistics.TotalLogicalReads;
    }

    /// <summary>
    /// Gets a collection of performance metrics as key-value pairs for easy display or serialization
    /// </summary>
    /// <param name="statistics">The query statistics to analyze</param>
    /// <returns>Read-only collection of performance metrics</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="statistics"/> is null</exception>
    public static IReadOnlyList<KeyValuePair<string, string>> GetPerformanceMetrics(
        this QueryStatistics statistics)
    {
        ArgumentNullException.ThrowIfNull(statistics);

        var metrics = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("ExecutionCount", statistics.ExecutionCount.ToString(CultureInfo.InvariantCulture)),
            new KeyValuePair<string, string>("AverageExecutionTimeMs", statistics.AverageExecutionTime.TotalMilliseconds.ToString("F1", CultureInfo.InvariantCulture)),
            new KeyValuePair<string, string>("TotalExecutionTimeMs", statistics.TotalExecutionTime.TotalMilliseconds.ToString("F1", CultureInfo.InvariantCulture)),
            new KeyValuePair<string, string>("MinimumExecutionTimeMs", statistics.MinimumExecutionTime.TotalMilliseconds.ToString("F1", CultureInfo.InvariantCulture)),
            new KeyValuePair<string, string>("MaximumExecutionTimeMs", statistics.MaximumExecutionTime.TotalMilliseconds.ToString("F1", CultureInfo.InvariantCulture)),
            new KeyValuePair<string, string>("TotalLogicalReads", statistics.TotalLogicalReads.ToString("N0", CultureInfo.InvariantCulture)),
            new KeyValuePair<string, string>("AverageLogicalReads", statistics.AverageLogicalReads.ToString("N0", CultureInfo.InvariantCulture)),
            new KeyValuePair<string, string>("TotalPhysicalReads", statistics.TotalPhysicalReads.ToString("N0", CultureInfo.InvariantCulture)),
            new KeyValuePair<string, string>("TotalLogicalWrites", statistics.TotalLogicalWrites.ToString("N0", CultureInfo.InvariantCulture)),
            new KeyValuePair<string, string>("RowsAffected", statistics.RowsAffected.ToString(CultureInfo.InvariantCulture)),
            new KeyValuePair<string, string>("AverageRowsReturned", statistics.AverageRowsReturned.ToString(CultureInfo.InvariantCulture)),
            new KeyValuePair<string, string>("MaxRowsReturned", statistics.MaxRowsReturned.ToString(CultureInfo.InvariantCulture)),
            new KeyValuePair<string, string>("TotalCpuTimeMs", statistics.TotalCpuTime.TotalMilliseconds.ToString("F1", CultureInfo.InvariantCulture)),
            new KeyValuePair<string, string>("TotalWaitTimeMs", statistics.TotalWaitTime.TotalMilliseconds.ToString("F1", CultureInfo.InvariantCulture)),
            new KeyValuePair<string, string>("MostCommonWaitType", statistics.MostCommonWaitType),
            new KeyValuePair<string, string>("PeakMemoryUsageMB", statistics.PeakMemoryUsageMB.ToString(CultureInfo.InvariantCulture)),
            new KeyValuePair<string, string>("AverageMemoryUsageMB", statistics.AverageMemoryUsageMB.ToString(CultureInfo.InvariantCulture)),
            new KeyValuePair<string, string>("LastCompilationTime", statistics.LastCompilationTime.ToString("O", CultureInfo.InvariantCulture)),
            new KeyValuePair<string, string>("IsCached", statistics.IsCached.ToString()),
            new KeyValuePair<string, string>("CacheKey", statistics.CacheKey ?? "null"),
            new KeyValuePair<string, string>("PlanHandle", statistics.PlanHandle.ToString(CultureInfo.InvariantCulture)),
            new KeyValuePair<string, string>("FirstExecution", statistics.FirstExecution.ToString("O", CultureInfo.InvariantCulture)),
            new KeyValuePair<string, string>("LastExecution", statistics.LastExecution.ToString("O", CultureInfo.InvariantCulture))
        };

        return metrics.AsReadOnly();
    }

    /// <summary>
    /// Determines if the query execution pattern indicates a potential parameter sniffing issue
    /// </summary>
    /// <param name="statistics">The query statistics to analyze</param>
    /// <returns>True if potential parameter sniffing is detected, otherwise false</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="statistics"/> is null</exception>
    public static bool HasPotentialParameterSniffing(this QueryStatistics statistics)
    {
        ArgumentNullException.ThrowIfNull(statistics);

        // Parameter sniffing is likely when:
        // 1. High variance in execution times
        // 2. High variance in rows returned
        // 3. Query is cached but has high physical reads

        if (statistics.ExecutionCount < 5)
            return false;

        var timeVariance = statistics.MaximumExecutionTime.TotalMilliseconds - statistics.MinimumExecutionTime.TotalMilliseconds;
        var rowsVariance = statistics.MaxRowsReturned - statistics.AverageRowsReturned;

        // Significant variance in execution times (> 10x difference)
        bool hasTimeVariance = timeVariance > statistics.AverageExecutionTime.TotalMilliseconds * 10;

        // Significant variance in rows returned (> 5x difference)
        bool hasRowsVariance = rowsVariance > statistics.AverageRowsReturned * 5;

        // High physical reads with caching enabled
        bool hasHighPhysicalReadsWithCache = statistics.IsCached && statistics.TotalPhysicalReads > 10000;

        return hasTimeVariance || hasRowsVariance || hasHighPhysicalReadsWithCache;
    }

    /// <summary>
    /// Gets a formatted performance trend indicator based on efficiency rating changes over time
    /// </summary>
    /// <param name="statistics">The query statistics to analyze</param>
    /// <param name="previousEfficiencyRating">The previous efficiency rating (0-100)</param>
    /// <returns>Formatted trend indicator with arrow and color code</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="statistics"/> is null</exception>
    public static string GetPerformanceTrendIndicator(
        this QueryStatistics statistics,
        double previousEfficiencyRating)
    {
        ArgumentNullException.ThrowIfNull(statistics);

        var currentRating = statistics.GetEfficiencyRating();
        var difference = currentRating - previousEfficiencyRating;

        return difference switch
        {
            > 5 => "↗️ <green>Improved</green>",
            > 0 => "↗ <yellow>Slightly Improved</yellow>",
            < -5 => "↘️ <red>Declined</red>",
            < 0 => "↘ <red>Slightly Declined</red>",
            _ => "→ <gray>Stable</gray>"
        };
    }

    /// <summary>
    /// Calculates the total I/O cost as a weighted sum of logical reads, physical reads, and writes
    /// </summary>
    /// <param name="statistics">The query statistics to analyze</param>
    /// <returns>Total I/O cost score</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="statistics"/> is null</exception>
    public static double GetTotalIoCost(this QueryStatistics statistics)
    {
        ArgumentNullException.ThrowIfNull(statistics);

        // Weighted I/O cost calculation
        // Logical reads are typically 1x cost, physical reads are 10x due to disk access,
        // and writes are 5x due to potential logging and storage overhead
        const double logicalReadWeight = 1.0;
        const double physicalReadWeight = 10.0;
        const double logicalWriteWeight = 5.0;

        return (statistics.TotalLogicalReads * logicalReadWeight) +
               (statistics.TotalPhysicalReads * physicalReadWeight) +
               (statistics.TotalLogicalWrites * logicalWriteWeight);
    }

    /// <summary>
    /// Gets a summary of the most expensive execution metrics for bottleneck identification
    /// </summary>
    /// <param name="statistics">The query statistics to analyze</param>
    /// <returns>Formatted string highlighting the most expensive metrics</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="statistics"/> is null</exception>
    public static string GetBottleneckSummary(this QueryStatistics statistics)
    {
        ArgumentNullException.ThrowIfNull(statistics);

        var metrics = new List<(string Name, double Value, string Unit)>
        {
            ("Total Logical Reads", statistics.TotalLogicalReads, "reads"),
            ("Total Physical Reads", statistics.TotalPhysicalReads, "reads"),
            ("Total CPU Time", statistics.TotalCpuTime.TotalMilliseconds, "ms"),
            ("Total Wait Time", statistics.TotalWaitTime.TotalMilliseconds, "ms"),
            ("Peak Memory", statistics.PeakMemoryUsageMB, "MB")
        };

        // Sort by value descending
        var sortedMetrics = metrics.OrderByDescending(m => m.Value).ToList();

        var topMetrics = sortedMetrics.Take(3).ToList();

        var summaryParts = new List<string>();
        foreach (var metric in topMetrics)
        {
            summaryParts.Add($"{metric.Name}: {metric.Value:N0} {metric.Unit}");
        }

        return string.Join(" | ", summaryParts);
    }
}