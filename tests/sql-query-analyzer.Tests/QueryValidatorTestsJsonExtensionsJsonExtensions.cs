#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace SqlQueryAnalyzer.Tests;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="QueryValidatorTests"/>
/// type markers.
/// </summary>
public static class QueryValidatorTestsJsonExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        WriteIndented = false
    };

    /// <summary>
    /// Serializes a type marker representing <see cref="QueryValidatorTests"/> to a JSON string.
    /// </summary>
    /// <param name="value">This parameter is ignored; only the type context is used.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the query validator tests type marker.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this object value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(new { Type = nameof(QueryValidatorTests) }, options);
    }

    /// <summary>
    /// Deserializes a JSON string into a type marker representing <see cref="QueryValidatorTests"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A type marker object, or <see langword="null"/> if the JSON is empty.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static object? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<object>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a type marker representing <see cref="QueryValidatorTests"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized type marker if successful.</param>
    /// <returns><see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/>.</returns>
    public static bool TryFromJson(string json, out object? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<object>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}