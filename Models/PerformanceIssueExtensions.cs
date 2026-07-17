#nullable enable

using System.Globalization;
using SqlQueryAnalyzer.Constants;

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Provides extension methods for the <see cref="PerformanceIssue"/> class to enhance functionality
/// for working with performance issues in SQL query analysis.
/// </summary>
public static class PerformanceIssueExtensions
{
    /// <summary>
    /// Creates a deep copy of the performance issue.
    /// </summary>
    /// <param name="issue">The performance issue to copy.</param>
    /// <returns>A new <see cref="PerformanceIssue"/> instance with the same values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="issue"/> is null.</exception>
    public static PerformanceIssue DeepCopy(this PerformanceIssue issue)
    {
        ArgumentNullException.ThrowIfNull(issue);

        return new PerformanceIssue
        {
            IssueId = issue.IssueId,
            IssueType = issue.IssueType,
            Severity = issue.Severity,
            Description = issue.Description,
            AffectedClause = issue.AffectedClause,
            LineNumber = issue.LineNumber,
            ColumnNumber = issue.ColumnNumber,
            EstimatedPerformanceImpact = issue.EstimatedPerformanceImpact,
            AffectedRowCount = issue.AffectedRowCount,
            EstimatedTimeIncrease = issue.EstimatedTimeIncrease,
            RecommendedFix = issue.RecommendedFix,
            ExampleFix = issue.ExampleFix,
            Priority = issue.Priority,
            Metadata = new Dictionary<string, string>(issue.Metadata),
            DetectedAt = issue.DetectedAt
        };
    }

    /// <summary>
    /// Gets the formatted impact description including percentage and row count.
    /// </summary>
    /// <param name="issue">The performance issue.</param>
    /// <returns>A formatted string describing the performance impact.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="issue"/> is null.</exception>
    public static string GetImpactDescription(this PerformanceIssue issue)
    {
        ArgumentNullException.ThrowIfNull(issue);

        var impactText = issue.EstimatedPerformanceImpact.ToString("P0", CultureInfo.InvariantCulture);

        return issue.AffectedRowCount.HasValue && issue.AffectedRowCount > 0
            ? $"{impactText} impact on {issue.AffectedRowCount:N0} rows"
            : $"{impactText} performance impact";
    }

    /// <summary>
    /// Gets the formatted time increase description.
    /// </summary>
    /// <param name="issue">The performance issue.</param>
    /// <returns>A formatted string describing the estimated time increase, or null if not available.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="issue"/> is null.</exception>
    public static string? GetTimeIncreaseDescription(this PerformanceIssue issue)
    {
        ArgumentNullException.ThrowIfNull(issue);

        return issue.EstimatedTimeIncrease.HasValue
            ? $"Time increase: {issue.EstimatedTimeIncrease.Value.TotalMilliseconds:N0}ms"
            : null;
    }

    /// <summary>
    /// Gets the formatted location information (file, line, column).
    /// </summary>
    /// <param name="issue">The performance issue.</param>
    /// <returns>A formatted string with location information.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="issue"/> is null.</exception>
    public static string GetLocationInfo(this PerformanceIssue issue)
    {
        ArgumentNullException.ThrowIfNull(issue);

        return $"Line {issue.LineNumber}, Column {issue.ColumnNumber} in {issue.AffectedClause}";
    }

    /// <summary>
    /// Gets the formatted metadata as a collection of key-value pairs.
    /// </summary>
    /// <param name="issue">The performance issue.</param>
    /// <returns>A read-only collection of key-value pairs for the metadata.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="issue"/> is null.</exception>
    public static IReadOnlyCollection<KeyValuePair<string, string>> GetMetadataPairs(this PerformanceIssue issue)
    {
        ArgumentNullException.ThrowIfNull(issue);

        return issue.Metadata.AsReadOnly();
    }

    /// <summary>
    /// Determines whether this issue is actionable based on severity and impact.
    /// </summary>
    /// <param name="issue">The performance issue.</param>
    /// <param name="minSeverity">The minimum severity level to be considered actionable.</param>
    /// <param name="minImpact">The minimum impact percentage to be considered actionable.</param>
    /// <returns>True if the issue is actionable; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="issue"/> is null.</exception>
    public static bool IsActionable(
        this PerformanceIssue issue,
        IssueSeverity minSeverity = IssueSeverity.Warning,
        double minImpact = 10.0)
    {
        ArgumentNullException.ThrowIfNull(issue);

        return issue.Severity >= minSeverity
            && issue.EstimatedPerformanceImpact >= minImpact;
    }

    /// <summary>
    /// Gets the issue type as a formatted string.
    /// </summary>
    /// <param name="issue">The performance issue.</param>
    /// <returns>A formatted string representing the issue type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="issue"/> is null.</exception>
    public static string GetIssueTypeLabel(this PerformanceIssue issue)
    {
        ArgumentNullException.ThrowIfNull(issue);

        return issue.IssueType.ToString();
    }

    /// <summary>
    /// Gets the priority as a formatted string with emoji indicator.
    /// </summary>
    /// <param name="issue">The performance issue.</param>
    /// <returns>A formatted string with priority indicator.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="issue"/> is null.</exception>
    public static string GetPriorityLabel(this PerformanceIssue issue)
    {
        ArgumentNullException.ThrowIfNull(issue);

        return issue.Priority switch
        {
            1 => "🔴 P1 - Critical",
            2 => "🟠 P2 - High",
            3 => "🟡 P3 - Medium",
            4 => "🔵 P4 - Low",
            _ => $"P{issue.Priority} - Unknown"
        };
    }

    /// <summary>
    /// Filters a collection of performance issues by severity.
    /// </summary>
    /// <param name="issues">The collection of performance issues.</param>
    /// <param name="severity">The minimum severity level to include.</param>
    /// <returns>An enumerable of performance issues filtered by severity.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="issues"/> is null.</exception>
    public static IEnumerable<PerformanceIssue> FilterBySeverity(
        this IEnumerable<PerformanceIssue> issues,
        IssueSeverity severity)
    {
        ArgumentNullException.ThrowIfNull(issues);

        return issues.Where(i => i.Severity >= severity);
    }

    /// <summary>
    /// Filters a collection of performance issues by minimum impact percentage.
    /// </summary>
    /// <param name="issues">The collection of performance issues.</param>
    /// <param name="minImpact">The minimum impact percentage to include.</param>
    /// <returns>An enumerable of performance issues filtered by impact.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="issues"/> is null.</exception>
    public static IEnumerable<PerformanceIssue> FilterByImpact(
        this IEnumerable<PerformanceIssue> issues,
        double minImpact = 5.0)
    {
        ArgumentNullException.ThrowIfNull(issues);

        return issues.Where(i => i.EstimatedPerformanceImpact >= minImpact);
    }

    /// <summary>
    /// Orders a collection of performance issues by priority (descending).
    /// </summary>
    /// <param name="issues">The collection of performance issues.</param>
    /// <returns>An ordered enumerable of performance issues ordered by priority.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="issues"/> is null.</exception>
    public static IOrderedEnumerable<PerformanceIssue> OrderByPriority(
        this IEnumerable<PerformanceIssue> issues)
    {
        ArgumentNullException.ThrowIfNull(issues);

        return issues.OrderByDescending(i => i.Priority)
            .ThenByDescending(i => i.EstimatedPerformanceImpact);
    }

    /// <summary>
    /// Groups performance issues by their issue type.
    /// </summary>
    /// <param name="issues">The collection of performance issues.</param>
    /// <returns>A dictionary grouping issues by their type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="issues"/> is null.</exception>
    public static IReadOnlyDictionary<IssueType, IReadOnlyList<PerformanceIssue>> GroupByIssueType(
        this IEnumerable<PerformanceIssue> issues)
    {
        ArgumentNullException.ThrowIfNull(issues);

        return issues.GroupBy(i => i.IssueType)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<PerformanceIssue>)g.ToList().AsReadOnly()
            );
    }
}