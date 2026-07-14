#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
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
            errors.Add("QueryId exceeds maximum length of 100 characters.");
        }

        // Validate Query
        if (string.IsNullOrWhiteSpace(value.Query))
        {
            errors.Add("Query cannot be null or whitespace.");
        }
        else if (value.Query.Length > 100000)
        {
            errors.Add("Query exceeds maximum length of 100,000 characters.");
        }

        // Validate AnalyzedAt
        if (value.AnalyzedAt == default)
        {
            errors.Add("AnalyzedAt cannot be the default DateTime value.");
        }
        else if (value.AnalyzedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("AnalyzedAt cannot be in the future.");
        }
        else if (value.AnalyzedAt < new DateTime(2020, 1, 1))
        {
            errors.Add("AnalyzedAt cannot be before year 2020.");
        }

        // Validate Complexity
        if (!Enum.IsDefined(typeof(QueryComplexity), value.Complexity))
        {
            errors.Add("Complexity has an invalid enum value.");
        }

        // Validate PerformanceScore
        if (double.IsNaN(value.PerformanceScore))
        {
            errors.Add("PerformanceScore cannot be NaN.");
        }
        else if (double.IsInfinity(value.PerformanceScore))
        {
            errors.Add("PerformanceScore cannot be infinite.");
        }
        else if (value.PerformanceScore < 0 || value.PerformanceScore > 100)
        {
            errors.Add("PerformanceScore must be between 0 and 100 inclusive.");
        }

        // Validate EstimatedExecutionTime
        if (value.EstimatedExecutionTime < TimeSpan.Zero)
        {
            errors.Add("EstimatedExecutionTime cannot be negative.");
        }
        else if (value.EstimatedExecutionTime.TotalMilliseconds > 86400000) // 24 hours
        {
            errors.Add("EstimatedExecutionTime cannot exceed 24 hours.");
        }

        // Validate Issues
        if (value.Issues == null)
        {
            errors.Add("Issues collection cannot be null.");
        }
        else
        {
            foreach (var issue in value.Issues)
            {
                if (issue == null)
                {
                    errors.Add("Issues collection contains a null element.");
                    break;
                }
            }
        }

        // Validate IndexSuggestions
        if (value.IndexSuggestions == null)
        {
            errors.Add("IndexSuggestions collection cannot be null.");
        }
        else
        {
            foreach (var suggestion in value.IndexSuggestions)
            {
                if (suggestion == null)
                {
                    errors.Add("IndexSuggestions collection contains a null element.");
                    break;
                }
            }
        }

        // ExecutionPlan validation is now handled by QueryPlanValidation class

        // Validate Statistics
        if (value.Statistics == null)
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
        if (value.Metadata == null)
        {
            errors.Add("Metadata dictionary cannot be null.");
        }
        else if (value.Metadata.Count > 1000)
        {
            errors.Add("Metadata dictionary exceeds maximum size of 1000 entries.");
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
                    errors.Add("Metadata key exceeds maximum length of 255 characters.");
                    break;
                }
            }
        }

        // Validate computed properties are consistent
        var computedScore = value.ComplexityScore;
        if (computedScore < 0 || computedScore > 100)
        {
            errors.Add("Computed ComplexityScore is out of valid range (0-100).");
        }

        return errors.AsReadOnly();
    }

}