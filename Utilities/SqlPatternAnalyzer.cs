#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace SqlQueryAnalyzer.Utilities;

/// <summary>
/// Analyzes SQL patterns and identifies optimization opportunities.
/// All Regex fields are source-generated ([GeneratedRegex]) — no runtime compilation overhead.
/// FrozenSet is used for O(1) keyword membership tests on hot paths.
/// </summary>
public static partial class SqlPatternAnalyzer
{
    // FrozenSet gives a read-only perfect-hash lookup, faster than array + Any() + per-call Regex.
    private static readonly FrozenSet<string> s_functionNames =
        new[] { "UPPER", "LOWER", "CONVERT", "CAST", "DATEPART", "YEAR", "MONTH", "DAY" }
        .ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    private static readonly FrozenSet<string> s_aggregateNames =
        new[] { "SUM", "COUNT", "AVG", "MIN", "MAX", "STRING_AGG", "GROUP_CONCAT" }
        .ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    // ── Source-generated regexes ─────────────────────────────────────────────
    // Previously, several methods created a new Regex (or called Regex.Matches with
    // a fresh pattern string) on every invocation. [GeneratedRegex] compiles these
    // to a state machine at build time — zero allocation at call time.

    [GeneratedRegex(@"SELECT\s+\*", RegexOptions.IgnoreCase)]
    private static partial Regex SelectStarRegex();

    [GeneratedRegex(@"LIKE\s+'%", RegexOptions.IgnoreCase)]
    private static partial Regex LeadingWildcardRegex();

    // Single alternation replaces the original loop of per-function Regex.IsMatch calls.
    [GeneratedRegex(@"\b(UPPER|LOWER|CONVERT|CAST|DATEPART|YEAR|MONTH|DAY)\s*\(", RegexOptions.IgnoreCase)]
    private static partial Regex FunctionOnColumnRegex();

    [GeneratedRegex(@"FROM\s+\w+\s*,\s*\w+", RegexOptions.IgnoreCase)]
    private static partial Regex ImplicitJoinRegex();

    [GeneratedRegex(@"\bOR\b", RegexOptions.IgnoreCase)]
    private static partial Regex OrConditionRegex();

    [GeneratedRegex(@"SELECT\s+.*FROM\s+\(", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex SubqueryRegex();

    [GeneratedRegex(@"\bUNION\b", RegexOptions.IgnoreCase)]
    private static partial Regex UnionRegex();

    [GeneratedRegex(@"\bCASE\b", RegexOptions.IgnoreCase)]
    private static partial Regex CaseRegex();

    // Single alternation replaces the original loop of per-aggregate Regex.IsMatch calls.
    [GeneratedRegex(@"\b(SUM|COUNT|AVG|MIN|MAX|STRING_AGG|GROUP_CONCAT)\s*\(", RegexOptions.IgnoreCase)]
    private static partial Regex AggregateFunctionRegex();

    [GeneratedRegex(@"OVER\s*\(", RegexOptions.IgnoreCase)]
    private static partial Regex WindowFunctionRegex();

    [GeneratedRegex(@"WHERE\s+(.+?)(?=GROUP|ORDER|UNION|LIMIT|$)", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex WhereClauseRegex();

    [GeneratedRegex(@"ON\s+(.+?)(?=WHERE|GROUP|ORDER|UNION|$)", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex JoinConditionRegex();

    // Combined FROM/JOIN/INTO/UPDATE into one pass — previously four separate Regex.Matches calls.
    [GeneratedRegex(@"(?:FROM|JOIN|INTO|UPDATE)\s+(\w+)", RegexOptions.IgnoreCase)]
    private static partial Regex TableNameRegex();

    // ── Public API ───────────────────────────────────────────────────────────

    // Detect N+1 query patterns
    public static bool DetectNPlusOnePattern(List<string> queries)
    {
        if (queries.Count < 2)
            return false;

        var groupedByTable = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var query in queries)
        {
            foreach (var table in ExtractTablesFromQuery(query))
            {
                groupedByTable.TryGetValue(table, out int count);
                groupedByTable[table] = count + 1;
            }
        }

        foreach (var count in groupedByTable.Values)
        {
            if (count > 5) return true;
        }
        return false;
    }

    // Extract table names from query — single regex pass (was 4 passes).
    public static List<string> ExtractTablesFromQuery(string query)
    {
        var tables = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (Match match in TableNameRegex().Matches(query))
        {
            var table = match.Groups[1].Value;
            if (!string.IsNullOrWhiteSpace(table) && seen.Add(table))
                tables.Add(table);
        }

        return tables;
    }

    // Detect missing WHERE clause
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasMissingWhereClause(string query)
    {
        var isSelect = query.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase);
        return isSelect && !query.Contains("WHERE", StringComparison.OrdinalIgnoreCase);
    }

    // Detect SELECT *
    public static bool HasSelectStar(string query) =>
        SelectStarRegex().IsMatch(query);

    // Detect LIKE with leading wildcard
    public static bool HasLeadingWildcardLike(string query) =>
        LeadingWildcardRegex().IsMatch(query);

    // Detect function on column in WHERE — was looping over array + per-function Regex.
    public static bool HasFunctionOnColumn(string query)
    {
        if (!query.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
            return false;

        return FunctionOnColumnRegex().IsMatch(query);
    }

    // Detect implicit JOIN (comma-separated tables in FROM)
    public static bool HasImplicitJoin(string query) =>
        ImplicitJoinRegex().IsMatch(query);

    // Detect DISTINCT without ORDER BY
    public static bool HasDistinctWithoutOrder(string query)
    {
        var hasDistinct = query.Contains("DISTINCT", StringComparison.OrdinalIgnoreCase);
        var hasOrderBy = query.Contains("ORDER BY", StringComparison.OrdinalIgnoreCase);
        return hasDistinct && !hasOrderBy;
    }

    // Detect OR conditions in WHERE
    public static int CountOrConditions(string query)
    {
        if (!query.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
            return 0;

        return OrConditionRegex().Matches(ExtractWhereClause(query)).Count;
    }

    // Detect subquery pattern
    public static bool HasSubquery(string query) =>
        SubqueryRegex().IsMatch(query);

    // Detect UNION vs UNION ALL
    public static int CountUnion(string query) =>
        UnionRegex().Matches(query).Count;

    // Detect JOIN conditions
    public static List<string> ExtractJoinConditions(string query)
    {
        var conditions = new List<string>();
        foreach (Match match in JoinConditionRegex().Matches(query))
            conditions.Add(match.Groups[1].Value.Trim());
        return conditions;
    }

    // Extract WHERE clause
    public static string ExtractWhereClause(string query)
    {
        var match = WhereClauseRegex().Match(query);
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    // Detect CASE statement complexity
    public static int CountCaseStatements(string query) =>
        CaseRegex().Matches(query).Count;

    // Detect aggregate functions — was looping over array + per-aggregate Regex.
    public static bool HasAggregateFunction(string query) =>
        AggregateFunctionRegex().IsMatch(query);

    // Detect window functions
    public static bool HasWindowFunction(string query) =>
        WindowFunctionRegex().IsMatch(query);

    // Analyze query readability score (0-100)
    public static double CalculateReadabilityScore(string query)
    {
        var score = 100.0;

        if (HasSelectStar(query))
            score -= 10;

        if (HasImplicitJoin(query))
            score -= 20;

        if (HasMissingWhereClause(query) && !query.Contains("LIMIT", StringComparison.OrdinalIgnoreCase))
            score -= 15;

        score -= Math.Min(20, CountParentheses(query) * 2);

        if (HasLeadingWildcardLike(query))
            score -= 10;

        if (HasFunctionOnColumn(query))
            score -= 5;

        return Math.Max(0, score);
    }

    /// <summary>
    /// Count maximum parenthesis nesting depth.
    /// Uses ReadOnlySpan&lt;char&gt; to iterate without per-character boxing.
    /// </summary>
    public static int CountParentheses(string query)
    {
        var span = query.AsSpan();
        int maxLevel = 0;
        int currentLevel = 0;

        foreach (var c in span)
        {
            if (c == '(')
            {
                if (++currentLevel > maxLevel)
                    maxLevel = currentLevel;
            }
            else if (c == ')')
            {
                currentLevel--;
            }
        }

        return maxLevel;
    }

    // Generate optimization recommendations
    public static List<string> GenerateOptimizationRecommendations(string query)
    {
        var recommendations = new List<string>();

        if (HasSelectStar(query))
            recommendations.Add("Replace SELECT * with specific column names");

        if (HasMissingWhereClause(query) && !query.Contains("LIMIT", StringComparison.OrdinalIgnoreCase))
            recommendations.Add("Add WHERE clause or LIMIT to reduce result set");

        if (HasImplicitJoin(query))
            recommendations.Add("Replace implicit JOIN (comma-separated tables) with explicit JOIN syntax");

        if (HasFunctionOnColumn(query))
            recommendations.Add("Move functions to right side of comparison or use computed columns with indexes");

        if (HasLeadingWildcardLike(query))
            recommendations.Add("Use full-text search instead of LIKE with leading wildcard");

        if (CountOrConditions(query) > 2)
            recommendations.Add("Consider using UNION ALL instead of multiple OR conditions");

        if (HasSubquery(query))
            recommendations.Add("Review subquery - consider JOIN instead for better performance");

        var readability = CalculateReadabilityScore(query);
        if (readability < 50)
            recommendations.Add("Consider refactoring query for better readability and maintainability");

        return recommendations;
    }
}
