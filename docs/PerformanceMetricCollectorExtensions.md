# PerformanceMetricCollectorExtensions

Provides extension methods for retrieving performance metrics collected during SQL query analysis.

## API

### `GetSuccessfulAnalyses`

Gets the total number of successfully analyzed queries.

```csharp
public static int GetSuccessfulAnalyses(this IPerformanceMetricCollector collector)
```

**Parameters**
- `collector`: The performance metric collector instance.

**Return Value**
- The count of successfully analyzed queries.

**Exceptions**
- Throws `ArgumentNullException` if `collector` is `null`.

---

### `GetFailedAnalyses`

Gets the total number of failed query analyses.

```csharp
public static int GetFailedAnalyses(this IPerformanceMetricCollector collector)
```

**Parameters**
- `collector`: The performance metric collector instance.

**Return Value**
- The count of failed query analyses.

**Exceptions**
- Throws `ArgumentNullException` if `collector` is `null`.

---
### `GetCacheHits`

Gets the total number of cache hits during query analysis.

```csharp
public static int GetCacheHits(this IPerformanceMetricCollector collector)
```

**Parameters**
- `collector`: The performance metric collector instance.

**Return Value**
- The count of cache hits.

**Exceptions**
- Throws `ArgumentNullException` if `collector` is `null`.

---
### `GetCacheMisses`

Gets the total number of cache misses during query analysis.

```csharp
public static int GetCacheMisses(this IPerformanceMetricCollector collector)
```

**Parameters**
- `collector`: The performance metric collector instance.

**Return Value**
- The count of cache misses.

**Exceptions**
- Throws `ArgumentNullException` if `collector` is `null`.

## Usage

```csharp
// Example 1: Retrieving metrics after analyzing queries
var analyzer = new QueryAnalyzer();
var collector = analyzer.GetPerformanceMetrics();

int successful = collector.GetSuccessfulAnalyses();
int failed = collector.GetFailedAnalyses();
int hits = collector.GetCacheHits();
int misses = collector.GetCacheMisses();

Console.WriteLine($"Successful: {successful}, Failed: {failed}, Cache Hits: {hits}, Cache Misses: {misses}");
```

```csharp
// Example 2: Using metrics in a performance monitoring loop
var analyzer = new QueryAnalyzer();
var collector = analyzer.GetPerformanceMetrics();

while (true)
{
    analyzer.AnalyzeBatch(queries);
    var metrics = analyzer.GetPerformanceMetrics();

    Console.WriteLine($"Success Rate: {metrics.GetSuccessfulAnalyses() / (float)metrics.GetTotalAnalyses():P}");
    Thread.Sleep(1000);
}
```

## Notes

- All methods are thread-safe and may be called concurrently from multiple threads.
- Returned values reflect metrics collected up to the point of invocation; subsequent operations may alter these values.
- Metrics are reset when a new `IPerformanceMetricCollector` is obtained via `GetPerformanceMetrics()`.
