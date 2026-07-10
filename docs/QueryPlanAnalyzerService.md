# QueryPlanAnalyzerService

Provides analysis and interpretation of SQL execution plans, enabling extraction of structural plan details, identification of missing indexes, and detection of performance issues from XML-based execution plan representations.

## API

### QueryPlanAnalyzerService

Initializes a new instance of the service. No configuration parameters are required at construction.

### ParseExecutionPlanAsync

```csharp
public async Task<QueryPlan?> ParseExecutionPlanAsync(string executionPlanXml)
```

Parses a raw XML execution plan string into a structured `QueryPlan` object.

**Parameters:**
- `executionPlanXml` (`string`): The XML content of a SQL execution plan, typically obtained from `SET SHOWPLAN_XML ON` or `sys.dm_exec_query_plan`.

**Return Value:**
A `Task<QueryPlan?>` that yields the deserialized plan object on success, or `null` if the input is empty, whitespace-only, or cannot be parsed as a valid execution plan.

**Exceptions:**
- Throws `ArgumentException` when `executionPlanXml` is `null`.
- Throws `InvalidOperationException` when the XML is malformed or does not conform to the expected execution plan schema.

### GetMissingIndexesAsync

```csharp
public async Task<List<string>> GetMissingIndexesAsync(QueryPlan queryPlan)
```

Extracts missing index recommendations embedded within the execution plan.

**Parameters:**
- `queryPlan` (`QueryPlan`): A parsed execution plan object obtained from `ParseExecutionPlanAsync`.

**Return Value:**
A `Task<List<string>>` containing human-readable descriptions of recommended missing indexes. Returns an empty list when no missing index suggestions are present in the plan.

**Exceptions:**
- Throws `ArgumentNullException` when `queryPlan` is `null`.

### AnalyzePlanAsync

```csharp
public async Task<List<PerformanceIssue>> AnalyzePlanAsync(QueryPlan queryPlan)
```

Performs comprehensive performance analysis on an execution plan, identifying potential bottlenecks and problematic operators.

**Parameters:**
- `queryPlan` (`QueryPlan`): A parsed execution plan object obtained from `ParseExecutionPlanAsync`.

**Return Value:**
A `Task<List<PerformanceIssue>>` containing detected performance concerns such as table scans, key lookups, implicit conversions, or excessive operator costs. Returns an empty list when no issues are identified.

**Exceptions:**
- Throws `ArgumentNullException` when `queryPlan` is `null`.

## Usage

### Example 1: Basic Plan Parsing and Missing Index Detection

```csharp
var service = new QueryPlanAnalyzerService();

// Obtain execution plan XML from SQL Server
string planXml = await GetPlanFromSqlServerAsync();

QueryPlan? plan = await service.ParseExecutionPlanAsync(planXml);
if (plan is not null)
{
    List<string> missingIndexes = await service.GetMissingIndexesAsync(plan);
    foreach (string index in missingIndexes)
    {
        Console.WriteLine($"Recommended index: {index}");
    }
}
```

### Example 2: Full Performance Audit

```csharp
var service = new QueryPlanAnalyzerService();

string planXml = File.ReadAllText(@"C:\Plans\problematic-query.sqlplan");
QueryPlan? plan = await service.ParseExecutionPlanAsync(planXml);

if (plan is null)
{
    Console.WriteLine("Failed to parse execution plan.");
    return;
}

List<string> missingIndexes = await service.GetMissingIndexesAsync(plan);
List<PerformanceIssue> issues = await service.AnalyzePlanAsync(plan);

Console.WriteLine($"Found {missingIndexes.Count} missing index recommendations.");
Console.WriteLine($"Found {issues.Count} performance issues.");

foreach (PerformanceIssue issue in issues.OrderByDescending(i => i.Severity))
{
    Console.WriteLine($"[{issue.Severity}] {issue.Description} — Operator: {issue.OperatorName}");
}
```

## Notes

- All asynchronous methods are CPU-bound operations performing XML parsing and in-memory analysis; they do not establish database connections or perform I/O. ConfigureAwait(false) is safe when not synchronizing with a UI context.
- `ParseExecutionPlanAsync` returns `null` for empty or whitespace-only strings rather than throwing, allowing callers to gracefully handle missing plan data without try/catch blocks.
- `GetMissingIndexesAsync` and `AnalyzePlanAsync` require a non-null `QueryPlan` instance. Passing `null` will result in an `ArgumentNullException`; always guard with a null check after parsing.
- The service holds no mutable state and is inherently thread-safe. A single instance can be shared across multiple concurrent operations without synchronization.
- Execution plan XML from different SQL Server versions may contain version-specific elements. Parsing succeeds for standard Showplan XML schema versions; non-standard or heavily customized plans may cause `InvalidOperationException` during parsing.
- `PerformanceIssue` objects returned by `AnalyzePlanAsync` include a `Severity` property that callers should evaluate to prioritize remediation efforts. Issues are not deduplicated across plan operators.
