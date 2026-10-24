#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Captures execution statistics for a query
/// </summary>
public sealed class QueryStatistics
{
    /// <summary>
    /// Gets or sets the execution count.
    /// </summary>
    public int ExecutionCount { get; set; }
    /// <summary>
    /// Gets or sets the total execution time.
    /// </summary>
    public TimeSpan TotalExecutionTime { get; set; }
    /// <summary>
    /// The average execution time.
    /// </summary>
    public TimeSpan AverageExecutionTime => TimeSpan.FromMilliseconds(
        ExecutionCount > 0 ? TotalExecutionTime.TotalMilliseconds / ExecutionCount : 0);
    /// <summary>
    /// Gets or sets the minimum execution time.
    /// </summary>
    public TimeSpan MinimumExecutionTime { get; set; }
    /// <summary>
    /// Gets or sets the maximum execution time.
    /// </summary>
    public TimeSpan MaximumExecutionTime { get; set; }

    // I/O metrics
    /// <summary>
    /// Gets or sets the total logical reads.
    /// </summary>
    public long TotalLogicalReads { get; set; }
    /// <summary>
    /// Gets or sets the total physical reads.
    /// </summary>
    public long TotalPhysicalReads { get; set; }
    /// <summary>
    /// Gets or sets the total logical writes.
    /// </summary>
    public long TotalLogicalWrites { get; set; }
    /// <summary>
    /// The average logical reads.
    /// </summary>
    public long AverageLogicalReads => ExecutionCount > 0 ? TotalLogicalReads / ExecutionCount : 0;

    // Row counts
    /// <summary>
    /// Gets or sets the rows affected.
    /// </summary>
    public int RowsAffected { get; set; }
    /// <summary>
    /// Gets or sets the average rows returned.
    /// </summary>
    public int AverageRowsReturned { get; set; }
    /// <summary>
    /// Gets or sets the max rows returned.
    /// </summary>
    public int MaxRowsReturned { get; set; }

    // CPU metrics
    /// <summary>
    /// Gets or sets the total CPU time.
    /// </summary>
    public TimeSpan TotalCpuTime { get; set; }
    /// <summary>
    /// The average CPU time.
    /// </summary>
    public TimeSpan AverageCpuTime => TimeSpan.FromMilliseconds(
        ExecutionCount > 0 ? TotalCpuTime.TotalMilliseconds / ExecutionCount : 0);

    // Wait statistics
    /// <summary>
    /// Gets or sets the total wait time.
    /// </summary>
    public TimeSpan TotalWaitTime { get; set; }
    /// <summary>
    /// Gets or sets the most common wait type.
    /// </summary>
    public string MostCommonWaitType { get; set; } = string.Empty;

    // Memory
    /// <summary>
    /// Gets or sets the peak memory usage MB.
    /// </summary>
    public int PeakMemoryUsageMB { get; set; }
    /// <summary>
    /// Gets or sets the average memory usage MB.
    /// </summary>
    public int AverageMemoryUsageMB { get; set; }

    // Compilation and plan cache
    /// <summary>
    /// Gets or sets the last compilation time.
    /// </summary>
    public DateTime LastCompilationTime { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether is cached.
    /// </summary>
    public bool IsCached { get; set; }
    /// <summary>
    /// Gets or sets the cache key.
    /// </summary>
    public string? CacheKey { get; set; }
    /// <summary>
    /// Gets or sets the plan handle.
    /// </summary>
    public int PlanHandle { get; set; }

    // Data collection
    /// <summary>
    /// Gets or sets the first execution.
    /// </summary>
    public DateTime FirstExecution { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// Gets or sets the last execution.
    /// </summary>
    public DateTime LastExecution { get; set; } = DateTime.UtcNow;

    // Calculate if query is inefficient
    /// <summary>
    /// Gets or sets a value indicating whether is inefficient.
    /// </summary>
    public bool IsInefficient =>
        AverageExecutionTime.TotalMilliseconds > 1000 ||
        AverageLogicalReads > 10000 ||
        TotalPhysicalReads > 1000;

    // Calculate query efficiency rating
    /// <summary>
    /// Get efficiency rating.
    /// </summary>
    public double GetEfficiencyRating()
    {
        var rating = 100.0;

        // Penalize for long execution time
        if (AverageExecutionTime.TotalMilliseconds > 5000)
            rating -= 40;
        else if (AverageExecutionTime.TotalMilliseconds > 1000)
            rating -= 20;

        // Penalize for high I/O
        if (AverageLogicalReads > 50000)
            rating -= 30;
        else if (AverageLogicalReads > 10000)
            rating -= 15;

        // Penalize for high physical reads
        if (TotalPhysicalReads > 10000)
            rating -= 25;

        return Math.Max(0, Math.Min(100, rating));
    }

    // Generate performance summary
    /// <summary>
    /// Get performance summary.
    /// </summary>
    public string GetPerformanceSummary() =>
        $"Executions: {ExecutionCount} | " +
        $"Avg Time: {AverageExecutionTime.TotalMilliseconds:F1}ms | " +
        $"Logical Reads: {AverageLogicalReads:N0} | " +
        $"Efficiency: {GetEfficiencyRating():F1}%";

    // Get optimization recommendations based on statistics
    /// <summary>
    /// Get optimization recommendations.
    /// </summary>
    public List<string> GetOptimizationRecommendations()
    {
        var recommendations = new List<string>();

        if (AverageExecutionTime.TotalMilliseconds > 5000)
            recommendations.Add("Query execution time exceeds 5 seconds - consider indexing or query redesign");

        if (AverageLogicalReads > 100000)
            recommendations.Add("High logical reads - check for missing indexes or inefficient joins");

        if (TotalPhysicalReads > 1000)
            recommendations.Add("High physical reads - ensure indexes are properly configured");

        if (TotalWaitTime.TotalMilliseconds > 0)
            recommendations.Add($"Significant wait time detected ({MostCommonWaitType}) - may indicate contention");

        if (ExecutionCount > 100 && !IsCached)
            recommendations.Add("Query executed 100+ times but not cached - plan caching issue?");

        if (MaxRowsReturned > 100000)
            recommendations.Add("Large result set - consider pagination or filtering");

        return recommendations;
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    public override string ToString()
    {
        return $"QueryStatistics {{ ExecutionCount = {ExecutionCount}, TotalExecutionTime = {TotalExecutionTime}, MinimumExecutionTime = {MinimumExecutionTime}, MaximumExecutionTime = {MaximumExecutionTime}, TotalLogicalReads = {TotalLogicalReads}, TotalPhysicalReads = {TotalPhysicalReads} }}";
    }
}
