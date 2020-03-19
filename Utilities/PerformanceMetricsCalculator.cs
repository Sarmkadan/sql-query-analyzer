#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using SqlQueryAnalyzer.Models;
using ModelIndex = SqlQueryAnalyzer.Models.Index;

namespace SqlQueryAnalyzer.Utilities;

/// <summary>
/// Calculates performance metrics for queries and indexes
/// </summary>
public static class PerformanceMetricsCalculator
{
    // Calculate combined performance score
    public static double CalculateCombinedScore(QueryAnalysisResult analysis, double weight = 1.0)
    {
        var baseScore = analysis.PerformanceScore;
        var issueImpact = analysis.Issues.Sum(i => (double)i.Severity * i.EstimatedPerformanceImpact);
        var optimizationPotential = analysis.TotalOptimizationPotential * 0.5;

        return Math.Max(0, Math.Min(100, baseScore - issueImpact + optimizationPotential)) * weight;
    }

    // Estimate total optimization potential
    public static double EstimateTotalOptimization(List<PerformanceIssue> issues, List<IndexSuggestion> suggestions)
    {
        var issueOptimization = issues.Sum(i => i.EstimatedPerformanceImpact) * 0.8;
        var indexOptimization = suggestions.Sum(s => s.EstimatedPerformanceGain) * 0.6;

        return Math.Min(95, issueOptimization + indexOptimization);
    }

    // Calculate query complexity score
    public static int CalculateComplexityScore(DatabaseQuery query)
    {
        var score = 0;

        // Line count
        score += query.LineCount switch
        {
            < 5 => 10,
            < 10 => 20,
            < 20 => 30,
            < 50 => 40,
            _ => 50
        };

        // Number of tables
        score += query.ReferencedTables.Count switch
        {
            0 => 0,
            1 => 10,
            2 => 15,
            3 => 20,
            4 => 30,
            _ => 40
        };

        // Join complexity
        score += query.JoinConditions.Count switch
        {
            0 => 0,
            1 => 10,
            2 => 15,
            3 => 25,
            _ => 35
        };

        // Parameter count
        score += query.Parameters.Count switch
        {
            0 => 0,
            <= 3 => 5,
            <= 5 => 10,
            _ => 15
        };

        return Math.Min(100, score);
    }

    // Calculate index usage score (0-100)
    public static double CalculateIndexUsageScore(Index index)
    {
        if (!index.IsValid())
            return 0;

        var score = 100.0;

        // Penalty for not being used
        if (index.TotalUsageCount == 0)
            score -= 50;

        // Penalty for fragmentation
        score -= (index.FragmentationPercentage / 100) * 30;

        // Penalty for high maintenance cost
        var maintCost = index.EstimateCost();
        score -= (maintCost / 10.0) * 20;

        return Math.Max(0, score);
    }

    // Calculate index maintenance effort
    public static int CalculateMaintenanceEffort(List<Index> indexes)
    {
        if (indexes.Count == 0)
            return 0;

        var totalEffort = 0;
        foreach (var index in indexes)
        {
            if (index.IsFragmented || index.LastStatisticsUpdate == null ||
                index.LastStatisticsUpdate < DateTime.UtcNow.AddDays(-7))
            {
                totalEffort += index.EstimateCost();
            }
        }

        return totalEffort;
    }

    // Get performance trend (simulating trend calculation)
    public static string GetPerformanceTrend(List<QueryAnalysisResult> analysisHistory)
    {
        if (analysisHistory.Count < 2)
            return "Insufficient data";

        var recent = analysisHistory.TakeLast(3).Average(a => a.PerformanceScore);
        var older = analysisHistory.SkipLast(3).Take(3).Average(a => a.PerformanceScore);

        var trend = recent - older;
        return trend > 5 ? "Improving" : trend < -5 ? "Degrading" : "Stable";
    }

    // Calculate query execution time distribution
    public static Dictionary<string, int> CalculateExecutionTimeDistribution(QueryStatistics stats)
    {
        var distribution = new Dictionary<string, int>
        {
            { "< 10ms", 0 },
            { "10-100ms", 0 },
            { "100-500ms", 0 },
            { "500-1000ms", 0 },
            { "> 1000ms", 0 }
        };

        var avgMs = stats.AverageExecutionTime.TotalMilliseconds;

        if (avgMs < 10)
            distribution["< 10ms"] = stats.ExecutionCount;
        else if (avgMs < 100)
            distribution["10-100ms"] = stats.ExecutionCount;
        else if (avgMs < 500)
            distribution["100-500ms"] = stats.ExecutionCount;
        else if (avgMs < 1000)
            distribution["500-1000ms"] = stats.ExecutionCount;
        else
            distribution["> 1000ms"] = stats.ExecutionCount;

        return distribution;
    }

    // Calculate ROI for index creation
    public static double CalculateIndexROI(IndexSuggestion suggestion, long tableSizeKB)
    {
        // ROI = (Performance gain * Query savings) / (Index size + Maintenance cost)
        var queryBenefit = suggestion.EstimatedPerformanceGain * 10; // Arbitrary benefit unit
        var indexCost = (suggestion.EstimatedIndexSizeKB ?? 1000) +
                       ((suggestion.EstimatedMaintenanceCost ?? 5) * 100);

        if (indexCost == 0)
            return 0;

        return (queryBenefit / indexCost) * 100;
    }

    // Predict query execution time based on statistics
    public static TimeSpan PredictExecutionTime(QueryStatistics stats, int estimatedRows)
    {
        var averageMs = stats.AverageExecutionTime.TotalMilliseconds;
        var readsPerRow = stats.ExecutionCount > 0
            ? (double)stats.TotalLogicalReads / (stats.RowsAffected > 0 ? stats.RowsAffected : 1)
            : 1.0;

        var predictedMs = averageMs * (estimatedRows / (double)(stats.RowsAffected + 1));

        return TimeSpan.FromMilliseconds(Math.Min(predictedMs, 60000)); // Cap at 1 minute
    }
}
