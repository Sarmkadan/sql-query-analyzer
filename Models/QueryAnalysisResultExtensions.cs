#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using SqlQueryAnalyzer.Constants;

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Provides extension methods for <see cref="QueryAnalysisResult"/> to enhance functionality
/// and provide convenient operations for query analysis results.
/// </summary>
public static class QueryAnalysisResultExtensions
{
    /// <summary>
    /// Determines whether the query analysis result indicates a high-performance query.
    /// </summary>
    /// <param name="result">The query analysis result to check.</param>
    /// <returns>True if the query has a performance score of 90 or higher and no critical issues; otherwise, false.</returns>
    public static bool IsHighPerformance(this QueryAnalysisResult result)
    {
        if (result == null)
            throw new ArgumentNullException(nameof(result));

        return result.PerformanceScore >= 90 && !result.HasCriticalIssues;
    }

    /// <summary>
    /// Determines whether the query analysis result indicates a query that needs optimization.
    /// </summary>
    /// <param name="result">The query analysis result to check.</param>
    /// <returns>True if the query has a performance score below 70 or has critical issues; otherwise, false.</returns>
    public static bool NeedsOptimization(this QueryAnalysisResult result)
    {
        if (result == null)
            throw new ArgumentNullException(nameof(result));

        return result.PerformanceScore < 70 || result.HasCriticalIssues;
    }

    /// <summary>
    /// Gets the severity level of the query based on its performance score and issues.
    /// </summary>
    /// <param name="result">The query analysis result.</param>
    /// <returns>A string representing the overall severity level.</returns>
    public static string GetSeverityLevel(this QueryAnalysisResult result)
    {
        if (result == null)
            throw new ArgumentNullException(nameof(result));

        if (result.HasCriticalIssues)
            return "Critical";


        if (result.PerformanceScore < 60)
            return "High";

        if (result.PerformanceScore < 80)
            return "Medium";

        return "Low";
    }

    /// <summary>
    /// Creates a deep copy of the query analysis result to prevent mutation of the original.
    /// </summary>
    /// <param name="result">The query analysis result to copy.</param>
    /// <returns>A new <see cref="QueryAnalysisResult"/> instance with the same values.</returns>
    public static QueryAnalysisResult DeepCopy(this QueryAnalysisResult result)
    {
        if (result == null)
            throw new ArgumentNullException(nameof(result));

        var copy = new QueryAnalysisResult
        {
            QueryId = result.QueryId,
            Query = result.Query,
            AnalyzedAt = result.AnalyzedAt,
            Complexity = result.Complexity,
            PerformanceScore = result.PerformanceScore,
            EstimatedExecutionTime = result.EstimatedExecutionTime,
            Issues = new List<PerformanceIssue>(result.Issues),
            IndexSuggestions = new List<IndexSuggestion>(result.IndexSuggestions),
            ExecutionPlan = result.ExecutionPlan,
            Statistics = new QueryStatistics
            {
                ExecutionCount = result.Statistics.ExecutionCount,
                TotalCpuTime = result.Statistics.TotalCpuTime,
                TotalLogicalReads = result.Statistics.TotalLogicalReads,
                TotalLogicalWrites = result.Statistics.TotalLogicalWrites,
                TotalExecutionTime = result.Statistics.TotalExecutionTime,
                RowsAffected = result.Statistics.RowsAffected,
                AverageRowsReturned = result.Statistics.AverageRowsReturned,
                MaxRowsReturned = result.Statistics.MaxRowsReturned
            },
            Metadata = new Dictionary<string, object>(result.Metadata)
        };

        return copy;
    }

    /// <summary>
    /// Gets a formatted string representation of the query analysis result.
    /// </summary>
    /// <param name="result">The query analysis result.</param>
    /// <returns>A formatted string with key metrics.</returns>
    public static string FormatSummary(this QueryAnalysisResult result)
    {
        if (result == null)
            throw new ArgumentNullException(nameof(result));

        var severity = result.GetSeverityLevel();
        var severityText = severity switch
        {
            "Critical" => "🔴 CRITICAL",
            "High" => "🟠 HIGH",
            "Medium" => "🟡 MEDIUM",
            "Low" => "🟢 LOW",
            _ => "⚪ UNKNOWN"
        };

        return $"""
Query ID: {result.QueryId}
Query: {result.Query}
Analyzed: {result.AnalyzedAt:yyyy-MM-dd HH:mm:ss}
Complexity: {result.Complexity} (Score: {result.ComplexityScore})
Performance: {result.PerformanceScore:F1}/100
Execution Time: {result.EstimatedExecutionTime.TotalMilliseconds:F0}ms
Severity: {severityText}
Issues: {result.Issues.Count} total ({result.Issues.Count(i => i.Severity == IssueSeverity.Critical)} critical)
Index Suggestions: {result.IndexSuggestions.Count} ({result.TotalOptimizationPotential:F1}% potential)
""";
    }

    /// <summary>
    /// Serializes the query analysis result to a JSON string with formatting.
    /// </summary>
    /// <param name="result">The query analysis result.</param>
    /// <returns>A JSON string representation of the result.</returns>
    public static string ToJsonString(this QueryAnalysisResult result, bool indented = false)
    {
        if (result == null)
            throw new ArgumentNullException(nameof(result));

        var options = indented
            ? new JsonSerializerOptions { WriteIndented = true }
            : new JsonSerializerOptions { WriteIndented = false };

        return JsonSerializer.Serialize(result.ToJsonDictionary(), options);
    }
}