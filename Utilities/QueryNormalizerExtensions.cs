using System;
using System.Collections.Generic;

namespace SqlQueryAnalyzer.Utilities;

/// <summary>
/// Provides extension methods for <see cref="QueryNormalizer"/> that offer a more convenient API
/// for common normalization operations.
/// </summary>
public static class QueryNormalizerExtensions
{
    /// <summary>
    /// Normalizes and returns the query as a trimmed string.
    /// </summary>
    /// <param name="normalizer">The <see cref="QueryNormalizer"/> instance.</param>
    /// <param name="query">The SQL query to normalize.</param>
    /// <returns>The normalized, trimmed SQL query. Returns <see cref="string.Empty"/> if the input is null, empty, or whitespace.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="normalizer"/> is null.</exception>
    public static string NormalizeAndTrim(this QueryNormalizer normalizer, string query)
    {
        ArgumentNullException.ThrowIfNull(normalizer);
        return string.IsNullOrWhiteSpace(query)
            ? string.Empty
            : normalizer.Normalize(query).Trim();
    }

    /// <summary>
    /// Extracts unique table names as a read-only list.
    /// </summary>
    /// <param name="normalizer">The <see cref="QueryNormalizer"/> instance.</param>
    /// <param name="query">The SQL query to extract table names from.</param>
    /// <returns>A read-only list of unique table names in the order they appear in the query,
    /// with case-insensitive comparison.</returns>
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
    /// <returns>A read-only list of unique column names from the SELECT clause,
    /// with case-insensitive comparison. Returns an empty list if the query has no SELECT clause.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="normalizer"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="query"/> is null or empty.</exception>
    public static IReadOnlyList<string> GetColumnNames(this QueryNormalizer normalizer, string query)
    {
        ArgumentNullException.ThrowIfNull(normalizer);
        ArgumentException.ThrowIfNullOrEmpty(query);
        return normalizer.ExtractColumnNames(query);
    }

    /// <summary>
    /// Parameterizes a SQL query by replacing numeric and string literals with ? placeholders.
    /// Returns the parameterized query as a trimmed string.
    /// </summary>
    /// <param name="normalizer">The <see cref="QueryNormalizer"/> instance.</param>
    /// <param name="query">The SQL query to parameterize.</param>
    /// <returns>The parameterized, trimmed SQL query. Returns <see cref="string.Empty"/> if the input is null, empty, or whitespace.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="normalizer"/> is null.</exception>
    public static string ToParameterizedQueryAndTrim(this QueryNormalizer normalizer, string query)
    {
        ArgumentNullException.ThrowIfNull(normalizer);
        return string.IsNullOrWhiteSpace(query)
            ? string.Empty
            : normalizer.ToParameterizedQuery(query).Trim();
    }
}
