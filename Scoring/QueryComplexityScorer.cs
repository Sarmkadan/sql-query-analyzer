#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Linq;
using System.Text.RegularExpressions;
using SqlQueryAnalyzer.Constants;
using SqlQueryAnalyzer.Models;
using SqlQueryAnalyzer.Utilities;

namespace SqlQueryAnalyzer.Scoring;

/// <summary>
/// Computes a numeric complexity score for an analyzed SQL query.
/// </summary>
/// <remarks>
/// Scoring rules:
/// <list type="bullet">
///   <item>Base score: number of distinct tables referenced</item>
///   <item>+5 per full-table-scan issue</item>
///   <item>+3 per missing-index issue</item>
///   <item>+10 per N+1 detection</item>
///   <item>+2 per subquery</item>
/// </list>
/// </remarks>
public static partial class QueryComplexityScorer
{
    /// <summary>
    /// The complexity points added for each full-table-scan issue.
    /// </summary>
    private const int TableScanWeight = 5;

    /// <summary>
    /// The complexity points added for each missing-index issue.
    /// </summary>
    private const int MissingIndexWeight = 3;

    /// <summary>
    /// The complexity points added for each N+1 detection.
    /// </summary>
    private const int NPlusOneWeight = 10;

    /// <summary>
    /// The complexity points added for each subquery.
    /// </summary>
    private const int SubqueryWeight = 2;

    // Matches inline SELECT statements used as subqueries: (SELECT …)
    [GeneratedRegex(@"\(\s*SELECT\b", RegexOptions.IgnoreCase)]
    private static partial Regex SubqueryCountRegex();

    /// <summary>
    /// Computes the complexity score for the given <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The completed analysis result.</param>
    /// <returns>A non-negative integer score; higher means more complex.</returns>
    public static int ComputeScore(QueryAnalysisResult result)
    {
        // Base: distinct tables referenced in the query
        int tableCount = SqlPatternAnalyzer.ExtractTablesFromQuery(result.Query).Count;
        int score = tableCount;

        // +5 for each full table scan
        score += result.Issues.Count(i => i.IssueType == IssueType.TableScan) * TableScanWeight;

        // +3 for each missing index warning
        score += result.Issues.Count(i => i.IssueType == IssueType.MissingIndex) * MissingIndexWeight;

        // +10 for each N+1 detection
        score += result.Issues.Count(i => i.IssueType == IssueType.NPlusOne) * NPlusOneWeight;

        // +2 for each subquery
        score += SubqueryCountRegex().Matches(result.Query).Count * SubqueryWeight;

        return score;
    }
}
