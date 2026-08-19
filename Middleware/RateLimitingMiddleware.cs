#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
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
    private readonly ConcurrentDictionary<string, QueryRateLimit> _perQueryLimits = new();
    private int _activeAnalysis = 0;
    private DateTime _windowStart = DateTime.UtcNow;
    private int _requestsInWindow = 0;
    private readonly object _windowSync = new();

    public RateLimitingMiddleware(
        ILogger<RateLimitingMiddleware> logger,
        int maxQueriesPerSecond = 100,
        int maxConcurrentAnalysis = 10)
    {
        ArgumentNullException.ThrowIfNull(logger);
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
        ArgumentException.ThrowIfNullOrEmpty(queryHash);
        if (timeout == default)
            timeout = TimeSpan.FromSeconds(30);

        _logger.LogInformation("AcquireSlotAsync called with {QueryHash} and timeout {TimeoutSeconds}s", queryHash, timeout.TotalSeconds);

        var deadline = DateTime.UtcNow.Add(timeout);

        // Evict old entries periodically (simple approach: every time we enter)
        EvictOldEntries();

        while (DateTime.UtcNow < deadline)
        {
            if (TryAcquire())
            {
                var limit = _perQueryLimits.GetOrAdd(queryHash, _ => new QueryRateLimit { QueryHash = queryHash });
                Interlocked.Increment(ref limit.RequestCount);
                limit.LastRequestTime = DateTime.UtcNow;

                _logger.LogInformation("Rate limit slot acquired for {QueryHash}. Active: {ActiveCount}/{MaxConcurrent}", queryHash, Volatile.Read(ref _activeAnalysis), _maxConcurrentAnalysis);
                return;
            }

            await Task.Delay(100);
        }

        _logger.LogWarning("Rate limit timeout after {TimeoutSeconds}s for {QueryHash}. System is at capacity.", timeout.TotalSeconds, queryHash);
        throw new TimeoutException(
            $"Rate limit timeout after {timeout.TotalSeconds}s. System is at capacity.");
    }

    private bool TryAcquire()
    {
        if (Volatile.Read(ref _activeAnalysis) >= _maxConcurrentAnalysis)
        {
            _logger.LogWarning("Rate limit: {ActiveCount}/{MaxConcurrent} concurrent analysis slots in use", Volatile.Read(ref _activeAnalysis), _maxConcurrentAnalysis);
            return false;
        }

        lock (_windowSync)
        {
            var now = DateTime.UtcNow;
            if ((now - _windowStart).TotalSeconds >= 1.0)
            {
                _windowStart = now;
                _requestsInWindow = 0;
            }

            if (_requestsInWindow < _maxQueriesPerSecond)
            {
                Interlocked.Increment(ref _activeAnalysis);
                _requestsInWindow++;
                return true;
            }

            _logger.LogWarning("Rate limit: {RequestsInWindow}/{MaxQueriesPerSecond} requests in window", _requestsInWindow, _maxQueriesPerSecond);
            return false;
        }
    }

    private void EvictOldEntries()
    {
        var now = DateTime.UtcNow;
        var expiration = TimeSpan.FromMinutes(5); // Evict entries older than 5 minutes

        var evicted = 0;
        foreach (var entry in _perQueryLimits)
        {
            if (now - entry.Value.LastRequestTime > expiration)
            {
                if (_perQueryLimits.TryRemove(entry.Key, out _))
                    evicted++;
            }
        }

        if (evicted > 0)
            _logger.LogDebug("Evicted {EvictedCount} stale query rate limit entries", evicted);
    }

    /// <summary>
    /// Releases a previously acquired rate limit slot.
    /// Must be called after analysis completes to free resources.
    /// </summary>
    public void ReleaseSlot()
    {
        Interlocked.Decrement(ref _activeAnalysis);
        _logger.LogInformation("Rate limit slot released. Active: {ActiveCount}/{MaxConcurrent}", Volatile.Read(ref _activeAnalysis), _maxConcurrentAnalysis);
    }

    /// <summary>
    /// Returns current system load as percentage (0-100).
    /// Useful for monitoring and alerting.
    /// </summary>
    public double GetSystemLoad()
    {
        var load = (Volatile.Read(ref _activeAnalysis) * 100.0) / _maxConcurrentAnalysis;
        _logger.LogDebug("System load is {LoadPercent}% ({ActiveCount}/{MaxConcurrent})", load, Volatile.Read(ref _activeAnalysis), _maxConcurrentAnalysis);
        return load;
    }

    /// <summary>
    /// Gets rate limit statistics for a specific query hash.
    /// </summary>
    public QueryRateLimitStats GetQueryStats(string queryHash)
    {
        ArgumentException.ThrowIfNullOrEmpty(queryHash);
        _logger.LogInformation("Fetching stats for {QueryHash}", queryHash);
        var limit = _perQueryLimits.GetValueOrDefault(queryHash) ?? new QueryRateLimit { QueryHash = queryHash };
        _logger.LogDebug("Query stats: TotalRequests={TotalRequests}, LastRequestTime={LastRequestTime:O}, AverageIntervalMs={AverageIntervalMs}, IsThrottled={IsThrottled}",
            limit.RequestCount,
            limit.LastRequestTime,
            limit.GetAverageInterval(),
            limit.RequestCount > 100);
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
    internal IReadOnlyCollection<QueryRateLimit> GetPerQueryLimits()
{
    var limits = _perQueryLimits.Values.ToArray();
    _logger.LogDebug("Returning {Count} per-query rate limit entries", limits.Length);
    return limits;
}
}

/// <summary>
/// Tracks rate limit information for a single query.
/// </summary>
public class QueryRateLimit
{
    public string QueryHash { get; set; } = string.Empty;
    public int RequestCount; // Changed from property to field for Interlocked usage
    public DateTime FirstRequestTime { get; set; } = DateTime.UtcNow;
    public DateTime LastRequestTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Calculates average interval between requests in milliseconds.
    /// </summary>
    public double GetAverageInterval()
    {
        var count = Volatile.Read(ref RequestCount);
        if (count <= 1)
            return 0;

        var totalTime = (LastRequestTime - FirstRequestTime).TotalMilliseconds;
        return totalTime / (count - 1);
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
