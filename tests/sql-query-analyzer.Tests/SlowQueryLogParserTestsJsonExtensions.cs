using System.Text.Json;
using System.Text.Json.Serialization;

namespace SqlQueryAnalyzer.Tests
{
/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="SlowQueryLogParserTests"/> instances.
/// </summary>
public static class SlowQueryLogParserTestsJsonExtensions
{
private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
{
PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
WriteIndented = false,
DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};

/// <summary>
/// Serializes a <see cref="SlowQueryLogParserTests"/> instance to a JSON string.
/// </summary>
/// <param name="value">The instance to serialize. Cannot be null.</param>
/// <param name="indented">Whether to format the JSON with indentation for readability.</param>
/// <returns>A JSON string representation of the instance.</returns>
/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
public static string ToJson(this SlowQueryLogParserTests? value, bool indented = false)
{
ArgumentNullException.ThrowIfNull(value, nameof(value));

var options = indented
? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
: _jsonOptions;

return JsonSerializer.Serialize(value, options);
}

/// <summary>
/// Deserializes a JSON string to a <see cref="SlowQueryLogParserTests"/> instance.
/// </summary>
/// <param name="json">The JSON string to deserialize. Cannot be null or whitespace.</param>
/// <returns>The deserialized instance, or null if deserialization fails.</returns>
/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or whitespace.</exception>
public static SlowQueryLogParserTests? FromJson(string json)
{
ArgumentException.ThrowIfNullOrWhiteSpace(json, nameof(json));

try
{
return JsonSerializer.Deserialize<SlowQueryLogParserTests>(json, _jsonOptions);
}
catch (JsonException)
{
return null;
}
}

/// <summary>
/// Attempts to deserialize a JSON string to a <see cref="SlowQueryLogParserTests"/> instance.
/// </summary>
/// <param name="json">The JSON string to deserialize. Cannot be null or whitespace.</param>
/// <param name="value">Receives the deserialized instance if successful; otherwise, null.</param>
/// <returns>True if deserialization succeeds; otherwise, false.</returns>
/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or whitespace.</exception>
public static bool TryFromJson(string json, out SlowQueryLogParserTests? value)
{
ArgumentException.ThrowIfNullOrWhiteSpace(json, nameof(json));

value = null;

try
{
value = JsonSerializer.Deserialize<SlowQueryLogParserTests>(json, _jsonOptions);
return true;
}
catch (JsonException)
{
return false;
}
}
}
}