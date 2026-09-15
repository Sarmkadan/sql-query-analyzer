# AnalysisRequestDtoJsonExtensions

## Purpose
Provides JSON serialization and deserialization extension methods for the `AnalysisRequestDto` class, enabling conversion between the DTO and its JSON representation using web-friendly, camelCase-named properties.

## Members

### ToJson(value, indented = false)
Serializes an `AnalysisRequestDto` to a JSON string.

- **Parameters**:
  - `value`: The `AnalysisRequestDto` to serialize.
  - `indented`: Whether to format the JSON with indentation for readability. Defaults to `false`.
- **Returns**: A JSON string representation of the DTO.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `value` is null.

### FromJson(json)
Deserializes a JSON string to an `AnalysisRequestDto`.

- **Parameters**:
  - `json`: The JSON string to deserialize.
- **Returns**: The deserialized DTO, or `null` if the JSON is invalid.
- **Exceptions**:
  - `ArgumentException`: Thrown when `json` is null or empty.
  - `JsonException`: Thrown when the JSON is malformed and cannot be deserialized.

### TryFromJson(json, out value)
Attempts to deserialize a JSON string to an `AnalysisRequestDto`.

- **Parameters**:
  - `json`: The JSON string to deserialize.
  - `value`: Receives the deserialized DTO if successful.
- **Returns**: `true` if deserialization succeeded; otherwise, `false`.
- **Exceptions**:
  - `ArgumentException`: Thrown when `json` is null or empty.
  - `JsonException`: Thrown when the JSON is malformed and cannot be deserialized.

## Usage Example
```csharp
using System;
using SqlQueryAnalyzer.DTOs;

public class AnalysisRequestSerializer
{
    public void SerializeAndDeserialize()
    {
        var request = new AnalysisRequestDto
        {
            QueryText = "SELECT * FROM Orders WHERE CustomerId = @customerId",
            ApplicationName = "OrderService",
            ProcedureName = "GetOrdersByCustomer",
            IncludeIndexSuggestions = true,
            AnalyzeFragmentation = true,
            AnalyzePlan = false
        };

        // Serialize to a compact JSON string
        string json = request.ToJson();
        Console.WriteLine(json);

        // Serialize to an indented JSON string for readability
        string indentedJson = request.ToJson(indented: true);
        Console.WriteLine(indentedJson);

        // Deserialize back to a DTO
        AnalysisRequestDto? deserialized = AnalysisRequestDtoJsonExtensions.FromJson(json);
        Console.WriteLine($"QueryText: {deserialized?.QueryText}");

        // Try to deserialize, handling failure gracefully
        if (AnalysisRequestDtoJsonExtensions.TryFromJson(json, out AnalysisRequestDto? value))
        {
            Console.WriteLine($"Deserialized successfully: {value?.ProcedureName}");
        }
        else
        {
            Console.WriteLine("Deserialization failed.");
        }
    }
}
```