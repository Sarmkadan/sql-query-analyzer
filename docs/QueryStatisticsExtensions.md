# QueryStatisticsExtensions

The `QueryStatisticsExtensions` class provides a set of static extension methods that operate on a `QueryStatistics` instance. These methods offer derived metrics, formatted strings, and diagnostic indicators commonly used in SQL query performance analysis. They are designed to be stateless and do not modify the input object.

## API

All methods are extension methods on `QueryStatistics`. The first parameter (`this QueryStatistics statistics`) is implicit and represents the query statistics data to analyze. Unless otherwise noted, each method throws an `ArgumentNullException` if `statistics` is `null`.

### `GetAverageLogicalReadsFormatted`
- **Purpose**: Returns the average number of logical reads per execution, formatted as a human-readable string (e.g., `"1,234.56"`).
- **Parameters**: `QueryStatistics statistics`
- **Returns**: `string` – the formatted average logical reads value.
- **Throws**: `ArgumentNullException` if `statistics` is `null`.

### `GetLogicalReadsPerSecond`
- **Purpose**: Calculates the rate of logical reads per second based on the total logical reads and the query’s elapsed time.
- **Parameters**: `QueryStatistics statistics`
- **Returns**: `double` – logical reads per second. Returns `0` if elapsed time is zero or negative.
- **Throws**: `ArgumentNullException` if `statistics` is `null`.

### `GetCpuTimePerLogicalRead`
- **Purpose**: Computes the average CPU time (in microseconds) consumed per logical read.
- **Parameters**: `QueryStatistics statistics`
- **Returns**: `double` – CPU time per logical read. Returns `0` if logical reads is zero.
- **Throws**: `ArgumentNullException` if `statistics` is `null`.

### `GetPerformanceMetrics`
- **Purpose**: Returns a collection of key-value pairs representing common performance metrics (e.g., average logical reads, CPU time, elapsed time, logical reads per second). The keys are descriptive strings and the values are formatted strings.
- **Parameters**: `QueryStatistics statistics`
- **Returns**: `IReadOnlyList<KeyValuePair<string, string>>` – an ordered list of metric pairs.
- **Throws**: `ArgumentNullException` if `statistics` is `null`.

### `HasPotentialParameterSniffing`
- **Purpose**: Heuristically determines whether the query statistics indicate possible parameter sniffing issues. The heuristic compares execution count, average logical reads, and variance in plan choices.
- **Parameters**: `QueryStatistics statistics`
- **Returns**: `bool` – `true` if parameter sniffing is suspected; otherwise `false`.
- **Throws**: `ArgumentNullException` if `statistics` is `null`.

### `GetPerformanceTrendIndicator`
- **Purpose**: Returns a short textual indicator (e.g., `"Stable"`, `"Degrading"`, `"Improving"`) that summarizes the trend of key metrics over the last few executions.
- **Parameters**: `QueryStatistics statistics`
- **Returns**: `string` – the trend indicator.
- **Throws**: `ArgumentNullException` if `statistics` is `null`.

### `GetTotalIoCost`
- **Purpose**: Estimates the total I/O cost of the query by combining logical reads and physical reads using a weighted formula.
- **Parameters**: `QueryStatistics statistics`
- **Returns**: `double` – the estimated total I/O cost.
- **Throws**: `ArgumentNullException` if `statistics` is `null`.

### `GetBottleneckSummary`
- **Purpose**: Analyzes the query statistics and returns a human-readable summary identifying the most likely performance bottleneck (e.g., CPU-bound, I/O-bound, or memory-bound).
- **Parameters**: `QueryStatistics statistics`
- **Returns**: `string` – a description of the primary bottleneck.
- **Throws**: `ArgumentNullException` if `statistics` is `null`.

## Usage

### Example 1: Basic metrics and sniffing detection

```csharp
using SqlQueryAnalyzer;

QueryStatistics stats = QueryStatistics.LoadFromPlan("...");

string avgReads = stats.GetAverageLogicalReadsFormatted();
double readsPerSec = stats.GetLogicalReadsPerSecond();
bool sniffing = stats.HasPotentialParameterSniffing();

Console.WriteLine($"Average logical reads: {avgReads}");
Console.WriteLine($"Logical reads/sec: {readsPerSec:F2}");
Console.WriteLine($"Parameter sniffing suspected: {sniffing}");
```

### Example 2: Performance report generation

```csharp
using SqlQueryAnalyzer;

QueryStatistics stats = QueryStatistics.LoadFromPlan("...");

var metrics = stats.GetPerformanceMetrics();
string bottleneck = stats.GetBottleneckSummary();
string trend = stats.GetPerformanceTrendIndicator();

Console.WriteLine("Performance Metrics:");
foreach (var kvp in metrics)
{
    Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
}
Console.WriteLine($"Bottleneck: {bottleneck}");
Console.WriteLine($"Trend: {trend}");
```

## Notes

- **Null input**: All methods throw `ArgumentNullException` if the `statistics` argument is `null`. Always ensure the input is non-null before calling these extensions.
- **Division by zero**: `GetLogicalReadsPerSecond` and `GetCpuTimePerLogicalRead` handle zero denominators gracefully by returning `0`. No exception is thrown in these cases.
- **Heuristic nature**: `HasPotentialParameterSniffing` and `GetBottleneckSummary` are based on heuristics and may produce false positives or negatives. They are intended for diagnostic guidance, not definitive analysis.
- **Thread safety**: These extension methods are stateless and read-only with respect to the input object. They are safe to call concurrently from multiple threads as long as the `QueryStatistics` instance is not being mutated simultaneously. The methods themselves do not modify any shared state.
