#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Services;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Examples;

/// Demonstrates batch analysis of multiple queries with performance metrics
class BatchAnalyzer
{
    static async Task Main()
    {
        var services = new ServiceCollection()
            .AddLogging(config => config.AddConsole())
            .AddScoped<IQueryAnalyzerService, QueryAnalyzerService>()
            .BuildServiceProvider();

        var analyzer = services.GetRequiredService<IQueryAnalyzerService>();
        var logger = services.GetRequiredService<ILogger<BatchAnalyzer>>();

        logger.LogInformation("Batch Query Analysis Example");
        logger.LogInformation("============================\n");

        // Sample queries from a real application
        var queries = new List<DatabaseQuery>
        {
            new()
            {
                QueryText = "SELECT * FROM Orders WHERE CustomerId = @customerId",
                ApplicationName = "Orders Service",
                DatabaseName = "ProductionDB"
            },
            new()
            {
                QueryText = "SELECT o.Id, o.OrderDate, c.Name FROM Orders o JOIN Customers c ON o.CustomerId = c.Id",
                ApplicationName = "Reporting Service",
                DatabaseName = "ProductionDB"
            },
            new()
            {
                QueryText = "SELECT COUNT(*) FROM OrderItems WHERE OrderId IN (SELECT Id FROM Orders WHERE OrderDate > GETDATE() - 30)",
                ApplicationName = "Analytics Service",
                DatabaseName = "ProductionDB"
            },
            new()
            {
                QueryText = "UPDATE Orders SET Status = 'Shipped' WHERE OrderDate < GETDATE() - 1 AND Status = 'Processing'",
                ApplicationName = "Order Processing",
                DatabaseName = "ProductionDB"
            },
            new()
            {
                QueryText = "SELECT o.*, c.*, p.* FROM Orders o LEFT JOIN Customers c ON o.CustomerId = c.Id LEFT JOIN Products p ON p.Id IN (SELECT DISTINCT ProductId FROM OrderItems WHERE OrderId = o.Id)",
                ApplicationName = "Portal Service",
                DatabaseName = "ProductionDB"
            }
        };

        // Analyze batch
        logger.LogInformation($"Analyzing {queries.Count} queries...\n");

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var results = await analyzer.AnalyzeQueriesAsync(queries);

            stopwatch.Stop();

            // Generate summary report
            GenerateSummaryReport(results, queries, logger, stopwatch.ElapsedMilliseconds);

            // Generate detailed breakdown
            GenerateDetailedBreakdown(results, logger);

            // Generate application-level summary
            GenerateApplicationSummary(results, queries, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Batch analysis failed");
        }
    }

    static void GenerateSummaryReport(
        List<QueryAnalysisResult> results,
        List<DatabaseQuery> queries,
        ILogger logger,
        long elapsedMs)
    {
        logger.LogInformation("\n=== BATCH SUMMARY ===\n");

        var avgScore = results.Average(r => r.PerformanceScore);
        var totalIssues = results.Sum(r => r.Issues.Count);
        var criticalIssues = results.Sum(r => r.Issues.Count(i => i.Severity == IssueSeverity.Critical));
        var warnings = results.Sum(r => r.Issues.Count(i => i.Severity == IssueSeverity.Warning));
        var infos = results.Sum(r => r.Issues.Count(i => i.Severity == IssueSeverity.Info));

        logger.LogInformation($"Queries Analyzed: {results.Count}");
        logger.LogInformation($"Total Time: {elapsedMs}ms ({elapsedMs / (double)results.Count:F1}ms per query)");
        logger.LogInformation($"Average Score: {avgScore:F1}/100");

        logger.LogInformation($"\nIssue Breakdown:");
        logger.LogWarning($"  Critical: {criticalIssues}");
        logger.LogWarning($"  Warnings: {warnings}");
        logger.LogInformation($"  Info: {infos}");

        // Score distribution
        var excellent = results.Count(r => r.PerformanceScore >= 90);
        var good = results.Count(r => r.PerformanceScore >= 75 && r.PerformanceScore < 90);
        var acceptable = results.Count(r => r.PerformanceScore >= 60 && r.PerformanceScore < 75);
        var poor = results.Count(r => r.PerformanceScore >= 40 && r.PerformanceScore < 60);
        var critical = results.Count(r => r.PerformanceScore < 40);

        logger.LogInformation($"\nScore Distribution:");
        logger.LogInformation($"  ⭐⭐⭐⭐⭐ Excellent (90-100): {excellent}");
        logger.LogInformation($"  ⭐⭐⭐⭐ Good (75-89): {good}");
        logger.LogInformation($"  ⭐⭐⭐ Acceptable (60-74): {acceptable}");
        logger.LogWarning($"  ⭐⭐ Poor (40-59): {poor}");
        logger.LogError($"  ⭐ Critical (0-39): {critical}");
    }

    static void GenerateDetailedBreakdown(
        List<QueryAnalysisResult> results,
        ILogger logger)
    {
        logger.LogInformation("\n=== DETAILED BREAKDOWN ===\n");

        var issueFrequency = results
            .SelectMany(r => r.Issues)
            .GroupBy(i => i.IssueType)
            .OrderByDescending(g => g.Count())
            .Take(10);

        logger.LogInformation("Most Common Issues:");
        foreach (var group in issueFrequency)
        {
            logger.LogWarning($"  {group.Key}: {group.Count()} occurrences");
        }

        // Top improvement opportunities
        logger.LogInformation("\nQueries Needing Most Attention:");
        var worstQueries = results
            .OrderBy(r => r.PerformanceScore)
            .Take(3);

        foreach (var result in worstQueries)
        {
            logger.LogError($"\n  Score: {result.PerformanceScore:F1}/100");
            logger.LogError($"  Query: {result.QueryText.Substring(0, Math.Min(60, result.QueryText.Length))}...");
            logger.LogError($"  Top Issue: {result.Issues.FirstOrDefault()?.IssueType}");
        }
    }

    static void GenerateApplicationSummary(
        List<QueryAnalysisResult> results,
        List<DatabaseQuery> queries,
        ILogger logger)
    {
        logger.LogInformation("\n=== APPLICATION-LEVEL SUMMARY ===\n");

        var resultsByApp = results
            .Zip(queries, (r, q) => new { Result = r, Query = q })
            .GroupBy(x => x.Query.ApplicationName ?? "Unknown")
            .OrderByDescending(g => g.Average(x => x.Result.PerformanceScore));

        foreach (var appGroup in resultsByApp)
        {
            var avgScore = appGroup.Average(x => x.Result.PerformanceScore);
            var issueCount = appGroup.Sum(x => x.Result.Issues.Count);
            var indexSuggestions = appGroup.Sum(x => x.Result.IndexSuggestions.Count);

            logger.LogInformation($"\n{appGroup.Key}:");
            logger.LogInformation($"  Queries: {appGroup.Count()}");
            logger.LogInformation($"  Avg Score: {avgScore:F1}/100");
            logger.LogInformation($"  Total Issues: {issueCount}");
            logger.LogInformation($"  Index Suggestions: {indexSuggestions}");
        }
    }
}
