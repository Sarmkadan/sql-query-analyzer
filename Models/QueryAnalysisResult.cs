#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.Linq;
using SqlQueryAnalyzer.Constants;
using SqlQueryAnalyzer.Scoring;

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Represents the complete analysis result of a SQL query.
/// </summary>
public sealed class QueryAnalysisResult
{
    /// <summary>
    /// Gets or sets the unique identifier for the analysis result.
    /// </summary>
    public string QueryId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the SQL query string.
    /// </summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SQL query text (alias for <see cref="Query"/>).
    /// </summary>
    public string QueryText
    {
        get => Query;
        set => Query = value;
    }

    /// <summary>
    /// Gets or sets the timestamp when the analysis was performed.
    /// </summary>
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the complexity level of the query.
    /// </summary>
    public QueryComplexity Complexity { get; set; } = QueryComplexity.Medium;

    /// <summary>
    /// Gets or sets the performance score (0-100, where higher is better).
    /// </summary>
    public double PerformanceScore { get; set; }

    /// <summary>
    /// Gets or sets the estimated execution time for the query.
    /// </summary>
    public TimeSpan EstimatedExecutionTime { get; set; }

    /// <summary>
    /// Gets or sets the list of performance issues detected.
    /// </summary>
    public List<PerformanceIssue> Issues { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of index suggestions.
    /// </summary>
    public List<IndexSuggestion> IndexSuggestions { get; set; } = [];

    /// <summary>
    /// Gets or sets the execution plan for the query, if available.
    /// </summary>
    public QueryPlan? ExecutionPlan { get; set; }

    /// <summary>
    /// Gets or sets the query execution statistics.
    /// </summary>
    public QueryStatistics Statistics { get; set; } = new();

    /// <summary>
    /// Gets or sets additional metadata associated with the analysis.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = [];

    /// <summary>
    /// Gets the overall complexity score.
    /// Higher values indicate queries that are more costly to optimize.
    /// </summary>
    public int ComplexityScore => QueryComplexityScorer.ComputeScore(this);

    /// <summary>
    /// Gets a value indicating whether the query has any critical issues.
    /// </summary>
    public bool HasCriticalIssues => Issues.Any(i => i.Severity == IssueSeverity.Critical);

    /// <summary>
    /// Gets the total estimated performance gain from index suggestions.
    /// </summary>
    public double TotalOptimizationPotential =>
        IndexSuggestions.Sum(s => s.EstimatedPerformanceGain);

    /// <summary>
    /// Generates a summary string of the analysis results.
    /// </summary>
    /// <returns>A summary string.</returns>
    public string GetSummary()
    {
        var criticalCount = Issues.Count(i => i.Severity == IssueSeverity.Critical);
        var warningCount = Issues.Count(i => i.Severity == IssueSeverity.Warning);
        var infoCount = Issues.Count(i => i.Severity == IssueSeverity.Info);

        return $"Score: {PerformanceScore:F1}/100 | " +
               $"Issues: {criticalCount} critical, {warningCount} warnings, {infoCount} info | " +
               $"Optimization: {TotalOptimizationPotential:F1}%";
    }

    /// <summary>
    /// Exports the analysis result as a structured dictionary for JSON serialization.
    /// </summary>
    /// <returns>A dictionary representation of the analysis result.</returns>
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
            { "optimizationPotential", TotalOptimizationPotential },
            { "complexityScore", ComplexityScore }
        };
}
