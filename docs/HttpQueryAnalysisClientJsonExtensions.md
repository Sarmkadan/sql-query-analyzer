# HttpQueryAnalysisClientJsonExtensions

## Purpose
Provides System.Text.Json serialization extensions for the `HttpQueryAnalysisClient` class, enabling conversion between the client instance and its JSON representation using web-friendly, camelCase-named properties.

## Members

### ToJson(value, indented = false)
Serializes an `HttpQueryAnalysisClient` to a JSON string.

- **Parameters**:
  - `value`: The `HttpQueryAnalysisClient` to serialize.
  - `indented`: Whether to format the JSON with indentation for readability. Defaults to `false`.
- **Returns**: A JSON string representation of the client.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `value` is null.

### FromJson(json)
Deserializes a JSON string to an `HttpQueryAnalysisClient` instance.

- **Parameters**:
  - `json`: The JSON string to deserialize.
- **Returns**: The deserialized client, or `null` if the JSON is invalid.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `json` is null, empty, or whitespace.
  - `JsonException`: Thrown when the JSON is malformed and cannot be deserialized.

### TryFromJson(json, out value)
Attempts to deserialize a JSON string to an `HttpQueryAnalysisClient` instance.

- **Parameters**:
  - `json`: The JSON string to deserialize.
  - `value`: Receives the deserialized client if successful.
- **Returns**: `true` if deserialization succeeded; otherwise, `false`.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `json` is null, empty, or whitespace.
  - `JsonException`: Thrown when the JSON is malformed and cannot be deserialized.

## Usage Example
```csharp
using System;
using SqlQueryAnalyzer.Integration;

public class HttpQueryAnalysisClientSerializer
{
    public void SerializeAndDeserialize()
    {
        var client = new HttpQueryAnalysisClient
        {
            // Assuming HttpQueryAnalysisClient has properties like Endpoint, ApiKey, etc.
            // Adjust based on actual class definition
            Endpoint = "https://api.example.com/query",
            ApiKey = "your-api-key",
            TimeoutSeconds = 30
        };

        // Serialize to a compact JSON string
        string json = client.ToJson();
        Console.WriteLine(json);

        // Serialize to an indented JSON string for readability
        string indentedJson = client.ToJson(indented: true);
        Console.WriteLine(indentedJson);

        // Deserialize back to a client instance
        HttpQueryAnalysisClient? deserialized = HttpQueryAnalysisClientJsonExtensions.FromJson(json);
        Console.WriteLine($"Endpoint: {deserialized?.Endpoint}");

        // Try to deserialize, handling failure gracefully
        if (HttpQueryAnalysisClientJsonExtensions.TryFromJson(json, out HttpQueryAnalysisClient? value))
        {
            Console.WriteLine($"Deserialized successfully: {value?.Endpoint}");
        }
        else
        {
            Console.WriteLine("Deserialization failed.");
        }
    }
}
```