// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Services;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Examples;

/// Demonstrates basic query analysis with detailed reporting
class BasicAnalyzer
{
    static async Task Main()
    {
        var services = new ServiceCollection()
            .AddLogging(config => config.AddConsole())
            .AddScoped<IQueryAnalyzerService, QueryAnalyzerService>()
            .BuildServiceProvider();

        var analyzer = services.GetRequiredService<IQueryAnalyzerService>();
        var logger = services.GetRequiredService<ILogger<BasicAnalyzer>>();

        logger.LogInformation("SQL Query Analyzer - Basic Example");
        logger.LogInformation("===================================\n");

        // Analyze various queries
        var queries = new[]
        {
            new {
                Name = "SELECT *",
                Query = "SELECT * FROM Orders WHERE CustomerId = 1"
            },
            new {
                Name = "JOIN with OR",
                Query = "SELECT o.*, c.* FROM Orders o JOIN Customers c ON o.CustomerId = c.Id OR o.SalesPersonId = c.Id"
            },
            new {
                Name = "Function on Column",
                Query = "SELECT * FROM Orders WHERE YEAR(OrderDate) = 2024 AND CustomerId IN (1,2,3)"
            },
            new {
                Name = "Complex Query",
                Query = @"
                    SELECT DISTINCT
                        c.Id, c.Name, COUNT(*) as OrderCount,
                        AVG(o.TotalAmount) as AvgAmount
                    FROM Customers c
                    LEFT JOIN Orders o ON c.Id = o.CustomerId
                    WHERE c.Country = 'USA'
                        AND o.OrderDate > GETDATE() - 365
                        AND o.TotalAmount > 100
                    GROUP BY c.Id, c.Name
                    HAVING COUNT(*) > 5
                    ORDER BY AvgAmount DESC
                "
            }
        };

        foreach (var item in queries)
        {
            logger.LogInformation($"\n--- Analyzing: {item.Name} ---");
            await AnalyzeAndReport(analyzer, item.Query, logger);
        }

        logger.LogInformation("\n\nAnalysis complete!");
    }

    static async Task AnalyzeAndReport(
        IQueryAnalyzerService analyzer,
        string query,
        ILogger logger)
    {
        try
        {
            var result = await analyzer.AnalyzeQueryAsync(query);

            // Display score
            logger.LogInformation($"Performance Score: {result.PerformanceScore:F1}/100");
            logger.LogInformation($"Complexity: {result.Complexity}");

            // Display issues
            if (result.Issues.Count > 0)
            {
                logger.LogInformation($"\nIssues Found: {result.Issues.Count}");

                var byServerity = result.Issues
                    .GroupBy(i => i.Severity)
                    .OrderByDescending(g => g.Key);

                foreach (var group in byServerity)
                {
                    logger.LogWarning($"\n  {group.Key}:");
                    foreach (var issue in group.OrderByDescending(i => i.ImpactScore))
                    {
                        logger.LogWarning($"    • {issue.IssueType}: {issue.Description}");
                        logger.LogWarning($"      Fix: {issue.RecommendedFix}");
                        logger.LogWarning($"      Impact: {issue.ImpactScore}/10");
                    }
                }
            }
            else
            {
                logger.LogInformation("No issues found - query looks good!");
            }

            // Display suggestions
            if (result.IndexSuggestions.Count > 0)
            {
                logger.LogInformation($"\nIndex Suggestions: {result.IndexSuggestions.Count}");
                foreach (var suggestion in result.IndexSuggestions
                    .OrderByDescending(s => s.Roi)
                    .Take(3))
                {
                    logger.LogInformation($"  • {suggestion.SuggestedIndexName}");
                    logger.LogInformation($"    Columns: {string.Join(", ", suggestion.Columns)}");
                    logger.LogInformation($"    Est. ROI: {suggestion.Roi:F1}%");
                    logger.LogInformation($"    Est. Size: {suggestion.EstimatedSizeKB} KB");
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Analysis failed");
        }
    }
}
