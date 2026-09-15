# HttpQueryAnalysisClientJsonExtensionsJsonExtensions

## Purpose
Provides JSON serialization utilities for type markers used with HttpQueryAnalysisClient, enabling serialization of a type marker to JSON and deserialization back to an object.

## Members

### ToJson(value, indented = false)
Serializes a type marker representing HttpQueryAnalysisClient to a JSON string.

- **Parameters**:
  - `value`: The object used only for type context; its value is ignored.
  - `indented`: Whether to format the JSON with indentation for readability. Defaults to `false`.
- **Returns**: A JSON string representation of the HttpQueryAnalysisClient type marker.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `value` is null.

### FromJson(json)
Deserializes a JSON string into a type marker object.

- **Parameters**:
  - `json`: The JSON string to deserialize.
- **Returns**: A type marker object, or null if the JSON is empty or invalid.
- **Exceptions**:
  - `ArgumentException`: Thrown when `json` is null or empty.

### TryFromJson(json, out value)
Attempts to deserialize a JSON string into a type marker object.

- **Parameters**:
  - `json`: The JSON string to deserialize.
  - `value`: Receives the deserialized type marker if successful.
- **Returns**: true if deserialization succeeds; otherwise, false.
- **Exceptions**:
  - `ArgumentException`: Thrown when `json` is null or empty.

## Usage Example
```csharp
using System;
using System.Text.Json;
using SqlQueryAnalyzer.Integration;

public class HttpQueryAnalysisClientTypeMarkerExample
{
    public void SerializeAndDeserializeTypeMarker()
    {
        // Create an instance of HttpQueryAnalysisClient (or any object) to use as type context
        var client = new HttpQueryAnalysisClient
        {
            // Assuming HttpQueryAnalysisClient has properties like Endpoint, ApiKey, etc.
            // Adjust based on actual class definition
            Endpoint = "https://api.example.com/query",
            ApiKey = "your-api-key",
            TimeoutSeconds = 30
        };

        // Serialize the type marker to a compact JSON string
        string json = client.ToJson();
        Console.WriteLine(json); // Example output: {"Type":"HttpQueryAnalysisClient"}

        // Serialize the type marker to an indented JSON string for readability
        string indentedJson = client.ToJson(indented: true);
        Console.WriteLine(indentedJson);

        // Deserialize back to an object
        object? deserialized = HttpQueryAnalysisClientJsonExtensionsJsonExtensions.FromJson(json);
        Console.WriteLine($"Deserialized type: {deserialized}");

        // Try to deserialize, handling failure gracefully
        if (HttpQueryAnalysisClientJsonExtensionsJsonExtensions.TryFromJson(json, out object? value))
        {
            Console.WriteLine($"Deserialization successful: {value}");
        }
        else
        {
            Console.WriteLine("Deserialization failed.");
        }
    }
}
```