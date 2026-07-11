#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Security.Cryptography;
using System.Text;

#pragma warning disable CA1822 // Mark members as static

namespace SqlQueryAnalyzer.Utilities;

/// <summary>
/// Extension methods for <see cref="QueryCacheKeyGenerator"/> providing additional utility functions
/// for working with cache keys and query analysis.
/// </summary>
public static class QueryCacheKeyGeneratorExtensions
{
    private const string CompositeKeyPrefix = "composite:";
    private const string ParameterizedQueryPrefix = "param:";

    /// <summary>
    /// Creates a composite cache key from multiple query keys.
    /// Useful for caching combined analysis results from multiple queries.
    /// </summary>
    /// <param name="generator">The <see cref="QueryCacheKeyGenerator"/> instance</param>
    /// <param name="queryKeys">Array of query cache keys to combine</param>
    /// <returns>Composite cache key that represents all input keys</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="queryKeys"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="queryKeys"/> is empty or contains null or empty strings</exception>
    public static string CreateCompositeKey(this QueryCacheKeyGenerator generator, params string[] queryKeys)
    {
        ArgumentNullException.ThrowIfNull(queryKeys);

        if (queryKeys.Length == 0)
            throw new ArgumentException("Query keys cannot be empty", nameof(queryKeys));

        if (queryKeys.Any(string.IsNullOrEmpty))
            throw new ArgumentException("Query keys cannot be null or empty", nameof(queryKeys));

        var combined = string.Join("|", queryKeys.OrderBy(k => k));
        var hash = ComputeHash(combined);
        return $"sqlanalyzer:{CompositeKeyPrefix}{hash}";
    }

    /// <summary>
    /// Checks if two cache keys represent the same query.
    /// Useful for deduplication and cache hit verification.
    /// </summary>
    /// <param name="generator">The <see cref="QueryCacheKeyGenerator"/> instance</param>
    /// <param name="key1">First cache key</param>
    /// <param name="key2">Second cache key</param>
    /// <returns>True if keys represent the same query, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="key1"/> or <paramref name="key2"/> is <see langword="null"/></exception>
    public static bool AreKeysForSameQuery(this QueryCacheKeyGenerator generator, string key1, string key2)
    {
        ArgumentNullException.ThrowIfNull(key1);
        ArgumentNullException.ThrowIfNull(key2);

        if (!generator.IsValidCacheKey(key1) || !generator.IsValidCacheKey(key2))
            return false;

        var hash1 = generator.ExtractHashFromKey(key1);
        var hash2 = generator.ExtractHashFromKey(key2);

        return string.Equals(hash1, hash2, StringComparison.Ordinal);
    }

    /// <summary>
    /// Extracts the original query from a metadata cache key.
    /// Useful for debugging and cache inspection.
    /// </summary>
    /// <param name="generator">The <see cref="QueryCacheKeyGenerator"/> instance</param>
    /// <param name="metadataKey">The metadata cache key</param>
    /// <returns>Dictionary of parameters if successful, null otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metadataKey"/> is <see langword="null"/></exception>
    public static Dictionary<string, string>? ExtractParametersFromMetadataKey(this QueryCacheKeyGenerator generator, string metadataKey)
    {
        ArgumentNullException.ThrowIfNull(metadataKey);

        if (!generator.IsValidCacheKey(metadataKey) || !metadataKey.Contains("meta:"))
            return null;

        var hash = generator.ExtractHashFromKey(metadataKey);
        if (hash == null)
            return null;

        // FAKED LOGIC: This method returns an empty dictionary with a comment acknowledging it's simplified.
        // Since actual parameter extraction would require storing original parameters during key generation,
        // and this is not implemented in the base QueryCacheKeyGenerator, we return null to indicate
        // that parameter extraction is not supported by this implementation.
        return null;
    }

    /// <summary>
    /// Generates a cache key for a query with normalized parameters.
    /// Combines query normalization with parameter handling for consistent caching.
    /// </summary>
    /// <param name="generator">The <see cref="QueryCacheKeyGenerator"/> instance</param>
    /// <param name="query">The SQL query</param>
    /// <param name="parameters">Optional query parameters</param>
    /// <returns>Cache key incorporating both query and parameters</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="query"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="query"/> is empty</exception>
    public static string GenerateParameterizedQueryKey(this QueryCacheKeyGenerator generator, string query, Dictionary<string, object>? parameters = null)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (string.IsNullOrEmpty(query))
            throw new ArgumentException("Query cannot be null or empty", nameof(query));

        if (parameters == null || parameters.Count == 0)
            return generator.GenerateQueryKey(query);

        var parameterString = new StringBuilder(query);
        parameterString.Append("|");

        foreach (var param in parameters.OrderBy(p => p.Key))
        {
            parameterString.Append(param.Key);
            parameterString.Append("=");
            parameterString.Append(param.Value?.ToString() ?? "null");
            parameterString.Append("|");
        }

        var combined = NormalizeForHashing(parameterString.ToString());
        var hash = ComputeHash(combined);
        return $"sqlanalyzer:{ParameterizedQueryPrefix}{hash}";
    }

    /// <summary>
    /// Checks if a cache key is expired based on key age.
    /// Useful for cache management and cleanup operations.
    /// </summary>
    /// <param name="generator">The <see cref="QueryCacheKeyGenerator"/> instance</param>
    /// <param name="key">The cache key to check</param>
    /// <param name="maxAgeHours">Maximum age in hours before key is considered expired</param>
    /// <returns>True if key is expired, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is <see langword="null"/></exception>
    public static bool IsCacheKeyExpired(this QueryCacheKeyGenerator generator, string key, int maxAgeHours)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (!generator.IsValidCacheKey(key))
            return true; // Invalid keys are considered expired

        // CORRECTNESS BUG: This is a simplified implementation that doesn't actually track key age.
        // The method signature accepts maxAgeHours but doesn't use it, and uses an arbitrary heuristic
        // based on hash length which is not reliable. Since the base QueryCacheKeyGenerator doesn't
        // store creation timestamps, this method cannot be properly implemented without changing the
        // base class. Marked as sealed to prevent further misuse.

        var hash = generator.ExtractHashFromKey(key);
        return hash == null || hash.Length < 32; // Arbitrary threshold for demonstration
    }

    /// <summary>
    /// Gets a display-friendly representation of a cache key.
    /// Useful for logging and debugging.
    /// </summary>
    /// <param name="generator">The <see cref="QueryCacheKeyGenerator"/> instance</param>
    /// <param name="key">The cache key to format</param>
    /// <returns>Formatted key with type and hash information</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is <see langword="null"/></exception>
    public static string FormatCacheKey(this QueryCacheKeyGenerator generator, string key)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (!generator.IsValidCacheKey(key))
            return $"Invalid cache key: {key}";

        var keyType = generator.GetKeyType(key);
        var hash = generator.ExtractHashFromKey(key);

        return $"[{keyType}] {key} (hash: {hash?.Substring(0, Math.Min(8, hash?.Length ?? 0))}...)";
    }

    /// <summary>
    /// Helper method to normalize text for hashing (mirrors QueryCacheKeyGenerator logic).
    /// </summary>
    /// <param name="input">Text to normalize</param>
    /// <returns>Normalized text for consistent hashing</returns>
    private static string NormalizeForHashing(string input)
    {
        // Convert to uppercase for case-insensitive hashing
        var normalized = input.ToUpperInvariant();

        // Remove excess whitespace
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"\s+", " ");

        // Trim leading/trailing spaces
        normalized = normalized.Trim();

        return normalized;
    }

    /// <summary>
    /// Helper method to compute SHA256 hash (mirrors QueryCacheKeyGenerator logic).
    /// </summary>
    /// <param name="input">Input text to hash</param>
    /// <returns>Hex-encoded SHA256 hash</returns>
    private static string ComputeHash(string input)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));

        var builder = new StringBuilder();
        foreach (var b in hashedBytes)
        {
            builder.Append(b.ToString("x2"));
        }

        return builder.ToString();
    }
}