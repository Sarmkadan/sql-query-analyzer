# DatabaseQueryValidation

`DatabaseQueryValidation` provides extension methods for checking whether a `DatabaseQuery` contains the metadata and analyzed query details required by the application. The methods can return every detected problem, provide a boolean result, or throw when invalid data should stop processing.

## Purpose

Use these helpers after constructing, parsing, or deserializing a `DatabaseQuery` and before passing it to later processing stages. Validation checks:

- `QueryId` is not blank and, when it is a GUID, is not `Guid.Empty`.
- `QueryText`, `SchemaName`, and `CreatedBy` are not blank; `SchemaName` does not contain spaces.
- `QueryType` and `DatabaseType` are not `Unknown`.
- `CreatedDate` is non-default and UTC. When supplied, `ModifiedDate` is non-default and UTC, and requires `ModifiedBy`.
- `ReferencedTables` is non-null, contains at least one entry, and has no blank entries.
- `ReferencedColumns`, `JoinConditions`, and `WhereConditions` are non-null and have no blank entries.
- `Parameters` is non-null; its keys are not blank, its values are non-null, and every `ParameterInfo` has a parameter name and data type.
- `VariableDeclarations` is non-null and contains neither blank keys nor null or blank values.
- `LineCount` is not negative and is greater than zero when query text is present.
- When `ProcedureName`, `ModuleName`, `ApplicationName`, or `DatabaseName` is supplied, `QueryText` is also supplied.

The validation does not parse SQL or verify that `LineCount` exactly matches the text. Call `DatabaseQuery.Parse()` first when the query's analyzed fields need to be populated.

## Members

### `Validate(this DatabaseQuery value)`

Validates `value` and returns an `IReadOnlyList<string>` containing all detected problems. The list is empty when the query is valid and cannot be modified through the returned interface.

Throws `ArgumentNullException` when `value` is null.

### `IsValid(this DatabaseQuery value)`

Returns `true` when `Validate()` finds no problems; otherwise, returns `false`.

Throws `ArgumentNullException` when `value` is null.

`DatabaseQuery` also defines an instance method named `IsValid()` with a smaller set of checks. Calling `query.IsValid()` selects that instance method. Use `DatabaseQueryValidation.IsValid(query)` when the full validation described here is required.

### `EnsureValid(this DatabaseQuery value)`

Returns normally when `value` passes validation. When validation fails, it throws an `ArgumentException` whose message lists every detected problem. It throws `ArgumentNullException` when `value` is null.

## Example

```csharp
using System;
using SqlQueryAnalyzer.Models;

var query = new DatabaseQuery
{
    QueryText = "SELECT Id, Name FROM dbo.Customers WHERE IsActive = 1",
    QueryType = QueryType.Select,
    DatabaseType = DatabaseType.SqlServer,
    SchemaName = "dbo",
    CreatedBy = "reporting-service",
    CreatedDate = DateTime.UtcNow,
    LineCount = 1,
    ReferencedTables = ["dbo.Customers"],
    ReferencedColumns = ["Id", "Name", "IsActive"],
    WhereConditions = ["IsActive = 1"]
};

var problems = query.Validate();
if (problems.Count > 0)
{
    foreach (var problem in problems)
    {
        Console.WriteLine(problem);
    }
}

bool passesFullValidation = DatabaseQueryValidation.IsValid(query);

// Throws ArgumentException with all validation problems when invalid.
query.EnsureValid();
```
