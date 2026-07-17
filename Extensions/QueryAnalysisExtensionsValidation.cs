#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Extensions;

/// <summary>
/// Provides validation helpers for <see cref="QueryAnalysisExtensions"/> extension methods.
/// Ensures extension methods can be safely invoked and validates result quality.
/// </summary>
public static class QueryAnalysisExtensionsValidation
{
    /// <summary>
    /// Validates that extension methods on <see cref="QueryAnalysisResult"/> can be safely invoked.
    /// Checks for null references, empty collections, and out-of-range values.
    /// </summary>
    /// <param name="value">The query analysis result to validate.</param>
    /// <returns>List of validation problems; empty if all validations pass.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this QueryAnalysisResult value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate performance score range (0-100)
        if (value.PerformanceScore is < 0 or > 100)
        {
            problems.Add(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "PerformanceScore must be between 0 and 100, but was {0:F2}.",
                    value.PerformanceScore
                )
            );
        }

        // Validate complexity is defined
        if (!Enum.IsDefined(typeof(Constants.QueryComplexity), value.Complexity))
        {
            problems.Add(
                $"Complexity has invalid value {(int)value.Complexity}."
            );
        }

        // Validate issues collection
        if (value.Issues is null)
        {
            problems.Add("Issues collection is null.");
        }

        // Validate index suggestions collection
        if (value.IndexSuggestions is null)
        {
            problems.Add("IndexSuggestions collection is null.");
        }

        // Validate critical issues flag consistency
        bool hasCriticalIssues = value.Issues?.Any(i => i.Severity == Constants.IssueSeverity.Critical) ?? false;
        if (hasCriticalIssues != value.HasCriticalIssues)
        {
            problems.Add(
                $"HasCriticalIssues flag is inconsistent: HasCriticalIssues={value.HasCriticalIssues}, but found {value.Issues?.Count(i => i.Severity == Constants.IssueSeverity.Critical)} critical issues."
            );
        }

        // Validate total optimization potential is non-negative
        if (value.TotalOptimizationPotential < 0)
        {
            problems.Add(
                $"TotalOptimizationPotential cannot be negative, but was {value.TotalOptimizationPotential:F2}."
            );
        }

        // Validate query ID is not empty
        if (string.IsNullOrWhiteSpace(value.QueryId))
        {
            problems.Add("QueryId is null, empty, or whitespace.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if the query analysis result is valid (no validation problems).
    /// </summary>
    /// <param name="value">The query analysis result to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this QueryAnalysisResult value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures the query analysis result is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The query analysis result to validate.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="value"/> is invalid with specific problems listed in the exception message.</exception>
    public static void EnsureValid(this QueryAnalysisResult value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "QueryAnalysisResult is invalid:\n- " + string.Join("\n- ", problems)
            );
        }
    }

    /// <summary>
    /// Validates a collection of query analysis results.
    /// </summary>
    /// <param name="values">The collection to validate.</param>
    /// <returns>List of validation problems; empty if all results are valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="values"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this IEnumerable<QueryAnalysisResult> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        var problems = new List<string>();
        var index = 0;

        foreach (var value in values)
        {
            if (value == null)
            {
                problems.Add($"Item at index {index} is null.");
            }
            else
            {
                problems.AddRange(value.Validate().Select(p => $"[Item {index}] {p}"));
            }

            index++;
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if a collection of query analysis results is valid.
    /// </summary>
    /// <param name="values">The collection to check.</param>
    /// <returns><see langword="true"/> if all results are valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="values"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this IEnumerable<QueryAnalysisResult> values)
    {
        return values.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures all query analysis results in a collection are valid.
    /// </summary>
    /// <param name="values">The collection to validate.</param>
    /// <exception cref="ArgumentNullException"><paramref name="values"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">One or more results in the collection are invalid with specific problems listed in the exception message.</exception>
    public static void EnsureValid(this IEnumerable<QueryAnalysisResult> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        var problems = values.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "QueryAnalysisResult collection is invalid:\n- " + string.Join("\n- ", problems)
            );
        }
    }
}