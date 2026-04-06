// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Utilities;

/// <summary>
/// Aggregates statistics from multiple analysis results.
/// Computes trends, patterns, and summary metrics across batches.
/// Useful for understanding performance patterns over time.
/// </summary>
public class StatisticsAggregator
{
    private readonly List<QueryAnalysisResult> _results = new();

    /// <summary>
    /// Adds a result to the aggregation set.
    /// </summary>
    public void AddResult(QueryAnalysisResult result)
    {
        if (result != null)
            _results.Add(result);
    }

    /// <summary>
    /// Adds multiple results at once.
    /// </summary>
    public void AddResults(IEnumerable<QueryAnalysisResult> results)
    {
        _results.AddRange(results);
    }

    /// <summary>
    /// Calculates average performance score across all results.
    /// </summary>
    public double GetAveragePerformanceScore() =>
        _results.Count == 0 ? 0 : _results.Average(r => r.PerformanceScore);

    /// <summary>
    /// Calculates minimum performance score found.
    /// Identifies worst performing queries.
    /// </summary>
    public double GetMinPerformanceScore() =>
        _results.Count == 0 ? 0 : _results.Min(r => r.PerformanceScore);

    /// <summary>
    /// Calculates maximum performance score found.
    /// Identifies best performing queries.
    /// </summary>
    public double GetMaxPerformanceScore() =>
        _results.Count == 0 ? 0 : _results.Max(r => r.PerformanceScore);

    /// <summary>
    /// Calculates standard deviation of performance scores.
    /// Shows variability in query performance.
    /// </summary>
    public double GetPerformanceScoreStdDev()
    {
        if (_results.Count < 2)
            return 0;

        var avg = GetAveragePerformanceScore();
        var variance = _results.Average(r => Math.Pow(r.PerformanceScore - avg, 2));
        return Math.Sqrt(variance);
    }

    /// <summary>
    /// Returns percentile of performance scores.
    /// Useful for identifying outliers (95th percentile, etc).
    /// </summary>
    public double GetPercentile(double percentile)
    {
        if (_results.Count == 0)
            return 0;

        var sorted = _results.OrderBy(r => r.PerformanceScore).ToList();
        var index = (int)Math.Ceiling(percentile / 100.0 * sorted.Count) - 1;
        return sorted[Math.Max(0, index)].PerformanceScore;
    }

    /// <summary>
    /// Counts issues by severity across all results.
    /// Returns breakdown of critical/warning/info issues.
    /// </summary>
    public Dictionary<Constants.IssueSeverity, int> GetIssueCounts()
    {
        return new Dictionary<Constants.IssueSeverity, int>
        {
            { Constants.IssueSeverity.Critical, _results.Sum(r => r.Issues.Count(i => i.Severity == Constants.IssueSeverity.Critical)) },
            { Constants.IssueSeverity.Warning, _results.Sum(r => r.Issues.Count(i => i.Severity == Constants.IssueSeverity.Warning)) },
            { Constants.IssueSeverity.Info, _results.Sum(r => r.Issues.Count(i => i.Severity == Constants.IssueSeverity.Info)) }
        };
    }

    /// <summary>
    /// Counts issues by type across all results.
    /// Shows which issue types are most common.
    /// </summary>
    public Dictionary<Constants.IssueType, int> GetIssueTypeFrequency()
    {
        var frequency = new Dictionary<Constants.IssueType, int>();

        foreach (var result in _results)
        {
            foreach (var issue in result.Issues)
            {
                if (frequency.ContainsKey(issue.IssueType))
                    frequency[issue.IssueType]++;
                else
                    frequency[issue.IssueType] = 1;
            }
        }

        return frequency.OrderByDescending(x => x.Value)
            .ToDictionary(x => x.Key, x => x.Value);
    }

    /// <summary>
    /// Identifies most common optimization opportunities.
    /// Returns index suggestions ranked by frequency.
    /// </summary>
    public Dictionary<string, int> GetMostCommonOptimizations()
    {
        var opportunities = new Dictionary<string, int>();

        foreach (var result in _results)
        {
            foreach (var suggestion in result.IndexSuggestions)
            {
                var key = suggestion.ColumnName ?? "Unknown";
                if (opportunities.ContainsKey(key))
                    opportunities[key]++;
                else
                    opportunities[key] = 1;
            }
        }

        return opportunities.OrderByDescending(x => x.Value)
            .ToDictionary(x => x.Key, x => x.Value);
    }

    /// <summary>
    /// Calculates total optimization potential across all queries.
    /// Sums performance gains from all suggested optimizations.
    /// </summary>
    public double GetTotalOptimizationPotential() =>
        _results.Sum(r => r.TotalOptimizationPotential);

    /// <summary>
    /// Returns distribution of query complexity levels.
    /// Shows breakdown: Simple, Low, Medium, High, VeryHigh, Extreme.
    /// </summary>
    public Dictionary<Constants.QueryComplexity, int> GetComplexityDistribution()
    {
        var distribution = new Dictionary<Constants.QueryComplexity, int>();

        foreach (var complexity in Enum.GetValues(typeof(Constants.QueryComplexity)).Cast<Constants.QueryComplexity>())
        {
            distribution[complexity] = _results.Count(r => r.Complexity == complexity);
        }

        return distribution;
    }

    /// <summary>
    /// Generates summary report of aggregated statistics.
    /// </summary>
    public AggregationSummary GetSummary()
    {
        var issueCounts = GetIssueCounts();
        var totalIssues = issueCounts.Values.Sum();

        return new AggregationSummary
        {
            TotalQueries = _results.Count,
            AverageScore = GetAveragePerformanceScore(),
            MinScore = GetMinPerformanceScore(),
            MaxScore = GetMaxPerformanceScore(),
            ScoreStdDev = GetPerformanceScoreStdDev(),
            TotalIssuesFound = totalIssues,
            CriticalIssues = issueCounts[Constants.IssueSeverity.Critical],
            WarningIssues = issueCounts[Constants.IssueSeverity.Warning],
            InfoIssues = issueCounts[Constants.IssueSeverity.Info],
            TotalOptimizationPotential = GetTotalOptimizationPotential(),
            QueriesWithIssues = _results.Count(r => r.HasCriticalIssues),
            AverageBugDensity = totalIssues / (double)Math.Max(1, _results.Count)
        };
    }

    /// <summary>
    /// Clears all aggregated results.
    /// </summary>
    public void Clear() => _results.Clear();

    /// <summary>
    /// Returns count of aggregated results.
    /// </summary>
    public int Count => _results.Count;
}

/// <summary>
/// Summary of aggregated statistics.
/// </summary>
public class AggregationSummary
{
    public int TotalQueries { get; set; }
    public double AverageScore { get; set; }
    public double MinScore { get; set; }
    public double MaxScore { get; set; }
    public double ScoreStdDev { get; set; }
    public int TotalIssuesFound { get; set; }
    public int CriticalIssues { get; set; }
    public int WarningIssues { get; set; }
    public int InfoIssues { get; set; }
    public double TotalOptimizationPotential { get; set; }
    public int QueriesWithIssues { get; set; }
    public double AverageBugDensity { get; set; }

    public override string ToString() =>
        $"Analysis Summary:\n" +
        $"  Total Queries: {TotalQueries}\n" +
        $"  Avg Score: {AverageScore:F1}/100 (±{ScoreStdDev:F1})\n" +
        $"  Range: {MinScore:F0}-{MaxScore:F0}\n" +
        $"  Issues: {CriticalIssues} critical, {WarningIssues} warnings, {InfoIssues} info\n" +
        $"  Queries with Critical Issues: {QueriesWithIssues}\n" +
        $"  Potential Optimization: {TotalOptimizationPotential:F1}%\n" +
        $"  Avg Bug Density: {AverageBugDensity:F2} issues/query";
}
