// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Services;
using SqlQueryAnalyzer.Utilities;

namespace SqlQueryAnalyzer.Examples;

/// Demonstrates generating analysis reports in multiple formats
class ReportGeneration
{
    static async Task Main()
    {
        var services = new ServiceCollection()
            .AddLogging(config => config.AddConsole())
            .AddScoped<IQueryAnalyzerService, QueryAnalyzerService>()
            .BuildServiceProvider();

        var analyzer = services.GetRequiredService<IQueryAnalyzerService>();
        var logger = services.GetRequiredService<ILogger<ReportGeneration>>();

        logger.LogInformation("Report Generation Example");
        logger.LogInformation("=========================\n");

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

        logger.LogInformation("Analyzing query...\n");

        var result = await analyzer.AnalyzeQueryAsync(query);

        // Create output directory
        var outputDir = "./reports";
        Directory.CreateDirectory(outputDir);

        // Generate all report formats
        await GenerateAllReports(result, outputDir, logger);

        logger.LogInformation($"\n✓ All reports generated in '{outputDir}' directory");
    }

    static async Task GenerateAllReports(
        QueryAnalysisResult result,
        string outputDir,
        ILogger logger)
    {
        // Generate Text Report
        logger.LogInformation("Generating text report...");
        var textReport = ReportGenerator.GenerateTextReport(result);
        var textPath = Path.Combine(outputDir, "analysis.txt");
        await File.WriteAllTextAsync(textPath, textReport);
        logger.LogInformation($"  ✓ Saved to {textPath}");

        // Generate HTML Report
        logger.LogInformation("Generating HTML report...");
        var htmlReport = ReportGenerator.GenerateHtmlReport(result);
        var htmlPath = Path.Combine(outputDir, "analysis.html");
        await File.WriteAllTextAsync(htmlPath, htmlReport);
        logger.LogInformation($"  ✓ Saved to {htmlPath}");
        logger.LogInformation($"  → Open in browser: file://{Path.GetFullPath(htmlPath)}");

        // Generate JSON Report
        logger.LogInformation("Generating JSON report...");
        var jsonReport = ReportGenerator.GenerateJsonReport(result);
        var jsonPath = Path.Combine(outputDir, "analysis.json");
        await File.WriteAllTextAsync(jsonPath, jsonReport);
        logger.LogInformation($"  ✓ Saved to {jsonPath}");

        // Generate CSV Report
        logger.LogInformation("Generating CSV report...");
        var csvReport = ReportGenerator.GenerateCsvReport(result);
        var csvPath = Path.Combine(outputDir, "analysis.csv");
        await File.WriteAllTextAsync(csvPath, csvReport);
        logger.LogInformation($"  ✓ Saved to {csvPath}");

        // Generate Custom Summary
        logger.LogInformation("Generating executive summary...");
        var summary = GenerateExecutiveSummary(result);
        var summaryPath = Path.Combine(outputDir, "summary.md");
        await File.WriteAllTextAsync(summaryPath, summary);
        logger.LogInformation($"  ✓ Saved to {summaryPath}");

        // Generate Recommendations
        logger.LogInformation("Generating recommendations report...");
        var recommendations = GenerateRecommendations(result);
        var recPath = Path.Combine(outputDir, "recommendations.txt");
        await File.WriteAllTextAsync(recPath, recommendations);
        logger.LogInformation($"  ✓ Saved to {recPath}");
    }

    static string GenerateExecutiveSummary(QueryAnalysisResult result)
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
                .ThenByDescending(i => i.ImpactScore)
                .Take(5))
            {
                sb.AppendLine($"### {issue.IssueType}");
                sb.AppendLine($"- **Severity:** {issue.Severity}");
                sb.AppendLine($"- **Impact:** {issue.ImpactScore}/10");
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
                .OrderByDescending(s => s.Roi)
                .Take(3))
            {
                sb.AppendLine($"### {suggestion.SuggestedIndexName}");
                sb.AppendLine($"- **Columns:** {string.Join(", ", suggestion.Columns)}");
                sb.AppendLine($"- **Estimated ROI:** {suggestion.Roi:F1}%");
                sb.AppendLine($"- **Estimated Size:** {suggestion.EstimatedSizeKB} KB");
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    static string GenerateRecommendations(QueryAnalysisResult result)
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
                .OrderByDescending(s => s.Roi)
                .Take(3))
            {
                sb.AppendLine($"\n  CREATE INDEX {suggestion.SuggestedIndexName}");
                sb.AppendLine($"  ON {suggestion.TableName} ({string.Join(", ", suggestion.Columns)})");
                sb.AppendLine($"  -- Estimated ROI: {suggestion.Roi:F1}%, Size: {suggestion.EstimatedSizeKB} KB");
            }
        }

        sb.AppendLine("\n\nFor detailed analysis, see generated reports (HTML, JSON, CSV)");

        return sb.ToString();
    }
}
