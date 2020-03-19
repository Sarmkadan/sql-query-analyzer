#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Extensions;

/// <summary>
/// Extension methods for query analysis results.
/// Provides convenient operations on analysis data.
/// </summary>
public static class QueryAnalysisExtensions
{
    /// <summary>
    /// Gets all issues of a specific severity level.
    /// </summary>
    public static List<PerformanceIssue> GetIssuesBySeverity(
        this QueryAnalysisResult result,
        Constants.IssueSeverity severity)
    {
        return result.Issues.Where(i => i.Severity == severity).ToList();
    }

    /// <summary>
    /// Gets all issues of a specific type.
    /// </summary>
    public static List<PerformanceIssue> GetIssuesByType(
        this QueryAnalysisResult result,
        Constants.IssueType issueType)
    {
        return result.Issues.Where(i => i.IssueType == issueType).ToList();
    }

    /// <summary>
    /// Gets top N issues by performance impact.
    /// </summary>
    public static List<PerformanceIssue> GetTopIssuesByImpact(
        this QueryAnalysisResult result,
        int count = 5)
    {
        return result.Issues
            .OrderByDescending(i => i.EstimatedPerformanceImpact)
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Gets top N index suggestions by performance gain.
    /// </summary>
    public static List<IndexSuggestion> GetTopSuggestions(
        this QueryAnalysisResult result,
        int count = 3)
    {
        return result.IndexSuggestions
            .OrderByDescending(s => s.EstimatedPerformanceGain)
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Checks if result has any issues of critical severity.
    /// </summary>
    public static bool HasCriticalProblems(this QueryAnalysisResult result) =>
        result.HasCriticalIssues;

    /// <summary>
    /// Checks if result meets minimum performance threshold.
    /// </summary>
    public static bool MeetsPerformanceThreshold(this QueryAnalysisResult result, double threshold = 70.0) =>
        result.PerformanceScore >= threshold;

    /// <summary>
    /// Gets issue summary grouped by type.
    /// </summary>
    public static Dictionary<Constants.IssueType, int> GetIssueSummary(this QueryAnalysisResult result) =>
        result.Issues
            .GroupBy(i => i.IssueType)
            .ToDictionary(g => g.Key, g => g.Count());

    /// <summary>
    /// Calculates percentage improvement if all suggestions are implemented.
    /// </summary>
    public static double GetPotentialImprovement(this QueryAnalysisResult result)
    {
        // Score increases from total optimization potential
        var newScore = Math.Min(100, result.PerformanceScore + result.TotalOptimizationPotential);
        return newScore - result.PerformanceScore;
    }

    /// <summary>
    /// Gets criticality level (0-10 scale).
    /// 0 = no issues, 10 = critical problems.
    /// </summary>
    public static int GetCriticalityLevel(this QueryAnalysisResult result)
    {
        var criticalCount = result.Issues.Count(i => i.Severity == Constants.IssueSeverity.Critical);
        var warningCount = result.Issues.Count(i => i.Severity == Constants.IssueSeverity.Warning);

        var score = criticalCount * 3 + warningCount * 1;
        return Math.Min(10, score);
    }

    /// <summary>
    /// Gets a human-readable recommendation based on analysis.
    /// </summary>
    public static string GetRecommendation(this QueryAnalysisResult result)
    {
        if (result.PerformanceScore >= 90)
            return "Query is well-optimized. No action required.";

        if (result.PerformanceScore >= 70)
            return "Query has minor optimization opportunities. Consider implementing index suggestions.";

        var topIssue = result.GetTopIssuesByImpact(1).FirstOrDefault();
        if (topIssue != null)
            return $"Query has performance issues. Priority: Address {topIssue.IssueType}.";

        return "Query analysis complete. Review results for optimization opportunities.";
    }

    /// <summary>
    /// Merges multiple analysis results.
    /// Useful for batch analysis summaries.
    /// </summary>
    public static QueryAnalysisResult Merge(
        this IEnumerable<QueryAnalysisResult> results)
    {
        var resultList = results.ToList();
        if (!resultList.Any())
        {
            return new QueryAnalysisResult();
        }

        var merged = new QueryAnalysisResult
        {
            Query = "BATCH ANALYSIS",
            PerformanceScore = resultList.Average(r => r.PerformanceScore),
            Complexity = resultList.Max(r => r.Complexity),
            Issues = resultList.SelectMany(r => r.Issues).ToList(),
            IndexSuggestions = resultList.SelectMany(r => r.IndexSuggestions).Distinct().ToList()
        };

        return merged;
    }

    /// <summary>
    /// Exports analysis result to dictionary for serialization.
    /// </summary>
    public static Dictionary<string, object> ExportAsJson(this QueryAnalysisResult result)
    {
        return new Dictionary<string, object>
        {
            { "id", result.QueryId },
            { "score", result.PerformanceScore },
            { "complexity", result.Complexity.ToString() },
            { "issues", result.Issues.Count },
            { "criticalIssues", result.Issues.Count(i => i.Severity == Constants.IssueSeverity.Critical) },
            { "suggestions", result.IndexSuggestions.Count },
            { "potential", result.TotalOptimizationPotential }
        };
    }
}

/// <summary>
/// Extension methods for collections of analysis results.
/// </summary>
public static class AnalysisResultCollectionExtensions
{
    /// <summary>
    /// Filters results by complexity level.
    /// </summary>
    public static List<QueryAnalysisResult> FilterByComplexity(
        this IEnumerable<QueryAnalysisResult> results,
        Constants.QueryComplexity complexity)
    {
        return results.Where(r => r.Complexity == complexity).ToList();
    }

    /// <summary>
    /// Filters results by performance score threshold.
    /// </summary>
    public static List<QueryAnalysisResult> FilterByScore(
        this IEnumerable<QueryAnalysisResult> results,
        double minScore,
        double maxScore = 100)
    {
        return results
            .Where(r => r.PerformanceScore >= minScore && r.PerformanceScore <= maxScore)
            .ToList();
    }

    /// <summary>
    /// Filters results that have critical issues.
    /// </summary>
    public static List<QueryAnalysisResult> WithCriticalIssues(
        this IEnumerable<QueryAnalysisResult> results)
    {
        return results.Where(r => r.HasCriticalIssues).ToList();
    }

    /// <summary>
    /// Orders results by performance score (worst first).
    /// </summary>
    public static IOrderedEnumerable<QueryAnalysisResult> OrderByPerformance(
        this IEnumerable<QueryAnalysisResult> results)
    {
        return results.OrderBy(r => r.PerformanceScore);
    }

    /// <summary>
    /// Gets overall statistics for a batch of results.
    /// </summary>
    public static BatchStatistics GetBatchStatistics(this IEnumerable<QueryAnalysisResult> results)
    {
        var resultList = results.ToList();
        return new BatchStatistics
        {
            TotalQueries = resultList.Count,
            AverageScore = resultList.Average(r => r.PerformanceScore),
            WorstScore = resultList.Min(r => r.PerformanceScore),
            BestScore = resultList.Max(r => r.PerformanceScore),
            TotalIssuesFound = resultList.Sum(r => r.Issues.Count),
            QueriesWithIssues = resultList.Count(r => r.Issues.Count > 0)
        };
    }
}

/// <summary>
/// Statistics for a batch of analysis results.
/// </summary>
public sealed class BatchStatistics
{
    public int TotalQueries { get; set; }
    public double AverageScore { get; set; }
    public double WorstScore { get; set; }
    public double BestScore { get; set; }
    public int TotalIssuesFound { get; set; }
    public int QueriesWithIssues { get; set; }

    public override string ToString() =>
        $"Batch Stats: {TotalQueries} queries, avg={AverageScore:F1}, " +
        $"range={WorstScore:F0}-{BestScore:F0}, {TotalIssuesFound} issues in {QueriesWithIssues} queries";
}
