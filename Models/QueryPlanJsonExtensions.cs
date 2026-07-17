#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System;
using System.Text.Json;

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="QueryPlan"/> and related types.
/// Enables serialization of execution plans to JSON format and deserialization from JSON.
/// </summary>
public static class QueryPlanJsonExtensions
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
    /// Serializes a <see cref="QueryPlan"/> to a JSON string.
    /// </summary>
    /// <param name="value">The query plan to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the query plan.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this QueryPlan value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string into a <see cref="QueryPlan"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A query plan, or <see langword="null"/> if the JSON is empty or whitespace.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <see langword="null"/>, empty, or consists only of whitespace.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized into a <see cref="QueryPlan"/>.</exception>
    public static QueryPlan? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<QueryPlan>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="QueryPlan"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized query plan if successful; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <see langword="null"/> or empty.</exception>
    public static bool TryFromJson(string json, out QueryPlan? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<QueryPlan>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}