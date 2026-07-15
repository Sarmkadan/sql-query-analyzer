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

## IAnalysisEventPublisher

The `IAnalysisEventPublisher` interface implements the observer pattern for publishing domain events from the SQL query analysis pipeline. It decouples the analysis logic from side effects such as logging, caching, notifications, and other event-driven operations. Publishers maintain a list of subscribers and asynchronously dispatch events to all registered subscribers.

This interface is typically used to notify external systems about analysis lifecycle events like query start/completion, performance issues, or failures.

### Usage Example

```csharp
// Setup dependency injection (ASP.NET Core example)
services.AddSingleton<IAnalysisEventPublisher, AnalysisEventPublisher>();
services.AddSingleton<IAnalysisEventSubscriber, LoggingEventSubscriber>();
services.AddSingleton<IAnalysisEventSubscriber, NotificationEventSubscriber>();

// In your service class
public class QueryAnalyzerService
{
  private readonly IAnalysisEventPublisher _eventPublisher;

  public QueryAnalyzerService(IAnalysisEventPublisher eventPublisher)
  {
    _eventPublisher = eventPublisher;
  }

  public async Task AnalyzeQueryAsync(string queryId, string query)
  {
    // Publish analysis started event
    var startedEvent = new AnalysisStartedEvent
    {
      QueryId = queryId,
      Query = query,
      Metadata = new Dictionary<string, object>
      {
        {"user", "admin"},
        {"environment", "production"}
      }
    };
    await _eventPublisher.PublishAsync(startedEvent);

    try
    {
      // Perform analysis...
      var completedEvent = new AnalysisCompletedEvent
      {
        QueryId = queryId,
        PerformanceScore = 95.5,
        IssuesFound = 2,
        AnalysisDuration = TimeSpan.FromMilliseconds(150),
        Metadata = new Dictionary<string, object> { {"engine", "sql-server"} }
      };
      await _eventPublisher.PublishAsync(completedEvent);
    }
    catch (Exception ex)
    {
      var failedEvent = new AnalysisFailedEvent
      {
        QueryId = queryId,
        ErrorMessage = ex.Message,
        ExceptionType = ex.GetType().Name,
        Metadata = new Dictionary<string, object> { {"errorType", "timeout"} }
      };
      await _eventPublisher.PublishAsync(failedEvent);
    }
  }
}

// Custom subscriber example
public class CustomEventSubscriber : IAnalysisEventSubscriber
{
  private readonly ILogger<CustomEventSubscriber> _logger;

  public CustomEventSubscriber(ILogger<CustomEventSubscriber> logger)
  {
    _logger = logger;
  }

  public Task OnEventAsync(AnalysisEvent @event)
  {
    if (@event is CriticalIssueDetectedEvent critical)
    {
      _logger.LogCritical($"Critical issue in query {critical.QueryId}: {critical.Description}");
      // Send to monitoring system, etc.
    }
    return Task.CompletedTask;
  }
}
```

### Public Members

- `Subscribe(IAnalysisEventSubscriber subscriber)` - Registers a subscriber to receive all published events
- `Unsubscribe(IAnalysisEventSubscriber subscriber)` - Unregisters a subscriber
- `PublishAsync(AnalysisEvent @event)` - Publishes an event to all subscribers asynchronously

### Common Event Types

- `AnalysisStartedEvent` - Raised when query analysis begins
- `AnalysisCompletedEvent` - Raised when query analysis completes with results
- `CriticalIssueDetectedEvent` - Raised when critical performance issues are detected
- `AnalysisFailedEvent` - Raised when analysis fails with error details

### Event Properties (from AnalysisEvent base class)

- `Timestamp` - When the event occurred (UTC)
- `CorrelationId` - Unique identifier for correlating events
- `Metadata` - Dictionary of additional context data

### Implementation Notes

- Events are published asynchronously to avoid blocking the analysis pipeline
- Subscribers are invoked in parallel using `Task.WhenAll`
- Errors in individual subscribers are logged but don't prevent other subscribers from receiving the event
- The publisher is thread-safe for concurrent subscriptions/unsubscriptions

## QueryPlanExtensions

The `QueryPlanExtensions` class provides extension methods for `QueryPlan` that offer advanced analysis capabilities and utility functions for query performance optimization. These methods help identify expensive operations, detect performance issues, and calculate various cost metrics to assist in query optimization efforts.

### Usage Example

```csharp
// Assume we have a parsed QueryPlan from execution plan XML
var queryPlan = new QueryPlan
{
    PlanId = "Plan123",
    DatabaseName = "SalesDB",
    TotalEstimatedCost = 1250.5,
    TotalEstimatedCpuCost = 850.2,
    TotalEstimatedIoCost = 400.3,
    TotalLogicalReads = 15678,
    TotalElapsedTime = TimeSpan.FromMilliseconds(245)
};

// Initialize the plan with nodes, table accesses, and joins
queryPlan.Initialize();

// Calculate cost percentage of the most expensive node
var mostExpensiveNode = queryPlan.AllNodes.OrderByDescending(n => n.EstimatedCost).First();
double costPercentage = queryPlan.GetCostPercentage(mostExpensiveNode);
Console.WriteLine($"Most expensive node: {costPercentage}% of total cost");

// Get nodes that exceed a threshold (e.g., 100 cost units)
var expensiveNodes = queryPlan.GetNodesAboveThreshold(100);
Console.WriteLine($"Found {expensiveNodes.Count} nodes exceeding threshold");

// Calculate cumulative cost of all operations
double cumulativeCost = queryPlan.CalculateCumulativeCost();
Console.WriteLine($"Cumulative cost: {cumulativeCost}");

// Identify the most expensive table access
var expensiveTableAccess = queryPlan.GetMostExpensiveTableAccess();
if (expensiveTableAccess != null)
{
    Console.WriteLine($"Most expensive table: {expensiveTableAccess.ObjectName} (Cost: {expensiveTableAccess.EstimatedCost})");
}

// Identify the most expensive join operation
var expensiveJoin = queryPlan.GetMostExpensiveJoin();
if (expensiveJoin != null)
{
    Console.WriteLine($"Most expensive join: {expensiveJoin.JoinType} (Cost: {expensiveJoin.EstimatedCost})");
}

// Check for table scans (potential performance issue)
bool hasTableScans = queryPlan.HasTableScans();
Console.WriteLine($"Has table scans: {hasTableScans}");

// Get all filtering nodes (WHERE clauses, etc.)
var filteringNodes = queryPlan.GetFilteringNodes();
Console.WriteLine($"Found {filteringNodes.Count} filtering operations");

// Calculate CPU to I/O cost ratio
double cpuToIoRatio = queryPlan.GetCpuToIoCostRatio();
Console.WriteLine($"CPU/I/O cost ratio: {cpuToIoRatio:F2}");

// Get all sorting operations
var sortingNodes = queryPlan.GetSortingNodes();
Console.WriteLine($"Found {sortingNodes.Count} sorting operations");

// Get a comprehensive performance summary
var performanceSummary = queryPlan.GetPerformanceSummary();
Console.WriteLine($"Plan efficiency: {performanceSummary["hasTableScans"]}");

// Check if the plan is considered efficient
bool isEfficient = queryPlan.IsEfficient(maxTableScans: 1, maxCost: 1000.0);
Console.WriteLine($"Is efficient: {isEfficient}");

// Get all nodes that access a specific table
var usersTableNodes = queryPlan.GetNodesForTable("Users");
Console.WriteLine($"Found {usersTableNodes.Count} nodes accessing Users table");
```

### Public Members

- `GetCostPercentage(PlanNode node)` - Calculates the cost percentage of a specific node relative to total plan cost
- `GetNodesAboveThreshold(double threshold)` - Gets all nodes exceeding a specified cost threshold
- `CalculateCumulativeCost()` - Calculates the cumulative cost of all operations in the plan
- `GetMostExpensiveTableAccess()` - Gets the most expensive table access operation
- `GetMostExpensiveJoin()` - Gets the most expensive join operation
- `HasTableScans()` - Determines if the plan has any table scans (potential performance issue)
- `GetFilteringNodes()` - Gets all nodes that perform data filtering (WHERE clauses, etc.)
- `GetCpuToIoCostRatio()` - Calculates the estimated cost ratio between CPU and I/O operations
- `GetSortingNodes()` - Gets all nodes that perform sorting operations
- `GetPerformanceSummary()` - Gets a summary of plan performance characteristics
- `IsEfficient(int maxTableScans = 2, double maxCost = 1000.0)` - Determines if the plan is considered efficient
- `GetNodesForTable(string tableName)` - Gets all nodes that access a specific table

## QueryNormalizerBenchmarks

The `QueryNormalizerBenchmarks` class provides performance benchmarks for the `QueryNormalizer` utility, measuring the efficiency of SQL query normalization, table name extraction, and column name extraction operations. It uses BenchmarkDotNet to compare different normalization scenarios including simple queries, complex multi-join queries, and queries with embedded string literals.

### Usage Example

```csharp
// Create benchmark instance
var benchmarks = new QueryNormalizerBenchmarks();

// Initialize the normalizer (required before running benchmarks)
benchmarks.Setup();

// Benchmark simple query normalization
var simpleResult = benchmarks.NormalizeSimple();
Console.WriteLine($"Normalized simple query: {simpleResult}");

// Benchmark complex query normalization
var complexResult = benchmarks.NormalizeComplex();
Console.WriteLine($"Normalized complex query: {complexResult}");

// Benchmark query with string literals
var literalResult = benchmarks.NormalizeWithLiterals();
Console.WriteLine($"Normalized query with literals: {literalResult}");

// Benchmark table name extraction
var tableNames = benchmarks.ExtractTableNamesComplex();
Console.WriteLine($"Extracted tables: {string.Join(", ", tableNames)}");

// Benchmark column name extraction
var columnNames = benchmarks.ExtractColumnNamesComplex();
Console.WriteLine($"Extracted columns: {string.Join(", ", columnNames)}");
```

## QueryAnalysisPipelineBenchmarks

The `QueryAnalysisPipelineBenchmarks` class provides performance benchmarks for the complete SQL query parsing and analysis pipeline. It measures the efficiency of query parsing (including type detection and table extraction), SHA-256 hashing, and pattern analysis operations that represent a real-world analysis pass. Benchmarks are grouped by category (`Parse`, `Hash`, `Combined`) and use BenchmarkDotNet for accurate performance measurement.

This benchmark suite is valuable for performance profiling during development, helping identify regressions in query parsing speed or memory usage as the analyzer evolves.

### Usage Example

```csharp
// Create benchmark instance
var benchmarks = new QueryAnalysisPipelineBenchmarks();

// Benchmark simple query parsing
benchmarks.ParseSimpleQuery();

// Benchmark complex multi-join query parsing
benchmarks.ParseComplexQuery();

// Benchmark stored procedure parsing
benchmarks.ParseStoredProcQuery();

// Benchmark query hashing (parse + SHA-256)
var simpleHash = benchmarks.HashSimpleQuery();
var complexHash = benchmarks.HashComplexQuery();

// Benchmark join condition extraction
var joinConditions = benchmarks.ExtractJoinConditions();
Console.WriteLine($"Found {joinConditions.Count} join conditions");

// Benchmark full pattern suite (7 different pattern checks)
var patterns = benchmarks.FullPatternSuite();
```

### Public Members

- `ParseSimpleQuery()` - Parses a simple SELECT query with table extraction
- `ParseComplexQuery()` - Parses a complex 4-JOIN query with full pattern extraction
- `ParseStoredProcQuery()` - Parses a stored procedure with DECLARE, GROUP BY, and HAVING clauses
- `HashSimpleQuery()` - Parses and generates SHA-256 hash for a simple query
- `HashComplexQuery()` - Parses and generates SHA-256 hash for a complex query
- `FullPatternSuite()` - Runs 7 different pattern checks on a complex query (returns tuple of bool/int results)
- `ExtractJoinConditions()` - Extracts join conditions from a multi-join query

## SqlPatternAnalyzerBenchmarks

The `SqlPatternAnalyzerBenchmarks` class provides performance benchmarks for the `SqlPatternAnalyzer` utility, measuring the efficiency of various SQL pattern detection and analysis operations. It uses BenchmarkDotNet to benchmark N+1 query pattern detection, table extraction from complex queries, optimization recommendation generation, and various query analysis metrics like readability scoring and parentheses nesting depth.

This benchmark suite helps identify performance regressions in pattern detection algorithms and provides baseline measurements for query analysis operations.

### Usage Example

```csharp
// Create benchmark instance
var benchmarks = new SqlPatternAnalyzerBenchmarks();

// Initialize the benchmark setup (required before running benchmarks)
benchmarks.Setup();

// Benchmark N+1 pattern detection with repeated queries
bool hasNPlusOne = benchmarks.DetectNPlusOneRepeated();
Console.WriteLine($"N+1 pattern detected: {hasNPlusOne}");

// Benchmark N+1 pattern detection with diverse queries
bool hasNPlusOneDiverse = benchmarks.DetectNPlusOneDiverse();
Console.WriteLine($"N+1 pattern detected (diverse): {hasNPlusOneDiverse}");

// Benchmark table extraction from a problematic query
var problematicTables = benchmarks.ExtractTablesProblematic();
Console.WriteLine($"Extracted tables: {string.Join(", ", problematicTables)}");

// Benchmark table extraction from a nested subquery
var nestedTables = benchmarks.ExtractTablesNested();
Console.WriteLine($"Extracted nested tables: {string.Join(", ", nestedTables)}");

// Benchmark optimization recommendations for clean vs problematic queries
var cleanRecommendations = benchmarks.RecommendationsClean();
var problematicRecommendations = benchmarks.RecommendationsProblematic();
Console.WriteLine($"Clean query has {cleanRecommendations.Count} recommendations");
Console.WriteLine($"Problematic query has {problematicRecommendations.Count} recommendations");

// Benchmark readability scoring and complexity metrics
double readabilityScore = benchmarks.ReadabilityScoreProblematic();
int parenthesesCount = benchmarks.CountParenthesesNested();
Console.WriteLine($"Readability score: {readabilityScore:F2}");
Console.WriteLine($"Parentheses nesting depth: {parenthesesCount}");

// Benchmark function detection and OR condition counting
bool hasFunction = benchmarks.HasFunctionOnColumn();
int orCount = benchmarks.CountOrConditions();
Console.WriteLine($"Has function on column: {hasFunction}");
Console.WriteLine($"OR condition count: {orCount}");
```

### Public Members

- `Setup()` - Initializes benchmark data (required before running benchmarks)
- `DetectNPlusOneRepeated()` - Detects N+1 pattern in 20 repeated queries
- `DetectNPlusOneDiverse()` - Detects N+1 pattern in 6 diverse queries
- `ExtractTablesProblematic()` - Extracts tables from a 2-table implicit JOIN query
- `ExtractTablesNested()` - Extracts tables from a nested subquery
- `RecommendationsClean()` - Generates optimization recommendations for a clean query
- `RecommendationsProblematic()` - Generates optimization recommendations for a problematic query
- `ReadabilityScoreProblematic()` - Calculates readability score for a problematic query
- `CountParenthesesNested()` - Counts parentheses nesting depth in a complex query
- `HasFunctionOnColumn()` - Checks if a query contains a function on a column
- `CountOrConditions()` - Counts the number of OR conditions in a query

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

## QueryProfilerExtensions

The `QueryProfilerExtensions` class provides extension methods for registering the query profiler with the dependency injection container and for querying `QueryProfilerReport` instances to extract performance insights, filter critical issues, and generate comparison reports. These methods enable programmatic analysis of profiler results, batch operations on multiple reports, and environment-aware profiler configuration.

### Usage Example

```csharp
// Setup dependency injection with environment-aware settings
var services = new ServiceCollection();

// Register required analyzer services first
services.AddSingleton<IQueryAnalyzerService, QueryAnalyzerService>();
services.AddSingleton<IQueryPlanAnalyzerService, QueryPlanAnalyzerService>();
services.AddSingleton<IPerformanceIssueDetectorService, PerformanceIssueDetectorService>();

// Add query profiler with environment-specific settings
services.AddQueryProfilerForEnvironment(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development");

var serviceProvider = services.BuildServiceProvider();
var profilerService = serviceProvider.GetRequiredService<IQueryProfilerService>();

// Profile a query
var report = await profilerService.ProfileQueryAsync(
    "SELECT u.Name, COUNT(o.Id) as OrderCount " +
    "FROM Users u LEFT JOIN Orders o ON u.Id = o.UserId " +
    "GROUP BY u.Name HAVING COUNT(o.Id) > 5 " +
    "ORDER BY OrderCount DESC",
    "GetTopCustomers"
);

// Analyze the profiler report
if (report != null)
{
    // Get the bottleneck stage (slowest execution stage)
    var bottleneck = report.GetBottleneckStage();
    Console.WriteLine($"Bottleneck: {bottleneck?.StageName} ({bottleneck?.DurationMs}ms)");
    
    // Get slow stages (exceeding 100ms threshold)
    var slowStages = report.GetSlowStages(thresholdMs: 100);
    Console.WriteLine($"Slow stages: {slowStages.Count}");
    
    // Get critical metrics (CPU usage > 50)
    var criticalMetrics = report.GetCriticalMetrics(threshold: 50);
    Console.WriteLine($"Critical metrics: {criticalMetrics.Count}");
    
    // Get metrics by category
    var timingMetrics = report.GetMetricsByCategory(MetricCategory.Timing);
    var resourceMetrics = report.GetMetricsByCategory(MetricCategory.Resource);
    
    // Get suggestions by severity (Critical or High)
    var highSeveritySuggestions = report.GetSuggestionsBySeverity(SuggestionSeverity.High);
    Console.WriteLine($"High severity suggestions: {highSeveritySuggestions.Count}");
    
    // Get top 3 suggestions by estimated impact
    var topSuggestions = report.GetTopSuggestions(count: 3);
    foreach (var suggestion in topSuggestions)
    {
        Console.WriteLine($"- {suggestion.Description} (Impact: {suggestion.EstimatedImpactPercent}%)");
    }
    
    // Check if optimization is needed (score < 70)
    if (report.NeedsOptimization(threshold: 70))
    {
        Console.WriteLine("Query needs optimization!");
        
        // Export report data for telemetry
        var exportDict = report.ToExportDictionary();
        Console.WriteLine($"Exported {exportDict.Count} metrics");
        
        // Find specific metric
        var cpuMetric = report.FindMetric("Total CPU Time");
        if (cpuMetric != null)
        {
            Console.WriteLine($"CPU Time: {cpuMetric.Value} {cpuMetric.Unit}");
        }
    }
    
    // Batch operations on multiple reports
    var reports = new List<QueryProfilerReport> { report };
    
    // Filter reports with critical suggestions
    var criticalReports = reports.WithCriticalSuggestions();
    Console.WriteLine($"Critical reports: {criticalReports.Count}");
    
    // Filter reports needing optimization
    var optimizationReports = reports.NeedingOptimization(threshold: 70);
    Console.WriteLine($"Reports needing optimization: {optimizationReports.Count}");
    
    // Order reports by worst performance first
    var orderedReports = reports.OrderByWorstFirst();
    Console.WriteLine($"Worst report score: {orderedReports.First().PerformanceScore}");
    
    // Get batch summary statistics
    var batchSummary = reports.GetBatchSummary();
    Console.WriteLine($"Batch summary: {batchSummary}");
}

// Profile comparison between two query versions
var comparison = await profilerService.CompareQueriesAsync(
    "SELECT * FROM Users WHERE Status = 'active'",
    "SELECT * FROM Users WHERE Status = 'active' AND CreatedAt > '2024-01-01'"
);

// Get regressions (metrics that got worse)
var regressions = comparison.GetRegressions();
Console.WriteLine($"Regressions found: {regressions.Count}");

// Get improvements (metrics that got better)
var improvements = comparison.GetImprovements();
Console.WriteLine($"Improvements found: {improvements.Count}");

// Generate markdown comparison table
var markdownTable = comparison.ToMarkdownTable();
Console.WriteLine(markdownTable);
```

### Public Members

- `AddQueryProfiler(IServiceCollection services, ProfilerSettings? settings)` - Registers query profiler services with DI container
- `AddQueryProfilerForEnvironment(IServiceCollection services, string environmentName)` - Registers query profiler with environment-specific settings
- `GetBottleneckStage(this QueryProfilerReport report)` - Returns the slowest execution stage
- `GetSlowStages(this QueryProfilerReport report, double thresholdMs)` - Returns stages exceeding duration threshold
- `GetCriticalMetrics(this QueryProfilerReport report, double threshold)` - Returns metrics exceeding numeric threshold
- `GetMetricsByCategory(this QueryProfilerReport report, MetricCategory category)` - Returns metrics by category
- `GetSuggestionsByCategory(this QueryProfilerReport report, SuggestionCategory category)` - Returns suggestions by category
- `GetSuggestionsBySeverity(this QueryProfilerReport report, SuggestionSeverity minimumSeverity)` - Returns suggestions by severity
- `GetTopSuggestions(this QueryProfilerReport report, int count)` - Returns top suggestions by estimated impact
- `NeedsOptimization(this QueryProfilerReport report, double threshold)` - Checks if optimization is needed
- `ToExportDictionary(this QueryProfilerReport report)` - Exports report data as dictionary
- `FindMetric(this QueryProfilerReport report, string metricName)` - Finds metric by name
- `WithCriticalSuggestions(this IEnumerable<QueryProfilerReport> reports)` - Filters reports with critical suggestions
- `NeedingOptimization(this IEnumerable<QueryProfilerReport> reports, double threshold)` - Filters reports needing optimization
- `OrderByWorstFirst(this IEnumerable<QueryProfilerReport> reports)` - Orders reports by performance score
- `SuccessfulOnly(this IEnumerable<QueryProfilerReport> reports)` - Filters successful reports
- `GetBatchSummary(this IEnumerable<QueryProfilerReport> reports)` - Gets batch summary statistics
- `GetRegressions(this ProfileComparison comparison)` - Gets metric regressions from comparison
- `GetImprovements(this ProfileComparison comparison)` - Gets metric improvements from comparison
- `ToMarkdownTable(this ProfileComparison comparison)` - Generates markdown comparison table

## WebhookNotificationService

The `WebhookNotificationService` class sends webhook notifications for important SQL query analysis events to external systems like Slack, Microsoft Teams, Discord, or custom APIs. It implements the `IAnalysisEventSubscriber` interface to receive analysis events and sends notifications based on webhook configuration settings. The service includes retry logic for failed webhook deliveries and supports filtering notifications by event type (completion, failures, critical issues).

### Usage Example

```csharp
// Setup dependency injection (ASP.NET Core example)
services.AddSingleton<IAnalysisEventPublisher, AnalysisEventPublisher>();
services.AddSingleton<IAnalysisEventSubscriber, WebhookNotificationService>();

// Create and configure webhook notification service
var webhookService = new WebhookNotificationService(logger);

// Register webhook endpoints for different notification types
webhookService.RegisterWebhook(new WebhookConfiguration
{
    Name = "Slack Alerts",
    Url = "https://hooks.slack.com/services/YOUR/SLACK/WEBHOOK",
    Type = WebhookType.Slack,
    NotifyOnCompletion = true,
    NotifyOnCriticalIssues = true,
    NotifyOnFailures = true,
    CustomHeaders = new Dictionary<string, string>
    {
        {"X-Custom-Header", "value"}
    }
});

webhookService.RegisterWebhook(new WebhookConfiguration
{
    Name = "Teams Monitoring",
    Url = "https://your-organization.webhook.office.com/webhookb2/YOUR/TEAMS/WEBHOOK",
    Type = WebhookType.MicrosoftTeams,
    NotifyOnCriticalIssues = true,
    NotifyOnFailures = true
});

// Register webhook for custom API endpoint
webhookService.RegisterWebhook(new WebhookConfiguration
{
    Name = "Custom Analytics",
    Url = "https://api.example.com/webhooks/sql-analyzer",
    Type = WebhookType.Custom,
    NotifyOnCompletion = true,
    NotifyOnFailures = true,
    CustomHeaders = new Dictionary<string, string>
    {
        {"Authorization", "Bearer your-token-here"},
        {"X-API-Key", "your-api-key"}
    }
});

// Get current webhook count
int webhookCount = webhookService.GetWebhookCount();
Console.WriteLine($"Registered webhooks: {webhookCount}");

// Unregister a webhook when no longer needed
webhookService.UnregisterWebhook("Teams Monitoring");

// The service automatically receives events through IAnalysisEventSubscriber interface
// No manual event handling required - webhooks are sent automatically based on configuration
```

### Public Members

- `RegisterWebhook(WebhookConfiguration config)` - Registers a webhook endpoint for notifications
- `UnregisterWebhook(string webhookName)` - Unregisters a webhook by name
- `OnEventAsync(AnalysisEvent @event)` - Handles analysis events and sends relevant webhooks (async)
- `GetWebhookCount()` - Gets count of registered webhooks
- `WebhookConfiguration` - Configuration class with properties:
  - `Name` - Webhook display name
  - `Url` - Webhook endpoint URL
  - `Type` - Webhook type (Slack, MicrosoftTeams, Discord, Custom)
  - `Enabled` - Whether webhook is active
  - `NotifyOnCompletion` - Notify on analysis completion
  - `NotifyOnCriticalIssues` - Notify on critical issues
  - `NotifyOnFailures` - Notify on failures
  - `CustomHeaders` - Optional custom HTTP headers

### Webhook Types

- `Slack` - Slack webhook format
- `MicrosoftTeams` - Microsoft Teams webhook format  
- `Discard` - Discord webhook format
- `Custom` - Generic JSON webhook format

### Event Types Supported

- `CriticalIssueDetectedEvent` - Critical performance issues
- `AnalysisFailedEvent` - Analysis failures with error details
- `AnalysisCompletedEvent` - Successful analysis completion

### Implementation Notes

- Webhooks are sent asynchronously to avoid blocking the analysis pipeline
- Failed webhook deliveries are automatically retried (3 attempts by default)
- The service implements exponential backoff for retry logic
- Custom headers can be added for authentication or additional metadata
- Thread-safe for concurrent webhook registration/unregistration

## ExportService

The `ExportService` class provides centralized export functionality for SQL query analysis results, supporting multiple output formats (JSON, CSV, XML, HTML, text) and batch operations. It handles directory creation, error logging, and can generate comprehensive export packages with summary reports and recommendations.

### Usage Example

```csharp
// Setup dependency injection (ASP.NET Core example)
services.AddSingleton<ExportService>();

// Create export service
var exportService = new ExportService(logger);

// Analyze a query
var analyzer = new QueryAnalyzerService();
var result = await analyzer.AnalyzeAsync("SELECT * FROM Users WHERE CreatedAt > '2024-01-01'");

// Export single analysis to JSON
await exportService.ExportAsync(
    result,
    "./exports/analysis-2024-01-15.json",
    "json"
);

// Export batch of analyses to CSV
var batchResults = new List<QueryAnalysisResult> { result1, result2, result3 };
await exportService.ExportBatchAsync(
    batchResults,
    "./exports/batch-analysis.csv",
    "csv"
);

// Export to multiple formats simultaneously
await exportService.ExportMultipleFormatsAsync(
    result,
    "./exports/multi-format",
    "json", "csv", "html", "xml"
);

// Export with comprehensive report package
await exportService.ExportWithReportAsync(
    result,
    "./exports/full-report-2024-01-15"
);

// Check supported formats
var supportedFormats = exportService.GetSupportedFormats();
Console.WriteLine($"Supported formats: {string.Join(", ", supportedFormats)}");

// Check if specific format is supported
bool isSupported = exportService.IsFormatSupported("json");
Console.WriteLine($"JSON format supported: {isSupported}");
```

### Public Members

- `ExportService(ILogger<ExportService> logger)` - Constructor that initializes default formatters
- `RegisterFormatter(string format, IResultFormatter formatter)` - Registers a custom formatter for additional output formats
- `ExportAsync(QueryAnalysisResult result, string filePath, string format = "json")` - Exports single analysis result to file
- `ExportBatchAsync(List<QueryAnalysisResult> results, string filePath, string format = "json")` - Exports batch of results to file
- `ExportMultipleFormatsAsync(QueryAnalysisResult result, string outputDirectory, params string[] formats)` - Exports results to multiple formats simultaneously
- `ExportWithReportAsync(QueryAnalysisResult result, string outputDirectory)` - Exports analysis with summary report, recommendations, and multiple files
- `GetSupportedFormats()` - Gets list of supported export formats
- `IsFormatSupported(string format)` - Checks if format is supported

### Supported Formats

- `json` - JSON format (default)
- `csv` - CSV format for spreadsheet applications
- `xml` - XML format for system integration
- `html` - HTML format for web viewing
- `text` - Plain text format

### Export Configuration

The service automatically creates output directories if they don't exist and provides detailed logging for all operations. It supports:

- Single file exports
- Batch exports (multiple results to one file)
- Multi-format exports (same data to multiple formats)
- Complete report packages with summary, recommendations, and analysis files

### Implementation Notes

- Thread-safe for concurrent exports
- Comprehensive error handling with detailed logging
- Supports custom formatters via `RegisterFormatter` method
- Generates human-readable summary reports with performance metrics and optimization recommendations

## HttpQueryAnalysisClient

The `HttpQueryAnalysisClient` class is an HTTP client for integrating with remote SQL analyzer instances. It enables distributed analysis, API-first integration, and remote caching scenarios. The client implements retry logic with exponential backoff, connection pooling, and comprehensive error handling for reliable remote communication.

### Usage Example

```csharp
// Create HTTP client for remote analyzer
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<HttpQueryAnalysisClient>();
var client = new HttpQueryAnalysisClient(logger, "http://remote-analyzer:5000", timeoutSeconds: 60);

// Check service health before analysis
bool isHealthy = await client.IsHealthyAsync();
Console.WriteLine($"Remote analyzer is healthy: {isHealthy}");

// Get version information
string? version = await client.GetVersionAsync();
Console.WriteLine($"Remote analyzer version: {version ?? "unknown"}");

// Analyze a single query with retry logic
var singleResult = await client.AnalyzeQueryAsync(
    "SELECT * FROM Users WHERE CreatedAt > '2024-01-01' AND Status = 'active'",
    maxRetries: 5,
    backoffMs: 250
);
Console.WriteLine($"Single query analysis completed: {singleResult.Issues.Count} issues found");

// Analyze batch of queries in parallel
var queries = new[]
{
    "SELECT COUNT(*) FROM Orders WHERE OrderDate > '2024-01-01'",
    "SELECT * FROM Products WHERE Price > 100 ORDER BY Price DESC",
    "SELECT u.Name, COUNT(o.Id) as OrderCount FROM Users u LEFT JOIN Orders o ON u.Id = o.UserId GROUP BY u.Name"
};
var batchResults = await client.AnalyzeBatchAsync(queries);
Console.WriteLine($"Batch analysis completed: {batchResults.Count} queries analyzed");

foreach (var result in batchResults)
{
    Console.WriteLine($"Query: {result.Query.Substring(0, Math.Min(50, result.Query.Length))}...");
    Console.WriteLine($"  - Critical issues: {result.Issues.Count(i => i.Severity == IssueSeverity.Critical)}");
    Console.WriteLine($"  - Performance score: {result.PerformanceScore:F1}");
}
```

### Public Members

- `AnalyzeQueryAsync(string query, int maxRetries = 3, int backoffMs = 500)` - Analyzes a single SQL query with retry logic
- `AnalyzeBatchAsync(string[] queries)` - Analyzes multiple queries in batch for parallel processing
- `IsHealthyAsync()` - Checks health/availability of remote analyzer service
- `GetVersionAsync()` - Gets version information from remote analyzer
- `AnalysisRequest` - Request DTO with properties: Query, Options
- `BatchAnalysisRequest` - Batch request DTO with properties: Queries, MaxDegreeOfParallelism

## SqlPatternAnalyzerTests

The `SqlPatternAnalyzerTests` class provides unit tests for the `SqlPatternAnalyzer` utility, which detects common SQL anti-patterns and calculates query readability scores. It tests pattern detection methods like `HasSelectStar`, `HasLeadingWildcardLike`, `DetectNPlusOnePattern`, and `CalculateReadabilityScore`, as well as optimization recommendation generation. The test suite uses xUnit and FluentAssertions for clear, expressive test assertions.

### Usage Example

```csharp
// Create test service
var tests = new SqlPatternAnalyzerTests();

// Test SELECT * detection
tests.HasSelectStar_QueryContainsStar_ReturnsTrue();
tests.HasSelectStar_QueryWithNamedColumns_ReturnsFalse();

// Test LIKE pattern detection
tests.HasLeadingWildcardLike_PatternStartsWithPercent_ReturnsTrue();

// Test N+1 pattern detection
tests.DetectNPlusOnePattern_SingleQueryInList_ReturnsFalse();
tests.DetectNPlusOnePattern_SameTableAccessedMoreThanFiveTimes_ReturnsTrue();

// Test readability scoring
tests.CalculateReadabilityScore_WellWrittenQuery_ReturnsFullScore();
tests.CalculateReadabilityScore_SelectStarWithImplicitJoin_DeductsThirtyPoints();

// Test optimization recommendations
tests.GenerateOptimizationRecommendations_SelectStarQuery_IncludesColumnReplacementAdvice();
```

### Test Methods

- `HasSelectStar_QueryContainsStar_ReturnsTrue()` - Verifies SELECT * pattern detection
- `HasSelectStar_QueryWithNamedColumns_ReturnsFalse()` - Ensures explicit column names don't trigger false positives
- `HasLeadingWildcardLike_PatternStartsWithPercent_ReturnsTrue()` - Tests LIKE pattern with leading wildcard detection
- `DetectNPlusOnePattern_SingleQueryInList_ReturnsFalse()` - Validates single query returns false
- `DetectNPlusOnePattern_SameTableAccessedMoreThanFiveTimes_ReturnsTrue()` - Tests N+1 pattern detection
- `CalculateReadabilityScore_WellWrittenQuery_ReturnsFullScore()` - Verifies perfect score for clean queries
- `CalculateReadabilityScore_SelectStarWithImplicitJoin_DeductsThirtyPoints()` - Tests readability penalties
- `GenerateOptimizationRecommendations_SelectStarQuery_IncludesColumnReplacementAdvice()` - Validates optimization advice generation

## QueryValidatorTests

The `QueryValidatorTests` class provides unit tests for the `QueryValidator` utility, which validates SQL queries for correctness, safety, and formatting. It tests various scenarios including well-formed queries, empty strings, queries without recognized SQL keywords, null arguments, query sanitization, key generation consistency, and custom validation rules. The test suite uses xUnit and FluentAssertions for clear, expressive test assertions.

### Usage Example

```csharp
// Create validator instance
var validator = new QueryValidator();

// Validate a well-formed SELECT statement
bool isValid = validator.IsValidQuery("SELECT * FROM Users WHERE Id = 1");
Console.WriteLine($"Query is valid: {isValid}"); // True

// Handle empty string input
bool isEmptyValid = validator.IsValidQuery("");
Console.WriteLine($"Empty query is valid: {isEmptyValid}"); // False

// Validate query with no recognized SQL keywords
bool isKeywordValid = validator.IsValidQuery("This is just plain text without SQL");
Console.WriteLine($"Text without keywords is valid: {isKeywordValid}"); // False

// Validate database query with null argument (throws exception)
try
{
    validator.ValidateDatabaseQuery(null!);
}
catch (ValidationException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}

// Sanitize a query for display (truncates long queries)
string longQuery = new string('x', 2000);
string sanitized = validator.SanitizeQueryForDisplay(longQuery);
Console.WriteLine($"Sanitized length: {sanitized.Length}"); // Truncated with ellipsis

// Generate consistent query key (ignores whitespace differences)
string key1 = validator.GenerateQueryKey("SELECT * FROM Users WHERE Id=1");
string key2 = validator.GenerateQueryKey("SELECT * FROM Users WHERE Id = 1");
Console.WriteLine($"Keys are equal: {key1 == key2}"); // True

// Generate result key with proper prefix
string resultKey = validator.GenerateResultKey("SELECT COUNT(*) FROM Users");
Console.WriteLine($"Result key starts with 'result_': {resultKey.StartsWith("result_")}"); // True

// Register and use a custom validation rule
validator.RegisterCustomRule(query => query.Contains("WHERE"), "Query must contain WHERE clause");
validator.ValidateQuery("SELECT * FROM Users WHERE Id = 1", out var errors);
Console.WriteLine($"Validation errors: {errors.Count}"); // 0 - rule satisfied
```

### Usage Example

```csharp
// Create the test service
var service = new QueryPlanAnalyzerService(NullLogger<QueryPlanAnalyzerService>.Instance);

// Test parsing a valid execution plan
var validXmlPlan = @"<?xml version="1.0"?>
<ShowPlanXML>
  <Batch>
    <Statements>
      <StmtSimple StatementText="SELECT * FROM Users" />
    </Statements>
  </Batch>
</ShowPlanXML>";

// Parse the execution plan
var queryPlan = await service.ParseExecutionPlanAsync(validXmlPlan);

if (queryPlan != null)
{
    queryPlan.Initialize();
    
    // Get table scans from the plan
    var tableScans = queryPlan.GetTableScans();
    
    // Get missing index recommendations
    var missingIndexes = await service.GetMissingIndexesAsync(queryPlan);
}

// Test error handling with invalid input
Func<Task> act = async () => await service.ParseExecutionPlanAsync(null!);
act.Should().ThrowAsync<ArgumentException>();
```

### Test Cases

- `AnalyzeQueryPlan_InvalidQueryPlan_ThrowsException` - Validates that invalid query plans throw appropriate exceptions
- `ParseExecutionPlanAsync_ValidXmlPlan_ReturnsQueryPlan` - Tests successful parsing of valid XML execution plans
- `ParseExecutionPlanAsync_InvalidXml_ThrowsQueryPlanException` - Ensures invalid XML throws the correct exception type
- `GetTableScans_WithTableScans_ReturnsTableScans` - Verifies table scan detection functionality
- `GetMissingIndexes_WithTableScans_ReturnsRecommendations` - Tests missing index recommendation generation
