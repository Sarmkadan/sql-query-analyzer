using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace SqlQueryAnalyzer.Testing;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="SampleQueryProvider"/> data.
/// </summary>
public static class SampleQueryProviderJsonExtensions
{
	private sealed class SampleQueryData
	{
		public required Dictionary<string, string> AllSamples { get; init; }
		public required string RandomSample { get; init; }
		public required Dictionary<string, List<string>> SamplesByIssueType { get; init; }
	}

	private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
		WriteIndented = false
	};

	/// <summary>
	/// Serializes the sample queries to a JSON string.
	/// </summary>
	/// <param name="indented">Whether to format the JSON with indentation for readability.</param>
	/// <returns>A JSON string representation of the sample queries.</returns>
	public static string ToJson(bool indented = false)
	{
		var data = new SampleQueryData
		{
			AllSamples = SampleQueryProvider.GetAllSamples(),
			RandomSample = SampleQueryProvider.GetRandomSample(),
			SamplesByIssueType = SampleQueryProvider.GetSamplesByIssueType()
		};

		var options = indented
			? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
			: _jsonOptions;

		return JsonSerializer.Serialize(data, options);
	}

	/// <summary>
	/// Deserializes a JSON string to sample query data.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>The deserialized sample query data, or <see langword="null"/> if the JSON is empty.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
	/// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
	public static object? FromJson(string json)
	{
		ArgumentNullException.ThrowIfNull(json);

		return string.IsNullOrWhiteSpace(json)
			? null
			: JsonSerializer.Deserialize<object>(json, _jsonOptions);
	}

	/// <summary>
	/// Attempts to deserialize a JSON string to sample query data.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">Receives the deserialized sample query data if successful, otherwise <see langword="null"/>.</param>
	/// <returns><see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/>.</returns>
	/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <see langword="null"/> or empty.</exception>
	public static bool TryFromJson(string json, out object? value)
	{
		ArgumentException.ThrowIfNullOrEmpty(json);

		try
		{
			value = JsonSerializer.Deserialize<object>(json, _jsonOptions);
			return true;
		}
		catch (JsonException)
		{
			value = null;
			return false;
		}
	}
}