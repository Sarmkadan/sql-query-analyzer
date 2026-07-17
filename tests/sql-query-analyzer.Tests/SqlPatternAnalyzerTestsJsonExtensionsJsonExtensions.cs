#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text.Json;

namespace SqlQueryAnalyzer.Tests;

/// <summary>
/// Provides JSON serialization and deserialization extensions for type markers used in SQL pattern analyzer tests.
/// </summary>
public static class SqlPatternAnalyzerTestsTypeMarkerJsonExtensions
{
	private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false
	};

	/// <summary>
	/// Serializes a type marker to a JSON string.
	/// </summary>
	/// <param name="value">The type marker instance to serialize. This parameter provides type context only.</param>
	/// <param name="indented">Whether to format the JSON with indentation for readability.</param>
	/// <returns>A JSON string representation of the type marker.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
	public static string ToJson(this object value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		var options = indented
			? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
			: _jsonOptions;

		return JsonSerializer.Serialize(new { Type = "SqlPatternAnalyzerTestsTypeMarker" }, options);
	}

	/// <summary>
	/// Deserializes a JSON string into a type marker object.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>A type marker object, or <see langword="null"/> if the JSON is empty or whitespace.</returns>
	/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <see langword="null"/>, empty, or whitespace.</exception>
	/// <exception cref="JsonException">Thrown when the JSON is invalid and cannot be deserialized.</exception>
	public static object? FromJson(string json)
	{
		ArgumentException.ThrowIfNullOrEmpty(json);

		if (string.IsNullOrWhiteSpace(json))
		{
			return null;
		}

		return JsonSerializer.Deserialize<object>(json, _jsonOptions);
	}

	/// <summary>
	/// Attempts to deserialize a JSON string into a type marker object.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">Receives the deserialized type marker object if successful.</param>
	/// <returns><see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/>.</returns>
	/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <see langword="null"/> or empty.</exception>
	public static bool TryFromJson(string json, out object? value)
	{
		ArgumentException.ThrowIfNullOrEmpty(json);

		value = null;

		if (string.IsNullOrWhiteSpace(json))
		{
			return false;
		}

		try
		{
			value = JsonSerializer.Deserialize<object>(json, _jsonOptions);
			return true;
		}
		catch (JsonException)
		{
			return false;
		}
	}
}