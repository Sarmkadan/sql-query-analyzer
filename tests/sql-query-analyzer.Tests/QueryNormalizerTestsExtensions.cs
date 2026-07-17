#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using SqlQueryAnalyzer.Utilities;

namespace SqlQueryAnalyzer.Tests;

/// <summary>
/// Provides extension methods for <see cref="QueryNormalizerTests"/> to enable fluent assertions
/// and simplify test case generation for query normalization scenarios.
/// </summary>
public static class QueryNormalizerTestsExtensions
{
    /// <summary>
    /// Creates a normalized query from the specified SQL input, ensuring consistent formatting
    /// for assertion comparisons.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="sqlInput">The SQL query string to normalize.</param>
    /// <returns>The normalized query string with standardized formatting and capitalization.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sqlInput"/> is null.</exception>
    public static string NormalizeQuery(this QueryNormalizerTests tests, string sqlInput)
    {
        ArgumentNullException.ThrowIfNull(sqlInput);

        var normalizer = new QueryNormalizer();
        return normalizer.Normalize(sqlInput);
    }

    /// <summary>
    /// Extracts table names from the specified SQL query and returns them as a read-only collection.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="sqlQuery">The SQL query string to analyze.</param>
    /// <returns>A read-only list of unique table names found in the query, case-insensitive.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sqlQuery"/> is null.</exception>
    public static IReadOnlyList<string> ExtractTables(this QueryNormalizerTests tests, string sqlQuery)
    {
        ArgumentNullException.ThrowIfNull(sqlQuery);

        var normalizer = new QueryNormalizer();
        return normalizer.ExtractTableNames(sqlQuery);
    }

    /// <summary>
    /// Creates a test assertion that verifies SQL keyword normalization behavior.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="originalQuery">The original query with lowercase keywords.</param>
    /// <param name="expectedKeywords">The expected uppercase keywords that should be present.</param>
    /// <returns>A formatted assertion message for use in test assertions.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="originalQuery"/> or <paramref name="expectedKeywords"/> is null.</exception>
    public static string ShouldNormalizeKeywordsTo(this QueryNormalizerTests tests, string originalQuery, params string[] expectedKeywords)
        => $"Normalized query should contain keywords: {string.Join(", ", expectedKeywords)}";

    /// <summary>
    /// Creates a test assertion that verifies string literal preservation during normalization.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="queryWithLiteral">The query containing a string literal to preserve.</param>
    /// <param name="expectedLiteral">The expected literal value that should remain unchanged.</param>
    /// <returns>A formatted assertion message for use in test assertions.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="queryWithLiteral"/> or <paramref name="expectedLiteral"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="expectedLiteral"/> is empty.</exception>
    public static string ShouldPreserveLiteral(this QueryNormalizerTests tests, string queryWithLiteral, string expectedLiteral)
        => $"Normalized query should preserve string literal: '{expectedLiteral}'";

}