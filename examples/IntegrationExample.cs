// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using SqlQueryAnalyzer.Services;

namespace SqlQueryAnalyzer.Examples;

/// <summary>
/// Shows how to wire the analyzer into an ASP.NET Core DI container.
/// </summary>
public static class IntegrationExample
{
    public static void ConfigureServices(IServiceCollection services)
    {
        // Add required infrastructure
        services.AddLogging();

        // Register analyzer service
        services.AddScoped<IQueryAnalyzerService, QueryAnalyzerService>();
        
        // Register other optional components
        services.AddScoped<IIndexAnalyzerService, IndexAnalyzerService>();
    }
}
