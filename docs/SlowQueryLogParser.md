# SlowQueryLogParser

The `SlowQueryLogParser` class turns vendor-specific slow-query logs (MySQL, PostgreSQL, and SQL Server exports) into a unified list of `SlowQueryEntry` objects. It also provides helpers to pick the slowest entries and to group repeated statements by their normalized text, so the results can feed the analysis pipeline.

The class implements [`ISlowQueryLogParser`](ISlowQueryLogParser.md) and is declared `sealed partial`. Its regular expressions are generated at compile time with `[GeneratedRegex]`.

## API

### Constructor

#### `SlowQueryLogParser(ILogger<SlowQueryLogParser> logger)`

Creates a parser instance.

- **Parameters**
  - `logger` (`ILogger<SlowQueryLogParser>`): Receives warnings for skipped blocks and rows, and errors for entries that fail to parse.
- **Exceptions**
  - Throws `ArgumentNullException` if `logger` is `null`.

### `ParseMySqlLogAsync`

Parses the contents of a MySQL slow-query log.

- **Signature**: `Task<List<SlowQueryEntry>> ParseMySqlLogAsync(string logContent)`
- **Parameters**
  - `logContent` (`string`): The full text of the log. Blocks are separated by `# Time:` headers. `\r\n` line endings are normalized.
- **Return Value**
  - `Task<List<SlowQueryEntry>>`: The parsed entries, in log order. Blocks that lack a `# Time:` header or a `# Query_time:` line, or that contain no SQL text, are skipped with a warning.
- **Behavior**
  - `Duration` and `LockTime` are read from `Query_time` and `Lock_time` in seconds.
  - A `use <db>;` line sets `Database`. `#` comment lines and trailing `;` characters are removed from the SQL text.
  - If the timestamp cannot be parsed, `Timestamp` falls back to `DateTime.UtcNow`.
- **Exceptions**
  - Throws `ArgumentNullException` synchronously if `logContent` is `null`.

### `ParsePostgreSqlLogAsync`

Parses PostgreSQL log lines written by `log_min_duration_statement`.

- **Signature**: `Task<List<SlowQueryEntry>> ParsePostgreSqlLogAsync(string logContent)`
- **Parameters**
  - `logContent` (`string`): The log text. Each entry must match the pattern `<timestamp> [pid] user@database LOG:  duration: <n> ms  statement: <sql>`.
- **Return Value**
  - `Task<List<SlowQueryEntry>>`: The parsed entries. Lines that do not match the pattern are ignored.
- **Behavior**
  - `Duration` is read from the `duration` value in milliseconds.
  - `Timestamp` is parsed as UTC. If parsing fails, `DateTime.UtcNow` is used.
  - `LogSource` is set to `"PostgreSQL"`.
- **Exceptions**
  - Throws `ArgumentNullException` synchronously if `logContent` is `null`.

### `ParseSqlServerLogAsync`

Parses a tab-separated SQL Server slow-query export. The first line is treated as a header and is skipped.

- **Signature**: `Task<List<SlowQueryEntry>> ParseSqlServerLogAsync(string logContent)`
- **Parameters**
  - `logContent` (`string`): The export text. Columns are, in order: timestamp, query text, duration in milliseconds, execution count, and average rows.
- **Return Value**
  - `Task<List<SlowQueryEntry>>`: The parsed entries. Rows with fewer than five columns are skipped with a warning.
- **Behavior**
  - `Metadata` receives `execution_count` (column 4) and `avg_rows` (column 5).
  - `RowsSent` and `RowsExamined` are both taken from column 5 (`avg_rows`). The export has no separate row counts.
  - `LogSource` is set to `"SqlServer"`.
- **Exceptions**
  - Throws `ArgumentNullException` synchronously if `logContent` is `null`.

### `GetTopSlowQueries`

Returns the slowest entries, optionally filtered by a minimum duration.

- **Signature**: `List<SlowQueryEntry> GetTopSlowQueries(List<SlowQueryEntry> entries, int topN = 10, TimeSpan? minDuration = null)`
- **Parameters**
  - `entries` (`List<SlowQueryEntry>`): The entries to rank. The list is not modified.
  - `topN` (`int`, optional): The maximum number of entries to return. Defaults to `10`.
  - `minDuration` (`TimeSpan?`, optional): Entries shorter than this value are excluded. Defaults to `TimeSpan.Zero`, which keeps all entries.
- **Return Value**
  - `List<SlowQueryEntry>`: Entries sorted by `Duration`, longest first, with at most `topN` items.
- **Exceptions**
  - Throws `ArgumentNullException` if `entries` is `null`.
  - Throws `ArgumentOutOfRangeException` if `topN` is zero or negative.

### `GetAggregatedByNormalizedQuery`

Groups entries whose SQL text is the same after normalization.

- **Signature**: `List<SlowQueryEntry> GetAggregatedByNormalizedQuery(List<SlowQueryEntry> entries)`
- **Parameters**
  - `entries` (`List<SlowQueryEntry>`): The entries to group. The list is not modified.
- **Return Value**
  - `List<SlowQueryEntry>`: One entry per group, sorted by total `Duration`, longest first.
- **Normalization**
  - The text is lowercased. Quoted string literals (`'...'` and `"..."`) and numeric literals are replaced with `?`. Runs of whitespace are collapsed to one space.
  - For example, `SELECT * FROM t WHERE id = 42` and `select * from t where id = 7` fall into the same group.
- **Result Details**
  - `Duration`, `LockTime`, `RowsExamined`, and `RowsSent` are summed over the group.
  - `QueryText`, `Timestamp`, `UserHost`, `Database`, `LogSource`, and `EntryId` come from the first entry in the group.
  - `Metadata` is copied from the first entry and adds `occurrence_count` (number of entries in the group) and `max_duration_ms` (longest single execution, in milliseconds).
- **Exceptions**
  - Throws `ArgumentNullException` if `entries` is `null`.

## Usage Example

Parsing a single MySQL slow-query log entry and listing the slowest statements:

```csharp
using Microsoft.Extensions.Logging.Abstractions;
using SqlQueryAnalyzer.Services;

var parser = new SlowQueryLogParser(NullLogger<SlowQueryLogParser>.Instance);

const string log = """
    # Time: 2026-10-30T10:15:00.123456Z
    # User@Host: app[app] @ localhost []
    # Query_time: 2.500000  Lock_time: 0.000100 Rows_sent: 5  Rows_examined: 150000
    use shop;
    SELECT * FROM orders WHERE status = 'pending';
    """;

var entries = await parser.ParseMySqlLogAsync(log);
var slowest = parser.GetTopSlowQueries(entries, topN: 5, minDuration: TimeSpan.FromSeconds(1));

foreach (var entry in slowest)
{
    Console.WriteLine(entry.GetSummary());
    Console.WriteLine($"  database: {entry.Database}, full scan: {entry.IsFullScan}");
}
```

The parsed entry has these values:

| Property | Value |
|---|---|
| `Timestamp` | `2026-10-30 10:15:00.123456` (UTC) |
| `UserHost` | `app[app] @ localhost []` |
| `Duration` | `2.5 s` |
| `LockTime` | `0.0001 s` |
| `RowsSent` / `RowsExamined` | `5` / `150000` |
| `Database` | `shop` |
| `QueryText` | `SELECT * FROM orders WHERE status = 'pending'` |
| `LogSource` | `MySQL` |

`GetSummary()` produces a line like:

```
[MySQL] 2500ms | rows: 5/150000 | SELECT * FROM orders WHERE status = 'pending'
```

`IsFullScan` is `true` here, because the efficiency ratio is `5 / 150000`, which is below `0.01`.
