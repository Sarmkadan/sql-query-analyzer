#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Constants;
using SqlQueryAnalyzer.Models;
using SqlQueryAnalyzer.Services;
using SqlQueryAnalyzer.Utilities;

namespace SqlQueryAnalyzer.Examples;

/// Demonstrates generating analysis reports in multiple formats
public class ReportGenerationExample
{
    private readonly IQueryAnalyzerService _analyzer;
    private readonly ILogger<ReportGenerationExample> _logger;

    public ReportGenerationExample(IQueryAnalyzerService analyzer, ILogger<ReportGenerationExample> logger)
    {
        _analyzer = analyzer;
        _logger = logger;
    }

    public async Task RunExample()
    {
        _logger.LogInformation("Report Generation Example");
        _logger.LogInformation("=========================\n");

        // Complex query for analysis
        var query = @"
            SELECT
                c.CustomerId, c.CustomerName, c.Email,
                COUNT(o.OrderId) as TotalOrders,
                SUM(o.OrderAmount) as TotalSpent,
                AVG(o.OrderAmount) as AvgOrderValue,
                MAX(o.OrderDate) as LastOrderDate
            FROM Customers c
            LEFT JOIN Orders o ON c.CustomerId = o.CustomerId
            WHERE c.Country IN ('USA', 'Canada')
                AND c.CreatedDate >= DATEADD(YEAR, -1, GETDATE())
                AND DATEDIFF(DAY, o.OrderDate, GETDATE()) <= 90
            GROUP BY c.CustomerId, c.CustomerName, c.Email
            HAVING COUNT(o.OrderId) > 0
            ORDER BY TotalSpent DESC
        ";

        _logger.LogInformation("Analyzing query...\n");

        var result = await _analyzer.AnalyzeQueryAsync(query).ConfigureAwait(false);

        // Create output directory
        var outputDir = "./reports";
        Directory.CreateDirectory(outputDir);

        // Generate all report formats
        await GenerateAllReports(result, outputDir, _logger).ConfigureAwait(false);

        _logger.LogInformation("\n✓ All reports generated in '{OutputDir}' directory", outputDir);
    }

    private static async Task GenerateAllReports(
        QueryAnalysisResult result,
        string outputDir,
        ILogger logger)
    {
        // Generate Text Report
        logger.LogInformation("Generating text report...");
        var textReport = ReportGenerator.GenerateTextReport(result);
        var textPath = Path.Combine(outputDir, "analysis.txt");
        await File.WriteAllTextAsync(textPath, textReport).ConfigureAwait(false);
        logger.LogInformation("  ✓ Saved to {TextPath}", textPath);

        // Generate HTML Report
        logger.LogInformation("Generating HTML report...");
        var htmlReport = ReportGenerator.GenerateHtmlReport(result);
        var htmlPath = Path.Combine(outputDir, "analysis.html");
        await File.WriteAllTextAsync(htmlPath, htmlReport).ConfigureAwait(false);
        logger.LogInformation("  ✓ Saved to {HtmlPath}", htmlPath);
        logger.LogInformation($"  → Open in browser: file://{Path.GetFullPath(htmlPath)}");

        // Generate JSON Report
        logger.LogInformation("Generating JSON report...");
        var jsonReport = ReportGenerator.GenerateJsonReport(result);
        var jsonPath = Path.Combine(outputDir, "analysis.json");
        await File.WriteAllTextAsync(jsonPath, jsonReport).ConfigureAwait(false);
        logger.LogInformation("  ✓ Saved to {JsonPath}", jsonPath);

        // Generate CSV Report
        logger.LogInformation("Generating CSV report...");
        var csvReport = ReportGenerator.GenerateCsvReport(result);
        var csvPath = Path.Combine(outputDir, "analysis.csv");
        await File.WriteAllTextAsync(csvPath, csvReport).ConfigureAwait(false);
        logger.LogInformation("  ✓ Saved to {CsvPath}", csvPath);

        // Generate Custom Summary
        logger.LogInformation("Generating executive summary...");
        var summary = GenerateExecutiveSummary(result);
        var summaryPath = Path.Combine(outputDir, "summary.md");
        await File.WriteAllTextAsync(summaryPath, summary).ConfigureAwait(false);
        logger.LogInformation("  ✓ Saved to {SummaryPath}", summaryPath);

        // Generate Recommendations
        logger.LogInformation("Generating recommendations report...");
        var recommendations = GenerateRecommendations(result);
        var recPath = Path.Combine(outputDir, "recommendations.txt");
        await File.WriteAllTextAsync(recPath, recommendations).ConfigureAwait(false);
        logger.LogInformation("  ✓ Saved to {RecPath}", recPath);
    }

    private static string GenerateExecutiveSummary(QueryAnalysisResult result)
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine("# Query Analysis Executive Summary");
        sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();

        // Score
        sb.AppendLine($"## Performance Score: {result.PerformanceScore:F1}/100");
        sb.AppendLine($"**Complexity:** {result.Complexity}");
        sb.AppendLine();

        // Issues Summary
        if (result.Issues.Count > 0)
        {
            sb.AppendLine($"## Issues Found: {result.Issues.Count}");
            sb.AppendLine();

            var critical = result.Issues.Count(i => i.Severity == IssueSeverity.Critical);
            var warnings = result.Issues.Count(i => i.Severity == IssueSeverity.Warning);
            var info = result.Issues.Count(i => i.Severity == IssueSeverity.Info);

            sb.AppendLine($"| Severity | Count |");
            sb.AppendLine($"|----------|-------|");
            sb.AppendLine($"| Critical | {critical} |");
            sb.AppendLine($"| Warning  | {warnings} |");
            sb.AppendLine($"| Info     | {info} |");
            sb.AppendLine();
        }

        // Top Issues
        if (result.Issues.Count > 0)
        {
            sb.AppendLine("## Top Issues");
            sb.AppendLine();

            foreach (var issue in result.Issues
                .OrderByDescending(i => i.Severity)
                .ThenByDescending(i => i.EstimatedPerformanceImpact) // Assuming ImpactScore was meant to be EstimatedPerformanceImpact
                .Take(5))
            {
                sb.AppendLine($"### {issue.IssueType}");
                sb.AppendLine($"- **Severity:** {issue.Severity}");
                sb.AppendLine($"- **Impact:** {issue.EstimatedPerformanceImpact}/10"); // Assuming ImpactScore was meant to be EstimatedPerformanceImpact
                sb.AppendLine($"- **Description:** {issue.Description}");
                sb.AppendLine($"- **Recommendation:** {issue.RecommendedFix}");
                sb.AppendLine();
            }
        }

        // Index Suggestions
        if (result.IndexSuggestions.Count > 0)
        {
            sb.AppendLine("## Index Optimization Opportunities");
            sb.AppendLine();

            foreach (var suggestion in result.IndexSuggestions
                .OrderByDescending(s => s.EstimatedPerformanceGain) // Assuming Roi was meant to be EstimatedPerformanceGain
                .Take(3))
            {
                sb.AppendLine($"### {suggestion.IndexName}"); // Assuming SuggestedIndexName was meant to be IndexName
                sb.AppendLine($"- **Columns:** {string.Join(", ", suggestion.IndexColumns)}"); // Assuming Columns was meant to be IndexColumns
                sb.AppendLine($"- **Estimated Gain:** {suggestion.EstimatedPerformanceGain:F1}%"); // Assuming Roi was meant to be EstimatedPerformanceGain
                sb.AppendLine($"- **Estimated Size:** {suggestion.EstimatedSizeKB} KB"); // Assuming EstimatedSizeKB property is available
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    private static string GenerateRecommendations(QueryAnalysisResult result)
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine("QUERY ANALYSIS RECOMMENDATIONS");
        sb.AppendLine("==============================");
        sb.AppendLine();
        sb.AppendLine($"Analysis Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Performance Score: {result.PerformanceScore:F1}/100");
        sb.AppendLine();

        // Priority recommendations
        sb.AppendLine("PRIORITY ACTIONS:");
        sb.AppendLine("-----------------");

        var criticalIssues = result.Issues.Where(i => i.Severity == IssueSeverity.Critical).ToList();

        if (criticalIssues.Count > 0)
        {
            sb.AppendLine("\n🔴 CRITICAL - Address Immediately:");
            foreach (var issue in criticalIssues)
            {
                sb.AppendLine($"\n  • {issue.IssueType}");
                sb.AppendLine($"    Problem: {issue.Description}");
                sb.AppendLine($"    Solution: {issue.RecommendedFix}");
            }
        }
        else
        {
            sb.AppendLine("\n✓ No critical issues found");
        }

        // Secondary recommendations
        var warnings = result.Issues.Where(i => i.Severity == IssueSeverity.Warning).ToList();

        if (warnings.Count > 0)
        {
            sb.AppendLine($"\n🟡 WARNINGS - Review and Implement (Total: {warnings.Count}):");
            foreach (var warning in warnings.Take(5))
            {
                sb.AppendLine($"\n  • {warning.IssueType}");
                sb.AppendLine($"    {warning.RecommendedFix}");
            }

            if (warnings.Count > 5)
            {
                sb.AppendLine($"\n  ... and {warnings.Count - 5} more warnings");
            }
        }

        // Index recommendations
        if (result.IndexSuggestions.Count > 0)
        {
            sb.AppendLine($"\n📊 INDEX OPTIMIZATION ({result.IndexSuggestions.Count} suggestions):");
            sb.AppendLine($"\nTop ROI Opportunities:");

            foreach (var suggestion in result.IndexSuggestions
                .OrderByDescending(s => s.EstimatedPerformanceGain) // Assuming Roi was meant to be EstimatedPerformanceGain
                .Take(3))
            {
                sb.AppendLine($"\n  CREATE INDEX {suggestion.IndexName}"); // Assuming SuggestedIndexName was meant to be IndexName
                sb.AppendLine($"  ON {suggestion.TableName} ({string.Join(", ", suggestion.IndexColumns)})"); // Assuming Columns was meant to be IndexColumns
                sb.AppendLine($"  -- Estimated ROI: {suggestion.EstimatedPerformanceGain:F1}%, Size: {suggestion.EstimatedSizeKB} KB"); // Assuming Roi and EstimatedSizeKB
            }
        }

        sb.AppendLine("\n\nFor detailed analysis, see generated reports (HTML, JSON, CSV)");

        return sb.ToString();
    }
}
