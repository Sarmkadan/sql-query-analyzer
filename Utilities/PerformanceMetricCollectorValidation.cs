#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================


using System;
using System.Collections.Generic;
using System.Globalization;

namespace SqlQueryAnalyzer.Utilities;

/// <summary>
/// Provides validation helpers for <see cref="PerformanceMetricCollector"/> instances.
/// Validates null values, empty strings, out-of-range numbers, and default dates.
/// </summary>
public static class PerformanceMetricCollectorValidation
{
    /// <summary>
    /// Validates a <see cref="PerformanceMetricCollector"/> instance.
    /// </summary>
    /// <param name="value">The performance metric collector to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this PerformanceMetricCollector? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate via the report which contains aggregated data
        var report = value.GetReport();

        // Validate ExecutionTimeMs (average)
        var executionTimeMs = report.AverageExecutionTimeMs;
        if (executionTimeMs < 0)
        {
            errors.Add("ExecutionTimeMs must be non-negative.");
        }

        // Validate IssueCount (total issues detected)
        var issueCount = report.TotalIssuesDetected;
        if (issueCount < 0)
        {
            errors.Add("IssueCount must be non-negative.");
        }

        // Validate TotalAnalyses
        var totalAnalyses = report.TotalAnalyses;
        if (totalAnalyses < 0)
        {
            errors.Add("TotalAnalyses must be non-negative.");
        }

        // Validate SuccessfulAnalyses
        var successfulAnalyses = report.SuccessfulAnalyses;
        if (successfulAnalyses < 0)
        {
            errors.Add("SuccessfulAnalyses must be non-negative.");
        }
        else if (successfulAnalyses > totalAnalyses)
        {
            errors.Add("SuccessfulAnalyses cannot exceed TotalAnalyses.");
        }

        // Validate FailedAnalyses
        var failedAnalyses = report.FailedAnalyses;
        if (failedAnalyses < 0)
        {
            errors.Add("FailedAnalyses must be non-negative.");
        }
        else if (failedAnalyses + successfulAnalyses != totalAnalyses)
        {
            errors.Add("FailedAnalyses + SuccessfulAnalyses must equal TotalAnalyses.");
        }

        // Validate CacheHits
        var cacheHits = report.CacheHits;
        if (cacheHits < 0)
        {
            errors.Add("CacheHits must be non-negative.");
        }

        // Validate CacheMisses
        var cacheMisses = report.CacheMisses;
        if (cacheMisses < 0)
        {
            errors.Add("CacheMisses must be non-negative.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="PerformanceMetricCollector"/> is valid.
    /// </summary>
    /// <param name="value">The performance metric collector to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this PerformanceMetricCollector? value)
    {
        return value?.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="PerformanceMetricCollector"/> is valid.
    /// </summary>
    /// <param name="value">The performance metric collector to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid.</exception>
    public static void EnsureValid(this PerformanceMetricCollector? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"PerformanceMetricCollector is invalid:{Environment.NewLine}- {
                    string.Join(Environment.NewLine + "- ", errors)
                }");
        }
    }

}