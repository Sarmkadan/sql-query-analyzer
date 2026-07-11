# SqlQueryAnalyzerOptionsExtensions

Extension methods for `SqlQueryAnalyzerOptions` that provide utility functions for analyzing SQL queries.

## API

### `IsValid`
Determines whether the provided `SqlQueryAnalyzerOptions` instance is valid for use with the analyzer.
- **Returns**: `true` if the options are valid; otherwise, `false`.
- **Throws**: Does not throw exceptions.

### `IsAnalyzerEnabled`
Checks whether query analysis is enabled in the provided options.
- **Returns**: `true` if analysis is enabled; otherwise, `false`.
- **Throws**: Does not throw exceptions.

### `GetNormalizedProvider`
Retrieves the normalized database provider name from the options.
- **Returns**: A `string` representing the normalized provider name (e.g., "sqlserver", "postgresql").
- **Throws**: Does not throw exceptions.

### `HasCriticalAnalysisEnabled`
Determines whether critical analysis features are enabled in the options.
- **Returns**: `true` if critical analysis is enabled; otherwise, `false`.
- **Throws**: Does not throw exceptions.

### `GetConnectionTimeoutMs`
Gets the connection timeout value in milliseconds from the options.
- **Returns**: An `int` representing the timeout in milliseconds.
- **Throws**: Does not throw exceptions.

### `GetMaxConcurrentThreads`
Retrieves the maximum number of concurrent threads allowed for analysis.
- **Returns**: An `int` representing the maximum concurrent threads.
- **Throws**: Does not throw exceptions.

### `ShouldEnableDetailedLogging`
Determines whether detailed logging is enabled in the options.
- **Returns**: `true` if detailed logging is enabled; otherwise, `false`.
- **Throws**: Does not throw exceptions.

### `GetIgnorePatterns`
Gets the list of patterns to ignore during analysis.
- **Returns**: An `IReadOnlyList<string>` of ignore patterns.
- **Throws**: Does not throw exceptions.

### `ShouldAnalyzeExecutionPlans`
Determines whether execution plan analysis is enabled in the options.
- **Returns**: `true` if execution plan analysis is enabled; otherwise, `false`.
- **Throws**: Does not throw exceptions.

### `GetMaxQueryLength`
Retrieves the maximum allowed query length for analysis.
- **Returns**: An `int` representing the maximum query length.
- **Throws**: Does not throw exceptions.

## Usage
