#nullable enable

using System;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Caching;

/// <summary>
/// Provides extension methods for <see cref="QueryAnalysisCache"/>.
/// </summary>
public static class QueryAnalysisCacheExtensions
{
    /// <summary>
    /// Gets the cached result for a query, or adds it if not present.
    /// </summary>
    /// <param name="cache">The cache instance.</param>
    /// <param name="query">The query to analyze.</param>
    /// <param name="valueFactory">A function to generate the analysis result if not cached.</param>
    /// <returns>The analysis result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cache"/> or <paramref name="valueFactory"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="query"/> is null or empty.</exception>
    public static QueryAnalysisResult GetOrAdd(
        this QueryAnalysisCache cache,
        string query,
        Func<string, QueryAnalysisResult> valueFactory)
    {
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(valueFactory);
        ArgumentException.ThrowIfNullOrEmpty(query);

        if (cache.TryGetResult(query, out var result) && result is not null)
        {
            return result;
        }

        var newResult = valueFactory(query);
        cache.Set(query, newResult);
        return newResult;
    }

    /// <summary>
    /// Checks if the cache is at maximum capacity.
    /// </summary>
    /// <param name="cache">The cache instance.</param>
    /// <returns>True if the cache is full.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cache"/> is null.</exception>
    public static bool IsFull(this QueryAnalysisCache cache)
    {
        ArgumentNullException.ThrowIfNull(cache);

        var stats = cache.GetStatistics();
        return cache.Count >= stats.MaxEntries;
    }

    /// <summary>
    /// Returns a string summary of the cache statistics.
    /// </summary>
    /// <param name="cache">The cache instance.</param>
    /// <returns>A formatted string summary.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cache"/> is null.</exception>
    public static string GetSummary(this QueryAnalysisCache cache)
    {
        ArgumentNullException.ThrowIfNull(cache);

        return cache.GetStatistics().ToString();
    }
}
