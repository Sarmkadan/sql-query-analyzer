#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Configuration;
using SqlQueryAnalyzer.Constants;
using SqlQueryAnalyzer.Models;
using SqlQueryAnalyzer.Visualization;

namespace SqlQueryAnalyzer.Services;

/// <summary>
/// Profiles SQL queries by orchestrating execution plan analysis, pipeline-stage timing,
/// resource-usage measurement, and targeted suggestion generation.
/// <para>
/// Integrates with <see cref="IQueryAnalyzerService"/>, <see cref="IQueryPlanAnalyzerService"/>,
/// <see cref="IPerformanceIssueDetectorService"/>, and <see cref="IExecutionPlanVisualizer"/>
/// to produce comprehensive <see cref="QueryProfilerReport"/> instances.
/// </para>
/// </summary>
public sealed class QueryProfilerService : IQueryProfilerService
{
    internal readonly IQueryAnalyzerService _queryAnalyzer;
    internal readonly IQueryPlanAnalyzerService _planAnalyzer;
    internal readonly IPerformanceIssueDetectorService _issueDetector;
    internal readonly IExecutionPlanVisualizer _planVisualizer;
    internal readonly ProfilerSettings _settings;
    private readonly ILogger<QueryProfilerService> _logger;

    public QueryProfilerService(
        IQueryAnalyzerService queryAnalyzer,
        IQueryPlanAnalyzerService planAnalyzer,
        IPerformanceIssueDetectorService issueDetector,
        IExecutionPlanVisualizer planVisualizer,
        ProfilerSettings settings,
        ILogger<QueryProfilerService> logger)
    {
        _queryAnalyzer = queryAnalyzer ?? throw new ArgumentNullException(nameof(queryAnalyzer));
        _planAnalyzer = planAnalyzer ?? throw new ArgumentNullException(nameof(planAnalyzer));
        _issueDetector = issueDetector ?? throw new ArgumentNullException(nameof(issueDetector));
        _planVisualizer = planVisualizer ?? throw new ArgumentNullException(nameof(planVisualizer));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<QueryProfilerReport> ProfileQueryAsync(
        string queryText,
        ProfilerOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queryText);

        if (queryText.Length > _settings.MaxQueryLengthChars)
            throw new ArgumentException(
                $"Query length {queryText.Length} exceeds the configured limit of {_settings.MaxQueryLengthChars} characters.",
                nameof(queryText));

        var query = new DatabaseQuery { QueryText = queryText };
        query.Parse();
        return await ProfileQueryAsync(query, options);
    }

    /// <inheritdoc/>
    public async Task<QueryProfilerReport> ProfileQueryAsync(
        DatabaseQuery query,
        ProfilerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(query);

        options ??= BuildDefaultOptions();

        _logger.LogInformation("Starting profiler run for query {QueryId}", query.QueryId);

        var totalTimer = Stopwatch.StartNew();
        var report = new QueryProfilerReport
        {
            QueryId = query.QueryId,
            QueryText = query.QueryText,
            ProfiledAt = DateTime.UtcNow,
            Options = options
        };

        try
        {
            if (options.WarmUpIterations > 0)
                await ExecuteWarmUpAsync(query, options.WarmUpIterations);

            if (options.CaptureTimings)
            {
                var stages = await MeasureStagesAsync(query);
                report.ExecutionStages.AddRange(stages);
            }

            var analysisResult = await _queryAnalyzer.AnalyzeQueryAsync(query);
            report.AnalysisResult = analysisResult;
            report.PerformanceScore = analysisResult.PerformanceScore;

            if (options.CaptureResourceUsage)
                report.ResourceUsage = TakeResourceSnapshot(report.ExecutionStages);

            if (options.CaptureExecutionPlan && analysisResult.ExecutionPlan != null)
            {
                report.ExecutionPlan = analysisResult.ExecutionPlan;

                if (options.IncludePlanVisualization)
                    report.PlanVisualization = _planVisualizer.Render(analysisResult.ExecutionPlan);
            }

            report.Metrics.AddRange(BuildMetrics(report));
            report.Suggestions = await GenerateSuggestionsAsync(report);

            totalTimer.Stop();
            report.TotalProfilingDurationMs = totalTimer.Elapsed.TotalMilliseconds;

            _logger.LogInformation(
                "Profiling complete — {QueryId} | Score={Score:F1} | Stages={Stages} | Suggestions={Suggestions} | TotalMs={TotalMs:F0}",
                query.QueryId,
                report.PerformanceScore,
                report.ExecutionStages.Count,
                report.Suggestions.Count,
                report.TotalProfilingDurationMs);
        }
        catch (Exception ex)
        {
            totalTimer.Stop();
            report.TotalProfilingDurationMs = totalTimer.Elapsed.TotalMilliseconds;
            report.HasError = true;
            report.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Profiling failed for query {QueryId}", query.QueryId);
        }

        return report;
    }

    /// <inheritdoc/>
    public async Task<List<QueryProfilerReport>> ProfileBatchAsync(
        IEnumerable<DatabaseQuery> queries,
        ProfilerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(queries);

        var queryList = queries.ToList();

        if (queryList.Count > _settings.MaxBatchSize)
            throw new ArgumentException(
                $"Batch size {queryList.Count} exceeds the configured limit of {_settings.MaxBatchSize}. " +
                "Split the batch and submit in smaller chunks.",
                nameof(queries));

        _logger.LogInformation("Batch profiling {Count} queries", queryList.Count);

        var reports = new List<QueryProfilerReport>(queryList.Count);
        foreach (var query in queryList)
            reports.Add(await ProfileQueryAsync(query, options));

        _logger.LogInformation(
            "Batch profiling complete — {Total} reports, {Failed} failed",
            reports.Count,
            reports.Count(r => r.HasError));

        return reports;
    }

    /// <inheritdoc/>
    public async Task<List<ProfilerSuggestion>> GenerateSuggestionsAsync(QueryProfilerReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        if (report.AnalysisResult == null)
            return [];

        var suggestions = new List<ProfilerSuggestion>();

        suggestions.AddRange(BuildIssueBasedSuggestions(report.AnalysisResult));
        suggestions.AddRange(BuildPlanBasedSuggestions(report));
        suggestions.AddRange(BuildStageSuggestions(report.ExecutionStages));
        suggestions.AddRange(BuildResourceSuggestions(report.ResourceUsage));

        var ranked = suggestions
            .OrderBy(s => s.Priority)
            .ThenByDescending(s => s.EstimatedImpactPercent)
            .ToList();

        return await Task.FromResult(ranked);
    }

    /// <inheritdoc/>
    public async Task<ProfileComparison> CompareProfilesAsync(
        QueryProfilerReport baseline,
        QueryProfilerReport candidate)
    {
        ArgumentNullException.ThrowIfNull(baseline);
        ArgumentNullException.ThrowIfNull(candidate);

        var comparison = new ProfileComparison
        {
            BaselineQueryId = baseline.QueryId,
            CandidateQueryId = candidate.QueryId,
            ComparedAt = DateTime.UtcNow
        };

        comparison.ScoreDelta = candidate.PerformanceScore - baseline.PerformanceScore;
        comparison.IsRegression = comparison.ScoreDelta < -_settings.RegressionThreshold;
        comparison.IsImprovement = comparison.ScoreDelta > _settings.ImprovementThreshold;

        comparison.BaselineTotalMs = baseline.ExecutionStages.Sum(s => s.DurationMs);
        comparison.CandidateTotalMs = candidate.ExecutionStages.Sum(s => s.DurationMs);
        comparison.TimingDeltaMs = comparison.CandidateTotalMs - comparison.BaselineTotalMs;
        comparison.TimingDeltaPercent = comparison.BaselineTotalMs > 0
            ? comparison.TimingDeltaMs / comparison.BaselineTotalMs * 100.0
            : 0;

        comparison.BaselineIssueCount = baseline.AnalysisResult?.Issues.Count ?? 0;
        comparison.CandidateIssueCount = candidate.AnalysisResult?.Issues.Count ?? 0;

        comparison.MetricDeltas = DiffMetrics(baseline.Metrics, candidate.Metrics);
        comparison.Summary = BuildComparisonSummary(comparison);

        _logger.LogInformation(
            "Profile comparison — {Summary}",
            comparison.Summary);

        return await Task.FromResult(comparison);
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private ProfilerOptions BuildDefaultOptions() => new()
    {
        CaptureExecutionPlan = _settings.CaptureExecutionPlanByDefault,
        CaptureTimings = _settings.CaptureTimingsByDefault,
        CaptureResourceUsage = _settings.CaptureResourceUsageByDefault,
        MaxDurationMs = _settings.DefaultMaxDurationMs,
        IncludePlanVisualization = _settings.IncludePlanVisualizationByDefault
    };

    private async Task ExecuteWarmUpAsync(DatabaseQuery query, int iterations)
    {
        for (var i = 0; i < iterations; i++)
        {
            _logger.LogDebug("Warm-up iteration {I}/{Total} for {QueryId}", i + 1, iterations, query.QueryId);
            await _queryAnalyzer.AnalyzeQueryAsync(query);
        }
    }

    private async Task<List<ExecutionStage>> MeasureStagesAsync(DatabaseQuery query)
    {
        var stages = new List<ExecutionStage>();

        stages.Add(await TimedStageAsync("Parse", async () =>
        {
            var q = new DatabaseQuery { QueryText = query.QueryText };
            q.Parse();
            return (object)q;
        }));

        stages.Add(await TimedStageAsync("ComplexityAnalysis", async () =>
        {
            var c = await _queryAnalyzer.DetermineComplexityAsync(query);
            return (object)c;
        }));

        stages.Add(await TimedStageAsync("IssueDetection", async () =>
        {
            var issues = await _issueDetector.DetectIssuesAsync(query);
            return (object)issues;
        }));

        stages.Add(await TimedStageAsync("JoinAnalysis", async () =>
        {
            var joinIssues = await _issueDetector.DetectJoinIssuesAsync(query);
            return (object)joinIssues;
        }));

        stages.Add(await TimedStageAsync("IndexOpportunities", async () =>
        {
            var idxIssues = await _issueDetector.DetectIndexOpportunitiesAsync(query);
            return (object)idxIssues;
        }));

        return stages;
    }

    private static async Task<ExecutionStage> TimedStageAsync(
        string stageName,
        Func<Task<object>> work)
    {
        var sw = Stopwatch.StartNew();
        Exception? caughtException = null;

        try
        {
            await work();
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }
        finally
        {
            sw.Stop();
        }

        return new ExecutionStage
        {
            Name = stageName,
            DurationMs = sw.Elapsed.TotalMilliseconds,
            HasError = caughtException != null,
            ErrorMessage = caughtException?.Message
        };
    }

    private ResourceUsage TakeResourceSnapshot(List<ExecutionStage> stages)
    {
        var process = Process.GetCurrentProcess();
        process.Refresh();

        return new ResourceUsage
        {
            WorkingSetBytes = process.WorkingSet64,
            PeakWorkingSetBytes = process.PeakWorkingSet64,
            TotalProcessorTimeMs = process.TotalProcessorTime.TotalMilliseconds,
            StageCount = stages.Count,
            TotalStagesMs = stages.Sum(s => s.DurationMs)
        };
    }

    private List<ProfilerMetric> BuildMetrics(QueryProfilerReport report)
    {
        var metrics = new List<ProfilerMetric>
        {
            new() { Name = "PerformanceScore",    Value = report.PerformanceScore,                                   Unit = "score",  Category = MetricCategory.Quality },
            new() { Name = "TotalStagesDuration", Value = report.ExecutionStages.Sum(s => s.DurationMs),            Unit = "ms",     Category = MetricCategory.Timing  },
            new() { Name = "IssueCount",          Value = report.AnalysisResult?.Issues.Count ?? 0,                 Unit = "issues", Category = MetricCategory.Quality },
            new() { Name = "CriticalIssueCount",  Value = report.AnalysisResult?.Issues.Count(i => i.Severity == IssueSeverity.Critical) ?? 0, Unit = "issues", Category = MetricCategory.Quality },
            new() { Name = "WarningIssueCount",   Value = report.AnalysisResult?.Issues.Count(i => i.Severity == IssueSeverity.Warning) ?? 0,  Unit = "issues", Category = MetricCategory.Quality },
            new() { Name = "IndexSuggestions",    Value = report.AnalysisResult?.IndexSuggestions.Count ?? 0,       Unit = "items",  Category = MetricCategory.Quality }
        };

        if (report.ResourceUsage != null)
        {
            metrics.Add(new() { Name = "WorkingSetMB",  Value = report.ResourceUsage.WorkingSetMb,          Unit = "MB", Category = MetricCategory.Resource });
            metrics.Add(new() { Name = "ProcessorTime", Value = report.ResourceUsage.TotalProcessorTimeMs,  Unit = "ms", Category = MetricCategory.Resource });
        }

        if (report.ExecutionPlan != null)
        {
            metrics.Add(new() { Name = "PlanNodeCount",      Value = report.ExecutionPlan.AllNodes.Count,                                Unit = "nodes", Category = MetricCategory.Plan });
            metrics.Add(new() { Name = "TableScanCount",     Value = report.ExecutionPlan.GetTableScans().Count,                        Unit = "scans", Category = MetricCategory.Plan });
            metrics.Add(new() { Name = "IndexSeekCount",     Value = report.ExecutionPlan.AllNodes.Count(n => n.NodeType == "Index Seek"), Unit = "seeks", Category = MetricCategory.Plan });
            metrics.Add(new() { Name = "EstimatedTotalCost", Value = report.ExecutionPlan.TotalEstimatedCost,                           Unit = "units", Category = MetricCategory.Plan });
            metrics.Add(new() { Name = "EstimatedRows",      Value = report.ExecutionPlan.TotalEstimatedRows,                           Unit = "rows",  Category = MetricCategory.Plan });
        }

        return metrics;
    }

    private static List<ProfilerSuggestion> BuildIssueBasedSuggestions(QueryAnalysisResult analysis)
    {
        return analysis.Issues
            .OrderByDescending(i => i.EstimatedPerformanceImpact)
            .Take(10)
            .Select((issue, idx) => new ProfilerSuggestion
            {
                Category = SuggestionCategory.QueryStructure,
                Priority = idx + 1,
                Title = $"Resolve {issue.IssueType} in {issue.AffectedClause} clause",
                Description = issue.Description,
                Recommendation = issue.RecommendedFix,
                ExampleCode = issue.ExampleFix,
                EstimatedImpactPercent = issue.EstimatedPerformanceImpact,
                Severity = issue.Severity == IssueSeverity.Critical
                    ? SuggestionSeverity.Critical
                    : issue.Severity == IssueSeverity.Warning
                        ? SuggestionSeverity.Warning
                        : SuggestionSeverity.Info
            })
            .ToList();
    }

    private static List<ProfilerSuggestion> BuildPlanBasedSuggestions(QueryProfilerReport report)
    {
        if (report.ExecutionPlan == null)
            return [];

        var suggestions = new List<ProfilerSuggestion>();
        var tableScans = report.ExecutionPlan.GetTableScans();

        if (tableScans.Count > 0)
        {
            var impactedTables = tableScans
                .Where(n => !string.IsNullOrEmpty(n.ObjectName))
                .Select(n => n.ObjectName)
                .Distinct()
                .ToList();

            var tableList = impactedTables.Count > 0
                ? $" on: {string.Join(", ", impactedTables)}"
                : string.Empty;

            suggestions.Add(new ProfilerSuggestion
            {
                Category = SuggestionCategory.IndexOptimization,
                Priority = 1,
                Title = $"Eliminate {tableScans.Count} full table scan(s) with targeted indexes",
                Description = $"The execution plan contains {tableScans.Count} full table scan(s){tableList}. " +
                              "Full scans read every row regardless of selectivity, causing excessive I/O on large tables.",
                Recommendation = "Create covering indexes that include the most frequently filtered columns (WHERE, JOIN conditions) " +
                                 "for the affected tables. Start with the table scan that has the highest estimated row count.",
                ExampleCode = "CREATE INDEX IX_Table_Column ON dbo.Table (FilteredColumn) INCLUDE (SelectedColumn1, SelectedColumn2);",
                EstimatedImpactPercent = Math.Min(60, tableScans.Count * 15.0),
                Severity = SuggestionSeverity.Warning
            });
        }

        var expensiveOps = report.ExecutionPlan.GetExpensiveOperations(5);
        var highCostOps = expensiveOps.Where(op => op.EstimatedCost > 5.0).ToList();

        if (highCostOps.Count > 0)
        {
            var combinedCost = highCostOps.Sum(o => o.EstimatedCost);
            suggestions.Add(new ProfilerSuggestion
            {
                Category = SuggestionCategory.ExecutionPlan,
                Priority = 2,
                Title = $"Review {highCostOps.Count} high-cost plan operator(s) (total cost {combinedCost:F2})",
                Description = $"The top {highCostOps.Count} operator(s) carry a combined optimizer cost of {combinedCost:F2} units. " +
                              "High-cost operators often indicate missing indexes, data type mismatches, or suboptimal query structure.",
                Recommendation = "Investigate each operator above 5.0 cost units individually. Common fixes: " +
                                 "add or rebuild indexes, rewrite correlated sub-queries as JOINs, replace CTEs with temp tables " +
                                 "when used multiple times, and ensure column statistics are up to date.",
                EstimatedImpactPercent = Math.Min(40, highCostOps.Count * 8.0),
                Severity = highCostOps.Any(o => o.EstimatedCost > 20.0)
                    ? SuggestionSeverity.Critical
                    : SuggestionSeverity.Warning
            });
        }

        if (report.ExecutionPlan.Joins.Any(j => j.JoinType.Contains("Hash", StringComparison.OrdinalIgnoreCase)))
        {
            suggestions.Add(new ProfilerSuggestion
            {
                Category = SuggestionCategory.ExecutionPlan,
                Priority = 3,
                Title = "Hash join operator(s) detected — verify join column indexes",
                Description = "Hash joins are chosen when the optimizer cannot use a more efficient nested-loop or merge join. " +
                              "They require significant memory grants and can spill to disk on large data sets.",
                Recommendation = "Ensure that both sides of every hash join have appropriate indexes. " +
                                 "Check that join column data types match exactly to avoid implicit conversions. " +
                                 "Update statistics on all joined tables.",
                EstimatedImpactPercent = 20,
                Severity = SuggestionSeverity.Warning
            });
        }

        return suggestions;
    }

    private List<ProfilerSuggestion> BuildStageSuggestions(List<ExecutionStage> stages)
    {
        var slowStages = stages
            .Where(s => !s.HasError && s.DurationMs > _settings.SlowStageThresholdMs)
            .OrderByDescending(s => s.DurationMs)
            .ToList();

        return slowStages.Select(stage => new ProfilerSuggestion
        {
            Category = SuggestionCategory.Runtime,
            Priority = 8,
            Title = $"Slow '{stage.Name}' stage ({stage.DurationMs:F0}ms exceeds {_settings.SlowStageThresholdMs:F0}ms threshold)",
            Description = $"The '{stage.Name}' pipeline stage took {stage.DurationMs:F1}ms, " +
                          $"which is {stage.DurationMs / _settings.SlowStageThresholdMs:F1}× above the configured threshold.",
            Recommendation = $"Profile the '{stage.Name}' stage in isolation. Consider caching intermediate results, " +
                             "pre-computing expensive sub-expressions, or increasing available parallelism.",
            EstimatedImpactPercent = Math.Min(30, stage.DurationMs / _settings.SlowStageThresholdMs * 5),
            Severity = SuggestionSeverity.Info
        }).ToList();
    }

    private List<ProfilerSuggestion> BuildResourceSuggestions(ResourceUsage? usage)
    {
        if (usage == null)
            return [];

        var suggestions = new List<ProfilerSuggestion>();

        if (usage.WorkingSetBytes > _settings.HighMemoryThresholdBytes)
        {
            suggestions.Add(new ProfilerSuggestion
            {
                Category = SuggestionCategory.ResourceUsage,
                Priority = 4,
                Title = $"High memory utilization during profiling ({usage.WorkingSetMb:F0} MB working set)",
                Description = $"Process working set reached {usage.WorkingSetMb:F0} MB, which exceeds the " +
                              $"{_settings.HighMemoryThresholdBytes / (1024.0 * 1024.0):F0} MB warning threshold.",
                Recommendation = "Reduce result set size using pagination (OFFSET/FETCH NEXT), stream results instead of buffering, " +
                                 "project only required columns, and avoid materializing large intermediate result sets in memory.",
                ExampleCode = "SELECT col1, col2 FROM Table ORDER BY Id OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY;",
                EstimatedImpactPercent = 20,
                Severity = SuggestionSeverity.Warning
            });
        }

        return suggestions;
    }

    private static List<MetricDelta> DiffMetrics(
        List<ProfilerMetric> baseline,
        List<ProfilerMetric> candidate)
    {
        var candidateIndex = candidate.ToDictionary(m => m.Name, StringComparer.OrdinalIgnoreCase);
        var deltas = new List<MetricDelta>();

        foreach (var bm in baseline)
        {
            if (!candidateIndex.TryGetValue(bm.Name, out var cm))
                continue;

            var delta = cm.Value - bm.Value;
            var deltaPercent = bm.Value != 0 ? delta / bm.Value * 100.0 : 0;

            deltas.Add(new MetricDelta
            {
                MetricName = bm.Name,
                BaselineValue = bm.Value,
                CandidateValue = cm.Value,
                Delta = delta,
                DeltaPercent = deltaPercent,
                Unit = bm.Unit,
                Category = bm.Category
            });
        }

        return deltas;
    }

    private static string BuildComparisonSummary(ProfileComparison c)
    {
        var verdict = c.IsImprovement ? "IMPROVED" : c.IsRegression ? "REGRESSED" : "UNCHANGED";
        var issueChange = c.CandidateIssueCount - c.BaselineIssueCount;
        var issueDir = issueChange < 0 ? $"−{-issueChange}" : issueChange > 0 ? $"+{issueChange}" : "no change";

        return $"[{verdict}] Score delta={c.ScoreDelta:+0.0;-0.0} | " +
               $"Timing delta={c.TimingDeltaPercent:+0.0;-0.0}% ({c.TimingDeltaMs:+0;-0}ms) | " +
               $"Issues {c.BaselineIssueCount} → {c.CandidateIssueCount} ({issueDir})";
    }
}
