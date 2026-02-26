#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Constants;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Services;

/// <summary>
/// Detects performance issues in SQL queries.
/// Regex fields are source-generated ([GeneratedRegex]) — no runtime compilation.
/// Private detection helpers are synchronous; only the public entry points return
/// ValueTask/Task to match the interface contract.
/// </summary>
public partial class PerformanceIssueDetectorService : IPerformanceIssueDetectorService
{
    private readonly ILogger<PerformanceIssueDetectorService> _logger;

    public PerformanceIssueDetectorService(ILogger<PerformanceIssueDetectorService> logger)
    {
        _logger = logger;
    }

    // ── Source-generated regexes ─────────────────────────────────────────────
    // Previously these were created fresh inside each method call.

    [GeneratedRegex(@"(\w+)\s*=\s*(\w+)")]
    private static partial Regex JoinColumnRegex();

    [GeneratedRegex(@"(UPPER|LOWER|CONVERT|CAST|DATEPART|YEAR|MONTH|DAY)\s*\(", RegexOptions.IgnoreCase)]
    private static partial Regex FunctionOnColumnRegex();

    [GeneratedRegex(@"LIKE\s+'%", RegexOptions.IgnoreCase)]
    private static partial Regex LeadingWildcardRegex();

    // ── Public interface ─────────────────────────────────────────────────────

    public async Task<List<PerformanceIssue>> DetectIssuesAsync(DatabaseQuery query)
    {
        _logger.LogInformation($"Detecting issues in query: {query.QueryId}");

        var issues = new List<PerformanceIssue>();

        // Private helpers are synchronous — no awaiting overhead for in-process work.
        issues.AddRange(DetectSelectStarIssues(query));
        issues.AddRange(await DetectJoinIssuesAsync(query));
        issues.AddRange(DetectLeadingWildcardIssues(query));
        issues.AddRange(DetectFunctionOnColumnIssues(query));
        issues.AddRange(await DetectIndexOpportunitiesAsync(query));
        issues.AddRange(DetectImplicitConversionIssues(query));

        issues = issues.OrderByDescending(i => i.Severity)
                       .ThenByDescending(i => i.EstimatedPerformanceImpact)
                       .ToList();

        _logger.LogInformation($"Found {issues.Count} performance issues");

        return issues;
    }

    public ValueTask<List<PerformanceIssue>> DetectNPlusOneAsync(List<DatabaseQuery> queries)
    {
        if (queries == null)
            throw new ArgumentNullException(nameof(queries), "The query collection provided for N+1 detection must not be null.");

        _logger.LogInformation($"Detecting N+1 query patterns in {queries.Count} queries");

        var issues = new List<PerformanceIssue>();

        var tableGroups = queries
            .Where(q => q.ReferencedTables.Count > 0)
            .GroupBy(q => q.ReferencedTables.FirstOrDefault() ?? string.Empty)
            .ToList();

        foreach (var group in tableGroups)
        {
            if (group.Count() > 10)
            {
                issues.Add(new PerformanceIssue
                {
                    IssueType = IssueType.NPlusOne,
                    Severity = IssueSeverity.Critical,
                    Description = $"Potential N+1 pattern: {group.Count()} queries accessing {group.Key}",
                    EstimatedPerformanceImpact = 50.0,
                    RecommendedFix = "Use JOIN or batch queries instead of loop-based queries",
                    Priority = 1
                });
            }
        }

        return ValueTask.FromResult(issues);
    }

    public ValueTask<List<PerformanceIssue>> DetectJoinIssuesAsync(DatabaseQuery query)
    {
        _logger.LogInformation("Detecting join-related issues");

        var issues = new List<PerformanceIssue>();

        if (query.JoinConditions.Count == 0)
            return ValueTask.FromResult(issues);

        var matches = JoinColumnRegex().Matches(string.Join(" ", query.JoinConditions));

        if (matches.Count > 0)
        {
            issues.Add(new PerformanceIssue
            {
                IssueType = IssueType.IneffectiveJoin,
                Severity = IssueSeverity.Warning,
                Description = $"Found {matches.Count} join conditions - verify types match for optimal performance",
                EstimatedPerformanceImpact = 15.0,
                RecommendedFix = "Ensure joined columns have compatible data types",
                Priority = 2
            });
        }

        if (query.JoinConditions.Count < Math.Max(1, query.ReferencedTables.Count - 1))
        {
            issues.Add(new PerformanceIssue
            {
                IssueType = IssueType.CrossJoin,
                Severity = IssueSeverity.Critical,
                Description = "Potential Cartesian product - missing or incomplete join condition",
                EstimatedPerformanceImpact = 90.0,
                RecommendedFix = "Add proper join conditions between all related tables",
                Priority = 1
            });
        }

        return ValueTask.FromResult(issues);
    }

    public ValueTask<List<PerformanceIssue>> DetectIndexOpportunitiesAsync(DatabaseQuery query)
    {
        _logger.LogInformation("Analyzing index opportunities");

        var issues = new List<PerformanceIssue>();

        if (query.WhereConditions.Count == 0)
            return ValueTask.FromResult(issues);

        var conditions = string.Join(" ", query.WhereConditions);

        if (conditions.Contains(" OR ", StringComparison.OrdinalIgnoreCase))
        {
            issues.Add(new PerformanceIssue
            {
                IssueType = IssueType.OrCondition,
                Severity = IssueSeverity.Warning,
                Description = "OR condition in WHERE clause may prevent index usage",
                AffectedClause = "WHERE",
                EstimatedPerformanceImpact = 20.0,
                RecommendedFix = "Consider using UNION ALL instead of OR, or create appropriate indexes",
                ExampleFix = "SELECT * FROM Table1 WHERE col1 = 'A' UNION ALL SELECT * FROM Table1 WHERE col2 = 'B'",
                Priority = 2
            });
        }

        if (conditions.Contains("LIKE '%", StringComparison.OrdinalIgnoreCase))
        {
            issues.Add(new PerformanceIssue
            {
                IssueType = IssueType.LeadingWildcard,
                Severity = IssueSeverity.Warning,
                Description = "LIKE with leading wildcard prevents index usage",
                AffectedClause = "WHERE",
                EstimatedPerformanceImpact = 30.0,
                RecommendedFix = "Use full-text search or CONTAINS instead of LIKE with leading wildcard",
                Priority = 2
            });
        }

        return ValueTask.FromResult(issues);
    }

    // ── Private synchronous helpers ──────────────────────────────────────────

    private static List<PerformanceIssue> DetectSelectStarIssues(DatabaseQuery query)
    {
        if (!query.QueryText.Contains("SELECT *", StringComparison.OrdinalIgnoreCase))
            return [];

        return
        [
            new PerformanceIssue
            {
                IssueType = IssueType.SelectStar,
                Severity = IssueSeverity.Info,
                Description = "SELECT * used - may retrieve unnecessary columns",
                AffectedClause = "SELECT",
                EstimatedPerformanceImpact = 10.0,
                RecommendedFix = "Specify only required columns",
                ExampleFix = "SELECT col1, col2, col3 FROM table...",
                Priority = 3
            }
        ];
    }

    private static List<PerformanceIssue> DetectLeadingWildcardIssues(DatabaseQuery query)
    {
        // Leading wildcard detection is handled by DetectIndexOpportunitiesAsync.
        // This stub is retained for extensibility (e.g., detecting wildcards outside WHERE).
        return [];
    }

    private static List<PerformanceIssue> DetectFunctionOnColumnIssues(DatabaseQuery query)
    {
        var matches = FunctionOnColumnRegex().Matches(query.QueryText);

        if (matches.Count == 0)
            return [];

        return
        [
            new PerformanceIssue
            {
                IssueType = IssueType.FunctionOnColumn,
                Severity = IssueSeverity.Warning,
                Description = $"Found {matches.Count} function(s) applied to columns - may prevent index usage",
                AffectedClause = "WHERE",
                EstimatedPerformanceImpact = 25.0,
                RecommendedFix = "Move functions to the right side of comparison or use computed columns with indexes",
                Priority = 2
            }
        ];
    }

    private static List<PerformanceIssue> DetectImplicitConversionIssues(DatabaseQuery query)
    {
        if (!query.QueryText.Contains('='))
            return [];

        return
        [
            new PerformanceIssue
            {
                IssueType = IssueType.ImplicitConversion,
                Severity = IssueSeverity.Info,
                Description = "Potential implicit type conversion in comparison - verify column types",
                EstimatedPerformanceImpact = 5.0,
                RecommendedFix = "Ensure compared values have matching data types",
                Priority = 3
            }
        ];
    }
}
