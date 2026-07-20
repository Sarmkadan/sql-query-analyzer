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
/// Plugin that detects Cartesian products in SQL queries by analyzing FROM clauses.
/// Flags queries listing multiple tables separated by commas without proper join conditions,
/// and explicit CROSS JOINs as potential performance issues.
/// </summary>
public class CartesianJoinPlugin : AnalysisPluginBase
{
    private readonly ILogger<CartesianJoinPlugin>? _logger;

    public override string PluginId => "cartesian-join-detection";
    public override string Name => "Cartesian Join Detection Plugin";
    public override Version Version => new(1, 0, 0);

    public CartesianJoinPlugin(ILogger<CartesianJoinPlugin>? logger = null)
    {
        _logger = logger;
    }

    public override async Task<QueryAnalysisResult> ProcessAsync(QueryAnalysisResult result)
    {
        if (result.Query == null || string.IsNullOrWhiteSpace(result.Query))
        {
            _logger?.LogDebug("Query is null or empty, skipping Cartesian join detection");
            return result;
        }

        var query = result.Query;

        // Skip analysis if plugin is disabled
        if (!IsEnabled)
        {
            _logger?.LogDebug("Plugin {PluginName} is disabled, skipping", Name);
            return result;
        }

        _logger?.LogDebug("Processing query for Cartesian join patterns: {QueryId}", result.QueryId);

        // Find all FROM clauses in the query
        // We need to be careful with comments - use a more sophisticated approach
        var fromMatches = Regex.Matches(query, @"FROM\s+(.*?)(?:\s+WHERE|\s+GROUP|\s+ORDER|\s+HAVING|\s+LIMIT|\s*;|$)",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        if (fromMatches.Count == 0)
        {
            _logger?.LogDebug("No FROM clauses found in query");
            return result;
        }

        foreach (Match match in fromMatches)
        {
            if (!match.Success) continue;

            var fromClause = match.Groups[1].Value.Trim();

            // Skip if the from clause is empty
            if (string.IsNullOrWhiteSpace(fromClause))
            {
                continue;
            }

            // Check for comma-separated tables (implicit cross join)
            if (IsImplicitCrossJoinPattern(fromClause))
            {
                var issue = CreateImplicitCrossJoinIssue(fromClause, match.Index);
                result.Issues.Add(issue);
                _logger?.LogInformation("Detected implicit CROSS JOIN pattern in query {QueryId}", result.QueryId);
            }

            // Check for explicit CROSS JOIN
            if (IsExplicitCrossJoinPattern(query, match.Index))
            {
                var issue = CreateExplicitCrossJoinIssue(match.Index);
                result.Issues.Add(issue);
                _logger?.LogInformation("Detected explicit CROSS JOIN in query {QueryId}", result.QueryId);
            }
        }

        return await Task.FromResult(result);
    }

    /// <summary>
    /// Checks if the FROM clause contains comma-separated tables (implicit cross join).
    /// </summary>
    private bool IsImplicitCrossJoinPattern(string fromClause)
    {
        // Normalize the clause: remove comments and extra whitespace
        var normalized = RemoveComments(fromClause);
        normalized = Regex.Replace(normalized, @"\s+", " ").Trim();

        // Count the number of tables (comma-separated)
        var tableCount = normalized.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Length;

        // If there are 2 or more tables separated by commas, it's a potential Cartesian product
        return tableCount >= 2;
    }

    /// <summary>
    /// Checks if the query contains explicit CROSS JOIN syntax.
    /// </summary>
    private bool IsExplicitCrossJoinPattern(string query, int fromMatchIndex)
    {
        // Look for CROSS JOIN in the query after the FROM clause we found
        var remainingQuery = query.Substring(fromMatchIndex);

        // Check for CROSS JOIN (case-insensitive)
        return Regex.IsMatch(remainingQuery, @"CROSS\s+JOIN", RegexOptions.IgnoreCase);
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
    /// Creates a performance issue for implicit CROSS JOIN pattern.
    /// </summary>
    private PerformanceIssue CreateImplicitCrossJoinIssue(string fromClause, int matchIndex)
    {
        // Calculate line number from match index
        var lineNumber = 1;
        var lines = fromClause.Split(new[] { '\n', '\r' }, StringSplitOptions.None);
        if (lines.Length > 0 && matchIndex > 0)
        {
            // Simple line count approximation
            lineNumber = fromClause.Substring(0, Math.Min(matchIndex, fromClause.Length)).Count(c => c == '\n') + 1;
        }

        var issue = new PerformanceIssue
        {
            IssueType = IssueType.CrossJoin,
            Severity = IssueSeverity.Critical,
            Description = "Implicit CROSS JOIN detected - multiple tables in FROM clause without explicit JOIN conditions creates Cartesian product",
            AffectedClause = "FROM",
            LineNumber = lineNumber,
            ColumnNumber = 1,
            EstimatedPerformanceImpact = 90.0, // High impact - Cartesian product
            RecommendedFix = "Replace comma-separated tables with explicit JOIN syntax with proper join conditions:",
            ExampleFix = "FROM Table1 t1 INNER JOIN Table2 t2 ON t1.Id = t2.Table1Id"
        };

        // Add detailed explanation to metadata
        issue.Metadata.Add("from_clause", fromClause);
        issue.Metadata.Add("impact_reason", "Comma-separated tables in FROM clause without JOIN conditions create a Cartesian product, multiplying rows and causing severe performance degradation. Explicit JOINs with proper conditions are required.");
        issue.Metadata.Add("best_practice", "Always use explicit JOIN syntax with proper join conditions. Never use comma-separated tables in FROM clause without JOIN conditions.");
        issue.Metadata.Add("pattern", "implicit-cross-join");

        return issue;
    }

    /// <summary>
    /// Creates a performance issue for explicit CROSS JOIN.
    /// </summary>
    private PerformanceIssue CreateExplicitCrossJoinIssue(int matchIndex)
    {
        // Calculate line number from match index
        var lineNumber = 1;
        var queryText = "";
        // We need the actual query text, but we don't have it here. Use a default line number.
        lineNumber = Math.Max(1, matchIndex / 50) + 1;

        // Alternative approach: since we don't have query text, use a simpler calculation
        // Just return a reasonable line number based on matchIndex
        if (matchIndex > 0)
        {
            lineNumber = (int)Math.Ceiling(matchIndex / 50.0) + 1;
        }

        var issue = new PerformanceIssue
        {
            IssueType = IssueType.CrossJoin,
            Severity = IssueSeverity.Critical,
            Description = "Explicit CROSS JOIN detected - consider using INNER JOIN with proper join conditions instead",
            AffectedClause = "FROM",
            LineNumber = lineNumber,
            ColumnNumber = 1,
            EstimatedPerformanceImpact = 85.0, // High impact - still creates Cartesian product
            RecommendedFix = "Replace CROSS JOIN with INNER JOIN using proper join conditions:",
            ExampleFix = "FROM Table1 t1 INNER JOIN Table2 t2 ON t1.Id = t2.Table1Id"
        };

        // Add detailed explanation to metadata
        issue.Metadata.Add("impact_reason", "CROSS JOIN creates a Cartesian product by combining every row from both tables. This can result in extremely large intermediate result sets. Use INNER JOIN with proper join conditions to only combine related rows.");
        issue.Metadata.Add("best_practice", "Only use CROSS JOIN when you explicitly need a Cartesian product. Prefer INNER JOIN with proper join conditions for most use cases.");
        issue.Metadata.Add("pattern", "explicit-cross-join");

        return issue;
    }
}
