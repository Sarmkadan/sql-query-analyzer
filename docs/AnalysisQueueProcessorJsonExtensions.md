# AnalysisQueueProcessorJsonExtensions

Provides System.Text.Json serialization and deserialization extensions for `AnalysisQueueProcessor`.

## Purpose

Enables JSON serialization of the processor state including queued tasks, active tasks, and configuration settings.

## Members

### ToJson

```csharp
public static string ToJson(this AnalysisQueueProcessor value, bool indented = false)
```

Serializes the `AnalysisQueueProcessor` instance to a JSON string.

#### Parameters
- `value`: The processor instance to serialize.
- `indented`: Whether to format the JSON with indentation for readability. Default is `false`.

#### Returns
A JSON string representation of the processor.

#### Exceptions
- `ArgumentNullException`: Thrown when `value` is `null`.

### FromJson

```csharp
public static AnalysisQueueProcessor? FromJson(string json)
```

Deserializes a JSON string to an `AnalysisQueueProcessor` instance.

#### Parameters
- `json`: The JSON string to deserialize.

#### Returns
An `AnalysisQueueProcessor` instance, or `null` if deserialization fails.

#### Exceptions
- `ArgumentException`: Thrown when `json` is `null`, empty, or whitespace.
- `JsonException`: Thrown when the JSON is invalid.

### TryFromJson

```csharp
public static bool TryFromJson(string json, out AnalysisQueueProcessor? value)
```

Attempts to deserialize a JSON string to an `AnalysisQueueProcessor` instance.

#### Parameters
- `json`: The JSON string to deserialize.
- `value`: Receives the deserialized instance if successful, otherwise `null`.

#### Returns
`true` if deserialization succeeded; otherwise, `false`.

#### Exceptions
- `ArgumentException`: Thrown when `json` is `null`, empty, or whitespace.

## Example

```csharp
using SqlQueryAnalyzer.BackgroundWorkers;
using System.Text.Json;

// Create a processor instance (assuming AnalysisQueueProcessor is defined elsewhere)
var processor = new AnalysisQueueProcessor();

// Serialize to JSON (compact)
string json = processor.ToJson();

// Serialize to JSON (indented for readability)
string indentedJson = processor.ToJson(indented: true);

// Deserialize from JSON
AnalysisQueueProcessor? deserialized = AnalysisQueueProcessorJsonExtensions.FromJson(json);

// Try deserialization (safe)
if (AnalysisQueueProcessorJsonExtensions.TryFromJson(json, out var processorFromTry))
{
    // Use processorFromTry
}
```