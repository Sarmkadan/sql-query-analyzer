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
/// <see cref="ReportGenerator"/> static class.
/// Note: ReportGenerator is a static class with no state to serialize.
/// </summary>
public static class ReportGeneratorJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes a <see cref="ReportGenerator"/> static class reference to a JSON string.
    /// Note: ReportGenerator is a static class and contains no state to serialize.
    /// This method returns a JSON object indicating this is a ReportGenerator type.
    /// </summary>
    /// <param name="value">The ReportGenerator static class reference to serialize (always null for static class).</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation indicating this is a ReportGenerator static class.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this object? value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(new { Type = "ReportGenerator.StaticClass" }, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="ReportGenerator"/> instance.
    /// Note: ReportGenerator is a static class and cannot be deserialized.
    /// This method always returns null.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>Always null, as ReportGenerator is a static class and cannot be instantiated.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
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

        // ReportGenerator is static and cannot be instantiated
        return null;
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="ReportGenerator"/> instance.
    /// Note: ReportGenerator is a static class and cannot be deserialized.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives null, as ReportGenerator cannot be deserialized.</param>
    /// <returns>Always false, as ReportGenerator is a static class and cannot be instantiated.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson(string json, out object? value)
    {
        value = null;

        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            JsonSerializer.Deserialize<object>(json, _jsonOptions);
            // JSON is valid, but we still can't create a ReportGenerator instance
            return false;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
