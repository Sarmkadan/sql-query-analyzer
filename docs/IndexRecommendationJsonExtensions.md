# IndexRecommendationJsonExtensions

## Purpose

`IndexRecommendationJsonExtensions` provides `System.Text.Json` helpers for serializing and deserializing `IndexRecommendation` instances. It uses camel-case JSON property names, omits null-valued properties, and produces compact JSON unless indentation is requested.

The static class is defined in the `SqlQueryAnalyzer.Models` namespace.

## Members

### ToJson(value, indented = false)

```csharp
public static string ToJson(
    this IndexRecommendation value,
    bool indented = false)
```

Serializes an `IndexRecommendation` to JSON.

- **Parameters**:
  - `value`: The recommendation to serialize.
  - `indented`: `true` to format the JSON for readability; `false` to produce compact JSON. Defaults to `false`.
- **Returns**: The JSON representation of the recommendation.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `value` is `null`.

### FromJson(json)

```csharp
public static IndexRecommendation? FromJson(string json)
```

Deserializes JSON to an `IndexRecommendation`. Malformed JSON or JSON that cannot be converted to the model returns `null` instead of propagating `JsonException`.

- **Parameters**:
  - `json`: The JSON string to deserialize.
- **Returns**: The deserialized recommendation, or `null` when deserialization fails or the JSON value is `null`.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `json` is `null`.
  - `ArgumentException`: Thrown when `json` is empty.

### TryFromJson(json, out value)

```csharp
public static bool TryFromJson(
    string json,
    out IndexRecommendation? value)
```

Attempts to deserialize JSON without propagating `JsonException`.

- **Parameters**:
  - `json`: The JSON string to deserialize.
  - `value`: Receives the deserialized recommendation, or `null` if deserialization fails.
- **Returns**: `true` when no `JsonException` occurs; otherwise, `false`. The JSON literal `null` therefore returns `true` and sets `value` to `null`.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `json` is `null`.
  - `ArgumentException`: Thrown when `json` is empty.

## Usage Example

```csharp
using System;
using SqlQueryAnalyzer.Models;

var recommendation = new IndexRecommendation
{
    RecommendationId = "rec-orders-customer",
    TableName = "sales.Orders",
    KeyColumns = ["CustomerId", "OrderDate"],
    IncludeColumns = ["TotalAmount"],
    IndexType = "NONCLUSTERED",
    ImpactScore = 82.5,
    Rationale = "Supports customer order lookups.",
    Source = RecommendationSource.WhereClause,
    RecommendedAt = new DateTime(2026, 9, 15, 12, 0, 0, DateTimeKind.Utc)
};

recommendation.GenerateScript();

string json = recommendation.ToJson(indented: true);
Console.WriteLine(json);

IndexRecommendation? restored =
    IndexRecommendationJsonExtensions.FromJson(json);
Console.WriteLine(restored?.GeneratedScript);

if (IndexRecommendationJsonExtensions.TryFromJson(json, out var parsed))
{
    Console.WriteLine(parsed?.TableName);
}

bool valid = IndexRecommendationJsonExtensions.TryFromJson(
    "{ invalid json }",
    out IndexRecommendation? invalidRecommendation);
Console.WriteLine(valid); // False
```
