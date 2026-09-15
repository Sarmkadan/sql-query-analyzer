# IndexSuggestionValidation

`IndexSuggestionValidation` provides extension methods that validate an
`IndexSuggestion` before it is used. Validation reports all detected problems as
human-readable messages and does not modify the suggestion.

## Purpose

The validator checks the required identity, index definition, performance,
generated SQL, and analysis metadata fields. Its rules require:

- `SuggestionId`, `TableName`, `IndexName`, `GeneratedCreateScript`,
  `GeneratedDropScript`, and `Rationale` to be nonblank.
- `IndexColumns` to be non-null and contain at least one entry, with every entry
  nonblank.
- Every entry in the optional `IncludeColumns` and `ConflictingIndexes`
  collections to be nonblank when those collections are present.
- `IndexType` to be nonblank and contain no spaces.
- `EstimatedPerformanceGain` to be a finite, non-negative number.
- `EstimatedExecutionTimeReduction` to be a finite number from 0 through 100.
- `EstimatedIndexSizeKB` to be positive when specified.
- `EstimatedMaintenanceCost` to be non-negative when specified.
- `AffectedQueries` to be positive.
- `SuggestedAt` to be non-default and no more than one hour in the future.

The boolean properties `AlreadyExists`, `IsComposite`, and `IsCovering`, along
with compatibility aliases such as `ColumnName`, are not validated directly.

## Members

### `Validate(IndexSuggestion value)`

```csharp
public static IReadOnlyList<string> Validate(this IndexSuggestion value)
```

Evaluates all validation rules and returns a read-only list of error messages.
The list is empty when the suggestion is valid. Invalid collection entries are
reported with their zero-based index. Throws `ArgumentNullException` when
`value` is `null`, including when `IndexColumns` is `null`.

### `IsValid(IndexSuggestion value)`

```csharp
public static bool IsValid(this IndexSuggestion value)
```

Returns `true` when `Validate` returns no errors; otherwise, returns `false`.
Throws `ArgumentNullException` when `value` is `null`.

`IndexSuggestion` also has an instance method named `IsValid()` that checks a
smaller set of fields. Calling `suggestion.IsValid()` selects that instance
method. Call `IndexSuggestionValidation.IsValid(suggestion)` when the complete
validation rules documented here are required.

### `EnsureValid(IndexSuggestion value)`

```csharp
public static void EnsureValid(this IndexSuggestion value)
```

Returns normally when the suggestion is valid. If validation fails, it throws
an `ArgumentException` whose message contains every error returned by
`Validate`. Throws `ArgumentNullException` when `value` is `null`.

## Example

```csharp
using SqlQueryAnalyzer.Models;

var suggestion = new IndexSuggestion
{
    SuggestionId = Guid.NewGuid().ToString(),
    TableName = "Orders",
    IndexName = "IX_Orders_CustomerId_CreatedAt",
    IndexColumns = ["CustomerId", "CreatedAt"],
    IncludeColumns = ["TotalAmount"],
    IndexType = "NONCLUSTERED",
    EstimatedPerformanceGain = 72.5,
    EstimatedExecutionTimeReduction = 48,
    EstimatedIndexSizeKB = 4096,
    EstimatedMaintenanceCost = 3,
    GeneratedCreateScript =
        "CREATE NONCLUSTERED INDEX IX_Orders_CustomerId_CreatedAt " +
        "ON Orders (CustomerId, CreatedAt) INCLUDE (TotalAmount);",
    GeneratedDropScript =
        "DROP INDEX IF EXISTS IX_Orders_CustomerId_CreatedAt ON Orders;",
    AffectedQueries = 12,
    SuggestedAt = DateTime.UtcNow,
    Rationale = "Supports customer order history lookups."
};

var errors = suggestion.Validate();

if (IndexSuggestionValidation.IsValid(suggestion))
{
    suggestion.EnsureValid();
    Console.WriteLine("The index suggestion is valid.");
}
else
{
    foreach (var error in errors)
    {
        Console.WriteLine(error);
    }
}
```
