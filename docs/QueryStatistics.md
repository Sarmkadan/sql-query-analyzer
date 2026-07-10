# QueryStatistics

The `QueryStatistics` type encapsulates a set of runtime metrics collected for a single SQL query plan within the `sql-query-analyzer` library. It provides detailed information about execution frequency, resource consumption (CPU, I/O, memory), wait statistics, and caching state. Instances are typically produced by the analyzer after monitoring a query over a period of time and are intended to be read-only snapshots of the aggregated data.

## API

The following public members are available on `QueryStatistics`. All properties are read-only.

- **`ExecutionCount`** (`int`)  
  Total number of times the query has been executed since the statistics were last reset or since the query was first observed.

- **`TotalExecutionTime`** (`TimeSpan`)  
  Cumulative wall-clock time spent executing the query across all executions.

- **`MinimumExecutionTime`** (`TimeSpan`)  
  Shortest single execution time observed.

- **`MaximumExecutionTime`** (`TimeSpan`)  
  Longest single execution time observed.

- **`TotalLogicalReads`** (`long`)  
  Total number of logical (cache) page reads performed by all executions.

- **`TotalPhysicalReads`** (`long`)  
  Total number of physical (disk) page reads performed by all executions.

- **`TotalLogicalWrites`** (`long`)  
  Total number of logical page writes performed by all executions.

- **`RowsAffected`** (`int`)  
  Total number of rows affected by the query across all executions (for DML statements; for `SELECT` this is typically zero).

- **`AverageRowsReturned`** (`int`)  
  Average number of rows returned per execution (for `SELECT` statements; for DML this is usually zero).

- **`MaxRowsReturned`** (`int`)  
  Maximum number of rows returned in a single execution.

- **`TotalCpuTime`** (`TimeSpan`)  
  Cumulative CPU time consumed by the query across all executions.

- **`TotalWaitTime`** (`TimeSpan`)  
  Cumulative wait time (time the query spent waiting for resources) across all executions.

- **`MostCommonWaitType`** (`string`)  
  The most frequently occurring wait type encountered during executions (e.g., `"PAGEIOLATCH_SH"`). May be `null` if no waits were recorded.

- **`PeakMemoryUsageMB`** (`int`)  
  Highest memory grant (in megabytes) used by any single execution.

- **`AverageMemoryUsageMB`** (`int`)  
  Average memory grant (in megabytes) used across all executions.

- **`LastCompilationTime`** (`DateTime`)  
  Timestamp of the most recent query plan compilation or recompilation.

- **`IsCached`** (`bool`)  
  Indicates whether the query plan is currently present in the plan cache.

- **`CacheKey`** (`string?`)  
  A unique identifier for the cached plan, or `null` when `IsCached` is `false`.

- **`PlanHandle`** (`int`)  
  An opaque handle representing the compiled plan in the cache. Value is zero when the plan is not cached.

- **`FirstExecution`** (`DateTime`)  
  Timestamp of the first recorded execution of the query.

None of these members throw exceptions under normal usage. However, accessing `CacheKey` or `MostCommonWaitType` may return `null`; callers should check for null before using the value.

## Usage

### Example 1: Enumerating and printing query statistics

```csharp
using SqlQueryAnalyzer;
using System;
using System.Collections.Generic;

public class QueryReporter
{
    public void PrintStatistics(IEnumerable<QueryStatistics> statsCollection)
    {
        foreach (var stats in statsCollection)
        {
            Console.WriteLine($"Query (PlanHandle={stats.PlanHandle}):");
            Console.WriteLine($"  Executions: {stats.ExecutionCount}");
            Console.WriteLine($"  Total CPU: {stats.TotalCpuTime.TotalMilliseconds:F1} ms");
            Console.WriteLine($"  Avg Memory: {stats.AverageMemoryUsageMB} MB");
            Console.WriteLine($"  Cached: {stats.IsCached} (Key: {stats.CacheKey ?? "N/A"})");
            Console.WriteLine($"  First exec: {stats.FirstExecution:O}");
            Console.WriteLine();
        }
    }
}
```

### Example 2: Identifying the most resource-intensive query

```csharp
using SqlQueryAnalyzer;
using System;
using System.Linq;

public class QueryAnalyzer
{
    public QueryStatistics FindMostExpensive(QueryStatistics[] allStats)
    {
        return allStats
            .OrderByDescending(s => s.TotalCpuTime + s.TotalWaitTime)
            .FirstOrDefault();
    }

    public void ReportHighWait(QueryStatistics[] allStats)
    {
        var highWait = allStats
            .Where(s => s.TotalWaitTime.TotalMilliseconds > 1000)
            .OrderByDescending(s => s.TotalWaitTime)
            .Take(5);

        foreach (var stats in highWait)
        {
            Console.WriteLine($"Plan {stats.PlanHandle}: Wait={stats.TotalWaitTime.TotalMilliseconds:F0} ms, " +
                              $"Most common wait: {stats.MostCommonWaitType ?? "none"}");
        }
    }
}
```

## Notes

- **Edge cases**  
  - When `ExecutionCount` is zero, all time‑based and count‑based properties (`TotalExecutionTime`, `AverageRowsReturned`, etc.) will be at their default values (`TimeSpan.Zero`, `0`, etc.).  
  - `MinimumExecutionTime` and `MaximumExecutionTime` are equal to `TotalExecutionTime` when only one execution has occurred.  
  - `RowsAffected` and `AverageRowsReturned` are independent; for `SELECT` queries `RowsAffected` is typically `0`, while for `INSERT`/`UPDATE`/`DELETE` `AverageRowsReturned` is usually `0`.  
  - `CacheKey` is `null` and `PlanHandle` is `0` when `IsCached` is `false`.  
  - `LastCompilationTime` and `FirstExecution` may be `DateTime.MinValue` if the statistics were never populated (e.g., for a newly created instance).  
  - `MostCommonWaitType` may be `null` if no waits were recorded, even if `TotalWaitTime` is non‑zero (the wait type may be unknown or aggregated).

- **Thread safety**  
  Instances of `QueryStatistics` are designed to be immutable after creation. Reading properties from a single instance is safe from multiple threads as long as no thread is writing to the instance. However, the library that produces these objects may update them internally before exposing them; once exposed, the object should not be modified. If the same instance is shared across threads without synchronization, callers should treat it as read‑only. For concurrent access to a collection of statistics, use standard synchronization mechanisms (e.g., `ConcurrentBag<T>` or locking).
