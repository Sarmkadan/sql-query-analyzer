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
/// Represents the complete analysis result of a SQL query
/// </summary>
public sealed class QueryAnalysisResult
{
    public string QueryId { get; set; } = Guid.NewGuid().ToString();
    public string Query { get; set; } = string.Empty;
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    public QueryComplexity Complexity { get; set; } = QueryComplexity.Medium;
    public double PerformanceScore { get; set; } // 0-100, higher is better
    public TimeSpan EstimatedExecutionTime { get; set; }

    public List<PerformanceIssue> Issues { get; set; } = [];
    public List<IndexSuggestion> IndexSuggestions { get; set; } = [];
    public QueryPlan? ExecutionPlan { get; set; }
    public QueryStatistics Statistics { get; set; } = new();

    // Calculate if query has critical issues
    public bool HasCriticalIssues => Issues.Any(i => i.Severity == IssueSeverity.Critical);

    // Calculate total estimated impact
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
