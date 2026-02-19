#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

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
    private readonly Dictionary<string, CacheEntry> _cache = new();
    private readonly int _maxEntries;
    private readonly TimeSpan _entryTtl;

    public QueryAnalysisCache(
        ILogger<QueryAnalysisCache> logger,
        QueryCacheKeyGenerator keyGenerator,
        int maxEntries = 1000,
        int ttlSeconds = 3600)
    {
        _logger = logger;
        _keyGenerator = keyGenerator;
        _maxEntries = maxEntries;
        _entryTtl = TimeSpan.FromSeconds(ttlSeconds);
    }

    /// <summary>
    /// Tries to get cached analysis result for a query.
    /// Returns true and sets result if found and not expired.
    /// </summary>
    public bool TryGetResult(string query, out QueryAnalysisResult? result)
    {
        result = null;

        try
        {
            var key = _keyGenerator.GenerateQueryKey(query);

            if (_cache.TryGetValue(key, out var entry))
            {
                // Check if entry has expired
                if (DateTime.UtcNow - entry.CreatedAt > _entryTtl)
                {
                    _cache.Remove(key);
                    _logger.LogDebug($"Cache entry expired: {key}");
                    return false;
                }

                // Update access time for LRU
                entry.LastAccessedAt = DateTime.UtcNow;
                entry.AccessCount++;

                result = entry.Result;
                _logger.LogDebug($"Cache hit for query. Key: {key}");
                return true;
            }

            _logger.LogDebug($"Cache miss for query. Key: {key}");
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
        try
        {
            var key = _keyGenerator.GenerateQueryKey(query);

            // Remove old entry if exists
            if (_cache.ContainsKey(key))
            {
                _cache.Remove(key);
            }

            // Evict LRU entries if at capacity
            if (_cache.Count >= _maxEntries)
            {
                EvictLruEntry();
            }

            var entry = new CacheEntry
            {
                Key = key,
                Result = result,
                CreatedAt = DateTime.UtcNow,
                LastAccessedAt = DateTime.UtcNow
            };

            _cache[key] = entry;
            _logger.LogDebug($"Cached analysis result. Key: {key}, Size: {_cache.Count}/{_maxEntries}");
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
        try
        {
            var key = _keyGenerator.GenerateQueryKey(query);
            if (_cache.Remove(key))
            {
                _logger.LogDebug($"Cache invalidated for query. Key: {key}");
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
        var count = _cache.Count;
        _cache.Clear();
        _logger.LogInformation($"Cache cleared. {count} entries removed.");
    }

    /// <summary>
    /// Removes expired entries from cache.
    /// Can be called periodically as maintenance.
    /// </summary>
    public void RemoveExpiredEntries()
    {
        var keysToRemove = _cache
            .Where(kvp => DateTime.UtcNow - kvp.Value.CreatedAt > _entryTtl)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
        }

        if (keysToRemove.Count > 0)
        {
            _logger.LogDebug($"Removed {keysToRemove.Count} expired cache entries");
        }
    }

    /// <summary>
    /// Gets cache statistics for monitoring.
    /// </summary>
    public CacheStatistics GetStatistics()
    {
        var hits = _cache.Values.Sum(e => e.AccessCount);
        var misses = Math.Max(0, hits - _cache.Count);

        return new CacheStatistics
        {
            TotalEntries = _cache.Count,
            MaxEntries = _maxEntries,
            Hits = hits,
            Misses = misses,
            HitRate = hits > 0 ? (hits * 100.0) / (hits + misses) : 0,
            AverageAccessCount = _cache.Count > 0 ? (double)hits / _cache.Count : 0,
            OldestEntryAge = _cache.Count > 0
                ? (DateTime.UtcNow - _cache.Values.Min(e => e.CreatedAt)).TotalSeconds
                : 0
        };
    }

    /// <summary>
    /// Evicts least-recently-used entry from cache.
    /// </summary>
    private void EvictLruEntry()
    {
        if (_cache.Count == 0)
            return;

        var lruEntry = _cache
            .OrderBy(kvp => kvp.Value.LastAccessedAt)
            .First();

        _cache.Remove(lruEntry.Key);
        _logger.LogDebug($"Evicted LRU cache entry: {lruEntry.Key}");
    }

    /// <summary>
    /// Returns current cache size.
    /// </summary>
    public int Count => _cache.Count;
}

/// <summary>
/// Represents a single cached analysis result.
/// </summary>
internal class CacheEntry
{
    public string Key { get; set; } = string.Empty;
    public QueryAnalysisResult Result { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
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