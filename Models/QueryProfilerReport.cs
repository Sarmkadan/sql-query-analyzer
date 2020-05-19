#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Per-invocation configuration overrides for a profiler run.
/// Passed to <c>IQueryProfilerService.ProfileQueryAsync</c> to tune what data is collected.
/// </summary>
public sealed class ProfilerOptions
{
    /// <summary>Capture and embed the execution plan in the profiler report.</summary>
    public bool CaptureExecutionPlan { get; set; } = true;

    /// <summary>Measure wall-clock time for each pipeline stage.</summary>
    public bool CaptureTimings { get; set; } = true;

    /// <summary>Collect a CPU and memory snapshot during the profiling run.</summary>
    public bool CaptureResourceUsage { get; set; } = true;

    /// <summary>Maximum wall-clock budget before the profiler aborts, in milliseconds.</summary>
    public int MaxDurationMs { get; set; } = 30_000;

    /// <summary>Number of warm-up iterations executed before metrics are collected.</summary>
    public int WarmUpIterations { get; set; } = 0;

    /// <summary>Number of measurement iterations whose results are averaged.</summary>
    public int MeasurementIterations { get; set; } = 1;

    /// <summary>Include an ASCII tree visualization of the execution plan in the report.</summary>
    public bool IncludePlanVisualization { get; set; } = true;
}

/// <summary>
/// Complete profiling report produced by the query profiler.
/// Combines pipeline stage timings, execution plan analysis, resource usage, and ranked suggestions.
/// </summary>
public sealed class QueryProfilerReport
{
    /// <summary>Unique identifier that matches the originating <see cref="DatabaseQuery.QueryId"/>.</summary>
    public string QueryId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>SQL text that was profiled.</summary>
    public string QueryText { get; set; } = string.Empty;

    /// <summary>UTC timestamp when profiling started.</summary>
    public DateTime ProfiledAt { get; set; } = DateTime.UtcNow;

    /// <summary>Options that governed this profiling run.</summary>
    public ProfilerOptions Options { get; set; } = new();

    /// <summary>Overall performance score on a 0–100 scale (100 = optimal).</summary>
    public double PerformanceScore { get; set; }

    /// <summary>Total wall-clock time consumed by the profiler itself, in milliseconds.</summary>
    public double TotalProfilingDurationMs { get; set; }

    /// <summary>Set when a fatal error occurred during profiling.</summary>
    public bool HasError { get; set; }

    /// <summary>Error message populated when <see cref="HasError"/> is <c>true</c>.</summary>
    public string? ErrorMessage { get; set; }

    // ── Sub-reports ──────────────────────────────────────────────────────────

    /// <summary>Full query analysis result produced by the core analyzer.</summary>
    public QueryAnalysisResult? AnalysisResult { get; set; }

    /// <summary>Parsed execution plan, available when the plan was captured and parseable.</summary>
    public QueryPlan? ExecutionPlan { get; set; }

    /// <summary>ASCII/text visualization of the execution plan tree.</summary>
    public PlanVisualization? PlanVisualization { get; set; }

    // ── Collected data ───────────────────────────────────────────────────────

    /// <summary>Individual pipeline-stage timings recorded during profiling.</summary>
    public List<ExecutionStage> ExecutionStages { get; set; } = [];

    /// <summary>System resource snapshot captured after the analysis run.</summary>
    public ResourceUsage? ResourceUsage { get; set; }

    /// <summary>Scalar metrics aggregated during profiling.</summary>
    public List<ProfilerMetric> Metrics { get; set; } = [];

    /// <summary>Ranked, actionable optimization suggestions derived from the analysis.</summary>
    public List<ProfilerSuggestion> Suggestions { get; set; } = [];

    // ── Computed helpers ─────────────────────────────────────────────────────

    /// <summary>Returns <c>true</c> when at least one suggestion carries critical severity.</summary>
    public bool HasCriticalSuggestions =>
        Suggestions.Any(s => s.Severity == SuggestionSeverity.Critical);

    /// <summary>
    /// Sum of all suggestion impact estimates, capped at 100 % to avoid misleading totals.
    /// </summary>
    public double TotalEstimatedImprovementPercent =>
        Math.Min(100.0, Suggestions.Sum(s => s.EstimatedImpactPercent));

    /// <summary>Returns a single-line log-friendly summary of this report.</summary>
    public string GetSummary()
    {
        var issueCount = AnalysisResult?.Issues.Count ?? 0;
        return $"QueryId={QueryId} | Score={PerformanceScore:F1}/100 | Issues={issueCount} | " +
               $"Suggestions={Suggestions.Count} | ProfilingMs={TotalProfilingDurationMs:F0}";
    }

    /// <summary>
    /// Serializes key metrics to a flat dictionary suitable for structured logging or export.
    /// </summary>
    public Dictionary<string, object> ToMetricsDictionary()
    {
        var dict = new Dictionary<string, object>
        {
            { "queryId", QueryId },
            { "profiledAt", ProfiledAt },
            { "performanceScore", PerformanceScore },
            { "totalProfilingDurationMs", TotalProfilingDurationMs },
            { "stageCount", ExecutionStages.Count },
            { "suggestionCount", Suggestions.Count },
            { "hasError", HasError }
        };

        foreach (var metric in Metrics)
            dict[$"metric_{metric.Name}"] = metric.Value;

        return dict;
    }
}

/// <summary>
/// A single timed pipeline stage captured during a profiling run.
/// </summary>
public sealed class ExecutionStage
{
    /// <summary>Stage label (e.g., Parse, ComplexityAnalysis, IssueDetection).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Wall-clock duration of this stage in milliseconds.</summary>
    public double DurationMs { get; set; }

    /// <summary>Set when this stage completed with an unhandled exception.</summary>
    public bool HasError { get; set; }

    /// <summary>Exception message when <see cref="HasError"/> is <c>true</c>.</summary>
    public string? ErrorMessage { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        HasError
            ? $"{Name}: ERROR ({ErrorMessage})"
            : $"{Name}: {DurationMs:F1}ms";
}

/// <summary>
/// A scalar metric collected during a single profiling run.
/// </summary>
public sealed class ProfilerMetric
{
    /// <summary>Machine-readable metric identifier.</summary>
    public required string Name { get; set; }

    /// <summary>Numeric value of the metric.</summary>
    public double Value { get; set; }

    /// <summary>Human-readable unit label (e.g., ms, MB, score, nodes).</summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>Grouping category used when rendering reports.</summary>
    public MetricCategory Category { get; set; }

    /// <inheritdoc/>
    public override string ToString() => $"{Name}={Value:F2} {Unit}";
}

/// <summary>
/// Logical grouping for <see cref="ProfilerMetric"/> items.
/// </summary>
public enum MetricCategory
{
    /// <summary>Wall-clock or CPU timing measurements.</summary>
    Timing = 0,

    /// <summary>Analysis quality indicators such as score and issue counts.</summary>
    Quality = 1,

    /// <summary>System resource measurements: memory and CPU.</summary>
    Resource = 2,

    /// <summary>Execution-plan structural statistics.</summary>
    Plan = 3
}

/// <summary>
/// System resource snapshot captured by the profiler at the end of an analysis run.
/// </summary>
public sealed class ResourceUsage
{
    /// <summary>Process working-set size at the time of capture, in bytes.</summary>
    public long WorkingSetBytes { get; set; }

    /// <summary>Highest working-set value observed during the process lifetime, in bytes.</summary>
    public long PeakWorkingSetBytes { get; set; }

    /// <summary>Cumulative processor time consumed by the process, in milliseconds.</summary>
    public double TotalProcessorTimeMs { get; set; }

    /// <summary>Number of pipeline stages that completed before this snapshot was taken.</summary>
    public int StageCount { get; set; }

    /// <summary>Sum of all stage durations in milliseconds.</summary>
    public double TotalStagesMs { get; set; }

    /// <summary>Working-set size converted to megabytes.</summary>
    public double WorkingSetMb => WorkingSetBytes / (1024.0 * 1024.0);
}

/// <summary>
/// Severity level assigned to a <see cref="ProfilerSuggestion"/>.
/// </summary>
public enum SuggestionSeverity
{
    /// <summary>Minor improvement opportunity with low risk.</summary>
    Info = 0,

    /// <summary>Noticeable performance degradation that should be addressed soon.</summary>
    Warning = 1,

    /// <summary>Significant performance problem requiring immediate attention.</summary>
    Critical = 2
}

/// <summary>
/// Semantic category for <see cref="ProfilerSuggestion"/> items used to group and filter suggestions.
/// </summary>
public enum SuggestionCategory
{
    /// <summary>Problems with the query structure (SELECT *, missing WHERE, etc.).</summary>
    QueryStructure = 0,

    /// <summary>Missing or suboptimal index coverage.</summary>
    IndexOptimization = 1,

    /// <summary>Issues identified directly from the execution plan tree.</summary>
    ExecutionPlan = 2,

    /// <summary>Slow pipeline stages or timing anomalies detected at runtime.</summary>
    Runtime = 3,

    /// <summary>Elevated memory or CPU utilization during execution.</summary>
    ResourceUsage = 4,

    /// <summary>Schema-level concerns such as missing statistics or column type mismatches.</summary>
    Schema = 5
}

/// <summary>
/// A single, actionable optimization suggestion produced by the profiler.
/// Suggestions are ranked by <see cref="Priority"/> and <see cref="EstimatedImpactPercent"/>.
/// </summary>
public sealed class ProfilerSuggestion
{
    /// <summary>Semantic category this suggestion belongs to.</summary>
    public SuggestionCategory Category { get; set; }

    /// <summary>Application order when multiple suggestions are implemented (1 = highest priority).</summary>
    public int Priority { get; set; }

    /// <summary>Concise display title shown in report headers.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Detailed description of the identified problem.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Specific, actionable recommendation for resolving the problem.</summary>
    public string Recommendation { get; set; } = string.Empty;

    /// <summary>Optional example SQL or configuration snippet demonstrating the fix.</summary>
    public string? ExampleCode { get; set; }

    /// <summary>
    /// Estimated percentage-point improvement in performance score if this suggestion is applied.
    /// </summary>
    public double EstimatedImpactPercent { get; set; }

    /// <summary>Severity of the underlying performance issue.</summary>
    public SuggestionSeverity Severity { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"[P{Priority}][{Severity}][{Category}] {Title} (~{EstimatedImpactPercent:F0}% impact)";
}

/// <summary>
/// Structured comparison between a baseline <see cref="QueryProfilerReport"/> and a candidate,
/// typically produced after rewriting or tuning a query.
/// </summary>
public sealed class ProfileComparison
{
    /// <summary>Query ID of the baseline (original) report.</summary>
    public string BaselineQueryId { get; set; } = string.Empty;

    /// <summary>Query ID of the candidate (optimized) report.</summary>
    public string CandidateQueryId { get; set; } = string.Empty;

    /// <summary>UTC timestamp when this comparison was computed.</summary>
    public DateTime ComparedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Performance score delta (candidate − baseline). Positive values indicate improvement.</summary>
    public double ScoreDelta { get; set; }

    /// <summary>Set when the score delta exceeds the configured regression threshold (negative delta).</summary>
    public bool IsRegression { get; set; }

    /// <summary>Set when the score delta exceeds the configured improvement threshold (positive delta).</summary>
    public bool IsImprovement { get; set; }

    /// <summary>Total stage duration of the baseline report in milliseconds.</summary>
    public double BaselineTotalMs { get; set; }

    /// <summary>Total stage duration of the candidate report in milliseconds.</summary>
    public double CandidateTotalMs { get; set; }

    /// <summary>Absolute timing delta (candidate − baseline) in milliseconds. Negative = faster.</summary>
    public double TimingDeltaMs { get; set; }

    /// <summary>Timing delta expressed as a percentage of the baseline. Negative = faster.</summary>
    public double TimingDeltaPercent { get; set; }

    /// <summary>Number of detected issues in the baseline report.</summary>
    public int BaselineIssueCount { get; set; }

    /// <summary>Number of detected issues in the candidate report.</summary>
    public int CandidateIssueCount { get; set; }

    /// <summary>Per-metric deltas for fine-grained diff inspection.</summary>
    public List<MetricDelta> MetricDeltas { get; set; } = [];

    /// <summary>Human-readable one-line summary of the comparison outcome.</summary>
    public string Summary { get; set; } = string.Empty;
}

/// <summary>
/// The delta for a single metric between two compared profiler reports.
/// </summary>
public sealed class MetricDelta
{
    /// <summary>Name of the compared metric.</summary>
    public string MetricName { get; set; } = string.Empty;

    /// <summary>Value in the baseline report.</summary>
    public double BaselineValue { get; set; }

    /// <summary>Value in the candidate report.</summary>
    public double CandidateValue { get; set; }

    /// <summary>Absolute delta (candidate − baseline).</summary>
    public double Delta { get; set; }

    /// <summary>Delta as a percentage of the baseline. Positive = candidate is higher.</summary>
    public double DeltaPercent { get; set; }

    /// <summary>Unit label inherited from the source metric.</summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>Category inherited from the source metric.</summary>
    public MetricCategory Category { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{MetricName}: {BaselineValue:F2} → {CandidateValue:F2} ({DeltaPercent:+0.0;-0.0}%) {Unit}";
}

