# IndexRecommendationExtensions

## Purpose

`IndexRecommendationExtensions` provides convenience methods for presenting and working with `IndexRecommendation` objects. It can create a concise summary, return the complete ordered set of index columns, classify a recommendation by impact, and ensure that its T-SQL creation script has been generated.

The static class is defined in the `SqlQueryAnalyzer.Models` namespace. Every member throws `ArgumentNullException` when the recommendation is `null`.

## Members

### GetSummary(recommendation)

```csharp
public static string GetSummary(this IndexRecommendation recommendation)
```

Returns an invariant-culture summary containing the table name, index type, and impact score. The impact score uses the `P1` percentage format, so a value of `0.82` is displayed as `82.0 %` or the invariant-culture equivalent.

- **Parameters**:
  - `recommendation`: The recommendation to summarize.
- **Returns**: A formatted summary in the form `Table: ..., Index Type: ..., Impact: ...`.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `recommendation` is `null`.

### GetAllColumns(recommendation)

```csharp
public static IReadOnlyList<string> GetAllColumns(this IndexRecommendation recommendation)
```

Returns a new read-only list containing the key columns first, followed by the included columns. Changes to the returned list are not permitted, and later collection changes on the recommendation do not alter that list.

- **Parameters**:
  - `recommendation`: The recommendation whose columns are retrieved.
- **Returns**: The ordered key and included column names. A `null` `IncludeColumns` collection is treated as empty.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `recommendation` is `null`.

### IsHighImpact(recommendation, threshold)

```csharp
public static bool IsHighImpact(
    this IndexRecommendation recommendation,
    double threshold = 0.75)
```

Compares `ImpactScore` with a configurable inclusive threshold.

- **Parameters**:
  - `recommendation`: The recommendation to evaluate.
  - `threshold`: The minimum score considered high impact. Defaults to `0.75` and must be between `0.0` and `1.0`, inclusive.
- **Returns**: `true` when `ImpactScore` is greater than or equal to `threshold`; otherwise `false`.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `recommendation` is `null`.
  - `ArgumentException`: Thrown when `threshold` is less than `0.0` or greater than `1.0`.

### EnsureScriptGenerated(recommendation)

```csharp
public static string EnsureScriptGenerated(
    this IndexRecommendation recommendation)
```

Returns the existing `GeneratedScript` when it contains non-whitespace text. If it is empty or whitespace, calls `IndexRecommendation.GenerateScript()`, which populates the property, and returns the generated T-SQL.

- **Parameters**:
  - `recommendation`: The recommendation whose creation script is required.
- **Returns**: The existing or newly generated `CREATE INDEX` script.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `recommendation` is `null`.

## Usage Example

```csharp
using System;
using System.Collections.Generic;
using SqlQueryAnalyzer.Models;

var recommendation = new IndexRecommendation
{
    TableName = "sales.Orders",
    IndexType = "NONCLUSTERED",
    ImpactScore = 0.82,
    KeyColumns = ["CustomerId", "OrderDate"],
    IncludeColumns = ["TotalAmount"]
};

string summary = recommendation.GetSummary();
IReadOnlyList<string> columns = recommendation.GetAllColumns();
bool isHighImpact = recommendation.IsHighImpact();
string script = recommendation.EnsureScriptGenerated();

Console.WriteLine(summary);
Console.WriteLine(string.Join(", ", columns));
Console.WriteLine($"High impact: {isHighImpact}");
Console.WriteLine(script);
```

The example produces a summary for the recommendation, returns the columns in the order `CustomerId`, `OrderDate`, `TotalAmount`, classifies the `0.82` score as high impact using the default threshold, and generates the index creation script because `GeneratedScript` was initially empty.
