# ISlowQueryLogParser

The `ISlowQueryLogParser` interface defines a contract for parsing slow query logs from different database systems (MySQL, PostgreSQL, SQL Server) and for retrieving the top slow queries from the parsed results. It abstracts away database-specific log formats and provides a unified way to analyze slow query performance across heterogeneous environments.

## API

### `SlowQueryLogParser`
- **Type**: Property (get-only)
- **Returns**: `SlowQueryLogParser` – the underlying parser instance associated with this interface.
- **Description**: Provides access to the concrete `SlowQueryLogParser` object. This property can be used to configure parser settings or to reuse the same parser instance across multiple parse operations.

### `ParseMySqlLogAsync`
- **Signature**: `Task<List<SlowQueryEntry>> ParseMySqlLogAsync(string logFilePath)`
- **Parameters**:  
  - `logFilePath` – The path to the MySQL slow query log file.
- **Returns**: A task that resolves to a list of `SlowQueryEntry` objects parsed from the log.
- **Throws**:  
  - `FileNotFoundException` if the specified file does not exist.  
  - `FormatException` if the log content cannot be parsed according to the MySQL slow query log format.  
  - `IOException` on read errors.

### `ParsePostgreSqlLogAsync`
- **Signature**: `Task<List<SlowQueryEntry>> ParsePostgreSqlLogAsync(string logFilePath)`
- **Parameters**:  
  - `logFilePath` – The path to the PostgreSQL slow query log file.
- **Returns**: A task that resolves to a list of `SlowQueryEntry` objects parsed from the log.
- **Throws**:  
  - `FileNotFoundException` if the file is missing.  
  - `FormatException` if the log content is not valid PostgreSQL slow query format.  
  - `IOException` on read errors.

### `ParseSqlServerLogAsync`
- **Signature**: `Task<List<SlowQueryEntry>> ParseSqlServerLogAsync(string logFilePath)`
- **Parameters**:  
  - `logFilePath` – The path to the SQL Server slow query log file.
- **Returns**: A task that resolves to a list of `SlowQueryEntry` objects parsed from the log.
- **Throws**:  
  - `FileNotFoundException` if the file does not exist.  
  - `FormatException` if the log content does not match the SQL Server slow query log format.  
  - `IOException` on read errors.

### `GetTopSlowQueries`
- **Signature**: `List<SlowQueryEntry> GetTopSlowQueries(int count)`
- **Parameters**:  
  - `count` – The number of top slow queries to retrieve.
- **Returns**: A list of `SlowQueryEntry` objects representing the slowest queries, sorted by execution time in descending order.
- **Throws**:  
  - `ArgumentOutOfRangeException` if `count` is less than 1.

## Usage

### Example 1: Parse a MySQL slow query log and display the top 5 queries

```csharp
ISlowQueryLogParser parser = new SlowQueryLogParser();
List<SlowQueryEntry> entries = await parser.ParseMySqlLogAsync("mysql-slow.log");
List<SlowQueryEntry> top5 = parser.GetTopSlowQueries(5);
foreach (var entry in top5)
{
    Console.WriteLine($"{entry.QueryText} - {entry.DurationMs} ms");
}
```

### Example 2: Parse logs from multiple databases and aggregate results

```csharp
ISlowQueryLogParser parser = new SlowQueryLogParser();
var mysqlEntries = await parser.ParseMySqlLogAsync("mysql-slow.log");
var pgEntries = await parser.ParsePostgreSqlLogAsync("postgresql-slow.log");
var allEntries = mysqlEntries.Concat(pgEntries).ToList();
var top10 = parser.GetTopSlowQueries(10);
// Process top10...
```

## Notes

- **Thread Safety**: The `Parse*LogAsync` methods are safe to call concurrently on different log files. However, `GetTopSlowQueries` depends on the internal state of the parser; calling it while a parse operation is in progress may produce inconsistent results. It is recommended to call `GetTopSlowQueries` only after all asynchronous parse tasks have completed.
- **Edge Cases**: Empty log files return an empty list. Malformed or unrecognized log lines are silently skipped; no exception is thrown for individual malformed entries. The parser expects standard log formats; custom log formats may require preprocessing or additional configuration.
- **Resource Management**: The `SlowQueryLogParser` property exposes the underlying parser instance. If the parser implements `IDisposable`, the caller is responsible for disposing it when no longer needed.
- **Performance**: Parsing large log files is I/O-bound. Using the asynchronous methods prevents blocking the calling thread and improves responsiveness in UI or server applications.
