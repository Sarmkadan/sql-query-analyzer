using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace SqlQueryAnalyzer.Configuration;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="ProfilerSettings"/> instances.
/// </summary>
public static class ProfilerSettingsJsonExtensions
{
    private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        WriteIndented = false,
    };

    /// <summary>
    /// Serializes the <see cref="ProfilerSettings"/> instance to a JSON string using camelCase property naming.
    /// </summary>
    /// <param name="value">The settings to serialize. Must not be <see langword="null"/>.</param>
    /// <param name="indented">Whether to indent the JSON for readability. When <see langword="true"/>, the output is formatted with indentation.</param>
    /// <returns>A JSON string representation of the settings with camelCase property names.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this ProfilerSettings value, bool indented = false) =>
        JsonSerializer.Serialize(value, indented ? new JsonSerializerOptions(_options) { WriteIndented = true } : _options);

    /// <summary>
    /// Deserializes a JSON string into a <see cref="ProfilerSettings"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize. Can be <see langword="null"/> or empty, in which case <see langword="null"/> is returned.</param>
    /// <returns>The deserialized settings instance, or <see langword="null"/> if the JSON is <see langword="null"/> or empty.</returns>
    /// <exception cref="JsonException">Thrown if the JSON is invalid or cannot be deserialized into a <see cref="ProfilerSettings"/> instance.</exception>
    public static ProfilerSettings? FromJson(string? json) =>
        string.IsNullOrEmpty(json)
            ? null
            : JsonSerializer.Deserialize<ProfilerSettings>(json, _options);

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="ProfilerSettings"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize. Can be <see langword="null"/> or empty, in which case the method returns <see langword="true"/> and <paramref name="value"/> is set to <see langword="null"/>.</param>
    /// <param name="value">Receives the deserialized settings if successful; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if deserialization succeeded or the JSON was <see langword="null"/>/empty; otherwise, <see langword="false"/>.</returns>
    public static bool TryFromJson(string? json, out ProfilerSettings? value)
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