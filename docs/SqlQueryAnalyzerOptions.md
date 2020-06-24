# SqlQueryAnalyzerOptions

`SqlQueryAnalyzerOptions` is the top-level configuration class for the SQL Query Analyzer engine. It aggregates all tunable settings—database connectivity, analysis sensitivity, caching, performance constraints, logging verbosity, and index severity thresholds—into a single object that governs how queries are inspected, which anti-patterns are flagged, and how results are reported.

## API

### DatabaseOptions Database
Contains the database-level configuration such as default schema, catalog, and dialect-specific settings. This object is consulted when establishing connections and interpreting query metadata.

### AnalysisOptions Analysis
Holds the core analysis parameters, including which rule sets are active and any custom rule definitions. The analyzer reads this to determine the scope of inspection.

### CacheOptions Cache
Defines caching behavior for execution plans, parsed ASTs, and analysis results. Controls cache duration, storage limits, and invalidation policies.

### PerformanceOptions Performance
Specifies performance-related thresholds and limits, such as maximum allowed query cost estimates and acceptable execution-time ranges before a warning is raised.

### LoggingOptions Logging
Configures logging sinks, minimum severity levels, and structured logging format options used throughout the analysis pipeline.

### string Provider
The ADO.NET provider invariant name (e.g., `"System.Data.SqlClient"`, `"Npgsql"`). Determines which driver is used to connect and how execution plans are retrieved.

### string ConnectionString
The full connection string passed to the provider when opening database connections. May include credentials, server address, and database name.

### int ConnectionPoolSize
Maximum number of concurrent database connections maintained in the pool. When exceeded, additional connection requests block until a connection is released.

### int ConnectionTimeoutSeconds
Time in seconds the analyzer waits for a connection to be established before throwing a timeout exception. Applies to both initial connections and pool retrieval.

### bool EnableConnectionLogging
When `true`, every connection open, close, and pool-related event is emitted to the configured logging sinks. Useful for diagnosing connectivity issues.

### int MaxThreads
Upper bound on the number of worker threads used for parallel query analysis. The scheduler will not spin up more than this number of concurrent tasks.

### bool DetectNPlusOne
Enables or disables the N+1 query detection rule. When `true`, the analyzer traces ORM-generated queries and flags repeated single-entity fetches that could be batched.

### bool DetectMissingIndexes
Enables or disables the missing-index analysis. When `true`, execution plans are scanned for index recommendations and reported as warnings.

### bool DetectJoinIssues
Enables or disables join-related inspections, including cartesian products, missing join conditions, and inefficient join algorithms.

### bool AnalyzeExecutionPlans
When `true`, the analyzer retrieves and inspects actual or estimated execution plans for submitted queries. Disabling this skips plan retrieval entirely.

### double CriticalIssueSensitivity
A multiplier between `0.0` and `1.0` (inclusive) that adjusts the threshold at which an issue is classified as critical. Lower values make the analyzer more sensitive, flagging more issues as critical.

### bool EnableDetailedLogging
When `true`, the analyzer emits verbose diagnostic information including AST dumps, plan XML, and intermediate rule evaluation states.

### IndexSeverityThresholdsOptions IndexSeverity
Defines the cost and impact thresholds that determine whether a missing-index recommendation is reported as `Info`, `Warning`, or `Critical`.

### List\<string\> IgnorePatterns
A collection of regular expression patterns. Any query whose text matches one of these patterns is excluded from analysis and reporting.

### bool Enabled
Master switch for the analyzer. When `false`, all analysis is bypassed regardless of other settings. Queries pass through uninspected.

## Usage

### Example 1: Basic Configuration for a SQL Server Environment

```csharp
var options = new SqlQueryAnalyzerOptions
{
    Provider = "System.Data.SqlClient",
    ConnectionString = "Server=.;Database=SalesDb;Integrated Security=true;",
    ConnectionPoolSize = 10,
    ConnectionTimeoutSeconds = 30,
    Enabled = true,
    DetectNPlusOne = true,
    DetectMissingIndexes = true,
    DetectJoinIssues = true,
    AnalyzeExecutionPlans = true,
    CriticalIssueSensitivity = 0.7,
    IgnorePatterns = new List<string> { @"^EXEC\s+sp_", @"^PRINT\s" }
};

options.IndexSeverity = new IndexSeverityThresholdsOptions
{
    WarningCostThreshold = 50.0,
    CriticalCostThreshold = 200.0
};
```

### Example 2: Lightweight Analysis with Detailed Logging Disabled

```csharp
var options = new SqlQueryAnalyzerOptions
{
    Provider = "Npgsql",
    ConnectionString = "Host=pg-server;Database=analytics;Username=analyzer;Password=***",
    ConnectionPoolSize = 4,
    MaxThreads = 2,
    Enabled = true,
    DetectNPlusOne = false,
    DetectMissingIndexes = true,
    DetectJoinIssues = false,
    AnalyzeExecutionPlans = false,
    EnableDetailedLogging = false,
    EnableConnectionLogging = false,
    CriticalIssueSensitivity = 0.9,
    IgnorePatterns = new List<string> { @"SELECT\s+1" }
};
```

## Notes

- Setting `Enabled` to `false` short-circuits all analysis; no connections are opened, no plans are retrieved, and no rules are evaluated. This takes precedence over every other setting.
- `CriticalIssueSensitivity` values outside `[0.0, 1.0]` are clamped internally. A value of `0.0` classifies nearly every detected issue as critical; `1.0` requires extremely high confidence before assigning the critical label.
- `IgnorePatterns` are evaluated against the raw query text before parsing. Patterns are case-sensitive by default and must be valid .NET regular expressions. An invalid pattern will cause an `ArgumentException` when the analyzer first attempts to match against it.
- `MaxThreads` interacts with `ConnectionPoolSize`: if `MaxThreads` exceeds the pool size, threads may block waiting for connections, potentially causing timeouts governed by `ConnectionTimeoutSeconds`.
- When `AnalyzeExecutionPlans` is `false`, `DetectMissingIndexes` has no effect because index recommendations are derived from plan inspection.
- `SqlQueryAnalyzerOptions` is not thread-safe by itself. If multiple threads mutate the options object while an analysis is in progress, behavior is undefined. Configure the object once before passing it to the analyzer and treat it as immutable thereafter.
- `ConnectionString` is stored as plain text. Avoid logging it when `EnableConnectionLogging` or `EnableDetailedLogging` is active unless a sanitized logging layer is in place.
