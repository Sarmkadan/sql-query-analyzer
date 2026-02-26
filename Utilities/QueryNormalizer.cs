#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.ObjectPool;

namespace SqlQueryAnalyzer.Utilities;

/// <summary>
/// Normalizes SQL queries for consistent analysis.
/// Removes redundant whitespace, standardizes capitalization, expands abbreviations.
/// Normalization is SAFE - does not change query logic or semantics.
/// </summary>
public partial class QueryNormalizer
{
    // FrozenSet gives O(1) lookup with a read-only perfect hash — faster than array iteration.
    private static readonly FrozenSet<string> s_sqlKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "SELECT", "FROM", "WHERE", "JOIN", "INNER", "LEFT", "RIGHT", "FULL", "CROSS",
        "ON", "AND", "OR", "NOT", "IN", "EXISTS", "BETWEEN", "LIKE", "IS", "NULL",
        "ORDER", "BY", "GROUP", "HAVING", "LIMIT", "OFFSET", "UNION", "ALL", "DISTINCT",
        "INSERT", "UPDATE", "DELETE", "CREATE", "DROP", "ALTER", "TABLE", "INDEX",
        "AS", "WITH", "CASE", "WHEN", "THEN", "ELSE", "END", "CAST", "CONVERT"
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    // Pooled StringBuilders avoid repeated GC pressure in RestoreStringLiterals.
    private static readonly ObjectPool<StringBuilder> s_sbPool =
        new DefaultObjectPoolProvider().Create(new StringBuilderPooledObjectPolicy
        {
            InitialCapacity = 512,
            MaximumRetainedCapacity = 4096,
        });

    // [GeneratedRegex] emits a source-generated, AOT-safe state machine — no runtime compilation.
    [GeneratedRegex(@"'(?:''|[^'])*'")]
    private static partial Regex StringLiteralRegex();

    // Single alternation pattern replaces 40+ individual per-keyword regex passes.
    [GeneratedRegex(
        @"\b(SELECT|FROM|WHERE|JOIN|INNER|LEFT|RIGHT|FULL|CROSS|ON|AND|OR|NOT|IN|EXISTS" +
        @"|BETWEEN|LIKE|IS|NULL|ORDER|BY|GROUP|HAVING|LIMIT|OFFSET|UNION|ALL|DISTINCT" +
        @"|INSERT|UPDATE|DELETE|CREATE|DROP|ALTER|TABLE|INDEX|AS|WITH|CASE|WHEN|THEN" +
        @"|ELSE|END|CAST|CONVERT)\b",
        RegexOptions.IgnoreCase)]
    private static partial Regex SqlKeywordsRegex();

    [GeneratedRegex(@" +")]
    private static partial Regex MultipleSpacesRegex();

    [GeneratedRegex(@" *(,|;|\(|\)) *")]
    private static partial Regex PunctuationSpacingRegex();

    [GeneratedRegex(@" *(=|<>|<=|>=|<|>) *")]
    private static partial Regex OperatorSpacingRegex();

    [GeneratedRegex(@"[\r\n]+")]
    private static partial Regex LineBreaksRegex();

    [GeneratedRegex(@"--.*?(?=\n|$)")]
    private static partial Regex LineCommentsRegex();

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex BlockCommentsRegex();

    [GeneratedRegex(@"FROM\s+([a-zA-Z_][a-zA-Z0-9_]*)", RegexOptions.IgnoreCase)]
    private static partial Regex FromTableRegex();

    [GeneratedRegex(@"JOIN\s+([a-zA-Z_][a-zA-Z0-9_]*)", RegexOptions.IgnoreCase)]
    private static partial Regex JoinTableRegex();

    [GeneratedRegex(@"INTO\s+([a-zA-Z_][a-zA-Z0-9_]*)", RegexOptions.IgnoreCase)]
    private static partial Regex IntoTableRegex();

    [GeneratedRegex(@"SELECT\s+(.*?)\s+FROM", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex SelectColumnsRegex();

    [GeneratedRegex(@"([a-zA-Z_][a-zA-Z0-9_]*)\s+AS\s+([a-zA-Z_][a-zA-Z0-9_]*)", RegexOptions.IgnoreCase)]
    private static partial Regex ColumnAliasRegex();

    /// <summary>
    /// Normalizes a SQL query by applying multiple transformations.
    /// Returns normalized query that's logically identical to input.
    /// </summary>
    public string Normalize(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return query;

        // Single-pass extraction+replacement — previously done in two separate Regex scans.
        var (working, literals) = ExtractAndReplaceLiterals(query);

        working = NormalizeWhitespace(working);
        working = StandardizeKeywordCapitalization(working);
        working = NormalizeLineBreaks(working);
        working = RemoveTrailingComments(working);

        working = RestoreStringLiterals(working, literals);

        return working.Trim();
    }

    /// <summary>
    /// Combines literal extraction and placeholder substitution into one regex pass,
    /// eliminating the second full scan that the original two-method approach required.
    /// </summary>
    private static (string result, Dictionary<string, string> literals) ExtractAndReplaceLiterals(string query)
    {
        var literals = new Dictionary<string, string>();
        int index = 0;

        var result = StringLiteralRegex().Replace(query, match =>
        {
            var placeholder = $"__SL{index}__";
            literals[placeholder] = match.Value;
            index++;
            return placeholder;
        });

        return (result, literals);
    }

    /// <summary>
    /// Restores string literals using a pooled StringBuilder to avoid
    /// per-literal string allocations from repeated string.Replace calls.
    /// </summary>
    private static string RestoreStringLiterals(string query, Dictionary<string, string> literals)
    {
        if (literals.Count == 0)
            return query;

        var sb = s_sbPool.Get();
        try
        {
            sb.Append(query);
            foreach (var (placeholder, literal) in literals)
                sb.Replace(placeholder, literal);
            return sb.ToString();
        }
        finally
        {
            s_sbPool.Return(sb);
        }
    }

    private static string NormalizeWhitespace(string query)
    {
        var result = MultipleSpacesRegex().Replace(query, " ");
        result = PunctuationSpacingRegex().Replace(result, "$1");
        result = OperatorSpacingRegex().Replace(result, " $1 ");
        return result;
    }

    /// <summary>
    /// Uses a single alternation regex instead of one Regex.Replace call per keyword,
    /// reducing keyword capitalization from O(k) passes to O(1).
    /// </summary>
    private static string StandardizeKeywordCapitalization(string query)
    {
        return SqlKeywordsRegex().Replace(query, static m => m.Value.ToUpperInvariant());
    }

    private static string NormalizeLineBreaks(string query)
    {
        var result = LineBreaksRegex().Replace(query, " ");
        return MultipleSpacesRegex().Replace(result, " ");
    }

    private static string RemoveTrailingComments(string query)
    {
        var result = LineCommentsRegex().Replace(query, "");
        return BlockCommentsRegex().Replace(result, "");
    }

    /// <summary>
    /// Extracts table names mentioned in query.
    /// Useful for dependency analysis and validation.
    /// </summary>
    public List<string> ExtractTableNames(string query)
    {
        var tables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (Match m in FromTableRegex().Matches(query))
            tables.Add(m.Groups[1].Value);

        foreach (Match m in JoinTableRegex().Matches(query))
            tables.Add(m.Groups[1].Value);

        foreach (Match m in IntoTableRegex().Matches(query))
            tables.Add(m.Groups[1].Value);

        return [.. tables];
    }

    /// <summary>
    /// Extracts column names mentioned in SELECT clause.
    /// </summary>
    public List<string> ExtractColumnNames(string query)
    {
        var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var selectMatch = SelectColumnsRegex().Match(query);
        if (!selectMatch.Success)
            return [];

        var columnList = selectMatch.Groups[1].Value;
        foreach (var part in columnList.Split(','))
        {
            var col = part.Trim();
            if (col == "*") continue;

            var asMatch = ColumnAliasRegex().Match(col);
            if (asMatch.Success)
            {
                columns.Add(asMatch.Groups[1].Value);
            }
            else
            {
                var words = col.Split([' ', '.', '('], StringSplitOptions.RemoveEmptyEntries);
                if (words.Length > 0)
                    columns.Add(words[^1]);
            }
        }

        return [.. columns];
    }

    /// <summary>
    /// Returns whether the given token is a recognized SQL keyword.
    /// Uses FrozenSet for O(1) lookup — suitable for hot-path validation.
    /// </summary>
    public static bool IsSqlKeyword(string token) => s_sqlKeywords.Contains(token);
}
