// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SqlQueryAnalyzer.Constants;

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Represents a detected performance issue in a SQL query
/// </summary>
public class PerformanceIssue
{
    public string IssueId { get; set; } = Guid.NewGuid().ToString();
    public IssueType IssueType { get; set; }
    public IssueSeverity Severity { get; set; }
    public string Description { get; set; } = string.Empty;
    public string AffectedClause { get; set; } = string.Empty; // e.g., "WHERE", "JOIN"
    public int LineNumber { get; set; }
    public int ColumnNumber { get; set; }

    // Impact metrics
    public double EstimatedPerformanceImpact { get; set; } // 0-100 scale
    public int? AffectedRowCount { get; set; }
    public TimeSpan? EstimatedTimeIncrease { get; set; }

    // Resolution information
    public string RecommendedFix { get; set; } = string.Empty;
    public string ExampleFix { get; set; } = string.Empty;
    public int Priority { get; set; } = 1; // 1 = highest

    // Metadata
    public Dictionary<string, string> Metadata { get; set; } = [];
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;

    // Validate issue data
    public bool IsValid() =>
        !string.IsNullOrWhiteSpace(Description) &&
        EstimatedPerformanceImpact >= 0 &&
        EstimatedPerformanceImpact <= 100 &&
        Priority >= 1 &&
        Priority <= 5;

    // Get human-readable severity
    public string GetSeverityLabel() => Severity switch
    {
        IssueSeverity.Critical => "🔴 CRITICAL",
        IssueSeverity.Warning => "🟡 WARNING",
        IssueSeverity.Info => "ℹ️ INFO",
        _ => "UNKNOWN"
    };

    // Format issue for display
    public string GetFormattedMessage() =>
        $"{GetSeverityLabel()} [{IssueType}] at line {LineNumber}: {Description}";

    // Check if this issue is critical
    public bool IsCritical => Severity == IssueSeverity.Critical;

    // Compare issues by priority
    public int ComparePriority(PerformanceIssue other)
    {
        // First compare by severity
        var severityComparison = other.Severity.CompareTo(Severity);
        if (severityComparison != 0) return severityComparison;

        // Then by impact
        return other.EstimatedPerformanceImpact.CompareTo(EstimatedPerformanceImpact);
    }
}
