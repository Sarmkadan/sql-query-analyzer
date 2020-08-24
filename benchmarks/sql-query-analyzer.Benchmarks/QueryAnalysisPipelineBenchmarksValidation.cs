#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SqlQueryAnalyzer.Benchmarks;

/// <summary>
/// Validation helpers for <see cref="QueryAnalysisPipelineBenchmarks"/> to ensure benchmark configurations are valid.
/// </summary>
public static class QueryAnalysisPipelineBenchmarksValidation
{
    /// <summary>
    /// Validates that a <see cref="QueryAnalysisPipelineBenchmarks"/> instance is properly configured.
    /// </summary>
    /// <param name="value">The benchmarks instance to validate.</param>
    /// <returns>A list of validation messages (empty if valid).</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this QueryAnalysisPipelineBenchmarks value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate that benchmark methods produce valid results
        try
        {
            value.ParseSimpleQuery();
        }
        catch (Exception ex)
        {
            errors.Add($"ParseSimpleQuery() threw: {ex.Message}");
        }

        try
        {
            value.ParseComplexQuery();
        }
        catch (Exception ex)
        {
            errors.Add($"ParseComplexQuery() threw: {ex.Message}");
        }

        try
        {
            value.ParseStoredProcQuery();
        }
        catch (Exception ex)
        {
            errors.Add($"ParseStoredProcQuery() threw: {ex.Message}");
        }

        try
        {
            var hashResult = value.HashSimpleQuery();
            if (string.IsNullOrWhiteSpace(hashResult))
            {
                errors.Add("HashSimpleQuery() returned null or empty hash");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"HashSimpleQuery() threw: {ex.Message}");
        }

        try
        {
            var complexHash = value.HashComplexQuery();
            if (string.IsNullOrWhiteSpace(complexHash))
            {
                errors.Add("HashComplexQuery() returned null or empty hash");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"HashComplexQuery() threw: {ex.Message}");
        }

        try
        {
            var joinConditions = value.ExtractJoinConditions();
            if (joinConditions == null)
            {
                errors.Add("ExtractJoinConditions() returned null");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"ExtractJoinConditions() threw: {ex.Message}");
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
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing validation messages.</exception>
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