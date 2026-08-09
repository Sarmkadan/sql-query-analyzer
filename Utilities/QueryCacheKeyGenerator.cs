#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Security.Cryptography;
using System.Text;
using SqlQueryAnalyzer.Utilities;

namespace SqlQueryAnalyzer.Utilities;

/// <summary>
/// Generates cache keys for queries and analysis results.
/// Ensures consistent key generation across the application.
/// Keys are deterministic: same input always produces same key.
/// </summary>
public class QueryCacheKeyGenerator
{
    private const string KeyPrefix = "sqlanalyzer:";
    private const string QueryHashPrefix = "query:";
    private const string ResultHashPrefix = "result:";

    // Tracks when each key was generated so callers can check key age (see IsCacheKeyExpired
    // in QueryCacheKeyGeneratorExtensions). Keyed by the full cache key string.
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, DateTimeOffset> s_keyCreatedAt = new();

    /// <summary>
    /// Generates a cache key for a query.
    /// Uses SHA256 hash of normalized query for efficient key generation.
    /// </summary>
    public string GenerateQueryKey(string query)
    {
        ArgumentException.ThrowIfNullOrEmpty(query);

        var normalized = NormalizeForHashing(query);
        var hash = ComputeHash(normalized);
        return TrackCreation($"{KeyPrefix}{QueryHashPrefix}{hash}");
    }

    /// <summary>
    /// Generates a cache key for analysis results.
    /// Links result key to query key for quick lookups.
    /// </summary>
    public string GenerateResultKey(string query)
    {
        ArgumentException.ThrowIfNullOrEmpty(query);
        ArgumentNullException.ThrowIfNull(query);

        var normalized = NormalizeForHashing(query);
        var hash = ComputeHash(normalized);
        return TrackCreation($"{KeyPrefix}{ResultHashPrefix}{hash}");
    }

    /// <summary>
    /// Generates a cache key for analysis metadata.
    /// Used for storing analysis configuration and settings.
    /// </summary>
    public string GenerateMetadataKey(string query, Dictionary<string, string>? parameters = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(query);

        var builder = new StringBuilder(query);

        if (parameters != null)
        {
            foreach (var param in parameters.OrderBy(p => p.Key))
            {
                builder.Append("|");
                builder.Append(param.Key);
                builder.Append("=");
                builder.Append(param.Value);
            }
        }

        var normalized = NormalizeForHashing(builder.ToString());
        var hash = ComputeHash(normalized);
        return TrackCreation($"{KeyPrefix}meta:{hash}");
    }

    /// <summary>
    /// Generates a batch cache key for multiple queries.
    /// Useful for caching batch analysis results.
    /// </summary>
    public string GenerateBatchKey(string[] queries)
    {
        ArgumentNullException.ThrowIfNull(queries);
        if (queries.Length == 0)
            throw new ArgumentException("Query list cannot be empty", nameof(queries));

        var combined = string.Join("|", queries.Select(q => ComputeHash(NormalizeForHashing(q))));
        var hash = ComputeHash(combined);
        return TrackCreation($"{KeyPrefix}batch:{hash}");
    }

    /// <summary>
    /// Records the creation time of a generated key and returns it unchanged.
    /// </summary>
    private static string TrackCreation(string key)
    {
        s_keyCreatedAt[key] = DateTimeOffset.UtcNow;
        return key;
    }

    /// <summary>
    /// Gets the time a cache key was generated, if this instance (or another instance in the
    /// same process) has generated it. Returns null for keys generated in a previous process
    /// or never seen by this generator.
    /// </summary>
    public DateTimeOffset? GetKeyCreatedAt(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        return s_keyCreatedAt.TryGetValue(key, out var createdAt) ? createdAt : null;
    }

    /// <summary>
    /// Normalizes query for consistent hashing.
    /// Uses QueryNormalizer to ensure full normalization including parameterization.
    /// </summary>
    private string NormalizeForHashing(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // Use QueryNormalizer for full normalization including:
        // - Lowercase keywords
        // - Parameterization (replaces literals with placeholders)
        // - Comment removal
        // - Whitespace normalization
        var normalizer = new QueryNormalizer();
        return normalizer.ToParameterizedQuery(input);
    }

    /// <summary>
    /// Computes SHA256 hash of input string.
    /// Returns hex-encoded hash.
    /// </summary>
    private string ComputeHash(string input)
    {
        ArgumentException.ThrowIfNullOrEmpty(input);

        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));

        // Convert to hex string for cache key compatibility
        var builder = new StringBuilder();
        foreach (var b in hashedBytes)
        {
            builder.Append(b.ToString("x2"));
        }

        return builder.ToString();
    }

    /// <summary>
    /// Validates if a key was generated by this generator.
    /// Checks for proper prefix and format.
    /// </summary>
    public bool IsValidCacheKey(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        return key.StartsWith(KeyPrefix) &&
            (key.Contains(QueryHashPrefix) ||
             key.Contains(ResultHashPrefix) ||
             key.Contains("meta:") ||
             key.Contains("batch:"));
    }

    /// <summary>
    /// Extracts query hash from a cache key.
    /// Useful for debugging and statistics.
    /// </summary>
    public string? ExtractHashFromKey(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        if (!IsValidCacheKey(key))
            return null;

        var parts = key.Split(":");
        return parts.Length > 2 ? parts.Last() : null;
    }

    /// <summary>
    /// Gets the type of cache key.
    /// </summary>
    public CacheKeyType GetKeyType(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        if (key.Contains(QueryHashPrefix))
            return CacheKeyType.Query;
        if (key.Contains(ResultHashPrefix))
            return CacheKeyType.Result;
        if (key.Contains("meta:"))
            return CacheKeyType.Metadata;
        if (key.Contains("batch:"))
            return CacheKeyType.Batch;

        return CacheKeyType.Unknown;
    }
}

/// <summary>
/// Type of cache key.
/// </summary>
public enum CacheKeyType
{
    Query,
    Result,
    Metadata,
    Batch,
    Unknown
}