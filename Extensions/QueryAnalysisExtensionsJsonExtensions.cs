#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Text.Json;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Extensions;

/// <summary>
/// Provides System.Text.Json serialization extensions for query analysis result collections.
/// Complements the <see cref="QueryAnalysisExtensions"/> static class by enabling serialization
/// of <see cref="QueryAnalysisResult"/> and related types.
/// </summary>
public static class QueryAnalysisExtensionsJsonExtensions
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
    /// Serializes a collection of <see cref="QueryAnalysisResult"/> to a JSON string.
    /// </summary>
    /// <param name="value">The collection of analysis results to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the analysis results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this IEnumerable<QueryAnalysisResult> value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string into a collection of <see cref="QueryAnalysisResult"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A collection of analysis results, or <see langword="null"/> if the JSON is empty or whitespace.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <see langword="null"/>, empty, or consists only of whitespace.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static IEnumerable<QueryAnalysisResult>? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<IEnumerable<QueryAnalysisResult>>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a collection of <see cref="QueryAnalysisResult"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized collection if successful; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <see langword="null"/> or empty.</exception>
    public static bool TryFromJson(string json, out IEnumerable<QueryAnalysisResult>? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<IEnumerable<QueryAnalysisResult>>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}