// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Captures execution statistics for a query
/// </summary>
public class QueryStatistics
{
    public int ExecutionCount { get; set; }
    public TimeSpan TotalExecutionTime { get; set; }
    public TimeSpan AverageExecutionTime => TimeSpan.FromMilliseconds(
        ExecutionCount > 0 ? TotalExecutionTime.TotalMilliseconds / ExecutionCount : 0);
    public TimeSpan MinimumExecutionTime { get; set; }
    public TimeSpan MaximumExecutionTime { get; set; }

    // I/O metrics
    public long TotalLogicalReads { get; set; }
    public long TotalPhysicalReads { get; set; }
    public long TotalLogicalWrites { get; set; }
    public long AverageLogicalReads => ExecutionCount > 0 ? TotalLogicalReads / ExecutionCount : 0;

    // Row counts
    public int RowsAffected { get; set; }
    public int AverageRowsReturned { get; set; }
    public int MaxRowsReturned { get; set; }

    // CPU metrics
    public TimeSpan TotalCpuTime { get; set; }
    public TimeSpan AverageCpuTime => TimeSpan.FromMilliseconds(
        ExecutionCount > 0 ? TotalCpuTime.TotalMilliseconds / ExecutionCount : 0);

    // Wait statistics
    public TimeSpan TotalWaitTime { get; set; }
    public string MostCommonWaitType { get; set; } = string.Empty;

    // Memory
    public int PeakMemoryUsageMB { get; set; }
    public int AverageMemoryUsageMB { get; set; }

    // Compilation and plan cache
    public DateTime LastCompilationTime { get; set; }
    public bool IsCached { get; set; }
    public string? CacheKey { get; set; }
    public int PlanHandle { get; set; }

    // Data collection
    public DateTime FirstExecution { get; set; } = DateTime.UtcNow;
    public DateTime LastExecution { get; set; } = DateTime.UtcNow;

    // Calculate if query is inefficient
    public bool IsInefficient =>
        AverageExecutionTime.TotalMilliseconds > 1000 ||
        AverageLogicalReads > 10000 ||
        TotalPhysicalReads > 1000;

    // Calculate query efficiency rating
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
    public string GetPerformanceSummary() =>
        $"Executions: {ExecutionCount} | " +
        $"Avg Time: {AverageExecutionTime.TotalMilliseconds:F1}ms | " +
        $"Logical Reads: {AverageLogicalReads:N0} | " +
        $"Efficiency: {GetEfficiencyRating():F1}%";

    // Get optimization recommendations based on statistics
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
}
