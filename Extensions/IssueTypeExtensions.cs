#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using SqlQueryAnalyzer.Constants;

namespace SqlQueryAnalyzer.Extensions;

/// <summary>
/// Extension methods for <see cref="IssueType"/>.
/// Provides human-readable names for performance issue types.
/// </summary>
public static class IssueTypeExtensions
{
    /// <summary>
    /// Gets a human-readable display name for the issue type.
    /// </summary>
    /// <param name="issueType">The issue type to describe.</param>
    /// <returns>A short, human-readable name for the issue type.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="issueType"/> is not a defined <see cref="IssueType"/> value.</exception>
    public static string GetDisplayName(this IssueType issueType)
    {
        return issueType switch
        {
            IssueType.TableScan => "Table Scan",
            IssueType.NPlusOne => "N+1 Query",
            IssueType.MissingIndex => "Missing Index",
            IssueType.UnusedIndex => "Unused Index",
            IssueType.ImplicitConversion => "Implicit Conversion",
            IssueType.NonSargable => "Non-SARGable Predicate",
            IssueType.IneffectiveJoin => "Ineffective Join",
            IssueType.CrossJoin => "Cross Join",
            IssueType.OrCondition => "OR Condition",
            IssueType.SubqueryOptimization => "Subquery Optimization",
            IssueType.IndexFragmentation => "Index Fragmentation",
            IssueType.MissingStatistics => "Missing Statistics",
            IssueType.OutdatedStatistics => "Outdated Statistics",
            IssueType.FunctionOnColumn => "Function on Column",
            IssueType.LeadingWildcard => "Leading Wildcard",
            IssueType.SelectStar => "SELECT *",
            IssueType.LargeResultSet => "Large Result Set",
            IssueType.JoinOnDifferentTypes => "Join on Different Types",
            IssueType.InefficientUnion => "Inefficient UNION",
            IssueType.ClusteredIndexIssue => "Clustered Index Issue",
            _ => throw new ArgumentOutOfRangeException(nameof(issueType), issueType, "Unknown issue type.")
        };
    }
}
