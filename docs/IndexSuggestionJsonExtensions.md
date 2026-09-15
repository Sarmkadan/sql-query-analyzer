# IndexSuggestionJsonExtensions

## Purpose

`IndexSuggestionJsonExtensions` provides a consistent way to serialize and deserialize `IndexSuggestion` objects with `System.Text.Json`. It uses web-oriented serializer defaults, camel-case JSON property names, and compact output unless indentation is requested.

The type is declared in the `SqlQueryAnalyzer.Models` namespace.

## Members

### `ToJson(this IndexSuggestion value, bool indented = false)`

Serializes an `IndexSuggestion` to JSON.

- `value` is the suggestion to serialize. Passing `null` throws `ArgumentNullException`.
- `indented` controls whether the output is formatted across multiple lines. It defaults to `false`.
- The return value is a JSON string whose property names use camel case.

### `FromJson(string json)`

Deserializes JSON into an `IndexSuggestion` and returns `IndexSuggestion?`.

- Passing `null` or an empty string throws `ArgumentException`.
- Leading and trailing whitespace is removed before deserialization.
- Invalid JSON, whitespace-only input, or JSON that cannot be converted to `IndexSuggestion` throws `JsonException`.
- The JSON literal `null` produces a `null` result.

### `TryFromJson(string json, out IndexSuggestion? value)`

Attempts to deserialize JSON without propagating `JsonException`.

- Passing `null` or an empty string still throws `ArgumentException`.
- On valid JSON, it returns `true` and assigns the deserialized result to `value`. For the JSON literal `null`, this means `true` is returned with `value` set to `null`.
- When deserialization throws `JsonException`, it returns `false` and sets `value` to `null`. This includes whitespace-only input.

## Example

```csharp
using SqlQueryAnalyzer.Models;

var suggestion = new IndexSuggestion
{
    SuggestionId = "suggestion-42",
    TableName = "Orders",
    IndexName = "IX_Orders_CustomerId",
    IndexColumns = ["CustomerId"],
    IncludeColumns = ["OrderDate", "Total"],
    EstimatedPerformanceGain = 35.5,
    EstimatedExecutionTimeReduction = 28.0
};

// Compact JSON with camel-case property names.
string json = suggestion.ToJson();

// Indented output is useful for logs or diagnostics.
string readableJson = suggestion.ToJson(indented: true);

IndexSuggestion? restored = IndexSuggestionJsonExtensions.FromJson(json);

if (IndexSuggestionJsonExtensions.TryFromJson(json, out var parsed))
{
    Console.WriteLine(parsed?.IndexName);
}

bool valid = IndexSuggestionJsonExtensions.TryFromJson("not json", out var invalid);
// valid is false; invalid is null.
```

`FromJson` and `TryFromJson` are ordinary static methods; only `ToJson` is declared as an extension method.
