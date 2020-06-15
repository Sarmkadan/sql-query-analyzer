# AnalysisRequestDto

Represents the complete result of a SQL query analysis operation. This type aggregates the original request parameters, computed performance metrics, detected issues, index recommendations, and a human-readable summary into a single transfer object intended for serialization and consumption by API clients or downstream services.

## API

### `public string QueryText`
The original SQL query text submitted for analysis. This value is required and must not be null or empty. It serves as the primary input from which all other metrics and suggestions are derived.

### `public string? ApplicationName`
An optional identifier for the application or service that originated the query. When provided, it is used to contextualize recommendations and group analyses by source. Null when the caller did not supply an application name.

### `public string? ProcedureName`
An optional name of the stored procedure containing the query, if applicable. Used to correlate analysis results with specific database objects. Null when the query is ad-hoc or the procedure name is unknown.

### `public string? ModuleName`
An optional module or component name within the application. Provides finer-grained origin tracking than `ApplicationName` alone. Null when not specified.

### `public bool IncludeIndexSuggestions`
Indicates whether the analysis engine should generate index recommendations. When `true`, the `IndexSuggestions` list may be populated with proposed indexes. When `false`, `IndexSuggestions` will be empty regardless of detected opportunities.

### `public bool AnalyzeFragmentation`
Controls whether index fragmentation statistics are evaluated during analysis. When `true`, fragmentation-related issues may appear in the `Issues` list. When `false`, fragmentation checks are skipped entirely.

### `public bool AnalyzePlan`
Determines whether the query execution plan is parsed and inspected. When `true`, the `ExecutionPlanXml` property should contain a valid plan document, and plan-based issues are included in the analysis. When `false`, plan analysis is bypassed.

### `public string? ExecutionPlanXml`
The XML representation of the query's estimated or actual execution plan. Required when `AnalyzePlan` is `true`; otherwise null. Must be well-formed XML conforming to the SQL Server execution plan schema to produce valid plan-based diagnostics.

### `public string QueryId`
A unique identifier assigned to this analysis instance. This value is always populated and can be used to retrieve cached results or correlate logs across distributed systems.

### `public double PerformanceScore`
A computed numeric score representing overall query performance, typically on a scale where lower values indicate better performance. Derived from cost estimates, issue severity, and execution statistics. The exact range and interpretation depend on the analysis engine configuration.

### `public string ComplexityLevel`
A categorical label describing the structural complexity of the query (e.g., "Low", "Medium", "High", "Critical"). Determined by factors such as join count, subquery depth, and operator variety.

### `public int IssueCount`
The total number of performance issues detected, regardless of severity. Includes both critical and non-critical issues. Always equal to the count of elements in the `Issues` list.

### `public int CriticalIssueCount`
The subset of `IssueCount` representing issues classified as critical severity. These typically indicate problems requiring immediate attention, such as missing indexes on large tables or scan operations on high-traffic objects.

### `public List<PerformanceIssueDto> Issues`
A list of all detected performance issues, each represented as a `PerformanceIssueDto`. May be empty if no issues were found. Each entry contains at minimum an `IssueType`, `Severity`, and `Description`.

### `public List<IndexSuggestionDto> IndexSuggestions`
A list of proposed index creations or modifications. Populated only when `IncludeIndexSuggestions` is `true` and the analysis engine identifies beneficial index changes. Each entry includes the target table, column specifications, and estimated impact.

### `public string Summary`
A human-readable, plain-text summary of the analysis findings. Includes key metrics, the most critical issues, and top recommendations. Suitable for display in dashboards or notification messages.

### `public long AnalysisTimeMs`
The total wall-clock time consumed by the analysis operation, measured in milliseconds. Includes time spent on plan parsing, statistics retrieval, and recommendation generation.

### `public string IssueType`
A classification code for the primary type of issue represented by this DTO when used as part of an `Issues` collection entry. Examples include `"MissingIndex"`, `"ImplicitConversion"`, or `"ParameterSniffing"`.

### `public string Severity`
The severity level assigned to this issue. Common values are `"Critical"`, `"Warning"`, and `"Info"`. The value directly influences `CriticalIssueCount` when set to `"Critical"`.

### `public string Description`
A detailed, human-readable explanation of the issue, including affected objects, observed behavior, and potential remediation steps. This field is always populated when the DTO represents an issue entry.

## Usage

### Example 1: Submitting a query for full analysis and reading results

```csharp
var request = new AnalysisRequestDto
{
    QueryText = "SELECT * FROM Orders WHERE CustomerId = 42",
    ApplicationName = "OrderService",
    IncludeIndexSuggestions = true,
    AnalyzeFragmentation = true,
    AnalyzePlan = true,
    ExecutionPlanXml = retrievedPlanXml
};

AnalysisRequestDto result = await analyzer.AnalyzeAsync(request);

Console.WriteLine($"Score: {result.PerformanceScore}, Complexity: {result.ComplexityLevel}");
Console.WriteLine($"Issues: {result.IssueCount} ({result.CriticalIssueCount} critical)");
Console.WriteLine(result.Summary);

foreach (var suggestion in result.IndexSuggestions)
{
    Console.WriteLine($"Suggested index on {suggestion.TableName}: {suggestion.Columns}");
}
```

### Example 2: Lightweight analysis without plan inspection

```csharp
var request = new AnalysisRequestDto
{
    QueryText = "EXEC usp_GetCustomerHistory @CustomerId = 99",
    ProcedureName = "usp_GetCustomerHistory",
    ModuleName = "ReportingModule",
    IncludeIndexSuggestions = false,
    AnalyzeFragmentation = false,
    AnalyzePlan = false
};

AnalysisRequestDto result = await analyzer.AnalyzeAsync(request);

if (result.CriticalIssueCount > 0)
{
    foreach (var issue in result.Issues.Where(i => i.Severity == "Critical"))
    {
        logger.LogCritical($"Query {result.QueryId}: {issue.IssueType} - {issue.Description}");
    }
}
```

## Notes

- `QueryText` must be a non-empty string. Supplying null or whitespace will cause the analysis engine to throw an `ArgumentException` before processing begins.
- `ExecutionPlanXml` must be valid XML when `AnalyzePlan` is `true`. Malformed or empty plan documents result in a plan-specific issue being appended to `Issues` rather than a thrown exception, allowing partial results to be returned.
- `PerformanceScore` and `ComplexityLevel` are computed values; assigning them manually on a request object has no effect—they are overwritten by the analysis engine.
- `IssueCount` and `CriticalIssueCount` are derived from the `Issues` list. They are always consistent with the list contents after analysis completes.
- `AnalysisTimeMs` reflects server-side processing time only. Network latency and serialization overhead are excluded.
- This type is designed for serialization and data transfer. It is not thread-safe for concurrent mutation; instances should be treated as immutable snapshots after analysis completes. Concurrent reads from multiple threads are safe provided no thread modifies the object.
- When `IncludeIndexSuggestions` is `false`, `IndexSuggestions` is guaranteed to be an empty list, not null.
- The `IssueType`, `Severity`, and `Description` members are meaningful primarily within the context of `PerformanceIssueDto` entries in the `Issues` list. Their presence at the root level of `AnalysisRequestDto` supports flattened serialization formats where issue details are projected onto the parent object.
