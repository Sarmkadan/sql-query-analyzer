#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;

namespace SqlQueryAnalyzer.Integration;

/// <summary>
/// Provides System.Text.Json serialization extensions for HttpQueryAnalysisClient.
/// Enables easy JSON serialization/deserialization of HttpQueryAnalysisClient instances.
/// </summary>
public static class HttpQueryAnalysisClientJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>
    /// Serializes the HttpQueryAnalysisClient instance to a JSON string.
    /// </summary>
    /// <param name="value">The HttpQueryAnalysisClient instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the HttpQueryAnalysisClient.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this HttpQueryAnalysisClient value, bool indented = false) =>
        value is null
            ? throw new ArgumentNullException(nameof(value))
            : JsonSerializer.Serialize(value, GetOptions(indented));

    /// <summary>
    /// Deserializes a JSON string to an HttpQueryAnalysisClient instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>An HttpQueryAnalysisClient instance, or <see langword="null"/> if deserialization fails.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>, empty, or whitespace.</exception>
    public static HttpQueryAnalysisClient? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        try
        {
            return JsonSerializer.Deserialize<HttpQueryAnalysisClient>(json, _jsonSerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to an HttpQueryAnalysisClient instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized HttpQueryAnalysisClient instance, or <see langword="null"/> if deserialization fails.</param>
    /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>, empty, or whitespace.</exception>
    public static bool TryFromJson(string json, out HttpQueryAnalysisClient? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        try
        {
            value = JsonSerializer.Deserialize<HttpQueryAnalysisClient>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    private static JsonSerializerOptions GetOptions(bool indented) => indented
        ? new JsonSerializerOptions(_jsonSerializerOptions)
        {
            WriteIndented = true,
        }
        : _jsonSerializerOptions;
}