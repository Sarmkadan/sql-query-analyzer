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
/// Plugin that detects ORDER BY clauses without pagination (TOP/LIMIT/OFFSET-FETCH)
/// which may indicate full result set sorting rather than paginated sorting.
/// Flags ORDER BY without TOP/LIMIT/OFFSET-FETCH as potential full-sort operations.
/// </summary>
public class UnboundedOrderByPlugin : AnalysisPluginBase
{
    private readonly ILogger<UnboundedOrderByPlugin>? _logger;

    public override string PluginId => "unbounded-orderby-detection";
    public override string Name => "Unbounded ORDER BY Detection Plugin";
    public override Version Version => new(1, 0, 0);

    public UnboundedOrderByPlugin(ILogger<UnboundedOrderByPlugin>? logger = null)
    {
        _logger = logger;
    }

    public override async Task<QueryAnalysisResult> ProcessAsync(QueryAnalysisResult result)
    {
        if (result.Query == null || string.IsNullOrWhiteSpace(result.Query))
        {
            _logger?.LogDebug("Query is null or empty, skipping Unbounded ORDER BY detection");
            return result;
        }

        var query = result.Query;

        // Skip analysis if plugin is disabled
        if (!IsEnabled)
        {
            _logger?.LogDebug("Plugin {PluginName} is disabled, skipping", Name);
            return result;
        }

        _logger?.LogDebug("Processing query for unbounded ORDER BY patterns: {QueryId}", result.QueryId);

        // Find all ORDER BY clauses in the query using a simpler approach
        // Match ORDER BY followed by column list until end of statement
        var orderByMatches = Regex.Matches(query,
            @"ORDER\s+BY\s+([^;]*?)(?=\s*(?:;|$))",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        if (orderByMatches.Count == 0)
        {
            _logger?.LogDebug("No ORDER BY clauses found in query");
            return result;
        }

        foreach (Match match in orderByMatches)
        {
            if (!match.Success) continue;

            var orderByClause = match.Groups[1].Value.Trim();

            // Skip if the ORDER BY clause is empty
            if (string.IsNullOrWhiteSpace(orderByClause))
            {
                continue;
            }

            // Check if this ORDER BY has pagination (TOP/LIMIT/OFFSET-FETCH) before the ORDER BY
            if (!HasPagination(query, match.Index))
            {
                var issue = CreateUnboundedOrderByIssue(orderByClause, match.Index);
                result.Issues.Add(issue);
                _logger?.LogInformation("Detected unbounded ORDER BY pattern in query {QueryId}", result.QueryId);
            }
        }

        return await Task.FromResult(result);
    }

    /// <summary>
    /// Checks if the query has pagination clauses (TOP/LIMIT/OFFSET-FETCH) that bound the result set.
    /// </summary>
    private bool HasPagination(string query, int orderByMatchIndex)
    {
        // Check for TOP clause before ORDER BY
        var beforeOrderBy = query.Substring(0, orderByMatchIndex);
        var afterOrderBy = query.Substring(orderByMatchIndex);

        // Check for various pagination patterns
        // TOP must be before ORDER BY
        var hasTop = Regex.IsMatch(beforeOrderBy, @"\bTOP\s+\d+\b", RegexOptions.IgnoreCase);

        // LIMIT, OFFSET-FETCH, FETCH NEXT can be before or after ORDER BY in different SQL dialects
        var hasLimit = Regex.IsMatch(query, @"\bLIMIT\s+\d+\b", RegexOptions.IgnoreCase);
        var hasOffsetFetch = Regex.IsMatch(query, @"\bOFFSET\s+\d+\s+ROWS\s+FETCH\s+NEXT\s+\d+\s+ROWS\s+ONLY\b",
            RegexOptions.IgnoreCase);
        var hasFetchNext = Regex.IsMatch(query, @"\bFETCH\s+NEXT\s+\d+\s+ROWS\s+ONLY\b",
            RegexOptions.IgnoreCase);

        // ROW_NUMBER must be before ORDER BY (in the subquery)
        var hasRowNum = Regex.IsMatch(beforeOrderBy, @"\bROW_NUMBER\s*\(",
            RegexOptions.IgnoreCase);

        return hasTop || hasLimit || hasOffsetFetch || hasFetchNext || hasRowNum;
    }

    /// <summary>
    /// Creates a performance issue for unbounded ORDER BY pattern.
    /// </summary>
    private PerformanceIssue CreateUnboundedOrderByIssue(string orderByClause, int matchIndex)
    {
        // Calculate line number from match index
        var lineNumber = 1;
        var lines = orderByClause.Split(new[] { '\n', '\r' }, StringSplitOptions.None);
        if (lines.Length > 0 && matchIndex > 0)
        {
            // Simple line count approximation
            lineNumber = orderByClause.Substring(0, Math.Min(matchIndex, orderByClause.Length)).Count(c => c == '\n') + 1;
        }

        var issue = new PerformanceIssue
        {
            IssueType = IssueType.LargeResultSet,
            Severity = IssueSeverity.Info,
            Description = "ORDER BY without pagination (TOP/LIMIT/OFFSET-FETCH) detected - may cause full result set sorting",
            AffectedClause = "ORDER BY",
            LineNumber = lineNumber,
            ColumnNumber = 1,
            EstimatedPerformanceImpact = 20.0, // Moderate impact - sorting large datasets
            RecommendedFix = "Add pagination to ORDER BY clause to limit result set size:",
            ExampleFix = "SELECT column1, column2 FROM table_name ORDER BY column1 OFFSET 0 ROWS FETCH NEXT 100 ROWS ONLY"
        };

        // Add detailed explanation to metadata
        issue.Metadata.Add("orderby_clause", orderByClause);
        issue.Metadata.Add("impact_reason",
            "ORDER BY without pagination clauses (TOP/LIMIT/OFFSET-FETCH) sorts the entire result set, " +
            "which can be expensive for large datasets. Adding pagination limits sorting to only the rows " +
            "that will be returned.");
        issue.Metadata.Add("best_practice",
            "Always use pagination with ORDER BY when dealing with large result sets. " +
            "Use TOP/LIMIT/OFFSET-FETCH to limit the number of rows sorted.");
        issue.Metadata.Add("pattern", "unbounded-orderby");

        return issue;
    }
}
