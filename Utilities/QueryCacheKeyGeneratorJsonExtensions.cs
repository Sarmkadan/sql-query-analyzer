#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace SqlQueryAnalyzer.Utilities;

/// <summary>
/// Provides System.Text.Json serialization helpers for <see cref="QueryCacheKeyGenerator"/>.
/// Enables JSON serialization and deserialization of cache key generators.
/// </summary>
public static class QueryCacheKeyGeneratorJsonExtensions
{
    /// <summary>
    /// Gets the default JSON serialization options for <see cref="QueryCacheKeyGenerator"/>.
    /// Uses camelCase property naming and excludes type information for compact serialization.
    /// </summary>
    private static readonly JsonSerializerOptions s_jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
    };

    /// <summary>
    /// Serializes a <see cref="QueryCacheKeyGenerator"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The cache key generator to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the cache key generator.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static string ToJson(this QueryCacheKeyGenerator value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(s_jsonOptions) { WriteIndented = true }
            : s_jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a <see cref="QueryCacheKeyGenerator"/> instance from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized cache key generator instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is empty or whitespace.</exception>
    /// <exception cref="JsonException">Thrown if the JSON is invalid or cannot be deserialized.</exception>
    public static QueryCacheKeyGenerator? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        return JsonSerializer.Deserialize<QueryCacheKeyGenerator>(json, s_jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a <see cref="QueryCacheKeyGenerator"/> instance from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized cache key generator if successful.</param>
    /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is empty or whitespace.</exception>
    public static bool TryFromJson(string json, out QueryCacheKeyGenerator? value)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        try
        {
            value = JsonSerializer.Deserialize<QueryCacheKeyGenerator>(json, s_jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}