# IndexExtensions

## Purpose

`IndexExtensions` provides calculation and formatting helpers for `Index` objects. It builds a qualified database object name, summarizes index activity, converts storage size to mebibytes, and counts key plus included columns.

The static class is defined in the `SqlQueryAnalyzer.Models` namespace.

## Members

### GetQualifiedName(index)

```csharp
public static string GetQualifiedName(this Index index)
```

Returns the index name in `SchemaName.TableName.IndexName` format.

- **Parameters**:
  - `index`: The index to format.
- **Returns**: The fully qualified index name.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `index` is `null`.
  - `ArgumentException`: Thrown when `SchemaName`, `TableName`, or `IndexName` is `null` or empty.

### GetTotalUserOperations(index)

```csharp
public static long GetTotalUserOperations(this Index index)
```

Adds the index's user seeks, scans, lookups, and updates. Updates are included in the total alongside read operations.

- **Parameters**:
  - `index`: The index whose usage counters are summed.
- **Returns**: The sum of `UserSeeks`, `UserScans`, `UserLookups`, and `UserUpdates`.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `index` is `null`.
  - `OverflowException`: Thrown when the checked sum is outside the range of a `long`.

### GetSizeInMegabytes(index)

```csharp
public static double GetSizeInMegabytes(this Index index)
```

Converts `SizeInBytes` to mebibytes by dividing by 1,048,576 (`1024 * 1024`). The result is returned as a `double`, so fractional values are preserved.

- **Parameters**:
  - `index`: The index whose size is converted.
- **Returns**: The index size in MiB.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `index` is `null`.

### GetEffectiveColumnCount(index)

```csharp
public static int GetEffectiveColumnCount(this Index index)
```

Counts both key columns and included columns. A `null` collection contributes zero to the result.

- **Parameters**:
  - `index`: The index whose columns are counted.
- **Returns**: The combined number of entries in `Columns` and `IncludeColumns`.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `index` is `null`.

## Usage Example

```csharp
using System;
using SqlQueryAnalyzer.Models;

var index = new Index
{
    SchemaName = "sales",
    TableName = "Orders",
    IndexName = "IX_Orders_CustomerId",
    SizeInBytes = 25L * 1024 * 1024,
    UserSeeks = 1_200,
    UserScans = 25,
    UserLookups = 75,
    UserUpdates = 300,
    Columns =
    [
        new IndexColumn { ColumnName = "CustomerId", KeyOrdinal = 1 }
    ],
    IncludeColumns = ["OrderDate", "TotalAmount"]
};

// Index also defines an instance method with this name, so invoke the
// extension class explicitly when the extension method is required.
string qualifiedName = IndexExtensions.GetQualifiedName(index);
long operations = index.GetTotalUserOperations();
double sizeInMiB = index.GetSizeInMegabytes();
int columnCount = index.GetEffectiveColumnCount();

Console.WriteLine(qualifiedName);       // sales.Orders.IX_Orders_CustomerId
Console.WriteLine(operations);          // 1600
Console.WriteLine($"{sizeInMiB} MiB"); // 25 MiB
Console.WriteLine(columnCount);         // 3
```
