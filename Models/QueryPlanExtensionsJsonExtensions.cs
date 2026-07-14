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
/// Provides System.Text.Json serialization extensions for <see cref="QueryPlanExtensions"/>.
/// Enables easy JSON serialization/deserialization of type markers for QueryPlanExtensions.
/// </summary>
public static class QueryPlanExtensionsJsonExtensions
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
    /// Serializes a type marker representing QueryPlanExtensions to a JSON string.
    /// </summary>
    /// <param name="value">This parameter is ignored; only the type context is used.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the QueryPlanExtensions type marker.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this object value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(new { Type = nameof(QueryPlanExtensions) }, options);
    }

    /// <summary>
    /// Deserializes a JSON string into a type marker representing QueryPlanExtensions.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A type marker object, or <see langword="null"/> if the JSON is empty or invalid.</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is null or empty.</exception>
    public static object? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            return JsonSerializer.Deserialize<object>(json, _jsonSerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a type marker representing QueryPlanExtensions.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized type marker if successful.</param>
    /// <returns><see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson(string json, out object? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<object>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}