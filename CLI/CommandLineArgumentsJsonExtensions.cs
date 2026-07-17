#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace SqlQueryAnalyzer.CLI;

/// <summary>
/// Provides JSON serialization and deserialization extensions for CommandLineArguments.
/// </summary>
public static class CommandLineArgumentsJsonExtensions
{
	private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.General)
	{
		WriteIndented = false,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
		Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
	};

	/// <summary>
	/// Converts CommandLineArguments to a JSON string.
	/// </summary>
	/// <param name="value">The command line arguments to serialize.</param>
	/// <param name="indented">Whether to format the JSON with indentation for readability.</param>
	/// <returns>A JSON string representation of the command line arguments.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
	public static string ToJson(this CommandLineArguments value, bool indented = false) =>
		JsonSerializer.Serialize(value, indented
			? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
			: _jsonOptions);

	/// <summary>
	/// Parses CommandLineArguments from a JSON string.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>The deserialized CommandLineArguments instance if successful; otherwise, null.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
	/// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
	public static CommandLineArguments? FromJson(string json)
	{
		ArgumentNullException.ThrowIfNull(json);

		if (string.IsNullOrWhiteSpace(json))
		{
			return null;
		}

		return JsonSerializer.Deserialize<CommandLineArguments>(json, _jsonOptions);
	}

	/// <summary>
	/// Attempts to parse CommandLineArguments from a JSON string.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">Receives the deserialized CommandLineArguments if successful.</param>
	/// <returns>True if deserialization succeeded; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
	public static bool TryFromJson(string json, out CommandLineArguments? value)
	{
		value = null;

		ArgumentNullException.ThrowIfNull(json);

		if (string.IsNullOrWhiteSpace(json))
		{
			return false;
		}

		try
		{
			value = JsonSerializer.Deserialize<CommandLineArguments>(json, _jsonOptions);
			return true;
		}
		catch (JsonException)
		{
			return false;
		}
	}
}
