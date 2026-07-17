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
/// Provides System.Text.Json serialization helpers for <see cref="PerformanceMetricCollector"/> instances.
/// Includes methods for converting performance metric collectors to and from JSON strings.
/// </summary>
public static class PerformanceMetricCollectorExtensionsJsonExtensions
{
    /// <summary>
    /// Configured JSON serializer options with camelCase naming policy.
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes a <see cref="PerformanceMetricCollector"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The performance metric collector to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>JSON string representation of the performance metric collector.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this PerformanceMetricCollector value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(JsonOptions) { WriteIndented = true }
            : JsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="PerformanceMetricCollector"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized performance metric collector, or <see langword="null"/> if input is empty.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="JsonException">Thrown when JSON is invalid.</exception>
    public static PerformanceMetricCollector? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        return JsonSerializer.Deserialize<PerformanceMetricCollector>(json, JsonOptions);
    }

    /// <summary>
    /// Tries to deserialize a JSON string to a <see cref="PerformanceMetricCollector"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized performance metric collector, or <see langword="null"/> on failure.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out PerformanceMetricCollector? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<PerformanceMetricCollector>(json, JsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}