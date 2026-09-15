# CliApplicationHostJsonExtensions

Provides JSON serialization helpers for `CliApplicationHost`.

## Purpose

This static class contains extension methods for serializing and deserializing `CliApplicationHost` instances to and from JSON. It uses camelCase naming policy and provides options for indented output.

## Members

### JsonOptions (private)

Configured JSON serializer options with camelCase naming policy and no indentation by default.

### ToJson

Serializes a `CliApplicationHost` instance to JSON.

```csharp
public static string ToJson(this CliApplicationHost value, bool indented = false)
```

- **value**: The host to serialize.
- **indented**: Whether to format the JSON with indentation (default: false).
- **Returns**: JSON string representation of the host.
- **Exceptions**: 
  - `ArgumentNullException` if `value` is null.

### FromJson

Deserializes a JSON string to a `CliApplicationHost` instance.

```csharp
public static CliApplicationHost? FromJson(string json)
```

- **json**: The JSON string to deserialize.
- **Returns**: The deserialized host, or null if deserialization fails.
- **Exceptions**:
  - `ArgumentNullException` if `json` is null.
  - `ArgumentException` if `json` is empty.
  - `JsonException` if JSON is invalid or cannot be deserialized to `CliApplicationHost`.

### TryFromJson

Tries to deserialize a JSON string to a `CliApplicationHost` instance.

```csharp
public static bool TryFromJson(string json, out CliApplicationHost? value)
```

- **json**: The JSON string to deserialize.
- **value**: The deserialized host, or null on failure.
- **Returns**: True if deserialization succeeded, false otherwise.
- **Exceptions**:
  - `ArgumentNullException` if `json` is null.
  - `ArgumentException` if `json` is empty.

## Example

```csharp
using SqlQueryAnalyzer.CLI;

// Create a host instance
var host = new CliApplicationHost
{
    // Initialize properties as needed
    // Example: host.SomeProperty = "value";
};

// Serialize to JSON (compact)
string json = host.ToJson();

// Serialize to JSON (indented)
string indentedJson = host.ToJson(indented: true);

// Deserialize from JSON
CliApplicationHost? deserializedHost = CliApplicationHostJsonExtensions.FromJson(json);

// Try deserialization (safe)
if (CliApplicationHostJsonExtensions.TryFromJson(json, out var hostFromTry))
{
    // Use hostFromTry
}
```