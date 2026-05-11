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
        score += result.Issues.Count(i => i.IssueType == IssueType.TableScan) * 5;

        // +3 for each missing index warning
        score += result.Issues.Count(i => i.IssueType == IssueType.MissingIndex) * 3;

        // +10 for each N+1 detection
        score += result.Issues.Count(i => i.IssueType == IssueType.NPlusOne) * 10;

        // +2 for each subquery
        score += SubqueryCountRegex().Matches(result.Query).Count * 2;

        return score;
    }
}
