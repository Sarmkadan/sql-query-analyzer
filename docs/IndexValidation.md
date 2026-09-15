# IndexValidation

`IndexValidation` provides extension methods for validating an `Index` model
before it is consumed, persisted, or used to generate SQL. Validation does not
modify the index. It returns human-readable errors for all detected problems,
except that validation stops at the first invalid entry within each column
collection.

## Validation rules

`Validate` checks the following values:

- `IndexId` must be nonblank and parseable as a 32- or 36-character GUID.
- `IndexName`, `TableName`, `SchemaName`, and `FileGroup` must be nonblank,
  no longer than 128 characters, and valid SQL identifiers.
- A valid SQL identifier cannot start with a digit and may contain only letters,
  digits, underscores, and dollar signs.
- `FilterPredicate`, when present, must not exceed 2,048 characters.
- `Columns` must be non-null and contain at least one entry. Every entry must be
  non-null, have a valid `ColumnName` no longer than 128 characters, and have a
  non-negative `KeyOrdinal`.
- Every value in `IncludeColumns`, when the collection is present, must be a
  valid, nonblank SQL identifier no longer than 128 characters.
- `SizeInBytes`, `PageCount`, `UserSeeks`, `UserScans`, `UserLookups`,
  `UserUpdates`, `LastUserSeekTime`, `LastUserScanTime`, `FragmentCount`, and
  `TotalMaintenanceOperations` must be non-negative.
- `FragmentationPercentage` must be between 0 and 100, inclusive.
- `CreatedDate` must be non-default. `CreatedDate`, `LastModifiedDate`, and
  `LastStatisticsUpdate` cannot be more than five minutes in the future.
- `HealthNotes`, when present, must not exceed 1,024 characters.

`IndexType`, `HealthStatus`, and the Boolean properties are not validated. A
negative `LastUserScanTime` currently produces the same validation error twice
because that property is checked twice by the implementation.

## Members

### `Validate(Index value)`

```csharp
public static IReadOnlyList<string> Validate(this Index value)
```

Validates all supported properties and returns a read-only list of error
messages. The list is empty when the index is valid. Throws
`ArgumentNullException` when `value` is `null`.

For `Columns` and `IncludeColumns`, only the first invalid entry in each
collection is reported; validation of the remaining index properties continues.

### `IsValid(Index value)`

```csharp
public static bool IsValid(this Index value)
```

Returns `true` when `Validate` returns no errors; otherwise, returns `false`.
Throws `ArgumentNullException` when `value` is `null`.

This extension has the same name as the instance `Index.IsValid()` method. Call
`IndexValidation.IsValid(index)` explicitly when the comprehensive validation
rules on this page are required.

### `EnsureValid(Index value)`

```csharp
public static void EnsureValid(this Index value)
```

Returns normally when the index is valid. If validation fails, throws an
`ArgumentException` whose message contains every error returned by `Validate`,
one per line. Throws `ArgumentNullException` when `value` is `null`.

## Example

```csharp
using SqlQueryAnalyzer.Models;

var index = new SqlQueryAnalyzer.Models.Index
{
    IndexId = Guid.NewGuid().ToString(),
    IndexName = "IX_Orders_CustomerId",
    TableName = "Orders",
    SchemaName = "dbo",
    IndexType = IndexType.Nonclustered,
    FileGroup = "PRIMARY",
    Columns =
    [
        new IndexColumn
        {
            ColumnName = "CustomerId",
            KeyOrdinal = 1
        }
    ],
    IncludeColumns = ["OrderDate", "TotalAmount"],
    CreatedDate = DateTime.UtcNow
};

var errors = index.Validate();

if (errors.Count == 0)
{
    index.EnsureValid();
    Console.WriteLine("The index is valid.");
}
else
{
    foreach (var error in errors)
    {
        Console.WriteLine(error);
    }
}
```
