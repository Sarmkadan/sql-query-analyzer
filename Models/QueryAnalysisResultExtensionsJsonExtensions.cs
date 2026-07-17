#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ====================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="QueryAnalysisResult"/>
/// to enable JSON serialization of <see cref="QueryAnalysisResult"/> instances.
/// </summary>
public static class QueryAnalysisResultExtensionsJsonExtensions
{
    /// <summary>
    /// Shared JSON serialization options with camelCase property naming.
    /// </summary>
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>
    /// Serializes a <see cref="QueryAnalysisResult"/> to a JSON string.
    /// </summary>
    /// <param name="value">The query analysis result to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the query analysis result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(QueryAnalysisResult value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a <see cref="QueryAnalysisResult"/> from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized query analysis result, or <see langword="null"/> if the JSON is empty, invalid, or whitespace.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="json"/> is empty or whitespace.</exception>
    public static QueryAnalysisResult? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            return JsonSerializer.Deserialize<QueryAnalysisResult>(json, _jsonSerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a <see cref="QueryAnalysisResult"/> from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized query analysis result if successful.</param>
    /// <returns><see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson(string json, out QueryAnalysisResult? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<QueryAnalysisResult>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}