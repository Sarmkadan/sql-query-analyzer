#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Constants;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Plugins;

/// <summary>
/// Plugin that detects SELECT DISTINCT combined with JOINs as potential join-fanout smell.
/// Flags queries using SELECT DISTINCT with multiple tables as potential performance issues.
/// Suggests verifying join keys are properly constrained to avoid unnecessary duplicate elimination.
/// </summary>
public class DistinctAbusePlugin : AnalysisPluginBase
{
    private readonly ILogger<DistinctAbusePlugin>? _logger;

    public override string PluginId => "distinct-abuse-detection";
    public override string Name => "DISTINCT Abuse Detection Plugin";
    public override Version Version => new(1, 0, 0);

    public DistinctAbusePlugin(ILogger<DistinctAbusePlugin>? logger = null)
    {
        _logger = logger;
    }

    public override async Task<QueryAnalysisResult> ProcessAsync(QueryAnalysisResult result)
    {
        if (result.Query == null || string.IsNullOrWhiteSpace(result.Query))
        {
            _logger?.LogDebug("Query is null or empty, skipping DISTINCT abuse detection");
            return result;
        }

        var query = result.Query;

        // Skip analysis if plugin is disabled
        if (!IsEnabled)
        {
            _logger?.LogDebug("Plugin {PluginName} is disabled, skipping", Name);
            return result;
        }

        _logger?.LogDebug("Processing query for DISTINCT abuse patterns: {QueryId}", result.QueryId);

        // Find all SELECT statements with DISTINCT
        var distinctMatches = Regex.Matches(query,
            @"SELECT\s+DISTINCT\s+(.*?)\s+FROM",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        if (distinctMatches.Count == 0)
        {
            _logger?.LogDebug("No SELECT DISTINCT statements found in query");
            return result;
        }

        foreach (Match match in distinctMatches)
        {
            if (!match.Success) continue;

            var selectClause = match.Groups[1].Value.Trim();
            var fromClauseStart = match.Index + match.Length;

            // Skip if the select clause is empty
            if (string.IsNullOrWhiteSpace(selectClause))
            {
                continue;
            }

            // Check if this SELECT has JOINs in the FROM clause
            var remainingQuery = query.Substring(fromClauseStart);
            if (HasJoinClauses(remainingQuery))
            {
                var issue = CreateDistinctAbuseIssue(match.Index, selectClause, fromClauseStart);
                result.Issues.Add(issue);
                _logger?.LogInformation("Detected DISTINCT with JOIN pattern in query {QueryId}", result.QueryId);
            }
        }

        return await Task.FromResult(result);
    }

    /// <summary>
    /// Checks if the query contains JOIN clauses after SELECT DISTINCT.
    /// </summary>
    private bool HasJoinClauses(string queryAfterFrom)
    {
        // Normalize the query: remove comments and extra whitespace
        var normalized = RemoveComments(queryAfterFrom);
        normalized = Regex.Replace(normalized, @"\s+", " ").Trim();

        // Check for various JOIN patterns
        var hasExplicitJoin = Regex.IsMatch(normalized,
            @"(INNER|LEFT|RIGHT|FULL|CROSS)\s+JOIN\b",
            RegexOptions.IgnoreCase);

        var hasCommaJoin = Regex.IsMatch(normalized, @",\s*\w+", RegexOptions.IgnoreCase);

        return hasExplicitJoin || hasCommaJoin;
    }

    /// <summary>
    /// Removes SQL comments from the text to avoid false positives.
    /// </summary>
    private string RemoveComments(string text)
    {
        // Remove single-line comments (-- to end of line)
        var result = Regex.Replace(text, @"--.*?(?=\r?\n|$)", "", RegexOptions.Multiline);

        // Remove multi-line comments (/* ... */)
        result = Regex.Replace(result, @"/\*.*?\*/", "", RegexOptions.Singleline);

        return result;
    }

    /// <summary>
    /// Creates a performance issue for SELECT DISTINCT with JOIN pattern.
    /// </summary>
    private PerformanceIssue CreateDistinctAbuseIssue(int matchIndex, string selectClause, int fromClauseStart)
    {
        // Calculate line number from match index
        var lineNumber = 1;
        if (matchIndex > 0)
        {
            var linesBefore = selectClause.Substring(0, Math.Min(matchIndex, selectClause.Length));
            lineNumber = linesBefore.Count(c => c == '\n') + 1;
        }

        var issue = new PerformanceIssue
        {
            IssueType = IssueType.LargeResultSet,
            Severity = IssueSeverity.Warning,
            Description = "SELECT DISTINCT combined with JOINs may indicate join-fanout smell - verify join keys properly constrain results",
            AffectedClause = "SELECT DISTINCT",
            LineNumber = lineNumber,
            ColumnNumber = 1,
            EstimatedPerformanceImpact = 40.0, // Moderate to high impact
            RecommendedFix = "Review join conditions to ensure they properly constrain the result set, or consider removing DISTINCT if duplicates are acceptable:",
            ExampleFix = "FROM Table1 t1 INNER JOIN Table2 t2 ON t1.Id = t2.Table1Id WHERE t1.Status = 'Active'"
        };

        // Add detailed explanation to metadata
        issue.Metadata.Add("select_clause", selectClause);
        issue.Metadata.Add("impact_reason",
            "SELECT DISTINCT combined with JOINs can indicate a join-fanout smell where multiple tables are joined without proper constraints. " +
            "The database must first materialize the full join result (potentially huge) before eliminating duplicates, causing unnecessary I/O and memory usage. " +
            "This pattern often suggests missing join conditions or WHERE clause predicates.");
        issue.Metadata.Add("best_practice",
            "Avoid SELECT DISTINCT with multiple tables unless absolutely necessary. " +
            "Ensure JOIN conditions are properly constrained with equality predicates on foreign keys. " +
            "Consider adding WHERE clause filters to reduce the result set before duplicate elimination.");
        issue.Metadata.Add("pattern", "distinct-with-joins");
        issue.Metadata.Add("join_verification_check", "Verify that all join keys are properly constrained and that the WHERE clause filters appropriately");

        return issue;
    }
}
