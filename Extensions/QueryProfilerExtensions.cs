// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using SqlQueryAnalyzer.Configuration;
using SqlQueryAnalyzer.Models;
using SqlQueryAnalyzer.Services;
using SqlQueryAnalyzer.Visualization;

namespace SqlQueryAnalyzer.Extensions;

/// <summary>
/// Extension methods for registering the query profiler with the DI container
/// and for querying <see cref="QueryProfilerReport"/> and <see cref="ProfileComparison"/> instances.
/// </summary>
public static class QueryProfilerExtensions
{
    // ── DI registration ───────────────────────────────────────────────────────

    /// <summary>
    /// Registers the query profiler and its dependencies with the DI container as singletons.
    /// <para>
    /// Prerequisites — the following services must already be registered before calling this method:
    /// <list type="bullet">
    ///   <item><see cref="IQueryAnalyzerService"/></item>
    ///   <item><see cref="IQueryPlanAnalyzerService"/></item>
    ///   <item><see cref="IPerformanceIssueDetectorService"/></item>
    /// </list>
    /// </para>
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="settings">
    /// Optional profiler settings. When <c>null</c>, <see cref="ProfilerSettings"/> defaults are used.
    /// </param>
    /// <returns>The same <paramref name="services"/> instance for fluent chaining.</returns>
    public static IServiceCollection AddQueryProfiler(
        this IServiceCollection services,
        ProfilerSettings? settings = null)
    {
        services.AddSingleton(settings ?? new ProfilerSettings());
        services.AddSingleton<IExecutionPlanVisualizer, ExecutionPlanVisualizer>();
        services.AddSingleton<IQueryProfilerService, QueryProfilerService>();
        return services;
    }

    /// <summary>
    /// Registers the query profiler using environment-specific default settings.
    /// Uses <see cref="ProfilerSettings.ForDevelopment"/> when
    /// <paramref name="environmentName"/> contains "Development" (case-insensitive);
    /// otherwise uses <see cref="ProfilerSettings.ForProduction"/>.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="environmentName">The hosting environment name (e.g., "Development", "Staging", "Production").</param>
    /// <returns>The same <paramref name="services"/> instance for fluent chaining.</returns>
    public static IServiceCollection AddQueryProfilerForEnvironment(
        this IServiceCollection services,
        string environmentName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(environmentName);

        var settings = environmentName.Contains("Development", StringComparison.OrdinalIgnoreCase)
            ? ProfilerSettings.ForDevelopment()
            : ProfilerSettings.ForProduction();

        return services.AddQueryProfiler(settings);
    }

    // ── Report extensions ─────────────────────────────────────────────────────

    /// <summary>
    /// Returns the slowest pipeline stage in the report.
    /// Returns <c>null</c> when <see cref="QueryProfilerReport.ExecutionStages"/> is empty.
    /// </summary>
    public static ExecutionStage? GetBottleneckStage(this QueryProfilerReport report) =>
        report.ExecutionStages.MaxBy(s => s.DurationMs);

    /// <summary>
    /// Returns all stages whose duration exceeds the specified threshold in milliseconds.
    /// </summary>
    public static List<ExecutionStage> GetSlowStages(this QueryProfilerReport report, double thresholdMs = 100.0) =>
        report.ExecutionStages.Where(s => s.DurationMs > thresholdMs).ToList();

    /// <summary>
    /// Returns metrics whose value exceeds the given numeric threshold.
    /// </summary>
    public static List<ProfilerMetric> GetCriticalMetrics(
        this QueryProfilerReport report,
        double threshold)
    {
        return report.Metrics.Where(m => m.Value > threshold).ToList();
    }

    /// <summary>
    /// Returns metrics belonging to the specified category.
    /// </summary>
    public static List<ProfilerMetric> GetMetricsByCategory(
        this QueryProfilerReport report,
        MetricCategory category)
    {
        return report.Metrics.Where(m => m.Category == category).ToList();
    }

    /// <summary>
    /// Returns all suggestions that belong to the specified category.
    /// </summary>
    public static List<ProfilerSuggestion> GetSuggestionsByCategory(
        this QueryProfilerReport report,
        SuggestionCategory category)
    {
        return report.Suggestions.Where(s => s.Category == category).ToList();
    }

    /// <summary>
    /// Returns all suggestions at or above the specified severity level.
    /// </summary>
    public static List<ProfilerSuggestion> GetSuggestionsBySeverity(
        this QueryProfilerReport report,
        SuggestionSeverity minimumSeverity)
    {
        return report.Suggestions.Where(s => s.Severity >= minimumSeverity).ToList();
    }

    /// <summary>
    /// Returns the top <paramref name="count"/> suggestions ordered by estimated impact (highest first).
    /// </summary>
    public static List<ProfilerSuggestion> GetTopSuggestions(
        this QueryProfilerReport report,
        int count = 5)
    {
        return report.Suggestions
            .OrderByDescending(s => s.EstimatedImpactPercent)
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Returns <c>true</c> when the performance score is below the specified threshold.
    /// The default threshold of 70 is aligned with the v1.x analysis scoring contract.
    /// </summary>
    public static bool NeedsOptimization(this QueryProfilerReport report, double threshold = 70.0) =>
        report.PerformanceScore < threshold;

    /// <summary>
    /// Returns a flat export dictionary combining all metrics and stage timings.
    /// Suitable for structured logging, telemetry pipelines, or JSON serialization.
    /// </summary>
    public static Dictionary<string, object> ToExportDictionary(this QueryProfilerReport report)
    {
        var dict = report.ToMetricsDictionary();

        dict["suggestions"] = report.Suggestions.Select(s => s.ToString()).ToList();
        dict["stages"] = report.ExecutionStages.Select(s => s.ToString()).ToList();

        if (report.PlanVisualization != null)
        {
            dict["planTreeText"] = report.PlanVisualization.TextTree;
            dict["bottleneckCount"] = report.PlanVisualization.Bottlenecks.Count;
            dict["planStats"] = report.PlanVisualization.Stats;
        }

        return dict;
    }

    /// <summary>
    /// Returns the single metric with the given name, or <c>null</c> if not present.
    /// The name comparison is case-insensitive.
    /// </summary>
    public static ProfilerMetric? FindMetric(this QueryProfilerReport report, string metricName) =>
        report.Metrics.FirstOrDefault(m =>
            m.Name.Equals(metricName, StringComparison.OrdinalIgnoreCase));

    // ── Batch extensions ──────────────────────────────────────────────────────

    /// <summary>
    /// Filters reports that contain at least one suggestion with critical severity.
    /// </summary>
    public static List<QueryProfilerReport> WithCriticalSuggestions(
        this IEnumerable<QueryProfilerReport> reports)
    {
        return reports.Where(r => r.HasCriticalSuggestions).ToList();
    }

    /// <summary>
    /// Filters reports that scored below the performance threshold and require optimization.
    /// </summary>
    public static List<QueryProfilerReport> NeedingOptimization(
        this IEnumerable<QueryProfilerReport> reports,
        double threshold = 70.0)
    {
        return reports.Where(r => r.NeedsOptimization(threshold)).ToList();
    }

    /// <summary>
    /// Returns reports ordered by performance score, worst-performing first.
    /// </summary>
    public static IOrderedEnumerable<QueryProfilerReport> OrderByWorstFirst(
        this IEnumerable<QueryProfilerReport> reports)
    {
        return reports.OrderBy(r => r.PerformanceScore);
    }

    /// <summary>
    /// Filters out reports that encountered an error during profiling.
    /// </summary>
    public static List<QueryProfilerReport> SuccessfulOnly(
        this IEnumerable<QueryProfilerReport> reports)
    {
        return reports.Where(r => !r.HasError).ToList();
    }

    /// <summary>
    /// Aggregates summary statistics for a batch of profiler reports.
    /// Returns a zeroed <see cref="ProfilerBatchSummary"/> when the sequence is empty.
    /// </summary>
    public static ProfilerBatchSummary GetBatchSummary(
        this IEnumerable<QueryProfilerReport> reports)
    {
        var list = reports.ToList();
        if (list.Count == 0)
            return new ProfilerBatchSummary();

        var successful = list.Where(r => !r.HasError).ToList();

        return new ProfilerBatchSummary
        {
            TotalReports = list.Count,
            SuccessfulReports = successful.Count,
            FailedReports = list.Count(r => r.HasError),
            AverageScore = successful.Count > 0 ? successful.Average(r => r.PerformanceScore) : 0,
            WorstScore = successful.Count > 0 ? successful.Min(r => r.PerformanceScore) : 0,
            BestScore = successful.Count > 0 ? successful.Max(r => r.PerformanceScore) : 0,
            TotalSuggestions = list.Sum(r => r.Suggestions.Count),
            TotalCriticalSuggestions = list.Sum(r => r.Suggestions.Count(s => s.Severity == SuggestionSeverity.Critical)),
            ReportsNeedingOptimization = successful.Count(r => r.NeedsOptimization()),
            AverageProfilingDurationMs = list.Average(r => r.TotalProfilingDurationMs)
        };
    }

    // ── Comparison extensions ─────────────────────────────────────────────────

    /// <summary>
    /// Returns metric deltas that represent measurable regressions:
    /// timing/resource metrics that increased, or quality issue counts that increased.
    /// </summary>
    public static List<MetricDelta> GetRegressions(this ProfileComparison comparison) =>
        comparison.MetricDeltas
            .Where(d =>
                (d.Category == MetricCategory.Timing && d.Delta > 0) ||
                (d.Category == MetricCategory.Resource && d.Delta > 0) ||
                (d.Category == MetricCategory.Quality && d.MetricName.Contains("Issue", StringComparison.OrdinalIgnoreCase) && d.Delta > 0))
            .OrderByDescending(d => d.DeltaPercent)
            .ToList();

    /// <summary>
    /// Returns metric deltas that represent measurable improvements:
    /// score metrics that increased, timing that decreased, or issue counts that decreased.
    /// </summary>
    public static List<MetricDelta> GetImprovements(this ProfileComparison comparison) =>
        comparison.MetricDeltas
            .Where(d =>
                (d.Category == MetricCategory.Quality && d.MetricName.Contains("Score", StringComparison.OrdinalIgnoreCase) && d.Delta > 0) ||
                (d.Category == MetricCategory.Timing && d.Delta < 0) ||
                (d.Category == MetricCategory.Quality && d.MetricName.Contains("Issue", StringComparison.OrdinalIgnoreCase) && d.Delta < 0))
            .OrderBy(d => d.DeltaPercent)
            .ToList();

    /// <summary>
    /// Returns a formatted Markdown-style comparison table of all metric deltas.
    /// </summary>
    public static string ToMarkdownTable(this ProfileComparison comparison)
    {
        if (comparison.MetricDeltas.Count == 0)
            return "_No comparable metrics found._";

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("| Metric | Baseline | Candidate | Delta | Δ% | Unit |");
        sb.AppendLine("|--------|----------|-----------|-------|----|------|");

        foreach (var d in comparison.MetricDeltas.OrderBy(d => d.Category).ThenBy(d => d.MetricName))
        {
            var sign = d.Delta >= 0 ? "+" : string.Empty;
            sb.AppendLine(
                $"| {d.MetricName} | {d.BaselineValue:F2} | {d.CandidateValue:F2} | {sign}{d.Delta:F2} | {d.DeltaPercent:+0.0;-0.0}% | {d.Unit} |");
        }

        sb.AppendLine();
        sb.AppendLine($"**{comparison.Summary}**");
        return sb.ToString();
    }
}

/// <summary>
/// Aggregated statistics for a batch of <see cref="QueryProfilerReport"/> instances.
/// Produced by <see cref="QueryProfilerExtensions.GetBatchSummary"/>.
/// </summary>
public class ProfilerBatchSummary
{
    /// <summary>Total number of reports in the batch (includes failed ones).</summary>
    public int TotalReports { get; set; }

    /// <summary>Number of reports that completed without a fatal error.</summary>
    public int SuccessfulReports { get; set; }

    /// <summary>Number of reports that encountered a fatal error during profiling.</summary>
    public int FailedReports { get; set; }

    /// <summary>Average performance score across successful reports.</summary>
    public double AverageScore { get; set; }

    /// <summary>Lowest performance score among successful reports.</summary>
    public double WorstScore { get; set; }

    /// <summary>Highest performance score among successful reports.</summary>
    public double BestScore { get; set; }

    /// <summary>Total number of suggestions across all reports.</summary>
    public int TotalSuggestions { get; set; }

    /// <summary>Total number of critical-severity suggestions across all reports.</summary>
    public int TotalCriticalSuggestions { get; set; }

    /// <summary>Number of successful reports whose score fell below the optimization threshold.</summary>
    public int ReportsNeedingOptimization { get; set; }

    /// <summary>Average wall-clock profiling duration across all reports in milliseconds.</summary>
    public double AverageProfilingDurationMs { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"Batch[{TotalReports} reports, {FailedReports} failed | " +
        $"avg={AverageScore:F1} range={WorstScore:F0}–{BestScore:F0} | " +
        $"{TotalSuggestions} suggestions ({TotalCriticalSuggestions} critical) | " +
        $"{ReportsNeedingOptimization} need optimization | " +
        $"avg profiling={AverageProfilingDurationMs:F0}ms]";
}
