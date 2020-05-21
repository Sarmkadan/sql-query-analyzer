// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using SqlQueryAnalyzer.Services;

namespace SqlQueryAnalyzer.Examples;

/// <summary>
/// Demonstrates the minimal setup required to start using the SQL Query Analyzer.
/// </summary>
public class BasicUsage
{
    public static async Task RunAsync()
    {
        // 1. Setup DI container
        var services = new ServiceCollection()
            .AddScoped<IQueryAnalyzerService, QueryAnalyzerService>()
            .BuildServiceProvider();

        // 2. Resolve the analyzer service
        var analyzer = services.GetRequiredService<IQueryAnalyzerService>();

        // 3. Analyze a query
        string query = "SELECT * FROM Orders WHERE CustomerId = 1";
        var result = await analyzer.AnalyzeQueryAsync(query);

        // 4. Print results
        Console.WriteLine($"Performance Score: {result.PerformanceScore:F1}/100");
        foreach (var issue in result.Issues)
        {
            Console.WriteLine($"[{issue.Severity}] {issue.IssueType}: {issue.Description}");
        }
    }
}
