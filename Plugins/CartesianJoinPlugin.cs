#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Constants;
using SqlQueryAnalyzer.Models;
using SqlQueryAnalyzer.Utilities;

namespace SqlQueryAnalyzer.Plugins;

/// <summary>
/// Plugin that detects Cartesian products in SQL queries by analyzing FROM clauses.
/// Flags queries listing multiple tables separated by commas without proper join conditions,
/// and explicit CROSS JOINs as potential performance issues.
/// </summary>
public class CartesianJoinPlugin : AnalysisPluginBase
{
    /// <summary>
    /// Pattern used to capture the contents of a FROM clause up to the next SQL clause.
    /// </summary>
    private const string FromClausePattern = @"FROM\s+(.*?)(?:\s+WHERE|\s+GROUP|\s+ORDER|\s+HAVING|\s+LIMIT|\s*;|$)";

    /// <summary>
    /// Pattern used to identify explicit CROSS JOIN syntax.
    /// </summary>
    private const string ExplicitCrossJoinPattern = @"CROSS\s+JOIN";

    /// <summary>
    /// Minimum number of comma-separated tables that indicates a potential implicit cross join.
    /// </summary>
    private const int MinimumImplicitCrossJoinTableCount = 2;

    /// <summary>
    /// Estimated performance impact assigned to implicit cross joins.
    /// </summary>
    private const double ImplicitCrossJoinPerformanceImpact = 90.0;

    /// <summary>
    /// Estimated performance impact assigned to explicit cross joins.
    /// </summary>
    private const double ExplicitCrossJoinPerformanceImpact = 85.0;

    /// <summary>
    /// Approximate number of query characters used to estimate each line.
    /// </summary>
    private const int ApproximateCharactersPerLine = 50;

    /// <summary>
    /// SQL clause associated with detected Cartesian joins.
    /// </summary>
    private const string FromClauseName = "FROM";

    /// <summary>
    /// Example replacement shared by implicit and explicit cross join issues.
    /// </summary>
    private const string JoinExampleFix = "FROM Table1 t1 INNER JOIN Table2 t2 ON t1.Id = t2.Table1Id";

    /// <summary>
    /// Metadata key used for the reason behind a detected performance impact.
    /// </summary>
    private const string ImpactReasonMetadataKey = "impact_reason";

    /// <summary>
    /// Metadata key used for the recommended join best practice.
    /// </summary>
    private const string BestPracticeMetadataKey = "best_practice";

    /// <summary>
    /// Metadata key used for the detected Cartesian join pattern.
    /// </summary>
    private const string PatternMetadataKey = "pattern";

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
        var fromMatches = Regex.Matches(query, FromClausePattern,
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
        // Normalize the clause: remove comments and collapse whitespace via the shared normalizer.
        var normalized = fromClause.RemoveSqlComments().NormalizeSqlWhitespace();

        // Count the number of tables (comma-separated)
        var tableCount = normalized.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Length;

        // If there are 2 or more tables separated by commas, it's a potential Cartesian product
        return tableCount >= MinimumImplicitCrossJoinTableCount;
    }

    /// <summary>
    /// Checks if the query contains explicit CROSS JOIN syntax.
    /// </summary>
    private bool IsExplicitCrossJoinPattern(string query, int fromMatchIndex)
    {
        // Look for CROSS JOIN in the query after the FROM clause we found
        var remainingQuery = query.Substring(fromMatchIndex);

        // Check for CROSS JOIN (case-insensitive)
        return Regex.IsMatch(remainingQuery, ExplicitCrossJoinPattern, RegexOptions.IgnoreCase);
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
            AffectedClause = FromClauseName,
            LineNumber = lineNumber,
            ColumnNumber = 1,
            EstimatedPerformanceImpact = ImplicitCrossJoinPerformanceImpact, // High impact - Cartesian product
            RecommendedFix = "Replace comma-separated tables with explicit JOIN syntax with proper join conditions:",
            ExampleFix = JoinExampleFix
        };

        // Add detailed explanation to metadata
        issue.Metadata.Add("from_clause", fromClause);
        issue.Metadata.Add(ImpactReasonMetadataKey, "Comma-separated tables in FROM clause without JOIN conditions create a Cartesian product, multiplying rows and causing severe performance degradation. Explicit JOINs with proper conditions are required.");
        issue.Metadata.Add(BestPracticeMetadataKey, "Always use explicit JOIN syntax with proper join conditions. Never use comma-separated tables in FROM clause without JOIN conditions.");
        issue.Metadata.Add(PatternMetadataKey, "implicit-cross-join");

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
        lineNumber = Math.Max(1, matchIndex / ApproximateCharactersPerLine) + 1;

        // Alternative approach: since we don't have query text, use a simpler calculation
        // Just return a reasonable line number based on matchIndex
        if (matchIndex > 0)
        {
            lineNumber = (int)Math.Ceiling(matchIndex / (double)ApproximateCharactersPerLine) + 1;
        }

        var issue = new PerformanceIssue
        {
            IssueType = IssueType.CrossJoin,
            Severity = IssueSeverity.Critical,
            Description = "Explicit CROSS JOIN detected - consider using INNER JOIN with proper join conditions instead",
            AffectedClause = FromClauseName,
            LineNumber = lineNumber,
            ColumnNumber = 1,
            EstimatedPerformanceImpact = ExplicitCrossJoinPerformanceImpact, // High impact - still creates Cartesian product
            RecommendedFix = "Replace CROSS JOIN with INNER JOIN using proper join conditions:",
            ExampleFix = JoinExampleFix
        };

        // Add detailed explanation to metadata
        issue.Metadata.Add(ImpactReasonMetadataKey, "CROSS JOIN creates a Cartesian product by combining every row from both tables. This can result in extremely large intermediate result sets. Use INNER JOIN with proper join conditions to only combine related rows.");
        issue.Metadata.Add(BestPracticeMetadataKey, "Only use CROSS JOIN when you explicitly need a Cartesian product. Prefer INNER JOIN with proper join conditions for most use cases.");
        issue.Metadata.Add(PatternMetadataKey, "explicit-cross-join");

        return issue;
    }
}
