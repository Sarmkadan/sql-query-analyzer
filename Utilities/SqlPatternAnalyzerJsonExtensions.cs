#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Text.Json;

namespace SqlQueryAnalyzer.Utilities;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="SqlPatternAnalyzer"/> analysis results.
/// <para>
/// Note: <see cref="SqlPatternAnalyzer"/> is a static class that performs SQL pattern analysis.
/// This extension class provides serialization capabilities for analysis results and patterns
/// that can be serialized. If <see cref="SqlPatternAnalyzer"/> is refactored to be an instance-based
/// class in the future, implement additional serialization methods here.
/// </para>
/// </summary>
public static class SqlPatternAnalyzerJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    /// <summary>
    /// Serializes a SQL query string to a JSON string.
    /// </summary>
    /// <param name="query">The SQL query to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the SQL query.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is <see langword="null"/>.</exception>
    public static string ToJson(this string query, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(query);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(query, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a SQL query string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized SQL query string, or <see langword="null"/> if the JSON is empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is <see langword="null"/>.</exception>
    public static string? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        return json.Length == 0 ? null : JsonSerializer.Deserialize<string>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a SQL query string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="query">Receives the deserialized query string if successful.</param>
    /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is <see langword="null"/>.</exception>
    public static bool TryFromJson(string json, out string? query)
    {
        ArgumentNullException.ThrowIfNull(json);

        query = null;
        if (json.Length == 0)
        {
            return false;
        }

        try
        {
            query = JsonSerializer.Deserialize<string>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Serializes a list of SQL query strings to a JSON string.
    /// </summary>
    /// <param name="queries">The list of SQL queries to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the SQL queries.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="queries"/> is <see langword="null"/>.</exception>
    public static string ToJson(this System.Collections.Generic.List<string> queries, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(queries);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(queries, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a list of SQL query strings.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized list of SQL queries, or <see langword="null"/> if the JSON is empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is <see langword="null"/>.</exception>
    public static System.Collections.Generic.List<string>? FromJsonToQueriesList(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        return json.Length == 0 ? null : JsonSerializer.Deserialize<System.Collections.Generic.List<string>>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a list of SQL query strings.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="queries">Receives the deserialized list of queries if successful.</param>
    /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is <see langword="null"/>.</exception>
    public static bool TryFromJson(string json, out System.Collections.Generic.List<string>? queries)
    {
        ArgumentNullException.ThrowIfNull(json);

        queries = null;
        if (json.Length == 0)
        {
            return false;
        }

        try
        {
            queries = JsonSerializer.Deserialize<System.Collections.Generic.List<string>>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
