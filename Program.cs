#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Configuration;
using SqlQueryAnalyzer.Services;
using SqlQueryAnalyzer.Repositories;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer;

class Program
{
    static async Task Main(string[] args)
    {
        var services = new ServiceCollection();

        // Configure logging
        services.AddLogging(config =>
        {
            config.AddConsole();
            config.SetMinimumLevel(LogLevel.Information);
        });

        // Register configuration
        services.AddSingleton<IConnectionConfiguration, SqlServerConfiguration>();

        // Register repositories
        services.AddSingleton<IQueryRepository, QueryRepository>();
        services.AddSingleton<IAnalysisRepository, AnalysisRepository>();
        services.AddSingleton<IIndexRepository, IndexRepository>();

        // Register services
        services.AddSingleton<IQueryAnalyzerService, QueryAnalyzerService>();
        services.AddSingleton<IIndexAnalyzerService, IndexAnalyzerService>();
        services.AddSingleton<IQueryPlanAnalyzerService, QueryPlanAnalyzerService>();
        services.AddSingleton<IPerformanceIssueDetectorService, PerformanceIssueDetectorService>();
        services.AddSingleton<IExplainPlanParserService, ExplainPlanParserService>();

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("Starting SQL Query Analyzer v1.0.0");

            var analyzer = serviceProvider.GetRequiredService<IQueryAnalyzerService>();

            // Example usage
            await RunAnalysisExample(analyzer, logger);

            logger.LogInformation("Analysis completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Application terminated with error");
            Environment.Exit(1);
        }
    }

    // Demonstrate basic analyzer usage
    static async Task RunAnalysisExample(IQueryAnalyzerService analyzer, ILogger logger)
    {
        var sampleQueries = new[]
        {
            "SELECT * FROM Orders o JOIN Customers c ON o.CustomerId = c.Id WHERE c.Country = 'USA'",
            "SELECT OrderId, CustomerId FROM Orders WHERE OrderDate > GETDATE() - 30",
            "SELECT DISTINCT c.Id FROM Customers c WHERE NOT EXISTS (SELECT 1 FROM Orders o WHERE o.CustomerId = c.Id)"
        };

        foreach (var query in sampleQueries)
        {
            logger.LogInformation($"Analyzing: {query.Substring(0, Math.Min(60, query.Length))}...");
            var result = await analyzer.AnalyzeQueryAsync(query);

            logger.LogInformation($"Issues found: {result.Issues.Count}");
            foreach (var issue in result.Issues)
            {
                logger.LogWarning($"  - {issue.IssueType}: {issue.Description}");
            }
        }
    }
}
