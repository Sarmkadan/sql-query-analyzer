#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;

namespace SqlQueryAnalyzer.Middleware;

/// <summary>
/// Rate limiting middleware to prevent resource exhaustion.
/// Implements token bucket algorithm for fair resource allocation.
/// Tracks per-query and global analysis rate limits.
/// </summary>
public class RateLimitingMiddleware
{
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly int _maxQueriesPerSecond;
    private readonly int _maxConcurrentAnalysis;
    private readonly Dictionary<string, QueryRateLimit> _perQueryLimits = new();
    private readonly object _sync = new();
    private int _activeAnalysis = 0;
    private DateTime _windowStart = DateTime.UtcNow;
    private int _requestsInWindow = 0;

    public RateLimitingMiddleware(
        ILogger<RateLimitingMiddleware> logger,
        int maxQueriesPerSecond = 100,
        int maxConcurrentAnalysis = 10)
    {
        _logger = logger;
        _maxQueriesPerSecond = maxQueriesPerSecond;
        _maxConcurrentAnalysis = maxConcurrentAnalysis;
    }

    /// <summary>
    /// Checks if a query can be analyzed based on rate limits.
    /// Acquires a rate limit token, blocking if necessary.
    /// </summary>
    public async Task AcquireSlotAsync(string queryHash, TimeSpan timeout = default)
    {
        if (timeout == default)
            timeout = TimeSpan.FromSeconds(30);

        var deadline = DateTime.UtcNow.Add(timeout);

        while (DateTime.UtcNow < deadline)
        {
            // The check-then-increment must be atomic: this class exists to guard
            // concurrent callers, so unsynchronized ++ on shared counters would let
            // two racing callers both pass the limit check and oversubscribe slots.
            lock (_sync)
            {
                if (_activeAnalysis < _maxConcurrentAnalysis && IsWithinRateLimit())
                {
                    // Acquire slot
                    _activeAnalysis++;
                    _requestsInWindow++;

                    var limit = GetOrCreateQueryLimit(queryHash);
                    limit.RequestCount++;
                    limit.LastRequestTime = DateTime.UtcNow;

                    _logger.LogDebug($"Rate limit slot acquired. Active: {_activeAnalysis}/{_maxConcurrentAnalysis}");
                    return;
                }

                if (_activeAnalysis >= _maxConcurrentAnalysis)
                    _logger.LogWarning($"Rate limit: {_activeAnalysis}/{_maxConcurrentAnalysis} concurrent analysis slots in use");
                else
                    _logger.LogWarning($"Rate limit: {_requestsInWindow}/{_maxQueriesPerSecond} requests in window");
            }

            await Task.Delay(100);
        }

        throw new TimeoutException(
            $"Rate limit timeout after {timeout.TotalSeconds}s. System is at capacity.");
    }

    /// <summary>
    /// Releases a previously acquired rate limit slot.
    /// Must be called after analysis completes to free resources.
    /// </summary>
    public void ReleaseSlot()
    {
        lock (_sync)
        {
            if (_activeAnalysis > 0)
            {
                _activeAnalysis--;
                _logger.LogDebug($"Rate limit slot released. Active: {_activeAnalysis}/{_maxConcurrentAnalysis}");
            }
        }
    }

    /// <summary>
    /// Returns current system load as percentage (0-100).
    /// Useful for monitoring and alerting.
    /// </summary>
    public double GetSystemLoad() => (_activeAnalysis * 100.0) / _maxConcurrentAnalysis;

    /// <summary>
    /// Gets rate limit statistics for a specific query hash.
    /// </summary>
    public QueryRateLimitStats GetQueryStats(string queryHash)
    {
        QueryRateLimit limit;
        lock (_sync)
        {
            limit = GetOrCreateQueryLimit(queryHash);
        }
        return new QueryRateLimitStats
        {
            QueryHash = queryHash,
            TotalRequests = limit.RequestCount,
            LastRequestTime = limit.LastRequestTime,
            AverageIntervalMs = limit.GetAverageInterval(),
            IsThrottled = limit.RequestCount > 100
        };
    }

/// <summary>
/// Gets all per-query rate limit tracking dictionaries.
/// </summary>
/// <returns>Collection of query rate limits.</returns>
internal IReadOnlyCollection<QueryRateLimit> GetPerQueryLimits() => _perQueryLimits.Values;

    /// <summary>
    /// Resets rate limit window after 1 second has elapsed.
    /// Called internally to track moving window.
    /// </summary>
    private bool IsWithinRateLimit()
    {
        var now = DateTime.UtcNow;
        if ((now - _windowStart).TotalSeconds >= 1.0)
        {
            _windowStart = now;
            _requestsInWindow = 0;
        }

        return _requestsInWindow < _maxQueriesPerSecond;
    }

    private QueryRateLimit GetOrCreateQueryLimit(string queryHash)
    {
        if (!_perQueryLimits.TryGetValue(queryHash, out var limit))
        {
            limit = new QueryRateLimit { QueryHash = queryHash };
            _perQueryLimits[queryHash] = limit;
        }

        return limit;
    }
}

/// <summary>
/// Tracks rate limit information for a single query.
/// </summary>
public class QueryRateLimit
{
    public string QueryHash { get; set; } = string.Empty;
    public int RequestCount { get; set; }
    public DateTime FirstRequestTime { get; set; } = DateTime.UtcNow;
    public DateTime LastRequestTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Calculates average interval between requests in milliseconds.
    /// </summary>
    public double GetAverageInterval()
    {
        if (RequestCount <= 1)
            return 0;

        var totalTime = (LastRequestTime - FirstRequestTime).TotalMilliseconds;
        return totalTime / (RequestCount - 1);
    }
}

/// <summary>
/// Statistics about query rate limiting.
/// </summary>
public class QueryRateLimitStats
{
    public string QueryHash { get; set; } = string.Empty;
    public int TotalRequests { get; set; }
    public DateTime LastRequestTime { get; set; }
    public double AverageIntervalMs { get; set; }
    public bool IsThrottled { get; set; }
}
