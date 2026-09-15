# ExportServiceJsonExtensions

## Purpose
Provides JSON serialization and deserialization extension methods for the `ExportService` class, enabling conversion between the service instance and its JSON representation using web-friendly, camelCase-named properties.

## Members

### ToJson(value, indented = false)
Serializes an `ExportService` to a JSON string.

- **Parameters**:
  - `value`: The `ExportService` to serialize.
  - `indented`: Whether to format the JSON with indentation for readability. Defaults to `false`.
- **Returns**: A JSON string representation of the service.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `value` is null.

### FromJson(json)
Deserializes a JSON string to an `ExportService`.

- **Parameters**:
  - `json`: The JSON string to deserialize.
- **Returns**: The deserialized service, or `null` if the JSON is empty or whitespace.
- **Exceptions**:
  - `ArgumentException`: Thrown when `json` is null or empty.
  - `JsonException`: Thrown when the JSON is invalid or cannot be deserialized.

### TryFromJson(json, out value)
Attempts to deserialize a JSON string to an `ExportService`.

- **Parameters**:
  - `json`: The JSON string to deserialize.
  - `value`: Receives the deserialized service if successful.
- **Returns**: `true` if deserialization succeeded; otherwise, `false`.
- **Exceptions**:
  - `ArgumentException`: Thrown when `json` is null or empty.

## Usage Example
```csharp
using System;
using Microsoft.Extensions.Logging.Abstractions;
using SqlQueryAnalyzer.Export;

public class ExportServiceSerializer
{
    public void SerializeAndDeserialize()
    {
        var service = new ExportService(NullLogger<ExportService>.Instance);

        // Serialize to a compact JSON string
        string json = service.ToJson();
        Console.WriteLine(json);

        // Serialize to an indented JSON string for readability
        string indentedJson = service.ToJson(indented: true);
        Console.WriteLine(indentedJson);

        // Deserialize back to a service instance
        ExportService? deserialized = ExportServiceJsonExtensions.FromJson(json);
        Console.WriteLine($"Deserialized: {deserialized != null}");

        // Try to deserialize, handling failure gracefully
        if (ExportServiceJsonExtensions.TryFromJson(json, out ExportService? value))
        {
            Console.WriteLine("Deserialization succeeded.");
        }
        else
        {
            Console.WriteLine("Deserialization failed.");
        }
    }
}
```