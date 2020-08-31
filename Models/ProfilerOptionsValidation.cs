#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Provides validation helpers for <see cref="ProfilerOptions"/> instances.
/// </summary>
public static class ProfilerOptionsValidation
{
    /// <summary>
    /// Validates the specified <see cref="ProfilerOptions"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The options instance to validate.</param>
    /// <returns>An empty list if the instance is valid; otherwise, a list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this ProfilerOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate MaxDurationMs
        if (value.MaxDurationMs <= 0)
        {
            errors.Add(
                $"MaxDurationMs must be positive, but was {value.MaxDurationMs}.");
        }
        else if (value.MaxDurationMs > 3_600_000) // 1 hour in milliseconds
        {
            errors.Add(
                $"MaxDurationMs ({value.MaxDurationMs}) exceeds maximum allowed value of 3,600,000 ms (1 hour).");
        }

        // Validate WarmUpIterations
        if (value.WarmUpIterations < 0)
        {
            errors.Add(
                $"WarmUpIterations cannot be negative, but was {value.WarmUpIterations}.");
        }

        // Validate MeasurementIterations
        if (value.MeasurementIterations <= 0)
        {
            errors.Add(
                $"MeasurementIterations must be positive, but was {value.MeasurementIterations}.");
        }
        else if (value.MeasurementIterations > 1_000)
        {
            errors.Add(
                $"MeasurementIterations ({value.MeasurementIterations}) exceeds reasonable maximum of 1,000.");
        }

        return errors;
    }

    /// <summary>
    /// Determines whether the specified <see cref="ProfilerOptions"/> instance is valid.
    /// </summary>
    /// <param name="value">The options instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this ProfilerOptions value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="ProfilerOptions"/> instance is valid, throwing an <see cref="ArgumentException"/>
    /// with a detailed message listing all validation problems if it is not.
    /// </summary>
    /// <param name="value">The options instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is invalid, containing a list of all validation problems.</exception>
    public static void EnsureValid(this ProfilerOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();

        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"ProfilerOptions is invalid. Problems:\n\n- {string.Join("\n- ", errors)}",
            nameof(value));
    }
}