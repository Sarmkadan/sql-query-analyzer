#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
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

        // Validate logger dependency
        if (value.GetLogger() is null)
        {
            errors.Add("Logger dependency cannot be null.");
        }

        // Validate index severity thresholds
        if (value.GetIndexSeverityThresholds() is null)
        {
            errors.Add("Index severity thresholds cannot be null.");
        }
        else if (value.GetIndexSeverityThresholds() is IndexSeverityThresholds thresholds)
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
        ArgumentNullException.ThrowIfNull(thresholds);

        var errors = new List<string>();

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

    /// <summary>
    /// Gets the logger dependency from the service instance.
    /// </summary>
    /// <param name="service">The service instance.</param>
    /// <returns>The logger instance, or null if not available.</returns>
    private static ILogger<PerformanceIssueDetectorService>? GetLogger(this PerformanceIssueDetectorService service)
        => service.GetFieldValue<ILogger<PerformanceIssueDetectorService>>("_logger");

    /// <summary>
    /// Gets the index severity thresholds from the service instance.
    /// </summary>
    /// <param name="service">The service instance.</param>
    /// <returns>The thresholds instance, or null if not available.</returns>
    private static IndexSeverityThresholds? GetIndexSeverityThresholds(this PerformanceIssueDetectorService service)
        => service.GetFieldValue<IndexSeverityThresholds>("_indexSeverity");

    /// <summary>
    /// Gets a field value using reflection.
    /// </summary>
    /// <typeparam name="T">The field type.</typeparam>
    /// <param name="service">The service instance.</param>
    /// <param name="fieldName">The field name.</param>
    /// <returns>The field value, or null if not found or inaccessible.</returns>
    private static T? GetFieldValue<T>(this PerformanceIssueDetectorService service, string fieldName)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(fieldName);

        var field = typeof(PerformanceIssueDetectorService).GetField(
            fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        return field?.GetValue(service) as T;
    }
}
