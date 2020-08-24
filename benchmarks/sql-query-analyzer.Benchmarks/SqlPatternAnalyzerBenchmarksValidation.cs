#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace SqlQueryAnalyzer.Benchmarks;

/// <summary>
/// Provides validation helpers for <see cref="SqlPatternAnalyzerBenchmarks"/> instances.
/// </summary>
public static class SqlPatternAnalyzerBenchmarksValidation
{
    /// <summary>
    /// Validates the specified <see cref="SqlPatternAnalyzerBenchmarks"/> instance.
    /// </summary>
    /// <param name="value">The benchmarks instance to validate.</param>
    /// <returns>A list of human-readable validation problems, or an empty list if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this SqlPatternAnalyzerBenchmarks value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate Setup method - this is a benchmark setup, so we validate the state it creates
        // Since Setup is called by BenchmarkDotNet, we can't validate its internal state directly
        // But we can validate that the benchmark instance itself is not in a default state

        // Validate boolean flags - should not be default(bool)
        if (value.DetectNPlusOneRepeated == default)
        {
            problems.Add("DetectNPlusOneRepeated should not be the default value (false).");
        }

        if (value.DetectNPlusOneDiverse == default)
        {
            problems.Add("DetectNPlusOneDiverse should not be the default value (false).");
        }

        if (value.HasFunctionOnColumn == default)
        {
            problems.Add("HasFunctionOnColumn should not be the default value (false).");
        }

        // Validate lists - should not be null and should not contain null/empty strings
        ValidateList(problems, value.ExtractTablesProblematic, nameof(value.ExtractTablesProblematic));
        ValidateList(problems, value.ExtractTablesNested, nameof(value.ExtractTablesNested));
        ValidateList(problems, value.RecommendationsClean, nameof(value.RecommendationsClean));
        ValidateList(problems, value.RecommendationsProblematic, nameof(value.RecommendationsProblematic));

        // Validate readability score - should be a reasonable value (0-100 range)
        if (value.ReadabilityScoreProblematic < 0 || value.ReadabilityScoreProblematic > 100)
        {
            problems.Add(
                $"ReadabilityScoreProblematic should be between 0 and 100, but was {value.ReadabilityScoreProblematic:F2}.");
        }

        // Validate parentheses count - should be non-negative
        if (value.CountParenthesesNested < 0)
        {
            problems.Add(
                $"CountParenthesesNested should be non-negative, but was {value.CountParenthesesNested}.");
        }

        // Validate OR conditions count - should be non-negative
        if (value.CountOrConditions < 0)
        {
            problems.Add(
                $"CountOrConditions should be non-negative, but was {value.CountOrConditions}.");
        }

        return problems;
    }

    /// <summary>
    /// Determines whether the specified <see cref="SqlPatternAnalyzerBenchmarks"/> instance is valid.
    /// </summary>
    /// <param name="value">The benchmarks instance to check.</param>
    /// <returns>True if the instance is valid; otherwise, false.</returns>
    public static bool IsValid(this SqlPatternAnalyzerBenchmarks value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="SqlPatternAnalyzerBenchmarks"/> instance is valid.
    /// </summary>
    /// <param name="value">The benchmarks instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is not valid.</exception>
    public static void EnsureValid(this SqlPatternAnalyzerBenchmarks value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"SqlPatternAnalyzerBenchmarks instance is not valid. Problems:\n{string.Join("\n", problems)}");
        }
    }

    /// <summary>
    /// Validates a list, checking for null, empty strings, and null elements.
    /// </summary>
    /// <param name="problems">The list to add problems to.</param>
    /// <param name="list">The list to validate.</param>
    /// <param name="propertyName">The name of the property being validated.</param>
    private static void ValidateList(List<string> problems, List<string>? list, string propertyName)
    {
        if (list is null)
        {
            problems.Add($"{propertyName} should not be null.");
            return;
        }

        if (list.Count == 0)
        {
            problems.Add($"{propertyName} should not be empty.");
        }

        for (var i = 0; i < list.Count; i++)
        {
            var item = list[i];
            if (string.IsNullOrEmpty(item))
            {
                problems.Add(
                    $"{propertyName}[{i}] should not be null or empty, but was {(item is null ? "null" : $"\"{item}\"")}.");
            }
        }
    }
}