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
/// Provides validation helpers for <see cref="QueryNormalizerBenchmarks"/> to ensure benchmarks are properly configured and return valid results.
/// </summary>
public static class QueryNormalizerBenchmarksValidation
{
    /// <summary>
    /// Validates that the <see cref="QueryNormalizerBenchmarks"/> instance and its benchmark methods are properly configured.
    /// </summary>
    /// <param name="value">The benchmarks instance to validate.</param>
    /// <returns>A list of human-readable problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this QueryNormalizerBenchmarks value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate that the normalizer field is initialized (would be set by Setup())
        // This can't be checked directly as it's private, but we can check the results

        try
        {
            // Check NormalizeSimple
            var simpleResult = value.NormalizeSimple();
            if (string.IsNullOrWhiteSpace(simpleResult))
            {
                problems.Add("NormalizeSimple() returned null or whitespace");
            }
            else if (simpleResult.Contains("select", StringComparison.OrdinalIgnoreCase) == false)
            {
                problems.Add("NormalizeSimple() result doesn't contain expected SELECT keyword");
            }

            // Check NormalizeComplex
            var complexResult = value.NormalizeComplex();
            if (string.IsNullOrWhiteSpace(complexResult))
            {
                problems.Add("NormalizeComplex() returned null or whitespace");
            }
            else if (complexResult.Contains("SELECT", StringComparison.Ordinal) == false)
            {
                problems.Add("NormalizeComplex() result doesn't contain expected SELECT keyword");
            }

            // Check NormalizeWithLiterals
            var literalsResult = value.NormalizeWithLiterals();
            if (string.IsNullOrWhiteSpace(literalsResult))
            {
                problems.Add("NormalizeWithLiterals() returned null or whitespace");
            }
            else if (literalsResult.Contains("SELECT", StringComparison.Ordinal) == false)
            {
                problems.Add("NormalizeWithLiterals() result doesn't contain expected SELECT keyword");
            }

            // Check ExtractTableNamesComplex
            var tableNames = value.ExtractTableNamesComplex();
            if (tableNames is null)
            {
                problems.Add("ExtractTableNamesComplex() returned null");
            }
            else if (tableNames.Count == 0)
            {
                problems.Add("ExtractTableNamesComplex() returned empty list");
            }
            else
            {
                foreach (var tableName in tableNames)
                {
                    if (string.IsNullOrWhiteSpace(tableName))
                    {
                        problems.Add("ExtractTableNamesComplex() returned list containing null or whitespace entry");
                        break;
                    }

                    if (tableName.Any(char.IsWhiteSpace))
                    {
                        problems.Add($"ExtractTableNamesComplex() returned table name '{tableName}' containing whitespace");
                        break;
                    }
                }
            }

            // Check ExtractColumnNamesComplex
            var columnNames = value.ExtractColumnNamesComplex();
            if (columnNames is null)
            {
                problems.Add("ExtractColumnNamesComplex() returned null");
            }
            else if (columnNames.Count == 0)
            {
                problems.Add("ExtractColumnNamesComplex() returned empty list");
            }
            else
            {
                foreach (var columnName in columnNames)
                {
                    if (string.IsNullOrWhiteSpace(columnName))
                    {
                        problems.Add("ExtractColumnNamesComplex() returned list containing null or whitespace entry");
                        break;
                    }

                    if (columnName.Any(char.IsWhiteSpace))
                    {
                        problems.Add($"ExtractColumnNamesComplex() returned column name '{columnName}' containing whitespace");
                        break;
                    }
                }
            }
        }
        catch (Exception ex) when (ex is not ArgumentNullException)
        {
            problems.Add($"Benchmark execution threw exception: {ex.GetType().Name}: {ex.Message}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the <see cref="QueryNormalizerBenchmarks"/> instance is valid.
    /// </summary>
    /// <param name="value">The benchmarks instance to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this QueryNormalizerBenchmarks value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the <see cref="QueryNormalizerBenchmarks"/> instance is valid.
    /// </summary>
    /// <param name="value">The benchmarks instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid, containing a list of problems.</exception>
    public static void EnsureValid(this QueryNormalizerBenchmarks value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"QueryNormalizerBenchmarks instance is invalid. Problems:\n- {string.Join("\n- ", problems)}",
                nameof(value));
        }
    }
}
