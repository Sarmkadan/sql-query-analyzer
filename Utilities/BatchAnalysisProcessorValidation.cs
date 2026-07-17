#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics.CodeAnalysis;

namespace SqlQueryAnalyzer.Utilities;

/// <summary>
/// Provides validation helpers for <see cref="BatchAnalysisProcessor"/> instances.
/// </summary>
public static class BatchAnalysisProcessorValidation
{
    /// <summary>
    /// Validates a <see cref="BatchAnalysisProcessor"/> instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate([NotNull] this BatchAnalysisProcessor? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // BatchAnalysisProcessor has no public properties to validate
        // All validation is done through constructor parameter validation

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="BatchAnalysisProcessor"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid([NotNullWhen(true)] this BatchAnalysisProcessor? value)
        => value != null && Validate(value).Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="BatchAnalysisProcessor"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid, containing a list of problems.</exception>
    public static void EnsureValid([NotNull] this BatchAnalysisProcessor? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        ArgumentNullException.ThrowIfNull(problems);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"BatchAnalysisProcessor is not valid. Problems: {string.Join("; ", problems)}");
        }
    }
}