#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SqlQueryAnalyzer.Utilities;

/// <summary>
/// Provides validation helpers for <see cref="StatisticsAggregator"/> instances.
/// Validates aggregated statistics for correctness, completeness, and data integrity.
/// All validation methods are implemented as extension methods for <see cref="StatisticsAggregator"/>.
/// </summary>
public static class StatisticsAggregatorValidation
{
    /// <summary>
    /// Validates the statistics aggregator instance for potential issues.
    /// Validates the aggregation summary returned by GetSummary().
    /// </summary>
    /// <param name="value">The statistics aggregator to validate</param>
    /// <returns>List of human-readable validation problems (empty if valid)</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this StatisticsAggregator value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Get the aggregation summary for validation
        var summary = value.GetSummary();

        // Validate aggregated counts
        if (summary.TotalQueries < 0)
        {
            errors.Add($"TotalQueries cannot be negative (found: {summary.TotalQueries})");
        }

        // Validate score metrics
        if (summary.AverageScore is double.NaN or double.PositiveInfinity or double.NegativeInfinity)
        {
            errors.Add("AverageScore is NaN or Infinity");
        }
        else if (summary.AverageScore < 0 || summary.AverageScore > 100)
        {
            errors.Add($"AverageScore must be between 0 and 100 (found: {summary.AverageScore:F2})");
        }

        if (summary.MinScore is double.NaN or double.PositiveInfinity or double.NegativeInfinity)
        {
            errors.Add("MinScore is NaN or Infinity");
        }
        else if (summary.MinScore < 0 || summary.MinScore > 100)
        {
            errors.Add($"MinScore must be between 0 and 100 (found: {summary.MinScore:F2})");
        }

        if (summary.MaxScore is double.NaN or double.PositiveInfinity or double.NegativeInfinity)
        {
            errors.Add("MaxScore is NaN or Infinity");
        }
        else if (summary.MaxScore < 0 || summary.MaxScore > 100)
        {
            errors.Add($"MaxScore must be between 0 and 100 (found: {summary.MaxScore:F2})");
        }

        // Validate that Min <= Average <= Max
        if (summary.TotalQueries > 0)
        {
            if (summary.MinScore > summary.MaxScore)
            {
                errors.Add("MinScore cannot be greater than MaxScore");
            }

            if (summary.AverageScore < summary.MinScore || summary.AverageScore > summary.MaxScore)
            {
                errors.Add("AverageScore should be between MinScore and MaxScore");
            }
        }

        // Validate standard deviation
        if (summary.ScoreStdDev is double.NaN or double.PositiveInfinity or double.NegativeInfinity)
        {
            errors.Add("ScoreStdDev is NaN or Infinity");
        }
        else if (summary.ScoreStdDev < 0)
        {
            errors.Add($"ScoreStdDev cannot be negative (found: {summary.ScoreStdDev:F2})");
        }

        // Validate issue counts
        if (summary.TotalIssuesFound < 0)
        {
            errors.Add($"TotalIssuesFound cannot be negative (found: {summary.TotalIssuesFound})");
        }

        // Validate severity counts
        if (summary.CriticalIssues < 0)
        {
            errors.Add($"CriticalIssues cannot be negative (found: {summary.CriticalIssues})");
        }

        if (summary.WarningIssues < 0)
        {
            errors.Add($"WarningIssues cannot be negative (found: {summary.WarningIssues})");
        }

        if (summary.InfoIssues < 0)
        {
            errors.Add($"InfoIssues cannot be negative (found: {summary.InfoIssues})");
        }

        // Validate that severity counts sum to total issues
        var severitySum = summary.CriticalIssues + summary.WarningIssues + summary.InfoIssues;
        if (severitySum != summary.TotalIssuesFound)
        {
            errors.Add($"Severity issue counts ({severitySum}) do not match TotalIssuesFound ({summary.TotalIssuesFound})");
        }

        // Validate optimization potential
        if (summary.TotalOptimizationPotential is double.NaN or double.PositiveInfinity or double.NegativeInfinity)
        {
            errors.Add("TotalOptimizationPotential is NaN or Infinity");
        }
        else if (summary.TotalOptimizationPotential < 0)
        {
            errors.Add($"TotalOptimizationPotential cannot be negative (found: {summary.TotalOptimizationPotential:F2})");
        }

        // Validate QueriesWithIssues
        if (summary.QueriesWithIssues < 0)
        {
            errors.Add($"QueriesWithIssues cannot be negative (found: {summary.QueriesWithIssues})");
        }

        if (summary.QueriesWithIssues > summary.TotalQueries)
        {
            errors.Add($"QueriesWithIssues ({summary.QueriesWithIssues}) cannot exceed TotalQueries ({summary.TotalQueries})");
        }

        // Validate AverageBugDensity
        if (summary.AverageBugDensity is double.NaN or double.PositiveInfinity or double.NegativeInfinity)
        {
            errors.Add("AverageBugDensity is NaN or Infinity");
        }
        else if (summary.AverageBugDensity < 0)
        {
            errors.Add($"AverageBugDensity cannot be negative (found: {summary.AverageBugDensity:F2})");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the statistics aggregator contains valid data.
    /// </summary>
    /// <param name="value">The statistics aggregator to check</param>
    /// <returns>True if the aggregator is valid; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static bool IsValid(this StatisticsAggregator value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the statistics aggregator contains valid data.
    /// Throws an <see cref="ArgumentException"/> with detailed error messages if validation fails.
    /// </summary>
    /// <param name="value">The statistics aggregator to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails with detailed error messages</exception>
    public static void EnsureValid(this StatisticsAggregator value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"StatisticsAggregator validation failed:{Environment.NewLine}" +
            string.Join(Environment.NewLine, errors.Select((error, index) => $"  {index + 1}. {error}")));
    }
}
