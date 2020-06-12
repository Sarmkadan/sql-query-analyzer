#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace SqlQueryAnalyzer.Tests;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="QueryValidatorTests"/>.
/// </summary>
public static class QueryValidatorTestsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>
    /// Serializes a <see cref="QueryValidatorTests"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the instance.</returns>
    public static string ToJson(this QueryValidatorTests value, bool indented = false)
    {
        if (value is null)
        {
            return "null";
        }

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions)
            {
                WriteIndented = true,
            }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="QueryValidatorTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized instance, or null if the JSON is null or empty.</returns>
    public static QueryValidatorTests? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "null")
        {
            return null;
        }

        return JsonSerializer.Deserialize<QueryValidatorTests>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="QueryValidatorTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized instance if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out QueryValidatorTests? value)
    {
        value = default;

        if (string.IsNullOrWhiteSpace(json) || json == "null")
        {
            return true;
        }

        try
        {
            value = JsonSerializer.Deserialize<QueryValidatorTests>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}