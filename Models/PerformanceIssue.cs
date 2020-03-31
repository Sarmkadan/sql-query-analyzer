#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SqlQueryAnalyzer.Constants;

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Represents a detected performance issue in a SQL query.
/// </summary>
public sealed class PerformanceIssue
{
    /// <summary>
    /// Gets or sets the unique identifier for the issue.
    /// </summary>
    public string IssueId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the type of the performance issue.
    /// </summary>
    public IssueType IssueType { get; set; }

    /// <summary>
    /// Gets or sets the severity of the performance issue.
    /// </summary>
    public IssueSeverity Severity { get; set; }

    /// <summary>
    /// Gets or sets the description of the performance issue.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the affected SQL clause (e.g., "WHERE", "JOIN").
    /// </summary>
    public string AffectedClause { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the line number where the issue was detected.
    /// </summary>
    public int LineNumber { get; set; }

    /// <summary>
    /// Gets or sets the column number where the issue was detected.
    /// </summary>
    public int ColumnNumber { get; set; }

    /// <summary>
    /// Gets or sets the estimated performance impact (0-100 scale).
    /// </summary>
    public double EstimatedPerformanceImpact { get; set; }

    /// <summary>
    /// Gets the performance impact score (alias for <see cref="EstimatedPerformanceImpact"/>).
    /// </summary>
    public double ImpactScore => EstimatedPerformanceImpact;

    /// <summary>
    /// Gets or sets the estimated number of rows affected by the issue.
    /// </summary>
    public int? AffectedRowCount { get; set; }

    /// <summary>
    /// Gets or sets the estimated time increase caused by the issue.
    /// </summary>
    public TimeSpan? EstimatedTimeIncrease { get; set; }

    /// <summary>
    /// Gets or sets the recommended fix for the performance issue.
    /// </summary>
    public string RecommendedFix { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an example of how to fix the issue.
    /// </summary>
    public string ExampleFix { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the priority of the issue (1 = highest).
    /// </summary>
    public int Priority { get; set; } = 1;

    /// <summary>
    /// Gets or sets additional metadata for the issue.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = [];

    /// <summary>
    /// Gets or sets the timestamp when the issue was detected.
    /// </summary>
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Validates the performance issue data.
    /// </summary>
    /// <returns>True if the issue data is valid; otherwise, false.</returns>
    public bool IsValid() =>
        !string.IsNullOrWhiteSpace(Description) &&
        EstimatedPerformanceImpact >= 0 &&
        EstimatedPerformanceImpact <= 100 &&
        Priority >= 1 &&
        Priority <= 5;

    /// <summary>
    /// Gets the human-readable severity label.
    /// </summary>
    /// <returns>A string representing the severity label.</returns>
    public string GetSeverityLabel() => Severity switch
    {
        IssueSeverity.Critical => "🔴 CRITICAL",
        IssueSeverity.Warning => "🟡 WARNING",
        IssueSeverity.Info => "ℹ️ INFO",
        _ => "UNKNOWN"
    };

    /// <summary>
    /// Formats the issue for display.
    /// </summary>
    /// <returns>A formatted message string.</returns>
    public string GetFormattedMessage() =>
        $"{GetSeverityLabel()} [{IssueType}] at line {LineNumber}: {Description}";

    /// <summary>
    /// Gets a value indicating whether this issue is critical.
    /// </summary>
    public bool IsCritical => Severity == IssueSeverity.Critical;

    /// <summary>
    /// Compares this issue with another issue by priority.
    /// </summary>
    /// <param name="other">The other issue to compare with.</param>
    /// <returns>A value indicating the relative order of the issues.</returns>
    public int ComparePriority(PerformanceIssue other)
    {
        // First compare by severity
        var severityComparison = other.Severity.CompareTo(Severity);
        if (severityComparison != 0) return severityComparison;

        // Then by impact
        return other.EstimatedPerformanceImpact.CompareTo(EstimatedPerformanceImpact);
    }
}
