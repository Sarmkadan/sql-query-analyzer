# QueryPlanAnalyzerServiceExtensions

The `QueryPlanAnalyzerServiceExtensions` class provides a set of static extension methods designed to augment the `QueryPlanAnalyzerService` with high-level analytical capabilities. These methods facilitate the extraction of specific operational patterns, performance metrics, and structured reports from SQL query execution plans without requiring direct manipulation of the underlying plan tree. By leveraging asynchronous operations where I/O or complex traversal is involved, this utility layer enables efficient diagnostic workflows for database performance tuning and optimization.

## API

### GetExpensiveOperationsAsync
Asynchronously identifies and retrieves a list of plan nodes representing operations with high resource consumption, such as excessive CPU usage or memory grants.
*   **Parameters**: Accepts the target `QueryPlanAnalyzerService` instance as the `this` parameter.
*   **Returns**: A `Task<IReadOnlyList<PlanNode>>` containing the identified expensive operations.
*   **Throws**: May throw exceptions if the underlying plan service is uninitialized or if the plan tree cannot be traversed.

### GetIndexOperationsAsync
Asynchronously scans the execution plan to locate all nodes involving index interactions, including seeks, scans, and lookups.
*   **Parameters**: Accepts the target `QueryPlanAnalyzerService` instance as the `this` parameter.
*   **Returns**: A `Task<IReadOnlyList<PlanNode>>` containing nodes where index operations occur.
*   **Throws**: May throw exceptions if the plan structure is invalid or inaccessible.

### GetPlanSummaryAsync
Asynchronously generates a concise summary of the execution plan in a key-value format, highlighting metadata such as total cost, estimated rows, and execution type.
*   **Parameters**: Accepts the target `QueryPlanAnalyzerService` instance as the `this` parameter.
*   **Returns**: A `Task<Dictionary<string, object>>` mapping summary keys to their respective values.
*   **Throws**: May throw exceptions if the plan data is corrupted or missing required metadata fields.

### GroupByIssueType
Synchronously categorizes detected performance issues by their specific `IssueType`, providing a dictionary mapping each issue category to a list of associated `PerformanceIssue` instances.
*   **Parameters**: Accepts the target `QueryPlanAnalyzerService` instance as the `this` parameter.
*   **Returns**: An `IReadOnlyDictionary<IssueType, IReadOnlyList<PerformanceIssue>>` where keys represent issue categories and values are lists of specific issues.
*   **Throws**: May throw exceptions if the internal issue detection logic encounters an inconsistent state.

### GetHighImpactTableScansAsync
Asynchronously filters the execution plan to identify table scan operations that exceed a defined threshold for impact, typically based on row counts or I/O cost.
*   **Parameters**: Accepts the target `QueryPlanAnalyzerService` instance as the `this` parameter.
*   **Returns**: A `Task<IReadOnlyList<PlanNode>>` containing high-impact table scan nodes.
*   **Throws**: May throw exceptions if the plan traversal fails or if cost estimation data is unavailable.

### GetPerformanceScoreAsync
Asynchronously calculates an aggregate performance score for the analyzed query plan, typically normalized on a numerical scale.
*   **Parameters**: Accepts the target `QueryPlanAnalyzerService` instance as the `this` parameter.
*   **Returns**: A `Task<int>` representing the calculated performance score.
*   **Throws**: May throw exceptions if the scoring algorithm encounters missing data required for calculation.

### GetAnalysisReportAsync
Asynchronously compiles a comprehensive textual report detailing the analysis findings, including identified bottlenecks, recommendations, and plan statistics.
*   **Parameters**: Accepts the target `QueryPlanAnalyzerService` instance as the `this` parameter.
*   **Returns**: A `Task<string>` containing the formatted analysis report.
*   **Throws**: May throw exceptions if report generation fails due to internal formatting errors or missing plan data.

## Usage

The following example demonstrates how to retrieve expensive operations and calculate a performance score for a given analyzer service instance.

```csharp
using SqlQueryAnalyzer;
using SqlQueryAnalyzer.Models;

public async Task AnalyzeQueryPerformanceAsync(QueryPlanAnalyzerService analyzer)
{
    // Retrieve operations consuming significant resources
    var expensiveOps = await analyzer.GetExpensiveOperationsAsync();
    
    foreach (var op in expensiveOps)
    {
        Console.WriteLine($"High Cost Node: {op.OperationType} (Cost: {op.EstimatedCost})");
    }

    // Calculate the overall performance score
    int score = await analyzer.GetPerformanceScoreAsync();
    Console.WriteLine($"Overall Performance Score: {score}/100");
}
```

The next example illustrates generating a full analysis report and grouping detected issues by their type for targeted remediation.

```csharp
using SqlQueryAnalyzer;
using SqlQueryAnalyzer.Enums;
using System.Collections.Generic;

public async Task GenerateDiagnosticReportAsync(QueryPlanAnalyzerService analyzer)
{
    // Generate the full text report
    string report = await analyzer.GetAnalysisReportAsync();
    System.IO.File.WriteAllText("diagnostics.txt", report);

    // Group issues by type for specific handling
    var issuesByType = analyzer.GroupByIssueType;

    if (issuesByType.TryGetValue(IssueType.MissingIndex, out var missingIndices))
    {
        foreach (var issue in missingIndices)
        {
            Console.WriteLine($"Recommendation: Create index for {issue.TargetObject}");
        }
    }
}
```

## Notes

*   **Thread Safety**: As this class consists entirely of static extension methods operating on passed instances, thread safety depends on the implementation of the underlying `QueryPlanAnalyzerService`. While the extension methods themselves do not maintain static mutable state, concurrent calls to asynchronous methods on the same service instance should be coordinated if the service is not internally thread-safe.
*   **Synchronous vs Asynchronous**: The `GroupByIssueType` property is the only synchronous member; it assumes that issue detection has already occurred or is computationally inexpensive enough to run on the calling thread. All other members are asynchronous and should be awaited to prevent blocking the calling thread during plan traversal or I/O operations.
*   **Empty Results**: Methods returning lists (`IReadOnlyList<PlanNode>`) will return an empty collection rather than `null` if no matching nodes are found. Consumers should check for `Count == 0` rather than performing null checks.
*   **Dependency State**: These extensions assume the `QueryPlanAnalyzerService` instance has been properly initialized with a valid execution plan. Invoking these methods on a service instance without a loaded plan will likely result in runtime exceptions.
