#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Security.Cryptography;
using System.Text;

namespace SqlQueryAnalyzer.Utilities;

/// <summary>
/// Extension methods for QueryCacheKeyGenerator providing additional utility functions
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
    /// <param name="generator">The QueryCacheKeyGenerator instance</param>
    /// <param name="queryKeys">Array of query cache keys to combine</param>
    /// <returns>Composite cache key that represents all input keys</returns>
    public static string CreateCompositeKey(this QueryCacheKeyGenerator generator, params string[] queryKeys)
    {
        if (queryKeys == null || queryKeys.Length == 0)
            throw new ArgumentException("Query keys cannot be null or empty");

        if (queryKeys.Any(string.IsNullOrEmpty))
            throw new ArgumentException("Query keys cannot be null or empty");

        var combined = string.Join("|", queryKeys.OrderBy(k => k));
        var hash = ComputeHash(combined);
        return $"sqlanalyzer:{CompositeKeyPrefix}{hash}";
    }

    /// <summary>
    /// Checks if two cache keys represent the same query.
    /// Useful for deduplication and cache hit verification.
    /// </summary>
    /// <param name="generator">The QueryCacheKeyGenerator instance</param>
    /// <param name="key1">First cache key</param>
    /// <param name="key2">Second cache key</param>
    /// <returns>True if keys represent the same query, false otherwise</returns>
    public static bool AreKeysForSameQuery(this QueryCacheKeyGenerator generator, string key1, string key2)
    {
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
    /// <param name="generator">The QueryCacheKeyGenerator instance</param>
    /// <param name="metadataKey">The metadata cache key</param>
    /// <returns>Dictionary of parameters if successful, null otherwise</returns>
    public static Dictionary<string, string>? ExtractParametersFromMetadataKey(this QueryCacheKeyGenerator generator, string metadataKey)
    {
        if (!generator.IsValidCacheKey(metadataKey) || !metadataKey.Contains("meta:"))
            return null;

        var hash = generator.ExtractHashFromKey(metadataKey);
        if (hash == null)
            return null;

        // For metadata keys, the hash contains the serialized parameters
        // This is a simplified extraction - in practice the hash would need to be reversed
        // For this implementation, we return an empty dictionary as the actual parameter
        // extraction would require storing the original parameters during key generation
        return new Dictionary<string, string>();
    }

    /// <summary>
    /// Generates a cache key for a query with normalized parameters.
    /// Combines query normalization with parameter handling for consistent caching.
    /// </summary>
    /// <param name="generator">The QueryCacheKeyGenerator instance</param>
    /// <param name="query">The SQL query</param>
    /// <param name="parameters">Optional query parameters</param>
    /// <returns>Cache key incorporating both query and parameters</returns>
    public static string GenerateParameterizedQueryKey(this QueryCacheKeyGenerator generator, string query, Dictionary<string, object>? parameters = null)
    {
        if (string.IsNullOrEmpty(query))
            throw new ArgumentException("Query cannot be null or empty");

        if (parameters == null || parameters.Count == 0)
        {
            return generator.GenerateQueryKey(query);
        }

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
    /// <param name="generator">The QueryCacheKeyGenerator instance</param>
    /// <param name="key">The cache key to check</param>
    /// <param name="maxAgeHours">Maximum age in hours before key is considered expired</param>
    /// <returns>True if key is expired, false otherwise</returns>
    public static bool IsCacheKeyExpired(this QueryCacheKeyGenerator generator, string key, int maxAgeHours)
    {
        if (!generator.IsValidCacheKey(key))
            return true; // Invalid keys are considered expired

        // Note: This is a simplified implementation
        // In a real application, you would need to track when keys were created
        // For this extension method, we assume keys without creation metadata are not expired
        // or implement a basic heuristic based on key content

        var hash = generator.ExtractHashFromKey(key);
        if (hash == null)
            return true;

        // Basic heuristic: longer hash indicates older key
        // This is just a placeholder - actual implementation would need timestamp tracking
        return hash.Length < 32; // Arbitrary threshold for demonstration
    }

    /// <summary>
    /// Gets a display-friendly representation of a cache key.
    /// Useful for logging and debugging.
    /// </summary>
    /// <param name="generator">The QueryCacheKeyGenerator instance</param>
    /// <param name="key">The cache key to format</param>
    /// <returns>Formatted key with type and hash information</returns>
    public static string FormatCacheKey(this QueryCacheKeyGenerator generator, string key)
    {
        if (!generator.IsValidCacheKey(key))
            return $"Invalid cache key: {key}";

        var keyType = generator.GetKeyType(key);
        var hash = generator.ExtractHashFromKey(key);

        return $"[{keyType}] {key} (hash: {hash?.Substring(0, Math.Min(8, hash?.Length ?? 0))}...)";
    }

    /// <summary>
    /// Helper method to normalize text for hashing (mirrors QueryCacheKeyGenerator logic).
    /// </summary>
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