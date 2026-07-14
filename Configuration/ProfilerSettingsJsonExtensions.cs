using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace SqlQueryAnalyzer.Configuration;

public static class ProfilerSettingsJsonExtensions
{
    private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        WriteIndented = false,
    };

    /// <summary>
    /// Serializes the <see cref="ProfilerSettings"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The settings to serialize.</param>
    /// <param name="indented">Whether to indent the JSON for readability.</param>
    /// <returns>A JSON string representation of the settings.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static string ToJson(this ProfilerSettings value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_options) { WriteIndented = true }
            : _options;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string into a <see cref="ProfilerSettings"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized settings, or null if the JSON is null or empty.</returns>
    /// <exception cref="JsonException">Thrown if the JSON is invalid or cannot be deserialized.</exception>
    public static ProfilerSettings? FromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<ProfilerSettings>(json, _options);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="ProfilerSettings"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized settings if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out ProfilerSettings? value)
    {
        value = null;

        if (string.IsNullOrEmpty(json))
        {
            return true;
        }

        try
        {
            value = JsonSerializer.Deserialize<ProfilerSettings>(json, _options);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}