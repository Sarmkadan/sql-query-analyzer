# IndexJsonExtensions

## Purpose

`IndexJsonExtensions` provides `System.Text.Json` helpers for converting an `Index` model to and from JSON. JSON property names use camel case, null-valued properties are omitted, and serialization is compact unless indentation is requested.

The static class is defined in the `SqlQueryAnalyzer.Models` namespace.

## Members

### ToJson(value, indented = false)

```csharp
public static string ToJson(this Index value, bool indented = false)
```

Serializes an `Index` instance to JSON.

- **Parameters**:
  - `value`: The index to serialize.
  - `indented`: `true` to produce indented JSON; `false` to produce compact JSON. Defaults to `false`.
- **Returns**: The JSON representation of the index.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `value` is `null`.

### FromJson(json)

```csharp
public static Index? FromJson(string json)
```

Deserializes JSON to an `Index` instance. Unlike `TryFromJson`, malformed JSON is reported to the caller as an exception.

- **Parameters**:
  - `json`: The JSON string to deserialize.
- **Returns**: The deserialized index, or `null` when the JSON value is `null`.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `json` is `null`.
  - `ArgumentException`: Thrown when `json` is empty.
  - `JsonException`: Thrown when `json` is malformed or cannot be converted to an `Index`.

### TryFromJson(json, out value)

```csharp
public static bool TryFromJson(string json, out Index? value)
```

Attempts to deserialize JSON without propagating `JsonException`. If deserialization fails or produces `null`, the method returns `false` and sets `value` to `null`.

- **Parameters**:
  - `json`: The JSON string to deserialize.
  - `value`: Receives the deserialized index on success; otherwise, `null`.
- **Returns**: `true` when deserialization produces a non-null `Index`; otherwise, `false`.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `json` is `null`.
  - `ArgumentException`: Thrown when `json` is empty.

## Usage Example

```csharp
using System;
using SqlQueryAnalyzer.Models;

var index = new Index
{
    IndexName = "IX_Orders_CustomerId",
    TableName = "Orders",
    SchemaName = "sales",
    IndexType = IndexType.Nonclustered,
    Columns =
    [
        new IndexColumn
        {
            ColumnName = "CustomerId",
            KeyOrdinal = 1
        }
    ],
    IncludeColumns = ["OrderDate", "TotalAmount"]
};

string json = index.ToJson(indented: true);
Console.WriteLine(json);

Index? restored = IndexJsonExtensions.FromJson(json);
Console.WriteLine(restored?.GetQualifiedName());

if (IndexJsonExtensions.TryFromJson(json, out Index? parsed))
{
    Console.WriteLine($"Parsed {parsed.IndexName} with {parsed.Columns.Count} key column(s).");
}
```
