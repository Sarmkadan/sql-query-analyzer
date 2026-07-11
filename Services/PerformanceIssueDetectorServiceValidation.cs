#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using SqlQueryAnalyzer.Configuration;

namespace SqlQueryAnalyzer.Services;

/// <summary>
/// Provides validation helpers for <see cref="PerformanceIssueDetectorService"/> instances.
/// </summary>
public static class PerformanceIssueDetectorServiceValidation
{
    /// <summary>
    /// Validates the specified <see cref="PerformanceIssueDetectorService"/> instance.
    /// </summary>
    /// <param name="value">The service instance to validate.</param>
    /// <returns>A list of validation errors; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this PerformanceIssueDetectorService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate logger field (required dependency) - use reflection to access private field
        var loggerField = typeof(PerformanceIssueDetectorService).GetField(
            "_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (loggerField?.GetValue(value) is null)
        {
            errors.Add("Logger dependency cannot be null.");
        }

        // Validate index severity thresholds field
        var severityField = typeof(PerformanceIssueDetectorService).GetField(
            "_indexSeverity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (severityField?.GetValue(value) is null)
        {
            errors.Add("Index severity thresholds cannot be null.");
        }
        else if (severityField.GetValue(value) is IndexSeverityThresholds thresholds)
        {
            errors.AddRange(ValidateIndexSeverityThresholds(thresholds));
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="PerformanceIssueDetectorService"/> instance is valid.
    /// </summary>
    /// <param name="value">The service instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this PerformanceIssueDetectorService? value)
        => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="PerformanceIssueDetectorService"/> instance is valid.
    /// </summary>
    /// <param name="value">The service instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is invalid, containing the validation errors.</exception>
    public static void EnsureValid(this PerformanceIssueDetectorService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"PerformanceIssueDetectorService is invalid:{Environment.NewLine} - {string.Join($"{Environment.NewLine} - ", errors)}");
        }
    }

    private static IReadOnlyList<string> ValidateIndexSeverityThresholds(IndexSeverityThresholds thresholds)
    {
        var errors = new List<string>();

        if (thresholds is null)
        {
            errors.Add("Index severity thresholds instance cannot be null.");
            return errors.AsReadOnly();
        }

        // Validate InfoMaxRows threshold
        if (thresholds.InfoMaxRows <= 0)
        {
            errors.Add(
                $"InfoMaxRows must be positive, but was {thresholds.InfoMaxRows}.");
        }

        // Validate WarningMaxRows threshold
        if (thresholds.WarningMaxRows <= 0)
        {
            errors.Add(
                $"WarningMaxRows must be positive, but was {thresholds.WarningMaxRows}.");
        }

        // Validate InfoMaxCost threshold
        if (thresholds.InfoMaxCost <= 0)
        {
            errors.Add(
                $"InfoMaxCost must be positive, but was {thresholds.InfoMaxCost}.");
        }

        // Validate WarningMaxCost threshold
        if (thresholds.WarningMaxCost <= 0)
        {
            errors.Add(
                $"WarningMaxCost must be positive, but was {thresholds.WarningMaxCost}.");
        }

        // Validate threshold relationships
        if (thresholds.WarningMaxRows < thresholds.InfoMaxRows)
        {
            errors.Add(
                $"WarningMaxRows ({thresholds.WarningMaxRows}) cannot be less than InfoMaxRows ({thresholds.InfoMaxRows}).");
        }

        if (thresholds.WarningMaxCost < thresholds.InfoMaxCost)
        {
            errors.Add(
                $"WarningMaxCost ({thresholds.WarningMaxCost}) cannot be less than InfoMaxCost ({thresholds.InfoMaxCost}).");
        }

        return errors.AsReadOnly();
    }
}