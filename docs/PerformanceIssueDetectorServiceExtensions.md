# PerformanceIssueDetectorServiceExtensions

The `PerformanceIssueDetectorServiceExtensions` class provides a set of static extension methods that simplify common operations on a performance issue detection service and on collections of `PerformanceIssue` objects. The async methods (`DetectIssuesAsync`, `DetectNPlusOneAsync`, `DetectJoinIssuesAsync`, `DetectIndexOpportunitiesAsync`) extend a service instance to perform specific types of analysis. The synchronous methods (`FilterBySeverity`, `GroupByIssueType`, `CalculateTotalImpact`, `GetPrioritizedFixes`) extend an `IEnumerable<PerformanceIssue>` to enable filtering, grouping, aggregation, and prioritization of detected issues.

## API

### `DetectIssuesAsync`
- **Purpose**: Asynchronously detects all performance issues in the current analysis context.
- **Parameters**: The service instance (the extended type).
- **Returns**: `Task<IReadOnlyList<PerformanceIssue>>` – a list of all detected performance issues.
- **Throws**: `ArgumentNullException` if the service instance is `null`.

### `DetectNPlusOneAsync`
- **Purpose**: Asynchronously detects N+1 query issues.
- **Parameters**: The service instance.
- **Returns**: `ValueTask<IReadOnlyList<PerformanceIssue>>` – a list of N+1 related performance issues.
- **Throws**: `ArgumentNullException` if the service instance is `null`.

### `DetectJoinIssuesAsync`
- **Purpose**: Asynchronously detects join-related performance issues (e.g., missing join predicates, Cartesian joins).
- **Parameters**: The service instance.
- **Returns**: `Task<IReadOnlyList<PerformanceIssue>>` – a list of join-related performance issues.
- **Throws**: `ArgumentNullException` if the service instance is `null`.

### `DetectIndexOpportunitiesAsync`
- **Purpose**: Asynchronously detects missing or underutilized index opportunities.
- **Parameters**: The service instance.
- **Returns**: `Task<IReadOnlyList<PerformanceIssue>>` – a list of index-related performance issues.
- **Throws**: `ArgumentNullException` if the service instance is `null`.

### `FilterBySeverity`
- **Purpose**: Filters a collection of performance issues to only those matching a specified severity level.
- **Parameters**:
  - `issues` – the source collection of `PerformanceIssue` objects.
  - `severity` – the `Severity` level to filter by.
- **Returns**: `IEnumerable<PerformanceIssue>` – the filtered sequence.
- **Throws**: `ArgumentNullException` if `issues` is `null`.

### `GroupByIssueType`
- **Purpose**: Groups a collection of performance issues by their `IssueType`.
- **Parameters**: `issues` – the source collection of `PerformanceIssue` objects.
- **Returns**: `IReadOnlyDictionary<IssueType, IReadOnlyList<PerformanceIssue>>` – a dictionary mapping each issue type to its list of issues.
- **Throws**: `ArgumentNullException` if `issues` is `null`.

### `CalculateTotalImpact`
- **Purpose**: Calculates the total cumulative impact score of all performance issues in the collection.
- **Parameters**: `issues` – the source collection of `PerformanceIssue` objects.
- **Returns**: `double` – the total impact value.
- **Throws**: `ArgumentNullException` if `issues` is `null`.

### `GetPrioritizedFixes`
- **Purpose**: Returns a list of human-readable fix recommendations ordered by priority (highest impact first).
- **Parameters**: `issues` – the source collection of `PerformanceIssue` objects.
- **Returns**: `IReadOnlyList<string>` – prioritized fix descriptions.
- **Throws**: `ArgumentNullException` if `issues` is `null`.

## Usage

The following examples demonstrate typical usage of the extension methods.

```csharp
using SqlQueryAnalyzer.Performance;
using System;
using System.Linq;
using System.Threading.Tasks;

public class Example1
{
    public async Task AnalyzeAndFilterAsync(IPerformanceIssueDetectorService detector)
    {
        // Detect all issues asynchronously
        IReadOnlyList<PerformanceIssue> allIssues = await detector.DetectIssuesAsync();

        // Filter to only high-severity issues
        var highSeverityIssues = allIssues.FilterBySeverity(Severity.High);

        // Group by issue type
        var grouped = highSeverityIssues.GroupByIssueType();

        // Output grouped results
        foreach (var kvp in grouped)
        {
            Console.WriteLine($"{kvp.Key}: {kvp.Value.Count} issues");
        }
    }
}
```

```csharp
using SqlQueryAnalyzer.Performance;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Example2
{
    public async Task PrioritizeAndReportAsync(IPerformanceIssueDetectorService detector)
    {
        // Detect N+1 and join issues separately
        var nPlusOneIssues = await detector.DetectNPlusOneAsync();
        var joinIssues = await detector.DetectJoinIssuesAsync();

        // Combine results
        var combined = new List<PerformanceIssue>(nPlusOneIssues);
        combined.AddRange(joinIssues);

        // Calculate total impact
        double totalImpact = combined.CalculateTotalImpact();
        Console.WriteLine($"Total estimated impact: {totalImpact:F2}");

        // Get prioritized fix recommendations
        IReadOnlyList<string> fixes = combined.GetPrioritizedFixes();
        foreach (string fix in fixes)
        {
            Console.WriteLine(fix);
        }
    }
}
```

## Notes

- All extension methods throw `ArgumentNullException` when the extended instance (the service or the collection) is `null`. Ensure that the input is not `null` before calling.
- The async methods (`DetectIssuesAsync`, `DetectNPlusOneAsync`, `DetectJoinIssuesAsync`, `DetectIndexOpportunitiesAsync`) are designed to be used with dependency injection or a service instance that implements the underlying detection logic. They do not maintain any internal state and are thread-safe as long as the service instance itself is thread-safe.
- The synchronous methods (`FilterBySeverity`, `GroupByIssueType`, `CalculateTotalImpact`, `GetPrioritizedFixes`) are pure functions that operate on the provided collection. They are thread-safe and can be called concurrently on different collections without side effects.
- When the input collection is empty, `FilterBySeverity` returns an empty sequence, `GroupByIssueType` returns an empty dictionary, `CalculateTotalImpact` returns `0.0`, and `GetPrioritizedFixes` returns an empty list.
- The `Severity` parameter in `FilterBySeverity` is expected to be a valid enum value; passing an undefined value may result in no matches or undefined behavior depending on the implementation.
