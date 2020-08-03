#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;

namespace SqlQueryAnalyzer.API;

/// <summary>
/// Provides JSON serialization helpers for <see cref="AnalysisController"/>.
/// </summary>
public static class AnalysisControllerJsonExtensions
{
    /// <summary>
    /// Configured JSON serializer options with camelCase naming policy.
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes an <see cref="AnalysisController"/> instance to JSON.
    /// </summary>
    /// <param name="value">The controller to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>JSON string representation of the controller.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this AnalysisController value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        JsonOptions.WriteIndented = indented;
        return JsonSerializer.Serialize(value, JsonOptions);
    }

    /// <summary>
    /// Deserializes a JSON string to an <see cref="AnalysisController"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized controller, or null if input is empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown when JSON is invalid.</exception>
    public static AnalysisController? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        return JsonSerializer.Deserialize<AnalysisController>(json, JsonOptions);
    }

    /// <summary>
    /// Tries to deserialize a JSON string to an <see cref="AnalysisController"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized controller, or null on failure.</param>
    /// <returns>True if deserialization succeeded, false otherwise.</returns>
    public static bool TryFromJson(string json, out AnalysisController? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        try
        {
            value = JsonSerializer.Deserialize<AnalysisController>(json, JsonOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
