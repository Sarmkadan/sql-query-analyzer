#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SqlQueryAnalyzer.Configuration;

namespace SqlQueryAnalyzer.Services;

/// <summary>
/// Provides validation helpers for <see cref="QueryProfilerService"/> to ensure service
/// dependencies and configuration are valid before use.
/// </summary>
public static class QueryProfilerServiceValidation
{
    /// <summary>
    /// Validates the specified <see cref="QueryProfilerService"/> instance and returns a list of
    /// human-readable validation problems. Returns an empty list if the instance is valid.
    /// </summary>
    /// <param name="value">The service instance to validate.</param>
    /// <returns>A read-only list of validation problem descriptions.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this QueryProfilerService value)
    {
        ArgumentNullException.ThrowIfNull(value);

        // QueryProfilerService has comprehensive null checks in its constructor
        // and public methods. Since all fields are private, we can only validate
        // that the instance itself is not null, which is already done by ArgumentNullException.
        // The service's constructor ensures all dependencies are non-null.

        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the specified <see cref="QueryProfilerService"/> instance is valid.
    /// </summary>
    /// <param name="value">The service instance to check.</param>
    /// <returns>True if the instance is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this QueryProfilerService value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="QueryProfilerService"/> instance is valid.
    /// Throws an <see cref="ArgumentException"/> with a detailed message listing all validation problems.
    /// </summary>
    /// <param name="value">The service instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is not valid, containing a list of problems.</exception>
    public static void EnsureValid(this QueryProfilerService value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"QueryProfilerService is not valid. Problems: {string.Join(", ", problems)}.",
            nameof(value));
    }

    // No additional private validation methods needed for behavioral validation approach
    // The validation is based on the service's public interface and constructor guarantees
}