#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Caching;
using SqlQueryAnalyzer.Utilities;

namespace SqlQueryAnalyzer.Extensions;

/// <summary>
/// Extension methods for registering QueryAnalysisCache in DI container.
/// </summary>
public static class QueryAnalysisCacheExtensions
{
    /// <summary>
    /// Adds QueryAnalysisCache services to the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddQueryAnalysisCache(
        this IServiceCollection services,
        Action<QueryAnalysisCacheOptions>? configure = null)
    {
        // Register the cache key generator
        services.AddSingleton<QueryCacheKeyGenerator>();

        // Register the cache with default options
        services.AddSingleton<QueryAnalysisCache>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<QueryAnalysisCache>>();
            var keyGenerator = provider.GetRequiredService<QueryCacheKeyGenerator>();

            var options = new QueryAnalysisCacheOptions();
            configure?.Invoke(options);

            var cache = new QueryAnalysisCache(
                logger,
                keyGenerator,
                options.MaxCacheSize ?? 1000,
                options.TtlSeconds ?? 3600);

            // Set the singleton instance for static access
            QueryAnalysisCache.SetInstance(cache);

            return cache;
        });

        return services;
    }

    /// <summary>
    /// Adds QueryAnalysisCache services to the DI container with custom options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="maxCacheSize">Maximum number of entries in cache (default: 1000).</param>
    /// <param name="ttlSeconds">Time-to-live for cache entries in seconds (default: 3600).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddQueryAnalysisCache(
        this IServiceCollection services,
        int maxCacheSize,
        int ttlSeconds = 3600)
    {
        return services.AddQueryAnalysisCache(options =>
        {
            options.MaxCacheSize = maxCacheSize;
            options.TtlSeconds = ttlSeconds;
        });
    }
}

/// <summary>
/// Options for configuring QueryAnalysisCache.
/// </summary>
public class QueryAnalysisCacheOptions
{
    /// <summary>
    /// Maximum number of entries in cache (default: 1000).
    /// </summary>
    public int? MaxCacheSize { get; set; }

    /// <summary>
    /// Time-to-live for cache entries in seconds (default: 3600 = 1 hour).
    /// </summary>
    public int? TtlSeconds { get; set; }
}