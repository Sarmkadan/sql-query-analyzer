#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SqlQueryAnalyzer.Utilities;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="QueryNormalizer"/>.
/// </summary>
public static class QueryNormalizerJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes a <see cref="QueryNormalizer"/> type reference to a JSON string.
    /// Note: QueryNormalizer is a utility class with no state to serialize.
    /// This method returns a JSON object indicating this is a QueryNormalizer type.
    /// </summary>
    /// <param name="value">The QueryNormalizer type reference to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation indicating this is a QueryNormalizer type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this QueryNormalizer? value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(new { Type = "QueryNormalizer.UtilityClass" }, options);
    }

    /// <summary>
    /// Deserializes a JSON string into a <see cref="QueryNormalizer"/> instance.
    /// Note: QueryNormalizer is a utility class with no state and cannot be instantiated.
    /// This method always returns null.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>Always null, as QueryNormalizer is a utility class and cannot be instantiated.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="json"/> is empty or whitespace.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is malformed.</exception>
    public static QueryNormalizer? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        // Attempt to deserialize to verify JSON is valid
        JsonSerializer.Deserialize<object>(json, _jsonOptions);

        // QueryNormalizer is a utility class with no state to deserialize
        return null;
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="QueryNormalizer"/> instance.
    /// Note: QueryNormalizer is a utility class with no state and cannot be instantiated.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives null, as QueryNormalizer cannot be deserialized.</param>
    /// <returns>Always false, as QueryNormalizer is a utility class and cannot be instantiated.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="json"/> is empty or whitespace.</exception>
    public static bool TryFromJson(string json, out QueryNormalizer? value)
    {
        value = null;

        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            JsonSerializer.Deserialize<object>(json, _jsonOptions);
            // JSON is valid, but we still can't create a QueryNormalizer instance
            return false;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Serializes a parameterized query representation to JSON.
    /// </summary>
    /// <param name="query">The SQL query to parameterize and serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string containing the parameterized query.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="query"/> is null.</exception>
    public static string ToParameterizedJson(this QueryNormalizer? normalizer, string query, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(normalizer);
        ArgumentNullException.ThrowIfNull(query);

        var parameterized = normalizer.ToParameterizedQuery(query);
        var result = new { ParameterizedQuery = parameterized };

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(result, options);
    }
}