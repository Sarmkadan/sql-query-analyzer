using System;
using System.Collections.Generic;

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Provides extension methods for <see cref="ProfilerOptions"/>.
/// </summary>
public static class ProfilerOptionsExtensions
{
    /// <summary>
    /// Checks if the profiler is configured for high-precision measurement by ensuring warm-up
    /// iterations are enabled and multiple measurement iterations are performed.
    /// </summary>
    /// <param name="options">The profiler options instance.</param>
    /// <returns><c>true</c> if <see cref="ProfilerOptions.WarmUpIterations"/> is greater than 0 and
    /// <see cref="ProfilerOptions.MeasurementIterations"/> is greater than 1; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <c>null</c>.</exception>
    public static bool IsHighPrecision(this ProfilerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.WarmUpIterations > 0 && options.MeasurementIterations > 1;
    }

    /// <summary>
    /// Returns an enumerable of active profiling feature names based on the boolean configuration flags.
    /// </summary>
    /// <param name="options">The profiler options instance.</param>
    /// <returns>An enumerable of enabled feature names corresponding to the active profiling features.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <c>null</c>.</exception>
    public static IEnumerable<string> GetActiveFeatures(this ProfilerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.CaptureExecutionPlan)
        {
            yield return nameof(options.CaptureExecutionPlan);
        }

        if (options.CaptureTimings)
        {
            yield return nameof(options.CaptureTimings);
        }

        if (options.CaptureResourceUsage)
        {
            yield return nameof(options.CaptureResourceUsage);
        }

        if (options.IncludePlanVisualization)
        {
            yield return nameof(options.IncludePlanVisualization);
        }
    }
}
