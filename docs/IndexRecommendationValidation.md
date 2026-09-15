# IndexRecommendationValidation

`IndexRecommendationValidation` provides extension methods for validating an
`IndexRecommendation` before it is consumed or persisted. Validation reports all
detected problems as human-readable messages and does not modify the recommendation.

The validation rules require:

- `RecommendationId` to be nonblank and parseable as a GUID.
- `TableName` to be nonblank and no longer than 128 characters.
- `KeyColumns` to be non-null and contain at least one valid SQL identifier.
- Each key or included column to be nonblank, no longer than 128 characters, and
  composed of letters, digits, underscores, or dollar signs without starting with
  a digit.
- `IndexType` to be `CLUSTERED` or `NONCLUSTERED` (case-insensitive) and no longer
  than 32 characters.
- `ImpactScore` to be between 0 and 100, inclusive.
- `Rationale` to be nonblank and no longer than 2,048 characters.
- `GeneratedScript` to be nonblank and no longer than 8,192 characters.
- `RecommendedAt` to be non-default and no more than five minutes in the future.

`Source` is not checked because it is an enum value.

## Members

### `Validate(IndexRecommendation value)`

```csharp
public static IReadOnlyList<string> Validate(this IndexRecommendation value)
```

Validates `value` against every rule and returns a read-only list of error
messages. The list is empty when the recommendation is valid. Throws
`ArgumentNullException` when `value` is `null`.

For column collections, validation stops after the first invalid entry in each
collection, while validation of the remaining recommendation properties continues.

### `IsValid(IndexRecommendation value)`

```csharp
public static bool IsValid(this IndexRecommendation value)
```

Returns `true` when `Validate` produces no errors; otherwise, returns `false`.
Throws `ArgumentNullException` when `value` is `null`.

### `EnsureValid(IndexRecommendation value)`

```csharp
public static void EnsureValid(this IndexRecommendation value)
```

Returns normally when the recommendation is valid. When validation fails, it
throws an `ArgumentException` whose message contains every error returned by
`Validate`. It throws `ArgumentNullException` when `value` is `null`.

## Example

```csharp
using SqlQueryAnalyzer.Models;

var recommendation = new IndexRecommendation
{
    RecommendationId = Guid.NewGuid().ToString(),
    TableName = "Orders",
    KeyColumns = ["CustomerId", "CreatedAt"],
    IncludeColumns = ["TotalAmount"],
    IndexType = "NONCLUSTERED",
    ImpactScore = 82.5,
    Rationale = "Supports customer order history lookups.",
    GeneratedScript =
        "CREATE NONCLUSTERED INDEX IX_Orders_CustomerId_CreatedAt " +
        "ON Orders (CustomerId, CreatedAt) INCLUDE (TotalAmount);",
    Source = RecommendationSource.WhereClause,
    RecommendedAt = DateTime.UtcNow
};

var errors = recommendation.Validate();

if (errors.Count == 0)
{
    recommendation.EnsureValid();
    Console.WriteLine("The index recommendation is valid.");
}
else
{
    foreach (var error in errors)
    {
        Console.WriteLine(error);
    }
}
```
