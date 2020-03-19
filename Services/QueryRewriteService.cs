#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Services;

/// <summary>
/// Generates query rewrite suggestions and complementary index recommendations
/// derived from structural analysis of a <see cref="DatabaseQuery"/>.
/// </summary>
public interface IQueryRewriteService
{
    /// <summary>
    /// Analyses a query and returns all applicable rewrite suggestions,
    /// each paired with index recommendations that make the transformation most effective.
    /// </summary>
    /// <param name="query">The parsed query to analyse.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task<List<QueryRewriteSuggestion>> GenerateRewriteSuggestionsAsync(
        DatabaseQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Derives standalone index recommendations from the query's WHERE and JOIN
    /// structure without proposing a full textual rewrite.
    /// </summary>
    /// <param name="query">The parsed query to analyse.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task<List<IndexSuggestion>> GenerateIndexRecommendationsAsync(
        DatabaseQuery query,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Pattern-based implementation of <see cref="IQueryRewriteService"/>.
/// Uses regex and clause-level inspection to produce prioritised, actionable suggestions.
/// </summary>
public class QueryRewriteService : IQueryRewriteService
{
    private readonly ILogger<QueryRewriteService> _logger;

    public QueryRewriteService(ILogger<QueryRewriteService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<List<QueryRewriteSuggestion>> GenerateRewriteSuggestionsAsync(
        DatabaseQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        _logger.LogInformation("Generating rewrite suggestions for query: {QueryId}", query.QueryId);
        cancellationToken.ThrowIfCancellationRequested();

        var candidates = new List<QueryRewriteSuggestion?>();

        try
        {
            candidates.Add(SuggestExplicitColumns(query));
            // WHERE-dependent suggestions are only applicable when the query has a WHERE clause
            if (query.WhereConditions.Count > 0)
            {
                candidates.Add(SuggestOrToUnionAll(query));
                candidates.Add(SuggestFunctionSargability(query));
            }
            candidates.Add(SuggestSubqueryToJoin(query));
            candidates.Add(SuggestNotInToNotExists(query));
            candidates.Add(SuggestUnionToUnionAll(query));
            candidates.Add(SuggestPagination(query));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating rewrite suggestions for query: {QueryId}", query.QueryId);
            throw;
        }

        var result = candidates
            .Where(s => s is not null && s.IsValid())
            .Cast<QueryRewriteSuggestion>()
            .OrderBy(s => s.Priority)
            .ThenByDescending(s => s.EstimatedImprovementPercent)
            .ToList();

        _logger.LogInformation($"Generated {result.Count} rewrite suggestion(s) for query: {query.QueryId}");
        return await Task.FromResult(result);
    }

    /// <inheritdoc/>
    public async Task<List<IndexSuggestion>> GenerateIndexRecommendationsAsync(
        DatabaseQuery query,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Generating index recommendations for query: {query.QueryId}");
        cancellationToken.ThrowIfCancellationRequested();

        var suggestions = new List<IndexSuggestion>();

        try
        {
            suggestions.AddRange(RecommendWhereClauseIndexes(query));
            suggestions.AddRange(RecommendJoinColumnIndexes(query));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error generating index recommendations for query: {query.QueryId}");
            throw;
        }

        _logger.LogInformation($"Generated {suggestions.Count} index recommendation(s) for query: {query.QueryId}");
        return await Task.FromResult(suggestions);
    }

    // ──────────────────────────── rewrite helpers ────────────────────────────

    private QueryRewriteSuggestion? SuggestExplicitColumns(DatabaseQuery query)
    {
        if (!query.QueryText.Contains("SELECT *", StringComparison.OrdinalIgnoreCase))
            return null;

        return new QueryRewriteSuggestion
        {
            OriginalQuery = query.QueryText,
            RewrittenQuery = query.QueryText.Replace(
                "SELECT *", "SELECT col1, col2 /* specify required columns */",
                StringComparison.OrdinalIgnoreCase),
            RewriteType = RewriteType.ExplicitColumnSelection,
            AffectedClause = "SELECT",
            Rationale = "SELECT * retrieves all columns, increasing I/O and preventing covering-index usage.",
            AdditionalNotes = "Replace the placeholder column list with the columns the application actually needs.",
            EstimatedImprovementPercent = 15.0,
            IsBreakingChange = false,
            IsAutoApplicable = false,
            Priority = 3
        };
    }

    private QueryRewriteSuggestion? SuggestOrToUnionAll(DatabaseQuery query)
    {
        var conditions = string.Join(" ", query.WhereConditions);
        if (!conditions.Contains(" OR ", StringComparison.OrdinalIgnoreCase))
            return null;

        var table = query.ReferencedTables.FirstOrDefault() ?? "YourTable";
        var rewrite =
            $"-- Each branch can now use its own index seek\n" +
            $"SELECT * FROM {table} WHERE <first_condition>\n" +
            $"UNION ALL\n" +
            $"SELECT * FROM {table} WHERE <second_condition>;";

        return new QueryRewriteSuggestion
        {
            OriginalQuery = query.QueryText,
            RewrittenQuery = rewrite,
            RewriteType = RewriteType.OrToUnionAll,
            AffectedClause = "WHERE",
            Rationale = "OR predicates on separate indexed columns force a scan; UNION ALL lets each branch use its own index seek.",
            AdditionalNotes = "Only safe when the two branches return mutually exclusive rows; add a deduplication step otherwise.",
            EstimatedImprovementPercent = 30.0,
            IsBreakingChange = false,
            IsAutoApplicable = false,
            Priority = 2
        };
    }

    private QueryRewriteSuggestion? SuggestSubqueryToJoin(DatabaseQuery query)
    {
        var hasInSubquery = Regex.IsMatch(
            query.QueryText, @"\bIN\s*\(\s*SELECT\b", RegexOptions.IgnoreCase);

        if (!hasInSubquery)
            return null;

        return new QueryRewriteSuggestion
        {
            OriginalQuery = query.QueryText,
            RewrittenQuery =
                "-- JOIN gives the optimizer better cardinality estimates\n" +
                "SELECT DISTINCT outer.*\n" +
                "FROM OuterTable outer\n" +
                "INNER JOIN InnerTable inner ON outer.Id = inner.OuterId\n" +
                "WHERE <additional predicates>;",
            RewriteType = RewriteType.SubqueryToJoin,
            AffectedClause = "WHERE",
            Rationale = "IN (SELECT ...) prevents the optimizer from choosing optimal join strategies; an explicit JOIN exposes cardinality to the planner.",
            AdditionalNotes = "Add DISTINCT if the join can produce duplicates. Validate NULL semantics when the subquery may return NULLs.",
            EstimatedImprovementPercent = 25.0,
            IsBreakingChange = false,
            IsAutoApplicable = false,
            Priority = 2
        };
    }

    private QueryRewriteSuggestion? SuggestNotInToNotExists(DatabaseQuery query)
    {
        if (!Regex.IsMatch(query.QueryText, @"\bNOT\s+IN\s*\(\s*SELECT\b", RegexOptions.IgnoreCase))
            return null;

        return new QueryRewriteSuggestion
        {
            OriginalQuery = query.QueryText,
            RewrittenQuery =
                "-- NOT EXISTS handles NULLs correctly and typically yields a better plan\n" +
                "SELECT outer.*\n" +
                "FROM OuterTable outer\n" +
                "WHERE NOT EXISTS (\n" +
                "    SELECT 1 FROM InnerTable inner\n" +
                "    WHERE inner.OuterId = outer.Id\n" +
                ");",
            RewriteType = RewriteType.NotInToNotExists,
            AffectedClause = "WHERE",
            Rationale = "NOT IN returns zero rows when the subquery contains any NULL; NOT EXISTS is NULL-safe and commonly produces a more efficient plan.",
            AdditionalNotes = "Ensure the correlated join key on the inner table is indexed.",
            EstimatedImprovementPercent = 20.0,
            IsBreakingChange = false,
            IsAutoApplicable = false,
            Priority = 2
        };
    }

    private QueryRewriteSuggestion? SuggestFunctionSargability(DatabaseQuery query)
    {
        var functionPattern = @"(UPPER|LOWER|CONVERT|CAST|DATEPART|YEAR|MONTH|DAY|LTRIM|RTRIM|ISNULL)\s*\(";
        if (!Regex.IsMatch(query.QueryText, functionPattern, RegexOptions.IgnoreCase))
            return null;

        return new QueryRewriteSuggestion
        {
            OriginalQuery = query.QueryText,
            RewrittenQuery =
                "-- Move the function to the right-hand side to allow index seeks\n" +
                "-- Before: WHERE UPPER(col) = 'VALUE'\n" +
                "-- After:  WHERE col = 'value'   -- or use a case-insensitive collation\n\n" +
                "-- If the transformation is not possible, index a computed column:\n" +
                "-- ALTER TABLE T ADD ColUpper AS UPPER(col) PERSISTED;\n" +
                "-- CREATE INDEX IX_T_ColUpper ON T (ColUpper);",
            RewriteType = RewriteType.FunctionSargability,
            AffectedClause = "WHERE",
            Rationale = "Wrapping an indexed column in a scalar function forces a full scan; restoring SARGability enables an index seek.",
            AdditionalNotes = "A persisted computed column with a matching index is the fallback when the function cannot be moved.",
            EstimatedImprovementPercent = 35.0,
            IsBreakingChange = false,
            IsAutoApplicable = false,
            Priority = 1
        };
    }

    private QueryRewriteSuggestion? SuggestUnionToUnionAll(DatabaseQuery query)
    {
        if (!Regex.IsMatch(query.QueryText, @"\bUNION\b(?!\s+ALL)", RegexOptions.IgnoreCase))
            return null;

        return new QueryRewriteSuggestion
        {
            OriginalQuery = query.QueryText,
            RewrittenQuery = Regex.Replace(
                query.QueryText,
                @"\bUNION\b(?!\s+ALL)",
                "UNION ALL",
                RegexOptions.IgnoreCase),
            RewriteType = RewriteType.UnionToUnionAll,
            AffectedClause = "UNION",
            Rationale = "UNION performs an implicit DISTINCT sort across the combined result; UNION ALL skips that sort when duplicates are acceptable.",
            AdditionalNotes = "Only substitute UNION ALL when the query branches are guaranteed to produce non-overlapping rows or when the caller tolerates duplicates.",
            EstimatedImprovementPercent = 20.0,
            IsBreakingChange = true,
            IsAutoApplicable = false,
            Priority = 3
        };
    }

    private QueryRewriteSuggestion? SuggestPagination(DatabaseQuery query)
    {
        var hasRowLimit =
            Regex.IsMatch(query.QueryText, @"\bTOP\s+\d+\b", RegexOptions.IgnoreCase) ||
            Regex.IsMatch(query.QueryText, @"\bFETCH\s+NEXT\b", RegexOptions.IgnoreCase) ||
            query.QueryText.Contains("LIMIT", StringComparison.OrdinalIgnoreCase);

        if (hasRowLimit)
            return null;

        if (!query.QueryText.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            return null;

        return new QueryRewriteSuggestion
        {
            OriginalQuery = query.QueryText,
            RewrittenQuery =
                "-- SQL Server / Azure SQL\n" +
                "SELECT col1, col2 FROM YourTable\n" +
                "ORDER BY Id\n" +
                "OFFSET @page * @pageSize ROWS\n" +
                "FETCH NEXT @pageSize ROWS ONLY;\n\n" +
                "-- PostgreSQL / MySQL\n" +
                "SELECT col1, col2 FROM your_table\n" +
                "ORDER BY id\n" +
                "LIMIT @pageSize OFFSET @page * @pageSize;",
            RewriteType = RewriteType.ResultSetPagination,
            AffectedClause = "SELECT",
            Rationale = "Unbounded result sets consume excessive memory and network bandwidth; pagination constrains resource usage and improves response latency.",
            AdditionalNotes = "Pagination requires a deterministic ORDER BY clause. Ensure the sort column has a supporting index.",
            EstimatedImprovementPercent = 40.0,
            IsBreakingChange = true,
            IsAutoApplicable = false,
            Priority = 2
        };
    }

    // ──────────────────────────── index helpers ───────────────────────────────

    private List<IndexSuggestion> RecommendWhereClauseIndexes(DatabaseQuery query)
    {
        var suggestions = new List<IndexSuggestion>();
        if (query.WhereConditions.Count == 0 || query.ReferencedTables.Count == 0)
            return suggestions;

        var whereText = string.Join(" ", query.WhereConditions);
        var columnPattern = @"(\b\w+\b)\s*(?:=|>|<|>=|<=|LIKE|IN)\s*";
        var columns = Regex.Matches(whereText, columnPattern, RegexOptions.IgnoreCase)
            .Select(m => m.Groups[1].Value)
            .Where(c => c.Length > 1)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (columns.Count == 0)
            return suggestions;

        foreach (var table in query.ReferencedTables)
        {
            var suggestion = new IndexSuggestion
            {
                TableName = table,
                IndexColumns = columns.Take(2).ToList(),
                IncludeColumns = columns.Skip(2).Take(3).ToList(),
                IndexType = "NONCLUSTERED",
                EstimatedPerformanceGain = Math.Min(50.0, 30.0 + columns.Count * 4),
                EstimatedExecutionTimeReduction = 25.0,
                Rationale = "Covering index on WHERE-clause columns to enable index seeks and avoid key lookups.",
                AffectedQueries = 1
            };
            suggestion.GenerateIndexName();
            suggestion.GenerateCreateScript();
            suggestion.GenerateDropScript();
            suggestions.Add(suggestion);
        }

        return suggestions;
    }

    private List<IndexSuggestion> RecommendJoinColumnIndexes(DatabaseQuery query)
    {
        var suggestions = new List<IndexSuggestion>();
        if (query.JoinConditions.Count == 0)
            return suggestions;

        var joinPattern = @"(\w+)\.(\w+)\s*=\s*(\w+)\.(\w+)";

        foreach (var condition in query.JoinConditions)
        {
            var match = Regex.Match(condition, joinPattern, RegexOptions.IgnoreCase);
            if (!match.Success) continue;

            var pairs = new[]
            {
                (table: match.Groups[1].Value, column: match.Groups[2].Value),
                (table: match.Groups[3].Value, column: match.Groups[4].Value)
            };

            foreach (var (table, column) in pairs)
            {
                if (!query.ReferencedTables.Any(t => t.Equals(table, StringComparison.OrdinalIgnoreCase)))
                    continue;

                var suggestion = new IndexSuggestion
                {
                    TableName = table,
                    IndexColumns = [column],
                    IndexType = "NONCLUSTERED",
                    EstimatedPerformanceGain = 25.0,
                    EstimatedExecutionTimeReduction = 20.0,
                    Rationale = $"Index on join column {table}.{column} to avoid nested-loop scans.",
                    AffectedQueries = 1
                };
                suggestion.GenerateIndexName();
                suggestion.GenerateCreateScript();
                suggestion.GenerateDropScript();
                suggestions.Add(suggestion);
            }
        }

        return suggestions;
    }
}
