#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using Microsoft.Extensions.Logging;

namespace SqlQueryAnalyzer.Utilities;

/// <summary>
/// Provides validation helpers for <see cref="SqlInjectionDetector"/> instances.
/// </summary>
public static class SqlInjectionDetectorValidation
{
    /// <summary>
    /// Validates the specified <see cref="SqlInjectionDetector"/> instance.
    /// Since <see cref="SqlInjectionDetector"/> uses constructor injection for its dependencies,
    /// validation simply ensures the instance itself is not null.
    /// </summary>
    /// <param name="value">The SQL injection detector to validate.</param>
    /// <returns>A read-only list of validation problems; empty if validation succeeds.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this SqlInjectionDetector? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the specified <see cref="SqlInjectionDetector"/> instance is valid.
    /// </summary>
    /// <param name="value">The SQL injection detector to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this SqlInjectionDetector? value)
    {
        return value is not null;
    }

    /// <summary>
    /// Validates the specified <see cref="SqlInjectionDetector"/> instance and throws an <see cref="ArgumentException"/> if validation fails.
    /// </summary>
    /// <param name="value">The SQL injection detector to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails.</exception>
    public static void EnsureValid(this SqlInjectionDetector? value)
    {
        ArgumentNullException.ThrowIfNull(value);
    }
}