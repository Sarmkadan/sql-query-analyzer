## AnalysisController

The `AnalysisController` class provides REST API endpoints for SQL query analysis. It exposes three main operations: single query analysis, batch query analysis, and health checks. The controller is designed to work with ASP.NET Core or similar web frameworks and returns standardized API responses with success status, data payloads, and appropriate HTTP status codes.

### Usage Example

```csharp
// Example: Analyzing a single query
var controller = new AnalysisController(analyzerService, logger);

var request = new AnalysisRequest
{
    Query = "SELECT * FROM Users WHERE Id = 1",
    Options = new Dictionary<string, string>
    {
        {"timeout", "30"},
        {"includeExecutionPlan", "true"}
    }
};

var response = await controller.AnalyzeAsync(request);

if (response.Success)
{
    Console.WriteLine($"Analysis completed: {response.Data}");
}
else
{
    Console.WriteLine($"Error: {response.Message}");
}

// Example: Batch analysis
var batchRequest = new BatchAnalysisRequest
{
    Queries = new[]
    {
        "SELECT * FROM Orders WHERE CustomerId = 1",
        "SELECT * FROM Products WHERE CategoryId = 5",
        "SELECT COUNT(*) FROM Users"
    },
    MaxDegreeOfParallelism = 4
};

var batchResponse = await controller.AnalyzeBatchAsync(batchRequest);

// Example: Health check
var healthResponse = await controller.GetHealthAsync();
if (healthResponse.Data?.IsHealthy == true)
{
    Console.WriteLine($"Service version: {healthResponse.Data.Version}");
}
```

### Public Members

- `AnalyzeAsync(AnalysisRequest request)` - Analyzes a single SQL query
- `AnalyzeBatchAsync(BatchAnalysisRequest request)` - Analyzes multiple queries in batch
- `GetHealthAsync()` - Gets health status of the analyzer service
- `ApiResponse<T>` - Generic API response wrapper with properties: Success, Data, Message, StatusCode, Errors, Timestamp
- `HealthStatus` - Health status response with properties: IsHealthy, Message, Version, Timestamp, Details

## Configuration

The application uses the `IOptions` pattern for configuration, supporting JSON files and environment variables. See `appsettings.example.json` for a template.

### SqlQueryAnalyzerOptions

| Section | Description |
| :--- | :--- |
| `Database` | Database connection settings (Provider, ConnectionString, etc.) |
| `Analysis` | Analysis behavior settings (MaxThreads, Detection switches) |
| `Cache` | Caching provider and limits |
| `Performance` | Timeout, rate limiting, and batching settings |
| `Logging` | Logging level, file paths, and rotation settings |

### DatabaseOptions

| Property | Description |
| --- | --- |
| `Provider` | Database provider (SqlServer, PostgreSQL, MySQL) |
| `ConnectionString` | Connection string to database |
| `ConnectionPoolSize` | Connection pool size |
| `ConnectionTimeoutSeconds` | Connection timeout in seconds |
| `EnableConnectionLogging` | Enable connection logging |

### AnalysisOptions

| Property | Description |
| --- | --- |
| `MaxThreads` | Maximum threads for analysis |
| `DetectNPlusOne` | Detect N+1 query patterns |
| `DetectMissingIndexes` | Detect missing indexes |
| `DetectJoinIssues` | Detect join issues |
| `AnalyzeExecutionPlans` | Analyze execution plans |
| `CriticalIssueSensitivity` | Sensitivity for critical issues |
| `EnableDetailedLogging` | Enable detailed logging |
| `IndexSeverity` | Index severity thresholds |
| `IgnorePatterns` | Patterns to ignore |

### CacheOptions

| Property | Description |
| --- | --- |
| `Enabled` | Enable caching |
| `Provider` | Caching provider (InMemory, Redis) |
| `MaxEntries` | Maximum cache entries |
| `MaxSizeBytes` | Maximum cache size in bytes |
| `ExpirationSeconds` | Cache expiration in seconds |
| `RedisConnectionString` | Redis connection string |

### PerformanceOptions

| Property | Description |
| --- | --- |
| `TimeoutSeconds` | Timeout in seconds |
| `MaxQueryLength` | Maximum query length |
| `RateLimitQueriesPerSecond` | Rate limit queries per second |
| `MaxConcurrentAnalysis` | Maximum concurrent analysis |
| `EnableBatching` | Enable batching |
| `BatchSize` | Batch size |

### LoggingOptions

| Property | Description |
| --- | --- |
| `MinimumLevel` | Minimum logging level |
| `ConsoleLogging` | Enable console logging |
| `FileLogging` | Enable file logging |
| `LogFilePath` | Log file path |
| `LogMaxFileSizeBytes` | Maximum log file size in bytes |
| `LogMaxBackupFiles` | Maximum log backup files |

## SqlQueryAnalyzerException

The `SqlQueryAnalyzerException` is the base exception class for all exceptions thrown by the SQL Query Analyzer. It inherits from `System.Exception` and provides two standard constructors for creating exception instances with custom error messages and optional inner exceptions. This exception serves as the foundation for more specific exception types like `AnalysisException`, `InvalidQueryException`, `DatabaseConnectionException`, and others.

### Usage Example

```csharp
try
{
    // Simulate an analysis error
    throw new SqlQueryAnalyzerException("Failed to analyze SQL query due to syntax error");
}
catch (SqlQueryAnalyzerException ex)
{
    Console.WriteLine($"SQL Query Analyzer Error: {ex.Message}");
    // Handle the exception appropriately
}

// Example with inner exception
try
{
    // Some operation that might fail
}
catch (Exception innerEx)
{
    throw new SqlQueryAnalyzerException("Analysis failed during query processing", innerEx);
}
```
