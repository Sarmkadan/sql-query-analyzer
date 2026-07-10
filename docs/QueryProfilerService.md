# QueryProfilerService

The `QueryProfilerService` provides asynchronous utilities for analyzing SQL queries, collecting execution metrics, generating optimization suggestions, and comparing profiling results. It is intended to be used in diagnostic tools, performance testing harnesses, or monitoring components that require programmatic access to query profiling data.

## API

### QueryProfilerService()
Creates a new instance of the service. The constructor has no parameters and does not throw exceptions under normal circumstances.

### ProfileQueryAsync(string query, CancellationToken cancellationToken = default)
Profiles a single SQL query and returns a detailed report.

- **purpose**: Executes the supplied query against the configured data source, measures execution time, resource usage, and returns a `QueryProfilerReport` containing the results.
- **parameters**:
  - `query`: The SQL statement to profile. Must not be `null` or whitespace.
  - `cancellationToken`: Optional token to cancel the operation.
- **return value**: A `Task<QueryProfilerReport>` that completes with the profiling report.
- **exceptions**:
  - `ArgumentNullException` if `query` is `null`.
  - `ArgumentException` if `query` consists only of whitespace.
  - `InvalidOperationException` if the service cannot establish a connection to the data source.
  - `OperationCanceledException` if the operation is canceled via `cancellationToken`.

### ProfileQueryAsync(string query, IEnumerable<SqlParameter> parameters, CancellationToken cancellationToken = default)
Profiles a single SQL query with supplied parameters.

- **purpose**: Similar to the parameter‑less overload, but allows parameterized queries to be profiled with the given `SqlParameter` values.
- **parameters**:
  - `query`: The SQL statement containing parameter placeholders. Must not be `null` or whitespace.
  - `parameters`: Collection of `SqlParameter` objects providing values for the placeholders. May be `null` or empty if the query has no parameters.
  - `cancellationToken`: Optional token to cancel the operation.
- **return value**: A `Task<QueryProfilerReport>` that completes with the profiling report.
- **exceptions**:
  - `ArgumentNullException` if `query` is `null`.
  - `ArgumentException` if `query` consists only of whitespace.
  - `InvalidOperationException` if the service cannot establish a connection to the data source.
  - `OperationCanceledException` if the operation is canceled via `cancellationToken`.

### ProfileBatchAsync(IEnumerable<string> queries, CancellationToken cancellationToken = default)
Profiles a collection of SQL queries concurrently and returns a list of reports.

- **purpose**: Executes each query in `queries` (using the parameter‑less overload internally) and gathers individual `QueryProfilerReport` instances. The method returns when all queries have completed or when cancellation is requested.
- **parameters**:
  - `queries`: Sequence of SQL statements to profile. Must not be `null`; individual elements must not be `null` or whitespace.
  - `cancellationToken`: Optional token to cancel the operation.
- **return value**: A `Task<List<QueryProfilerReport>>` that completes with a list of reports in the same order as the input sequence.
- **exceptions**:
  - `ArgumentNullException` if `queries` is `null`.
  - `ArgumentException` if any element in `queries` is `null` or whitespace.
  - `InvalidOperationException` if the service cannot establish a connection to the data source.
  - `OperationCanceledException` if the operation is canceled via `cancellationToken`.

### GenerateSuggestionsAsync(QueryProfilerReport report, CancellationToken cancellationToken = default)
Produces optimization suggestions based on a profiling report.

- **purpose**: Analyzes the metrics contained in `report` and returns a list of `ProfilerSuggestion` objects that recommend indexes, query rewrites, or configuration changes.
- **parameters**:
  - `report`: The `QueryProfilerReport` to analyze. Must not be `null`.
  - `cancellationToken`: Optional token to cancel the operation.
- **return value**: A `Task<List<ProfilerSuggestion>>` that completes with the generated suggestions.
- **exceptions**:
  - `ArgumentNullException` if `report` is `null`.
  - `InvalidOperationException` if the report lacks required data for suggestion generation.
  - `OperationCanceledException` if the operation is canceled via `cancellationToken`.

### CompareProfilesAsync(QueryProfilerReport baseline, QueryProfilerReport current, CancellationToken cancellationToken = default)
Compares two profiling reports to highlight performance differences.

- **purpose**: Computes deltas between `baseline` and `current` reports (e.g., execution time, reads, CPU) and returns a `ProfileComparison` summarizing regressions or improvements.
- **parameters**:
  - `baseline`: The reference `QueryProfilerReport`. Must not be `null`.
  - `current`: The report to compare against the baseline. Must not be `null`.
  - `cancellationToken`: Optional token to cancel the operation.
- **return value**: A `Task<ProfileComparison>` that completes with the comparison result.
- **exceptions**:
  - `ArgumentNullException` if either `baseline` or `current` is `null`.
  - `InvalidOperationException` if the reports are incompatible (e.g., different query texts) and cannot be compared.
  - `OperationCanceledException` if the operation is canceled via `cancellationToken`.

## Usage

```csharp
using System.Threading;
using System.Threading.Tasks;
using SqlQueryAnalyzer.Profiling;

// Example 1: Profile a single query and display its duration.
var profiler = new QueryProfilerService();
string sql = @"SELECT o.OrderId, c.CustomerName
               FROM Orders o
               JOIN Customers c ON o.CustomerId = c.CustomerId
               WHERE o.OrderDate >= @StartDate";

var report = await profiler.ProfileQueryAsync(
    sql,
    new[] { new SqlParameter("@StartDate", DateTime.UtcNow.AddDays(-30)) },
    CancellationToken.None);

Console.WriteLine($"Query executed in {report.Duration.TotalMilliseconds:F0} ms");
```

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using SqlQueryAnalyzer.Profiling;

// Example 2: Profile a batch of queries, generate suggestions, and compare results.
var profiler = new QueryProfilerService();
IEnumerable<string> batch = new[]
{
    "SELECT * FROM Products WHERE Price > 100",
    "SELECT CategoryId, COUNT(*) FROM Products GROUP BY CategoryId",
    "UPDATE Inventory SET Quantity = Quantity - 1 WHERE ProductId = 42"
};

List<QueryProfilerReport> reports = await profiler.ProfileBatchAsync(batch);

// Generate suggestions for the most expensive report.
var expensive = reports.Find(r => r.Duration == reports.Max(x => x.Duration));
List<ProfilerSuggestion> suggestions = await profiler.GenerateSuggestionsAsync(expensive);

// Compare the first and last report to see if performance changed over time.
ProfileComparison comparison = await profiler.CompareProfilesAsync(reports[0], reports[^1]);

// Use suggestions and comparison as needed (e.g., logging, alerting).
```

## Notes

- The service does not maintain mutable state after construction; therefore instances are safe to use concurrently from multiple threads.
- All public methods are asynchronous and respect the supplied `CancellationToken`. If cancellation is triggered, any in‑flight database operations are aborted as far as the underlying provider allows.
- Passing `null` or whitespace‑only strings for query parameters results in an `ArgumentException` before any I/O is attempted.
- The `ProfileBatchAsync` method executes queries with the same degree of parallelism as the underlying data source permits; callers should consider throttling large batches to avoid overwhelming the server.
- Generated suggestions are based on heuristics and may not be applicable in all environments; they should be reviewed before applying to production systems.
- Comparison operations require that both reports correspond to the same query text; otherwise an `InvalidOperationException` is thrown.
