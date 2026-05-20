#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Configuration;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Services;

/// <summary>
/// Analyzes SQL text and produces index recommendations.
/// </summary>
public interface IIndexRecommendationEngine
{
    /// <summary>
    /// Analyzes a SQL query and recommends indexes based on WHERE, JOIN, ORDER BY, GROUP BY clauses.
    /// </summary>
    Task<List<IndexRecommendation>> RecommendAsync(string queryText);

    /// <summary>
    /// Scores competing index recommendations and returns a ranked list.
    /// </summary>
    List<IndexRecommendation> RankRecommendations(List<IndexRecommendation> recommendations);

    /// <summary>
    /// Detects redundant or overlapping index candidates.
    /// </summary>
    List<string> DetectRedundancies(List<IndexRecommendation> recommendations);
}

/// <summary>
/// Default implementation of <see cref="IIndexRecommendationEngine"/>.
/// </summary>
public sealed partial class IndexRecommendationEngine : IIndexRecommendationEngine
{
    private readonly ILogger<IndexRecommendationEngine> _logger;
    private readonly AnalyzerSettings _settings;

    /// <summary>
    /// Initializes the engine with default analyzer settings.
    /// </summary>
    public IndexRecommendationEngine(ILogger<IndexRecommendationEngine> logger)
        : this(logger, AnalyzerSettingsFactory.CreateDefault())
    {
    }

    /// <summary>
    /// Initializes the engine with explicit analyzer settings.
    /// </summary>
    public IndexRecommendationEngine(ILogger<IndexRecommendationEngine> logger, AnalyzerSettings settings)
    {
        _logger = logger;
        _settings = settings;
    }

    /// <inheritdoc/>
    public Task<List<IndexRecommendation>> RecommendAsync(string queryText)
    {
        if (string.IsNullOrWhiteSpace(queryText))
            return Task.FromResult(new List<IndexRecommendation>());

        var aliases = ExtractAliases(queryText);
        _logger.LogDebug("Using {Count} table alias mappings", aliases.Count);
        var tableColumns = aliases.Values.Distinct(StringComparer.OrdinalIgnoreCase)
            .ToDictionary(t => t, _ => new HashSet<string>(StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);

        var recommendations = new List<IndexRecommendation>();
        recommendations.AddRange(BuildSingleColumnRecommendations(ExtractPredicateColumns(ExtractClause(queryText, WhereClauseRegex()), aliases), RecommendationSource.WhereClause, 70, "WHERE predicate"));
        recommendations.AddRange(BuildSingleColumnRecommendations(ExtractJoinColumns(queryText, aliases), RecommendationSource.JoinCondition, 65, "JOIN condition"));
        recommendations.AddRange(BuildSingleColumnRecommendations(ExtractListClauseColumns(ExtractClause(queryText, OrderByClauseRegex()), aliases), RecommendationSource.OrderBy, 50, "ORDER BY clause"));
        recommendations.AddRange(BuildSingleColumnRecommendations(ExtractListClauseColumns(ExtractClause(queryText, GroupByClauseRegex()), aliases), RecommendationSource.GroupBy, 45, "GROUP BY clause"));

        foreach (var recommendation in recommendations)
        {
            if (!tableColumns.TryGetValue(recommendation.TableName, out var columns))
            {
                columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                tableColumns[recommendation.TableName] = columns;
            }

            foreach (var column in recommendation.KeyColumns)
                columns.Add(column);
        }

        foreach (var entry in tableColumns.Where(e => e.Value.Count > 1))
        {
            var composite = new IndexRecommendation
            {
                TableName = entry.Key,
                KeyColumns = entry.Value.ToList(),
                ImpactScore = Math.Min(95, 75 + (entry.Value.Count * 5)),
                Source = RecommendationSource.Composite,
                Rationale = $"Composite index for {entry.Key} combines multiple predicates and sort/group keys detected in the query."
            };
            composite.GenerateScript();
            recommendations.Add(composite);
        }

        var ranked = RankRecommendations(recommendations);
        var redundancies = DetectRedundancies(ranked);
        if (redundancies.Count > 0)
            _logger.LogInformation("Detected {Count} overlapping index recommendation(s)", redundancies.Count);

        _logger.LogInformation("Generated {Count} index recommendation(s)", ranked.Count);
        return Task.FromResult(ranked);
    }

    /// <inheritdoc/>
    public List<IndexRecommendation> RankRecommendations(List<IndexRecommendation> recommendations)
    {
        return recommendations
            .GroupBy(r => $"{r.TableName}|{string.Join(",", r.KeyColumns)}|{r.Source}", StringComparer.OrdinalIgnoreCase)
            .Select(g => g.OrderByDescending(r => r.ImpactScore).First())
            .OrderByDescending(r => r.ImpactScore)
            .ThenByDescending(r => r.Source == RecommendationSource.Composite)
            .ThenByDescending(r => r.KeyColumns.Count)
            .ToList();
    }

    /// <inheritdoc/>
    public List<string> DetectRedundancies(List<IndexRecommendation> recommendations)
    {
        var results = new List<string>();

        foreach (var group in recommendations.GroupBy(r => r.TableName, StringComparer.OrdinalIgnoreCase))
        {
            var items = group.ToList();
            for (var i = 0; i < items.Count; i++)
            {
                for (var j = i + 1; j < items.Count; j++)
                {
                    if (IsPrefix(items[i].KeyColumns, items[j].KeyColumns) || IsPrefix(items[j].KeyColumns, items[i].KeyColumns))
                    {
                        results.Add($"{group.Key}: overlapping candidates [{string.Join(", ", items[i].KeyColumns)}] and [{string.Join(", ", items[j].KeyColumns)}]");
                    }
                }
            }
        }

        return results.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private List<IndexRecommendation> BuildSingleColumnRecommendations(List<(string Table, string Column)> columns, RecommendationSource source, double score, string rationalePrefix)
    {
        return columns
            .Where(c => !string.IsNullOrWhiteSpace(c.Table) && !string.IsNullOrWhiteSpace(c.Column))
            .Distinct()
            .Select(c =>
            {
                var recommendation = new IndexRecommendation
                {
                    TableName = c.Table,
                    KeyColumns = [c.Column],
                    ImpactScore = score,
                    Source = source,
                    Rationale = $"Index on {c.Table}({c.Column}) improves the detected {rationalePrefix}."
                };
                recommendation.GenerateScript();
                return recommendation;
            })
            .ToList();
    }

    private static Dictionary<string, string> ExtractAliases(string queryText)
    {
        var aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (Match match in TableAliasRegex().Matches(queryText))
        {
            var table = match.Groups["table"].Value;
            var alias = match.Groups["alias"].Success ? match.Groups["alias"].Value : table;
            if (IsClauseKeyword(alias))
                alias = table;

            if (!aliases.ContainsKey(alias))
                aliases[alias] = table;
            if (!aliases.ContainsKey(table))
                aliases[table] = table;
        }

        return aliases;
    }

    private static List<(string Table, string Column)> ExtractPredicateColumns(string clause, Dictionary<string, string> aliases)
    {
        var results = new List<(string Table, string Column)>();
        foreach (Match match in PredicateColumnRegex().Matches(clause))
        {
            var table = ResolveTable(match.Groups["alias"].Value, aliases);
            var column = match.Groups["column"].Value;
            if (!string.IsNullOrWhiteSpace(table))
                results.Add((table, column));
        }

        return results;
    }

    private static List<(string Table, string Column)> ExtractJoinColumns(string queryText, Dictionary<string, string> aliases)
    {
        var results = new List<(string Table, string Column)>();
        foreach (Match joinMatch in JoinClauseRegex().Matches(queryText))
        {
            var clause = joinMatch.Groups["condition"].Value;
            foreach (Match columnMatch in JoinPairRegex().Matches(clause))
            {
                var leftTable = ResolveTable(columnMatch.Groups["leftAlias"].Value, aliases);
                var rightTable = ResolveTable(columnMatch.Groups["rightAlias"].Value, aliases);
                if (!string.IsNullOrWhiteSpace(leftTable))
                    results.Add((leftTable, columnMatch.Groups["leftColumn"].Value));
                if (!string.IsNullOrWhiteSpace(rightTable))
                    results.Add((rightTable, columnMatch.Groups["rightColumn"].Value));
            }
        }

        return results;
    }

    private static List<(string Table, string Column)> ExtractListClauseColumns(string clause, Dictionary<string, string> aliases)
    {
        var results = new List<(string Table, string Column)>();
        foreach (var part in clause.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var cleaned = SortDirectionRegex().Replace(part, string.Empty).Trim();
            var match = QualifiedColumnRegex().Match(cleaned);
            if (!match.Success)
                continue;

            var table = ResolveTable(match.Groups["alias"].Value, aliases);
            if (!string.IsNullOrWhiteSpace(table))
                results.Add((table, match.Groups["column"].Value));
        }

        return results;
    }

    private static string ExtractClause(string queryText, Regex regex)
    {
        var match = regex.Match(queryText);
        return match.Success ? match.Groups["clause"].Value : string.Empty;
    }

    private static string ResolveTable(string alias, Dictionary<string, string> aliases)
    {
        if (aliases.TryGetValue(alias, out var table))
            return table;

        return aliases.Count == 1 ? aliases.Values.First() : string.Empty;
    }

    private static bool IsPrefix(List<string> left, List<string> right)
    {
        if (left.Count >= right.Count)
            return false;

        for (var i = 0; i < left.Count; i++)
        {
            if (!string.Equals(left[i], right[i], StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }

    private static bool IsClauseKeyword(string value) =>
        value.Equals("WHERE", StringComparison.OrdinalIgnoreCase) ||
        value.Equals("JOIN", StringComparison.OrdinalIgnoreCase) ||
        value.Equals("ON", StringComparison.OrdinalIgnoreCase) ||
        value.Equals("ORDER", StringComparison.OrdinalIgnoreCase) ||
        value.Equals("GROUP", StringComparison.OrdinalIgnoreCase) ||
        value.Equals("HAVING", StringComparison.OrdinalIgnoreCase) ||
        value.Equals("LIMIT", StringComparison.OrdinalIgnoreCase);

    [GeneratedRegex(@"\b(?:FROM|JOIN)\s+(?<table>\w+)(?:\s+(?:AS\s+)?(?<alias>\w+))?", RegexOptions.IgnoreCase)]
    private static partial Regex TableAliasRegex();

    [GeneratedRegex(@"\bWHERE\s+(?<clause>.+?)(?=\bGROUP\s+BY\b|\bORDER\s+BY\b|\bHAVING\b|\bLIMIT\b|$)", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex WhereClauseRegex();

    [GeneratedRegex(@"\bORDER\s+BY\s+(?<clause>.+?)(?=\bLIMIT\b|$)", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex OrderByClauseRegex();

    [GeneratedRegex(@"\bGROUP\s+BY\s+(?<clause>.+?)(?=\bHAVING\b|\bORDER\s+BY\b|\bLIMIT\b|$)", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex GroupByClauseRegex();

    [GeneratedRegex(@"(?:=|<>|!=|>=|<=|>|<|\bLIKE\b|\bIN\b|\bBETWEEN\b|\bIS\b)", RegexOptions.IgnoreCase)]
    private static partial Regex PredicateOperatorRegex();

    [GeneratedRegex(@"(?:(?<alias>\w+)\.)?(?<column>\w+)\s*(?:=|<>|!=|>=|<=|>|<|\bLIKE\b|\bIN\b|\bBETWEEN\b|\bIS\b)", RegexOptions.IgnoreCase)]
    private static partial Regex PredicateColumnRegex();

    [GeneratedRegex(@"\bJOIN\b.+?\bON\s+(?<condition>.+?)(?=\bJOIN\b|\bWHERE\b|\bGROUP\s+BY\b|\bORDER\s+BY\b|\bHAVING\b|$)", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex JoinClauseRegex();

    [GeneratedRegex(@"(?:(?<leftAlias>\w+)\.)?(?<leftColumn>\w+)\s*=\s*(?:(?<rightAlias>\w+)\.)?(?<rightColumn>\w+)", RegexOptions.IgnoreCase)]
    private static partial Regex JoinPairRegex();

    [GeneratedRegex(@"(?:(?<alias>\w+)\.)?(?<column>\w+)", RegexOptions.IgnoreCase)]
    private static partial Regex QualifiedColumnRegex();

    [GeneratedRegex(@"\s+(ASC|DESC)\b", RegexOptions.IgnoreCase)]
    private static partial Regex SortDirectionRegex();
}
