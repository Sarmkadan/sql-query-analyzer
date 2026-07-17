# CommandLineArgumentsExtensions

Provides a centralized, read-only facade over parsed command-line arguments and environment-driven configuration for the SQL Query Analyzer tool. All members are static and expose validated, normalized, or fallback-resolved values derived from the application's startup arguments and environment, ensuring consistent interpretation of user intent across the analysis pipeline.

## API

### ShouldWriteToFile
```csharp
public static bool ShouldWriteToFile
```
Indicates whether the application should persist output to a file rather than (or in addition to) standard streams. Returns `true` when a file-output flag was explicitly set in the command-line arguments; otherwise `false`.

### GetOutputFilePathWithExtension
```csharp
public static string? GetOutputFilePathWithExtension
```
Returns the fully resolved output file path, including the appropriate file extension based on the requested output format (e.g., `.json`, `.csv`). Returns `null` when no output file path was specified or when the path cannot be resolved. The caller must handle the `null` case when `ShouldWriteToFile` is `true`.

### IsVerboseEnabled
```csharp
public static bool IsVerboseEnabled
```
Returns `true` if verbose logging or detailed console output was requested via the `--verbose` flag. When `false`, only essential output should be produced.

### GetEffectiveConnectionString
```csharp
public static string? GetEffectiveConnectionString
```
Resolves the SQL Server connection string using the following precedence: an explicitly provided connection string argument, a named connection string from configuration, or an environment variable. Returns `null` if no connection string can be determined. Callers must validate the result before attempting a database connection.

### IsCacheEnabled
```csharp
public static bool IsCacheEnabled
```
Returns `true` when query analysis result caching is active. Caching is enabled by default and can be disabled via a command-line flag. When `false`, every analysis request results in a fresh execution against the target database.

### GetNormalizedSqlServerVersion
```csharp
public static string GetNormalizedSqlServerVersion
```
Returns the target SQL Server version as a normalized string (e.g., `"2019"`, `"2022"`). If no version is explicitly specified, a default version is returned. The value is guaranteed non-null and is suitable for use in compatibility checks and feature gating.

### GetNormalizedSeverityFilter
```csharp
public static IReadOnlyList<string> GetNormalizedSeverityFilter
```
Returns a read-only list of severity levels (e.g., `"Error"`, `"Warning"`, `"Information"`) that should be included in the analysis output. When no filter is specified, all severities are included. The returned list is never `null` and is safe for enumeration.

### ShouldAnalyzeExecutionPlan
```csharp
public static bool ShouldAnalyzeExecutionPlan
```
Returns `true` when the command-line arguments request analysis of the query execution plan in addition to the query text. When `false`, execution plan analysis is skipped, which can improve performance for large batches.

### GetEffectiveMaxResults
```csharp
public static int? GetEffectiveMaxResults
```
Returns the maximum number of analysis results to produce, or `null` if no limit is imposed. The value is resolved from the `--max-results` argument, falling back to a default if the argument is absent. A `null` return means "unlimited."

### ShouldExportSuggestions
```csharp
public static bool ShouldExportSuggestions
```
Returns `true` when the tool should export actionable index or query rewrite suggestions in a structured format. When `false`, suggestions are either omitted or presented only in human-readable form.

## Usage

### Example 1: Configuring a full analysis run with output to file
```csharp
if (CommandLineArgumentsExtensions.ShouldWriteToFile)
{
    string? outputPath = CommandLineArgumentsExtensions.GetOutputFilePathWithExtension;
    if (outputPath is null)
    {
        Console.Error.WriteLine("Output file path could not be resolved.");
        return;
    }

    bool verbose = CommandLineArgumentsExtensions.IsVerboseEnabled;
    string sqlVersion = CommandLineArgumentsExtensions.GetNormalizedSqlServerVersion;
    IReadOnlyList<string> severities = CommandLineArgumentsExtensions.GetNormalizedSeverityFilter;

    Console.WriteLine($"Analyzing for SQL Server {sqlVersion}...");
    // Proceed with analysis, writing results to outputPath
}
```

### Example 2: Conditional execution plan analysis with caching and limits
```csharp
string? connectionString = CommandLineArgumentsExtensions.GetEffectiveConnectionString;
if (connectionString is null)
{
    throw new InvalidOperationException("No connection string available.");
}

bool analyzePlan = CommandLineArgumentsExtensions.ShouldAnalyzeExecutionPlan;
bool cacheEnabled = CommandLineArgumentsExtensions.IsCacheEnabled;
int? maxResults = CommandLineArgumentsExtensions.GetEffectiveMaxResults;

var options = new AnalysisOptions
{
    ConnectionString = connectionString,
    AnalyzeExecutionPlan = analyzePlan,
    UseCache = cacheEnabled,
    MaxResults = maxResults,
    ExportSuggestions = CommandLineArgumentsExtensions.ShouldExportSuggestions
};

await analyzer.RunAsync(options);
```

## Notes

- All members are static and read the state from a single, immutable snapshot of parsed arguments. They do not reflect changes made after initial parsing.
- `GetOutputFilePathWithExtension` may return `null` even when `ShouldWriteToFile` is `true` if the path argument was malformed or unresolvable; always check for `null`.
- `GetEffectiveConnectionString` returns `null` when no connection string is provided via any source. Callers must handle this gracefully, typically by aborting with a clear error message.
- `GetNormalizedSeverityFilter` returns an empty list when no filter is specified, meaning "include all severities." Do not assume a non-empty list.
- `GetNormalizedSqlServerVersion` never returns `null`; it falls back to a default version defined in the application configuration.
- `GetEffectiveMaxResults` returns `null` to indicate "no limit." Callers should treat `null` as unbounded and avoid applying a default numeric cap.
- All members are thread-safe because they read from an immutable backing store initialized once at startup. No synchronization is required.
- The extension methods are designed to be consumed early in the pipeline (e.g., during host configuration or service registration) and should not be invoked repeatedly in hot paths; cache the values locally if needed.
