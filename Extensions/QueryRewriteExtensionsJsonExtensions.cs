#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================

using System;
using System.Collections.Generic;
using System.Text.Json;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Extensions;

/// <summary>
/// System.Text.Json serialization extensions for query rewrite types.
/// Provides methods to serialize and deserialize <see cref="QueryRewriteSuggestion"/> and related types.
/// </summary>
public static class QueryRewriteExtensionsJsonExtensions
{
    /// <summary>
    /// Shared JSON serialization options with camelCase property naming.
    /// </summary>
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        IgnoreNullValues = true
    };

    /// <summary>
    /// Serializes a <see cref="QueryRewriteSuggestion"/> to a JSON string.
    /// </summary>
    /// <param name="suggestion">The suggestion to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the suggestion.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="suggestion"/> is null.</exception>
    public static string ToJson(this QueryRewriteSuggestion suggestion, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(suggestion);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions)
            {
                WriteIndented = true
            }
            : _jsonOptions;

        return JsonSerializer.Serialize(suggestion, options);
    }

    /// <summary>
    /// Serializes a collection of <see cref="QueryRewriteSuggestion"/> to a JSON string.
    /// </summary>
    /// <param name="suggestions">The suggestions to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the suggestions.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="suggestions"/> is null.</exception>
    public static string ToJson(this IEnumerable<QueryRewriteSuggestion> suggestions, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(suggestions);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions)
            {
                WriteIndented = true
            }
            : _jsonOptions;

        return JsonSerializer.Serialize(suggestions, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="QueryRewriteSuggestion"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized instance, or null if the JSON is invalid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is malformed or cannot be deserialized.</exception>
    public static QueryRewriteSuggestion? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        return JsonSerializer.Deserialize<QueryRewriteSuggestion>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="QueryRewriteSuggestion"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized instance if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    public static bool TryFromJson(string json, out QueryRewriteSuggestion? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = JsonSerializer.Deserialize<QueryRewriteSuggestion>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}