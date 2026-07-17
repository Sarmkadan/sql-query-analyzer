#nullable enable

using System.Text.Json;

namespace SqlQueryAnalyzer.CLI;

/// <summary>
/// Provides JSON serialization helpers for <see cref="CliApplicationHost"/>.
/// </summary>
public static class CliApplicationHostJsonExtensions
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
    /// Serializes a <see cref="CliApplicationHost"/> instance to JSON.
    /// </summary>
    /// <param name="value">The host to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>JSON string representation of the host.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this CliApplicationHost value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = JsonOptions;
        if (indented)
        {
            options = new JsonSerializerOptions(JsonOptions)
            {
                PropertyNamingPolicy = JsonOptions.PropertyNamingPolicy,
                WriteIndented = true
            };
        }
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="CliApplicationHost"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized host.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty.</exception>
    /// <exception cref="JsonException">Thrown when JSON is invalid or cannot be deserialized to <see cref="CliApplicationHost"/>.</exception>
    public static CliApplicationHost? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        return JsonSerializer.Deserialize<CliApplicationHost>(json, JsonOptions);
    }

    /// <summary>
    /// Tries to deserialize a JSON string to a <see cref="CliApplicationHost"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized host, or null on failure.</param>
    /// <returns>True if deserialization succeeded, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty.</exception>
    public static bool TryFromJson(string json, out CliApplicationHost? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        try
        {
            value = JsonSerializer.Deserialize<CliApplicationHost>(json, JsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}