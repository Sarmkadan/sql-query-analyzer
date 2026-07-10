# AnalyzerSettings

The `AnalyzerSettings` class serves as the central configuration container for the SQL Query Analyzer, aggregating settings for database connectivity, analysis rules, caching strategies, performance tuning, and logging behavior. It provides mechanisms to load configurations from persistent storage, validate the current state of settings against runtime requirements, and persist changes back to disk, ensuring a consistent and configurable environment for SQL analysis operations.

## API

### Properties

#### `Database`
*   **Type:** `DatabaseSettings`
*   **Description:** Gets or sets the nested configuration object containing database-specific parameters.
*   **Remarks:** This property encapsulates settings related to the target database schema and metadata retrieval.

#### `Analysis`
*   **Type:** `AnalysisSettings`
*   **Description:** Gets or sets the nested configuration object defining the rules and thresholds for query analysis.
*   **Remarks:** Controls the behavior of specific analyzers such as N+1 detection and index suggestions.

#### `Cache`
*   **Type:** `CacheSettings`
*   **Description:** Gets or sets the nested configuration object managing caching policies for analysis results and execution plans.
*   **Remarks:** Used to optimize performance by reducing redundant database calls during repeated analyses.

#### `Performance`
*   **Type:** `PerformanceSettings`
*   **Description:** Gets or sets the nested configuration object governing resource utilization limits and concurrency settings.
*   **Remarks:** Includes settings for thread pooling and timeout configurations.

#### `Logging`
*   **Type:** `LoggingSettings`
*   **Description:** Gets or sets the nested configuration object specifying log levels, outputs, and verbosity.
*   **Remarks:** Determines the granularity of diagnostic information emitted during the analysis process.

#### `Provider`
*   **Type:** `string`
*   **Description:** Gets or sets the ADO.NET provider name (e.g., `System.Data.SqlClient`, `Npgsql`) used to establish connections.
*   **Remarks:** Must match a registered provider in the runtime environment.

#### `ConnectionString`
*   **Type:** `string`
*   **Description:** Gets or sets the database connection string used by the analyzer to connect to the target instance.
*   **Remarks:** Should contain valid authentication and server details; sensitive data should be handled according to security best practices.

#### `ConnectionPoolSize`
*   **Type:** `int`
*   **Description:** Gets or sets the maximum number of connections allowed in the connection pool.
*   **Remarks:** Values less than 1 may result in validation errors or default to system minimums depending on the provider implementation.

#### `ConnectionTimeoutSeconds`
*   **Type:** `int`
*   **Description:** Gets or sets the time in seconds to wait before terminating a connection attempt.
*   **Remarks:** A value of 0 typically indicates an infinite timeout, subject to provider constraints.

#### `EnableConnectionLogging`
*   **Type:** `bool`
*   **Description:** Gets or sets a value indicating whether to log detailed connection open/close events.
*   **Remarks:** Enabling this may increase log volume significantly in high-concurrency scenarios.

#### `MaxThreads`
*   **Type:** `int`
*   **Description:** Gets or sets the maximum number of concurrent threads the analyzer may utilize for parallel processing.
*   **Remarks:** Setting this too high relative to system resources may degrade overall performance.

#### `DetectNPlusOne`
*   **Type:** `bool`
*   **Description:** Gets or sets a value indicating whether the analyzer should actively detect N+1 query patterns.
*   **Remarks:** Enabling this requires execution plan analysis and may increase processing time per query.

#### `DetectMissingIndexes`
*   **Type:** `bool`
*   **Description:** Gets or sets a value indicating whether the analyzer should suggest missing indexes based on query patterns.
*   **Remarks:** Relies on database statistics and execution plan data; accuracy varies by provider.

#### `DetectJoinIssues`
*   **Type:** `bool`
*   **Description:** Gets or sets a value indicating whether the analyzer should identify potential join inefficiencies or cartesian products.
*   **Remarks:** Useful for detecting logical errors in complex queries.

#### `AnalyzeExecutionPlans`
*   **Type:** `bool`
*   **Description:** Gets or sets a value indicating whether to retrieve and parse actual or estimated execution plans.
*   **Remarks:** This is a resource-intensive operation; disabling it improves speed but reduces analysis depth.

#### `CriticalIssueSensitivity`
*   **Type:** `double`
*   **Description:** Gets or sets the sensitivity threshold (0.0 to 1.0) for flagging issues as critical.
*   **Remarks:** Lower values result in more issues being classified as critical; values outside the 0.0–1.0 range should be validated.

#### `EnableDetailedLogging`
*   **Type:** `bool`
*   **Description:** Gets or sets a value indicating whether to emit verbose debug information to the logging system.
*   **Remarks:** Should generally be disabled in production environments to prevent performance overhead.

### Methods

#### `LoadFromFile`
*   **Signature:** `public static AnalyzerSettings LoadFromFile(string path)`
*   **Description:** Deserializes an `AnalyzerSettings` instance from a JSON or XML file located at the specified path.
*   **Parameters:**
    *   `path` (`string`): The absolute or relative file path to the configuration file.
*   **Returns:** An initialized `AnalyzerSettings` object populated with data from the file.
*   **Exceptions:**
    *   Throws `FileNotFoundException` if the specified path does not exist.
    *   Throws `InvalidOperationException` if the file format is invalid or deserialization fails.

#### `SaveToFile`
*   **Signature:** `public void SaveToFile(string path)`
*   **Description:** Serializes the current instance of `AnalyzerSettings` and writes it to the specified file path.
*   **Parameters:**
    *   `path` (`string`): The destination file path. If the file exists, it will be overwritten.
*   **Returns:** `void`
*   **Exceptions:**
    *   Throws `UnauthorizedAccessException` if the application lacks write permissions for the target directory.
    *   Throws `IOException` if the disk is full or the file is locked by another process.

#### `Validate`
*   **Signature:** `public List<string> Validate()`
*   **Description:** Performs a comprehensive validation of all properties within the settings object to ensure runtime compatibility.
*   **Parameters:** None.
*   **Returns:** A `List<string>` containing error messages for any invalid configurations. If the list is empty, the settings are valid.
*   **Exceptions:** Does not throw exceptions for validation failures; instead, it aggregates error messages into the return list.

## Usage

### Example 1: Loading and Validating Configuration
This example demonstrates loading settings from a standard configuration file, validating them before use, and handling potential configuration errors.

```csharp
using System;
using System.Collections.Generic;
using SqlQueryAnalyzer.Configuration;

public class AnalyzerBootstrapper
{
    public void Initialize(string configPath)
    {
        try
        {
            // Load settings from disk
            var settings = AnalyzerSettings.LoadFromFile(configPath);

            // Validate the loaded configuration
            List<string> errors = settings.Validate();

            if (errors.Count > 0)
            {
                Console.WriteLine("Configuration validation failed:");
                foreach (var error in errors)
                {
                    Console.WriteLine($"- {error}");
                }
                return;
            }

            // Apply settings to the analyzer service
            Console.WriteLine($"Initializing analyzer for provider: {settings.Provider}");
            Console.WriteLine($"Max threads: {settings.MaxThreads}");
            Console.WriteLine($"N+1 Detection: {settings.DetectNPlusOne}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load configuration: {ex.Message}");
        }
    }
}
```

### Example 2: Programmatic Configuration and Persistence
This example shows how to construct an `AnalyzerSettings` object programmatically, adjust specific analysis flags, and persist the new configuration.

```csharp
using System;
using SqlQueryAnalyzer.Configuration;

public class ConfigGenerator
{
    public void CreateStrictAnalysisConfig(string outputPath)
    {
        var settings = new AnalyzerSettings
        {
            Provider = "System.Data.SqlClient",
            ConnectionString = "Server=localhost;Database=TestDB;Trusted_Connection=True;",
            ConnectionPoolSize = 20,
            ConnectionTimeoutSeconds = 30,
            MaxThreads = 4,
            
            // Enable strict analysis rules
            DetectNPlusOne = true,
            DetectMissingIndexes = true,
            DetectJoinIssues = true,
            AnalyzeExecutionPlans = true,
            
            // Set high sensitivity for critical issues
            CriticalIssueSensitivity = 0.85,
            
            // Configure logging
            EnableConnectionLogging = false,
            EnableDetailedLogging = true
        };

        // Initialize nested objects if not auto-initialized by constructor
        if (settings.Database == null) settings.Database = new DatabaseSettings();
        if (settings.Analysis == null) settings.Analysis = new AnalysisSettings();
        if (settings.Cache == null) settings.Cache = new CacheSettings();
        if (settings.Performance == null) settings.Performance = new PerformanceSettings();
        if (settings.Logging == null) settings.Logging = new LoggingSettings();

        // Validate before saving
        var errors = settings.Validate();
        if (errors.Count == 0)
        {
            settings.SaveToFile(outputPath);
            Console.WriteLine($"Configuration saved successfully to {outputPath}");
        }
        else
        {
            Console.WriteLine("Cannot save invalid configuration.");
        }
    }
}
```

## Notes

### Thread Safety
The `AnalyzerSettings` class is not inherently thread-safe for write operations. While reading simple value types (e.g., `bool`, `int`, `string`) is generally atomic on most platforms, modifying properties such as `ConnectionString` or nested objects like `Analysis` while another thread is actively using the settings for an analysis job can lead to inconsistent states or race conditions. It is recommended to treat instances of `AnalyzerSettings` as immutable once passed to an analyzer service, or to utilize external locking mechanisms if dynamic updates are required during runtime.

### Validation Edge Cases
The `Validate` method aggregates errors rather than failing fast. Users should be aware that certain properties have interdependencies. For instance, setting `AnalyzeExecutionPlans` to `true` while `ConnectionTimeoutSeconds` is set to a very low value may result in timeout errors during actual execution, though `Validate` might only check for non-negative integers. Additionally, `CriticalIssueSensitivity` accepts a `double`; values outside the logical range of 0.0 to 1.0 should be explicitly checked by the consumer if the internal validation logic does not enforce strict bounds.

### Resource Management
Properties like `MaxThreads` and `ConnectionPoolSize` directly influence system resource consumption. Setting `MaxThreads` significantly higher than the number of available CPU cores may cause context switching overhead, while a `ConnectionPoolSize` that exceeds the database server's maximum connection limit will result in connection failures. These values should be tuned according to the specific deployment environment's capacity.
