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
/// Plugin that detects SELECT * usage in SQL queries and recommends explicit column lists.
/// Flags SELECT * patterns (case-insensitive) while ignoring COUNT(*) and stars inside comments/strings.
/// </summary>
public class SelectStarPlugin : AnalysisPluginBase
{
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(1);
    private static readonly Regex SelectClauseRegex = new(
        @"SELECT\s+(.*?)\s+FROM",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline,
        RegexTimeout);
    private static readonly Regex StarRegex = new(
        @"^\*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline,
        RegexTimeout);
    private static readonly Regex TableStarRegex = new(
        @"^\w+\.\*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline,
        RegexTimeout);

    private readonly ILogger<SelectStarPlugin>? _logger;

    public override string PluginId => "select-star-detection";
    public override string Name => "SELECT * Detection Plugin";
    public override Version Version => new(1, 0, 0);

    public SelectStarPlugin(ILogger<SelectStarPlugin>? logger = null)
    {
        _logger = logger;
    }

    public override Task<QueryAnalysisResult> ProcessAsync(QueryAnalysisResult result)
    {
        // Skip analysis if plugin is disabled
        if (!IsEnabled)
        {
            _logger?.LogDebug("Plugin {PluginName} is disabled, skipping", Name);
            return Task.FromResult(result);
        }

        if (result.Query == null || string.IsNullOrWhiteSpace(result.Query))
        {
            _logger?.LogDebug("Query is null or empty, skipping SELECT * detection");
            return Task.FromResult(result);
        }

        var query = result.Query;

        _logger?.LogDebug("Processing query for SELECT * patterns: {QueryId}", result.QueryId);

        // Find all SELECT statements in the query
        var selectMatches = SelectClauseRegex.Matches(query);

        if (selectMatches.Count == 0)
        {
            _logger?.LogDebug("No SELECT statements found in query");
            return Task.FromResult(result);
        }

        foreach (Match match in selectMatches)
        {
            AddIssueForMatch(result, match);
        }

        return Task.FromResult(result);
    }

    private void AddIssueForMatch(QueryAnalysisResult result, Match match)
    {
        if (!match.Success) return;

        var selectClause = match.Groups[1].Value.Trim();

        // Skip if the select clause is empty (shouldn't happen but just in case)
        if (string.IsNullOrWhiteSpace(selectClause))
        {
            return;
        }

        // Check if this is a SELECT * pattern
        if (IsSelectStarPattern(selectClause))
        {
            var issue = CreateSelectStarIssue(selectClause, match.Index);
            result.Issues.Add(issue);
            _logger?.LogInformation("Detected SELECT * pattern in query {QueryId}", result.QueryId);
        }
    }

    /// <summary>
    /// Checks if the SELECT clause contains a SELECT * pattern.
    /// </summary>
    private bool IsSelectStarPattern(string selectClause)
    {
        // Normalize the clause: remove comments and collapse whitespace via the shared normalizer.
        var normalized = selectClause.RemoveSqlComments().NormalizeSqlWhitespace();

        // Check if the normalized clause is just "*" or "table.*"
        if (StarRegex.IsMatch(normalized))
        {
            return true;
        }

        if (TableStarRegex.IsMatch(normalized))
        {
            return true;
        }

        // Check for multiple columns separated by commas where one is *
        var columns = normalized.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var column in columns)
        {
            var trimmedColumn = column.Trim();
            if (StarRegex.IsMatch(trimmedColumn) || TableStarRegex.IsMatch(trimmedColumn))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Creates a performance issue for SELECT * pattern.
    /// </summary>
    private PerformanceIssue CreateSelectStarIssue(string selectClause, int matchIndex)
    {
        // Calculate line number from match index
        var lineNumber = 1;
        var lines = selectClause.Split(new[] { '\n', '\r' }, StringSplitOptions.None);
        if (lines.Length > 0 && matchIndex > 0)
        {
            // Simple line count approximation
            lineNumber = selectClause.Substring(0, Math.Min(matchIndex, selectClause.Length)).Count(c => c == '\n') + 1;
        }

        var issue = new PerformanceIssue
        {
            IssueType = IssueType.SelectStar,
            Severity = IssueSeverity.Warning,
            Description = "SELECT * detected - consider using explicit column list for better performance and maintainability",
            AffectedClause = "SELECT",
            LineNumber = lineNumber,
            ColumnNumber = 1,
            EstimatedPerformanceImpact = 15.0, // Moderate impact for SELECT *
            RecommendedFix = "Replace SELECT * with explicit column names to:",
            ExampleFix = "SELECT column1, column2, column3 FROM table_name"
        };

        // Add detailed explanation to metadata
        issue.Metadata.Add("select_clause", selectClause);
        issue.Metadata.Add("impact_reason", "SELECT * retrieves all columns including unused ones, increasing I/O, memory usage, and network traffic. Explicit column lists improve query clarity and performance.");
        issue.Metadata.Add("best_practice", "Always specify only the columns you need in SELECT statements.");

        return issue;
    }
}
