# DatabaseQueryExtensions

## Purpose

`DatabaseQueryExtensions` provides convenience methods for inspecting a `DatabaseQuery`. It can determine whether a query references a table, return the names of its parameters, and identify supported data definition language (DDL) operations.

The class is defined in the `SqlQueryAnalyzer.Models` namespace.

## Members

### IsTableReferenced(query, tableName)

```csharp
public static bool IsTableReferenced(this DatabaseQuery query, string tableName)
```

Determines whether `query.ReferencedTables` contains `tableName`. The comparison is case-insensitive and uses ordinal comparison rules.

- **Parameters**:
  - `query`: The `DatabaseQuery` to inspect.
  - `tableName`: The table name to find.
- **Returns**: `true` when the table is referenced; otherwise, `false`.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `query` is `null`.
  - `ArgumentNullException`: Thrown when `tableName` is `null`.
  - `ArgumentException`: Thrown when `tableName` is empty.

### GetParameterNames(query)

```csharp
public static IReadOnlyList<string> GetParameterNames(this DatabaseQuery query)
```

Returns a read-only snapshot of the keys in `query.Parameters`. Changes made to the parameter dictionary after this method returns are not reflected in the returned list. Names retain the dictionary's enumeration order.

- **Parameters**:
  - `query`: The `DatabaseQuery` whose parameter names are requested.
- **Returns**: A read-only list containing the parameter names. The list is empty when the parameter dictionary is empty.
- **Exceptions**:
  - `NullReferenceException`: Thrown when `query` is `null`.

### IsDdl(query)

```csharp
public static bool IsDdl(this DatabaseQuery query)
```

Determines whether the query is classified as a supported DDL operation. The method returns `true` only when `QueryType` is `QueryType.Create` or `QueryType.Drop`; all other query types return `false`.

- **Parameters**:
  - `query`: The `DatabaseQuery` to classify.
- **Returns**: `true` for `Create` and `Drop` queries; otherwise, `false`.
- **Exceptions**:
  - `NullReferenceException`: Thrown when `query` is `null`.

## Usage Example

```csharp
using System;
using System.Collections.Generic;
using SqlQueryAnalyzer.Models;

var query = new DatabaseQuery
{
    QueryType = QueryType.Create,
    ReferencedTables = ["Customers"],
    Parameters =
    {
        ["@region"] = new ParameterInfo
        {
            ParameterName = "@region",
            DataType = "nvarchar"
        }
    }
};

bool referencesCustomers = query.IsTableReferenced("customers"); // true
IReadOnlyList<string> parameterNames = query.GetParameterNames();
bool isDdl = query.IsDdl(); // true

Console.WriteLine($"References Customers: {referencesCustomers}");
Console.WriteLine($"Parameters: {string.Join(", ", parameterNames)}");
Console.WriteLine($"Is DDL: {isDdl}");
```

The lowercase lookup succeeds because `IsTableReferenced` compares table names without regard to case.
