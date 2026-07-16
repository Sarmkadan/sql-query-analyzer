# SQL Query Analyzer

Detects common SQL performance anti-patterns - SELECT *, missing WHERE/LIMIT,
implicit (cartesian) joins, non-sargable predicates, and N+1 access - and emits
optimization recommendations with a performance score.

New here? Start with the focused **[Quickstart & Rule Catalog](docs/QUICKSTART.md)**
for install, CLI/library usage, the full rule list, and example output. The
sections below are the complete API reference.

## Architecture

The tool is a single .NET 10 console application: `Program.cs` wires up a
singleton service graph, `DatabaseQuery.Parse()` does regex-based extraction
(no SQL grammar parser), `PerformanceIssueDetectorService` runs the detectors,
and `QueryAnalyzerService` orchestrates scoring and index suggestions. All
storage (repositories, cache, queue) is in-memory. For the full module
breakdown, data flow, design rationale, and known limitations, see
**[docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)**.

---

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

## QueryPlan

The `QueryPlan` class represents a parsed SQL execution plan with comprehensive performance metrics, execution statistics, and structural information about query operations. It captures the execution plan tree, identifies expensive operations, detects table scans and missing indexes, and provides methods for plan analysis and optimization recommendations. This type is essential for understanding query performance characteristics and identifying optimization opportunities.

### Usage Example

```csharp
// Parse an execution plan from SQL Server XML format
var queryPlan = new QueryPlan
{
    DatabaseName = "SalesDB",
    Format = PlanFormat.SqlServer,
    IsEstimated = true,
    TotalEstimatedCost = 1250.5,
    TotalEstimatedCpuCost = 850.2,
    TotalEstimatedIoCost = 400.3,
    TotalEstimatedRows = 15000,
    TotalLogicalReads = 15678,
    TotalPhysicalReads = 45,
    TotalElapsedTime = TimeSpan.FromMilliseconds(245)
};

// Build the execution plan tree structure
queryPlan.RootNode = new PlanNode
{
    NodeType = "Index Seek",
    ObjectName = "IX_Orders_CustomerId",
    EstimatedCost = 850.2,
    EstimatedRows = 15000,
    EstimatedIoCost = 400.3,
    EstimatedCpuCost = 450.1,
    Depth = 0,
    Properties = new Dictionary<string, string>
    {
        {"Seek Predicate", "[CustomerId] = [@CustomerId]"},
        {"Actual Execution", "true"}
    }
};

queryPlan.RootNode.Children.Add(new PlanNode
{
    NodeType = "Key Lookup",
    ObjectName = "Orders",
    EstimatedCost = 400.3,
    EstimatedRows = 15000,
    Depth = 1
});

// Initialize the plan to extract all nodes, table accesses, and joins
queryPlan.Initialize();

// Access basic plan properties
Console.WriteLine($"Plan ID: {queryPlan.PlanId}");
Console.WriteLine($"Database: {queryPlan.DatabaseName}");
Console.WriteLine($"Captured: {queryPlan.CapturedAt}");
Console.WriteLine($"Format: {queryPlan.Format}");
Console.WriteLine($"Total Cost: {queryPlan.TotalEstimatedCost}");
Console.WriteLine($"Estimated Rows: {queryPlan.TotalEstimatedRows}");
Console.WriteLine($"Logical Reads: {queryPlan.TotalLogicalReads}");

// Get expensive operations (top 5 by cost)
var expensiveOps = queryPlan.GetExpensiveOperations();
Console.WriteLine($"\nTop {expensiveOps.Count} expensive operations:");
foreach (var op in expensiveOps)
{
    Console.WriteLine($"- {op.NodeType} on {op.ObjectName}: {op.EstimatedCost} cost");
}

// Detect table scans (potential performance issues)
var tableScans = queryPlan.GetTableScans();
if (tableScans.Any())
{
    Console.WriteLine($"\n⚠️ Found {tableScans.Count} table scans:");
    foreach (var scan in tableScans)
    {
        Console.WriteLine($"- Table: {scan.ObjectName}");
    }
}

// Get all index operations
var indexOps = queryPlan.GetIndexOperations();
Console.WriteLine($"\nIndex operations: {indexOps.Count}");

// Detect missing indexes
var missingIndexes = queryPlan.DetectMissingIndexes();
Console.WriteLine($"\nMissing index recommendations:");
foreach (var recommendation in missingIndexes)
{
    Console.WriteLine($"- {recommendation}");
}

// Get plan summary
var summary = queryPlan.ToSummary();
Console.WriteLine($"\nPlan summary: {summary.Count} metrics");
```

### Public Members

- `PlanId` - Unique identifier for the execution plan (auto-generated GUID)
- `DatabaseName` - Name of the database being analyzed
- `CapturedAt` - Timestamp when the plan was captured (UTC)
- `IsEstimated` - Whether this is an estimated plan (EXPLAIN) or actual execution plan
- `Format` - Format of the execution plan (SqlServer, PostgreSQL, MySql, Oracle, Json)
- `RootNode` - Root node of the execution plan tree
- `TotalEstimatedCost` - Total estimated cost of the query
- `TotalEstimatedIoCost` - Total estimated I/O cost
- `TotalEstimatedCpuCost` - Total estimated CPU cost
- `TotalEstimatedRows` - Total estimated number of rows
- `TotalElapsedTime` - Total elapsed time
- `TotalLogicalReads` - Total logical reads performed
- `TotalPhysicalReads` - Total physical reads performed
- `AllNodes` - List of all nodes in the execution plan tree
- `TableAccesses` - List of table access operations (scans, seeks)
- `Joins` - List of join operations in the plan
- `Initialize()` - Initializes the plan structure and extracts all nodes, table accesses, and joins
- `GetExpensiveOperations(int topN = 5)` - Gets the top N most expensive operations by estimated cost
- `GetTableScans()` - Gets all table scan operations (potential performance issues)
- `GetIndexOperations()` - Gets all index operations (seeks and scans)
- `DetectMissingIndexes()` - Detects potential missing indexes based on table scans
- `ToSummary()` - Exports plan summary as a dictionary for serialization



## QueryAnalysisExtensions

The `QueryAnalysisExtensions` class provides extension methods for `QueryAnalysisResult` and `IEnumerable<QueryAnalysisResult>` that enable convenient analysis, filtering, and aggregation operations on SQL query analysis results. These methods help identify performance issues, calculate improvement potential, and generate actionable recommendations for query optimization.

### Usage Example

```csharp
// Analyze a SQL query using the analyzer service
var analyzer = new QueryAnalyzerService();
var result = await analyzer.AnalyzeAsync(
    "SELECT u.Name, COUNT(o.Id) as OrderCount " +
    "FROM Users u LEFT JOIN Orders o ON u.Id = o.UserId " +
    "GROUP BY u.Name HAVING COUNT(o.Id) > 5 " +
    "ORDER BY OrderCount DESC");

// Check if query has critical problems
if (result.HasCriticalProblems())
{
    Console.WriteLine("⚠️ Critical issues detected!");
}

// Get top 5 issues by impact
var topIssues = result.GetTopIssuesByImpact(5);
foreach (var issue in topIssues)
{
    Console.WriteLine($"- {issue.IssueType}: {issue.Description} (Impact: {issue.EstimatedPerformanceImpact:F1}%)");
}

// Get top 3 index suggestions
var topSuggestions = result.GetTopSuggestions(3);
foreach (var suggestion in topSuggestions)
{
    Console.WriteLine($"- Create index on {suggestion.TableName}.{suggestion.ColumnName}");
}

// Calculate potential improvement
var improvement = result.GetPotentialImprovement();
Console.WriteLine($"Potential improvement: {improvement:F1} percentage points");

// Check if query meets performance threshold
if (!result.MeetsPerformanceThreshold(threshold: 80))
{
    Console.WriteLine("⚠️ Query performance below threshold!");
}

// Get issue summary by type
var issueSummary = result.GetIssueSummary();
foreach (var kvp in issueSummary)
{
    Console.WriteLine($"- {kvp.Key}: {kvp.Value} issues");
}

// Get criticality level (0-10 scale)
var criticality = result.GetCriticalityLevel();
Console.WriteLine($"Criticality level: {criticality}/10");

// Get human-readable recommendation
var recommendation = result.GetRecommendation();
Console.WriteLine($"Recommendation: {recommendation}");

// Filter results by complexity
var complexQueries = result.FilterByComplexity(Constants.QueryComplexity.High);

// Filter results by score range
var goodQueries = result.FilterByScore(minScore: 80);

// Export as JSON-compatible dictionary
var exportData = result.ExportAsJson();
Console.WriteLine($"Exported {exportData.Count} fields");

// Batch operations with multiple results
var batchResults = new List<QueryAnalysisResult> { result1, result2, result3 };
var batchStats = batchResults.GetBatchStatistics();
Console.WriteLine($"Batch: {batchStats.TotalQueries} queries, avg score: {batchStats.AverageScore:F1}");
```

### Public Members

- `GetIssuesBySeverity(this QueryAnalysisResult result, Constants.IssueSeverity severity)` - Gets all issues of a specific severity level
- `GetIssuesByType(this QueryAnalysisResult result, Constants.IssueType issueType)` - Gets all issues of a specific type
- `GetTopIssuesByImpact(this QueryAnalysisResult result, int count = 5)` - Gets top N issues by performance impact
- `GetTopSuggestions(this QueryAnalysisResult result, int count = 3)` - Gets top N index suggestions by performance gain
- `HasCriticalProblems(this QueryAnalysisResult result)` - Checks if result has any issues of critical severity
- `MeetsPerformanceThreshold(this QueryAnalysisResult result, double threshold = 70.0)` - Checks if result meets minimum performance threshold
- `GetIssueSummary(this QueryAnalysisResult result)` - Gets issue summary grouped by type
- `GetPotentialImprovement(this QueryAnalysisResult result)` - Calculates percentage improvement if all suggestions are implemented
- `GetCriticalityLevel(this QueryAnalysisResult result)` - Gets criticality level (0-10 scale)
- `GetRecommendation(this QueryAnalysisResult result)` - Gets a human-readable recommendation based on analysis
- `Merge(this IEnumerable<QueryAnalysisResult> results)` - Merges multiple analysis results
- `ExportAsJson(this QueryAnalysisResult result)` - Exports analysis result to dictionary for serialization
- `FilterByComplexity(this IEnumerable<QueryAnalysisResult> results, Constants.QueryComplexity complexity)` - Filters results by complexity level
- `FilterByScore(this IEnumerable<QueryAnalysisResult> results, double minScore, double maxScore = 100)` - Filters results by performance score threshold
- `WithCriticalIssues(this IEnumerable<QueryAnalysisResult> results)` - Filters results that have critical issues
- `OrderByPerformance(this IEnumerable<QueryAnalysisResult> results)` - Orders results by performance score (worst first)
- `GetBatchStatistics(this IEnumerable<QueryAnalysisResult> results)` - Gets overall statistics for a batch of results

- `BatchStatistics.TotalQueries` - Total number of queries in batch
- `BatchStatistics.AverageScore` - Average performance score across all queries
- `BatchStatistics.WorstScore` - Worst performance score in batch
- `BatchStatistics.BestScore` - Best performance score in batch
- `BatchStatistics.TotalIssuesFound` - Total number of issues across all queries
- `BatchStatistics.QueriesWithIssues` - Number of queries with at least one issue


## QueryAnalysisResultExtensions

The `QueryAnalysisResultExtensions` class provides extension methods for `QueryAnalysisResult` that enhance functionality with convenient operations for analyzing query performance results. These methods help determine query severity levels, check performance thresholds, create deep copies of results, format summaries, and serialize results to JSON for logging or API responses.

### Usage Example

```csharp
// Analyze a SQL query using the analyzer service
var analyzer = new QueryAnalyzerService();
var result = await analyzer.AnalyzeAsync(
    "SELECT u.Name, COUNT(o.Id) as OrderCount " +
    "FROM Users u LEFT JOIN Orders o ON u.Id = o.UserId " +
    "GROUP BY u.Name HAVING COUNT(o.Id) > 5 " +
    "ORDER BY OrderCount DESC");

// Check if query is high performance (score >= 90 and no critical issues)
bool isHighPerformance = result.IsHighPerformance();
Console.WriteLine($"High performance: {isHighPerformance}");

// Check if query needs optimization (score < 70 or has critical issues)
bool needsOptimization = result.NeedsOptimization();
Console.WriteLine($"Needs optimization: {needsOptimization}");

// Get severity level based on performance score and issues
string severityLevel = result.GetSeverityLevel();
Console.WriteLine($"Severity level: {severityLevel}");

// Create a deep copy to prevent mutation of original result
var resultCopy = result.DeepCopy();

// Format a human-readable summary of the analysis
string summary = result.FormatSummary();
Console.WriteLine(summary);

// Serialize to JSON for logging or API response
string json = result.ToJsonString(indented: true);
Console.WriteLine(json);
```

### Public Members

- `IsHighPerformance(this QueryAnalysisResult result)` - Determines if query has performance score >= 90 and no critical issues
- `NeedsOptimization(this QueryAnalysisResult result)` - Determines if query needs optimization (score < 70 or has critical issues)
- `GetSeverityLevel(this QueryAnalysisResult result)` - Gets severity level (Critical, High, Medium, Low) based on performance score and issues
- `DeepCopy(this QueryAnalysisResult result)` - Creates a deep copy of the query analysis result to prevent mutation of the original
- `FormatSummary(this QueryAnalysisResult result)` - Gets a formatted string representation of the query analysis result with key metrics
- `ToJsonString(this QueryAnalysisResult result, bool indented = false)` - Serializes the query analysis result to a JSON string with optional formatting

## QueryRewriteSuggestion

The `QueryRewriteSuggestion` class represents a recommended SQL query transformation that improves performance by addressing common anti-patterns such as SELECT *, implicit joins, non-sargable predicates, and inefficient subqueries. Each suggestion includes the original and rewritten query text, the type of transformation, estimated improvement percentage, risk assessment, and related index recommendations to make the optimization effective.

This type is generated by the query analyzer when it detects opportunities to rewrite queries for better performance, and provides developers with actionable optimization recommendations with safety assessments.

### Usage Example

```csharp
// Analyze a query and get rewrite suggestions
var analyzer = new QueryAnalyzerService();
var rewriteService = new QueryRewriteService();

var suggestions = await rewriteService.GetRewriteSuggestionsAsync(
    "SELECT * FROM Orders WHERE CustomerId = 1 AND Status = 'active'",
    "GetActiveCustomerOrders"
);

// Filter suggestions by type and applicability
var joinOptimizations = suggestions.OfType(RewriteType.SubqueryToJoin);
var autoApplicable = suggestions.GetAutoApplicable();

// Apply the highest priority suggestion
var topSuggestion = suggestions.OrderByImpact().FirstOrDefault();
if (topSuggestion != null && topSuggestion.IsAutoApplicable)
{
    Console.WriteLine($"Applying rewrite: {topSuggestion.GetSummary()}");
    Console.WriteLine($"Risk level: {topSuggestion.GetRiskLevel()}");
    Console.WriteLine($"Original query: {topSuggestion.OriginalQuery}");
    Console.WriteLine($"Rewritten query: {topSuggestion.RewrittenQuery}");
    
    // Check related index suggestions
    foreach (var indexSuggestion in topSuggestion.RelatedIndexSuggestions)
    {
        Console.WriteLine($"- Create index: {indexSuggestion.TableName}.{indexSuggestion.ColumnName}");
    }
}
```

### Public Members

- `SuggestionId` - Unique identifier for the suggestion (auto-generated GUID)
- `OriginalQuery` - The original SQL query text before transformation
- `RewrittenQuery` - The optimized SQL query text after transformation
- `RewriteType` - Type of rewrite transformation applied (e.g., ExplicitColumnSelection, SubqueryToJoin, FunctionSargability)
- `AffectedClause` - SQL clause primarily targeted by the rewrite (e.g., "SELECT", "WHERE", "JOIN")
- `Rationale` - Human-readable explanation for why this rewrite improves performance
- `AdditionalNotes` - Additional caveats or implementation considerations
- `EstimatedImprovementPercent` - Estimated performance improvement (0-100 scale)
- `IsBreakingChange` - Whether applying this rewrite changes the observable result set
- `IsAutoApplicable` - Whether the rewrite is safe to apply programmatically without manual review
- `Priority` - Suggestion priority (1 = highest priority)
- `RelatedIndexSuggestions` - List of index suggestions that complement this rewrite
- `GeneratedAt` - Timestamp when the suggestion was generated (UTC)
- `IsValid()` - Validates that the suggestion contains enough data to be acted upon
- `GetRiskLevel()` - Returns risk assessment: LOW, MEDIUM, or HIGH based on rewrite characteristics
- `GetSummary()` - Generates a one-line summary including rewrite type, rationale, and estimated improvement
- `ToJsonDictionary()` - Exports suggestion data as a structured dictionary for JSON serialization


## QueryRewriteExtensions

The `QueryRewriteExtensions` class provides extension methods for `IQueryRewriteService` and `IEnumerable<QueryRewriteSuggestion>` that enable dependency injection registration and LINQ-style convenience operations for SQL query rewrite suggestions. These methods help filter, sort, and analyze query rewrite suggestions to identify optimal optimization opportunities.

### Usage Example

```csharp
// Setup dependency injection (ASP.NET Core example)
services.AddQueryRewriteService();

// Create a query rewrite service
var rewriteService = new QueryRewriteService();

// Analyze a query to get rewrite suggestions
var suggestions = await rewriteService.GetRewriteSuggestionsAsync(
    "SELECT * FROM Orders WHERE CustomerId = 1 AND Status = 'active'",
    "GetActiveCustomerOrders"
);

// Filter suggestions
var autoApplicable = suggestions.GetAutoApplicable();
var nonBreaking = suggestions.GetNonBreaking();
var whereClauseSuggestions = suggestions.ForClause("WHERE");

// Order suggestions by impact
var orderedSuggestions = suggestions.OrderByImpact();

// Get index suggestions for database optimization
var indexSuggestions = suggestions.GetAllIndexSuggestions();

// Calculate total estimated improvement
var totalImprovement = suggestions.GetTotalEstimatedImprovement();
Console.WriteLine($"Total estimated improvement: {totalImprovement:F1}%");

// Get a summary of all suggestions
var summary = suggestions.GetRewriteSummary();
Console.WriteLine(summary);

// Filter by specific rewrite type
var joinOptimizations = suggestions.OfType(RewriteType.JoinOptimization);
var whereOptimizations = suggestions.OfType(RewriteType.WhereClauseOptimization);

// Display top 3 suggestions by impact
foreach (var suggestion in orderedSuggestions.Take(3))
{
    Console.WriteLine($"- {suggestion.Description} (Impact: {suggestion.EstimatedImprovementPercent}%)");
}
```

### Public Members

- `AddQueryRewriteService(this IServiceCollection services)` - Registers `IQueryRewriteService` with the DI container as a singleton
- `GetAutoApplicable(this IEnumerable<QueryRewriteSuggestion> suggestions)` - Filters suggestions to those that are safe to apply programmatically without manual review
- `GetNonBreaking(this IEnumerable<QueryRewriteSuggestion> suggestions)` - Filters suggestions that do not alter the observable result set
- `OfType(this IEnumerable<QueryRewriteSuggestion> suggestions, RewriteType rewriteType)` - Returns suggestions of a specific rewrite type
- `ForClause(this IEnumerable<QueryRewriteSuggestion> suggestions, string clause)` - Returns suggestions that target a specific SQL clause (e.g. "WHERE", "SELECT")
- `OrderByImpact(this IEnumerable<QueryRewriteSuggestion> suggestions)` - Orders suggestions by estimated performance improvement, highest first
- `GetTotalEstimatedImprovement(this IEnumerable<QueryRewriteSuggestion> suggestions)` - Calculates the sum of all estimated improvements, capped at 100%
- `GetAllIndexSuggestions(this IEnumerable<QueryRewriteSuggestion> suggestions)` - Collects all `IndexSuggestion` items embedded in the rewrite suggestions into a deduplicated, prioritized flat list
- `GetRewriteSummary(this IEnumerable<QueryRewriteSuggestion> suggestions)` - Gets a human-readable summary of the full rewrite suggestion set

## QueryAnalysisResult

The `QueryAnalysisResult` class represents the complete analysis result of a SQL query, including performance metrics, detected issues, optimization suggestions, and execution statistics. This type is the primary return value from query analysis operations and provides comprehensive information for performance monitoring, optimization recommendations, and reporting.

### Usage Example

```csharp
// Analyze a SQL query using the analyzer service
var analyzer = new QueryAnalyzerService();
var result = await analyzer.AnalyzeAsync(
    "SELECT u.Name, COUNT(o.Id) as OrderCount " +
    "FROM Users u LEFT JOIN Orders o ON u.Id = o.UserId " +
    "GROUP BY u.Name HAVING COUNT(o.Id) > 5 " +
    "ORDER BY OrderCount DESC");

// Access basic analysis properties
Console.WriteLine($"Query ID: {result.QueryId}");
Console.WriteLine($"Query: {result.Query}");
Console.WriteLine($"Analyzed at: {result.AnalyzedAt}");
Console.WriteLine($"Complexity: {result.Complexity}");
Console.WriteLine($"Performance score: {result.PerformanceScore:F1}/100");
Console.WriteLine($"Estimated execution time: {result.EstimatedExecutionTime.TotalMilliseconds}ms");

// Access performance issues and suggestions
Console.WriteLine($"Issues found: {result.Issues.Count}");
foreach (var issue in result.Issues.OrderByDescending(i => i.EstimatedPerformanceImpact))
{
    Console.WriteLine($"- [{issue.Severity}] {issue.IssueType}: {issue.Description}");
}

Console.WriteLine($"Index suggestions: {result.IndexSuggestions.Count}");
foreach (var suggestion in result.IndexSuggestions.OrderByDescending(s => s.EstimatedPerformanceGain))
{
    Console.WriteLine($"- Create index on {suggestion.TableName}.{suggestion.ColumnName} ({suggestion.EstimatedPerformanceGain}% gain)");
}

// Access execution plan if available
if (result.ExecutionPlan != null)
{
    Console.WriteLine($"Execution plan cost: {result.ExecutionPlan.TotalEstimatedCost}");
}

// Access statistics
Console.WriteLine($"Rows returned: {result.Statistics.AverageRowsReturned}");
Console.WriteLine($"Total execution time: {result.Statistics.TotalExecutionTime.TotalMilliseconds}ms");

// Generate summary and export to JSON
Console.WriteLine($"\n{result.GetSummary()}");

var exportData = result.ToJsonDictionary();
Console.WriteLine($"\nExport contains {exportData.Count} fields");
```

### Public Members

- `QueryId` - Unique identifier for the analysis result (auto-generated GUID)
- `Query` - The SQL query string being analyzed
- `QueryText` - Alias for Query property
- `AnalyzedAt` - Timestamp when the analysis was performed (UTC)
- `Complexity` - Complexity level of the query (Low, Medium, High)
- `PerformanceScore` - Performance score (0-100, where higher is better)
- `EstimatedExecutionTime` - Estimated execution time for the query
- `Issues` - List of performance issues detected
- `IndexSuggestions` - List of index suggestions for optimization
- `ExecutionPlan` - Execution plan for the query (nullable)
- `Statistics` - Query execution statistics
- `Metadata` - Additional metadata associated with the analysis
- `ComplexityScore` - Computed complexity score (higher values indicate more costly queries)
- `HasCriticalIssues` - Boolean indicating if any critical issues were found
- `TotalOptimizationPotential` - Total estimated performance gain from all index suggestions
- `GetSummary()` - Generates a summary string of the analysis results
- `ToJsonDictionary()` - Exports the analysis result as a structured dictionary for JSON serialization

## DatabaseQuery

The `DatabaseQuery` class represents a parsed SQL query with comprehensive metadata, lineage information, and analysis context. It captures query structure, referenced database objects, parameters, and execution context to support query analysis, optimization recommendations, and performance monitoring. This type serves as the foundation for SQL query parsing and analysis operations throughout the analyzer.

### Usage Example

```csharp
// Parse a SQL query with full context
var query = new DatabaseQuery
{
    QueryText = @"SELECT u.UserId, u.Name, u.Email, o.OrderCount, o.TotalAmount " +
                @"FROM Users u " +
                @"JOIN (
                    SELECT UserId, COUNT(*) as OrderCount, SUM(Amount) as TotalAmount " +
                    @"FROM Orders WHERE OrderDate > '2024-01-01' GROUP BY UserId
                ) o ON u.UserId = o.UserId " +
                @"WHERE u.Status = 'active' AND u.CreatedAt > @minDate " +
                @"ORDER BY o.TotalAmount DESC",
    
    DatabaseName = "ECommerceDB",
    SchemaName = "dbo",
    ApplicationName = "OrderProcessingService",
    Environment = "Production",
    CreatedBy = "data-analyst@company.com",
    
    // Parameters
    Parameters = new Dictionary<string, ParameterInfo>
    {
        {"@minDate", new ParameterInfo { ParameterName = "@minDate", DataType = "datetime" }}
    },
    
    // Variable declarations
    VariableDeclarations = new Dictionary<string, string>
    {
        {"@cutoffDate", "'2024-01-01'"}
    }
};

// Parse the query to extract metadata
query.Parse();

// Access parsed information
Console.WriteLine($"Query ID: {query.QueryId}");
Console.WriteLine($"Query Type: {query.QueryType}");
Console.WriteLine($"Database: {query.DatabaseName}");
Console.WriteLine($"Schema: {query.SchemaName}");
Console.WriteLine($"Tables: {string.Join(", ", query.ReferencedTables)}");
Console.WriteLine($"Columns: {string.Join(", ", query.ReferencedColumns)}");
Console.WriteLine($"Line Count: {query.LineCount}");
Console.WriteLine($"Statement Count: {query.StatementCount}");
Console.WriteLine($"Complexity: {query.CyclomaticComplexity:F2}");
Console.WriteLine($"Parameters: {query.Parameters.Count}");
Console.WriteLine($"Variables: {query.VariableDeclarations.Count}");
Console.WriteLine($"Join Conditions: {string.Join("; ", query.JoinConditions)}");
Console.WriteLine($"Where Conditions: {string.Join("; ", query.WhereConditions)}");

// Generate hash for deduplication
string hash = query.GenerateHash();
Console.WriteLine($"Query Hash: {hash}");

// Get summary
Console.WriteLine($"\n{query.GetSummary()}");
```

### Public Members

- `QueryId` - Unique identifier for the query (auto-generated GUID)
- `QueryText` - Raw SQL query text
- `ProcedureName` - Name of the stored procedure, if applicable
- `ModuleName` - Module name
- `ApplicationName` - Application name
- `DatabaseName` - Database name
- `QueryType` - Type of query (SELECT, INSERT, UPDATE, DELETE, etc.)
- `DatabaseType` - Database type (SqlServer, PostgreSQL, MySQL, Oracle, SQLite)
- `SchemaName` - Schema name (default: "dbo")
- `CreatedBy` - Creator identifier
- `CreatedDate` - Creation timestamp (UTC)
- `ModifiedBy` - Last modifier identifier
- `ModifiedDate` - Last modification timestamp (UTC)
- `ReferencedTables` - List of referenced table names
- `ReferencedColumns` - List of referenced column names
- `JoinConditions` - List of JOIN condition strings
- `WhereConditions` - List of WHERE condition strings
- `Parameters` - Dictionary of query parameters with type information
- `VariableDeclarations` - Dictionary of variable declarations
- `LineCount` - Number of lines in the query
- `StatementCount` - Number of SQL statements
- `CyclomaticComplexity` - Computed cyclomatic complexity
- `SourceFile` - Source file path
- `SourceLineNumber` - Source line number
- `CallingMethod` - Calling method name
- `Environment` - Execution environment (Development, Staging, Production)
- `QueryHash` - SHA-256 hash for deduplication
- `NormalizedQuery` - Normalized query text for analysis
- `Parse()` - Parses query text and extracts metadata
- `IsValid()` - Validates query structure
- `GetSummary()` - Generates a summary string
- `GenerateHash()` - Generates SHA-256 hash for deduplication

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

## SlowQueryEntry

The `SlowQueryEntry` class represents a structured slow-query log entry with comprehensive performance metrics and contextual information. It captures query execution details including duration, lock time, rows examined/sent, timestamp, user information, and database context, enabling performance analysis and optimization recommendations.

### Usage Example

```csharp
// Create a slow query entry from application monitoring
var slowQuery = new SlowQueryEntry
{
    QueryText = "SELECT * FROM Orders WHERE CustomerId = 123 AND Status = 'active'",
    Duration = TimeSpan.FromMilliseconds(450),
    LockTime = TimeSpan.FromMilliseconds(120),
    RowsExamined = 15678,
    RowsSent = 123,
    Timestamp = DateTime.UtcNow,
    UserHost = "app-server-01@10.0.1.45",
    Database = "ECommerceDB",
    LogSource = "MySql"
};

// Add custom metadata for additional context
slowQuery.Metadata.Add("application", "OrderProcessingService");
slowQuery.Metadata.Add("environment", "production");
slowQuery.Metadata.Add("query_hash", "a1b2c3d4e5f6");

// Check if this represents a full table scan
if (slowQuery.IsFullScan)
{
    Console.WriteLine("⚠️ Potential full table scan detected!");
}

// Calculate efficiency ratio
Console.WriteLine($"Efficiency: {slowQuery.EfficiencyRatio:P1} ({slowQuery.RowsSent}/{slowQuery.RowsExamined} rows)");

// Generate a summary for logging
string summary = slowQuery.GetSummary();
Console.WriteLine(summary);
```

### Public Members

- `EntryId` - Unique identifier for the parsed entry (auto-generated GUID)
- `QueryText` - SQL text extracted from the log
- `Duration` - Total query duration
- `LockTime` - Time spent waiting on locks
- `RowsExamined` - Rows examined by the query
- `RowsSent` - Rows returned to the caller
- `Timestamp` - Timestamp of the logged execution
- `UserHost` - User and host information from the log entry
- `Database` - Database name associated with the query
- `LogSource` - Source engine for the entry (MySql, PostgreSql, SqlServer, etc.)
- `Metadata` - Additional engine-specific attributes as key-value pairs
- `EfficiencyRatio` - Ratio of rows returned to rows examined (calculated property)
- `IsFullScan` - Indicates whether the entry likely represents a full scan (calculated property)
- `GetSummary()` - Builds a short textual summary of the slow query entry

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

## ProfilerOptions

The `ProfilerOptions` class provides runtime configuration for query profiling sessions. It controls what data is collected (execution plans, timings, resource usage), sets timeouts and iteration counts, and enables plan visualizations. Use these options when calling `IQueryProfilerService.ProfileQueryAsync` to override default profiling behavior on a per-query basis.

### Usage Example

```csharp
// Configure profiler options for a specific query analysis
var profilerOptions = new ProfilerOptions
{
    // Capture execution plan for detailed analysis
    CaptureExecutionPlan = true,
    
    // Measure timing for each pipeline stage
    CaptureTimings = true,
    
    // Collect CPU and memory usage snapshot
    CaptureResourceUsage = true,
    
    // Set maximum duration before aborting (30 seconds by default)
    MaxDurationMs = 15_000,
    
    // Run 2 warm-up iterations before collecting metrics
    WarmUpIterations = 2,
    
    // Average results across 3 measurement iterations
    MeasurementIterations = 3,
    
    // Include ASCII visualization of execution plan in report
    IncludePlanVisualization = true
};

// Use with profiler service
var profilerService = serviceProvider.GetRequiredService<IQueryProfilerService>();

// Profile a query with custom options
var report = await profilerService.ProfileQueryAsync(
    "SELECT u.Name, COUNT(o.Id) as OrderCount " +
    "FROM Users u LEFT JOIN Orders o ON u.Id = o.UserId " +
    "GROUP BY u.Name HAVING COUNT(o.Id) > 5 " +
    "ORDER BY OrderCount DESC",
    "GetTopCustomers",
    profilerOptions
);

// Access the collected data from the report
if (report != null)
{
    Console.WriteLine($"Performance Score: {report.PerformanceScore:F1}/100");
    Console.WriteLine($"Profiling Duration: {report.TotalProfilingDurationMs:F0}ms");
    
    if (report.ExecutionPlan != null)
    {
        Console.WriteLine($"Execution Plan Cost: {report.ExecutionPlan.TotalEstimatedCost}");
    }
    
    if (report.PlanVisualization != null)
    {
        Console.WriteLine("Plan Visualization:");
        Console.WriteLine(report.PlanVisualization.Text);
    }
    
    Console.WriteLine($"Suggestions: {report.Suggestions.Count}");
}
```

### Public Members

- `CaptureExecutionPlan` - Capture and embed the execution plan in the profiler report (default: `true`)
- `CaptureTimings` - Measure wall-clock time for each pipeline stage (default: `true`)
- `CaptureResourceUsage` - Collect a CPU and memory snapshot during profiling (default: `true`)
- `MaxDurationMs` - Maximum wall-clock budget before aborting, in milliseconds (default: `30_000`)
- `WarmUpIterations` - Number of warm-up iterations executed before metrics collection (default: `0`)
- `MeasurementIterations` - Number of measurement iterations whose results are averaged (default: `1`)
- `IncludePlanVisualization` - Include ASCII tree visualization of execution plan in report (default: `true`)



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

## DatabaseConnectionValidatorExtensions

The `DatabaseConnectionValidatorExtensions` class provides extension methods for validating database connections and formatting SQL queries without executing them. These methods help verify database connectivity, check query syntax, and generate diagnostic reports for troubleshooting connection and formatting issues.

### Usage Example

```csharp
// Assume we have a database connection string
string connectionString = "Server=localhost;Database=TestDB;User Id=sa;Password=your_password;";

// Validate database connection asynchronously
var validationResult = await DatabaseConnectionValidatorExtensions.ValidateConnectionAsync(connectionString);

if (validationResult.IsConnectionSuccessful)
{
    Console.WriteLine("✅ Connection successful!");
    Console.WriteLine($"Database version: {validationResult.GetFormattedVersion()}");
}
else
{
    Console.WriteLine("❌ Connection failed!");
    Console.WriteLine($"Error: {validationResult.GetErrorSummary()}");
}

// Validate query format only (without executing)
bool isValidFormat = await DatabaseConnectionValidatorExtensions.ValidateFormatOnlyAsync(
    "SELECT * FROM Users WHERE Id = 1 AND Status = 'active'"
);
Console.WriteLine($"Query format is valid: {isValidFormat}");

// Generate a comprehensive diagnostic report
string diagnosticReport = validationResult.GenerateDiagnosticReport();
Console.WriteLine("Diagnostic Report:");
Console.WriteLine(diagnosticReport);

// Check specific validation properties
if (validationResult.IsConnectionSuccessful && validationResult.GetErrorSummary().Length == 0)
{
    Console.WriteLine("✅ All connection checks passed!");
}
```

### Public Members

- `ValidateConnectionAsync(string connectionString)` - Validates database connection and returns connection validation result
- `ValidateFormatOnlyAsync(string query)` - Validates SQL query format without executing it
- `GetErrorSummary(this ConnectionValidationResult result)` - Gets a summary of connection errors
- `IsConnectionSuccessful(this ConnectionValidationResult result)` - Checks if connection was successful
- `GetFormattedVersion(this ConnectionValidationResult result)` - Gets formatted database version information
- `GenerateDiagnosticReport(this ConnectionValidationResult result)` - Generates a comprehensive diagnostic report

## SqlQueryAnalyzerOptionsExtensions

The `SqlQueryAnalyzerOptionsExtensions` class provides extension methods for `SqlQueryAnalyzerOptions` that validate configuration, retrieve normalized values, and determine feature availability. These methods ensure consistent access to configuration values and provide safe defaults for missing or invalid settings.

### Usage Example

```csharp
// Configure SQL query analyzer options
var options = new SqlQueryAnalyzerOptions
{
    Database = new DatabaseOptions
    {
        Provider = "SqlServer",
        ConnectionString = "Server=localhost;Database=TestDB;User=sa;Password=your_password;",
        ConnectionTimeoutSeconds = 30,
        EnableConnectionLogging = true
    },
    Analysis = new AnalysisOptions
    {
        MaxThreads = 8,
        DetectNPlusOne = true,
        DetectMissingIndexes = true,
        DetectJoinIssues = true,
        AnalyzeExecutionPlans = true,
        IndexSeverity = new Dictionary<string, int> { { "IX_Users_Email", 80 } },
        IgnorePatterns = new List<string> { "SELECT * FROM Logs" }
    },
    Cache = new CacheOptions { Enabled = true },
    Performance = new PerformanceOptions { MaxQueryLength = 2048 },
    Logging = new LoggingOptions { MinimumLevel = "Information" }
};

// Validate configuration
bool isValid = options.IsValid();
Console.WriteLine($"Configuration is valid: {isValid}");

// Check if analyzer is enabled
bool isEnabled = options.IsAnalyzerEnabled();
Console.WriteLine($"Analyzer is enabled: {isEnabled}");

// Get normalized provider name
string normalizedProvider = options.GetNormalizedProvider();
Console.WriteLine($"Normalized provider: {normalizedProvider}");

// Check if critical analysis features are enabled
bool hasCriticalAnalysis = options.HasCriticalAnalysisEnabled();
Console.WriteLine($"Critical analysis enabled: {hasCriticalAnalysis}");

// Get connection timeout in milliseconds
int timeoutMs = options.GetConnectionTimeoutMs();
Console.WriteLine($"Connection timeout: {timeoutMs}ms");

// Get maximum concurrent threads (clamped between 1-100)
int maxThreads = options.GetMaxConcurrentThreads();
Console.WriteLine($"Max concurrent threads: {maxThreads}");

// Check if detailed logging should be enabled
bool enableDetailedLogging = options.ShouldEnableDetailedLogging();
Console.WriteLine($"Detailed logging enabled: {enableDetailedLogging}");

// Get ignore patterns (returns empty list if null)
var ignorePatterns = options.GetIgnorePatterns();
Console.WriteLine($"Ignore patterns count: {ignorePatterns.Count}");

// Check if execution plan analysis is enabled
bool analyzeExecutionPlans = options.ShouldAnalyzeExecutionPlans();
Console.WriteLine($"Execution plan analysis enabled: {analyzeExecutionPlans}");

// Get maximum query length (minimum 1024)
int maxQueryLength = options.GetMaxQueryLength();
Console.WriteLine($"Max query length: {maxQueryLength}");
```

### Public Members

- `IsValid(this SqlQueryAnalyzerOptions options)` - Validates that all required options are properly configured
- `IsAnalyzerEnabled(this SqlQueryAnalyzerOptions options)` - Determines if the analyzer is enabled for execution
- `GetNormalizedProvider(this SqlQueryAnalyzerOptions options)` - Gets the effective provider name (normalized to lowercase)
- `HasCriticalAnalysisEnabled(this SqlQueryAnalyzerOptions options)` - Determines if any critical analysis features are enabled
- `GetConnectionTimeoutMs(this SqlQueryAnalyzerOptions options)` - Gets the effective connection timeout in milliseconds
- `GetMaxConcurrentThreads(this SqlQueryAnalyzerOptions options)` - Gets the effective maximum concurrent analysis threads (clamped between 1-100)
- `ShouldEnableDetailedLogging(this SqlQueryAnalyzerOptions options)` - Determines if detailed logging should be enabled
- `GetIgnorePatterns(this SqlQueryAnalyzerOptions options)` - Gets the list of patterns to ignore (returns empty list if null)
- `ShouldAnalyzeExecutionPlans(this SqlQueryAnalyzerOptions options)` - Determines if execution plan analysis is enabled
- `GetMaxQueryLength(this SqlQueryAnalyzerOptions options)` - Gets the effective maximum query length limit (minimum 1024)


## QueryCacheKeyGeneratorExtensions

The `QueryCacheKeyGeneratorExtensions` class provides extension methods for `QueryCacheKeyGenerator` that extend cache key generation and management capabilities. These methods enable composite key generation, key comparison, parameter extraction, key expiration checking, and formatted key display for advanced caching scenarios in SQL query analysis scenarios.

### Usage Example

```csharp
// Create cache key generator
var generator = new QueryCacheKeyGenerator();

// Generate a cache key for a simple query
string simpleKey = generator.GenerateQueryKey(
    "SELECT * FROM Users WHERE Id = 1 AND Status = 'active'"
);
Console.WriteLine($"Simple key: {simpleKey}");

// Generate a cache key for a query with parameters
var parameters = new Dictionary<string, object>
{
    {"UserId", 123},
    {"Status", "active"},
    {"CreatedAfter", new DateTime(2024, 1, 1)}
};
string parameterizedKey = generator.GenerateParameterizedQueryKey(
    "SELECT * FROM Users WHERE Id = @UserId AND Status = @Status AND CreatedAt > @CreatedAfter",
    parameters
);
Console.WriteLine($"Parameterized key: {parameterizedKey}");

// Create a composite key from multiple query keys
string compositeKey = generator.CreateCompositeKey(
    generator.GenerateQueryKey("SELECT * FROM Users"),
    generator.GenerateQueryKey("SELECT * FROM Orders"),
    generator.GenerateQueryKey("SELECT COUNT(*) FROM Products")
);
Console.WriteLine($"Composite key: {compositeKey}");

// Check if two keys represent the same query
bool sameQuery = generator.AreKeysForSameQuery(simpleKey, parameterizedKey);
Console.WriteLine($"Keys represent same query: {sameQuery}");

// Check if a cache key is expired (older than 24 hours)
bool isExpired = generator.IsCacheKeyExpired(simpleKey, maxAgeHours: 24);
Console.WriteLine($"Key is expired: {isExpired}");

// Format a cache key for display/logging
string formattedKey = generator.FormatCacheKey(simpleKey);
Console.WriteLine($"Formatted key: {formattedKey}");

// Extract parameters from a metadata key (returns null as parameters cannot be recovered from hash)
var extractedParams = generator.ExtractParametersFromMetadataKey(
    generator.GenerateParameterizedQueryKey(
        "SELECT * FROM Users WHERE Id = @UserId",
        new Dictionary<string, object> { {"UserId", 123} }
    )
);
Console.WriteLine($"Extracted parameters: {(extractedParams == null ? "null (parameters cannot be recovered from hash)" : extractedParams.Count)}");
```

### Public Members

- `CreateCompositeKey(this QueryCacheKeyGenerator generator, params string[] queryKeys)` - Creates a composite cache key from multiple query keys for combined analysis results
- `AreKeysForSameQuery(this QueryCacheKeyGenerator generator, string key1, string key2)` - Checks if two cache keys represent the same query for deduplication
- `ExtractParametersFromMetadataKey(this QueryCacheKeyGenerator generator, string metadataKey)` - Extracts parameters from a metadata cache key (returns null as parameters cannot be recovered from hash)
- `GenerateParameterizedQueryKey(this QueryCacheKeyGenerator generator, string query, Dictionary<string, object>? parameters = null)` - Generates a cache key for a query with normalized parameters
- `IsCacheKeyExpired(this QueryCacheKeyGenerator generator, string key, int maxAgeHours)` - Checks if a cache key is expired based on key age
- `FormatCacheKey(this QueryCacheKeyGenerator generator, string key)` - Gets a display-friendly representation of a cache key for logging and debugging

## IndexRecommendation

The `IndexRecommendation` class represents a recommended index derived from SQL query analysis. It encapsulates the details needed to create an optimal index that would improve query performance, including the target table, key columns, included columns, estimated impact score, and the generated SQL script. This type is generated by index recommendation heuristics when analyzing queries with frequent table scans, missing WHERE clauses, or suboptimal join operations.

### Usage Example

```csharp
// Create an index recommendation for a frequently queried column
var recommendation = new IndexRecommendation
{
    TableName = "Users",
    KeyColumns = new List<string> { "Email" },
    IncludeColumns = new List<string> { "Name", "Status", "CreatedAt" },
    IndexType = "NONCLUSTERED",
    ImpactScore = 92.5,
    Rationale = "Index on Email column will significantly improve login query performance and cover common user lookup scenarios",
    Source = RecommendationSource.WhereClause
};

// Generate the CREATE INDEX script
recommendation.GenerateScript();

Console.WriteLine($"Recommendation ID: {recommendation.RecommendationId}");
Console.WriteLine($"Table: {recommendation.TableName}");
Console.WriteLine($"Key Columns: {string.Join(", ", recommendation.KeyColumns)}");
Console.WriteLine($"Include Columns: {string.Join(", ", recommendation.IncludeColumns)}");
Console.WriteLine($"Impact Score: {recommendation.ImpactScore}");
Console.WriteLine($"Generated Script:\n{recommendation.GeneratedScript}");
Console.WriteLine($"Recommended At: {recommendation.RecommendedAt}");
Console.WriteLine($"Source: {recommendation.Source}");
```

### Public Members

- `RecommendationId` - Unique identifier for the recommendation (auto-generated GUID)
- `TableName` - Target table for the recommended index
- `KeyColumns` - Key columns that should define the index
- `IncludeColumns` - Optional included columns that make the index covering
- `IndexType` - Type of index to create (e.g., "NONCLUSTERED")
- `ImpactScore` - Estimated performance impact on a 0-100 scale
- `Rationale` - Human-readable explanation for the recommendation
- `GeneratedScript` - Generated CREATE INDEX script
- `Source` - Clause or heuristic that produced the recommendation
- `RecommendedAt` - Timestamp when the recommendation was created
- `GenerateScript()` - Generates a CREATE INDEX statement for this recommendation

## PerformanceIssue

The `PerformanceIssue` class represents a detected performance problem in a SQL query, including detailed information about the issue type, severity, location, estimated impact, and recommended fixes. This type is used throughout the SQL Query Analyzer to track and report performance anti-patterns such as SELECT *, missing WHERE/LIMIT clauses, implicit joins, non-sargable predicates, and N+1 access patterns.

### Usage Example

```csharp
// Create a performance issue for a SELECT * query
var issue = new PerformanceIssue
{
    IssueType = IssueType.SelectStar,
    Severity = IssueSeverity.Critical,
    Description = "Query uses SELECT * which retrieves all columns and prevents proper indexing",
    AffectedClause = "SELECT",
    LineNumber = 5,
    ColumnNumber = 10,
    EstimatedPerformanceImpact = 85.5,
    AffectedRowCount = 1000000,
    EstimatedTimeIncrease = TimeSpan.FromMilliseconds(450),
    RecommendedFix = "Replace SELECT * with explicit column names",
    ExampleFix = "SELECT UserId, Name, Email FROM Users WHERE Id = 1",
    Priority = 1,
    Metadata = new Dictionary<string, string>
    {
        {"TableName", "Users"},
        {"ColumnCount", "15"},
        {"IndexedColumns", "UserId, Email"}
    }
};

// Display the issue
Console.WriteLine(issue.GetFormattedMessage());
Console.WriteLine($"Impact: {issue.EstimatedPerformanceImpact}%");
Console.WriteLine($"Priority: {issue.Priority}");
Console.WriteLine($"Critical: {issue.IsCritical}");

// Compare priorities with another issue
var otherIssue = new PerformanceIssue
{
    IssueType = IssueType.ImplicitJoin,
    Severity = IssueSeverity.Warning,
    EstimatedPerformanceImpact = 60.0,
    Priority = 2
};

int comparison = issue.ComparePriority(otherIssue);
Console.WriteLine("Comparison result: {comparison} (negative means this issue has higher priority)");
```

### Public Members

- `IssueId` - Unique identifier for the issue (auto-generated GUID)
- `IssueType` - Type of performance issue detected (e.g., SelectStar, ImplicitJoin, MissingWhereClause)
- `Severity` - Severity level (Critical, Warning, Info)
- `Description` - Detailed description of the performance issue
- `AffectedClause` - SQL clause affected by the issue (e.g., "SELECT", "WHERE", "JOIN")
- `LineNumber` - Line number where issue was detected (1-based)
- `ColumnNumber` - Column number where issue was detected
- `EstimatedPerformanceImpact` - Estimated performance impact (0-100 scale)
- `ImpactScore` - Alias for EstimatedPerformanceImpact
- `AffectedRowCount` - Estimated number of rows affected
- `EstimatedTimeIncrease` - Estimated time increase caused by the issue
- `RecommendedFix` - Recommended solution for the issue
- `ExampleFix` - Example of how to fix the issue
- `Priority` - Priority level (1 = highest, 5 = lowest)
- `Metadata` - Additional context/data about the issue
- `DetectedAt` - Timestamp when issue was detected (UTC)
- `IsValid()` - Validates the issue data
- `GetSeverityLabel()` - Returns human-readable severity label with emoji
- `GetFormattedMessage()` - Formats issue for display with severity, type, and location
- `IsCritical` - Gets whether issue is critical severity
- `ComparePriority(PerformanceIssue other)` - Compares issues by severity then impact

## QueryStatistics

The `QueryStatistics` class captures comprehensive execution statistics for SQL queries, including performance metrics, I/O operations, memory usage, and compilation information. This type is essential for query performance monitoring, identifying optimization opportunities, and generating actionable recommendations based on historical execution data.

### Usage Example

```csharp
// Collect statistics from query execution monitoring
var stats = new QueryStatistics
{
    ExecutionCount = 1567,
    TotalExecutionTime = TimeSpan.FromMilliseconds(4500),
    MinimumExecutionTime = TimeSpan.FromMilliseconds(120),
    MaximumExecutionTime = TimeSpan.FromMilliseconds(2500),
    TotalLogicalReads = 1256789,
    TotalPhysicalReads = 1245,
    TotalLogicalWrites = 567,
    RowsAffected = 1234,
    AverageRowsReturned = 890,
    MaxRowsReturned = 2345,
    TotalCpuTime = TimeSpan.FromMilliseconds(1850),
    TotalWaitTime = TimeSpan.FromMilliseconds(670),
    MostCommonWaitType = "PAGEIOLATCH_SH",
    PeakMemoryUsageMB = 256,
    AverageMemoryUsageMB = 189,
    LastCompilationTime = DateTime.UtcNow.AddDays(-1),
    IsCached = true,
    CacheKey = "SELECT_Users_Where_Status_Active",
    PlanHandle = 123456789,
    FirstExecution = DateTime.UtcNow.AddDays(-30),
    LastExecution = DateTime.UtcNow
};

// Check if query is inefficient
if (stats.IsInefficient)
{
    Console.WriteLine("⚠️ Query is inefficient!");
    Console.WriteLine(stats.GetPerformanceSummary());
}

// Get efficiency rating (0-100 scale)
double efficiency = stats.GetEfficiencyRating();
Console.WriteLine($"Efficiency rating: {efficiency:F1}%");

// Generate optimization recommendations
var recommendations = stats.GetOptimizationRecommendations();
foreach (var recommendation in recommendations)
{
    Console.WriteLine($"- {recommendation}");
}

// Display key metrics
Console.WriteLine($"Average execution time: {stats.AverageExecutionTime.TotalMilliseconds:F1}ms");
Console.WriteLine($"Average logical reads: {stats.AverageLogicalReads:N0}");
Console.WriteLine($"Average CPU time: {stats.AverageCpuTime.TotalMilliseconds:F1}ms");
Console.WriteLine($"Peak memory usage: {stats.PeakMemoryUsageMB}MB");
```

### Public Members

- `ExecutionCount` - Total number of times the query has been executed
- `TotalExecutionTime` - Sum of all execution times
- `AverageExecutionTime` - Average execution time per execution
- `MinimumExecutionTime` - Fastest execution time recorded
- `MaximumExecutionTime` - Slowest execution time recorded
- `TotalLogicalReads` - Total logical reads performed
- `TotalPhysicalReads` - Total physical reads performed
- `TotalLogicalWrites` - Total logical writes performed
- `AverageLogicalReads` - Average logical reads per execution
- `RowsAffected` - Total rows affected by the query
- `AverageRowsReturned` - Average rows returned per execution
- `MaxRowsReturned` - Maximum rows returned in a single execution
- `TotalCpuTime` - Total CPU time consumed
- `AverageCpuTime` - Average CPU time per execution
- `TotalWaitTime` - Total wait time due to resource contention
- `MostCommonWaitType` - Most frequent wait type observed
- `PeakMemoryUsageMB` - Peak memory usage in megabytes
- `AverageMemoryUsageMB` - Average memory usage in megabytes
- `LastCompilationTime` - When the query was last compiled
- `IsCached` - Whether the query plan is cached
- `CacheKey` - Cache key for the query plan
- `PlanHandle` - Unique identifier for the execution plan
- `FirstExecution` - When the query was first executed
- `LastExecution` - When the query was last executed
- `IsInefficient` - Calculated property indicating if query is inefficient
- `GetEfficiencyRating()` - Returns a performance rating (0-100)
- `GetPerformanceSummary()` - Generates a formatted performance summary string
- `GetOptimizationRecommendations()` - Returns list of optimization suggestions


## SqlInjectionDetectorExtensions

The `SqlInjectionDetectorExtensions` class provides extension methods for `SqlInjectionDetector` that enhance SQL injection vulnerability detection with filtering, grouping, and reporting capabilities. These methods help analyze, categorize, and generate comprehensive reports on detected vulnerabilities, making it easier to identify and prioritize security issues in SQL queries.

### Usage Example

```csharp
// Create SQL injection detector
var detector = new SqlInjectionDetector();

// Analyze a query for SQL injection vulnerabilities
var issues = detector.Analyze(
    "SELECT * FROM Users WHERE username = '" + userInput + "' AND password = '" + passwordInput + "'"
);

// Check if any critical or high severity issues were found
bool hasCriticalIssues = detector.HasCriticalIssues(issues);
Console.WriteLine($"Has critical issues: {hasCriticalIssues}");

// Filter issues by severity (only show Critical and High)
var highPriorityIssues = detector.FilterBySeverity(issues, "High");
Console.WriteLine($"High priority issues: {highPriorityIssues.Count}");

// Group issues by type to see which patterns are most common
var issuesByType = detector.GroupByType(issues);
foreach (var group in issuesByType)
{
    Console.WriteLine($"- {group.Key}: {group.Value.Count} issues");
}

// Generate a summary report
string summaryReport = detector.GenerateSummaryReport(issues);
Console.WriteLine(summaryReport);

// Generate a detailed report with line numbers
string detailedReport = detector.GenerateDetailedReport(
    issues, 
    "SELECT * FROM Users WHERE username = 'admin' -- OR '1'='1'"
);
Console.WriteLine(detailedReport);
```

### Public Members

- `FilterBySeverity(this SqlInjectionDetector detector, List<SqlInjectionIssue> issues, string minSeverity = "Medium")` - Filters detected vulnerabilities by severity level (Critical, High, Medium, Low)
- `GroupByType(this SqlInjectionDetector detector, List<SqlInjectionIssue> issues)` - Groups detected vulnerabilities by their type
- `GenerateSummaryReport(this SqlInjectionDetector detector, List<SqlInjectionIssue> issues)` - Generates a summary report of detected vulnerabilities with counts by severity
- `GenerateDetailedReport(this SqlInjectionDetector detector, List<SqlInjectionIssue> issues, string query)` - Generates a detailed analysis report with location information and line numbers
- `HasCriticalIssues(this SqlInjectionDetector detector, List<SqlInjectionIssue> issues)` - Checks if any critical or high severity vulnerabilities were detected


## AnalysisQueueProcessor

The `AnalysisQueueProcessor` class is a background worker that processes SQL query analysis requests from a queue asynchronously. It enables fire-and-forget analysis workflows, handles task persistence, implements retry logic, and provides progress tracking through a simple API. The processor manages a configurable number of concurrent tasks and tracks both queued and active tasks for monitoring purposes.

### Usage Example

```csharp
// Setup dependency injection (ASP.NET Core example)
services.AddSingleton<IQueryAnalyzerService, QueryAnalyzerService>();
services.AddSingleton<AnalysisQueueProcessor>();

// Create and configure the queue processor
var analyzerService = new QueryAnalyzerService();
var processor = new AnalysisQueueProcessor(analyzerService, logger, maxConcurrentTasks: 4);

// Start the processor to begin processing queued tasks
processor.Start();

// Enqueue analysis tasks (fire-and-forget)
string taskId1 = processor.EnqueueAnalysis(
    "SELECT * FROM Users WHERE Status = 'active'",
    result => Console.WriteLine($"Task completed with score: {result.PerformanceScore}")
);

string taskId2 = processor.EnqueueAnalysis(
    "SELECT u.Name, COUNT(o.Id) as OrderCount FROM Users u JOIN Orders o ON u.Id = o.UserId GROUP BY u.Name"
);

// Monitor task progress
var stats = processor.GetStatistics();
Console.WriteLine($"Queue status: {stats.QueuedCount} queued, {stats.ActiveCount}/{stats.MaxConcurrency} active");

// Get task status
var taskStatus = processor.GetTaskStatus(taskId1);
if (taskStatus != null)
{
    Console.WriteLine($"Task {taskStatus.TaskId} status: {taskStatus.Status}");
    if (taskStatus.Result != null)
    {
        Console.WriteLine($"Performance score: {taskStatus.Result.PerformanceScore}");
    }
}

// Gracefully stop the processor when shutting down
await processor.StopAsync(TimeSpan.FromSeconds(10));
```

### Public Members

- `AnalysisQueueProcessor(IQueryAnalyzerService analyzerService, ILogger<AnalysisQueueProcessor> logger, int maxConcurrentTasks = 5)` - Constructor that accepts analyzer service, logger, and maximum concurrent tasks
- `EnqueueAnalysis(string query, Action<QueryAnalysisResult>? onComplete = null)` - Enqueues a query for analysis and returns a task ID for tracking
- `Start()` - Starts the background processor to process queued tasks
- `StopAsync(TimeSpan timeout)` - Stops the processor gracefully, waiting for active tasks to complete
- `GetTaskStatus(string taskId)` - Gets the current status of a queued task
- `GetStatistics()` - Gets queue statistics including queued count, active count, max concurrency, and processing metrics
- `AnalysisTask` - Nested class representing a queued analysis task with properties:
  - `TaskId` - Unique identifier for the task
  - `Query` - The SQL query being analyzed
  - `Status` - Current task status (Queued, InProgress, Completed, Failed, Cancelled)
  - `CreatedAt` - When the task was created
  - `StartedAt` - When the task started processing
  - `CompletedAt` - When the task completed
  - `Result` - Analysis result if completed successfully
  - `ErrorMessage` - Error message if task failed
  - `OnComplete` - Optional callback invoked when task completes
  - `GetElapsedTime()` - Returns the time taken to process the task
- `QueueStatistics` - Class containing queue statistics with properties:
  - `QueuedCount` - Number of tasks waiting in queue
  - `ActiveCount` - Number of currently active tasks
  - `MaxConcurrency` - Maximum concurrent tasks allowed
  - `TotalProcessed` - Total number of tasks processed
  - `AverageProcessingTimeMs` - Average processing time in milliseconds
- `AnalysisTaskStatus` - Enum representing task status: Queued, InProgress, Completed, Failed, Cancelled

## ErrorHandlingMiddlewareExtensions

The `ErrorHandlingMiddlewareExtensions` class provides extension methods for `ErrorHandlingMiddleware` that enhance error handling capabilities with retry logic, error reporting, and fallback mechanisms. These methods help create robust error handling strategies for operations that may fail due to transient issues, enabling graceful degradation and recovery when possible.

### Usage Example

```csharp
// Setup dependency injection (ASP.NET Core example)
services.AddScoped<ErrorHandlingMiddleware>();

// Resolve the middleware instance
var middleware = serviceProvider.GetRequiredService<ErrorHandlingMiddleware>();

// Example 1: Execute with error handling (fire-and-forget with success tracking)
bool success = await middleware.ExecuteWithErrorHandlingAsync(
    async () => 
    {
        // Your operation here
        await Task.Delay(100);
        Console.WriteLine("Operation completed successfully");
    },
    "DatabaseBackupOperation"
);

Console.WriteLine($"Operation succeeded: {success}");

// Example 2: Create error report from exception
try
{
    // Some operation that might fail
    await Task.Delay(100);
}
catch (Exception ex)
{
    string errorMessage = middleware.FormatErrorMessage(
        ex,
        "DatabaseBackupService.BackupDatabase"
    );
    logger.LogError(errorMessage);
    
    var errorReport = middleware.CreateErrorReport(
        ex.Message,
        "DatabaseBackupService.BackupDatabase"
    );
    
    // Use errorReport for error tracking or monitoring
}

// Example 3: Execute with retry logic (3 attempts by default)
var result = await middleware.ExecuteWithRetryAsync(
    async () => 
    {
        // Your operation here
        return await databaseService.GetUserAsync(123);
    },
    "GetUserOperation",
    maxRetries: 5
);

Console.WriteLine($"Result: {result}");

// Example 4: Execute with cache fallback
var cachedUser = await middleware.ExecuteWithCacheFallbackAsync(
    async () => 
    {
        // Try to get fresh data
        return await databaseService.GetUserAsync(123);
    },
    () => 
    {
        // Fallback to cached data
        return cacheService.Get<User>("user_123");
    },
    "GetUserWithCacheFallback"
);

Console.WriteLine($"User: {cachedUser?.Name ?? "Not found"}");
```

### Public Members

- `ExecuteWithErrorHandlingAsync(this ErrorHandlingMiddleware middleware, Func<Task> action, string operationName)` - Executes an action with error handling and returns a boolean indicating success
- `CreateErrorReport(this ErrorHandlingMiddleware middleware, string errorMessage, string context)` - Creates an error report from a string message and context
- `ExecuteWithRetryAsync<T>(this ErrorHandlingMiddleware middleware, Func<Task<T>> operation, string operationName, int maxRetries = 3)` - Executes an operation with simple retry logic when it fails
- `FormatErrorMessage(this ErrorHandlingMiddleware middleware, Exception ex, string context)` - Creates a formatted error message string for logging or user display
- `ExecuteWithCacheFallbackAsync<T>(this ErrorHandlingMiddleware middleware, Func<Task<T>> operation, Func<T> cachedResultProvider, string operationName)` - Attempts to execute an operation with automatic degradation to a cached result

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

## PlanVisualization

The `PlanVisualization` class represents a visualization of a query optimization plan, including the hierarchical structure, cost distribution, detected bottlenecks, and statistical metrics. It provides methods to generate compact reports and string representations for debugging, logging, and display purposes.

This type is typically used in query analysis pipelines to present optimization recommendations, performance insights, and actionable suggestions to developers or monitoring systems.

### Usage Example

```csharp
// Create a visualization for a query optimization plan
var visualization = new PlanVisualization
{
    TextTree = @"
Root (Cost: 100.0%)
├── Filter (Cost: 45.0%)
│   ├── Table Scan [Users] (Cost: 30.0%)
│   └── Index Seek [IX_Users_Email] (Cost: 15.0%)
├── Join [INNER] (Cost: 35.0%)
│   ├── Table Scan [Orders] (Cost: 20.0%)
│   └── Index Seek [IX_Orders_UserId] (Cost: 15.0%)
└── Sort (Cost: 20.0%)
    └── Stream Aggregate (Cost: 10.0%)
    "",
    CostDistribution = @"{
  \"Filter\": 45.0,
  \"Join\": 35.0,
  \"Sort\": 20.0
}"",
    Bottlenecks = new List<BottleneckAnnotation>
    {
        new BottleneckAnnotation
        {
            NodeId = "Filter",
            Description = "High-cost filter operation",
            Severity = "High",
            Recommendation = "Add index on filtered column"
        },
        new BottleneckAnnotation
        {
            NodeId = "Sort",
            Description = "Expensive sorting operation",
            Severity = "Medium",
            Recommendation = "Consider adding ORDER BY columns to index"
        }
    },
    Stats = new Dictionary<string, object>
    {
        {"TotalCost", 1250.5},
        {"ExecutionTimeMs", 245},
        {"RowsProcessed", 15678},
        {"CriticalIssues", 2}
    },
    RenderedAt = DateTime.UtcNow,
    NodeId = "Root",
    NodeType = "Plan",
    ObjectName = "GetActiveCustomerOrders",
    EstimatedCost = 1250.5,
    Depth = 0,
    Recommendation = "Consider adding composite index on Users.Email and Orders.UserId"
};

// Generate a compact report
string compactReport = visualization.ToCompactReport();
Console.WriteLine(compactReport);

// Get string representation
string displayText = visualization.ToString();
Console.WriteLine(displayText);

// Access individual properties
Console.WriteLine($"Plan rendered at: {visualization.RenderedAt}");
Console.WriteLine($"Total bottlenecks: {visualization.Bottlenecks.Count}");
Console.WriteLine($"Cost distribution: {visualization.CostDistribution}");
```

### Public Members

- `TextTree` - Textual representation of the plan hierarchy/tree structure
- `CostDistribution` - JSON string representing cost distribution across plan nodes
- `Bottlenecks` - List of detected performance bottlenecks with recommendations
- `Stats` - Dictionary of statistical metrics and performance data
- `RenderedAt` - Timestamp when the visualization was generated
- `ToCompactReport()` - Generates a compact, human-readable report summary
- `NodeId` - Unique identifier for the plan node
- `NodeType` - Type of the node (e.g., "Plan", "Join", "Filter")
- `ObjectName` - Name of the query or object being analyzed
- `EstimatedCost` - Estimated cost of this plan node
- `Depth` - Depth level in the plan hierarchy
- `Recommendation` - Optimization recommendation for this specific node
- `ToString()` - Returns a formatted string representation of the visualization

## Index

The `Index` class represents a database index with comprehensive metadata and performance statistics. It captures index properties like type (clustered/nonclustered), uniqueness, column composition, storage metrics, usage statistics, and fragmentation levels. This type is essential for index analysis, health monitoring, and generating maintenance scripts.

### Usage Example

```csharp
// Create an index for the Users table
var userIndex = new Index
{
    IndexName = "IX_Users_Email",
    TableName = "Users",
    SchemaName = "dbo",
    IndexType = IndexType.Nonclustered,
    IsUnique = true,
    IsPrimaryKey = false,
    IsDisabled = false,
    IsFiltered = false,
    Columns = new List<IndexColumn>
    {
        new IndexColumn { ColumnName = "Email", KeyOrdinal = 1 },
        new IndexColumn { ColumnName = "CreatedAt", KeyOrdinal = 2 }
    },
    IncludeColumns = new List<string> { "Name", "Status" },
    SizeInBytes = 1572864, // 1.5MB
    PageCount = 192,
    FileGroup = "PRIMARY",
    FilterPredicate = null,
    UserSeeks = 12567,
    UserScans = 89,
    UserLookups = 45,
    UserUpdates = 234,
    LastUserSeekTime = 1234567890
};

// Generate maintenance scripts
Console.WriteLine($"Index qualified name: {userIndex.GetQualifiedName()}");
Console.WriteLine($"Column list: {userIndex.GetColumnList()}");
Console.WriteLine($"Include list: {userIndex.GetIncludeList()}");
Console.WriteLine($"Usage summary: {userIndex.GetUsageSummary()}");
Console.WriteLine($"Fragmentation status: {userIndex.GetFragmentationStatus()}");

// Generate CREATE INDEX script
string createScript = userIndex.GenerateCreateScript();
Console.WriteLine($"CREATE script:\n{createScript}");

// Generate REBUILD script for maintenance
string rebuildScript = userIndex.GenerateRebuildScript();
Console.WriteLine($"REBUILD script:\n{rebuildScript}");

// Check index health and maintenance needs
if (userIndex.IsFragmented)
{
    Console.WriteLine("⚠️ Index is fragmented and needs maintenance!");
}

if (userIndex.IsCandidateForRemoval)
{
    Console.WriteLine("🗑️ Index is a candidate for removal (unused and not critical)");
}
```

### Public Members

- `IndexId` - Unique identifier for the index (auto-generated GUID)
- `IndexName` - Name of the index
- `TableName` - Name of the table the index belongs to
- `SchemaName` - Schema name (defaults to "dbo")
- `IndexType` - Type of index (Clustered, Nonclustered, Unique, FullText, Spatial, Columnstore)
- `IsUnique` - Whether the index enforces uniqueness
- `IsPrimaryKey` - Whether this is a primary key index
- `IsDisabled` - Whether the index is disabled
- `IsFiltered` - Whether the index has a filter predicate
- `Columns` - List of index key columns with ordinal positions
- `IncludeColumns` - List of included columns for covering indexes
- `SizeInBytes` - Size of the index in bytes
- `PageCount` - Number of 8KB pages in the index
- `FileGroup` - Filegroup where the index is stored
- `FilterPredicate` - Filter condition for filtered indexes
- `UserSeeks` - Number of user seeks performed using this index
- `UserScans` - Number of user scans performed using this index
- `UserLookups` - Number of user lookups performed using this index
- `UserUpdates` - Number of updates performed on this index
- `LastUserSeekTime` - Last timestamp when this index was used for seeks
- `GetQualifiedName()` - Returns schema.table.index qualified name
- `GetColumnList()` - Returns formatted column list with sort directions
- `GetIncludeList()` - Returns INCLUDE clause for covering indexes
- `GetUsageSummary()` - Returns formatted usage statistics
- `GetFragmentationStatus()` - Returns human-readable fragmentation assessment
- `GenerateCreateScript()` - Generates CREATE INDEX statement
- `GenerateRebuildScript()` - Generates ALTER INDEX REBUILD statement
- `GenerateReorganizeScript()` - Generates ALTER INDEX REORGANIZE statement
- `IsUsed` - Whether the index has any usage
- `IsCandidateForRemoval` - Whether index should be considered for removal
- `IsFragmented` - Whether fragmentation exceeds 10%
- `IsValid()` - Validates required properties

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

## SampleQueryProvider

The `SampleQueryProvider` static class provides a comprehensive collection of sample SQL queries designed for testing, benchmarking, and educational purposes. It includes queries with various performance characteristics, anti-patterns, and complexity levels to help developers understand common SQL performance issues and optimization opportunities.

This provider is particularly useful for:
- Testing query analysis functionality
- Demonstrating performance anti-patterns
- Educational examples for SQL optimization workshops
- Benchmarking and performance testing
- Integration testing scenarios

### Usage Example

```csharp
// Get individual sample queries
string optimizedQuery = SampleQueryProvider.GetOptimizedQuery();
string selectStarQuery = SampleQueryProvider.GetSelectStarQuery();
string nPlusOneQuery = SampleQueryProvider.GetNPlusOneQuery();

// Get all samples as a dictionary
var allSamples = SampleQueryProvider.GetAllSamples();
foreach (var kvp in allSamples)
{
    Console.WriteLine($"Sample '{kvp.Key}': {kvp.Value.Substring(0, Math.Min(50, kvp.Value.Length))}...");
}

// Get a random sample for testing
string randomQuery = SampleQueryProvider.GetRandomSample();
Console.WriteLine($"Random query: {randomQuery}");

// Get samples grouped by issue type
var samplesByType = SampleQueryProvider.GetSamplesByIssueType();
foreach (var kvp in samplesByType)
{
    Console.WriteLine($"{kvp.Key} ({kvp.Value.Count} samples):");
    foreach (var query in kvp.Value.Take(3))
    {
        Console.WriteLine($"  - {query.Substring(0, Math.Min(40, query.Length))}...");
    }
}
```

### Public Members

- `GetOptimizedQuery()` - Returns a well-optimized query with high performance characteristics
- `GetSelectStarQuery()` - Returns a query using SELECT * (inefficient pattern)
- `GetNPlusOneQuery()` - Returns a query demonstrating N+1 access pattern
- `GetImplicitConversionQuery()` - Returns a query with implicit type conversion issue
- `GetNonSargableQuery()` - Returns a query with non-sargable predicate (function on column)
- `GetComplexJoinQuery()` - Returns a query with multiple JOIN operations
- `GetLeadingWildcardQuery()` - Returns a query with LIKE and leading wildcard (prevents index usage)
- `GetOrConditionQuery()` - Returns a query with OR condition (may prevent index usage)
- `GetSubqueryQuery()` - Returns a query using subqueries
- `GetDistinctQuery()` - Returns a query using DISTINCT
- `GetSimpleQuery()` - Returns a simple, well-structured query with no issues
- `GetAggregationQuery()` - Returns a query with aggregation functions
- `GetCteQuery()` - Returns a query using Common Table Expression (CTE)
- `GetVeryComplexQuery()` - Returns a very complex query with multiple CTEs and ranking
- `GetAllSamples()` - Returns all sample queries as a dictionary (key: sample name, value: query text)
- `GetRandomSample()` - Returns a randomly selected sample query
- `GetSamplesByIssueType()` - Returns sample queries grouped by issue type/categories
