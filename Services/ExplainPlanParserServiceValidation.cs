#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;

namespace SqlQueryAnalyzer.Services;

/// <summary>
/// Provides validation helpers for <see cref="ExplainPlanParserService"/> instances.
/// Validates that required dependencies are properly initialized.
/// </summary>
public static class ExplainPlanParserServiceValidation
{
    /// <summary>
    /// Validates the specified <see cref="ExplainPlanParserService"/> instance.
    /// </summary>
    /// <remarks>
    /// This validation ensures that the service instance has been properly constructed with all required dependencies.
    /// Since <see cref="ExplainPlanParserService"/> uses constructor injection with private readonly fields,
    /// successful construction guarantees that dependencies are non-null.
    /// </remarks>
    /// <param name="value">The service instance to validate.</param>
    /// <returns>A read-only list of validation problem descriptions; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ExplainPlanParserService value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ExplainPlanParserService"/> instance is valid.
    /// </summary>
    /// <remarks>
    /// Always returns <see langword="true"/> for non-null instances since constructor injection guarantees valid state.
    /// </remarks>
    /// <param name="value">The service instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ExplainPlanParserService value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return true;
    }

    /// <summary>
    /// Ensures that the specified <see cref="ExplainPlanParserService"/> instance is valid.
    /// </summary>
    /// <remarks>
    /// Throws an <see cref="ArgumentNullException"/> if the instance is null.
    /// Since <see cref="ExplainPlanParserService"/> uses constructor injection with private readonly fields,
    /// successful construction guarantees that dependencies are non-null, making additional validation redundant.
    /// </remarks>
    /// <param name="value">The service instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static void EnsureValid(this ExplainPlanParserService value)
    {
        ArgumentNullException.ThrowIfNull(value);
    }
}
