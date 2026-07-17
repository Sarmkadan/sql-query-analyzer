#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.Globalization;

namespace SqlQueryAnalyzer.Caching;

/// <summary>
/// Provides validation helpers for <see cref="QueryAnalysisCache"/> instances.
/// Validates cache state, statistics, and entry data for correctness and consistency.
/// </summary>
public static class QueryAnalysisCacheValidation
{
    /// <summary>
    /// Validates the cache instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The cache instance to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <remarks>
    /// Delegates all validation logic to <see cref="ValidateCacheStatistics"/> which handles
    /// comprehensive validation of cache statistics including entry counts, hit rate, access patterns,
    /// and age metrics.
    /// </remarks>
    public static IReadOnlyList<string> Validate(this QueryAnalysisCache? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();
        var stats = value.GetStatistics();
        ValidateCacheStatistics(stats, problems);

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the cache instance is valid.
    /// </summary>
    /// <param name="value">The cache instance to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this QueryAnalysisCache? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures the cache instance is valid, throwing an <see cref="ArgumentException"/> with details
    /// if any validation problems are found.
    /// </summary>
    /// <param name="value">The cache instance to validate.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <remarks>
    /// Throws an exception with a detailed message listing all validation problems when the cache
    /// fails validation.
    /// </remarks>
    public static void EnsureValid(this QueryAnalysisCache? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);

        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            "QueryAnalysisCache is invalid. Problems:\n- " + string.Join("\n- ", problems) + "\n"
        );
    }

    private static void ValidateCacheStatistics(CacheStatistics stats, List<string> problems)
    {
        ArgumentNullException.ThrowIfNull(problems);

        if (stats.TotalEntries < 0)
        {
            problems.Add($"TotalEntries must be non-negative: {stats.TotalEntries}");
        }

        if (stats.MaxEntries <= 0)
        {
            problems.Add($"MaxEntries must be positive: {stats.MaxEntries}");
        }

        if (stats.TotalEntries > stats.MaxEntries)
        {
            problems.Add(
                $"TotalEntries ({stats.TotalEntries}) cannot exceed MaxEntries ({stats.MaxEntries})"
            );
        }

        if (double.IsNaN(stats.HitRate) || double.IsInfinity(stats.HitRate))
        {
            problems.Add($"HitRate must be a valid number: {stats.HitRate}");
        }
        else if (stats.HitRate < 0 || stats.HitRate > 100)
        {
            problems.Add(
                $"HitRate must be between 0 and 100: {stats.HitRate.ToString(CultureInfo.InvariantCulture)}"
            );
        }

        if (double.IsNaN(stats.AverageAccessCount) || double.IsInfinity(stats.AverageAccessCount))
        {
            problems.Add($"AverageAccessCount must be a valid number: {stats.AverageAccessCount}");
        }
        else if (stats.AverageAccessCount < 0)
        {
            problems.Add(
                $"AverageAccessCount must be non-negative: {stats.AverageAccessCount.ToString(CultureInfo.InvariantCulture)}"
            );
        }

        if (double.IsNaN(stats.OldestEntryAge) || double.IsInfinity(stats.OldestEntryAge))
        {
            problems.Add($"OldestEntryAge must be a valid number: {stats.OldestEntryAge}");
        }
        else if (stats.OldestEntryAge < 0)
        {
            problems.Add(
                $"OldestEntryAge must be non-negative: {stats.OldestEntryAge.ToString(CultureInfo.InvariantCulture)}"
            );
        }

        if (stats.Hits < 0)
        {
            problems.Add($"Hits must be non-negative: {stats.Hits}");
        }

        if (stats.Misses < 0)
        {
            problems.Add($"Misses must be non-negative: {stats.Misses}");
        }
    }
}