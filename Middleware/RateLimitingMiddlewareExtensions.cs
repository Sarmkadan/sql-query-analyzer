#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace SqlQueryAnalyzer.Middleware;

/// <summary>
/// Provides extension methods for <see cref="RateLimitingMiddleware"/> that enable fluent APIs
/// for common rate limiting scenarios, monitoring, and system state inspection.
/// </summary>
public static class RateLimitingMiddlewareExtensions
{
    /// <summary>
    /// Attempts to acquire a rate limit slot with a timeout, returning success status.
    /// </summary>
    /// <param name="middleware">The rate limiting middleware instance.</param>
    /// <param name="queryHash">The hash of the query being analyzed.</param>
    /// <param name="timeout">Maximum time to wait for a slot.</param>
    /// <returns>True if slot was acquired, false if timeout occurred.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="middleware"/> or <paramref name="queryHash"/> is null.</exception>
    public static async Task<bool> TryAcquireSlotAsync(this RateLimitingMiddleware middleware, string queryHash, TimeSpan timeout = default)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        ArgumentException.ThrowIfNullOrEmpty(queryHash);

        try
        {
            await middleware.AcquireSlotAsync(queryHash, timeout);
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }

    /// <summary>
    /// Gets rate limit statistics for all tracked queries as a read-only collection.
    /// </summary>
    /// <param name="middleware">The rate limiting middleware instance.</param>
    /// <returns>Read-only collection of query statistics.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="middleware"/> is null.</exception>
    public static IReadOnlyList<QueryRateLimitStats> GetAllQueryStats(this RateLimitingMiddleware middleware)
    {
        ArgumentNullException.ThrowIfNull(middleware);

        return middleware.GetPerQueryLimits()
            .Select(limit => new QueryRateLimitStats
            {
                QueryHash = limit.QueryHash,
                TotalRequests = limit.RequestCount,
                LastRequestTime = limit.LastRequestTime,
                AverageIntervalMs = limit.GetAverageInterval(),
                IsThrottled = limit.RequestCount > 100
            })
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Gets the current system load as a normalized value between 0 and 1.
    /// </summary>
    /// <param name="middleware">The rate limiting middleware instance.</param>
    /// <returns>Normalized load value (0 = no load, 1 = fully loaded).</returns>
    /// <exception cref="ArgumentNullException"><paramref name="middleware"/> is null.</exception>
    public static double GetNormalizedLoad(this RateLimitingMiddleware middleware)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        return middleware.GetSystemLoad() / 100.0;
    }

    /// <summary>
    /// Gets rate limit statistics for queries that exceed the throttling threshold.
    /// </summary>
    /// <param name="middleware">The rate limiting middleware instance.</param>
    /// <param name="threshold">Minimum request count to be considered throttled.</param>
    /// <returns>Read-only collection of throttled query statistics.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="middleware"/> is null.</exception>
    public static IReadOnlyList<QueryRateLimitStats> GetThrottledQueries(this RateLimitingMiddleware middleware, int threshold = 100)
    {
        ArgumentNullException.ThrowIfNull(middleware);

        return middleware.GetAllQueryStats()
            .Where(stats => stats.IsThrottled && stats.TotalRequests >= threshold)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Gets the most active queries (highest request count) as a read-only collection.
    /// </summary>
    /// <param name="middleware">The rate limiting middleware instance.</param>
    /// <param name="count">Maximum number of queries to return.</param>
    /// <returns>Read-only collection of the most active queries, ordered by request count.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="middleware"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is less than 1.</exception>
    public static IReadOnlyList<QueryRateLimitStats> GetMostActiveQueries(this RateLimitingMiddleware middleware, int count = 10)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 1);

        return middleware.GetAllQueryStats()
            .OrderByDescending(stats => stats.TotalRequests)
            .Take(count)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Gets the average request interval across all tracked queries in milliseconds.
    /// </summary>
    /// <param name="middleware">The rate limiting middleware instance.</param>
    /// <returns>Average interval in milliseconds, or 0 if no data available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="middleware"/> is null.</exception>
    public static double GetAverageRequestIntervalMs(this RateLimitingMiddleware middleware)
    {
        ArgumentNullException.ThrowIfNull(middleware);

        var allStats = middleware.GetAllQueryStats();
        if (allStats.Count == 0)
            return 0;

        return allStats.Average(stats => stats.AverageIntervalMs);
    }

    /// <summary>
    /// Gets the total number of requests across all tracked queries.
    /// </summary>
    /// <param name="middleware">The rate limiting middleware instance.</param>
    /// <returns>Total request count.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="middleware"/> is null.</exception>
    public static int GetTotalRequests(this RateLimitingMiddleware middleware)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        return middleware.GetAllQueryStats().Sum(stats => stats.TotalRequests);
    }

    /// <summary>
    /// Gets the current request rate (requests per second) across all tracked queries.
    /// </summary>
    /// <param name="middleware">The rate limiting middleware instance.</param>
    /// <returns>Current request rate in requests per second.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="middleware"/> is null.</exception>
    public static double GetCurrentRequestRate(this RateLimitingMiddleware middleware)
    {
        ArgumentNullException.ThrowIfNull(middleware);

        var allStats = middleware.GetAllQueryStats();
        if (allStats.Count == 0)
            return 0;

        var totalRequests = allStats.Sum(stats => stats.TotalRequests);
        if (totalRequests == 0)
        {
            return 0;
        }

        var oldestRequestTime = allStats.Min(stats => stats.LastRequestTime);
        var timeSpan = DateTime.UtcNow - oldestRequestTime;

        return timeSpan.TotalSeconds > 0
            ? totalRequests / timeSpan.TotalSeconds
            : 0;
    }

    /// <summary>
    /// Gets a summary string representing the current system state.
    /// </summary>
    /// <param name="middleware">The rate limiting middleware instance.</param>
    /// <returns>Formatted summary string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="middleware"/> is null.</exception>
    public static string GetSystemStateSummary(this RateLimitingMiddleware middleware)
    {
        ArgumentNullException.ThrowIfNull(middleware);

        var load = middleware.GetSystemLoad();
        var totalRequests = middleware.GetTotalRequests();
        var currentRate = middleware.GetCurrentRequestRate();
        var activeSlots = middleware.GetNormalizedLoad();
        var throttledCount = middleware.GetThrottledQueries().Count;

        return string.Create(CultureInfo.InvariantCulture,
            $"Rate Limiter State: Load={load:F1}%, TotalRequests={totalRequests}, " +
            $"Rate={currentRate:F2}req/s, Active={activeSlots:P0}, Throttled={throttledCount}");
    }
}