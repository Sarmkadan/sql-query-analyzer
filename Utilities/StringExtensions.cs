#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;

namespace SqlQueryAnalyzer.Utilities;

/// <summary>
/// Extension methods for string manipulation
/// </summary>
public static class StringExtensions
{
    // Normalize whitespace in SQL queries
    /// <summary>
    /// Normalizes whitespace in SQL queries by replacing multiple whitespace characters with single spaces,
    /// normalizing line breaks, and trimming the result.
    /// </summary>
    /// <param name="query">The SQL query string to normalize.</param>
    /// <returns>The normalized query string, or the original string if it's null or empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string NormalizeSqlWhitespace(this string query)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.Length == 0)
            return query;

        // Replace multiple spaces with single space
        var normalized = Regex.Replace(query, @"\s+", " ");

        // Normalize line breaks
        normalized = normalized.Replace("\r\n", " ").Replace("\n", " ");

        return normalized.Trim();
    }

    // Remove SQL comments
    /// <summary>
    /// Removes both line comments (-- to end of line) and block comments (/* ... */) from SQL queries.
    /// </summary>
    /// <param name="query">The SQL query string to process.</param>
    /// <returns>The query with comments removed, or the original string if it's null or empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is null.</exception>
    public static string RemoveSqlComments(this string query)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.Length == 0)
            return query;

        // Remove line comments
        var withoutLineComments = Regex.Replace(query, @"--.*?(?=\r\n|\n|$)", string.Empty);

        // Remove block comments
        var withoutBlockComments = Regex.Replace(withoutLineComments, @"/\*[\s\S]*?\*/", string.Empty);

        return withoutBlockComments;
    }

    // Truncate string with ellipsis
    /// <summary>
    /// Truncates a string to the specified maximum length, adding an ellipsis (...) if the string is longer.
    /// </summary>
    /// <param name="text">The string to truncate.</param>
    /// <param name="maxLength">The maximum length of the result string.</param>
    /// <returns>The truncated string with ellipsis, or the original string if it's null, empty, or shorter than maxLength.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="text"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="maxLength"/> is negative.</exception>
    public static string Truncate(this string text, int maxLength)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentOutOfRangeException.ThrowIfNegative(maxLength);

        if (text.Length <= maxLength)
            return text;

        return text[..Math.Min(maxLength, text.Length)] + "...";
    }

    // Check if string is SQL keyword
    /// <summary>
    /// Determines whether the specified word is a common SQL keyword.
    /// </summary>
    /// <param name="word">The word to check.</param>
    /// <returns><see langword="true"/> if the word is a SQL keyword; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="word"/> is null.</exception>
    public static bool IsSqlKeyword(this string word)
    {
        ArgumentNullException.ThrowIfNull(word);

        return word.ToUpperInvariant() switch
        {
            "SELECT" or "FROM" or "WHERE" or "JOIN" or "LEFT" or "RIGHT" or "INNER" or "OUTER" or
            "ON" or "AND" or "OR" or "NOT" or "IN" or "BETWEEN" or "LIKE" or "EXISTS" or
            "ORDER" or "BY" or "GROUP" or "HAVING" or "LIMIT" or "OFFSET" or "INSERT" or
            "UPDATE" or "DELETE" or "CREATE" or "DROP" or "ALTER" or "TABLE" or "INDEX" or
            "DATABASE" or "PROCEDURE" or "FUNCTION" or "VIEW" or "TRIGGER" or "CONSTRAINT" or
            "PRIMARY" or "KEY" or "FOREIGN" or "UNIQUE" or "CHECK" or "DEFAULT" or "NULL" => true,
            _ => false
        };
    }

    // Capitalize first letter
    /// <summary>
    /// Capitalizes the first character of the string.
    /// </summary>
    /// <param name="text">The string to capitalize.</param>
    /// <returns>The string with the first character capitalized, or the original string if it's null or empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="text"/> is null.</exception>
    public static string CapitalizeFirst(this string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (text.Length == 0)
            return text;

        return char.ToUpperInvariant(text[0]) + text[1..];
    }

    // Convert to snake_case
    /// <summary>
    /// Converts a PascalCase or camelCase string to snake_case.
    /// </summary>
    /// <param name="text">The string to convert.</param>
    /// <returns>The snake_case representation of the string, or the original string if it's null or empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="text"/> is null.</exception>
    public static string ToSnakeCase(this string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (text.Length == 0)
            return text;

        var withUnderscores = Regex.Replace(text, @"([a-z0-9])([A-Z])", "$1_$2");
        return withUnderscores.ToLowerInvariant();
    }

    // Count occurrences of substring
    /// <summary>
    /// Counts the number of occurrences of a substring within a string.
    /// </summary>
    /// <param name="text">The string to search.</param>
    /// <param name="substring">The substring to count.</param>
    /// <returns>The number of occurrences of the substring.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="text"/> or <paramref name="substring"/> is null.</exception>
    public static int CountOccurrences(this string text, string substring)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(substring);

        if (substring.Length == 0)
            return 0;

        return (text.Length - text.Replace(substring, string.Empty).Length) / substring.Length;
    }

    // Check if contains SQL injection patterns
    /// <summary>
    /// Checks if the query contains common SQL injection patterns.
    /// </summary>
    /// <param name="query">The SQL query to check.</param>
    /// <returns><see langword="true"/> if suspicious patterns are found; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is null.</exception>
    public static bool ContainsSuspiciousPatterns(this string query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var suspiciousPatterns = new[]
        {
            @"'\s+or\s+",
            @";\s*drop\s+",
            @";\s*delete\s+",
            @"union\s+select",
            @"exec\s*\\(",
            @"execute\s*\\("
        };

        return suspiciousPatterns.Any(pattern =>
            Regex.IsMatch(query, pattern, RegexOptions.IgnoreCase));
    }

    // Extract query type
    /// <summary>
    /// Extracts the query type (SELECT, INSERT, UPDATE, DELETE, CREATE, DROP, UNKNOWN) from a SQL query.
    /// </summary>
    /// <param name="query">The SQL query to analyze.</param>
    /// <returns>The query type as a string.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is null.</exception>
    public static string ExtractQueryType(this string query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var normalized = query.NormalizeSqlWhitespace().ToUpperInvariant();

        return normalized switch
        {
            var s when s.StartsWith("SELECT") => "SELECT",
            var s when s.StartsWith("INSERT") => "INSERT",
            var s when s.StartsWith("UPDATE") => "UPDATE",
            var s when s.StartsWith("DELETE") => "DELETE",
            var s when s.StartsWith("CREATE") => "CREATE",
            var s when s.StartsWith("DROP") => "DROP",
            _ => "UNKNOWN"
        };
    }

    // Split query into statements
    /// <summary>
    /// Splits a SQL query into individual statements using semicolon as a delimiter.
    /// </summary>
    /// <param name="query">The SQL query to split.</param>
    /// <returns>A list of individual SQL statements.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is null.</exception>
    public static List<string> SplitStatements(this string query)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.Length == 0)
            return [];

        var statements = query.Split([";"], StringSplitOptions.RemoveEmptyEntries);
        return statements.Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
    }

    // Get line and column position
    /// <summary>
    /// Gets the line and column position for a given character index in the string.
    /// </summary>
    /// <param name="text">The text to analyze.</param>
    /// <param name="index">The character index to find the position for.</param>
    /// <returns>A tuple containing the line and column numbers (1-based).</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="text"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is negative or greater than the string length.</exception>
    public static (int Line, int Column) GetPosition(this string text, int index)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, text.Length);

        var line = 1;
        var column = 1;

        for (var i = 0; i < index; i++)
        {
            if (text[i] == '\n')
            {
                line++;
                column = 1;
            }
            else
            {
                column++;
            }
        }

        return (line, column);
    }
}