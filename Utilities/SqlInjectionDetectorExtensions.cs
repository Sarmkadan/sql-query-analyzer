#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlQueryAnalyzer.Utilities;

/// <summary>
/// Extension methods for SqlInjectionDetector providing additional functionality
/// for vulnerability analysis, result processing, and reporting.
/// </summary>
public static class SqlInjectionDetectorExtensions
{
    /// <summary>
    /// Filters detected vulnerabilities by severity level.
    /// Returns only issues with the specified severity or higher.
    /// </summary>
    /// <param name="detector">The detector instance</param>
    /// <param name="issues">List of detected vulnerabilities</param>
    /// <param name="minSeverity">Minimum severity level to include (Critical, High, Medium, Low)</param>
    /// <returns>Filtered list of vulnerabilities</returns>
    public static List<SqlInjectionIssue> FilterBySeverity(
        this SqlInjectionDetector detector,
        List<SqlInjectionIssue> issues,
        string minSeverity = "Medium")
    {
        if (detector == null)
            throw new ArgumentNullException(nameof(detector));

        if (issues == null)
            throw new ArgumentNullException(nameof(issues));

        var severityLevels = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            {"Critical", 4},
            {"High", 3},
            {"Medium", 2},
            {"Low", 1}
        };

        if (!severityLevels.TryGetValue(minSeverity, out var minLevel))
            throw new ArgumentException($"Invalid severity level: {minSeverity}. Expected: Critical, High, Medium, or Low");

        return issues.Where(issue =>
            severityLevels.TryGetValue(issue.Severity, out var issueLevel) &&
            issueLevel >= minLevel
        ).ToList();
    }

    /// <summary>
    /// Groups detected vulnerabilities by their type.
    /// Returns a dictionary mapping vulnerability types to lists of issues.
    /// </summary>
    /// <param name="detector">The detector instance</param>
    /// <param name="issues">List of detected vulnerabilities</param>
    /// <returns>Dictionary grouping issues by type</returns>
    public static Dictionary<string, List<SqlInjectionIssue>> GroupByType(
        this SqlInjectionDetector detector,
        List<SqlInjectionIssue> issues)
    {
        if (detector == null)
            throw new ArgumentNullException(nameof(detector));

        if (issues == null)
            throw new ArgumentNullException(nameof(issues));

        return issues
            .GroupBy(issue => issue.Type)
            .OrderByDescending(group => group.Count())
            .ToDictionary(
                group => group.Key,
                group => group.ToList()
            );
    }

    /// <summary>
    /// Generates a summary report of detected vulnerabilities.
    /// Includes counts by severity and a brief overview of issues.
    /// </summary>
    /// <param name="detector">The detector instance</param>
    /// <param name="issues">List of detected vulnerabilities</param>
    /// <returns>Formatted summary report string</returns>
    public static string GenerateSummaryReport(
        this SqlInjectionDetector detector,
        List<SqlInjectionIssue> issues)
    {
        if (detector == null)
            throw new ArgumentNullException(nameof(detector));

        if (issues == null)
            throw new ArgumentNullException(nameof(issues));

        var report = new StringBuilder();
        report.AppendLine("=== SQL Injection Detection Summary Report ===");
        report.AppendLine();

        var totalIssues = issues.Count;
        var criticalIssues = issues.Count(i => string.Equals(i.Severity, "Critical", StringComparison.OrdinalIgnoreCase));
        var highIssues = issues.Count(i => string.Equals(i.Severity, "High", StringComparison.OrdinalIgnoreCase));
        var mediumIssues = issues.Count(i => string.Equals(i.Severity, "Medium", StringComparison.OrdinalIgnoreCase));
        var lowIssues = issues.Count(i => string.Equals(i.Severity, "Low", StringComparison.OrdinalIgnoreCase));

        report.AppendLine($"Total Issues: {totalIssues}");
        report.AppendLine($"Critical: {criticalIssues}");
        report.AppendLine($"High: {highIssues}");
        report.AppendLine($"Medium: {mediumIssues}");
        report.AppendLine($"Low: {lowIssues}");
        report.AppendLine();

        if (totalIssues > 0)
        {
            var groupedByType = detector.GroupByType(issues);
            report.AppendLine("Issues by Type:");
            foreach (var group in groupedByType)
            {
                report.AppendLine($"  {group.Key}: {group.Value.Count}");
            }
            report.AppendLine();

            report.AppendLine("Top 5 Most Common Patterns:");
            var topPatterns = issues
                .GroupBy(i => i.Pattern)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => new { Pattern = g.Key, Count = g.Count() })
                .ToList();

            foreach (var patternGroup in topPatterns)
            {
                report.AppendLine($"  {patternGroup.Count}x: {patternGroup.Pattern}");
            }
        }
        else
        {
            report.AppendLine("No SQL injection vulnerabilities detected.");
        }

        report.AppendLine();
        report.AppendLine("=== End of Report ===");

        return report.ToString();
    }

    /// <summary>
    /// Generates a detailed analysis report with location information.
    /// Includes line numbers and formatted output for each issue.
    /// </summary>
    /// <param name="detector">The detector instance</param>
    /// <param name="issues">List of detected vulnerabilities</param>
    /// <param name="query">The original query being analyzed</param>
    /// <returns>Formatted detailed report string</returns>
    public static string GenerateDetailedReport(
        this SqlInjectionDetector detector,
        List<SqlInjectionIssue> issues,
        string query)
    {
        if (detector == null)
            throw new ArgumentNullException(nameof(detector));

        if (issues == null)
            throw new ArgumentNullException(nameof(issues));

        if (query == null)
            throw new ArgumentNullException(nameof(query));

        var report = new StringBuilder();
        report.AppendLine("=== SQL Injection Detailed Analysis Report ===");
        report.AppendLine();

        if (issues.Count == 0)
        {
            report.AppendLine("✓ No SQL injection vulnerabilities detected.");
            report.AppendLine();
            report.AppendLine("=== End of Report ===");
            return report.ToString();
        }

        report.AppendLine($"Total Issues Found: {issues.Count}");
        report.AppendLine();

        // Sort issues by location for better readability
        var sortedIssues = issues.OrderBy(i => i.Location).ToList();

        foreach (var issue in sortedIssues)
        {
            report.AppendLine($"Issue #{sortedIssues.IndexOf(issue) + 1}:");
            report.AppendLine($"  Type: {issue.Type}");
            report.AppendLine($"  Severity: {issue.Severity}");
            report.AppendLine($"  Location: Index {issue.Location}");

            // Calculate approximate line number
            var linesBefore = query.Substring(0, Math.Min(issue.Location, query.Length)).Split('\n').Length;
            report.AppendLine($"  Line: ~{linesBefore}");

            report.AppendLine($"  Pattern: {issue.Pattern}");
            report.AppendLine($"  Description: {issue.Description}");
            report.AppendLine();
        }

        // Add summary statistics
        var severityStats = sortedIssues
            .GroupBy(i => i.Severity)
            .Select(g => new { Severity = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToList();

        report.AppendLine("=== Severity Distribution ===");
        foreach (var stat in severityStats)
        {
            report.AppendLine($"{stat.Severity}: {stat.Count}");
        }

        report.AppendLine();
        report.AppendLine("=== End of Report ===");

        return report.ToString();
    }

    /// <summary>
    /// Checks if any critical or high severity vulnerabilities were detected.
    /// Returns true if dangerous issues exist, false otherwise.
    /// </summary>
    /// <param name="detector">The detector instance</param>
    /// <param name="issues">List of detected vulnerabilities</param>
    /// <returns>True if critical/high severity issues exist</returns>
    public static bool HasCriticalIssues(
        this SqlInjectionDetector detector,
        List<SqlInjectionIssue> issues)
    {
        if (detector == null)
            throw new ArgumentNullException(nameof(detector));

        if (issues == null)
            throw new ArgumentNullException(nameof(issues));

        return issues.Any(i =>
            string.Equals(i.Severity, "Critical", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(i.Severity, "High", StringComparison.OrdinalIgnoreCase)
        );
    }
}