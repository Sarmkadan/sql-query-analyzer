// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Configuration;
using SqlQueryAnalyzer.Services;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Examples;

/// <summary>
/// Demonstrates advanced configuration, custom options, and error handling.
/// </summary>
public class AdvancedUsage
{
    public static async Task RunAsync()
    {
        // 1. Configure settings
        var settings = new AnalyzerSettings();
        settings.Analysis.DetectNPlusOne = true;
        settings.Cache.Enabled = true;
        settings.Performance.TimeoutSeconds = 15;

        // 2. Setup DI with custom configuration
        var services = new ServiceCollection()
            .AddLogging(builder => builder.AddConsole())
            .AddSingleton(settings)
            .AddScoped<IQueryAnalyzerService, QueryAnalyzerService>()
            .BuildServiceProvider();

        var analyzer = services.GetRequiredService<IQueryAnalyzerService>();
        var logger = services.GetRequiredService<ILogger<AdvancedUsage>>();

        // 3. Analyze query with error handling
        string complexQuery = "SELECT * FROM Orders o JOIN Customers c ON o.CustomerId = c.Id";
        
        try
        {
            var result = await analyzer.AnalyzeQueryAsync(complexQuery);
            
            logger.LogInformation("Analysis complete.");
            logger.LogInformation($"Score: {result.PerformanceScore}");
            
            // Further process results...
        }
        catch (AnalysisException ex)
        {
            logger.LogError($"Analysis failed due to policy violation: {ex.Message}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred during analysis.");
        }
    }
}
