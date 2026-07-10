# SlowQueryEntry

The `SlowQueryEntry` class serves as the primary data transfer object within the `sql-query-analyzer` project, encapsulating the detailed metrics and context of a single slow query event captured from a database log. It aggregates execution statistics, such as duration and row counts, alongside environmental details like the user host and database name, providing a comprehensive snapshot for performance analysis and reporting.

## API

### EntryId
`public string EntryId`
A unique identifier assigned to this specific log entry. This property is used to track, reference, or deduplicate entries during analysis. It is never null for valid entries.

### QueryText
`public string QueryText`
Contains the full text of the SQL statement that was executed. This may include parameter placeholders depending on the logging configuration of the source database.

### Duration
`public TimeSpan Duration`
Represents the total execution time of the query. This value is derived from the difference between the query start and end times recorded in the log source.

### LockTime
`public TimeSpan LockTime`
Indicates the amount of time the query spent waiting for locks. A high value relative to `Duration` suggests contention issues rather than inefficient query planning.

### RowsExamined
`public long RowsExamined`
The total number of rows the database engine scanned or examined to execute the query. High values often indicate missing indexes or full table scans.

### RowsSent
`public long RowsSent`
The number of rows returned to the client. Comparing this with `RowsExamined` helps identify queries that process excessive data to return small result sets.

### Timestamp
`public DateTime Timestamp`
The precise date and time when the query execution commenced, recorded in the local time of the log source or normalized to UTC depending on the parser configuration.

### UserHost
`public string UserHost`
Identifies the client connection origin, typically containing the username and the host IP or hostname from which the query was issued.

### Database
`public string Database`
The name of the database context in which the query was executed.

### LogSource
`public string LogSource`
Specifies the origin of this log entry (e.g., "MySQL Slow Log", "PostgreSQL CSV Log", "Profiler Trace"). This aids in filtering or applying source-specific parsing rules during aggregation.

### Metadata
`public Dictionary<string, string> Metadata`
A collection of key-value pairs containing additional, non-standard attributes extracted from the log entry. This allows the system to preserve source-specific details (such as `Thread_id` or `Schema_version`) without requiring schema changes for every new database feature.

### GetSummary
`public string GetSummary`
A method that generates a concise, human-readable string summarizing the critical aspects of the entry (typically including `EntryId`, `Duration`, and `QueryText` truncated).
*   **Returns**: A formatted string representation.
*   **Throws**: No specific exceptions are thrown under normal operation; returns an empty string if critical data is missing.

## Usage

### Example 1: Filtering and Analysis
The following example demonstrates iterating through a collection of entries to identify queries that exhibit high lock contention relative to their total execution time.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

public class LockContentionAnalyzer
{
    public void Analyze(IEnumerable<SlowQueryEntry> entries)
    {
        var contentiousQueries = entries
            .Where(e => e.Duration > TimeSpan.Zero && 
                        (e.LockTime.TotalMilliseconds / e.Duration.TotalMilliseconds) > 0.5)
            .OrderByDescending(e => e.LockTime);

        foreach (var entry in contentiousQueries)
        {
            Console.WriteLine($"[{entry.EntryId}] High Lock Wait: {entry.LockTime}");
            Console.WriteLine($"Source: {entry.LogSource} | DB: {entry.Database}");
            Console.WriteLine($"User: {entry.UserHost}");
            Console.WriteLine($"Query: {entry.QueryText.Substring(0, Math.Min(100, entry.QueryText.Length))}...");
            Console.WriteLine("---");
        }
    }
}
```

### Example 2: Enriching Metadata and Generating Reports
This example shows how to augment an entry with custom analysis tags and generate a summary for a report.

```csharp
using System;
using System.Collections.Generic;

public class ReportGenerator
{
    public void ProcessEntry(SlowQueryEntry entry)
    {
        // Enrich metadata with analysis flags
        if (entry.RowsExamined > 1000000)
        {
            entry.Metadata["AnalysisFlag"] = "FullScanSuspected";
        }
        
        if (!string.IsNullOrEmpty(entry.Database) && entry.Database.StartsWith("temp_"))
        {
            entry.Metadata["Environment"] = "Temporary";
        }

        // Generate summary for logging or UI display
        string summary = entry.GetSummary;
        
        Console.WriteLine($"Report Entry: {summary}");
        Console.WriteLine($"Attached Metadata Count: {entry.Metadata.Count}");
    }
}
```

## Notes

*   **Thread Safety**: The `SlowQueryEntry` class is not thread-safe. While individual properties are simple types, the `Metadata` dictionary is a mutable reference type. If an instance is shared across multiple threads, external synchronization is required when reading or writing to the `Metadata` dictionary to prevent `InvalidOperationException` during enumeration or data corruption.
*   **Null Handling**: String properties such as `QueryText`, `UserHost`, and `Database` may be empty strings if the information was unavailable in the source log, but they are generally not null unless the object construction failed partially. Consumers should guard against empty strings when performing logical comparisons.
*   **TimeSpan Precision**: The `Duration` and `LockTime` properties rely on the resolution of the underlying database log. Some log sources only provide second-level precision, which may result in `Ticks` values that are multiples of 10,000,000.
*   **Metadata Mutability**: Since `Metadata` is exposed as a concrete `Dictionary<string, string>`, callers can modify its contents directly. Care should be taken not to overwrite keys reserved by the core analyzer (e.g., keys starting with `sys_`).
