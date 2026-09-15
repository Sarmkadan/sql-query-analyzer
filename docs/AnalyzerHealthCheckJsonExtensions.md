# AnalyzerHealthCheckJsonExtensions

## Purpose
Provides JSON serialization and deserialization extensions for the `AnalyzerHealthCheck` class, enabling conversion to and from JSON format with configurable options.

## Members

### ToJson(value, indented=false)
Serializes an `AnalyzerHealthCheck` instance to a JSON string.

- **Parameters**:
  - `value`: The health check to serialize.
  - `indented`: Optional. Whether to format the JSON with indentation for readability. Defaults to `false`.
- **Returns**: A JSON string representation of the health check.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `value` is null.

### FromJson(json)
Deserializes an `AnalyzerHealthCheck` instance from a JSON string.

- **Parameters**:
  - `json`: The JSON string to deserialize.
- **Returns**: The deserialized `AnalyzerHealthCheck` instance if successful; otherwise, `null`.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `json` is null.
  - `ArgumentException`: Thrown when `json` is empty or consists only of whitespace.
  - `JsonException`: Thrown when the JSON is invalid or cannot be deserialized.

### TryFromJson(json, out value)
Attempts to deserialize an `AnalyzerHealthCheck` instance from a JSON string without throwing exceptions.

- **Parameters**:
  - `json`: The JSON string to deserialize.
  - `value`: Receives the deserialized `AnalyzerHealthCheck` if successful; otherwise, `null`.
- **Returns**: `true` if deserialization succeeded; otherwise, `false`.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `json` is null.

## Usage Example
```csharp
using SqlQueryAnalyzer.Diagnostics;

// Create a health check
var healthCheck = new AnalyzerHealthCheck(...);

// Serialize to JSON (compact)
string json = healthCheck.ToJson();

// Serialize to JSON (indented)
string indentedJson = healthCheck.ToJson(indented: true);

// Deserialize from JSON
AnalyzerHealthCheck? parsed = AnalyzerHealthCheckJsonExtensions.FromJson(json);
if (parsed != null)
{
    Console.WriteLine($"Deserialized health check at: {parsed.CheckTime}");
}

// Try deserialization (safe)
if (AnalyzerHealthCheckJsonExtensions.TryFromJson(json, out var tryParsed))
{
    if (tryParsed != null)
    {
        Console.WriteLine($"Try deserialized health check at: {tryParsed.CheckTime}");
    }
}
```