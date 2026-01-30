// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SqlQueryAnalyzer.Utilities;

/// <summary>
/// Analyzes SQL patterns and identifies optimization opportunities
/// </summary>
public static class SqlPatternAnalyzer
{
    // Detect N+1 query patterns
    public static bool DetectNPlusOnePattern(List<string> queries)
    {
        if (queries.Count < 2)
            return false;

        var groupedByTable = new Dictionary<string, int>();

        foreach (var query in queries)
        {
            var tables = ExtractTablesFromQuery(query);
            foreach (var table in tables)
            {
                if (!groupedByTable.ContainsKey(table))
                    groupedByTable[table] = 0;
                groupedByTable[table]++;
            }
        }

        // If same table is accessed multiple times, likely N+1
        return groupedByTable.Values.Any(count => count > 5);
    }

    // Extract table names from query
    public static List<string> ExtractTablesFromQuery(string query)
    {
        var tables = new List<string>();
        var patterns = new[]
        {
            @"FROM\s+(\w+)",
            @"JOIN\s+(\w+)",
            @"INTO\s+(\w+)",
            @"UPDATE\s+(\w+)"
        };

        var seen = new HashSet<string>();

        foreach (var pattern in patterns)
        {
            var matches = Regex.Matches(query, pattern, RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                var table = match.Groups[1].Value;
                if (!string.IsNullOrWhiteSpace(table) && seen.Add(table))
                {
                    tables.Add(table);
                }
            }
        }

        return tables;
    }

    // Detect missing WHERE clause
    public static bool HasMissingWhereClause(string query)
    {
        var isSelect = query.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase);
        return isSelect && !query.Contains("WHERE", StringComparison.OrdinalIgnoreCase);
    }

    // Detect SELECT *
    public static bool HasSelectStar(string query)
    {
        return Regex.IsMatch(query, @"SELECT\s+\*", RegexOptions.IgnoreCase);
    }

    // Detect LIKE with leading wildcard
    public static bool HasLeadingWildcardLike(string query)
    {
        return Regex.IsMatch(query, @"LIKE\s+'%", RegexOptions.IgnoreCase);
    }

    // Detect function on column in WHERE
    public static bool HasFunctionOnColumn(string query)
    {
        if (!query.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
            return false;

        var functions = new[] { "UPPER", "LOWER", "CONVERT", "CAST", "DATEPART", "YEAR", "MONTH", "DAY" };
        return functions.Any(func =>
            Regex.IsMatch(query, $@"\b{func}\s*\(", RegexOptions.IgnoreCase));
    }

    // Detect implicit JOIN (comma-separated tables in FROM)
    public static bool HasImplicitJoin(string query)
    {
        return Regex.IsMatch(query, @"FROM\s+\w+\s*,\s*\w+", RegexOptions.IgnoreCase);
    }

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

        var whereClause = ExtractWhereClause(query);
        var orMatches = Regex.Matches(whereClause, @"\bOR\b", RegexOptions.IgnoreCase);
        return orMatches.Count;
    }

    // Detect subquery pattern
    public static bool HasSubquery(string query)
    {
        return Regex.IsMatch(query, @"SELECT\s+.*FROM\s+\(", RegexOptions.IgnoreCase);
    }

    // Detect UNION vs UNION ALL
    public static int CountUnion(string query)
    {
        var unionMatches = Regex.Matches(query, @"\bUNION\b", RegexOptions.IgnoreCase);
        return unionMatches.Count;
    }

    // Detect JOIN conditions
    public static List<string> ExtractJoinConditions(string query)
    {
        var conditions = new List<string>();
        var pattern = @"ON\s+(.+?)(?=WHERE|GROUP|ORDER|UNION|$)";
        var matches = Regex.Matches(query, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

        foreach (Match match in matches)
        {
            conditions.Add(match.Groups[1].Value.Trim());
        }

        return conditions;
    }

    // Extract WHERE clause
    public static string ExtractWhereClause(string query)
    {
        var match = Regex.Match(query, @"WHERE\s+(.+?)(?=GROUP|ORDER|UNION|LIMIT|$)",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    // Detect CASE statement complexity
    public static int CountCaseStatements(string query)
    {
        var caseMatches = Regex.Matches(query, @"\bCASE\b", RegexOptions.IgnoreCase);
        return caseMatches.Count;
    }

    // Detect aggregate functions
    public static bool HasAggregateFunction(string query)
    {
        var aggregates = new[] { "SUM", "COUNT", "AVG", "MIN", "MAX", "STRING_AGG", "GROUP_CONCAT" };
        return aggregates.Any(agg =>
            Regex.IsMatch(query, $@"\b{agg}\s*\(", RegexOptions.IgnoreCase));
    }

    // Detect window functions
    public static bool HasWindowFunction(string query)
    {
        return Regex.IsMatch(query, @"OVER\s*\(", RegexOptions.IgnoreCase);
    }

    // Analyze query readability score (0-100)
    public static double CalculateReadabilityScore(string query)
    {
        var score = 100.0;

        // Penalize for SELECT *
        if (HasSelectStar(query))
            score -= 10;

        // Penalize for implicit JOINs
        if (HasImplicitJoin(query))
            score -= 20;

        // Penalize for missing WHERE on SELECT
        if (HasMissingWhereClause(query) && !query.Contains("LIMIT", StringComparison.OrdinalIgnoreCase))
            score -= 15;

        // Penalize for complex nesting
        var nestingLevel = CountParentheses(query);
        score -= Math.Min(20, nestingLevel * 2);

        // Penalize for leading wildcards
        if (HasLeadingWildcardLike(query))
            score -= 10;

        // Penalize for functions on columns
        if (HasFunctionOnColumn(query))
            score -= 5;

        return Math.Max(0, score);
    }

    // Count parentheses level (nesting indicator)
    public static int CountParentheses(string query)
    {
        var maxLevel = 0;
        var currentLevel = 0;

        foreach (var c in query)
        {
            if (c == '(')
            {
                currentLevel++;
                maxLevel = Math.Max(maxLevel, currentLevel);
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
