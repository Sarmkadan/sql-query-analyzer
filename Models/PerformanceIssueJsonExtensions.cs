#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using SqlQueryAnalyzer.Constants;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="PerformanceIssue"/>.
/// </summary>
public static class PerformanceIssueJsonExtensions
{
	/// <summary>
	/// Shared JSON serialization options with camelCase property naming.
	/// </summary>
	private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false,
		IgnoreNullValues = true
	};

	/// <summary>
	/// Converts a <see cref="PerformanceIssue"/> instance to its JSON representation.
	/// </summary>
	/// <param name="value">The performance issue to serialize.</param>
	/// <param name="indented">Whether to format the JSON with indentation for readability.</param>
	/// <returns>A JSON string representing the performance issue.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
	public static string ToJson(this PerformanceIssue value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		var options = new JsonSerializerOptions(_jsonOptions)
		{
			WriteIndented = indented
		};

		return JsonSerializer.Serialize(value, options);
	}

	/// <summary>
	/// Deserializes a JSON string into a <see cref="PerformanceIssue"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>The deserialized performance issue, or null if the JSON is invalid.</returns>
	/// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is null or empty.</exception>
	public static PerformanceIssue? FromJson(string json)
	{
		ArgumentException.ThrowIfNullOrEmpty(json);

		try
		{
			return JsonSerializer.Deserialize<PerformanceIssue>(json, _jsonOptions);
		}
		catch (JsonException)
		{
			return null;
		}
	}

	/// <summary>
	/// Attempts to deserialize a JSON string into a <see cref="PerformanceIssue"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">Receives the deserialized performance issue if successful.</param>
	/// <returns>True if deserialization succeeded; otherwise, false.</returns>
	/// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is null or empty.</exception>
	public static bool TryFromJson(string json, out PerformanceIssue? value)
	{
		ArgumentException.ThrowIfNullOrEmpty(json);

		try
		{
			value = JsonSerializer.Deserialize<PerformanceIssue>(json, _jsonOptions);
			return true;
		}
		catch (JsonException)
		{
			value = null;
			return false;
		}
	}
}