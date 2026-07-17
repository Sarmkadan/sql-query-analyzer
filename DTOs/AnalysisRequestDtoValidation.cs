#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace SqlQueryAnalyzer.DTOs;

/// <summary>
/// Provides validation methods for <see cref="AnalysisRequestDto"/> instances.
/// </summary>
public static class AnalysisRequestDtoValidation
{
    /// <summary>
    /// Validates the specified analysis request DTO.
    /// </summary>
    /// <param name="value">The analysis request to validate.</param>
    /// <returns>A list of validation error messages. Empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this AnalysisRequestDto? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate QueryText (required)
        if (string.IsNullOrWhiteSpace(value.QueryText))
        {
            errors.Add("QueryText is required and cannot be empty or whitespace.");
        }
        else if (value.QueryText.Length > 1_000_000)
        {
            errors.Add("QueryText exceeds maximum length of 1,000,000 characters.");
        }

        // Validate ApplicationName (optional but if provided, has constraints)
        if (!string.IsNullOrEmpty(value.ApplicationName) && value.ApplicationName.Length > 256)
        {
            errors.Add("ApplicationName exceeds maximum length of 256 characters.");
        }

        // Validate ProcedureName (optional but if provided, has constraints)
        if (!string.IsNullOrEmpty(value.ProcedureName) && value.ProcedureName.Length > 256)
        {
            errors.Add("ProcedureName exceeds maximum length of 256 characters.");
        }

        // Validate ModuleName (optional but if provided, has constraints)
        if (!string.IsNullOrEmpty(value.ModuleName) && value.ModuleName.Length > 256)
        {
            errors.Add("ModuleName exceeds maximum length of 256 characters.");
        }

        // Validate ExecutionPlanXml (optional but if provided, has constraints)
        if (!string.IsNullOrEmpty(value.ExecutionPlanXml) && value.ExecutionPlanXml.Length > 10_000_000)
        {
            errors.Add("ExecutionPlanXml exceeds maximum length of 10,000,000 characters.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified analysis request DTO is valid.
    /// </summary>
    /// <param name="value">The analysis request to check.</param>
    /// <returns>True if the request is valid; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this AnalysisRequestDto? value)
    {
        return value is not null && value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified analysis request DTO is valid.
    /// </summary>
    /// <param name="value">The analysis request to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the request is invalid, containing validation error messages.</exception>
    public static void EnsureValid(this AnalysisRequestDto? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"AnalysisRequestDto validation failed:{Environment.NewLine}- {
                    string.Join(
                        $"{Environment.NewLine}- ",
                        errors
                    )
                }");
        }
    }
}
