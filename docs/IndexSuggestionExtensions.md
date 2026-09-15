# IndexSuggestionExtensions

## Purpose

`IndexSuggestionExtensions` provides convenience methods for inspecting and presenting `IndexSuggestion` objects. It combines key and included columns into one read-only result, checks whether an estimated performance gain meets a threshold, and creates a concise display string.

The static class is defined in the `SqlQueryAnalyzer.Models` namespace.

## Members

### GetAllColumns(suggestion)

```csharp
public static IReadOnlyList<string> GetAllColumns(
    this IndexSuggestion suggestion)
```

Returns a new read-only list containing `IndexColumns` first, followed by `IncludeColumns`. The method preserves the order and values of both source collections, including duplicate column names. Because the result is a new list, later changes to either source collection do not change the returned list.

- **Parameters**:
  - `suggestion`: The index suggestion whose columns are retrieved.
- **Returns**: The ordered key and included column names.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `suggestion`, `suggestion.IndexColumns`, or `suggestion.IncludeColumns` is `null`.

### HasSignificantGain(suggestion, threshold)

```csharp
public static bool HasSignificantGain(
    this IndexSuggestion suggestion,
    double threshold)
```

Compares `EstimatedPerformanceGain` with an inclusive threshold. The method does not validate or normalize the threshold.

- **Parameters**:
  - `suggestion`: The index suggestion to evaluate.
  - `threshold`: The minimum estimated performance gain considered significant.
- **Returns**: `true` when `EstimatedPerformanceGain` is greater than or equal to `threshold`; otherwise `false`.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `suggestion` is `null`.

### ToDisplayString(suggestion)

```csharp
public static string ToDisplayString(
    this IndexSuggestion suggestion)
```

Formats the index name, index type, table name, and estimated gain using invariant culture. The gain is rounded to one digit after the decimal point.

- **Parameters**:
  - `suggestion`: The index suggestion to format.
- **Returns**: A string in the form `IndexName (IndexType) on TableName. Est. Gain: 42.5%`.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `suggestion` is `null`.

## Usage Example

```csharp
using System;
using System.Collections.Generic;
using SqlQueryAnalyzer.Models;

var suggestion = new IndexSuggestion
{
    TableName = "sales.Orders",
    IndexName = "IX_Orders_CustomerId_OrderDate",
    IndexType = "NONCLUSTERED",
    IndexColumns = ["CustomerId", "OrderDate"],
    IncludeColumns = ["TotalAmount"],
    EstimatedPerformanceGain = 42.5
};

IReadOnlyList<string> columns = suggestion.GetAllColumns();
bool isSignificant = suggestion.HasSignificantGain(40.0);
string displayText = suggestion.ToDisplayString();

Console.WriteLine(string.Join(", ", columns));
// CustomerId, OrderDate, TotalAmount

Console.WriteLine(isSignificant);
// True

Console.WriteLine(displayText);
// IX_Orders_CustomerId_OrderDate (NONCLUSTERED) on sales.Orders. Est. Gain: 42.5%
```
