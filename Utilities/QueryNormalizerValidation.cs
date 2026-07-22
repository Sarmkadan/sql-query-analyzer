#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SqlQueryAnalyzer.Utilities;

/// <summary>
/// Provides safe validation extension methods for <see cref="QueryNormalizer"/> operations.
/// All methods include null/empty input validation and exception handling to prevent
/// exceptions from propagating to callers.
/// </summary>
public static class QueryNormalizerValidation
{
    /// <summary>
    /// Safely normalizes a SQL query with null/empty validation.
    /// </summary>
    /// <param name="normalizer">The <see cref="QueryNormalizer"/> instance.</param>
    /// <param name="query">The SQL query to normalize.</param>
    /// <param name="normalizedQuery">Receives the normalized query if validation succeeds.</param>
    /// <returns>True if normalization succeeded; false if validation failed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="normalizer"/> is null.</exception>
    public static bool TryNormalize(this QueryNormalizer normalizer, [NotNullWhen(true)] string? query, [NotNullWhen(true)] out string? normalizedQuery)
    {
        ArgumentNullException.ThrowIfNull(normalizer);

        normalizedQuery = null;
        if (string.IsNullOrWhiteSpace(query))
            return false;

        try
        {
            normalizedQuery = normalizer.Normalize(query);
            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Log exception details in debug builds for diagnostics
            System.Diagnostics.Debug.WriteLine($"Query normalization failed: {ex}");
            return false;
        }
    }

    /// <summary>
    /// Safely parameterizes a SQL query with null/empty validation.
    /// </summary>
    /// <param name="normalizer">The <see cref="QueryNormalizer"/> instance.</param>
    /// <param name="query">The SQL query to parameterize.</param>
    /// <param name="parameterizedQuery">Receives the parameterized query if validation succeeds.</param>
    /// <returns>True if parameterization succeeded; false if validation failed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="normalizer"/> is null.</exception>
    public static bool TryToParameterizedQuery(this QueryNormalizer normalizer, [NotNullWhen(true)] string? query, [NotNullWhen(true)] out string? parameterizedQuery)
    {
        ArgumentNullException.ThrowIfNull(normalizer);

        parameterizedQuery = null;
        if (string.IsNullOrWhiteSpace(query))
            return false;

        try
        {
            parameterizedQuery = normalizer.ToParameterizedQuery(query);
            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Log exception details in debug builds for diagnostics
            System.Diagnostics.Debug.WriteLine($"Query parameterization failed: {ex}");
            return false;
        }
    }

    /// <summary>
    /// Safely extracts table names with null/empty validation.
    /// </summary>
    /// <param name="normalizer">The <see cref="QueryNormalizer"/> instance.</param>
    /// <param name="query">The SQL query to extract table names from.</param>
    /// <param name="tableNames">Receives the extracted table names if validation succeeds.</param>
    /// <returns>True if extraction succeeded; false if validation failed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="normalizer"/> is null.</exception>
    public static bool TryExtractTableNames(this QueryNormalizer normalizer, string? query, [NotNullWhen(true)] out string[]? tableNames)
    {
        ArgumentNullException.ThrowIfNull(normalizer);

        tableNames = null;
        if (string.IsNullOrWhiteSpace(query))
            return false;

        try
        {
            var tables = normalizer.ExtractTableNames(query);
            tableNames = [.. tables];
            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Log exception details in debug builds for diagnostics
            System.Diagnostics.Debug.WriteLine($"Table name extraction failed: {ex}");
            return false;
        }
    }

    /// <summary>
    /// Safely extracts column names with null/empty validation.
    /// </summary>
    /// <param name="normalizer">The <see cref="QueryNormalizer"/> instance.</param>
    /// <param name="query">The SQL query to extract column names from.</param>
    /// <param name="columnNames">Receives the extracted column names if validation succeeds.</param>
    /// <returns>True if extraction succeeded; false if validation failed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="normalizer"/> is null.</exception>
    public static bool TryExtractColumnNames(this QueryNormalizer normalizer, string? query, [NotNullWhen(true)] out string[]? columnNames)
    {
        ArgumentNullException.ThrowIfNull(normalizer);

        columnNames = null;
        if (string.IsNullOrWhiteSpace(query))
            return false;

        try
        {
            var columns = normalizer.ExtractColumnNames(query);
            columnNames = [.. columns];
            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Log exception details in debug builds for diagnostics
            System.Diagnostics.Debug.WriteLine($"Column name extraction failed: {ex}");
            return false;
        }
    }
}