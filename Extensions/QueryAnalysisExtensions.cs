#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
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
    /// <param name="severity">The severity level to filter by.</param>
    /// <returns>List of performance issues matching the specified severity.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
    public static List<PerformanceIssue> GetIssuesBySeverity(
        this QueryAnalysisResult result,
        Constants.IssueSeverity severity)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.Issues.Where(i => i.Severity == severity).ToList();
    }

    /// <summary>
    /// Gets all issues of a specific type.
    /// </summary>
    /// <param name="issueType">The issue type to filter by.</param>
    /// <returns>List of performance issues matching the specified type.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
    public static List<PerformanceIssue> GetIssuesByType(
        this QueryAnalysisResult result,
        Constants.IssueType issueType)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.Issues.Where(i => i.IssueType == issueType).ToList();
    }

    /// <summary>
    /// Gets top N issues by performance impact.
    /// </summary>
    /// <param name="count">Maximum number of issues to return. Default is 5.</param>
    /// <returns>List of top performance issues ordered by impact.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is less than 1.</exception>
    public static List<PerformanceIssue> GetTopIssuesByImpact(
        this QueryAnalysisResult result,
        int count = 5)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 1);

        return result.Issues
            .OrderByDescending(i => i.EstimatedPerformanceImpact)
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Gets top N index suggestions by performance gain.
    /// </summary>
    /// <param name="count">Maximum number of suggestions to return. Default is 3.</param>
    /// <returns>List of top index suggestions ordered by estimated performance gain.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is less than 1.</exception>
    public static List<IndexSuggestion> GetTopSuggestions(
        this QueryAnalysisResult result,
        int count = 3)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 1);

        return result.IndexSuggestions
            .OrderByDescending(s => s.EstimatedPerformanceGain)
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Checks if result has any issues of critical severity.
    /// </summary>
    /// <returns><see langword="true"/> if critical issues exist; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
    public static bool HasCriticalProblems(this QueryAnalysisResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.HasCriticalIssues;
    }

    /// <summary>
    /// Checks if result meets minimum performance threshold.
    /// </summary>
    /// <param name="threshold">Minimum acceptable performance score (0-100). Default is 70.0.</param>
    /// <returns><see langword="true"/> if performance meets or exceeds threshold; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
    public static bool MeetsPerformanceThreshold(this QueryAnalysisResult result, double threshold = 70.0)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.PerformanceScore >= threshold;
    }

    /// <summary>
    /// Gets issue summary grouped by type.
    /// </summary>
    /// <returns>Dictionary mapping issue types to their occurrence counts.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
    public static Dictionary<Constants.IssueType, int> GetIssueSummary(this QueryAnalysisResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.Issues
            .GroupBy(i => i.IssueType)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    /// <summary>
    /// Calculates percentage improvement if all suggestions are implemented.
    /// </summary>
    /// <returns>Potential performance score improvement as a percentage point value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
    public static double GetPotentialImprovement(this QueryAnalysisResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        // Score increases from total optimization potential
        var newScore = Math.Min(100, result.PerformanceScore + result.TotalOptimizationPotential);
        return newScore - result.PerformanceScore;
    }

    /// <summary>
    /// Gets criticality level (0-10 scale).
    /// 0 = no issues, 10 = critical problems.
    /// </summary>
    /// <returns>Criticality level from 0 to 10.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
    public static int GetCriticalityLevel(this QueryAnalysisResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var criticalCount = result.Issues.Count(i => i.Severity == Constants.IssueSeverity.Critical);
        var warningCount = result.Issues.Count(i => i.Severity == Constants.IssueSeverity.Warning);

        var score = criticalCount * 3 + warningCount * 1;
        return Math.Min(10, score);
    }

    /// <summary>
    /// Gets a human-readable recommendation based on analysis.
    /// </summary>
    /// <returns>Recommendation string based on performance score and issues.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
    public static string GetRecommendation(this QueryAnalysisResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

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
    /// <param name="results">Collection of analysis results to merge.</param>
    /// <returns>Merged analysis result with averaged scores and combined issues/suggestions.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="results"/> is <see langword="null"/>.</exception>
    public static QueryAnalysisResult Merge(
        this IEnumerable<QueryAnalysisResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

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
    /// <returns>Dictionary containing key-value pairs for serialization.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
    public static Dictionary<string, object> ExportAsJson(this QueryAnalysisResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

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
    /// <param name="complexity">The complexity level to filter by.</param>
    /// <returns>List of results matching the specified complexity.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="results"/> is <see langword="null"/>.</exception>
    public static List<QueryAnalysisResult> FilterByComplexity(
        this IEnumerable<QueryAnalysisResult> results,
        Constants.QueryComplexity complexity)
    {
        ArgumentNullException.ThrowIfNull(results);
        return results.Where(r => r.Complexity == complexity).ToList();
    }

    /// <summary>
    /// Filters results by performance score threshold.
    /// </summary>
    /// <param name="minScore">Minimum performance score (inclusive).</param>
    /// <param name="maxScore">Maximum performance score (inclusive). Default is 100.</param>
    /// <returns>List of results within the specified score range.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="results"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="minScore"/> is greater than <paramref name="maxScore"/>.</exception>
    public static List<QueryAnalysisResult> FilterByScore(
        this IEnumerable<QueryAnalysisResult> results,
        double minScore,
        double maxScore = 100)
    {
        ArgumentNullException.ThrowIfNull(results);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minScore, maxScore);

        return results
            .Where(r => r.PerformanceScore >= minScore && r.PerformanceScore <= maxScore)
            .ToList();
    }

    /// <summary>
    /// Filters results that have critical issues.
    /// </summary>
    /// <returns>List of results containing critical issues.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="results"/> is <see langword="null"/>.</exception>
    public static List<QueryAnalysisResult> WithCriticalIssues(
        this IEnumerable<QueryAnalysisResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);
        return results.Where(r => r.HasCriticalIssues).ToList();
    }

    /// <summary>
    /// Orders results by performance score (worst first).
    /// </summary>
    /// <returns>Ordered sequence of results by ascending performance score.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="results"/> is <see langword="null"/>.</exception>
    public static IOrderedEnumerable<QueryAnalysisResult> OrderByPerformance(
        this IEnumerable<QueryAnalysisResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);
        return results.OrderBy(r => r.PerformanceScore);
    }

    /// <summary>
    /// Gets overall statistics for a batch of results.
    /// </summary>
    /// <param name="results">Collection of analysis results.</param>
    /// <returns><see cref="BatchStatistics"/> object containing batch metrics.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="results"/> is <see langword="null"/>.</exception>
    public static BatchStatistics GetBatchStatistics(this IEnumerable<QueryAnalysisResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

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