using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace SqlQueryAnalyzer.Benchmarks;

/// <summary>
/// Provides JSON serialization extensions for <see cref="SqlPatternAnalyzerBenchmarks"/>.
/// </summary>
public static class SqlPatternAnalyzerBenchmarksJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
    };

    /// <summary>
    /// Serializes the <see cref="SqlPatternAnalyzerBenchmarks"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The benchmarks instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the benchmarks.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this SqlPatternAnalyzerBenchmarks value, bool indented = false) =>
        JsonSerializer.Serialize(value, indented ? GetIndentedOptions() : _jsonOptions);

    /// <summary>
    /// Deserializes a JSON string to a <see cref="SqlPatternAnalyzerBenchmarks"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized benchmarks instance, or <see langword="null"/> if the JSON is empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is <see langword="null"/>.</exception>
    public static SqlPatternAnalyzerBenchmarks? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        return json.Length == 0 ? null : JsonSerializer.Deserialize<SqlPatternAnalyzerBenchmarks>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="SqlPatternAnalyzerBenchmarks"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized instance if successful.</param>
    /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is <see langword="null"/>.</exception>
    public static bool TryFromJson(string json, out SqlPatternAnalyzerBenchmarks? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        value = null;
        if (json.Length == 0)
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<SqlPatternAnalyzerBenchmarks>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static JsonSerializerOptions GetIndentedOptions()
    {
        var options = new JsonSerializerOptions(_jsonOptions)
        {
            WriteIndented = true,
        };
        return options;
    }
}