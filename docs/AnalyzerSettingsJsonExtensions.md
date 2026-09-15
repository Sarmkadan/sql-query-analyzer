# AnalyzerSettingsJsonExtensions

## Purpose
Provides JSON serialization and deserialization extensions for the `AnalyzerSettings` class, enabling conversion to and from JSON format with configurable options.

## Members

### ToJson(value, indented=false)
Serializes an `AnalyzerSettings` instance to a JSON string.

- **Parameters**:
  - `value`: The analyzer settings to serialize.
  - `indented`: Optional. Whether to format the JSON with indentation for readability. Defaults to `false`.
- **Returns**: A JSON string representation of the analyzer settings.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `value` is null.

### FromJson(json)
Deserializes an `AnalyzerSettings` instance from a JSON string.

- **Parameters**:
  - `json`: The JSON string to deserialize.
- **Returns**: The deserialized `AnalyzerSettings` instance if successful; otherwise, `null`.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `json` is null.
  - `JsonException`: Thrown when the JSON is invalid or cannot be deserialized.

### TryFromJson(json, out value)
Attempts to deserialize an `AnalyzerSettings` instance from a JSON string without throwing exceptions.

- **Parameters**:
  - `json`: The JSON string to deserialize.
  - `value`: Receives the deserialized `AnalyzerSettings` if successful; otherwise, `null`.
- **Returns**: `true` if deserialization succeeded; otherwise, `false`.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `json` is null.

## Usage Example
```csharp
using SqlQueryAnalyzer.Configuration;

// Create analyzer settings
var settings = new AnalyzerSettings
{
    MaxDegreeOfParallelism = 4,
    EnableQueryOptimization = true,
    ConnectionTimeoutSeconds = 30
};

// Serialize to JSON (compact)
string json = settings.ToJson();

// Serialize to JSON (indented)
string indentedJson = settings.ToJson(indented: true);

// Deserialize from JSON
AnalyzerSettings? parsedSettings = AnalyzerSettingsJsonExtensions.FromJson(json);
if (parsedSettings != null)
{
    Console.WriteLine($"Deserialized max degree of parallelism: {parsedSettings.MaxDegreeOfParallelism}");
}

// Try deserialization (safe)
if (AnalyzerSettingsJsonExtensions.TryFromJson(json, out var tryParsedSettings))
{
    if (tryParsedSettings != null)
    {
        Console.WriteLine($"Try deserialized connection timeout: {tryParsedSettings.ConnectionTimeoutSeconds}");
    }
}
```