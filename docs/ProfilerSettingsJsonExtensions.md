# ProfilerSettingsJsonExtensions

## Purpose
Provides JSON serialization and deserialization extensions for the `ProfilerSettings` class, enabling conversion to and from JSON format with configurable options.

## Members

### ToJson(value, indented=false)
Serializes the `ProfilerSettings` instance to a JSON string using camelCase property naming.

- **Parameters**:
  - `value`: The settings to serialize. Must not be null.
  - `indented`: Optional. Whether to indent the JSON for readability. When true, the output is formatted with indentation. Defaults to false.
- **Returns**: A JSON string representation of the settings with camelCase property names.
- **Exceptions**:
  - `ArgumentNullException`: Thrown if `value` is null.

### FromJson(json)
Deserializes a JSON string into a `ProfilerSettings` instance.

- **Parameters**:
  - `json`: The JSON string to deserialize. Can be null or empty, in which case null is returned.
- **Returns**: The deserialized settings instance, or null if the JSON is null or empty.
- **Exceptions**:
  - `JsonException`: Thrown if the JSON is invalid or cannot be deserialized into a `ProfilerSettings` instance.

### TryFromJson(json, out value)
Attempts to deserialize a JSON string into a `ProfilerSettings` instance.

- **Parameters**:
  - `json`: The JSON string to deserialize. Can be null or empty, in which case the method returns true and `value` is set to null.
  - `value`: Receives the deserialized settings if successful; otherwise, null.
- **Returns**: true if deserialization succeeded or the JSON was null/empty; otherwise, false.
- **Exceptions**: None (catches and handles `JsonException` internally).

## Usage Example
```csharp
using SqlQueryAnalyzer.Configuration;

// Create profiler settings
var settings = new ProfilerSettings
{
    ConnectionString = "Server=localhost;Database=test;User Id=sa;Password=your_password;",
    Query = "SELECT * FROM Users",
    Format = OutputFormat.Json
};

// Serialize to JSON (compact)
string json = settings.ToJson();

// Serialize to JSON (indented)
string indentedJson = settings.ToJson(indented: true);

// Deserialize from JSON
ProfilerSettings? parsedSettings = ProfilerSettingsJsonExtensions.FromJson(json);
if (parsedSettings != null)
{
    Console.WriteLine($"Deserialized query: {parsedSettings.Query}");
}

// Try deserialization (safe)
if (ProfilerSettingsJsonExtensions.TryFromJson(json, out var tryParsedSettings))
{
    if (tryParsedSettings != null)
    {
        Console.WriteLine($"Try deserialized format: {tryParsedSettings.Format}");
    }
}
```