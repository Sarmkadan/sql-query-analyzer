#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Utilities;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="StatisticsAggregator"/>.
/// Allows serialization and deserialization of aggregated statistics.
/// </summary>
public static class StatisticsAggregatorJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    /// <summary>
    /// Serializes a <see cref="StatisticsAggregator"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The statistics aggregator to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the statistics aggregator.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this StatisticsAggregator value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string into a <see cref="StatisticsAggregator"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize. Must not be null or empty.</param>
    /// <returns>A deserialized <see cref="StatisticsAggregator"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is malformed or cannot be deserialized into a <see cref="StatisticsAggregator"/> instance.</exception>
    public static StatisticsAggregator FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            return JsonSerializer.Deserialize<StatisticsAggregator>(json, _jsonOptions)
                ?? throw new JsonException("Deserialization returned null - JSON may be invalid or incomplete");
        }
        catch (JsonException ex)
        {
            throw new JsonException("Failed to deserialize StatisticsAggregator from JSON", ex);
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="StatisticsAggregator"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize. Must not be null or empty.</param>
    /// <param name="value">Receives the deserialized <see cref="StatisticsAggregator"/> instance if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson(string json, out StatisticsAggregator? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        value = null;
        try
        {
            value = JsonSerializer.Deserialize<StatisticsAggregator>(json, _jsonOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}