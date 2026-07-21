#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Caching;
using SqlQueryAnalyzer.Models;
using SqlQueryAnalyzer.CLI;

namespace SqlQueryAnalyzer.Middleware;

/// <summary>
/// Middleware that implements caching for query analysis results.
/// Checks cache before analysis and stores results after analysis.
/// Can be bypassed via context arguments.
/// </summary>
public class CachingMiddleware : IAnalysisMiddleware
{
    private readonly QueryAnalysisCache _cache;
    private readonly ILogger<CachingMiddleware> _logger;

    public CachingMiddleware(
        QueryAnalysisCache cache,
        ILogger<CachingMiddleware> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Executes the caching middleware.
    /// Checks cache first, then allows pipeline to continue if not found.
    /// Stores result in cache after successful analysis.
    /// </summary>
    public async Task ExecuteAsync(AnalysisContext context)
    {
        try
        {
            // Check if caching is disabled for this analysis
            if (IsCachingDisabled(context))
            {
                _logger.LogDebug("Caching disabled for this analysis, skipping cache check");
                return;
            }

            _logger.LogDebug("Checking cache for query analysis result");

            // Try to get cached result
            if (_cache.TryGetResult(context.Query, out var cachedResult) && cachedResult != null)
            {
                _logger.LogInformation("Cache hit - using cached analysis result");
                context.Result = cachedResult;
                context.ShouldContinue = false; // Skip remaining pipeline since we have cached result
                return;
            }

            _logger.LogDebug("Cache miss - proceeding with analysis");

            // Cache miss - continue with analysis
            // The AnalysisMiddleware will populate the result
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in caching middleware");
            // Don't fail the pipeline on cache errors - continue without caching
        }
    }


    /// <summary>
    /// Checks if caching should be bypassed for this analysis.
    /// </summary>
    private bool IsCachingDisabled(AnalysisContext context)
    {
        // Check if cache is explicitly disabled
        if (context.Arguments is CommandLineArguments args && !args.EnableCache)
        {
            return true;
        }

        // Check metadata for cache bypass
        if (context.Metadata.TryGetValue("bypassCache", out var bypassValue) &&
            bypassValue is bool bypass && bypass)
        {
            return true;
        }

        return false;
    }
}
