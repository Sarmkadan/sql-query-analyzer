#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using SqlQueryAnalyzer.Configuration;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Extensions;

/// <summary>
/// Provides validation helpers for types used by <see cref="QueryProfilerExtensions"/> extension methods.
/// </summary>
public static class QueryProfilerExtensionsValidation
{
    /// <summary>
    /// Validates the <see cref="ProfilerSettings"/> parameter for <see cref="QueryProfilerExtensions.AddQueryProfiler(IServiceCollection, ProfilerSettings)"/>.
    /// </summary>
    /// <param name="settings">The profiler settings to validate.</param>
    /// <returns>A list of validation errors; empty if the settings are valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this ProfilerSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return settings.Validate();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ProfilerSettings"/> are valid.
    /// </summary>
    /// <param name="settings">The profiler settings to check.</param>
    /// <returns><see langword="true"/> if the settings are valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this ProfilerSettings? settings)
        => Validate(settings).Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="ProfilerSettings"/> are valid.
    /// </summary>
    /// <param name="settings">The profiler settings to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the settings are invalid, containing the validation errors.</exception>
    public static void EnsureValid(this ProfilerSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var errors = settings.Validate();

        if (errors.Count is not 0)
        {
            throw new ArgumentException(
                $"ProfilerSettings are invalid:{Environment.NewLine} - {
                    string.Join($"{Environment.NewLine} - ", errors)
                }");
        }
    }

    /// <summary>
    /// Validates the <see cref="QueryProfilerReport"/> parameter.
    /// </summary>
    /// <param name="report">The profiler report to validate.</param>
    /// <returns>A list of validation errors; empty if the report is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="report"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this QueryProfilerReport? report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(report.QueryId))
        {
            errors.Add("QueryId cannot be null or whitespace.");
        }

        if (report.QueryText is null)
        {
            errors.Add("QueryText cannot be null.");
        }

        if (report.ProfiledAt == default)
        {
            errors.Add("ProfiledAt cannot be the default DateTime value.");
        }

        if (report.PerformanceScore is < 0 or > 100)
        {
            errors.Add("PerformanceScore must be between 0 and 100.");
        }

        if (report.TotalProfilingDurationMs < 0)
        {
            errors.Add("TotalProfilingDurationMs cannot be negative.");
        }

        if (report.ExecutionStages is null)
        {
            errors.Add("ExecutionStages collection cannot be null.");
        }

        if (report.Metrics is null)
        {
            errors.Add("Metrics collection cannot be null.");
        }

        if (report.Suggestions is null)
        {
            errors.Add("Suggestions collection cannot be null.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="QueryProfilerReport"/> is valid.
    /// </summary>
    /// <param name="report">The profiler report to check.</param>
    /// <returns><see langword="true"/> if the report is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this QueryProfilerReport? report)
        => Validate(report).Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="QueryProfilerReport"/> is valid.
    /// </summary>
    /// <param name="report">The profiler report to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the report is invalid, containing the validation errors.</exception>
    public static void EnsureValid(this QueryProfilerReport? report)
    {
        var errors = Validate(report);

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"QueryProfilerReport is invalid:{Environment.NewLine} - {
                    string.Join($"{Environment.NewLine} - ", errors)
                }");
        }
    }

    /// <summary>
    /// Validates the <see cref="ExecutionStage"/> parameter.
    /// </summary>
    /// <param name="stage">The execution stage to validate.</param>
    /// <returns>A list of validation errors; empty if the stage is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stage"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this ExecutionStage? stage)
    {
        ArgumentNullException.ThrowIfNull(stage);

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(stage.Name))
        {
            errors.Add("Stage Name cannot be null or whitespace.");
        }

        if (stage.DurationMs < 0)
        {
            errors.Add("Stage DurationMs cannot be negative.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ExecutionStage"/> is valid.
    /// </summary>
    /// <param name="stage">The execution stage to check.</param>
    /// <returns><see langword="true"/> if the stage is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this ExecutionStage? stage)
        => Validate(stage).Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="ExecutionStage"/> is valid.
    /// </summary>
    /// <param name="stage">The execution stage to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the stage is invalid, containing the validation errors.</exception>
    public static void EnsureValid(this ExecutionStage? stage)
    {
        var errors = Validate(stage);

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"ExecutionStage is invalid:{Environment.NewLine} - {
                    string.Join($"{Environment.NewLine} - ", errors)
                }");
        }
    }

    /// <summary>
    /// Validates the <see cref="ProfilerMetric"/> parameter.
    /// </summary>
    /// <param name="metric">The profiler metric to validate.</param>
    /// <returns>A list of validation errors; empty if the metric is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metric"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this ProfilerMetric? metric)
    {
        ArgumentNullException.ThrowIfNull(metric);

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(metric.Name))
        {
            errors.Add("Metric Name cannot be null or whitespace.");
        }

        if (metric.Value < 0)
        {
            errors.Add("Metric Value cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(metric.Unit))
        {
            errors.Add("Metric Unit cannot be null or whitespace.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ProfilerMetric"/> is valid.
    /// </summary>
    /// <param name="metric">The profiler metric to check.</param>
    /// <returns><see langword="true"/> if the metric is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this ProfilerMetric? metric)
        => Validate(metric).Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="ProfilerMetric"/> is valid.
    /// </summary>
    /// <param name="metric">The profiler metric to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the metric is invalid, containing the validation errors.</exception>
    public static void EnsureValid(this ProfilerMetric? metric)
    {
        var errors = Validate(metric);

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"ProfilerMetric is invalid:{Environment.NewLine} - {
                    string.Join($"{Environment.NewLine} - ", errors)
                }");
        }
    }

    /// <summary>
    /// Validates the <see cref="ProfilerSuggestion"/> parameter.
    /// </summary>
    /// <param name="suggestion">The profiler suggestion to validate.</param>
    /// <returns>A list of validation errors; empty if the suggestion is valid.</returns>
    public static IReadOnlyList<string> Validate(this ProfilerSuggestion? suggestion)
    {
        if (suggestion is null)
        {
            return new[] { "ProfilerSuggestion cannot be null." };
        }

        var errors = new List<string>();

        if (suggestion.Priority < 1)
        {
            errors.Add("Suggestion Priority must be at least 1.");
        }

        if (string.IsNullOrWhiteSpace(suggestion.Title))
        {
            errors.Add("Suggestion Title cannot be null or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(suggestion.Description))
        {
            errors.Add("Suggestion Description cannot be null or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(suggestion.Recommendation))
        {
            errors.Add("Suggestion Recommendation cannot be null or whitespace.");
        }

        if (suggestion.EstimatedImpactPercent < 0 || suggestion.EstimatedImpactPercent > 100)
        {
            errors.Add("Suggestion EstimatedImpactPercent must be between 0 and 100.");
        }

        if (suggestion.Severity < 0 || suggestion.Severity > (SuggestionSeverity)2)
        {
            errors.Add("Suggestion Severity must be a valid SuggestionSeverity value.");
        }

        if (suggestion.Category < 0 || suggestion.Category > (SuggestionCategory)5)
        {
            errors.Add("Suggestion Category must be a valid SuggestionCategory value.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ProfilerSuggestion"/> is valid.
    /// </summary>
    /// <param name="suggestion">The profiler suggestion to check.</param>
    /// <returns><see langword="true"/> if the suggestion is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this ProfilerSuggestion? suggestion)
        => Validate(suggestion).Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="ProfilerSuggestion"/> is valid.
    /// </summary>
    /// <param name="suggestion">The profiler suggestion to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the suggestion is invalid, containing the validation errors.</exception>
    public static void EnsureValid(this ProfilerSuggestion? suggestion)
    {
        var errors = Validate(suggestion);

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"ProfilerSuggestion is invalid:{Environment.NewLine} - {
                    string.Join($"{Environment.NewLine} - ", errors)
                }");
        }
    }

    /// <summary>
    /// Validates the <see cref="ProfileComparison"/> parameter.
    /// </summary>
    /// <param name="comparison">The profile comparison to validate.</param>
    /// <returns>A list of validation errors; empty if the comparison is valid.</returns>
    public static IReadOnlyList<string> Validate(this ProfileComparison? comparison)
    {
        if (comparison is null)
        {
            return new[] { "ProfileComparison cannot be null." };
        }

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(comparison.BaselineQueryId))
        {
            errors.Add("BaselineQueryId cannot be null or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(comparison.CandidateQueryId))
        {
            errors.Add("CandidateQueryId cannot be null or whitespace.");
        }

        if (comparison.ComparedAt == default)
        {
            errors.Add("ComparedAt cannot be the default DateTime value.");
        }

        if (string.IsNullOrWhiteSpace(comparison.Summary))
        {
            errors.Add("Summary cannot be null or whitespace.");
        }

        if (comparison.MetricDeltas is null)
        {
            errors.Add("MetricDeltas collection cannot be null.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ProfileComparison"/> is valid.
    /// </summary>
    /// <param name="comparison">The profile comparison to check.</param>
    /// <returns><see langword="true"/> if the comparison is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this ProfileComparison? comparison)
        => Validate(comparison).Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="ProfileComparison"/> is valid.
    /// </summary>
    /// <param name="comparison">The profile comparison to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the comparison is invalid, containing the validation errors.</exception>
    public static void EnsureValid(this ProfileComparison? comparison)
    {
        var errors = Validate(comparison);

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"ProfileComparison is invalid:{Environment.NewLine} - {
                    string.Join($"{Environment.NewLine} - ", errors)
                }");
        }
    }

    /// <summary>
    /// Validates the <see cref="MetricDelta"/> parameter.
    /// </summary>
    /// <param name="delta">The metric delta to validate.</param>
    /// <returns>A list of validation errors; empty if the delta is valid.</returns>
    public static IReadOnlyList<string> Validate(this MetricDelta? delta)
    {
        if (delta is null)
        {
            return new[] { "MetricDelta cannot be null." };
        }

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(delta.MetricName))
        {
            errors.Add("MetricName cannot be null or whitespace.");
        }

        if (delta.BaselineValue < 0)
        {
            errors.Add("BaselineValue cannot be negative.");
        }

        if (delta.CandidateValue < 0)
        {
            errors.Add("CandidateValue cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(delta.Unit))
        {
            errors.Add("Unit cannot be null or whitespace.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="MetricDelta"/> is valid.
    /// </summary>
    /// <param name="delta">The metric delta to check.</param>
    /// <returns><see langword="true"/> if the delta is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this MetricDelta? delta)
        => Validate(delta).Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="MetricDelta"/> is valid.
    /// </summary>
    /// <param name="delta">The metric delta to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the delta is invalid, containing the validation errors.</exception>
    public static void EnsureValid(this MetricDelta? delta)
    {
        var errors = Validate(delta);

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"MetricDelta is invalid:{Environment.NewLine} - {
                    string.Join($"{Environment.NewLine} - ", errors)
                }");
        }
    }
}