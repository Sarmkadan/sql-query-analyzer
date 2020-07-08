# SqlInjectionDetectorExtensions

Provides static helper members for analyzing and reporting SQL injection detection results. These members operate on the current detection context and return collections, grouped data, or formatted reports without requiring additional parameters.

## API

### FilterBySeverity
- **Purpose**: Returns a list of `SqlInjectionIssue` objects that have been filtered according to the detector's severity criteria.
- **Parameters**: None.
- **Return Value**: `List<SqlInjectionIssue>` containing the filtered issues; an empty list if no issues meet the criteria.
- **Exceptions**: May throw `InvalidOperationException` if the internal issue collection has not been initialized; may throw `NullReferenceException` if the underlying data source is null.

### GroupByType
- **Purpose**: Groups detected issues by their type identifier.
- **Parameters**: None.
- **Return Value**: `Dictionary<string, List<SqlInjectionIssue>>` where each key is an issue type and the corresponding value is a list of issues of that type; returns an empty dictionary when no issues are present.
- **Exceptions**: May throw `InvalidOperationException` if the issue collection has not been initialized; may throw `NullReferenceException` if the data source is null.

### GenerateSummaryReport
- **Purpose**: Produces a concise textual summary of the detection results.
- **Parameters**: None.
- **Return Value**: `string` containing the summary report; returns an empty string when there are no issues to report.
- **Exceptions**: May throw `InvalidOperationException` if the detector has not been run; may throw `NullReferenceException` if internal state is null.

### GenerateDetailedReport
- **Purpose**: Produces a comprehensive textual report detailing each detected issue.
- **Parameters**: None.
- **Return Value**: `string` containing the detailed report; returns an empty string when no issues are present.
- **Exceptions**: May throw `InvalidOperationException` if the detector has not been executed; may throw `NullReferenceException` if required data is missing.

### HasCriticalIssues
- **Purpose**: Indicates whether any detected issues are classified as critical.
- **Parameters**: None.
- **Return Value**: `true` if at least one critical issue exists; otherwise `false`.
- **Exceptions**: May throw `InvalidOperationException` if the issue collection has not been initialized; may throw `NullReferenceException` if the underlying data is null.

## Usage

```csharp
using SqlQueryAnalyzer;

// Assuming the detector has already been run and populated internal state.
var filtered = SqlInjectionDetectorExtensions.FilterBySeverity;
var grouped  = SqlInjectionDetectorExtensions.GroupByType;
var summary  = SqlInjectionDetectorExtensions.GenerateSummaryReport;
var detailed = SqlInjectionDetectorExtensions.GenerateDetailedReport;
bool critical = SqlInjectionDetectorExtensions.HasCriticalIssues;

// Example: act on the results
if (critical)
{
    Console.WriteLine("Critical issues detected!");
    Console.WriteLine(detailed);
}
else
{
    Console.WriteLine(summary);
}
```

```csharp
using System.Linq;
using SqlQueryAnalyzer;

// After analysis, retrieve issues of a specific type via the grouped dictionary.
var issuesByType = SqlInjectionDetectorExtensions.GroupByType;
if (issuesByType.TryGetValue("UnionBased", out var unionIssues))
{
    var count = unionIssues.Count;
    Console.WriteLine($"Found {count} Union‑based injection issues.");
}

// Generate a report only when issues exist.
var report = SqlInjectionDetectorExtensions.GenerateSummaryReport;
if (!string.IsNullOrEmpty(report))
{
    File.WriteAllText("InjectionReport.txt", report);
}
```

## Notes

- All members rely on the detector's internal state having been initialized prior to invocation. Calling them before analysis may result in exceptions.
- Return values reflect the current state at the moment of the call; concurrent modifications to the underlying issue collection by other threads can lead to inconsistent results.
- The static members are safe for concurrent **read‑only** access if the internal data is not being modified. If the detector can be updated while these members are being called, external synchronization (e.g., locking) is required to avoid race conditions.
- Empty collections or empty strings are returned rather than `null` when no data is available, simplifying null‑checking in consumer code.
- The `GroupByType` dictionary uses the issue type string as the key; callers should treat the key as case‑sensitive unless the detector normalizes it elsewhere.
