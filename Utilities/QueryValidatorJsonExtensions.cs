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
/// Provides System.Text.Json serialization extensions for serializing and deserializing
/// <see cref="QueryValidator"/> and related validation types.
/// </summary>
public static class QueryValidatorJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes a <see cref="QueryValidator"/> instance to a JSON string.
    /// Note: QueryValidator is a static class and contains no state to serialize.
    /// This method returns a simple JSON object indicating the static class.
    /// </summary>
    /// <param name="value">The validator instance to serialize (always null for static class).</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation indicating this is a static QueryValidator class.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(object? value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(new { Type = "QueryValidator.StaticClass" }, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="QueryValidator"/> instance.
    /// Note: QueryValidator is a static class and cannot be deserialized.
    /// This method always returns null.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>Always null, as QueryValidator is a static class and cannot be instantiated.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is malformed.</exception>
    public static object? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            // Attempt to deserialize to verify JSON is valid
            JsonSerializer.Deserialize<object>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            throw;
        }

        // QueryValidator is static and cannot be instantiated
        return null;
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="QueryValidator"/> instance.
    /// Note: QueryValidator is a static class and cannot be deserialized.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives null, as QueryValidator cannot be deserialized.</param>
    /// <returns>Always false, as QueryValidator is a static class and cannot be instantiated.</returns>
    public static bool TryFromJson(string json, out object? value)
    {
        value = null;

        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            JsonSerializer.Deserialize<object>(json, _jsonOptions);
            // JSON is valid, but we still can't create a QueryValidator instance
            return false;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
