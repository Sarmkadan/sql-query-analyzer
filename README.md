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
