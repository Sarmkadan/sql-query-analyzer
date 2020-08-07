#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Provides JSON serialization helpers for <see cref="QueryRewriteSuggestion"/>.
/// </summary>
public static class QueryRewriteSuggestionJsonExtensions
{
    /// <summary>
    /// Configured JSON serializer options with camelCase naming policy.
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes a <see cref="QueryRewriteSuggestion"/> instance to JSON.
    /// </summary>
    /// <param name="value">The query rewrite suggestion to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>JSON string representation of the query rewrite suggestion.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this QueryRewriteSuggestion value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        JsonOptions.WriteIndented = indented;
        return JsonSerializer.Serialize(value, JsonOptions);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="QueryRewriteSuggestion"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized query rewrite suggestion, or null if input is empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown when JSON is invalid.</exception>
    public static QueryRewriteSuggestion? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        return JsonSerializer.Deserialize<QueryRewriteSuggestion>(json, JsonOptions);
    }

    /// <summary>
    /// Tries to deserialize a JSON string to a <see cref="QueryRewriteSuggestion"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized query rewrite suggestion, or null on failure.</param>
    /// <returns>True if deserialization succeeded, false otherwise.</returns>
    public static bool TryFromJson(string json, out QueryRewriteSuggestion? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        try
        {
            value = JsonSerializer.Deserialize<QueryRewriteSuggestion>(json, JsonOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}