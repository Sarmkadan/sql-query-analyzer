#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SqlQueryAnalyzer.Constants;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Formatters;

/// <summary>
/// Markdown formatter for creating detailed analysis reports in Markdown format.
/// Produces a structured Markdown document with tables, summaries, and code blocks.
/// </summary>
public class MarkdownResultFormatter : IResultFormatter
{
    public string Format(QueryAnalysisResult result)
    {
        var lines = new List<string>();

        // Header
        lines.Add("# SQL Query Analysis Report");
        lines.Add("");
        lines.Add($"**Query ID:** `{result.QueryId}`");
        lines.Add($"**Score:** `{result.PerformanceScore:F1}/100`");
        lines.Add($"**Complexity:** `{result.Complexity}`");
        lines.Add($"**Analyzed:** `{result.AnalyzedAt:yyyy-MM-dd HH:mm:ss} UTC`");
        lines.Add("");

        // Summary section
        lines.Add("## Summary");
        lines.Add("");

        var criticalCount = result.Issues.Count(i => i.Severity == IssueSeverity.Critical);
        var warningCount = result.Issues.Count(i => i.Severity == IssueSeverity.Warning);
        var infoCount = result.Issues.Count(i => i.Severity == IssueSeverity.Info);

        lines.Add("| Metric | Value |");
        lines.Add("|--------|-------|");
        lines.Add($"| Total Issues | {result.Issues.Count} |");
        lines.Add($"| Critical Issues | {criticalCount} |");
        lines.Add($"| Warning Issues | {warningCount} |");
        lines.Add($"| Info Issues | {infoCount} |");
        lines.Add($"| Index Suggestions | {result.IndexSuggestions.Count} |");
        lines.Add($"| Optimization Potential | {result.TotalOptimizationPotential:F1}% |");
        lines.Add("");

        // Issues table
        lines.Add("## Issues Found");
        lines.Add("");

        if (result.Issues.Count == 0)
        {
            lines.Add("✅ No issues detected. Query looks good!");
        }
        else
        {
            lines.Add("| Severity | Rule | Message |");
            lines.Add("|----------|------|---------|");

            foreach (var issue in result.Issues.OrderByDescending(i => i.Severity).ThenByDescending(i => i.EstimatedPerformanceImpact))
            {
                var severityEmoji = issue.Severity switch
                {
                    IssueSeverity.Critical => "🔴",
                    IssueSeverity.Warning => "🟡",
                    IssueSeverity.Info => "ℹ️",
                    _ => "⚪"
                };

                var severityLabel = issue.Severity.ToString();
                var rule = issue.IssueType.ToString();
                var message = issue.Description.Replace("|", "‖"); // Escape pipes in markdown

                lines.Add($"| {severityEmoji} {severityLabel} | `{rule}` | {message} |");
            }
        }
        lines.Add("");

        // Index suggestions
        if (result.IndexSuggestions.Count > 0)
        {
            lines.Add("## Index Suggestions");
            lines.Add("");
            lines.Add("| Table | Column | Estimated Gain |");
            lines.Add("|-------|--------|----------------|");

            foreach (var suggestion in result.IndexSuggestions.OrderByDescending(s => s.EstimatedPerformanceGain))
            {
                var table = suggestion.TableName.Replace("|", "‖");
                var column = suggestion.ColumnName?.Replace("|", "‖") ?? "N/A";
                var gain = $"{suggestion.EstimatedPerformanceGain:F1}%";

                lines.Add($"| `{table}` | `{column}` | {gain} |");
            }
            lines.Add("");
        }

        // SQL Query section
        lines.Add("## SQL Query");
        lines.Add("");
        lines.Add("```sql");
        lines.Add(result.Query);
        lines.Add("```");
        lines.Add("");

        // Additional information
        lines.Add("## Additional Information");
        lines.Add("");
        lines.Add("| Metric | Value |");
        lines.Add("|--------|-------|");
        lines.Add($"| Complexity Score | {result.ComplexityScore} |");
        lines.Add($"| Has Critical Issues | {result.HasCriticalIssues} |");
        lines.Add($"| Estimated Execution Time | {result.EstimatedExecutionTime.TotalMilliseconds:F0} ms |");
        lines.Add("");

        return string.Join("\n", lines);
    }

    public string FormatBatch(IEnumerable<QueryAnalysisResult> results)
    {
        var resultsList = results.ToList();
        var lines = new List<string>();

        // Header
        lines.Add("# SQL Query Analysis - Batch Report");
        lines.Add("");
        lines.Add($"**Generated:** `{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC`");
        lines.Add($"**Total Queries Analyzed:** `{resultsList.Count}`");
        lines.Add("");

        // Summary statistics
        lines.Add("## Summary Statistics");
        lines.Add("");

        var allIssues = resultsList.SelectMany(r => r.Issues).ToList();
        var criticalCount = allIssues.Count(i => i.Severity == IssueSeverity.Critical);
        var warningCount = allIssues.Count(i => i.Severity == IssueSeverity.Warning);
        var infoCount = allIssues.Count(i => i.Severity == IssueSeverity.Info);
        var totalOptimization = resultsList.Sum(r => r.TotalOptimizationPotential);

        lines.Add("| Metric | Count |");
        lines.Add("|--------|-------|");
        lines.Add($"| Total Issues | {allIssues.Count} |");
        lines.Add($"| Critical Issues | {criticalCount} |");
        lines.Add($"| Warning Issues | {warningCount} |");
        lines.Add($"| Info Issues | {infoCount} |");
        lines.Add($"| Total Index Suggestions | {resultsList.Sum(r => r.IndexSuggestions.Count)} |");
        lines.Add($"| Total Optimization Potential | {totalOptimization:F1}% |");
        lines.Add("");

        // Individual query results
        lines.Add("## Query Details");
        lines.Add("");

        foreach (var result in resultsList)
        {
            var criticalCountSingle = result.Issues.Count(i => i.Severity == IssueSeverity.Critical);
            var warningCountSingle = result.Issues.Count(i => i.Severity == IssueSeverity.Warning);
            var infoCountSingle = result.Issues.Count(i => i.Severity == IssueSeverity.Info);

            lines.Add($"### {result.QueryId.Substring(0, Math.Min(20, result.QueryId.Length))}");
            lines.Add("");
            lines.Add("| Metric | Value |");
            lines.Add("|--------|-------|");
            lines.Add($"| Score | {result.PerformanceScore:F1}/100 |");
            lines.Add($"| Complexity | {result.Complexity} |");
            lines.Add($"| Issues | {result.Issues.Count} (C:{criticalCountSingle}, W:{warningCountSingle}, I:{infoCountSingle}) |");
            lines.Add($"| Index Suggestions | {result.IndexSuggestions.Count} |");
            lines.Add($"| Optimization Potential | {result.TotalOptimizationPotential:F1}% |");
            lines.Add("");

            if (result.Issues.Count > 0)
            {
                lines.Add("#### Top Issues");
                lines.Add("");
                lines.Add("| Severity | Rule | Impact |");
                lines.Add("|----------|------|--------|");

                foreach (var issue in result.Issues.OrderByDescending(i => i.EstimatedPerformanceImpact).Take(5))
                {
                    var severityEmoji = issue.Severity switch
                    {
                        IssueSeverity.Critical => "🔴",
                        IssueSeverity.Warning => "🟡",
                        IssueSeverity.Info => "ℹ️",
                        _ => "⚪"
                    };

                    var rule = issue.IssueType.ToString();
                    var impact = $"{issue.EstimatedPerformanceImpact:F1}%";

                    lines.Add($"| {severityEmoji} | `{rule}` | {impact} |");
                }
                lines.Add("");
            }
        }

        return string.Join("\n", lines);
    }

    public string GetFormatType() => "md";
}
