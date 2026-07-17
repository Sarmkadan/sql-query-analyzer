#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using SqlQueryAnalyzer.Constants;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Provides validation helpers for <see cref="QueryAnalysisResult"/> instances.
/// </summary>
public static class QueryAnalysisResultValidation
{
    /// <summary>
    /// Validates the specified <see cref="QueryAnalysisResult"/> instance.
    /// </summary>
    /// <param name="value">The analysis result to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this QueryAnalysisResult value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate QueryId
        if (string.IsNullOrWhiteSpace(value.QueryId))
        {
            errors.Add("QueryId cannot be null or whitespace.");
        }
        else if (value.QueryId.Length > 100)
        {
            errors.Add($"QueryId '{value.QueryId}' exceeds maximum length of 100 characters.");
        }

        // Validate Query
        if (string.IsNullOrWhiteSpace(value.Query))
        {
            errors.Add("Query cannot be null or whitespace.");
        }
        else if (value.Query.Length > 100000)
        {
            errors.Add($"Query exceeds maximum length of 100,000 characters (actual: {value.Query.Length:N0}).");
        }

        // Validate AnalyzedAt
        if (value.AnalyzedAt == default)
        {
            errors.Add("AnalyzedAt cannot be the default DateTime value.");
        }
        else if (value.AnalyzedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add($"AnalyzedAt '{value.AnalyzedAt:O}' cannot be in the future.");
        }
        else if (value.AnalyzedAt < new DateTime(2020, 1, 1))
        {
            errors.Add($"AnalyzedAt '{value.AnalyzedAt:O}' cannot be before year 2020.");
        }

        // Validate Complexity
        if (!Enum.IsDefined(typeof(QueryComplexity), value.Complexity))
        {
            errors.Add($"Complexity '{value.Complexity}' has an invalid enum value.");
        }

        // Validate PerformanceScore
        if (double.IsNaN(value.PerformanceScore))
        {
            errors.Add("PerformanceScore cannot be NaN.");
        }
        else if (double.IsInfinity(value.PerformanceScore))
        {
            errors.Add($"PerformanceScore '{value.PerformanceScore}' cannot be infinite.");
        }
        else if (value.PerformanceScore < 0 || value.PerformanceScore > 100)
        {
            errors.Add($"PerformanceScore '{value.PerformanceScore:F2}' must be between 0 and 100 inclusive.");
        }

        // Validate EstimatedExecutionTime
        if (value.EstimatedExecutionTime < TimeSpan.Zero)
        {
            errors.Add($"EstimatedExecutionTime '{value.EstimatedExecutionTime}' cannot be negative.");
        }
        else if (value.EstimatedExecutionTime.TotalMilliseconds > 86400000) // 24 hours
        {
            errors.Add($"EstimatedExecutionTime '{value.EstimatedExecutionTime}' cannot exceed 24 hours.");
        }

        // Validate Issues
        if (value.Issues is null)
        {
            errors.Add("Issues collection cannot be null.");
        }
        else
        {
            foreach (var issue in value.Issues)
            {
                if (issue is null)
                {
                    errors.Add("Issues collection contains a null element.");
                    break;
                }
            }
        }

        // Validate IndexSuggestions
        if (value.IndexSuggestions is null)
        {
            errors.Add("IndexSuggestions collection cannot be null.");
        }
        else
        {
            foreach (var suggestion in value.IndexSuggestions)
            {
                if (suggestion is null)
                {
                    errors.Add("IndexSuggestions collection contains a null element.");
                    break;
                }
            }
        }

        // ExecutionPlan validation is now handled by QueryPlanValidation class

        // Validate Statistics
        if (value.Statistics is null)
        {
            errors.Add("Statistics cannot be null.");
        }
        else
        {
            var statsErrors = QueryStatisticsValidation.Validate(value.Statistics);
            if (statsErrors.Count > 0)
            {
                errors.AddRange(statsErrors.Select(e => $"Statistics: {e}"));
            }
        }

        // Validate Metadata
        if (value.Metadata is null)
        {
            errors.Add("Metadata dictionary cannot be null.");
        }
        else if (value.Metadata.Count > 1000)
        {
            errors.Add($"Metadata dictionary contains {value.Metadata.Count} entries, exceeding maximum size of 1000.");
        }
        else
        {
            foreach (var key in value.Metadata.Keys)
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    errors.Add("Metadata contains a null or whitespace key.");
                    break;
                }
                else if (key.Length > 255)
                {
                    errors.Add($"Metadata key '{key}' exceeds maximum length of 255 characters (actual: {key.Length}).");
                    break;
                }
            }
        }

        // Validate computed properties are consistent
        var computedScore = value.ComplexityScore;
        if (computedScore < 0 || computedScore > 100)
        {
            errors.Add($"Computed ComplexityScore '{computedScore}' is out of valid range (0-100).");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="QueryAnalysisResult"/> instance is valid.
    /// </summary>
    /// <param name="value">The analysis result to check.</param>
    /// <returns>True if the instance is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this QueryAnalysisResult value) =>
        value.Validate().Count == 0;

    /// <summary>
    /// Ensures that a <see cref="QueryAnalysisResult"/> instance is valid, throwing an <see cref="ArgumentException"/> if it is not.
    /// </summary>
    /// <param name="value">The analysis result to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is invalid, containing a list of problems.</exception>
    public static void EnsureValid(this QueryAnalysisResult value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"QueryAnalysisResult validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }
}