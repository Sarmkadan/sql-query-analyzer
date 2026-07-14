#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace SqlQueryAnalyzer.Services;

/// <summary>
/// Provides validation helpers for <see cref="ExplainPlanParserService"/> instances.
/// Validates that required dependencies (_planAnalyzer and _logger) are not null.
/// </summary>
public static class ExplainPlanParserServiceValidation
{
    /// <summary>
    /// Validates the specified <see cref="ExplainPlanParserService"/> instance.
    /// </summary>
    /// <param name="value">The service instance to validate.</param>
    /// <returns>A read-only list of validation problem descriptions; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ExplainPlanParserService value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate private _planAnalyzer field via reflection
        var planAnalyzerField = typeof(ExplainPlanParserService).GetField(
            "_planAnalyzer",
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (planAnalyzerField?.GetValue(value) is null)
        {
            errors.Add("Plan analyzer dependency (_planAnalyzer) cannot be null.");
        }

        // Validate private _logger field via reflection
        var loggerField = typeof(ExplainPlanParserService).GetField(
            "_logger",
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (loggerField?.GetValue(value) is null)
        {
            errors.Add("Logger dependency (_logger) cannot be null.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ExplainPlanParserService"/> instance is valid.
    /// </summary>
    /// <param name="value">The service instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ExplainPlanParserService value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="ExplainPlanParserService"/> instance is valid.
    /// Throws an <see cref="ArgumentException"/> with a detailed message listing all validation problems.
    /// </summary>
    /// <param name="value">The service instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is not valid, containing a list of problems.</exception>
    public static void EnsureValid(this ExplainPlanParserService value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"ExplainPlanParserService is not valid. Problems:{Environment.NewLine} - {string.Join($"{Environment.NewLine} - ", problems)}",
            nameof(value));
    }
}
