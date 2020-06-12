#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlQueryAnalyzer.Utilities;

/// <summary>
/// Extension methods for <see cref="PerformanceMetricCollector"/> providing additional
/// utility and convenience methods for working with performance metrics.
/// </summary>
public static class PerformanceMetricCollectorExtensions
{
    /// <summary>
    /// Gets the total number of successful analyses.
    /// </summary>
    /// <param name="collector">The performance metric collector instance.</param>
    /// <returns>Total count of successful analyses.</returns>
    public static int GetSuccessfulAnalyses(this PerformanceMetricCollector collector)
    {
        ArgumentNullException.ThrowIfNull(collector);

        var report = collector.GetReport();
        return report.SuccessfulAnalyses;
    }

    /// <summary>
    /// Gets the total number of failed analyses.
    /// </summary>
    /// <param name="collector">The performance metric collector instance.</param>
    /// <returns>Total count of failed analyses.</returns>
    public static int GetFailedAnalyses(this PerformanceMetricCollector collector)
    {
        ArgumentNullException.ThrowIfNull(collector);

        var report = collector.GetReport();
        return report.FailedAnalyses;
    }

    /// <summary>
    /// Gets the total number of cache hits.
    /// </summary>
    /// <param name="collector">The performance metric collector instance.</param>
    /// <returns>Total count of cache hits.</returns>
    public static int GetCacheHits(this PerformanceMetricCollector collector)
    {
        ArgumentNullException.ThrowIfNull(collector);

        var report = collector.GetReport();
        return report.CacheHits;
    }

    /// <summary>
    /// Gets the total number of cache misses.
    /// </summary>
    /// <param name="collector">The performance metric collector instance.</param>
    /// <returns>Total count of cache misses.</returns>
    public static int GetCacheMisses(this PerformanceMetricCollector collector)
    {
        ArgumentNullException.ThrowIfNull(collector);

        var report = collector.GetReport();
        return report.CacheMisses;
    }
}