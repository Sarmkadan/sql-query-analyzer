#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace SqlQueryAnalyzer.Utilities;

/// <summary>
/// Provides validation helpers for <see cref="ConnectionValidationResult"/> instances.
/// Validates connection validation results for correctness and completeness.
/// </summary>
public static class DatabaseConnectionValidatorValidation
{
    /// <summary>
    /// Validates the provided <see cref="ConnectionValidationResult"/> instance.
    /// Returns a list of human-readable validation errors.
    /// </summary>
    /// <param name="value">The connection validation result to validate.</param>
    /// <returns>List of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ConnectionValidationResult value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate IsValid flag
        if (!value.IsValid)
        {
            errors.Add($"{nameof(ConnectionValidationResult.IsValid)} must be true.");
        }

        // Validate IsConnectionAlive flag
        if (!value.IsConnectionAlive)
        {
            errors.Add($"{nameof(ConnectionValidationResult.IsConnectionAlive)} must be true.");
        }

        // Validate Message - should not be null or empty when validation fails
        if (string.IsNullOrWhiteSpace(value.Message) && !value.IsValid)
        {
            errors.Add($"{nameof(ConnectionValidationResult.Message)} must be provided when validation fails.");
        }

        // Validate DatabaseVersion - should be non-empty when connection is alive
        if (value.IsConnectionAlive && string.IsNullOrWhiteSpace(value.DatabaseVersion))
        {
            errors.Add($"{nameof(ConnectionValidationResult.DatabaseVersion)} must be provided when connection is alive.");
        }

        // Validate Errors collection - should not be null and should be empty when valid
        if (value.Errors is null)
        {
            errors.Add($"{nameof(ConnectionValidationResult.Errors)} collection must not be null.");
        }
        else if (value.IsValid && value.Errors.Count > 0)
        {
            errors.Add($"{nameof(ConnectionValidationResult.Errors)} collection must be empty when {nameof(ConnectionValidationResult.IsValid)} is true.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the provided <see cref="ConnectionValidationResult"/> instance is valid.
    /// </summary>
    /// <param name="value">The connection validation result to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ConnectionValidationResult value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the provided <see cref="ConnectionValidationResult"/> instance is valid.
    /// Throws an <see cref="ArgumentException"/> with a detailed message listing all validation errors.
    /// </summary>
    /// <param name="value">The connection validation result to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the result is invalid.</exception>
    public static void EnsureValid(this ConnectionValidationResult value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"ConnectionValidationResult validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }
}