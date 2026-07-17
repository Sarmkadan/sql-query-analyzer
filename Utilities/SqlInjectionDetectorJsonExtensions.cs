#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SqlQueryAnalyzer.Utilities;

/// <summary>
/// Provides System.Text.Json serialization extensions for serializing and deserializing
/// <see cref="SqlInjectionDetector"/> and related types.
/// </summary>
public static class SqlInjectionDetectorJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes a <see cref="SqlInjectionDetector"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The detector instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the detector.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this SqlInjectionDetector value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="SqlInjectionDetector"/> instance.
    /// Note: SqlInjectionDetector requires an <see cref="ILogger{SqlInjectionDetector}"/> dependency through its constructor,
    /// so deserialization is not supported. This method returns null.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>Always null, as SqlInjectionDetector cannot be deserialized.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is malformed.</exception>
    public static SqlInjectionDetector? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            // Attempt to deserialize to verify JSON is valid
            JsonSerializer.Deserialize<SqlInjectionDetector>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            throw;
        }

        // SqlInjectionDetector requires ILogger dependency through constructor
        // and cannot be deserialized
        return null;
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="SqlInjectionDetector"/> instance.
    /// Note: SqlInjectionDetector requires an <see cref="ILogger{SqlInjectionDetector}"/> dependency through its constructor,
    /// so deserialization is not supported.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives null, as deserialization is not supported.</param>
    /// <returns>Always false, as SqlInjectionDetector cannot be deserialized.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson(string json, out SqlInjectionDetector? value)
    {
        value = null;

        if (string.IsNullOrEmpty(json))
        {
            return false;
        }

        try
        {
            JsonSerializer.Deserialize<SqlInjectionDetector>(json, _jsonOptions);
            // JSON is valid, but we still can't create a SqlInjectionDetector instance
            return false;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Serializes a <see cref="SqlInjectionIssue"/> to a JSON string.
    /// </summary>
    /// <param name="value">The issue to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the issue.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this SqlInjectionIssue value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="SqlInjectionIssue"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized <see cref="SqlInjectionIssue"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is malformed or cannot be deserialized.</exception>
    public static SqlInjectionIssue? FromJsonToSqlInjectionIssue(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<SqlInjectionIssue>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="SqlInjectionIssue"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized instance if successful, otherwise null.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson(string json, out SqlInjectionIssue? value)
    {
        value = string.IsNullOrEmpty(json)
            ? null
            : TryDeserialize(json);

        return value is not null;
    }

    private static SqlInjectionIssue? TryDeserialize(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<SqlInjectionIssue>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
