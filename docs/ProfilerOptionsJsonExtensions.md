# ProfilerOptionsJsonExtensions

Provides JSON serialization and deserialization extensions for `ProfilerOptions`.

## Purpose

This static class extends `ProfilerOptions` with methods to serialize instances to JSON strings and deserialize JSON strings back into `ProfilerOptions` objects. It uses `System.Text.Json` with camelCase property naming and null-ignoring settings.

## Members

### `ToJson(this ProfilerOptions value, bool indented = false)`

Serializes a `ProfilerOptions` instance to a JSON string.

- **Parameters**
  - `value`: The profiler options to serialize.
  - `indented`: (Optional) Whether to format the JSON with indentation for readability. Default is `false`.
- **Returns**: A JSON string representation of the profiler options.
- **Exceptions**: `ArgumentNullException` if `value` is `null`.

### `FromJson(string json)`

Deserializes a JSON string into a `ProfilerOptions` instance.

- **Parameters**
  - `json`: The JSON string to deserialize.
- **Returns**: The deserialized profiler options, or `null` if the JSON represents a null value.
- **Exceptions**: 
  - `ArgumentNullException` if `json` is `null`.
  - `JsonException` if the JSON is invalid or cannot be deserialized.

### `TryFromJson(string json, out ProfilerOptions? value)`

Attempts to deserialize a JSON string into a `ProfilerOptions` instance without throwing exceptions on failure.

- **Parameters**
  - `json`: The JSON string to deserialize.
  - `value`: Receives the deserialized profiler options, or `null` if deserialization fails or JSON represents a null value.
- **Returns**: `true` if deserialization succeeded; otherwise, `false`.
- **Exceptions**: `ArgumentNullException` if `json` is `null`.

## Example

```csharp
using SqlQueryAnalyzer.Models;

// Create an instance of ProfilerOptions
var options = new ProfilerOptions
{
    // Set properties as needed
    // For example:
    // TimeoutSeconds = 30,
    // MaxRows = 1000
};

// Serialize to JSON (compact)
string json = options.ToJson();
// Serialize to JSON (indented for readability)
string indentedJson = options.ToJson(indented: true);

// Deserialize from JSON
ProfilerOptions? deserialized = ProfilerOptionsJsonExtensions.FromJson(json);

// Try deserialization (safe)
if (ProfilerOptionsJsonExtensions.TryFromJson(json, out var tried))
{
    // Use tried
}
```