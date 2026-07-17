// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Text.Json;

namespace SqlQueryAnalyzer.Tests;

/// <summary>
/// Provides JSON serialization extensions for <see cref="ExplainPlanParserTests"/> instances.
/// Enables round-trip serialization/deserialization of test data for persistence and transport.
/// </summary>
public static class ExplainPlanParserTestsJsonExtensions
{
    /// <summary>
    /// JSON serializer options configured with camelCase naming policy for consistent serialization.
    /// </summary>
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    /// <summary>
    /// Serializes the specified <see cref="ExplainPlanParserTests"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The test instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the test instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this ExplainPlanParserTests value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        return JsonSerializer.Serialize(value, indented ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true } : _jsonOptions);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="ExplainPlanParserTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize. Must not be null or empty.</param>
    /// <returns>The deserialized test instance, or null if the JSON is empty or whitespace.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static ExplainPlanParserTests? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<ExplainPlanParserTests>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="ExplainPlanParserTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize. Must not be null or empty.</param>
    /// <param name="value">Receives the deserialized test instance if successful; otherwise, null.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out ExplainPlanParserTests? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<ExplainPlanParserTests>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}