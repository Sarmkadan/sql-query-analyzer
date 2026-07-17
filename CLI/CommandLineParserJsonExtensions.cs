#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace SqlQueryAnalyzer.CLI;

/// <summary>
/// Provides JSON serialization and deserialization extensions for CommandLineArguments.
/// </summary>
public static class CommandLineParserJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>
    /// Serializes a CommandLineArguments instance to a JSON string.
    /// </summary>
    /// <param name="value">The CommandLineArguments instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the CommandLineArguments.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this CommandLineArguments value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string into a CommandLineArguments instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A CommandLineArguments instance populated from the JSON data, or null if the JSON is empty or whitespace.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or whitespace.</exception>
    public static CommandLineArguments? FromJson(string json)
    {
    
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<CommandLineArguments>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a CommandLineArguments instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized CommandLineArguments instance if successful, otherwise null.</param>
/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>

/// <returns>True if deserialization succeeded; otherwise, false.</returns>
/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or whitespace.</exception>
    public static bool TryFromJson(string json, out CommandLineArguments? value)
    {
    
        try
        {
            value = JsonSerializer.Deserialize<CommandLineArguments>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}