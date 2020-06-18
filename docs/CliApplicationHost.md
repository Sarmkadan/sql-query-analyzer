# CliApplicationHost

A lightweight host for executing SQL queries from the command line, managing lifecycle, parsing arguments, and returning structured analysis results. It encapsulates command-line parsing, query execution orchestration, and result reporting for SQL query analysis tools.

## API

### `public CliApplicationHost`

Initializes a new instance of the command-line application host with default configuration. No parameters are required; all runtime state is derived from `Arguments` after parsing.

### `public async Task<int> RunAsync()`

Starts the host execution pipeline, including argument parsing, query validation, execution, and result reporting. Returns an exit code indicating success or failure.

- **Return value**: `Task<int>` — Exit code: `0` on success, non-zero on error.
- **Exceptions**: Throws `ArgumentException` if required arguments are missing or invalid; throws `InvalidOperationException` if `RunAsync` is called multiple times.

### `public string Query`

Gets the SQL query string to be analyzed. This value is populated after argument parsing from the command-line input.

- **Access**: Read-only after initialization.
- **Source**: Derived from parsed command-line arguments.

### `public CommandLineArguments Arguments`

Gets the parsed command-line arguments provided to the application. Contains raw input such as query text, flags, and configuration options.

- **Access**: Read-only after initialization.
- **Type**: `CommandLineArguments` — A structured representation of command-line input.

### `public QueryAnalysisResult? Result`

Gets the analysis result of the executed query, if available. This value is `null` if no query was executed or if analysis failed.

- **Access**: Read-only after execution.
- **Type**: `QueryAnalysisResult?` — Contains parsed metadata, execution stats, and diagnostics.

### `public bool ShouldContinue`

Indicates whether the host should continue processing after the current operation (e.g., after displaying results or errors). Useful for interactive or batch modes.

- **Access**: Read-write.
- **Default**: `true`.
- **Usage**: Set to `false` to halt further processing (e.g., on fatal errors).

### `public Dictionary<string, object> Metadata`

A mutable dictionary for storing arbitrary key-value metadata associated with the current execution context (e.g., session IDs, timestamps, custom flags).

- **Access**: Read-write.
- **Thread safety**: Not thread-safe; external synchronization required if accessed concurrently.

## Usage
