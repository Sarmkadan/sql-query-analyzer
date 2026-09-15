# PlanVisualizationJsonExtensions

Provides JSON serialization helpers for `PlanVisualization`.

## Purpose

This static class contains extension methods for serializing and deserializing `PlanVisualization` objects to and from JSON. It uses camelCase property naming and ignores null values during serialization.

## Members

### JsonOptions (private static readonly)

Shared `JsonSerializerOptions` instance configured with:
- `PropertyNamingPolicy = JsonNamingPolicy.CamelCase`
- `WriteIndented = false`
- `DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull`
- `ReferenceHandler = ReferenceHandler.IgnoreCycles`

### ToJson(this PlanVisualization value, bool indented = false)

Serializes a `PlanVisualization` instance to JSON.

**Parameters**
- `value`: The plan visualization to serialize.
- `indented`: Whether to format the JSON with indentation (default: false).

**Returns**
JSON string representation of the plan visualization.

**Exceptions**
- `ArgumentNullException`: Thrown when `value` is null.

### FromJson(string json)

Deserializes a JSON string to a `PlanVisualization` instance.

**Parameters**
- `json`: The JSON string to deserialize.

**Returns**
The deserialized plan visualization, or null if input is empty.

**Exceptions**
- `ArgumentNullException`: Thrown when `json` is null.
- `ArgumentException`: Thrown when `json` is empty.
- `JsonException`: Thrown when JSON is invalid.

### TryFromJson(string json, out PlanVisualization? value)

Tries to deserialize a JSON string to a `PlanVisualization` instance.

**Parameters**
- `json`: The JSON string to deserialize.
- `value`: The deserialized plan visualization, or null on failure.

**Returns**
True if deserialization succeeded, false otherwise.

**Exceptions**
- `ArgumentNullException`: Thrown when `json` is null.
- `ArgumentException`: Thrown when `json` is empty.

## Example

```csharp
using SqlQueryAnalyzer.Models;
using System.Text.Json;

// Create a plan visualization (example)
var plan = new PlanVisualization
{
    // Initialize properties as needed
};

// Serialize to JSON (compact)
string json = plan.ToJson();

// Serialize to JSON (indented)
string indentedJson = plan.ToJson(indented: true);

// Deserialize from JSON
PlanVisualization? deserialized = PlanVisualizationJsonExtensions.FromJson(json);

// Try deserialization (safe)
if (PlanVisualizationJsonExtensions.TryFromJson(json, out var planFromJson))
{
    // Use planFromJson
}
```