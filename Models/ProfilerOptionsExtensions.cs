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
    /// and multiple measurement iterations are enabled.
    /// </summary>
    /// <param name="options">The profiler options instance.</param>
    /// <returns><c>true</c> if warm-up is greater than 0 and measurement iterations are greater than 1; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public static bool IsHighPrecision(this ProfilerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.WarmUpIterations > 0 && options.MeasurementIterations > 1;
    }

    /// <summary>
    /// Returns a list of active profiling features based on the boolean configuration flags.
    /// </summary>
    /// <param name="options">The profiler options instance.</param>
    /// <returns>An enumerable of enabled feature names.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
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
