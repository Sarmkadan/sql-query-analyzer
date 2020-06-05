#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Provides extension methods for <see cref="SlowQueryEntry"/> to enhance query analysis capabilities.
/// </summary>
public static class SlowQueryEntryExtensions
{
    /// <summary>
    /// Formats the query duration in a human-readable format with appropriate units.
    /// </summary>
    /// <param name="entry">The slow query entry to format.</param>
    /// <returns>Formatted duration string (e.g., "1.2s", "500ms", "2.5ms").</returns>
    public static string FormatDuration(this SlowQueryEntry entry)
    {
        if (entry == null) throw new ArgumentNullException(nameof(entry));

        double totalMs = entry.Duration.TotalMilliseconds;

        if (totalMs >= 1000)
        {
            return $"{totalMs / 1000:F2}s";
        }
        else if (totalMs >= 1)
        {
            return $"{totalMs:F0}ms";
        }
        else
        {
            return $"{totalMs * 1000:F1}μs";
        }
    }

    /// <summary>
    /// Formats the lock time in a human-readable format with appropriate units.
    /// </summary>
    /// <param name="entry">The slow query entry to format.</param>
    /// <returns>Formatted lock time string (e.g., "800ms", "2.3s", "150μs").</returns>
    public static string FormatLockTime(this SlowQueryEntry entry)
    {
        if (entry == null) throw new ArgumentNullException(nameof(entry));

        double lockMs = entry.LockTime.TotalMilliseconds;

        if (lockMs >= 1000)
        {
            return $"{lockMs / 1000:F2}s";
        }
        else if (lockMs >= 1)
        {
            return $"{lockMs:F0}ms";
        }
        else
        {
            return $"{lockMs * 1000:F1}μs";
        }
    }

    /// <summary>
    /// Determines if this query is considered slow based on a threshold.
    /// </summary>
    /// <param name="entry">The slow query entry to check.</param>
    /// <param name="thresholdMilliseconds">The duration threshold in milliseconds to consider a query slow.</param>
    /// <returns>True if the query duration exceeds the threshold; otherwise, false.</returns>
    public static bool IsSlow(this SlowQueryEntry entry, double thresholdMilliseconds = 1000)
    {
        if (entry == null) throw new ArgumentNullException(nameof(entry));

        return entry.Duration.TotalMilliseconds > thresholdMilliseconds;
    }

    /// <summary>
    /// Gets a formatted efficiency score for the query.
    /// </summary>
    /// <param name="entry">The slow query entry to analyze.</param>
    /// <returns>Formatted efficiency score string (e.g., "95%", "50%", "1.2%").</returns>
    public static string GetEfficiencyScore(this SlowQueryEntry entry)
    {
        if (entry == null) throw new ArgumentNullException(nameof(entry));

        double ratio = entry.EfficiencyRatio * 100;
        return $"{ratio:F1}%";
    }
}