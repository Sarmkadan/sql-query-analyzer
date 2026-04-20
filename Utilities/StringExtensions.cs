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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string NormalizeSqlWhitespace(this string query)
    {
        if (string.IsNullOrEmpty(query))
            return query;

        // Replace multiple spaces with single space
        var normalized = Regex.Replace(query, @"\s+", " ");

        // Normalize line breaks
        normalized = normalized.Replace("\r\n", " ").Replace("\n", " ");

        return normalized.Trim();
    }

    // Remove SQL comments
    public static string RemoveSqlComments(this string query)
    {
        if (string.IsNullOrEmpty(query))
            return query;

        // Remove line comments
        var withoutLineComments = Regex.Replace(query, @"--.*?(?=\r\n|\n|$)", string.Empty);

        // Remove block comments
        var withoutBlockComments = Regex.Replace(withoutLineComments, @"/\*[\s\S]*?\*/", string.Empty);

        return withoutBlockComments;
    }

    // Truncate string with ellipsis
    public static string Truncate(this string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;

        return text.Substring(0, maxLength - 3) + "...";
    }

    // Check if string is SQL keyword
    public static bool IsSqlKeyword(this string word)
    {
        var keywords = new[]
        {
            "SELECT", "FROM", "WHERE", "JOIN", "LEFT", "RIGHT", "INNER", "OUTER",
            "ON", "AND", "OR", "NOT", "IN", "BETWEEN", "LIKE", "EXISTS",
            "ORDER", "BY", "GROUP", "HAVING", "LIMIT", "OFFSET", "INSERT",
            "UPDATE", "DELETE", "CREATE", "DROP", "ALTER", "TABLE", "INDEX",
            "DATABASE", "PROCEDURE", "FUNCTION", "VIEW", "TRIGGER", "CONSTRAINT",
            "PRIMARY", "KEY", "FOREIGN", "UNIQUE", "CHECK", "DEFAULT", "NULL"
        };

        return keywords.Contains(word.ToUpperInvariant());
    }

    // Capitalize first letter
    public static string CapitalizeFirst(this string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        return char.ToUpper(text[0]) + text.Substring(1);
    }

    // Convert to snake_case
    public static string ToSnakeCase(this string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var withUnderscores = Regex.Replace(text, @"([a-z0-9])([A-Z])", "$1_$2");
        return withUnderscores.ToLowerInvariant();
    }

    // Count occurrences of substring
    public static int CountOccurrences(this string text, string substring)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(substring))
            return 0;

        return (text.Length - text.Replace(substring, string.Empty).Length) / substring.Length;
    }

    // Check if contains SQL injection patterns
    public static bool ContainsSuspiciousPatterns(this string query)
    {
        var suspiciousPatterns = new[]
        {
            @"'\s+or\s+",
            @";\s*drop\s+",
            @";\s*delete\s+",
            @"union\s+select",
            @"exec\s*\(",
            @"execute\s*\("
        };

        return suspiciousPatterns.Any(pattern =>
            Regex.IsMatch(query, pattern, RegexOptions.IgnoreCase));
    }

    // Extract query type
    public static string ExtractQueryType(this string query)
    {
        var normalized = query.NormalizeSqlWhitespace().ToUpperInvariant();

        if (normalized.StartsWith("SELECT"))
            return "SELECT";
        if (normalized.StartsWith("INSERT"))
            return "INSERT";
        if (normalized.StartsWith("UPDATE"))
            return "UPDATE";
        if (normalized.StartsWith("DELETE"))
            return "DELETE";
        if (normalized.StartsWith("CREATE"))
            return "CREATE";
        if (normalized.StartsWith("DROP"))
            return "DROP";

        return "UNKNOWN";
    }

    // Split query into statements
    public static List<string> SplitStatements(this string query)
    {
        if (string.IsNullOrEmpty(query))
            return new List<string>();

        var statements = query.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
        return statements.Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
    }

    // Get line and column position
    public static (int Line, int Column) GetPosition(this string text, int index)
    {
        if (string.IsNullOrEmpty(text) || index < 0 || index >= text.Length)
            return (0, 0);

        var line = 1;
        var column = 1;

        for (int i = 0; i < index; i++)
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
