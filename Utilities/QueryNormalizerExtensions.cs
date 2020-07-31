using System;
using System.Collections.Generic;

namespace SqlQueryAnalyzer.Utilities;

/// <summary>
/// Provides extension methods for <see cref="QueryNormalizer"/>.
/// </summary>
public static class QueryNormalizerExtensions
{
    /// <summary>
    /// Normalizes and returns the query as a trimmed string.
    /// </summary>
    /// <param name="normalizer">The <see cref="QueryNormalizer"/> instance.</param>
    /// <param name="query">The SQL query to normalize.</param>
    /// <returns>The normalized, trimmed SQL query.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="normalizer"/> is null.</exception>
    public static string NormalizeAndTrim(this QueryNormalizer normalizer, string query)
    {
        ArgumentNullException.ThrowIfNull(normalizer);
        if (string.IsNullOrWhiteSpace(query))
            return string.Empty;
        return normalizer.Normalize(query).Trim();
    }

    /// <summary>
    /// Extracts unique table names as a read-only list.
    /// </summary>
    /// <param name="normalizer">The <see cref="QueryNormalizer"/> instance.</param>
    /// <param name="query">The SQL query to extract table names from.</param>
    /// <returns>A read-only list of unique table names.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="normalizer"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="query"/> is null or empty.</exception>
    public static IReadOnlyList<string> GetTableNames(this QueryNormalizer normalizer, string query)
    {
        ArgumentNullException.ThrowIfNull(normalizer);
        ArgumentException.ThrowIfNullOrEmpty(query);
        return normalizer.ExtractTableNames(query);
    }

    /// <summary>
    /// Extracts unique column names from the SELECT clause as a read-only list.
    /// </summary>
    /// <param name="normalizer">The <see cref="QueryNormalizer"/> instance.</param>
    /// <param name="query">The SQL query to extract column names from.</param>
    /// <returns>A read-only list of unique column names.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="normalizer"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="query"/> is null or empty.</exception>
    public static IReadOnlyList<string> GetColumnNames(this QueryNormalizer normalizer, string query)
    {
        ArgumentNullException.ThrowIfNull(normalizer);
        ArgumentException.ThrowIfNullOrEmpty(query);
        return normalizer.ExtractColumnNames(query);
    }
}
