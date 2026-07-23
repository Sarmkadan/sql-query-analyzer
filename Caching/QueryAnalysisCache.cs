#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Models;
using SqlQueryAnalyzer.Utilities;

namespace SqlQueryAnalyzer.Caching;

/// <summary>
/// In-memory cache for query analysis results.
/// Improves performance by avoiding re-analysis of identical queries.
/// Implements LRU eviction strategy when cache exceeds size limits.
/// </summary>
public sealed class QueryAnalysisCache
{
    private readonly ILogger<QueryAnalysisCache> _logger;
    private readonly QueryCacheKeyGenerator _keyGenerator;
    private readonly MemoryCache _cache;
    private readonly int _maxEntries;
    private readonly TimeSpan _entryTtl;
    private readonly ConcurrentDictionary<string, CacheEntryMetadata> _metadata = new();
    private readonly object _cacheLock = new();

    // Singleton instance for static access
    private static QueryAnalysisCache? _instance;

    public static QueryAnalysisCache Instance => _instance ?? throw new InvalidOperationException("QueryAnalysisCache not initialized. Register with DI first.");

    public QueryAnalysisCache(
        ILogger<QueryAnalysisCache> logger,
        QueryCacheKeyGenerator keyGenerator,
        int maxEntries = 1000,
        int ttlSeconds = 3600)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(keyGenerator);

        _logger = logger;
        _keyGenerator = keyGenerator;
        _maxEntries = maxEntries;
        _entryTtl = TimeSpan.FromSeconds(ttlSeconds);

        // Configure MemoryCache with size limit and thread-safe operations
        var cacheOptions = new MemoryCacheOptions
        {
            SizeLimit = maxEntries,
            ExpirationScanFrequency = TimeSpan.FromSeconds(30)
        };
        _cache = new MemoryCache(cacheOptions);
    }

    /// <summary>
    /// Sets the singleton instance (for DI integration).
    /// </summary>
    internal static void SetInstance(QueryAnalysisCache cache)
    {
        ArgumentNullException.ThrowIfNull(cache);
        _instance = cache;
    }

    /// <summary>
    /// Tries to get cached analysis result for a query.
    /// Returns true and sets result if found and not expired.
    /// </summary>
    public bool TryGetResult(string query, out QueryAnalysisResult? result)
    {
        ArgumentException.ThrowIfNullOrEmpty(query);

        result = null;

        try
        {
            var key = _keyGenerator.GenerateQueryKey(query);

            if (_cache.TryGetValue<CacheEntry>(key, out var entry))
            {
                // Update access metadata
                _metadata.AddOrUpdate(key,
                    _ => new CacheEntryMetadata { LastAccessedAt = DateTime.UtcNow, AccessCount = 1 },
                    (_, existing) =>
                    {
                        existing.LastAccessedAt = DateTime.UtcNow;
                        existing.AccessCount++;
                        return existing;
                    });

                result = entry.Result;
                _logger.LogDebug("Cache hit for query. Key: {Key}", key);
                return true;
            }

            _logger.LogDebug("Cache miss for query. Key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error retrieving from cache");
        }

        return false;
    }

    /// <summary>
    /// Stores analysis result in cache.
    /// Automatically evicts oldest entries if cache is at capacity.
    /// </summary>
    public void Set(string query, QueryAnalysisResult result)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(result);

        try
        {
            var key = _keyGenerator.GenerateQueryKey(query);

            var entry = new CacheEntry
            {
                Result = result
            };

            // Set with size = 1 (each entry counts as 1 unit toward limit)
            var cacheOptions = new MemoryCacheEntryOptions
            {
                Size = 1,
                Priority = CacheItemPriority.Normal,
                AbsoluteExpirationRelativeToNow = _entryTtl
            };

            // Track eviction callbacks for LRU tracking
            cacheOptions.RegisterPostEvictionCallback(EvictionCallback);

            _cache.Set(key, entry, cacheOptions);

            // Initialize or update metadata
            _metadata.AddOrUpdate(key,
                _ => new CacheEntryMetadata { LastAccessedAt = DateTime.UtcNow, AccessCount = 1 },
                (_, existing) =>
                {
                    existing.LastAccessedAt = DateTime.UtcNow;
                    existing.AccessCount++;
                    return existing;
                });

            _logger.LogDebug("Cached analysis result. Key: {Key}, Size: {Current}/{Max}",
                key, GetCacheCount(), _maxEntries);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error storing in cache");
        }
    }

    /// <summary>
    /// Invalidates cache entry for a specific query.
    /// </summary>
    public void Invalidate(string query)
    {
        ArgumentException.ThrowIfNullOrEmpty(query);

        try
        {
            var key = _keyGenerator.GenerateQueryKey(query);
            if (_cache.TryGetValue<CacheEntry>(key, out _))
            {
                _cache.Remove(key);
                _metadata.TryRemove(key, out _);
                _logger.LogDebug("Cache invalidated for query. Key: {Key}", key);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error invalidating cache");
        }
    }

    /// <summary>
    /// Clears entire cache.
    /// </summary>
    public void Clear()
    {
        lock (_cacheLock)
        {
            var count = GetCacheCount();
            _cache.Clear();
            _metadata.Clear();
            _logger.LogInformation("Cache cleared. {Count} entries removed.", count);
        }
    }

    /// <summary>
    /// Removes expired entries from cache.
    /// Can be called periodically as maintenance.
    /// </summary>
    public void RemoveExpiredEntries()
    {
        // MemoryCache automatically handles expiration during TryGetValue
        // This method is kept for backward compatibility
        var expiredKeys = _metadata.Keys
            .Where(key => !_cache.TryGetValue<CacheEntry>(key, out _))
            .ToList();

        foreach (var key in expiredKeys)
        {
            _metadata.TryRemove(key, out _);
        }

        if (expiredKeys.Count > 0)
        {
            _logger.LogDebug("Removed {Count} expired cache entries", expiredKeys.Count);
        }
    }

    /// <summary>
    /// Gets cache statistics for monitoring.
    /// </summary>
    public CacheStatistics GetStatistics()
    {
        var metadataValues = _metadata.Values.ToList();
        var hits = metadataValues.Sum(e => e.AccessCount);
        var totalEntries = GetCacheCount();

        return new CacheStatistics
        {
            TotalEntries = totalEntries,
            MaxEntries = _maxEntries,
            Hits = hits,
            Misses = Math.Max(0, hits - totalEntries),
            HitRate = hits > 0 ? (hits * 100.0) / (hits + Math.Max(0, totalEntries - hits)) : 0,
            AverageAccessCount = metadataValues.Count > 0 ? (double)hits / metadataValues.Count : 0,
            OldestEntryAge = metadataValues.Count > 0
                ? (DateTime.UtcNow - metadataValues.Min(e => e.CreatedAt)).TotalSeconds
                : 0
        };
    }

    /// <summary>
    /// Gets the current cache entry count safely (thread-safe).
    /// </summary>
    private int GetCacheCount()
    {
        // MemoryCache doesn't expose Count directly, so we track it via metadata
        // This is more accurate anyway since it reflects actual cached entries
        return _metadata.Count;
    }

    /// <summary>
    /// Evicts least-recently-used entry from cache.
    /// Called automatically by MemoryCache when size limit is reached.
    /// </summary>
    private void EvictLruEntry(string key, object value, EvictionReason reason, object state)
    {
        if (reason == EvictionReason.Capacity)
        {
            _metadata.TryRemove(key, out _);
            _logger.LogDebug("Evicted LRU cache entry: {Key}", key);
        }
    }

    /// <summary>
    /// Callback for MemoryCache eviction events.
    /// </summary>
    private void EvictionCallback(object key, object value, EvictionReason reason, object state)
    {
        EvictLruEntry((string)key, value, reason, state);
    }

    /// <summary>
    /// Returns current cache size.
    /// </summary>
    public int Count => GetCacheCount();

    /// <summary>
    /// Gets the current cache entry count safely (thread-safe).
    /// </summary>
    public int SafeCount => GetCacheCount();
}

/// <summary>
/// Represents a single cached analysis result.
/// </summary>
internal class CacheEntry
{
    public QueryAnalysisResult Result { get; set; } = null!;
}

/// <summary>
/// Metadata about cache entries for LRU tracking and statistics.
/// </summary>
internal class CacheEntryMetadata
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    public DateTime LastAccessedAt { get; set; }
    public int AccessCount { get; set; }
}

/// <summary>
/// Cache performance statistics.
/// </summary>
public sealed class CacheStatistics
{
    public int TotalEntries { get; set; }
    public int MaxEntries { get; set; }
    public long Hits { get; set; }
    public long Misses { get; set; }
    public double HitRate { get; set; }
    public double AverageAccessCount { get; set; }
    public double OldestEntryAge { get; set; }

    public override string ToString() =>
        $"Cache Stats: {TotalEntries}/{MaxEntries} entries, " +
        $"Hit Rate: {HitRate:F1}%, Avg Accesses: {AverageAccessCount:F1}";
}