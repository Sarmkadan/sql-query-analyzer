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
/// Plugin that detects SELECT * usage in SQL queries and recommends explicit column lists.
/// Flags SELECT * patterns (case-insensitive) while ignoring COUNT(*) and stars inside comments/strings.
/// </summary>
public class SelectStarPlugin : AnalysisPluginBase
{
    private readonly ILogger<SelectStarPlugin>? _logger;

    public override string PluginId => "select-star-detection";
    public override string Name => "SELECT * Detection Plugin";
    public override Version Version => new(1, 0, 0);

    public SelectStarPlugin(ILogger<SelectStarPlugin>? logger = null)
    {
        _logger = logger;
    }

    public override async Task<QueryAnalysisResult> ProcessAsync(QueryAnalysisResult result)
    {
        if (result.Query == null || string.IsNullOrWhiteSpace(result.Query))
        {
            _logger?.LogDebug("Query is null or empty, skipping SELECT * detection");
            return result;
        }

        var query = result.Query;

        // Skip analysis if plugin is disabled
        if (!IsEnabled)
        {
            _logger?.LogDebug("Plugin {PluginName} is disabled, skipping", Name);
            return result;
        }

        _logger?.LogDebug("Processing query for SELECT * patterns: {QueryId}", result.QueryId);

        // Find all SELECT statements in the query
        var selectMatches = Regex.Matches(query, @"SELECT\s+(.*?)\s+FROM", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        if (selectMatches.Count == 0)
        {
            _logger?.LogDebug("No SELECT statements found in query");
            return result;
        }

        foreach (Match match in selectMatches)
        {
            if (!match.Success) continue;

            var selectClause = match.Groups[1].Value.Trim();

            // Skip if the select clause is empty (shouldn't happen but just in case)
            if (string.IsNullOrWhiteSpace(selectClause))
            {
                continue;
            }

            // Check if this is a SELECT * pattern
            if (IsSelectStarPattern(selectClause))
            {
                var issue = CreateSelectStarIssue(selectClause, match.Index);
                result.Issues.Add(issue);
                _logger?.LogInformation("Detected SELECT * pattern in query {QueryId}", result.QueryId);
            }
        }

        return await Task.FromResult(result);
    }

    /// <summary>
    /// Checks if the SELECT clause contains a SELECT * pattern.
    /// </summary>
    private bool IsSelectStarPattern(string selectClause)
    {
        // Normalize the clause: remove whitespace and comments
        var normalized = RemoveComments(selectClause);
        normalized = Regex.Replace(normalized, @"\s+", " ").Trim();

        // Check for SELECT * (case-insensitive)
        // Pattern: SELECT followed by * (with optional whitespace)
        var starPattern = @"^\*$";

        // Also check for SELECT table.* patterns
        var tableStarPattern = @"^\w+\.\*$";

        // Check if the normalized clause is just "*" or "table.*"
        if (Regex.IsMatch(normalized, starPattern, RegexOptions.IgnoreCase))
        {
            return true;
        }

        if (Regex.IsMatch(normalized, tableStarPattern, RegexOptions.IgnoreCase))
        {
            return true;
        }

        // Check for multiple columns separated by commas where one is *
        var columns = normalized.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var column in columns)
        {
            var trimmedColumn = column.Trim();
            if (Regex.IsMatch(trimmedColumn, @"^\*$", RegexOptions.IgnoreCase) ||
                Regex.IsMatch(trimmedColumn, @"^\w+\.\*$", RegexOptions.IgnoreCase))
            {
                return true;
            }
        }

        return false;
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
