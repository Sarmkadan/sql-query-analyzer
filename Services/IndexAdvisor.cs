#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Services;

/// <summary>
/// Analyzes SQL queries and execution plans to recommend index creation.
/// Detects missing indexes on WHERE, JOIN, and ORDER BY columns.
/// </summary>
public sealed class IndexAdvisor
{
    private readonly ILogger<IndexAdvisor> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="IndexAdvisor"/>.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public IndexAdvisor(ILogger<IndexAdvisor> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Extracts columns used in WHERE clauses that may benefit from indexing.
    /// </summary>
    /// <param name="sql">The SQL query to analyze.</param>
    /// <returns>List of column names found in filter predicates.</returns>
    public List<string> ExtractFilterColumns(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            return new List<string>();

        _logger.LogDebug("Extracting filter columns from WHERE clause");

        var columns = new List<string>();
        var whereMatch = Regex.Match(sql, @"\bWHERE\b(.+?)(?:\bGROUP\b|\bORDER\b|\bHAVING\b|\bLIMIT\b|$)",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        if (!whereMatch.Success)
            return columns;

        var whereClause = whereMatch.Groups[1].Value;
        var colMatches = Regex.Matches(whereClause, @"(?:^|AND|OR)\s*(\w+(?:\.\w+)?)\s*(?:=|<>|!=|<|>|<=|>=|LIKE|IN|IS|BETWEEN)",
            RegexOptions.IgnoreCase);

        foreach (Match m in colMatches)
        {
            var col = m.Groups[1].Value;
            if (!columns.Contains(col, StringComparer.OrdinalIgnoreCase))
                columns.Add(col);
        }

        return columns;
    }

    /// <summary>
    /// Extracts columns used in JOIN ON conditions.
    /// </summary>
    /// <param name="sql">The SQL query.</param>
    /// <returns>List of column names used in join predicates.</returns>
    public List<string> ExtractJoinColumns(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            return new List<string>();

        var columns = new List<string>();
        var onMatches = Regex.Matches(sql,
            @"\bON\s+(\w+(?:\.\w+)?)\s*=\s*(\w+(?:\.\w+)?)",
            RegexOptions.IgnoreCase);

        foreach (Match m in onMatches)
        {
            var left = m.Groups[1].Value;
            var right = m.Groups[2].Value;
            if (!columns.Contains(left, StringComparer.OrdinalIgnoreCase)) columns.Add(left);
            if (!columns.Contains(right, StringComparer.OrdinalIgnoreCase)) columns.Add(right);
        }

        return columns;
    }

    /// <summary>
    /// Generates index recommendations based on query analysis.
    /// Combines filter, join, and order-by column analysis.
    /// </summary>
    /// <param name="sql">The SQL query to analyze.</param>
    /// <param name="tables">Known table names to correlate columns with tables.</param>
    /// <returns>List of index recommendations.</returns>
    public List<IndexRecommendation> Recommend(string sql, IReadOnlyList<string> tables)
    {
        if (string.IsNullOrWhiteSpace(sql))
            return new List<IndexRecommendation>();

        _logger.LogInformation("Generating index recommendations for {TableCount} table(s)", tables.Count);

        var recommendations = new List<IndexRecommendation>();
        var filterCols = ExtractFilterColumns(sql);
        var joinCols = ExtractJoinColumns(sql);
        var orderCols = ExtractOrderByColumns(sql);

        var allCols = filterCols
            .Concat(joinCols)
            .Concat(orderCols)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var col in allCols)
        {
            var parts = col.Split('.');
            var columnName = parts.Last();
            var tableHint = parts.Length > 1 ? parts.First() : null;

            var matchedTable = tableHint != null
                ? tables.FirstOrDefault(t => t.EndsWith(tableHint, StringComparison.OrdinalIgnoreCase)) ?? tableHint
                : tables.FirstOrDefault() ?? "unknown_table";

            var source = filterCols.Contains(col, StringComparer.OrdinalIgnoreCase) ? RecommendationSource.WhereClause
                : joinCols.Contains(col, StringComparer.OrdinalIgnoreCase) ? RecommendationSource.JoinCondition
                : RecommendationSource.OrderBy;

            var impact = source == RecommendationSource.WhereClause ? 80.0
                : source == RecommendationSource.JoinCondition ? 60.0
                : 30.0;

            var rec = new IndexRecommendation
            {
                TableName = matchedTable,
                KeyColumns = new List<string> { columnName },
                IndexType = "NONCLUSTERED",
                ImpactScore = impact,
                Source = source,
                Rationale = $"Column '{columnName}' used in {source} clause"
            };
            rec.GenerateScript();
            recommendations.Add(rec);
        }

        _logger.LogInformation("Generated {Count} index recommendation(s)", recommendations.Count);
        return recommendations;
    }

    /// <summary>
    /// Extracts columns referenced in ORDER BY clauses.
    /// </summary>
    /// <param name="sql">The SQL query.</param>
    /// <returns>List of column names in ORDER BY.</returns>
    public List<string> ExtractOrderByColumns(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            return new List<string>();

        var columns = new List<string>();
        var match = Regex.Match(sql, @"\bORDER\s+BY\s+(.+?)(?:\bLIMIT\b|\bOFFSET\b|\bFETCH\b|;|$)",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        if (!match.Success)
            return columns;

        var orderClause = match.Groups[1].Value;
        var colMatches = Regex.Matches(orderClause, @"(\w+(?:\.\w+)?)\s*(?:ASC|DESC|,|$)", RegexOptions.IgnoreCase);

        foreach (Match m in colMatches)
        {
            var col = m.Groups[1].Value;
            if (!string.Equals(col, "ASC", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(col, "DESC", StringComparison.OrdinalIgnoreCase)
                && !columns.Contains(col, StringComparer.OrdinalIgnoreCase))
            {
                columns.Add(col);
            }
        }

        return columns;
    }
}
