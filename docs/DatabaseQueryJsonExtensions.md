# DatabaseQueryJsonExtensions

## Purpose

`Models/DatabaseQueryJsonExtensions.cs` is currently an empty placeholder reserved for JSON-related extension methods for `DatabaseQuery`. It does not declare a namespace, type, serializer configuration, or extension methods, and therefore adds no runtime behavior to the library.

## Members

There are currently no members in `DatabaseQueryJsonExtensions.cs`. In particular, the file does not provide `ToJson`, `FromJson`, or `TryFromJson` methods for `DatabaseQuery`.

## Example

Because the placeholder exposes no API, serialize and deserialize a `DatabaseQuery` directly with `System.Text.Json` when JSON conversion is needed:

```csharp
using System;
using System.Text.Json;
using SqlQueryAnalyzer.Models;

var query = new DatabaseQuery
{
    QueryText = "SELECT Id FROM Customers",
    QueryType = QueryType.Select,
    ReferencedTables = ["Customers"]
};

string json = JsonSerializer.Serialize(query);
DatabaseQuery? restored = JsonSerializer.Deserialize<DatabaseQuery>(json);

Console.WriteLine(restored?.QueryText);
```

This example uses framework serialization directly; it does not call any member from `DatabaseQueryJsonExtensions.cs`.
