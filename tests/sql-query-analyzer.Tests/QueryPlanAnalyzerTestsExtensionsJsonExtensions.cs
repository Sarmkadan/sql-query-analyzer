using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SqlQueryAnalyzer.Tests
{
    /// <summary>
    /// Provides JSON serialization extensions for <see cref="QueryPlanAnalyzerTests"/>.
    /// </summary>
    public static class QueryPlanAnalyzerTestsExtensionsJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        /// <summary>
        /// Serializes a <see cref="QueryPlanAnalyzerTests"/> instance to a JSON string.
        /// </summary>
        /// <param name="value">The test fixture instance to serialize.</param>
        /// <param name="indented">Whether to indent the JSON for readability.</param>
        /// <returns>A JSON string representation of the test fixture.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static string ToJson(this QueryPlanAnalyzerTests value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            var options = indented
                ? new JsonSerializerOptions(_jsonSerializerOptions)
                {
                    WriteIndented = true,
                }
                : _jsonSerializerOptions;

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string to a <see cref="QueryPlanAnalyzerTests"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized test fixture instance, or null if the JSON is null or empty.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is null.</exception>
        public static QueryPlanAnalyzerTests? FromJson(string json)
        {
            ArgumentNullException.ThrowIfNull(json);

            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<QueryPlanAnalyzerTests>(json, _jsonSerializerOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to a <see cref="QueryPlanAnalyzerTests"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">Receives the deserialized test fixture instance if successful.</param>
        /// <returns>True if deserialization succeeded; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is null.</exception>
        public static bool TryFromJson(string json, out QueryPlanAnalyzerTests? value)
        {
            ArgumentNullException.ThrowIfNull(json);

            value = null;

            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            try
            {
                value = JsonSerializer.Deserialize<QueryPlanAnalyzerTests>(json, _jsonSerializerOptions);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}