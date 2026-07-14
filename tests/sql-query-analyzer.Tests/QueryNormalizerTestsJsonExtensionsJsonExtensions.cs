using System;
using System.Text.Json;

namespace SqlQueryAnalyzer.Tests
{
    public static class QueryNormalizerTestsJsonExtensionsJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
        };

        /// <summary>
        /// Serializes a <see cref="QueryNormalizerTests"/> instance to a JSON string.
        /// </summary>
        /// <param name="value">The instance to serialize.</param>
        /// <param name="indented">Whether to indent the JSON for readability.</param>
        /// <returns>A JSON string representation of the instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static string ToJson(this QueryNormalizerTests value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            return JsonSerializer.Serialize(value, indented ? GetIndentedOptions() : _jsonOptions);
        }

        /// <summary>
        /// Deserializes a <see cref="QueryNormalizerTests"/> instance from a JSON string.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized instance, or null if the JSON is null or empty.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is null.</exception>
        /// <exception cref="JsonException">Thrown if the JSON is invalid or cannot be deserialized.</exception>
        public static QueryNormalizerTests? FromJson(string json)
        {
            ArgumentNullException.ThrowIfNull(json);

            return JsonSerializer.Deserialize<QueryNormalizerTests>(json, _jsonOptions);
        }

        /// <summary>
        /// Attempts to deserialize a <see cref="QueryNormalizerTests"/> instance from a JSON string.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">Receives the deserialized instance if successful.</param>
        /// <returns>True if deserialization succeeded; otherwise, false.</returns>
        public static bool TryFromJson(string json, out QueryNormalizerTests? value)
        {
            ArgumentNullException.ThrowIfNull(json);

            try
            {
                value = JsonSerializer.Deserialize<QueryNormalizerTests>(json, _jsonOptions);
                return true;
            }
            catch (JsonException)
            {
                value = null;
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
}