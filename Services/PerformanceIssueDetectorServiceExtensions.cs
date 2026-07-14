#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Constants;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Services;

/// <summary>
/// Extension methods for <see cref="PerformanceIssueDetectorService"/> that provide
/// additional detection capabilities and convenience methods for common scenarios.
/// </summary>
public static class PerformanceIssueDetectorServiceExtensions
{
    /// <summary>
    /// Detects performance issues across multiple queries and returns a combined report.
    /// </summary>
    /// <param name="service">The detector service instance</param>
    /// <param name="queries">Collection of queries to analyze</param>
    /// <returns>Combined list of all detected issues across all queries</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="queries"/> is null</exception>
    public static async Task<IReadOnlyList<PerformanceIssue>> DetectIssuesAsync(
        this PerformanceIssueDetectorService service,
        IEnumerable<DatabaseQuery> queries)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(queries);

        var allIssues = new List<PerformanceIssue>();

        foreach (var query in queries)
        {
            var issues = await service.DetectIssuesAsync(query);
            allIssues.AddRange(issues);
        }

        return allIssues.AsReadOnly();
    }

    /// <summary>
    /// Detects N+1 query patterns specifically for queries referencing the same table.
    /// </summary>
    /// <param name="service">The detector service instance</param>
    /// <param name="queries">Collection of queries to analyze</param>
    /// <param name="tableName">Optional specific table name to check for N+1 patterns</param>
    /// <returns>List of N+1 issues detected</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="queries"/> is null</exception>
    public static async ValueTask<IReadOnlyList<PerformanceIssue>> DetectNPlusOneAsync(
        this PerformanceIssueDetectorService service,
        IEnumerable<DatabaseQuery> queries,
        string? tableName = null)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(queries);

        var filteredQueries = tableName == null
            ? queries.ToList()
            : queries.Where(q => q.ReferencedTables.Contains(tableName, StringComparer.OrdinalIgnoreCase)).ToList();

        if (filteredQueries.Count == 0)
        {
            return Array.Empty<PerformanceIssue>();
        }

        var issues = await service.DetectNPlusOneAsync(filteredQueries);
        return issues.AsReadOnly();
    }

    /// <summary>
    /// Detects join issues across multiple queries and returns a combined report.
    /// </summary>
    /// <param name="service">The detector service instance</param>
    /// <param name="queries">Collection of queries to analyze</param>
    /// <returns>Combined list of join-related issues across all queries</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="queries"/> is null</exception>
    public static async Task<IReadOnlyList<PerformanceIssue>> DetectJoinIssuesAsync(
        this PerformanceIssueDetectorService service,
        IEnumerable<DatabaseQuery> queries)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(queries);

        var allIssues = new List<PerformanceIssue>();

        foreach (var query in queries)
        {
            var issues = await service.DetectJoinIssuesAsync(query);
            allIssues.AddRange(issues);
        }

        return allIssues.AsReadOnly();
    }

    /// <summary>
    /// Detects index opportunities across multiple queries and returns a combined report.
    /// </summary>
    /// <param name="service">The detector service instance</param>
    /// <param name="queries">Collection of queries to analyze</param>
    /// <returns>Combined list of index-related issues across all queries</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="queries"/> is null</exception>
    public static async Task<IReadOnlyList<PerformanceIssue>> DetectIndexOpportunitiesAsync(
        this PerformanceIssueDetectorService service,
        IEnumerable<DatabaseQuery> queries)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(queries);

        var allIssues = new List<PerformanceIssue>();

        foreach (var query in queries)
        {
            var issues = await service.DetectIndexOpportunitiesAsync(query);
            allIssues.AddRange(issues);
        }

        return allIssues.AsReadOnly();
    }

    /// <summary>
    /// Filters detected issues by severity level.
    /// </summary>
    /// <param name="issues">Collection of performance issues</param>
    /// <param name="severity">Minimum severity level to include</param>
    /// <returns>Filtered collection of issues with severity >= specified level</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="issues"/> is null</exception>
    public static IEnumerable<PerformanceIssue> FilterBySeverity(
        this IEnumerable<PerformanceIssue> issues,
        IssueSeverity severity)
    {
        ArgumentNullException.ThrowIfNull(issues);

        return issues.Where(i => i.Severity >= severity);
    }

    /// <summary>
    /// Groups performance issues by their type for easier analysis.
    /// </summary>
    /// <param name="issues">Collection of performance issues</param>
    /// <returns>Dictionary grouping issues by their <see cref="IssueType"/></returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="issues"/> is null</exception>
    public static IReadOnlyDictionary<IssueType, IReadOnlyList<PerformanceIssue>> GroupByIssueType(
        this IEnumerable<PerformanceIssue> issues)
    {
        ArgumentNullException.ThrowIfNull(issues);

        return issues
            .GroupBy(i => i.IssueType)
            .ToDictionary(
                g => g.Key,
                g => g.ToList().AsReadOnly()
            )
            .ToDictionary(kvp => kvp.Key, kvp => (IReadOnlyList<PerformanceIssue>)kvp.Value);
    }

    /// <summary>
    /// Calculates total estimated performance impact across all issues.
    /// </summary>
    /// <param name="issues">Collection of performance issues</param>
    /// <returns>Sum of all estimated performance impacts</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="issues"/> is null</exception>
    public static double CalculateTotalImpact(this IEnumerable<PerformanceIssue> issues)
    {
        ArgumentNullException.ThrowIfNull(issues);

        return issues.Sum(i => i.EstimatedPerformanceImpact);
    }

    /// <summary>
    /// Creates a prioritized list of recommended fixes based on detected issues.
    /// </summary>
    /// <param name="issues">Collection of performance issues</param>
    /// <returns>Ordered list of unique recommended fixes</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="issues"/> is null</exception>
    public static IReadOnlyList<string> GetPrioritizedFixes(this IEnumerable<PerformanceIssue> issues)
    {
        ArgumentNullException.ThrowIfNull(issues);

        return issues
            .OrderByDescending(i => i.Severity)
            .ThenByDescending(i => i.EstimatedPerformanceImpact)
            .Select(i => i.RecommendedFix)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList()
            .AsReadOnly();
    }
}