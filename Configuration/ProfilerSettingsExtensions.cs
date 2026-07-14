using System;
using System.Globalization;

namespace SqlQueryAnalyzer.Configuration;

/// <summary>
/// Provides extension methods for the <see cref="ProfilerSettings"/> class.
/// </summary>
public static class ProfilerSettingsExtensions
{
    /// <summary>
    /// Validates the settings and throws if any errors are found.
    /// </summary>
    /// <param name="settings">The settings to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="settings"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if validation fails.</exception>
    public static void ValidateOrThrow(this ProfilerSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var errors = settings.Validate();
        if (errors.Count > 0)
        {
            throw new InvalidOperationException($"Profiler configuration is invalid: {string.Join("; ", errors)}");
        }
    }

    /// <summary>
    /// Gets the high memory threshold in megabytes.
    /// </summary>
    /// <param name="settings">The settings to query.</param>
    /// <returns>The high memory threshold in MB.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="settings"/> is null.</exception>
    public static double GetHighMemoryThresholdMb(this ProfilerSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        return settings.HighMemoryThresholdBytes / (1024.0 * 1024.0);
    }

    /// <summary>
    /// Determines if the settings are configured for low-overhead production usage.
    /// </summary>
    /// <param name="settings">The settings to check.</param>
    /// <returns>True if configured for production.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="settings"/> is null.</exception>
    public static bool IsProductionConfig(this ProfilerSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        return !settings.CaptureExecutionPlanByDefault
               && !settings.CaptureResourceUsageByDefault
               && !settings.IncludePlanVisualizationByDefault;
    }

    /// <summary>
    /// Generates a brief diagnostic summary string of the settings.
    /// </summary>
    /// <param name="settings">The settings to summarize.</param>
    /// <returns>A string summary.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="settings"/> is null.</exception>
    public static string ToDiagnosticString(this ProfilerSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        return string.Format(CultureInfo.InvariantCulture,
            "MaxDuration: {0}ms, MaxBatchSize: {1}, ProductionMode: {2}",
            settings.DefaultMaxDurationMs,
            settings.MaxBatchSize,
            settings.IsProductionConfig());
    }
}
