using System.Text.Json;
using System.Text.Json.Serialization;

namespace SqlQueryAnalyzer.Diagnostics;

public static class AnalyzerHealthCheckJsonExtensions
{
    private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes an <see cref="AnalyzerHealthCheck"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The health check to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the health check.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this AnalyzerHealthCheck value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        return JsonSerializer.Serialize(value, indented ? new JsonSerializerOptions(jsonSerializerOptions) { WriteIndented = true } : jsonSerializerOptions);
    }

    /// <summary>
    /// Deserializes an <see cref="AnalyzerHealthCheck"/> instance from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized health check, or <see langword="null"/> if the JSON is empty.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static AnalyzerHealthCheck? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<AnalyzerHealthCheck>(json, jsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize an <see cref="AnalyzerHealthCheck"/> instance from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized health check if successful; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
    public static bool TryFromJson(string json, out AnalyzerHealthCheck? value)
    {
        try
        {
            value = FromJson(json);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}