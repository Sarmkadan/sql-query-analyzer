#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;

namespace SqlQueryAnalyzer.Middleware;

/// <summary>
/// Provides validation helpers for <see cref="AnalysisPipeline"/> class
/// </summary>
public static class AnalysisPipelineValidation
{
    /// <summary>
    /// Validates all public members of an <see cref="AnalysisPipeline"/> instance
    /// </summary>
    /// <param name="value">The AnalysisPipeline instance to validate</param>
    /// <returns>A list of validation problems (empty if valid)</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static IReadOnlyList<string> Validate(this AnalysisPipeline value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate MiddlewareCount property (should be non-negative)
        if (value.MiddlewareCount < 0)
        {
            problems.Add("MiddlewareCount must be non-negative");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if an AnalysisPipeline instance is valid
    /// </summary>
    /// <param name="value">The AnalysisPipeline instance to check</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid(this AnalysisPipeline value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that an AnalysisPipeline instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The AnalysisPipeline instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails, containing all problems</exception>
    public static void EnsureValid(this AnalysisPipeline value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"AnalysisPipeline validation failed:{Environment.NewLine} - {string.Join($"{Environment.NewLine} - ", problems)}");
        }
    }
}
