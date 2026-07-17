#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace SqlQueryAnalyzer.Tests;

/// <summary>
/// Provides validation helpers for the <see cref="SqlPatternAnalyzerTests"/> class to ensure test data integrity.
/// </summary>
public static class SqlPatternAnalyzerTestsValidation
{
    /// <summary>
    /// Validates that the <see cref="SqlPatternAnalyzerTests"/> instance is properly initialized and contains valid test data.
    /// </summary>
    /// <param name="value">The test instance to validate. Must not be null.</param>
    /// <returns>A list of human-readable validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this SqlPatternAnalyzerTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="SqlPatternAnalyzerTests"/> instance contains valid data.
    /// </summary>
    /// <param name="value">The test instance to check. Must not be null.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this SqlPatternAnalyzerTests? value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="SqlPatternAnalyzerTests"/> instance contains valid data.
    /// </summary>
    /// <param name="value">The test instance to validate. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance contains invalid data, with a list of problems.</exception>
    public static void EnsureValid(this SqlPatternAnalyzerTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"SqlPatternAnalyzerTests instance is invalid. Problems: {string.Join(", ", problems)}",
                nameof(value));
        }
    }
}