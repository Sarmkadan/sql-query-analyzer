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
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="collector"/> is <see langword="null"/>.</exception>
    public static int GetSuccessfulAnalyses(this PerformanceMetricCollector collector)
    {
        ArgumentNullException.ThrowIfNull(collector);

        return collector.GetReport().SuccessfulAnalyses;
    }

    /// <summary>
    /// Gets the total number of failed analyses.
    /// </summary>
    /// <param name="collector">The performance metric collector instance.</param>
    /// <returns>Total count of failed analyses.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="collector"/> is <see langword="null"/>.</exception>
    public static int GetFailedAnalyses(this PerformanceMetricCollector collector)
    {
        ArgumentNullException.ThrowIfNull(collector);

        return collector.GetReport().FailedAnalyses;
    }

    /// <summary>
    /// Gets the total number of cache hits.
    /// </summary>
    /// <param name="collector">The performance metric collector instance.</param>
    /// <returns>Total count of cache hits.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="collector"/> is <see langword="null"/>.</exception>
    public static int GetCacheHits(this PerformanceMetricCollector collector)
    {
        ArgumentNullException.ThrowIfNull(collector);

        return collector.GetReport().CacheHits;
    }

    /// <summary>
    /// Gets the total number of cache misses.
    /// </summary>
    /// <param name="collector">The performance metric collector instance.</param>
    /// <returns>Total count of cache misses.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="collector"/> is <see langword="null"/>.</exception>
    public static int GetCacheMisses(this PerformanceMetricCollector collector)
    {
        ArgumentNullException.ThrowIfNull(collector);

        return collector.GetReport().CacheMisses;
    }

    /// <summary>
    /// Gets the average execution time in milliseconds across all analyses.
    /// </summary>
    /// <param name="collector">The performance metric collector instance.</param>
    /// <returns>The average execution time in milliseconds.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="collector"/> is <see langword="null"/>.</exception>
    public static double GetAverageExecutionTimeMs(this PerformanceMetricCollector collector)
    {
        ArgumentNullException.ThrowIfNull(collector);

        return collector.GetReport().AverageExecutionTimeMs;
    }

    /// <summary>
    /// Gets the cache hit ratio as a percentage.
    /// </summary>
    /// <param name="collector">The performance metric collector instance.</param>
    /// <returns>The cache hit ratio percentage.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="collector"/> is <see langword="null"/>.</exception>
    public static double GetCacheHitRatio(this PerformanceMetricCollector collector)
    {
        ArgumentNullException.ThrowIfNull(collector);

        return collector.GetReport().CacheHitRatio;
    }

    /// <summary>
    /// Gets the success rate as a percentage.
    /// </summary>
    /// <param name="collector">The performance metric collector instance.</param>
    /// <returns>The success rate percentage.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="collector"/> is <see langword="null"/>.</exception>
    public static double GetSuccessRate(this PerformanceMetricCollector collector)
    {
        ArgumentNullException.ThrowIfNull(collector);

        return collector.GetReport().SuccessRate;
    }

    /// <summary>
    /// Gets the throughput in queries analyzed per second.
    /// </summary>
    /// <param name="collector">The performance metric collector instance.</param>
    /// <returns>The throughput in queries per second.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="collector"/> is <see langword="null"/>.</exception>
    public static double GetThroughput(this PerformanceMetricCollector collector)
    {
        ArgumentNullException.ThrowIfNull(collector);

        return collector.GetReport().Throughput;
    }

    /// <summary>
    /// Gets the 50th percentile (median) execution time in milliseconds.
    /// </summary>
    /// <param name="collector">The performance metric collector instance.</param>
    /// <returns>The 50th percentile execution time in milliseconds.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="collector"/> is <see langword="null"/>.</exception>
    public static double GetP50ExecutionTimeMs(this PerformanceMetricCollector collector)
    {
        ArgumentNullException.ThrowIfNull(collector);

        return collector.GetReport().P50ExecutionTimeMs;
    }

    /// <summary>
    /// Gets the 95th percentile execution time in milliseconds.
    /// </summary>
    /// <param name="collector">The performance metric collector instance.</param>
    /// <returns>The 95th percentile execution time in milliseconds.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="collector"/> is <see langword="null"/>.</exception>
    public static double GetP95ExecutionTimeMs(this PerformanceMetricCollector collector)
    {
        ArgumentNullException.ThrowIfNull(collector);

        return collector.GetReport().P95ExecutionTimeMs;
    }

    /// <summary>
    /// Gets the 99th percentile execution time in milliseconds.
    /// </summary>
    /// <param name="collector">The performance metric collector instance.</param>
    /// <returns>The 99th percentile execution time in milliseconds.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="collector"/> is <see langword="null"/>.</exception>
    public static double GetP99ExecutionTimeMs(this PerformanceMetricCollector collector)
    {
        ArgumentNullException.ThrowIfNull(collector);

        return collector.GetReport().P99ExecutionTimeMs;
    }

    /// <summary>
    /// Gets the total number of issues detected across all analyses.
    /// </summary>
    /// <param name="collector">The performance metric collector instance.</param>
    /// <returns>The total number of issues detected.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="collector"/> is <see langword="null"/>.</exception>
    public static int GetTotalIssuesDetected(this PerformanceMetricCollector collector)
    {
        ArgumentNullException.ThrowIfNull(collector);

        return collector.GetReport().TotalIssuesDetected;
    }
}
