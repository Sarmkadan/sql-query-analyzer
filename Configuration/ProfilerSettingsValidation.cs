#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Validation helpers for ProfilerSettings configuration
// ===================================================================

using System;
using System.Collections.Generic;

namespace SqlQueryAnalyzer.Configuration;

/// <summary>
/// Provides validation helpers for <see cref="ProfilerSettings"/> configuration.
/// </summary>
public static class ProfilerSettingsValidation
{
    /// <summary>
    /// Validates the provided <see cref="ProfilerSettings"/> instance.
    /// </summary>
    /// <param name="value">The settings to validate.</param>
    /// <returns>List of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ProfilerSettings value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate numeric limits
        if (value.DefaultMaxDurationMs < 100)
        {
            errors.Add($"{nameof(ProfilerSettings.DefaultMaxDurationMs)} must be at least 100 ms.");
        }

        if (value.MaxBatchSize < 1)
        {
            errors.Add($"{nameof(ProfilerSettings.MaxBatchSize)} must be at least 1.");
        }

        if (value.MaxQueryLengthChars < 1)
        {
            errors.Add($"{nameof(ProfilerSettings.MaxQueryLengthChars)} must be at least 1.");
        }

        // Validate comparison thresholds
        if (value.RegressionThreshold < 0)
        {
            errors.Add($"{nameof(ProfilerSettings.RegressionThreshold)} must be non-negative.");
        }

        if (value.ImprovementThreshold < 0)
        {
            errors.Add($"{nameof(ProfilerSettings.ImprovementThreshold)} must be non-negative.");
        }

        if (value.SlowStageThresholdMs < 0)
        {
            errors.Add($"{nameof(ProfilerSettings.SlowStageThresholdMs)} must be non-negative.");
        }

        if (value.HighMemoryThresholdBytes < 0)
        {
            errors.Add($"{nameof(ProfilerSettings.HighMemoryThresholdBytes)} must be non-negative.");
        }

        // Validate Visualization settings
        errors.AddRange(value.Visualization.Validate());

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates the provided <see cref="VisualizationSettings"/> instance.
    /// </summary>
    /// <param name="value">The visualization settings to validate.</param>
    /// <returns>List of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this VisualizationSettings value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        if (value.MaxDepth < 1)
        {
            errors.Add($"Visualization.{nameof(VisualizationSettings.MaxDepth)} must be at least 1.");
        }

        if (value.MaxNodes < 1)
        {
            errors.Add($"Visualization.{nameof(VisualizationSettings.MaxNodes)} must be at least 1.");
        }

        if (value.CostBarWidth < 5)
        {
            errors.Add($"Visualization.{nameof(VisualizationSettings.CostBarWidth)} must be at least 5 characters.");
        }

        if (value.BottleneckCostThreshold < 0)
        {
            errors.Add($"Visualization.{nameof(VisualizationSettings.BottleneckCostThreshold)} must be non-negative.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the provided <see cref="ProfilerSettings"/> instance is valid.
    /// </summary>
    /// <param name="value">The settings to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ProfilerSettings value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the provided <see cref="ProfilerSettings"/> instance is valid.
    /// Throws an <see cref="ArgumentException"/> with a detailed message listing all validation errors.
    /// </summary>
    /// <param name="value">The settings to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the settings are invalid.</exception>
    public static void EnsureValid(this ProfilerSettings value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"ProfilerSettings validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }
}
