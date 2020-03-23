#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.Linq;
using SqlQueryAnalyzer.Constants;

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Represents the complete analysis result of a SQL query, including performance score,
/// detected issues, index suggestions, and execution plan details.
/// </summary>
public sealed class QueryAnalysisResult
{
    /// <summary>Unique identifier for this analysis run.</summary>
    public string QueryId { get; set; } = Guid.NewGuid().ToString();
    /// <summary>The original SQL query text that was analyzed.</summary>
    public string Query { get; set; } = string.Empty;
    /// <summary>UTC timestamp when the analysis was performed.</summary>
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    /// <summary>Assessed complexity level of the query (Simple, Medium, Complex, VeryComplex).</summary>
    public QueryComplexity Complexity { get; set; } = QueryComplexity.Medium;
    /// <summary>Overall performance score from 0 (worst) to 100 (best).</summary>
    public double PerformanceScore { get; set; }
    /// <summary>Estimated query execution time based on heuristic analysis.</summary>
    public TimeSpan EstimatedExecutionTime { get; set; }

    /// <summary>List of performance issues detected in the query.</summary>
    public List<PerformanceIssue> Issues { get; set; } = [];
    /// <summary>Suggested indexes that could improve query performance.</summary>
    public List<IndexSuggestion> IndexSuggestions { get; set; } = [];
    /// <summary>Parsed execution plan, or null if plan analysis was not performed.</summary>
    public QueryPlan? ExecutionPlan { get; set; }
    /// <summary>Detailed statistics about the query structure (table count, join count, etc.).</summary>
    public QueryStatistics Statistics { get; set; } = new();

    /// <summary>Whether any issues with Critical severity were found.</summary>
    public bool HasCriticalIssues => Issues.Any(i => i.Severity == IssueSeverity.Critical);

    /// <summary>Sum of estimated performance gain percentages from all index suggestions.</summary>
    public double TotalOptimizationPotential =>
        IndexSuggestions.Sum(s => s.EstimatedPerformanceGain);

    // Get summary of analysis
    public string GetSummary()
    {
        var criticalCount = Issues.Count(i => i.Severity == IssueSeverity.Critical);
        var warningCount = Issues.Count(i => i.Severity == IssueSeverity.Warning);
        var infoCount = Issues.Count(i => i.Severity == IssueSeverity.Info);

        return $"Score: {PerformanceScore:F1}/100 | " +
               $"Issues: {criticalCount} critical, {warningCount} warnings, {infoCount} info | " +
               $"Optimization: {TotalOptimizationPotential:F1}%";
    }

    // Export result as structured format
    public Dictionary<string, object> ToJsonDictionary() =>
        new()
        {
            { "queryId", QueryId },
            { "query", Query },
            { "analyzedAt", AnalyzedAt },
            { "complexity", Complexity.ToString() },
            { "performanceScore", PerformanceScore },
            { "estimatedExecutionTime", EstimatedExecutionTime.TotalMilliseconds },
            { "issueCount", Issues.Count },
            { "indexSuggestions", IndexSuggestions.Count },
            { "hasCriticalIssues", HasCriticalIssues },
            { "optimizationPotential", TotalOptimizationPotential }
        };
}
