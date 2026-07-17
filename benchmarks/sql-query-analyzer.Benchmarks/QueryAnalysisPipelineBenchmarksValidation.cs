#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;

namespace SqlQueryAnalyzer.Benchmarks;

/// <summary>
/// Validation helpers for <see cref="QueryAnalysisPipelineBenchmarks"/> to ensure benchmark configurations and results are valid.
/// </summary>
public static class QueryAnalysisPipelineBenchmarksValidation
{
    /// <summary>
    /// Validates that a <see cref="QueryAnalysisPipelineBenchmarks"/> instance is properly configured and that benchmark operations produce valid results.
    /// </summary>
    /// <param name="value">The benchmarks instance to validate.</param>
    /// <returns>A list of validation messages (empty if valid).</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this QueryAnalysisPipelineBenchmarks value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate benchmark queries can be parsed without throwing
        try
        {
            value.ParseSimpleQueryBenchmark();
        }
        catch (Exception ex)
        {
            errors.Add($"ParseSimpleQueryBenchmark() threw: {ex.Message} ({ex.GetType().Name})");
        }

        try
        {
            value.ParseComplexQueryBenchmark();
        }
        catch (Exception ex)
        {
            errors.Add($"ParseComplexQueryBenchmark() threw: {ex.Message} ({ex.GetType().Name})");
        }

        try
        {
            value.ParseStoredProcQueryBenchmark();
        }
        catch (Exception ex)
        {
            errors.Add($"ParseStoredProcQueryBenchmark() threw: {ex.Message} ({ex.GetType().Name})");
        }

        // Validate hash generation produces non-empty results
        try
        {
            var hashResult = value.HashSimpleQueryBenchmark();
            if (string.IsNullOrWhiteSpace(hashResult))
            {
                errors.Add("HashSimpleQueryBenchmark() returned null or empty hash");
            }
            else if (hashResult.Length != 64)
            {
                errors.Add($"HashSimpleQueryBenchmark() returned hash with unexpected length {hashResult.Length} (expected 64 for SHA-256)");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"HashSimpleQueryBenchmark() threw: {ex.Message} ({ex.GetType().Name})");
        }

        try
        {
            var complexHash = value.HashComplexQueryBenchmark();
            if (string.IsNullOrWhiteSpace(complexHash))
            {
                errors.Add("HashComplexQueryBenchmark() returned null or empty hash");
            }
            else if (complexHash.Length != 64)
            {
                errors.Add($"HashComplexQueryBenchmark() returned hash with unexpected length {complexHash.Length} (expected 64 for SHA-256)");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"HashComplexQueryBenchmark() threw: {ex.Message} ({ex.GetType().Name})");
        }

        // Validate join condition extraction returns valid results
        try
        {
            var joinConditions = value.ExtractJoinConditionsBenchmark();
            if (joinConditions == null)
            {
                errors.Add("ExtractJoinConditionsBenchmark() returned null");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"ExtractJoinConditionsBenchmark() threw: {ex.Message} ({ex.GetType().Name})");
        }

        // Validate combined pattern analysis returns valid tuple
        try
        {
            var patternResults = value.FullPatternSuiteBenchmark();
            if (patternResults == default)
            {
                errors.Add("FullPatternSuiteBenchmark() returned default tuple");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"FullPatternSuiteBenchmark() threw: {ex.Message} ({ex.GetType().Name})");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="QueryAnalysisPipelineBenchmarks"/> instance is valid.
    /// </summary>
    /// <param name="value">The benchmarks instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this QueryAnalysisPipelineBenchmarks value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="QueryAnalysisPipelineBenchmarks"/> instance is valid.
    /// </summary>
    /// <param name="value">The benchmarks instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown if the instance is not valid, containing validation messages.
    /// </exception>
    public static void EnsureValid(this QueryAnalysisPipelineBenchmarks value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"QueryAnalysisPipelineBenchmarks instance is not valid. Validation errors: {string.Join("; ", errors)}");
        }
    }
}