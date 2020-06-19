# CommandLineArguments

`CommandLineArguments` is a plain‑old‑CLR type that represents the set of options supplied to the **sql‑query‑analyzer** console application. Each public property corresponds to a command‑line switch or argument and is populated by the argument‑parsing layer before the analysis engine is invoked.

## API

| Member | Type | Description |
|--------|------|-------------|
| **Query** | `string?` | The raw SQL query text supplied directly on the command line (e.g., `--query "SELECT …"`). If `null`, the tool will look for a query file or other source. |
| **QueryFile** | `string?` | Path to a file that contains the SQL query to be analyzed. Takes precedence over `Query` when both are provided. |
| **OutputFormat** | `string?` | Desired format for the analysis result (e.g., `json`, `xml`, `text`). The value is case‑insensitive; unsupported values cause the caller to throw an `ArgumentException`. |
| **OutputPath** | `string?` | File system path where the formatted output will be written. If `null`, output is written to `stdout`. |
| **DatabaseConnection** | `string?` | Connection string used to validate the query against a live database. Invalid connection strings are not validated at this level; errors surface when the engine attempts to open the connection. |
| **Verbose** | `bool` | Enables detailed logging of the analysis process. When `true`, additional diagnostic messages are emitted to the console or log file. |
| **GenerateReport** | `bool` | Instructs the tool to produce a summary report (e.g., execution statistics, recommendation list) in addition to the primary output. |
| **BatchMode** | `bool` | When `true`, the analyzer processes multiple queries supplied via a directory or manifest file without interactive prompts. |
| **ConfigFile** | `string?` | Path to a JSON/YAML configuration file that can override default settings. The file is parsed before any other arguments; parsing errors raise a `FileNotFoundException` or `InvalidDataException`. |
| **ThreadCount** | `int` | Number of worker threads to use for parallel analysis. Must be greater than zero; otherwise an `ArgumentOutOfRangeException` is thrown. |
| **ShowExecutionPlan** | `bool` | Requests that the tool retrieve and display the execution plan for the supplied query (requires a live database connection). |
| **SqlServerVersion** | `string?` | Target SQL Server version (e.g., `2019`, `2022`). Used for version‑specific rule selection. Invalid or unsupported versions cause an `ArgumentException`. |
| **DryRun** | `bool` | Executes the argument validation and configuration steps without performing actual analysis. Useful for CI pipelines. |
| **ExportSuggestions** | `bool` | When `true`, the analyzer writes suggested index or query rewrites to a separate file. |
| **FilterBySeverity** | `string?` | Filters reported issues by severity level (`Low`, `Medium`, `High`). Case‑insensitive; unknown values are ignored. |
| **MaxResults** | `int?` | Upper limit on the number of issues to report. If `null`, all findings are returned. |
| **EnableCache** | `bool` | Enables caching of intermediate analysis results to improve subsequent runs. |
| **CachePath** | `string?` | Directory used to store cache files when `EnableCache` is `true`. The directory is created if it does not exist. |
| **SlowLogFile** | `string?` | Path to a file where queries exceeding a configurable latency threshold are logged. |
| **SlowLogFormat** | `string` | Format string used when writing entries to `SlowLogFile`. Must be a non‑empty string; otherwise an `ArgumentException` is thrown. |

### General Behaviour

* All string properties are nullable; a `null` value indicates that the corresponding option was not supplied.
* Boolean properties default to `false` unless the command line explicitly sets them.
* No member throws exceptions directly; validation is performed by the consuming code (e.g., the argument parser or the analysis engine) based on the documented constraints.

## Usage

### Example 1 – Simple ad‑hoc query analysis

```csharp
using SqlQueryAnalyzer;

var args = new CommandLineArguments
{
    Query = "SELECT TOP 10 * FROM dbo.Customers;",
    OutputFormat = "json",
    OutputPath = "analysis.json",
    Verbose = true,
    ThreadCount = 4,
    ShowExecutionPlan = true,
    DatabaseConnection = "Server=.;Database=Sales;Trusted_Connection=True;"
};

await Analyzer.RunAsync(args);
```

*The example supplies a raw query, requests JSON output written to a file, enables verbose logging, uses four parallel threads, and asks for the execution plan.*

### Example 2 – Batch processing with configuration and caching

```csharp
using SqlQueryAnalyzer;

var args = new CommandLineArguments
{
    QueryFile = @"C:\Queries\Batch\AllQueries.sql",
    ConfigFile = @"C:\Configs\analyzer-config.yaml",
    OutputFormat = "text",
    OutputPath = @"C:\Reports\BatchReport.txt",
    BatchMode = true,
    EnableCache = true,
    CachePath = @"C:\AnalyzerCache",
    ThreadCount = Environment.ProcessorCount,
    FilterBySeverity = "High",
    MaxResults = 50,
    DryRun = false
};

await Analyzer.RunAsync(args);
```

*This scenario processes a large script file in batch mode, applies settings from a YAML configuration file, enables result caching, and limits the output to the 50 most severe issues.*

## Notes

* **Mutual exclusivity** – `Query` and `QueryFile` are mutually exclusive; if both are non‑null the parser should decide which takes precedence (the implementation currently prefers `QueryFile`).  
* **Thread safety** – `CommandLineArguments` itself is a simple data container with no internal synchronization. It is safe to create and populate an instance on a single thread and then pass it to a background operation. Modifying the same instance concurrently is not thread‑safe.  
* **Validation responsibilities** – The type does not enforce range checks or format validation. Callers must validate `ThreadCount > 0`, non‑empty `SlowLogFormat`, supported `OutputFormat`, etc., before invoking the analysis engine.  
* **Cache directory handling** – When `EnableCache` is true and `CachePath` is `null`, the analyzer falls back to a default temporary directory. Supplying an explicit path that does not exist triggers directory creation; failure to create the directory results in an `IOException`.  
* **Version‑specific rules** – `SqlServerVersion` influences rule selection. If the version string does not match a known version, the engine defaults to the latest supported rule set but logs a warning.  
* **Dry‑run mode** – In `DryRun` mode the analyzer performs all configuration and validation steps, writes any generated reports to the specified locations, and then exits without contacting a database or executing the query. This mode is useful for CI validation pipelines.  
* **Error handling** – All members are simple properties; they never throw. Exceptions arise only from downstream processing (e.g., file I/O, database connection, argument validation). Consumers should wrap `Analyzer.RunAsync` in appropriate try/catch blocks to surface these errors.
