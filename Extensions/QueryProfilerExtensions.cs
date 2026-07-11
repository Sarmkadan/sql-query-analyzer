#nullable enable
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
    /// <item><see cref="IQueryAnalyzerService"/></item>
    /// <item><see cref="IQueryPlanAnalyzerService"/></item>
    /// <item><see cref="IPerformanceIssueDetectorService"/></item>
    /// </list>
    /// </para>
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="settings">
    /// Optional profiler settings. When <c>null</c>, <see cref="ProfilerSettings"/> defaults are used.
    /// </param>
    /// <returns>The same <paramref name="services"/> instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddQueryProfiler(
        this IServiceCollection services,
        ProfilerSettings? settings = null)
    {
        ArgumentNullException.ThrowIfNull(services);
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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> or <paramref name="environmentName"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="environmentName"/> is empty or whitespace.</exception>
    public static IServiceCollection AddQueryProfilerForEnvironment(
        this IServiceCollection services,
        string environmentName)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(environmentName);

        var settings = environmentName.Contains("Development", StringComparison.OrdinalIgnoreCase)
            ? ProfilerSettings.ForDevelopment()
            : ProfilerSettings.ForProduction();

        return services.AddQueryProfiler(settings);
    }

    // ── Report extensions ─────────────────────────────────────────────────────

    /// <summary>
    /// Returns the slowest pipeline stage in the report.
    /// Returns <c>null</c> when <see cref="QueryProfilerReport.ExecutionStages"/> is empty or null.
    /// </summary>
    /// <param name="report">The profiler report to analyze.</param>
    /// <returns>The stage with the maximum duration, or null if no stages exist.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="report"/> is null.</exception>
    public static ExecutionStage? GetBottleneckStage(this QueryProfilerReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        return report.ExecutionStages?.MaxBy(s => s.DurationMs);
    }

    /// <summary>
    /// Returns all stages whose duration exceeds the specified threshold in milliseconds.
    /// </summary>
    /// <param name="report">The profiler report to analyze.</param>
    /// <param name="thresholdMs">The duration threshold in milliseconds. Defaults to 100.0.</param>
    /// <returns>A list of slow stages, or empty list if no stages exceed the threshold.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="report"/> is null.</exception>
    public static List<ExecutionStage> GetSlowStages(
        this QueryProfilerReport report,
        double thresholdMs = 100.0)
    {
        ArgumentNullException.ThrowIfNull(report);
        return report.ExecutionStages
            .Where(s => s.DurationMs > thresholdMs)
            .ToList();
    }

    /// <summary>
    /// Returns metrics whose value exceeds the given numeric threshold.
    /// </summary>
    /// <param name="report">The profiler report to analyze.</param>
    /// <param name="threshold">The numeric threshold for filtering metrics.</param>
    /// <returns>A list of critical metrics, or empty list if no metrics exceed the threshold.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="report"/> is null.</exception>
    public static List<ProfilerMetric> GetCriticalMetrics(
        this QueryProfilerReport report,
        double threshold)
    {
        ArgumentNullException.ThrowIfNull(report);
        return report.Metrics.Where(m => m.Value > threshold).ToList();
    }

    /// <summary>
    /// Returns metrics belonging to the specified category.
    /// </summary>
    /// <param name="report">The profiler report to analyze.</param>
    /// <param name="category">The metric category to filter by.</param>
    /// <returns>A list of metrics in the specified category, or empty list if none found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="report"/> is null.</exception>
    public static List<ProfilerMetric> GetMetricsByCategory(
        this QueryProfilerReport report,
        MetricCategory category)
    {
        ArgumentNullException.ThrowIfNull(report);
        return report.Metrics.Where(m => m.Category == category).ToList();
    }

    /// <summary>
    /// Returns all suggestions that belong to the specified category.
    /// </summary>
    /// <param name="report">The profiler report to analyze.</param>
    /// <param name="category">The suggestion category to filter by.</param>
    /// <returns>A list of suggestions in the specified category, or empty list if none found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="report"/> is null.</exception>
    public static List<ProfilerSuggestion> GetSuggestionsByCategory(
        this QueryProfilerReport report,
        SuggestionCategory category)
    {
        ArgumentNullException.ThrowIfNull(report);
        return report.Suggestions.Where(s => s.Category == category).ToList();
    }

    /// <summary>
    /// Returns all suggestions at or above the specified severity level.
    /// </summary>
    /// <param name="report">The profiler report to analyze.</param>
    /// <param name="minimumSeverity">The minimum severity level to include.</param>
    /// <returns>A list of suggestions meeting the severity criteria, or empty list if none found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="report"/> is null.</exception>
    public static List<ProfilerSuggestion> GetSuggestionsBySeverity(
        this QueryProfilerReport report,
        SuggestionSeverity minimumSeverity)
    {
        ArgumentNullException.ThrowIfNull(report);
        return report.Suggestions.Where(s => s.Severity >= minimumSeverity).ToList();
    }

    /// <summary>
    /// Returns the top <paramref name="count"/> suggestions ordered by estimated impact (highest first).
    /// </summary>
    /// <param name="report">The profiler report to analyze.</param>
    /// <param name="count">The maximum number of suggestions to return. Defaults to 5.</param>
    /// <returns>A list of top suggestions by estimated impact, or empty list if no suggestions exist.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="report"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is negative.</exception>
    public static List<ProfilerSuggestion> GetTopSuggestions(
        this QueryProfilerReport report,
        int count = 5)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        return report.Suggestions
            .OrderByDescending(s => s.EstimatedImpactPercent)
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Returns <c>true</c> when the performance score is below the specified threshold.
    /// The default threshold of 70 is aligned with the v1.x analysis scoring contract.
    /// </summary>
    /// <param name="report">The profiler report to analyze.</param>
    /// <param name="threshold">The performance threshold below which optimization is needed. Defaults to 70.0.</param>
    /// <returns><c>true</c> if the performance score is below threshold; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="report"/> is null.</exception>
    public static bool NeedsOptimization(
        this QueryProfilerReport report,
        double threshold = 70.0)
    {
        ArgumentNullException.ThrowIfNull(report);
        return report.PerformanceScore < threshold;
    }

    /// <summary>
    /// Returns a flat export dictionary combining all metrics and stage timings.
    /// Suitable for structured logging, telemetry pipelines, or JSON serialization.
    /// </summary>
    /// <param name="report">The profiler report to export.</param>
    /// <returns>A dictionary containing all metrics, suggestions, and stage information.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="report"/> is null.</exception>
    public static Dictionary<string, object> ToExportDictionary(this QueryProfilerReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

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
    /// <param name="report">The profiler report to search.</param>
    /// <param name="metricName">The name of the metric to find.</param>
    /// <returns>The metric with the matching name, or null if not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="report"/> or <paramref name="metricName"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="metricName"/> is empty or whitespace.</exception>
    public static ProfilerMetric? FindMetric(
        this QueryProfilerReport report,
        string metricName)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentException.ThrowIfNullOrWhiteSpace(metricName);

        return report.Metrics.FirstOrDefault(m =>
            m.Name.Equals(metricName, StringComparison.OrdinalIgnoreCase));
    }

    // ── Batch extensions ──────────────────────────────────────────────────────

    /// <summary>
    /// Filters reports that contain at least one suggestion with critical severity.
    /// </summary>
    /// <param name="reports">The collection of reports to filter.</param>
    /// <returns>A list of reports containing critical suggestions.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="reports"/> is null.</exception>
    public static List<QueryProfilerReport> WithCriticalSuggestions(
        this IEnumerable<QueryProfilerReport> reports)
    {
        ArgumentNullException.ThrowIfNull(reports);
        return reports.Where(r => r.HasCriticalSuggestions).ToList();
    }

    /// <summary>
    /// Filters reports that scored below the performance threshold and require optimization.
    /// </summary>
    /// <param name="reports">The collection of reports to filter.</param>
    /// <param name="threshold">The performance threshold. Defaults to 70.0.</param>
    /// <returns>A list of reports needing optimization.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="reports"/> is null.</exception>
    public static List<QueryProfilerReport> NeedingOptimization(
        this IEnumerable<QueryProfilerReport> reports,
        double threshold = 70.0)
    {
        ArgumentNullException.ThrowIfNull(reports);
        return reports.Where(r => r.NeedsOptimization(threshold)).ToList();
    }

    /// <summary>
    /// Returns reports ordered by performance score, worst-performing first.
    /// </summary>
    /// <param name="reports">The collection of reports to order.</param>
    /// <returns>An ordered enumerable sorted by performance score ascending (worst first).</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="reports"/> is null.</exception>
    public static IOrderedEnumerable<QueryProfilerReport> OrderByWorstFirst(
        this IEnumerable<QueryProfilerReport> reports)
    {
        ArgumentNullException.ThrowIfNull(reports);
        return reports.OrderBy(r => r.PerformanceScore);
    }

    /// <summary>
    /// Filters out reports that encountered an error during profiling.
    /// </summary>
    /// <param name="reports">The collection of reports to filter.</param>
    /// <returns>A list of reports that completed successfully without errors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="reports"/> is null.</exception>
    public static List<QueryProfilerReport> SuccessfulOnly(
        this IEnumerable<QueryProfilerReport> reports)
    {
        ArgumentNullException.ThrowIfNull(reports);
        return reports.Where(r => !r.HasError).ToList();
    }

    /// <summary>
    /// Aggregates summary statistics for a batch of profiler reports.
    /// Returns a zeroed <see cref="ProfilerBatchSummary"/> when the sequence is empty.
    /// </summary>
    /// <param name="reports">The collection of reports to summarize.</param>
    /// <returns>A batch summary containing aggregated statistics.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="reports"/> is null.</exception>
    public static ProfilerBatchSummary GetBatchSummary(
        this IEnumerable<QueryProfilerReport> reports)
    {
        ArgumentNullException.ThrowIfNull(reports);

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
    /// <param name="comparison">The profile comparison to analyze.</param>
    /// <returns>A list of regression deltas, ordered by impact percentage descending.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="comparison"/> is null.</exception>
    public static List<MetricDelta> GetRegressions(this ProfileComparison comparison)
    {
        ArgumentNullException.ThrowIfNull(comparison);
        return comparison.MetricDeltas
            .Where(d =>
                (d.Category == MetricCategory.Timing && d.Delta > 0) ||
                (d.Category == MetricCategory.Resource && d.Delta > 0) ||
                (d.Category == MetricCategory.Quality && d.MetricName.Contains("Issue", StringComparison.OrdinalIgnoreCase) && d.Delta > 0))
            .OrderByDescending(d => d.DeltaPercent)
            .ToList();
    }

    /// <summary>
    /// Returns metric deltas that represent measurable improvements:
    /// score metrics that increased, timing that decreased, or issue counts that decreased.
    /// </summary>
    /// <param name="comparison">The profile comparison to analyze.</param>
    /// <returns>A list of improvement deltas, ordered by impact percentage ascending.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="comparison"/> is null.</exception>
    public static List<MetricDelta> GetImprovements(this ProfileComparison comparison)
    {
        ArgumentNullException.ThrowIfNull(comparison);
        return comparison.MetricDeltas
            .Where(d =>
                (d.Category == MetricCategory.Quality && d.MetricName.Contains("Score", StringComparison.OrdinalIgnoreCase) && d.Delta > 0) ||
                (d.Category == MetricCategory.Timing && d.Delta < 0) ||
                (d.Category == MetricCategory.Quality && d.MetricName.Contains("Issue", StringComparison.OrdinalIgnoreCase) && d.Delta < 0))
            .OrderBy(d => d.DeltaPercent)
            .ToList();
    }

    /// <summary>
    /// Returns a formatted Markdown-style comparison table of all metric deltas.
    /// </summary>
    /// <param name="comparison">The profile comparison to format.</param>
    /// <returns>A markdown table string, or a message indicating no comparable metrics were found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="comparison"/> is null.</exception>
    public static string ToMarkdownTable(this ProfileComparison comparison)
    {
        ArgumentNullException.ThrowIfNull(comparison);

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
public sealed class ProfilerBatchSummary
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