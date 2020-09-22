#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace SqlQueryAnalyzer.Validation;

/// <summary>
/// Provides validation helpers for <see cref="ValidationRuleEngine"/> instances.
/// </summary>
public static class ValidationRuleEngineValidation
{
    /// <summary>
    /// Validates the <see cref="ValidationRuleEngine"/> instance.
    /// Checks for null engine reference and validates internal state.
    /// </summary>
    /// <param name="value">The validation rule engine to validate.</param>
    /// <returns>A list of human-readable validation problems. Empty list if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static IReadOnlyList<string> Validate(this ValidationRuleEngine value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate internal state
        if (value.GetRuleCount() < 0)
        {
            problems.Add("Rule count is negative");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ValidationRuleEngine"/> instance is valid.
    /// </summary>
    /// <param name="value">The validation rule engine to check.</param>
    /// <returns>True if the engine is valid; otherwise, false.</returns>
    public static bool IsValid(this ValidationRuleEngine value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="ValidationRuleEngine"/> instance is valid.
    /// Throws an <see cref="ArgumentException"/> with detailed validation messages if the engine is not valid.
    /// </summary>
    /// <param name="value">The validation rule engine to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the engine has validation problems.</exception>
    public static void EnsureValid(this ValidationRuleEngine value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ValidationRuleEngine is not valid. Problems: {string.Join(", ", problems)}");
        }
    }
}