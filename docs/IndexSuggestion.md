# IndexSuggestion

Represents a recommendation for creating or modifying a database index produced by the SQL Query Analyzer. The type encapsulates all metadata needed to evaluate, generate scripts for, and track the impact of a suggested index.

## API

| Member | Purpose | Parameters | Return Value | Throws |
|--------|---------|------------|--------------|--------|
| `SuggestionId` | Unique identifier for the suggestion instance. | – | `string` | Does not throw under normal use. |
| `TableName` | Name of the table the index pertains to. | – | `string` | Does not throw under normal use. |
| `IndexName` | Name of the suggested index; populated by `GenerateIndexName`. | – | `string` | Does not throw under normal use. |
| `IndexColumns` | List of column names to be included in the index key. | – | `List<string>` | Does not throw under normal use. |
| `IncludeColumns` | List of column names to be added as INCLUDE columns. | – | `List<string>` | Does not throw under normal use. |
| `IndexType` | Type of index (e.g., "NONCLUSTERED", "CLUSTERED", "XML"). | – | `string` | Does not throw under normal use. |
| `EstimatedPerformanceGain` | Relative performance improvement expected from the index (0‑100). | – | `double` | Does not throw under normal use. |
| `EstimatedExecutionTimeReduction` | Estimated reduction in query execution time (milliseconds). | – | `double` | Does not throw under normal use. |
| `EstimatedIndexSizeKB` | Approximate size of the index in kilobytes; may be null if unknown. | – | `int?` | Does not throw under normal use. |
| `EstimatedMaintenanceCost` | Estimated ongoing maintenance cost (e.g., CPU‑seconds per day); may be null. | – | `int?` | Does not throw under normal use. |
| `GeneratedCreateScript` | T‑SQL script to create the index; filled by `GenerateCreateScript`. | – | `string` | Does not throw under normal use. |
| `GeneratedDropScript` | T‑SQL script to drop the index. | – | `string` | Does not throw under normal use. |
| `AffectedQueries` | Number of distinct queries that would benefit from the index. | – | `int` | Does not throw under normal use. |
| `SuggestedAt` | Timestamp when the suggestion was generated. | – | `DateTime` | Does not throw under normal use. |
| `Rationale` | Human‑readable explanation for why the index is recommended. | – | `string` | Does not throw under normal use. |
| `ConflictingIndexes` | List of existing index names that would conflict with this suggestion. | – | `List<string>` | Does not throw under normal use. |
| `AlreadyExists` | Indicates whether an identical index already exists on the table. | – | `bool` | Does not throw under normal use. |
| `IsValid` | Indicates whether the suggestion contains sufficient data to be considered actionable. | – | `bool` | Does not throw under normal use. |
| `GenerateIndexName()` | Constructs a default index name based on `TableName` and `IndexColumns` and assigns it to `IndexName`. | None | `void` | Throws `InvalidOperationException` if `TableName` is null/empty or `IndexColumns` is null/empty. |
| `GenerateCreateScript()` | Builds a CREATE INDEX statement using the current property values and stores it in `GeneratedCreateScript`. | None | `void` | Throws `InvalidOperationException` if any of `TableName`, `IndexName`, or `IndexColumns` are null/empty, or if `IndexType` is null/empty. |

## Usage

```csharp
using System;
using System.Collections.Generic;

// Example 1: Creating a suggestion and generating its scripts
var suggestion = new IndexSuggestion
{
    SuggestionId = "idx_001",
    TableName    = "Orders",
    IndexColumns = new List<string> { "CustomerId", "OrderDate" },
    IncludeColumns = new List<string> { "TotalAmount" },
    IndexType    = "NONCLUSTERED",
    EstimatedPerformanceGain = 42.5,
    EstimatedExecutionTimeReduction = 120,
    EstimatedIndexSizeKB = 1024,
    EstimatedMaintenanceCost = 5,
    AffectedQueries = 7,
    SuggestedAt = DateTime.UtcNow,
    Rationale = "Improves lookups by customer and date range.",
    ConflictingIndexes = new List<string> { "IX_Orders_CustomerId" },
    AlreadyExists = false,
    IsValid = true
};

suggestion.GenerateIndexName();          // populates IndexName
suggestion.GenerateCreateScript();       // populates GeneratedCreateScript

Console.WriteLine($"Suggested index: {suggestion.IndexName}");
Console.WriteLine(suggestion.GeneratedCreateScript);
```

```csharp
using System;
using System.Collections.Generic;

// Example 2: Checking validity before applying a suggestion
var suggestion = LoadSuggestionFromStore(); // hypothetical loader

if (!suggestion.IsValid)
{
    Console.WriteLine("Suggestion missing required data.");
    return;
}

if (suggestion.AlreadyExists)
{
    Console.WriteLine("Index already exists; skipping creation.");
    return;
}

// Optionally regenerate scripts if underlying data changed
suggestion.GenerateIndexName();
suggestion.GenerateCreateScript();

// Apply the script to the database (pseudo‑code)
Database.ExecuteNonQuery(suggestion.GeneratedCreateScript);
```

## Notes

- The `EstimatedIndexSizeKB` and `EstimatedMaintenanceCost` properties are nullable (`int?`) to allow cases where the analyzer cannot compute a reliable estimate; consumers should handle null values appropriately.
- `IndexColumns` and `IncludeColumns` are expected to be non‑null lists; if they are null, the generation methods will throw an `InvalidOperationException`.
- The type does **not** provide any synchronization mechanisms. Concurrent modification of the same `IndexSuggestion` instance from multiple threads is not thread‑safe and may lead to inconsistent state. External locking is required if shared access is needed.
- `GenerateIndexName` and `GenerateCreateScript` mutate the instance state; calling them repeatedly will overwrite the previously generated values.
- The `AlreadyExists` and `IsValid` flags are informational; they are not automatically updated by the generation methods and should be set by the caller based on external checks.
