# AnalysisPipelineJsonExtensions

## Purpose

`AnalysisPipelineJsonExtensions` provides `System.Text.Json` helpers for converting an `AnalysisPipeline` to and from JSON. Serialization uses web defaults, camel-case property names, and omits null-valued properties. Output is compact by default and can optionally be indented.

The deserialization helpers catch malformed JSON (`JsonException`). They do not suppress other errors, such as a pipeline constructor or dependency that cannot be populated from the JSON representation.

## Members

### `ToJson(value, indented = false)`

```csharp
public static string ToJson(this AnalysisPipeline value, bool indented = false)
```

Serializes an `AnalysisPipeline` instance to JSON.

- `value`: The pipeline to serialize. Passing `null` throws `ArgumentNullException`.
- `indented`: When `true`, produces readable indented JSON; otherwise, produces compact JSON. The default is `false`.
- Returns: The serialized JSON string.

### `FromJson(json)`

```csharp
public static AnalysisPipeline? FromJson(string json)
```

Deserializes JSON into an `AnalysisPipeline`.

- `json`: The JSON string to deserialize. Passing `null` or an empty string throws `ArgumentException`.
- Returns: The deserialized pipeline, or `null` when `System.Text.Json` reports malformed or incompatible JSON with a `JsonException`. The JSON literal `null` also produces `null`.

### `TryFromJson(json, out value)`

```csharp
public static bool TryFromJson(string json, out AnalysisPipeline? value)
```

Attempts to deserialize JSON without propagating `JsonException`.

- `json`: The JSON string to deserialize. Passing `null` or an empty string throws `ArgumentException`.
- `value`: Receives the deserialized pipeline, or `null` when a `JsonException` occurs.
- Returns: `true` when `JsonSerializer.Deserialize` completes without a `JsonException`; otherwise, `false`. Because valid JSON can represent `null`, a `true` result does not guarantee that `value` is non-null.

## Example

```csharp
using SqlQueryAnalyzer.Middleware;

public static class PipelineJsonExample
{
    public static void ShowSerialization(AnalysisPipeline pipeline)
    {
        // Compact JSON is suitable for storage or transport.
        string compactJson = pipeline.ToJson();

        // Indented JSON is convenient for logs or diagnostics.
        string readableJson = pipeline.ToJson(indented: true);

        Console.WriteLine(compactJson);
        Console.WriteLine(readableJson);

        // Malformed JSON is reported without throwing JsonException.
        if (!AnalysisPipelineJsonExtensions.TryFromJson("{not valid json}", out var value))
        {
            Console.WriteLine("The pipeline JSON is invalid.");
        }

        // FromJson provides the equivalent nullable result for malformed JSON.
        AnalysisPipeline? parsed =
            AnalysisPipelineJsonExtensions.FromJson("{not valid json}");
        Console.WriteLine(parsed is null);
    }
}
```
