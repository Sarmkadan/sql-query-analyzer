// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Utilities;

/// <summary>
/// Generates reports from analysis results
/// </summary>
public static class ReportGenerator
{
    // Generate text report
    public static string GenerateTextReport(QueryAnalysisResult analysis)
    {
        // Fix: Add missing input validation
        if (analysis == null)
            throw new ArgumentNullException(nameof(analysis), "Analysis result cannot be null.");

        var sb = new StringBuilder();

        sb.AppendLine("═════════════════════════════════════════════════════════════");
        sb.AppendLine("SQL QUERY ANALYSIS REPORT");
        sb.AppendLine("═════════════════════════════════════════════════════════════");
        sb.AppendLine();

        sb.AppendLine($"Query ID: {analysis.QueryId}");
        sb.AppendLine($"Analyzed: {analysis.AnalyzedAt:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();

        sb.AppendLine("PERFORMANCE METRICS");
        sb.AppendLine("─────────────────────────────────────────────────────────────");
        sb.AppendLine($"Performance Score: {analysis.PerformanceScore:F1}/100");
        sb.AppendLine($"Complexity Level: {analysis.Complexity}");
        sb.AppendLine($"Estimated Execution Time: {analysis.EstimatedExecutionTime.TotalMilliseconds:F0}ms");
        sb.AppendLine();

        sb.AppendLine("ISSUES DETECTED");
        sb.AppendLine("─────────────────────────────────────────────────────────────");
        if (analysis.Issues.Count == 0)
        {
            sb.AppendLine("No issues detected.");
        }
        else
        {
            var criticalCount = analysis.Issues.Count(i => i.Severity == Constants.IssueSeverity.Critical);
            var warningCount = analysis.Issues.Count(i => i.Severity == Constants.IssueSeverity.Warning);
            var infoCount = analysis.Issues.Count(i => i.Severity == Constants.IssueSeverity.Info);

            sb.AppendLine($"Total Issues: {analysis.Issues.Count}");
            sb.AppendLine($"  🔴 Critical: {criticalCount}");
            sb.AppendLine($"  🟡 Warnings: {warningCount}");
            sb.AppendLine($"  ℹ️ Info: {infoCount}");
            sb.AppendLine();

            foreach (var issue in analysis.Issues.OrderByDescending(i => i.Severity))
            {
                sb.AppendLine($"{issue.GetSeverityLabel()} - {issue.IssueType}");
                sb.AppendLine($"  Description: {issue.Description}");
                sb.AppendLine($"  Impact: {issue.EstimatedPerformanceImpact:F1}%");
                if (!string.IsNullOrEmpty(issue.RecommendedFix))
                    sb.AppendLine($"  Fix: {issue.RecommendedFix}");
                sb.AppendLine();
            }
        }

        sb.AppendLine("INDEX SUGGESTIONS");
        sb.AppendLine("─────────────────────────────────────────────────────────────");
        if (analysis.IndexSuggestions.Count == 0)
        {
            sb.AppendLine("No index suggestions at this time.");
        }
        else
        {
            sb.AppendLine($"Total Suggestions: {analysis.IndexSuggestions.Count}");
            sb.AppendLine($"Total Optimization Potential: {analysis.TotalOptimizationPotential:F1}%");
            sb.AppendLine();

            foreach (var suggestion in analysis.IndexSuggestions.OrderByDescending(s => s.EstimatedPerformanceGain))
            {
                sb.AppendLine($"Index: {suggestion.IndexName}");
                sb.AppendLine($"  Table: {suggestion.TableName}");
                sb.AppendLine($"  Columns: {string.Join(", ", suggestion.IndexColumns)}");
                sb.AppendLine($"  Estimated Gain: {suggestion.EstimatedPerformanceGain:F1}%");
                sb.AppendLine($"  Risk Level: {suggestion.GetRiskLevel()}");
                sb.AppendLine();
            }
        }

        sb.AppendLine("═════════════════════════════════════════════════════════════");

        return sb.ToString();
    }

    // Generate CSV report
    public static string GenerateCsvReport(List<QueryAnalysisResult> analyses)
    {
        var sb = new StringBuilder();

        // Header
        sb.AppendLine("QueryId,PerformanceScore,Complexity,IssueCount,CriticalIssues," +
                     "Suggestions,EstimatedGain,AnalyzedAt");

        // Data rows
        foreach (var analysis in analyses)
        {
            var criticalCount = analysis.Issues.Count(i => i.Severity == Constants.IssueSeverity.Critical);
            sb.AppendLine($"\"{analysis.QueryId}\"," +
                         $"{analysis.PerformanceScore:F1}," +
                         $"\"{analysis.Complexity}\"," +
                         $"{analysis.Issues.Count}," +
                         $"{criticalCount}," +
                         $"{analysis.IndexSuggestions.Count}," +
                         $"{analysis.TotalOptimizationPotential:F1}," +
                         $"\"{analysis.AnalyzedAt:yyyy-MM-dd HH:mm:ss}\"");
        }

        return sb.ToString();
    }

    // Generate JSON report
    public static string GenerateJsonReport(QueryAnalysisResult analysis)
    {
        var summary = new
        {
            analysis.QueryId,
            analysis.PerformanceScore,
            Complexity = analysis.Complexity.ToString(),
            analysis.EstimatedExecutionTime,
            IssueCount = analysis.Issues.Count,
            CriticalIssues = analysis.Issues.Count(i => i.Severity == Constants.IssueSeverity.Critical),
            Issues = analysis.Issues.Select(i => new
            {
                i.IssueType,
                Severity = i.Severity.ToString(),
                i.Description,
                i.EstimatedPerformanceImpact,
                i.RecommendedFix
            }),
            IndexSuggestions = analysis.IndexSuggestions.Select(s => new
            {
                s.TableName,
                s.IndexName,
                s.IndexColumns,
                s.EstimatedPerformanceGain
            }),
            analysis.AnalyzedAt
        };

        return System.Text.Json.JsonSerializer.Serialize(summary, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    // Generate HTML report
    public static string GenerateHtmlReport(QueryAnalysisResult analysis)
    {
        var sb = new StringBuilder();

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html>");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset=\"utf-8\">");
        sb.AppendLine("<title>SQL Query Analysis Report</title>");
        sb.AppendLine("<style>");
        sb.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; background: #f5f5f5; }");
        sb.AppendLine(".container { max-width: 1000px; margin: 0 auto; background: white; padding: 20px; border-radius: 5px; }");
        sb.AppendLine(".score { font-size: 36px; color: #2ecc71; font-weight: bold; }");
        sb.AppendLine(".critical { color: #e74c3c; }");
        sb.AppendLine(".warning { color: #f39c12; }");
        sb.AppendLine("table { width: 100%; border-collapse: collapse; margin: 10px 0; }");
        sb.AppendLine("th, td { padding: 10px; text-align: left; border-bottom: 1px solid #ddd; }");
        sb.AppendLine("th { background: #34495e; color: white; }");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");

        sb.AppendLine("<div class=\"container\">");
        sb.AppendLine("<h1>SQL Query Analysis Report</h1>");
        sb.AppendLine($"<p>Query ID: {analysis.QueryId} | Analyzed: {analysis.AnalyzedAt:yyyy-MM-dd HH:mm:ss}</p>");

        sb.AppendLine("<h2>Performance Metrics</h2>");
        sb.AppendLine($"<p class=\"score\">Score: {analysis.PerformanceScore:F1}/100</p>");
        sb.AppendLine($"<p>Complexity: {analysis.Complexity}</p>");
        sb.AppendLine($"<p>Estimated Execution Time: {analysis.EstimatedExecutionTime.TotalMilliseconds:F0}ms</p>");

        if (analysis.Issues.Count > 0)
        {
            sb.AppendLine("<h2>Issues</h2>");
            sb.AppendLine("<table>");
            sb.AppendLine("<tr><th>Type</th><th>Severity</th><th>Description</th><th>Impact</th></tr>");
            foreach (var issue in analysis.Issues)
            {
                var severityClass = issue.Severity == Constants.IssueSeverity.Critical ? "critical" : "warning";
                sb.AppendLine($"<tr>");
                sb.AppendLine($"<td>{issue.IssueType}</td>");
                sb.AppendLine($"<td class=\"{severityClass}\">{issue.Severity}</td>");
                sb.AppendLine($"<td>{issue.Description}</td>");
                sb.AppendLine($"<td>{issue.EstimatedPerformanceImpact:F1}%</td>");
                sb.AppendLine($"</tr>");
            }
            sb.AppendLine("</table>");
        }

        if (analysis.IndexSuggestions.Count > 0)
        {
            sb.AppendLine("<h2>Index Suggestions</h2>");
            sb.AppendLine("<table>");
            sb.AppendLine("<tr><th>Index</th><th>Table</th><th>Columns</th><th>Gain</th></tr>");
            foreach (var suggestion in analysis.IndexSuggestions)
            {
                sb.AppendLine($"<tr>");
                sb.AppendLine($"<td>{suggestion.IndexName}</td>");
                sb.AppendLine($"<td>{suggestion.TableName}</td>");
                sb.AppendLine($"<td>{string.Join(", ", suggestion.IndexColumns)}</td>");
                sb.AppendLine($"<td>{suggestion.EstimatedPerformanceGain:F1}%</td>");
                sb.AppendLine($"</tr>");
            }
            sb.AppendLine("</table>");
        }

        sb.AppendLine("</div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    // Generate executive summary
    public static string GenerateSummary(QueryAnalysisResult analysis)
    {
        var issues = analysis.Issues.Count;
        var critical = analysis.Issues.Count(i => i.Severity == Constants.IssueSeverity.Critical);
        var suggestions = analysis.IndexSuggestions.Count;

        return $"Score: {analysis.PerformanceScore:F0}/100 | " +
               $"Issues: {issues} ({critical} critical) | " +
               $"Index Suggestions: {suggestions} | " +
               $"Optimization Potential: {analysis.TotalOptimizationPotential:F1}%";
    }
}
