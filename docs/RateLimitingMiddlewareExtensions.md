# RateLimitingMiddlewareExtensions

The `RateLimitingMiddlewareExtensions` class provides a static utility surface for monitoring and interacting with the rate-limiting state within the SQL Query Analyzer pipeline. It exposes diagnostic methods to inspect current system load, retrieve statistics on throttled or active queries, and attempt to acquire execution slots asynchronously. This class is designed to support observability and dynamic flow control without requiring direct instantiation of the underlying middleware components.

## API

### TryAcquireSlotAsync
Attempts to acquire an available execution slot for a query based on the current rate-limiting configuration.
- **Purpose**: Determines if a new query can proceed immediately or if it must be delayed/throttled.
- **Parameters**: None explicitly listed in the signature context (implies reliance on internal middleware state).
- **Return Value**: `Task<bool>` – Returns `true` if a slot was successfully acquired; `false` if the system is at capacity.
- **Throws**: May throw if the internal rate-limiting semaphore or state manager is uninitialized or disposed.

### GetAllQueryStats
Retrieves a comprehensive list of rate-limiting statistics for all tracked queries.
- **Purpose**: Provides a full snapshot of query activity, including counts, intervals, and throttling events.
- **Parameters**: None.
- **Return Value**: `IReadOnlyList<QueryRateLimitStats>` – A read-only collection of statistics objects.
- **Throws**: None under normal operation; may throw `InvalidOperationException` if stats collection is not initialized.

### GetNormalizedLoad
Calculates the current system load as a normalized value.
- **Purpose**: Returns a scalar representing the intensity of current request traffic relative to the configured limit.
- **Parameters**: None.
- **Return Value**: `double` – A value typically between 0.0 (idle) and 1.0 (at limit), though it may exceed 1.0 during burst handling.
- **Throws**: None.

### GetThrottledQueries
Returns statistics specifically for queries that have been rate-limited or throttled.
- **Purpose**: Identifies queries that were delayed or rejected due to exceeding rate limits.
- **Parameters**: None.
- **Return Value**: `IReadOnlyList<QueryRateLimitStats>` – A subset of stats containing only throttled entries.
- **Throws**: None.

### GetMostActiveQueries
Retrieves statistics for the queries with the highest frequency or resource consumption.
- **Purpose**: Helps identify hot paths or potential abuse patterns by listing the most active queries.
- **Parameters**: None.
- **Return Value**: `IReadOnlyList<QueryRateLimitStats>` – A sorted list of the most active query statistics.
- **Throws**: None.

### GetAverageRequestIntervalMs
Computes the average time interval between incoming requests over the sampling window.
- **Purpose**: Measures the cadence of incoming traffic to assist in tuning rate-limit thresholds.
- **Parameters**: None.
- **Return Value**: `double` – The average interval in milliseconds.
- **Throws**: None; returns 0 if no requests have been recorded.

### GetTotalRequests
Returns the cumulative count of requests processed since the middleware started.
- **Purpose**: Provides a simple counter for total throughput.
- **Parameters**: None.
- **Return Value**: `int` – The total number of requests.
- **Throws**: None.

### GetCurrentRequestRate
Calculates the instantaneous rate of incoming requests.
- **Purpose**: Determines the current velocity of traffic (e.g., requests per second).
- **Parameters**: None.
- **Return Value**: `double` – The current request rate.
- **Throws**: None.

### GetSystemStateSummary
Generates a human-readable string summarizing the current health and state of the rate-limiting system.
- **Purpose**: Useful for logging, debugging, or dashboard displays to quickly assess system status.
- **Parameters**: None.
- **Return Value**: `string` – A formatted summary containing key metrics and state flags.
- **Throws**: None.

## Usage

### Example 1: Monitoring System Load and Throttling
This example demonstrates how to periodically check the system load and retrieve details about throttled queries for logging purposes.

```csharp
using System;
using System.Threading.Tasks;
using SqlQueryAnalyzer.RateLimiting;

public class RateLimitMonitor
{
    public async Task MonitorAsync()
    {
        double load = RateLimitingMiddlewareExtensions.GetNormalizedLoad();
        
        if (load > 0.85)
        {
            var throttled = RateLimitingMiddlewareExtensions.GetThrottledQueries();
            Console.WriteLine($"High load detected ({load:P2}). Throttled {throttled.Count} queries.");
            
            foreach (var stat in throttled)
            {
                Console.WriteLine($"- Query ID: {stat.QueryId}, Delay: {stat.ThrottleDelayMs}ms");
            }
        }

        string summary = RateLimitingMiddlewareExtensions.GetSystemStateSummary();
        await Console.Out.WriteLineAsync(summary);
    }
}
```

### Example 2: Attempting to Acquire a Slot Before Execution
This example shows how to use `TryAcquireSlotAsync` to gate expensive query analysis operations based on current capacity.

```csharp
using System;
using System.Threading.Tasks;
using SqlQueryAnalyzer.RateLimiting;

public class QueryExecutor
{
    public async Task ExecuteQuerySafelyAsync(string querySql)
    {
        bool slotAcquired = await RateLimitingMiddlewareExtensions.TryAcquireSlotAsync();

        if (!slotAcquired)
        {
            Console.WriteLine("System at capacity. Request rejected or queued.");
            // Handle rejection: return 503, queue for later, or fail fast
            return;
        }

        try
        {
            // Proceed with expensive analysis
            await PerformAnalysisAsync(querySql);
        }
        finally
        {
            // Note: Slot release logic is typically handled internally by the middleware 
            // upon task completion or via a disposable pattern not exposed in this static extension.
        }
    }

    private Task PerformAnalysisAsync(string sql)
    {
        // Simulation of analysis work
        return Task.Delay(100);
    }
}
```

## Notes

- **Thread Safety**: All methods in `RateLimitingMiddlewareExtensions` are thread-safe and designed to be called concurrently from multiple request threads. The underlying statistics collections are immutable snapshots (`IReadOnlyList`), ensuring that enumeration does not conflict with updates.
- **Initialization State**: These methods rely on the rate-limiting middleware being registered and initialized in the application pipeline. Calling these methods before the middleware is active may result in default values (e.g., 0 counts) or `InvalidOperationException` depending on the specific implementation of the internal state manager.
- **Sampling Windows**: Metrics such as `GetCurrentRequestRate` and `GetAverageRequestIntervalMs` are calculated over a rolling time window. Sudden spikes in traffic may not be immediately reflected until the window slides.
- **Slot Management**: `TryAcquireSlotAsync` returns a boolean indicating immediate availability. It does not block waiting for a slot to become free; callers must implement their own retry or backoff logic if `false` is returned.
- **Data Consistency**: Methods returning lists (e.g., `GetAllQueryStats`) provide a point-in-time snapshot. The data may be slightly stale by the time it is processed if the system is under extremely high churn, but consistency is maintained per call.
