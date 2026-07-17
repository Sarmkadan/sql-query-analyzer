#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using SqlQueryAnalyzer.Configuration;

namespace SqlQueryAnalyzer.Services;

/// <summary>
/// Provides validation helpers for <see cref="QueryProfilerService"/> to ensure service
/// dependencies, configuration, and runtime state are valid before use.
/// </summary>
public static class QueryProfilerServiceValidation
{
    /// <summary>
    /// Validates the specified <see cref="QueryProfilerService"/> instance and returns a list of
    /// human-readable validation problems. Returns an empty list if the instance is valid.
    /// </summary>
    /// <param name="value">The service instance to validate.</param>
    /// <returns>A read-only list of validation problem descriptions.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this QueryProfilerService value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate ProfilerSettings for runtime issues
        if (value._settings == null)
        {
            problems.Add("ProfilerSettings instance is null");
        }
        else
        {
            ValidateProfilerSettings(value._settings, problems);
        }

        // Validate that all required services are properly initialized
        ValidateServiceDependency(value._queryAnalyzer, nameof(QueryProfilerService._queryAnalyzer), problems);
        ValidateServiceDependency(value._planAnalyzer, nameof(QueryProfilerService._planAnalyzer), problems);
        ValidateServiceDependency(value._issueDetector, nameof(QueryProfilerService._issueDetector), problems);
        ValidateServiceDependency(value._planVisualizer, nameof(QueryProfilerService._planVisualizer), problems);

        return problems;
    }

    /// <summary>
    /// Determines whether the specified <see cref="QueryProfilerService"/> instance is valid.
    /// </summary>
    /// <param name="value">The service instance to check.</param>
    /// <returns>True if the instance is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this QueryProfilerService value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="QueryProfilerService"/> instance is valid.
    /// Throws an <see cref="ArgumentException"/> with a detailed message listing all validation problems.
    /// </summary>
    /// <param name="value">The service instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is not valid, containing a list of problems.</exception>
    public static void EnsureValid(this QueryProfilerService value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"QueryProfilerService is not valid. Problems: {string.Join(", ", problems)}.",
            nameof(value));
    }

    private static void ValidateProfilerSettings(ProfilerSettings settings, List<string> problems)
    {
        if (settings.MaxQueryLengthChars <= 0)
        {
            problems.Add($"ProfilerSettings.MaxQueryLengthChars must be positive, but was {settings.MaxQueryLengthChars}");
        }

        if (settings.MaxBatchSize <= 0)
        {
            problems.Add($"ProfilerSettings.MaxBatchSize must be positive, but was {settings.MaxBatchSize}");
        }

        if (settings.SlowStageThresholdMs <= 0)
        {
            problems.Add($"ProfilerSettings.SlowStageThresholdMs must be positive, but was {settings.SlowStageThresholdMs}");
        }

        if (settings.HighMemoryThresholdBytes <= 0)
        {
            problems.Add($"ProfilerSettings.HighMemoryThresholdBytes must be positive, but was {settings.HighMemoryThresholdBytes}");
        }

        if (settings.RegressionThreshold < 0)
        {
            problems.Add($"ProfilerSettings.RegressionThreshold must be non-negative, but was {settings.RegressionThreshold}");
        }

        if (settings.ImprovementThreshold < 0)
        {
            problems.Add($"ProfilerSettings.ImprovementThreshold must be non-negative, but was {settings.ImprovementThreshold}");
        }

        if (settings.DefaultMaxDurationMs <= 0)
        {
            problems.Add($"ProfilerSettings.DefaultMaxDurationMs must be positive, but was {settings.DefaultMaxDurationMs}");
        }
    }

    private static void ValidateServiceDependency<T>(T dependency, string dependencyName, List<string> problems)
    {
        if (dependency is null)
        {
            problems.Add($"Service dependency {dependencyName} is null");
        }
    }
}
