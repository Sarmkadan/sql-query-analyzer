# SqlQueryAnalyzerExceptionJsonExtensions

## Purpose
Provides JSON serialization and deserialization extension methods for the `SqlQueryAnalyzerException` class, enabling conversion between the exception and its JSON representation using web-friendly, camelCase-named properties.

## Members

### ToJson(value, indented = false)
Serializes a `SqlQueryAnalyzerException` to a JSON string.

- **Parameters**:
  - `value`: The `SqlQueryAnalyzerException` to serialize.
  - `indented`: Whether to format the JSON with indentation for readability. Defaults to `false`.
- **Returns**: A JSON string representation of the exception.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `value` is null.

### FromJson(json)
Deserializes a JSON string to a `SqlQueryAnalyzerException`.

- **Parameters**:
  - `json`: The JSON string to deserialize.
- **Returns**: The deserialized exception, or `null` if the JSON is null or empty.
- **Exceptions**:
  - `JsonException`: Thrown when the JSON is malformed and cannot be deserialized.

### TryFromJson(json, out value)
Attempts to deserialize a JSON string to a `SqlQueryAnalyzerException`.

- **Parameters**:
  - `json`: The JSON string to deserialize.
  - `value`: Receives the deserialized exception if successful.
- **Returns**: `true` if deserialization succeeded; otherwise, `false`.
- **Exceptions**:
  - `JsonException`: Thrown when the JSON is malformed and cannot be deserialized.

## Usage Example
```csharp
using System;
using SqlQueryAnalyzer.Exceptions;

public class ExceptionSerializer
{
    public void SerializeAndDeserialize()
    {
        var exception = new InvalidQueryException(
            "The query contains an unknown column reference.",
            "SELECT * FROM Orders WHERE CustomerId = @customerId",
            lineNumber: 1,
            columnNumber: 42);

        // Serialize to a compact JSON string
        string json = exception.ToJson();
        Console.WriteLine(json);

        // Serialize to an indented JSON string for readability
        string indentedJson = exception.ToJson(indented: true);
        Console.WriteLine(indentedJson);

        // Deserialize back to an exception
        SqlQueryAnalyzerException? deserialized = SqlQueryAnalyzerExceptionJsonExtensions.FromJson(json);
        Console.WriteLine($"Message: {deserialized?.Message}");

        // Try to deserialize, handling failure gracefully
        if (SqlQueryAnalyzerExceptionJsonExtensions.TryFromJson(json, out SqlQueryAnalyzerException? value))
        {
            Console.WriteLine($"Deserialized successfully: {value?.Message}");
        }
        else
        {
            Console.WriteLine("Deserialization failed.");
        }
    }
}
```