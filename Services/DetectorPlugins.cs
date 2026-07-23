#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SqlQueryAnalyzer.Configuration;
using SqlQueryAnalyzer.Constants;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Services;

/// <summary>
/// Detects <c>SELECT *</c> usage, which may retrieve unnecessary columns and increase
/// I/O, memory, and network overhead.
/// </summary>
public sealed class SelectStarDetectorPlugin : IDetectorPlugin
{
    /// <inheritdoc />
    public string RuleId => "select-star";

    /// <summary>
    /// Analyzes the query text for a literal <c>SELECT *</c> pattern.
    /// </summary>
    /// <param name="query">The query to analyze.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="query"/> is null.</exception>
    public IEnumerable<PerformanceIssue> Analyze(DatabaseQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (!query.QueryText.Contains("SELECT *", StringComparison.OrdinalIgnoreCase))
            yield break;

        yield return new PerformanceIssue
        {
            IssueType = IssueType.SelectStar,
            Severity = IssueSeverity.Info,
            Description = "SELECT * used - may retrieve unnecessary columns",
            AffectedClause = "SELECT",
            EstimatedPerformanceImpact = 10.0,
            RecommendedFix = "Specify only required columns",
            ExampleFix = "SELECT col1, col2, col3 FROM table...",
            Priority = 3
        };
    }
}

/// <summary>
/// Detects destructive statements (<c>DELETE</c>/<c>UPDATE</c>) missing a <c>WHERE</c>
/// clause, and <c>SELECT</c> statements missing any bound on the returned result set
/// (no <c>WHERE</c>, <c>TOP</c>, or <c>LIMIT</c>).
/// </summary>
public sealed class MissingWhereOrLimitDetectorPlugin : IDetectorPlugin
{
    /// <inheritdoc />
    public string RuleId => "missing-where-limit";

    /// <summary>
    /// Analyzes the query for missing filtering/bounding clauses.
    /// </summary>
    /// <param name="query">The query to analyze.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="query"/> is null.</exception>
    public IEnumerable<PerformanceIssue> Analyze(DatabaseQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var text = query.QueryText;
        var hasWhere = text.Contains("WHERE", StringComparison.OrdinalIgnoreCase);

        var isDestructive = Regex.IsMatch(text, @"^\s*(DELETE|UPDATE)\b", RegexOptions.IgnoreCase);
        if (isDestructive && !hasWhere)
        {
            yield return new PerformanceIssue
            {
                IssueType = IssueType.TableScan,
                Severity = IssueSeverity.Critical,
                Description = "DELETE/UPDATE without WHERE clause detected - would affect all rows",
                AffectedClause = "WHERE",
                EstimatedPerformanceImpact = 100.0,
                RecommendedFix = "Add a WHERE clause to scope the affected rows",
                Priority = 1
            };
            yield break;
        }

        var isSelect = Regex.IsMatch(text, @"^\s*SELECT\b", RegexOptions.IgnoreCase);
        var hasBound = hasWhere
            || Regex.IsMatch(text, @"\bTOP\s+\d+\b", RegexOptions.IgnoreCase)
            || Regex.IsMatch(text, @"\bLIMIT\s+\d+\b", RegexOptions.IgnoreCase)
            || Regex.IsMatch(text, @"\bFETCH\s+NEXT\s+\d+\s+ROWS\s+ONLY\b", RegexOptions.IgnoreCase);

        if (isSelect && !hasBound)
        {
            yield return new PerformanceIssue
            {
                IssueType = IssueType.TableScan,
                Severity = IssueSeverity.Warning,
                Description = "SELECT without WHERE, TOP, or LIMIT detected - may scan and return the entire table",
                AffectedClause = "WHERE",
                EstimatedPerformanceImpact = 40.0,
                RecommendedFix = "Add a WHERE clause or bound the result set with TOP/LIMIT",
                Priority = 2
            };
        }
    }
}

/// <summary>
/// Detects Cartesian products caused by missing or incomplete join conditions relative
/// to the number of tables referenced by the query.
/// </summary>
public sealed class CartesianJoinDetectorPlugin : IDetectorPlugin
{
    /// <inheritdoc />
    public string RuleId => "cartesian-join";

    /// <summary>
    /// Analyzes the join conditions against the referenced table count.
    /// </summary>
    /// <param name="query">The query to analyze.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="query"/> is null.</exception>
    public IEnumerable<PerformanceIssue> Analyze(DatabaseQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.JoinConditions.Count < Math.Max(1, query.ReferencedTables.Count - 1))
        {
            yield return new PerformanceIssue
            {
                IssueType = IssueType.CrossJoin,
                Severity = IssueSeverity.Critical,
                Description = "Potential Cartesian product - missing or incomplete join condition",
                EstimatedPerformanceImpact = 90.0,
                RecommendedFix = "Add proper join conditions between all related tables",
                Priority = 1
            };
        }
    }
}

/// <summary>
/// Detects join conditions and flags cases worth a manual type-compatibility check.
/// </summary>
public sealed partial class JoinColumnTypeDetectorPlugin : IDetectorPlugin
{
    [GeneratedRegex(@"(\w+)\s*=\s*(\w+)")]
    private static partial Regex JoinColumnRegex();

    /// <inheritdoc />
    public string RuleId => "join-column-type";

    /// <summary>
    /// Analyzes join conditions for column-to-column comparisons.
    /// </summary>
    /// <param name="query">The query to analyze.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="query"/> is null.</exception>
    public IEnumerable<PerformanceIssue> Analyze(DatabaseQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.JoinConditions.Count == 0)
            yield break;

        var matches = JoinColumnRegex().Matches(string.Join(" ", query.JoinConditions));
        if (matches.Count == 0)
            yield break;

        yield return new PerformanceIssue
        {
            IssueType = IssueType.IneffectiveJoin,
            Severity = IssueSeverity.Warning,
            Description = $"Found {matches.Count} join conditions - verify types match for optimal performance",
            EstimatedPerformanceImpact = 15.0,
            RecommendedFix = "Ensure joined columns have compatible data types",
            Priority = 2
        };
    }
}

/// <summary>
/// Detects non-sargable predicates: functions applied to columns, leading-wildcard
/// <c>LIKE</c> patterns, and <c>OR</c> conditions, all of which can prevent index usage.
/// </summary>
public sealed partial class NonSargablePredicateDetectorPlugin : IDetectorPlugin
{
    private readonly IndexSeverityThresholds _indexSeverity;

    [GeneratedRegex(@"(UPPER|LOWER|CONVERT|CAST|DATEPART|YEAR|MONTH|DAY)\s*\(", RegexOptions.IgnoreCase)]
    private static partial Regex FunctionOnColumnRegex();

    /// <summary>
    /// Initializes the plugin with the severity thresholds used to grade OR/wildcard findings.
    /// </summary>
    /// <param name="indexSeverity">Thresholds controlling how findings escalate in severity.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="indexSeverity"/> is null.</exception>
    public NonSargablePredicateDetectorPlugin(IndexSeverityThresholds indexSeverity)
    {
        ArgumentNullException.ThrowIfNull(indexSeverity);
        _indexSeverity = indexSeverity;
    }

    /// <inheritdoc />
    public string RuleId => "non-sargable-predicate";

    /// <summary>
    /// Analyzes the query text and WHERE conditions for non-sargable patterns.
    /// </summary>
    /// <param name="query">The query to analyze.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="query"/> is null.</exception>
    public IEnumerable<PerformanceIssue> Analyze(DatabaseQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var functionMatches = FunctionOnColumnRegex().Matches(query.QueryText);
        if (functionMatches.Count > 0)
        {
            yield return new PerformanceIssue
            {
                IssueType = IssueType.FunctionOnColumn,
                Severity = IssueSeverity.Warning,
                Description = $"Found {functionMatches.Count} function(s) applied to columns - may prevent index usage",
                AffectedClause = "WHERE",
                EstimatedPerformanceImpact = 25.0,
                RecommendedFix = "Move functions to the right side of comparison or use computed columns with indexes",
                Priority = 2
            };
        }

        if (query.WhereConditions.Count == 0)
            yield break;

        var conditions = string.Join(" ", query.WhereConditions);

        if (conditions.Contains(" OR ", StringComparison.OrdinalIgnoreCase))
        {
            var orSeverity = _indexSeverity.ResolveSeverity(estimatedCost: 20.0);
            yield return new PerformanceIssue
            {
                IssueType = IssueType.OrCondition,
                Severity = orSeverity,
                Description = "OR condition in WHERE clause may prevent index usage",
                AffectedClause = "WHERE",
                EstimatedPerformanceImpact = 20.0,
                RecommendedFix = "Consider using UNION ALL instead of OR, or create appropriate indexes",
                ExampleFix = "SELECT * FROM Table1 WHERE col1 = 'A' UNION ALL SELECT * FROM Table1 WHERE col2 = 'B'",
                Priority = 2
            };
        }

        if (conditions.Contains("LIKE '%", StringComparison.OrdinalIgnoreCase))
        {
            var wildcardSeverity = _indexSeverity.ResolveSeverity(estimatedCost: 30.0);
            yield return new PerformanceIssue
            {
                IssueType = IssueType.LeadingWildcard,
                Severity = wildcardSeverity,
                Description = "LIKE with leading wildcard prevents index usage",
                AffectedClause = "WHERE",
                EstimatedPerformanceImpact = 30.0,
                RecommendedFix = "Use full-text search or CONTAINS instead of LIKE with leading wildcard",
                Priority = 2
            };
        }
    }
}

/// <summary>
/// Flags equality comparisons that may involve an implicit type conversion between
/// mismatched column types.
/// </summary>
public sealed class ImplicitConversionDetectorPlugin : IDetectorPlugin
{
    /// <inheritdoc />
    public string RuleId => "implicit-conversion";

    /// <summary>
    /// Analyzes the query text for equality comparisons.
    /// </summary>
    /// <param name="query">The query to analyze.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="query"/> is null.</exception>
    public IEnumerable<PerformanceIssue> Analyze(DatabaseQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (!query.QueryText.Contains('='))
            yield break;

        yield return new PerformanceIssue
        {
            IssueType = IssueType.ImplicitConversion,
            Severity = IssueSeverity.Info,
            Description = "Potential implicit type conversion in comparison - verify column types",
            EstimatedPerformanceImpact = 5.0,
            RecommendedFix = "Ensure compared values have matching data types",
            Priority = 3
        };
    }
}
