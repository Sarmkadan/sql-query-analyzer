#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;

namespace SqlQueryAnalyzer.Diagnostics;

/// <summary>
/// Provides validation helpers for <see cref="AnalyzerHealthCheck"/> related types.
/// Validates health check results, component health status, and self-healing results.
/// </summary>
public static class AnalyzerHealthCheckValidation
{
    /// <summary>
    /// Validates the provided <see cref="HealthCheckResult"/> instance.
    /// Returns a list of human-readable validation errors.
    /// </summary>
    /// <param name="value">The health check result to validate.</param>
    /// <returns>List of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this HealthCheckResult value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate CheckTime - should be a recent date (within last 24 hours)
        if (value.CheckTime == default)
        {
            errors.Add($"{nameof(HealthCheckResult.CheckTime)} must be set to a non-default value.");
        }
        else if (value.CheckTime < DateTime.UtcNow.AddHours(-24))
        {
            errors.Add($"{nameof(HealthCheckResult.CheckTime)} is too old (older than 24 hours).");
        }

        // Validate Status - should be a valid HealthStatus enum value
        // HealthStatus is an enum, so any value is technically valid, but we can check for Unspecified
        if (value.Status == (HealthStatus)(-1))
        {
            errors.Add($"{nameof(HealthCheckResult.Status)} has an invalid value.");
        }

        // Validate CacheHealth - should not be null
        if (value.CacheHealth is null)
        {
            errors.Add($"{nameof(HealthCheckResult.CacheHealth)} must not be null.");
        }
        else if (string.IsNullOrWhiteSpace(value.CacheHealth.Component))
        {
            errors.Add($"{nameof(HealthCheckResult.CacheHealth)}.{nameof(ComponentHealth.Component)} must not be null or empty.");
        }

        // Validate RateLimiterHealth - should not be null
        if (value.RateLimiterHealth is null)
        {
            errors.Add($"{nameof(HealthCheckResult.RateLimiterHealth)} must not be null.");
        }
        else if (string.IsNullOrWhiteSpace(value.RateLimiterHealth.Component))
        {
            errors.Add($"{nameof(HealthCheckResult.RateLimiterHealth)}.{nameof(ComponentHealth.Component)} must not be null or empty.");
        }

        // Validate MetricsHealth - should not be null
        if (value.MetricsHealth is null)
        {
            errors.Add($"{nameof(HealthCheckResult.MetricsHealth)} must not be null.");
        }
        else if (string.IsNullOrWhiteSpace(value.MetricsHealth.Component))
        {
            errors.Add($"{nameof(HealthCheckResult.MetricsHealth)}.{nameof(ComponentHealth.Component)} must not be null or empty.");
        }

        // Validate DatabaseHealth - should not be null
        if (value.DatabaseHealth is null)
        {
            errors.Add($"{nameof(HealthCheckResult.DatabaseHealth)} must not be null.");
        }
        else if (string.IsNullOrWhiteSpace(value.DatabaseHealth.Component))
        {
            errors.Add($"{nameof(HealthCheckResult.DatabaseHealth)}.{nameof(ComponentHealth.Component)} must not be null or empty.");
        }

        // Validate Errors collection - should not be null
        if (value.Errors is null)
        {
            errors.Add($"{nameof(HealthCheckResult.Errors)} collection must not be null.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates the provided <see cref="ComponentHealth"/> instance.
    /// Returns a list of human-readable validation errors.
    /// </summary>
    /// <param name="value">The component health to validate.</param>
    /// <returns>List of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ComponentHealth value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Component - should not be null or empty
        if (string.IsNullOrWhiteSpace(value.Component))
        {
            errors.Add($"{nameof(ComponentHealth.Component)} must not be null or empty.");
        }

        // Validate Status - should be a valid HealthStatus enum value
        if (value.Status == (HealthStatus)(-1))
        {
            errors.Add($"{nameof(ComponentHealth.Status)} has an invalid value.");
        }

        // Validate Message - can be empty but not null
        if (value.Message is null)
        {
            errors.Add($"{nameof(ComponentHealth.Message)} must not be null.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates the provided <see cref="SelfHealResult"/> instance.
    /// Returns a list of human-readable validation errors.
    /// </summary>
    /// <param name="value">The self-heal result to validate.</param>
    /// <returns>List of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this SelfHealResult value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Success flag
        // No specific validation needed - can be true or false

        // Validate ActionsPerformed collection - should not be null
        if (value.ActionsPerformed is null)
        {
            errors.Add($"{nameof(SelfHealResult.ActionsPerformed)} collection must not be null.");
        }

        // Validate Error - should be null when Success is true
        if (value.Success && !string.IsNullOrEmpty(value.Error))
        {
            errors.Add($"{nameof(SelfHealResult.Error)} must be null when {nameof(SelfHealResult.Success)} is true.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the provided <see cref="HealthCheckResult"/> instance is valid.
    /// </summary>
    /// <param name="value">The health check result to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this HealthCheckResult value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Determines whether the provided <see cref="ComponentHealth"/> instance is valid.
    /// </summary>
    /// <param name="value">The component health to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ComponentHealth value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Determines whether the provided <see cref="SelfHealResult"/> instance is valid.
    /// </summary>
    /// <param name="value">The self-heal result to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this SelfHealResult value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the provided <see cref="HealthCheckResult"/> instance is valid.
    /// Throws an <see cref="ArgumentException"/> with a detailed message listing all validation errors.
    /// </summary>
    /// <param name="value">The health check result to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the result is invalid.</exception>
    public static void EnsureValid(this HealthCheckResult value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"HealthCheckResult validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }

    /// <summary>
    /// Ensures that the provided <see cref="ComponentHealth"/> instance is valid.
    /// Throws an <see cref="ArgumentException"/> with a detailed message listing all validation errors.
    /// </summary>
    /// <param name="value">The component health to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the result is invalid.</exception>
    public static void EnsureValid(this ComponentHealth value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"ComponentHealth validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }

    /// <summary>
    /// Ensures that the provided <see cref="SelfHealResult"/> instance is valid.
    /// Throws an <see cref="ArgumentException"/> with a detailed message listing all validation errors.
    /// </summary>
    /// <param name="value">The self-heal result to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the result is invalid.</exception>
    public static void EnsureValid(this SelfHealResult value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"SelfHealResult validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }
}
