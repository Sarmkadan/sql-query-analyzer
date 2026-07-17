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

## AnalysisRequestDtoExtensions

The `AnalysisRequestDtoExtensions` class provides essential utility methods for managing and validating `AnalysisRequestDto` objects, facilitating tasks such as context identification, configuration summary generation, and object cloning. It simplifies the handling of request metadata, ensuring consistent behavior across the analysis pipeline.

### Usage Example

```csharp
// Create a new analysis request
var request = new AnalysisRequestDto 
{ 
    QueryText = "SELECT * FROM Users", 
    AnalyzePlan = true,
    ApplicationName = "UserPortal" 
};

// Clone the request for a modified analysis
var clonedRequest = request.Clone();

// Generate context identifier for caching
string contextId = request.GetContextIdentifier();

// Verify analysis configuration
if (request.ShouldAnalyzePlan())
{
    Console.WriteLine("Plan analysis is enabled.");
}

// Display configuration summary
Console.WriteLine(request.GetConfigurationSummary());
```

### Public Members

- `GetContextIdentifier` - Creates a normalized identifier for the analysis context based on the request properties.
- `ShouldAnalyzePlan` - Determines whether the request should include plan analysis.
- `ShouldIncludeIndexSuggestions` - Determines whether index suggestions should be generated.
- `ShouldAnalyzeFragmentation` - Determines whether fragmentation analysis should be performed.
- `GetContextDisplayName` - Gets a display-friendly name for the analysis context.
- `GetConfigurationSummary` - Gets a summary of the analysis configuration flags.
- `Clone` - Creates a deep copy of the analysis request.

---

## CliApplicationHostExtensions


The `CliApplicationHostExtensions` class provides extension methods for the `CliApplicationHost` type, enhancing CLI functionality with common operations for result validation, metadata management, and query analysis utilities. It includes methods for accessing performance issues, checking for critical issues, managing metadata, retrieving performance scores, counting issues by severity, and accessing command-line arguments.

### Usage Example

```csharp
// Create a CLI application host instance
var host = new CliApplicationHost(
    query: "SELECT * FROM Users WHERE Status = 'active'",
    arguments: new CommandLineArguments(new[] { "--analyze-plan" }),
    result: new QueryAnalysisResult
    {
        PerformanceScore = 75.5,
        Issues = new List<PerformanceIssue>
        {
            new PerformanceIssue
            {
                Severity = IssueSeverity.Warning,
                Type = "SelectStar",
                Description = "Query uses SELECT * which retrieves all columns"
            },
            new PerformanceIssue
            {
                Severity = IssueSeverity.Critical,
                Type = "MissingWhere",
                Description = "Query has no WHERE clause"
            }
        }
    }
);

// Access performance issues
var issues = host.GetIssues();
Console.WriteLine($"Found {issues.Count()} issues");

// Check for critical issues
bool hasCritical = host.HasCriticalIssues();
Console.WriteLine($"Has critical issues: {hasCritical}");

// Manage metadata
host.SetMetadata("analysisId", Guid.NewGuid());
host.SetMetadata("userId", "user-123");
var analysisId = host.GetMetadata<Guid>("analysisId");
Console.WriteLine($"Analysis ID: {analysisId}");

// Get performance score as formatted string
string scoreString = host.GetPerformanceScoreString();
Console.WriteLine($"Performance Score: {scoreString}");

// Get issue counts by severity
var issueCounts = host.GetIssueCountsBySeverity();
foreach (var kvp in issueCounts)
{
    Console.WriteLine($"{kvp.Key}: {kvp.Value} issues");
}

// Check if analysis should continue
bool shouldContinue = host.ShouldContinueAnalysis();
Console.WriteLine($"Should continue: {shouldContinue}");

// Get the query text
string queryText = host.GetQueryText();
Console.WriteLine($"Query: {queryText}");

// Access command line arguments
var args = host.GetCommandLineArguments();
Console.WriteLine($"Arguments: {args}");
```

### Public Members

- `GetIssues` - Validates that the analysis result contains issues and returns an enumerable of performance issues
- `HasCriticalIssues` - Determines whether the analysis result has any critical issues
- `SetMetadata` - Adds or updates metadata with the specified key and value
- `GetMetadata<T>` - Gets metadata value by key or returns the default value if key doesn't exist
- `GetPerformanceScoreString` - Gets the performance score as a formatted string with invariant culture
- `GetIssueCountsBySeverity` - Gets the total number of issues grouped by severity level
- `ShouldContinueAnalysis` - Determines whether the analysis should continue based on the ShouldContinue flag
- `GetQueryText` - Gets the query text from the host's Query property
- `GetCommandLineArguments` - Gets the command line arguments from the host's Arguments property

---

## PerformanceIssueExtensions

The `PerformanceIssueExtensions` class provides extension methods for the `PerformanceIssue` type, facilitating the manipulation, filtering, and presentation of SQL performance issues. It includes methods for creating deep copies, generating human-readable impact and location descriptions, determining actionability, and grouping issues by their type and priority.

### Usage Example

```csharp
// Assuming you have a list of performance issues from the analyzer
// var issues = ...; 

// Filter actionable issues
var actionable = issues.FilterBySeverity(IssueSeverity.Warning)
                       .FilterByImpact(minImpact: 15.0);

// Order by priority
var sortedIssues = actionable.OrderByPriority();

// Group by issue type
var grouped = sortedIssues.GroupByIssueType();

foreach (var group in grouped)
{
    Console.WriteLine($"Issue Type: {group.Key}");
    foreach (var issue in group.Value)
    {
        // Get issue details
        Console.WriteLine($"  - {issue.GetPriorityLabel()} - {issue.GetImpactDescription()}");
        Console.WriteLine($"    Location: {issue.GetLocationInfo()}");
        
        if (issue.IsActionable()) {
            Console.WriteLine($"    Actionable! {issue.GetTimeIncreaseDescription()}");
        }
    }
}
```

### Public Members

- `DeepCopy` - Creates a deep copy of a performance issue
- `GetImpactDescription` - Gets a formatted string describing the performance impact
- `GetTimeIncreaseDescription` - Gets a formatted string describing the estimated time increase, if available
- `GetLocationInfo` - Gets a formatted string with location information (line, column, clause)
- `GetMetadataPairs` - Gets the metadata as a collection of key-value pairs
- `IsActionable` - Determines whether the issue is actionable based on severity and impact thresholds
- `GetIssueTypeLabel` - Gets the issue type as a string
- `GetPriorityLabel` - Gets the priority as a formatted string with emoji indicator
- `FilterBySeverity` - Filters a collection of performance issues by severity
- `FilterByImpact` - Filters a collection of performance issues by minimum impact percentage
- `OrderByPriority` - Orders a collection of performance issues by priority (descending)
- `GroupByIssueType` - Groups performance issues by their issue type
---

### Usage Example

```csharp
// Normalize SQL query whitespace
var query = "SELECT *\nFROM Users\r\nWHERE Status = 'active'";
var normalized = query.NormalizeSqlWhitespace();
Console.WriteLine(normalized);
// Output: "SELECT * FROM Users WHERE Status = 'active'"

// Remove SQL comments
var commentedQuery = "SELECT * FROM Users -- Get all users\nWHERE Status = 'active' /* active users only */";
var cleanQuery = commentedQuery.RemoveSqlComments();
Console.WriteLine(cleanQuery);
// Output: "SELECT * FROM Users\nWHERE Status = 'active' "

// Check for SQL keywords
var isKeyword = "SELECT".IsSqlKeyword();
Console.WriteLine(isKeyword); // Output: true

// Convert to snake_case
var pascalCase = "UserProfileSettings".ToSnakeCase();
Console.WriteLine(pascalCase); // Output: "user_profile_settings"

// Extract query type
var queryType = "SELECT * FROM Users".ExtractQueryType();
Console.WriteLine(queryType); // Output: "SELECT"

// Split query into statements
var multiQuery = "SELECT * FROM Users; INSERT INTO Logs VALUES (1); UPDATE Settings SET Value = 'test'";
var statements = multiQuery.SplitStatements();
foreach (var statement in statements) {
    Console.WriteLine(`Statement: {statement}`);
}
```

### Public Members

- `NormalizeSqlWhitespace` - Normalizes whitespace in SQL queries by replacing multiple whitespace characters with single spaces, normalizing line breaks, and trimming the result
- `RemoveSqlComments` - Removes both line comments (-- to end of line) and block comments (/* ... */) from SQL queries
- `Truncate` - Truncates a string to the specified maximum length, adding an ellipsis (...) if the string is longer
- `IsSqlKeyword` - Determines whether the specified word is a common SQL keyword
- `CapitalizeFirst` - Capitalizes the first character of the string
- `ToSnakeCase` - Converts a PascalCase or camelCase string to snake_case
- `CountOccurrences` - Counts the number of occurrences of a substring within a string
- `ContainsSuspiciousPatterns` - Checks if the query contains common SQL injection patterns
- `ExtractQueryType` - Extracts the query type (SELECT, INSERT, UPDATE, DELETE, CREATE, DROP, UNKNOWN) from a SQL query
- `SplitStatements` - Splits a SQL query into individual statements using semicolon as a delimiter
- `GetPosition` - Gets the line and column position for a given character index in the string

---

## QueryStatisticsExtensions

The `QueryStatisticsExtensions` class provides extension methods for the `QueryStatistics` type, enhancing its capability to analyze and present query execution performance. It includes methods for calculating formatted metrics, detecting potential performance issues like parameter sniffing, identifying bottlenecks, and generating performance trend indicators.

### Usage Example

```csharp
// Assuming you have a QueryStatistics object 'stats'
// var stats = ...;

// Get formatted average logical reads
var formattedReads = stats.GetAverageLogicalReadsFormatted();
Console.WriteLine($"Average Logical Reads: {formattedReads}");

// Calculate logical reads per second
var readsPerSec = stats.GetLogicalReadsPerSecond();
Console.WriteLine($"Reads per Second: {readsPerSec:F2}");

// Calculate CPU time per logical read ratio
var cpuPerRead = stats.GetCpuTimePerLogicalRead();
Console.WriteLine($"CPU Time per Logical Read: {cpuPerRead:F2}ms");

// Get a collection of all performance metrics
var metrics = stats.GetPerformanceMetrics();

// Detect potential parameter sniffing issues
bool isParameterSniffing = stats.HasPotentialParameterSniffing();
if (isParameterSniffing) {
    Console.WriteLine("Potential parameter sniffing detected!");
}

// Get performance trend indicator
string trend = stats.GetPerformanceTrendIndicator(previousEfficiencyRating: 85.0);
Console.WriteLine($"Trend: {trend}");

// Calculate total I/O cost
double ioCost = stats.GetTotalIoCost();
Console.WriteLine($"Total I/O Cost: {ioCost:F2}");

// Get a summary of the most significant bottlenecks
string bottleneck = stats.GetBottleneckSummary();
Console.WriteLine($"Bottleneck Summary: {bottleneck}");
```

### Public Members

- `GetAverageLogicalReadsFormatted` - Calculates the average logical reads per execution as a formatted string with thousands separator
- `GetLogicalReadsPerSecond` - Calculates the total logical reads per second across all executions
- `GetCpuTimePerLogicalRead` - Calculates the total CPU time per logical read ratio
- `GetPerformanceMetrics` - Gets a collection of performance metrics as key-value pairs for easy display or serialization
- `HasPotentialParameterSniffing` - Determines if the query execution pattern indicates a potential parameter sniffing issue
- `GetPerformanceTrendIndicator` - Gets a formatted performance trend indicator based on efficiency rating changes over time
- `GetTotalIoCost` - Calculates the total I/O cost as a weighted sum of logical reads, physical reads, and writes
- `GetBottleneckSummary` - Gets a summary of the most expensive execution metrics for bottleneck identification

---

## RateLimitingMiddlewareExtensions

The `RateLimitingMiddlewareExtensions` class provides extension methods for the `RateLimitingMiddleware` type, enabling fluent APIs for common rate limiting scenarios, monitoring, and system state inspection. It offers methods for acquiring rate limit slots, retrieving query statistics, calculating system load metrics, and generating system state summaries.

### Usage Example

```csharp
// Create and configure rate limiting middleware
var rateLimiter = new RateLimitingMiddleware(
    maxConcurrentRequests: 100,
    requestTimeout: TimeSpan.FromSeconds(30));

// Register rate limiting middleware in your application
services.AddSingleton(rateLimiter);

// Attempt to acquire a rate limit slot for a query
var queryHash = "SELECT_Users_Status_Active";
bool acquired = await rateLimiter.TryAcquireSlotAsync(queryHash);

if (acquired) {
    Console.WriteLine("Rate limit slot acquired successfully!");
    
    // Get statistics for all tracked queries
    var allStats = rateLimiter.GetAllQueryStats();
    Console.WriteLine($"Total tracked queries: {allStats.Count}");
    
    // Get system load metrics
    var load = rateLimiter.GetNormalizedLoad();
    Console.WriteLine($"System load: {load:P0}");
    
    var totalRequests = rateLimiter.GetTotalRequests();
    Console.WriteLine($"Total requests processed: {totalRequests}");
    
    var currentRate = rateLimiter.GetCurrentRequestRate();
    Console.WriteLine($"Current request rate: {currentRate:F2} req/s");
    
    // Get throttled queries (queries exceeding threshold)
    var throttled = rateLimiter.GetThrottledQueries(threshold: 100);
    Console.WriteLine($"Throttled queries: {throttled.Count}");
    
    // Get most active queries
    var activeQueries = rateLimiter.GetMostActiveQueries(count: 5);
    Console.WriteLine("Most active queries:");
    foreach (var query in activeQueries) {
        Console.WriteLine($"  - {query.QueryHash}: {query.TotalRequests} requests");
    }
    
    // Get system state summary
    var systemSummary = rateLimiter.GetSystemStateSummary();
    Console.WriteLine(systemSummary);
}
else {
    Console.WriteLine("Failed to acquire rate limit slot - timeout occurred");
}
```

### Public Members

- `TryAcquireSlotAsync` - Attempts to acquire a rate limit slot with a timeout, returning success status
- `GetAllQueryStats` - Gets rate limit statistics for all tracked queries as a read-only collection
- `GetNormalizedLoad` - Gets the current system load as a normalized value between 0 and 1
- `GetThrottledQueries` - Gets rate limit statistics for queries that exceed the throttling threshold
- `GetMostActiveQueries` - Gets the most active queries (highest request count) as a read-only collection
- `GetAverageRequestIntervalMs` - Gets the average request interval across all tracked queries in milliseconds
- `GetTotalRequests` - Gets the total number of requests across all tracked queries
- `GetCurrentRequestRate` - Gets the current request rate (requests per second) across all tracked queries
- `GetSystemStateSummary` - Gets a summary string representing the current system state

---

## DtoMapperJsonExtensions

The `DtoMapperJsonExtensions` class provides static extension methods for serializing and deserializing DTO types to and from JSON. It includes methods for converting AnalysisRequestDto, AnalysisResponseDto, PerformanceIssueDto, IndexSuggestionDto, BatchAnalysisRequestDto, BatchAnalysisResponseDto, IndexAnalysisRequestDto, and IndexAnalysisResponseDto objects to JSON strings and parsing them back from JSON, enabling easy storage and transmission of DTO data.

### Usage Example

```csharp
// Create an analysis request DTO
var request = new AnalysisRequestDto
{
    Query = "SELECT * FROM Users WHERE Status = 'active'",
    Options = new Dictionary<string, string> { { "format", "json" } }
};

// Serialize to JSON
string json = request.ToJson();
Console.WriteLine(json);

// Serialize with pretty printing for readability
string prettyJson = request.ToJson(indented: true);
File.WriteAllText("analysis_request.json", prettyJson);

// Deserialize from JSON
string jsonData = File.ReadAllText("analysis_request.json");
var deserializedRequest = DtoMapperJsonExtensions.FromJsonToAnalysisRequest(jsonData);

// Try to deserialize with error handling
if (DtoMapperJsonExtensions.TryFromJsonToAnalysisRequest(jsonData, out var result))
{
    Console.WriteLine("Successfully deserialized AnalysisRequestDto");
}

// Serialize an analysis response DTO
var response = new AnalysisResponseDto
{
    PerformanceScore = 85.5,
    Issues = new List<PerformanceIssueDto>(),
    IndexSuggestions = new List<IndexSuggestionDto>()
};

string responseJson = response.ToJson();
Console.WriteLine(responseJson);

// Deserialize response
var deserializedResponse = DtoMapperJsonExtensions.FromJsonToAnalysisResponse(responseJson);

// Serialize a batch analysis request
var batchRequest = new BatchAnalysisRequestDto
{
    Queries = new[] { "SELECT * FROM Users", "SELECT Name FROM Products" },
    MaxDegreeOfParallelism = 4
};

string batchJson = batchRequest.ToJson();
var batchDeserialized = DtoMapperJsonExtensions.FromJsonToBatchAnalysisRequest(batchJson);
```

### Public Members

- `ToJson(this AnalysisRequestDto value, bool indented = false)` - Serializes an AnalysisRequestDto to a JSON string, optionally formatted with indentation
- `FromJsonToAnalysisRequest(string json)` - Deserializes an AnalysisRequestDto from a JSON string
- `TryFromJsonToAnalysisRequest(string json, out AnalysisRequestDto? value)` - Attempts to deserialize an AnalysisRequestDto from a JSON string with error handling
- `ToJson(this AnalysisResponseDto value, bool indented = false)` - Serializes an AnalysisResponseDto to a JSON string, optionally formatted with indentation
- `FromJsonToAnalysisResponse(string json)` - Deserializes an AnalysisResponseDto from a JSON string
- `TryFromJsonToAnalysisResponse(string json, out AnalysisResponseDto? value)` - Attempts to deserialize an AnalysisResponseDto from a JSON string with error handling
- `ToJson(this PerformanceIssueDto value, bool indented = false)` - Serializes a PerformanceIssueDto to a JSON string, optionally formatted with indentation
- `FromJsonToPerformanceIssue(string json)` - Deserializes a PerformanceIssueDto from a JSON string
- `TryFromJsonToPerformanceIssue(string json, out PerformanceIssueDto? value)` - Attempts to deserialize a PerformanceIssueDto from a JSON string with error handling
- `ToJson(this IndexSuggestionDto value, bool indented = false)` - Serializes an IndexSuggestionDto to a JSON string, optionally formatted with indentation
- `FromJsonToIndexSuggestion(string json)` - Deserializes an IndexSuggestionDto from a JSON string
- `TryFromJsonToIndexSuggestion(string json, out IndexSuggestionDto? value)` - Attempts to deserialize an IndexSuggestionDto from a JSON string with error handling
- `ToJson(this BatchAnalysisRequestDto value, bool indented = false)` - Serializes a BatchAnalysisRequestDto to a JSON string, optionally formatted with indentation
- `FromJsonToBatchAnalysisRequest(string json)` - Deserializes a BatchAnalysisRequestDto from a JSON string
- `TryFromJsonToBatchAnalysisRequest(string json, out BatchAnalysisRequestDto? value)` - Attempts to deserialize a BatchAnalysisRequestDto from a JSON string with error handling
- `ToJson(this BatchAnalysisResponseDto value, bool indented = false)` - Serializes a BatchAnalysisResponseDto to a JSON string, optionally formatted with indentation
- `FromJsonToBatchAnalysisResponse(string json)` - Deserializes a BatchAnalysisResponseDto from a JSON string
- `TryFromJsonToBatchAnalysisResponse(string json, out BatchAnalysisResponseDto? value)` - Attempts to deserialize a BatchAnalysisResponseDto from a JSON string with error handling
- `ToJson(this IndexAnalysisRequestDto value, bool indented = false)` - Serializes an IndexAnalysisRequestDto to a JSON string, optionally formatted with indentation
- `FromJsonToIndexAnalysisRequest(string json)` - Deserializes an IndexAnalysisRequestDto from a JSON string
- `TryFromJsonToIndexAnalysisRequest(string json, out IndexAnalysisRequestDto? value)` - Attempts to deserialize an IndexAnalysisRequestDto from a JSON string with error handling

---

## ReportGenerator

The `ReportGenerator` class provides static methods for generating various report formats from SQL query analysis results. It supports text, CSV, JSON, and HTML output formats, making it easy to integrate analysis results into different reporting workflows and tools. Reports include performance metrics, detected issues, and index suggestions with severity assessments and optimization potential.

### Usage Example

```csharp
// Analyze a SQL query using the analyzer service
var analyzer = new QueryAnalyzerService();
var result = await analyzer.AnalyzeQueryAsync(
    "SELECT u.Name, COUNT(o.Id) as OrderCount FROM Users u LEFT JOIN Orders o ON u.Id = o.UserId WHERE u.Status = 'active' GROUP BY u.Name HAVING COUNT(o.Id) > 5 ORDER BY OrderCount DESC");

// Generate a formatted text report for console output
var textReport = ReportGenerator.GenerateTextReport(result);
Console.WriteLine(textReport);

// Generate a CSV report for data export
var csvReport = ReportGenerator.GenerateCsvReport(new List<QueryAnalysisResult> { result });
File.WriteAllText("analysis_results.csv", csvReport);

// Generate a JSON report for API responses
var jsonReport = ReportGenerator.GenerateJsonReport(result);
Console.WriteLine(jsonReport);

// Generate an HTML report for web dashboard integration
var htmlReport = ReportGenerator.GenerateHtmlReport(result);
File.WriteAllText("report.html", htmlReport);

// Generate an executive summary for quick insights
var summary = ReportGenerator.GenerateSummary(result);
Console.WriteLine(`Summary: {summary}`);
```

### Public Members

- `GenerateTextReport(QueryAnalysisResult analysis)` - Generates a formatted text report with performance metrics, issues, and index suggestions
- `GenerateCsvReport(List<QueryAnalysisResult> analyses)` - Generates a CSV report for batch analysis results with columns for key metrics
- `GenerateJsonReport(QueryAnalysisResult analysis)` - Generates a JSON report suitable for API responses and programmatic consumption
- `GenerateHtmlReport(QueryAnalysisResult analysis)` - Generates a styled HTML report for web dashboard integration
- `GenerateSummary(QueryAnalysisResult analysis)` - Generates a concise one-line executive summary with key metrics

---

## SqlPatternAnalyzer

The `SqlPatternAnalyzer` class provides static utility methods for detecting common SQL performance anti-patterns and analyzing query structure. It uses source-generated regular expressions for optimal performance and frozen collections for fast keyword lookups. The analyzer can identify issues like SELECT *, missing WHERE/LIMIT clauses, implicit joins, non-sargable predicates, N+1 patterns, and provides optimization recommendations with a readability score.




### Usage Example

```csharp
// Analyze a SQL query for common performance issues
var query = @"
SELECT u.*, o.OrderId, COUNT(i.ItemId) as ItemCount
FROM Users u, Orders o, OrderItems i
WHERE u.UserId = o.UserId
    AND o.OrderId = i.OrderId
    AND u.Status = 'active'
    AND o.OrderDate > '2024-01-01'
GROUP BY u.UserId, o.OrderId
HAVING COUNT(i.ItemId) > 5
ORDER BY ItemCount DESC
";

// Detect common anti-patterns
bool hasSelectStar = SqlPatternAnalyzer.HasSelectStar(query);
bool hasImplicitJoin = SqlPatternAnalyzer.HasImplicitJoin(query);
bool hasMissingWhere = SqlPatternAnalyzer.HasMissingWhereClause(query);
bool hasFunctionOnColumn = SqlPatternAnalyzer.HasFunctionOnColumn(query);
bool hasLeadingWildcardLike = SqlPatternAnalyzer.HasLeadingWildcardLike(query);

// Extract structural information
var tables = SqlPatternAnalyzer.ExtractTablesFromQuery(query);
var cteNames = SqlPatternAnalyzer.ExtractCteNames(query);
var whereClause = SqlPatternAnalyzer.ExtractWhereClause(query);
var joinConditions = SqlPatternAnalyzer.ExtractJoinConditions(query);

// Count complexity metrics
int orConditions = SqlPatternAnalyzer.CountOrConditions(query);
int unionCount = SqlPatternAnalyzer.CountUnion(query);
int caseStatements = SqlPatternAnalyzer.CountCaseStatements(query);
int parentheses = SqlPatternAnalyzer.CountParentheses(query);
bool hasAggregate = SqlPatternAnalyzer.HasAggregateFunction(query);
bool hasWindow = SqlPatternAnalyzer.HasWindowFunction(query);
bool hasSubquery = SqlPatternAnalyzer.HasSubquery(query);
bool hasDistinctWithoutOrder = SqlPatternAnalyzer.HasDistinctWithoutOrder(query);

// Calculate performance score
double readabilityScore = SqlPatternAnalyzer.CalculateReadabilityScore(query);

// Generate optimization recommendations
var recommendations = SqlPatternAnalyzer.GenerateOptimizationRecommendations(query);

Console.WriteLine($"Tables found: {string.Join(", ", tables)}");
Console.WriteLine($"CTE names: {string.Join(", ", cteNames)}");
Console.WriteLine($"Readability score: {readabilityScore:F1}/100");
Console.WriteLine($"\nOptimization recommendations:");
foreach (var recommendation in recommendations)
{
    Console.WriteLine($"- {recommendation}");
}
```

### Public Members

- `DetectNPlusOnePattern(List<string> queries)` - Detects N+1 query patterns by analyzing multiple queries for repeated table access
- `ExtractCteNames(string query)` - Extracts CTE alias names declared in WITH clauses
- `ExtractTablesFromQuery(string query)` - Extracts table names from query (excluding CTE aliases)
- `HasMissingWhereClause(string query)` - Detects SELECT queries without WHERE clauses
- `HasSelectStar(string query)` - Detects SELECT * patterns
- `HasLeadingWildcardLike(string query)` - Detects LIKE patterns with leading wildcards (non-sargable)
- `HasFunctionOnColumn(string query)` - Detects functions applied to columns in WHERE clauses
- `HasImplicitJoin(string query)` - Detects implicit (comma-separated) JOINs
- `HasDistinctWithoutOrder(string query)` - Detects DISTINCT without ORDER BY
- `CountOrConditions(string query)` - Counts OR conditions in WHERE clauses
- `HasSubquery(string query)` - Detects subquery patterns
- `CountUnion(string query)` - Counts UNION operations
- `ExtractJoinConditions(string query)` - Extracts JOIN condition strings
- `ExtractWhereClause(string query)` - Extracts the WHERE clause text
- `CountCaseStatements(string query)` - Counts CASE statement occurrences
- `HasAggregateFunction(string query)` - Detects aggregate functions (SUM, COUNT, etc.)
- `HasWindowFunction(string query)` - Detects window functions (OVER clauses)
- `CalculateReadabilityScore(string query)` - Calculates a readability score (0-100) based on detected patterns
- `CountParentheses(string query)` - Counts maximum parenthesis nesting depth
- `GenerateOptimizationRecommendations(string query)` - Generates optimization recommendations based on detected patterns

---

## SqlInjectionDetector

The `SqlInjectionDetector` class analyzes SQL queries for potential SQL injection vulnerabilities by detecting common injection patterns such as string concatenation, dynamic WHERE clause construction, comment injection, UNION-based attacks, time-based attacks, and boolean-based blind injection. It returns a list of `SqlInjectionIssue` objects with severity assessments and location information for each detected vulnerability.

This detector is useful for security analysis of dynamically generated SQL queries and can be integrated into CI/CD pipelines or security scanning tools to identify injection risks before code reaches production.

### Usage Example

```csharp
// Create SqlInjectionDetector instance with dependency injection
var services = new ServiceCollection();
services.AddLogging();
services.AddSingleton<SqlInjectionDetector>();

var serviceProvider = services.BuildServiceProvider();
var detector = serviceProvider.GetRequiredService<SqlInjectionDetector>();

// Analyze a query for SQL injection vulnerabilities
var query = @"SELECT * FROM Users WHERE Username = '" + userInput + @"' AND Status = 'active' ";
var issues = detector.DetectVulnerabilities(query);

if (issues.Any())
{
    Console.WriteLine($"⚠️ Found {issues.Count} potential SQL injection vulnerabilities:");
    foreach (var issue in issues.OrderByDescending(i => i.Severity))
    {
        Console.WriteLine($"- [{issue.Severity}] {issue.Type} at position {issue.Location}: {issue.Description}");
        Console.WriteLine($"  Pattern: {issue.Pattern}");
    }
}
else
{
    Console.WriteLine("✓ No SQL injection patterns detected");
}

// Access individual issue properties
foreach (var issue in issues)
{
    Console.WriteLine($"Issue Type: {issue.Type}");
    Console.WriteLine($"Severity: {issue.Severity}");
    Console.WriteLine($"Location: {issue.Location}");
    Console.WriteLine($"Description: {issue.Description}");
    Console.WriteLine($"Pattern: {issue.Pattern}");
    Console.WriteLine($"ToString(): {issue}");
}
```

### Public Members

- `SqlInjectionDetector(ILogger<SqlInjectionDetector> logger)` - Initializes the detector with logging support
- `DetectVulnerabilities(string query)` - Analyzes a SQL query and returns a list of detected SQL injection issues
- `List<SqlInjectionIssue> DetectVulnerabilities` - The list of detected SQL injection issues
- `Type` - The type of vulnerability detected (e.g., "String Concatenation", "UNION-based Injection")
- `Severity` - The severity level (Critical, High, Medium, Low)
- `Location` - The character position in the query where the pattern was detected
- `Pattern` - The actual pattern matched in the query
- `Description` - Human-readable description of the vulnerability
- `ToString()` - Returns a formatted string representation of the issue

---

## AnalyzerHealthCheck

The `AnalyzerHealthCheck` type performs health checks and self-healing attempts on components. It provides a `CheckHealthAsync` method to run a health check, an `AttemptSelfHealAsync` method to attempt self-healing, and exposes properties for the check time, status, cache health, rate limiter health, metrics health, database health, errors, and actions performed.

### Usage Example

```csharp
// Create an AnalyzerHealthCheck instance
var healthCheck = new AnalyzerHealthCheck("SqlQueryAnalyzer");

// Check health status
var healthResult = await healthCheck.CheckHealthAsync(CancellationToken.None);
Console.WriteLine($"Health Status: {healthResult.Status}");
Console.WriteLine($"Check Time: {healthResult.CheckTime}");

// Attempt self-healing if needed
if (healthResult.Status != HealthStatus.Healthy)
{
    var selfHealResult = await healthCheck.AttemptSelfHealAsync(CancellationToken.None);
    Console.WriteLine($"Self-heal successful: {selfHealResult.Success}");
    Console.WriteLine($"Actions performed: {string.Join(", ", selfHealResult.ActionsPerformed)}");
}

// Access component health details
Console.WriteLine($"Cache Health: {healthCheck.CacheHealth.Status}");
Console.WriteLine($"Rate Limiter Health: {healthCheck.RateLimiterHealth.Status}");
Console.WriteLine($"Metrics Health: {healthCheck.MetricsHealth.Status}");
Console.WriteLine($"Database Health: {healthCheck.DatabaseHealth.Status}");

if (healthCheck.Errors.Any())
{
    Console.WriteLine("Errors detected:");
    foreach (var error in healthCheck.Errors)
    {
        Console.WriteLine($"- {error}");
    }
}
```

### Public Members

- `AnalyzerHealthCheck(string component)` - Initializes a new health check for the specified component
- `CheckHealthAsync(CancellationToken cancellationToken)` - Runs a health check and returns a HealthCheckResult
- `AttemptSelfHealAsync(CancellationToken cancellationToken)` - Attempts to self-heal any unhealthy components
- `DateTime CheckTime` - The timestamp when the health check was performed
- `HealthStatus Status` - Overall health status (Healthy, Degraded, Unhealthy)
- `ComponentHealth CacheHealth` - Health status of the cache component
- `ComponentHealth RateLimiterHealth` - Health status of the rate limiter component
- `ComponentHealth MetricsHealth` - Health status of the metrics component
- `ComponentHealth DatabaseHealth` - Health status of the database component
- `List<string> Errors` - List of error messages encountered during health check
- `override string ToString()` - Returns a formatted string representation of the health check

## IResultFormatter

The `IResultFormatter` interface defines the contract for formatting SQL query analysis results into various output formats. It provides a standardized way to serialize analysis results for different consumption scenarios including console output, API responses, data export, and web dashboard integration. Implementations handle format-specific serialization logic while maintaining a consistent API surface.

### Usage Example

```csharp
// Create formatters for different output formats
var jsonFormatter = new JsonResultFormatter(prettyPrint: true);
var csvFormatter = new CsvResultFormatter();
var xmlFormatter = new XmlResultFormatter();
var textFormatter = new TextResultFormatter();
var htmlFormatter = new HtmlResultFormatter();

// Analyze a SQL query using the analyzer service
var analyzer = new QueryAnalyzerService();
var result = await analyzer.AnalyzeQueryAsync(
    "SELECT u.Name, COUNT(o.Id) as OrderCount FROM Users u LEFT JOIN Orders o ON u.Id = o.UserId WHERE u.Status = 'active' GROUP BY u.Name HAVING COUNT(o.Id) > 5 ORDER BY OrderCount DESC");

// Format the result using different formatters
var jsonOutput = jsonFormatter.Format(result);
Console.WriteLine(jsonOutput);

var csvOutput = csvFormatter.Format(result);
File.WriteAllText("analysis_results.csv", csvOutput);

var xmlOutput = xmlFormatter.Format(result);
Console.WriteLine(xmlOutput);

var textOutput = textFormatter.Format(result);
Console.WriteLine(textOutput);

var htmlOutput = htmlFormatter.Format(result);
File.WriteAllText("report.html", htmlOutput);

// Get format type identifiers
Console.WriteLine($"JSON format type: {jsonFormatter.GetFormatType()}");
Console.WriteLine($"CSV format type: {csvFormatter.GetFormatType()}");
Console.WriteLine($"XML format type: {xmlFormatter.GetFormatType()}");
Console.WriteLine($"Text format type: {textFormatter.GetFormatType()}");
Console.WriteLine($"HTML format type: {htmlFormatter.GetFormatType()}");

// Format multiple results as a batch
var batchResults = new List<QueryAnalysisResult> { result, /* additional results */ };
var jsonBatchOutput = jsonFormatter.FormatBatch(batchResults);
var csvBatchOutput = csvFormatter.FormatBatch(batchResults);
```

### Public Members

- `string Format(QueryAnalysisResult result)` - Formats a single query analysis result into string representation for the specific output format
- `string FormatBatch(IEnumerable<QueryAnalysisResult> results)` - Formats multiple query analysis results into string representation, useful for batch analysis output
- `string GetFormatType()` - Returns the format type identifier (e.g., "json", "csv", "xml", "text", "html")

---

## ComponentHealth

The `ComponentHealth` type represents the health status of a specific component with a status and message.

### Public Members

- `Status` - The health status (Healthy, Degraded, Unhealthy)
- `Message` - A descriptive message about the component's health

## HealthCheckResult

The `HealthCheckResult` type represents the result of a health check operation.

### Public Members

- `Status` - The overall health status
- `CheckTime` - When the health check was performed
- `Component` - The component being checked
- `Errors` - List of error messages

## SelfHealResult

The `SelfHealResult` type represents the outcome of a self-healing attempt.

### Public Members

- `Success` - Whether the self-healing was successful
- `ActionsPerformed` - List of actions performed during self-healing
- `Error` - Error message if the self-healing failed


## ValidationRuleEngine

The `ValidationRuleEngine` class provides a centralized mechanism for validating SQL queries against a set of registered validation rules. It maintains a collection of validation rules and provides methods to register new rules, validate queries against all registered rules, and retrieve validation results including errors and warnings. The engine supports both synchronous and asynchronous validation workflows and provides detailed feedback about validation failures.

### Usage Example

```csharp
// Create a new validation rule engine instance
var validationEngine = new ValidationRuleEngine();

// Register custom validation rules
validationEngine.RegisterRule(new SelectStarRule());
validationEngine.RegisterRule(new MissingWhereClauseRule());
validationEngine.RegisterRule(new ImplicitJoinRule());

// Validate a SQL query synchronously
var query = "SELECT * FROM Users u, Orders o WHERE u.Id = o.UserId";
var validationResult = validationEngine.ValidateQuery(query);

Console.WriteLine($"Validation successful: {validationResult.IsValid}");
Console.WriteLine($"Total rules registered: {validationEngine.GetRuleCount()}");
Console.WriteLine($"Errors found: {validationResult.Errors.Count}");
Console.WriteLine($"Warnings found: {validationResult.Warnings.Count}");

if (!validationResult.IsValid)
{
    Console.WriteLine("\nValidation errors:");
    foreach (var error in validationResult.Errors)
    {
        Console.WriteLine($"- {error}");
    }
}

if (validationResult.Warnings.Any())
{
    Console.WriteLine("\nValidation warnings:");
    foreach (var warning in validationResult.Warnings)
    {
        Console.WriteLine($"- {warning}");
    }
}

// Validate a query asynchronously
var asyncResult = await validationEngine.ValidateQueryAsync(query);

// Validate using the generic Validate method with rule-specific results
var ruleResult = validationEngine.Validate<SelectStarRule>(query);
Console.WriteLine($"Select* rule detected: {ruleResult.IsValid}");
Console.WriteLine($"Rule-specific errors: {ruleResult.Errors.Count}");

// Check overall engine status
Console.WriteLine($"\nEngine status: {validationEngine}");
```

### Public Members

- `ValidationRuleEngine()` - Initializes a new validation rule engine with an empty rule collection
- `ValidateQuery(string query)` - Validates a SQL query against all registered rules synchronously
- `ValidateQueryAsync(string query, CancellationToken cancellationToken = default)` - Validates a SQL query against all registered rules asynchronously
- `RegisterRule(IValidationRule rule)` - Registers a new validation rule with the engine
- `GetRuleCount()` - Returns the total number of registered validation rules
- `Validate<T>(string query)` - Validates a query using a specific rule type and returns rule-specific results
- `RuleValidationResult Validate<T>(string query)` - Rule-specific validation result containing errors and warnings for a specific rule type
- `RuleValidationResult Validate(string query, IValidationRule rule)` - Validates a query using a specific rule instance
- `bool IsValid` - Indicates whether the last validation was successful
- `List<string> Errors` - List of error messages from the last validation
- `List<string> Warnings` - List of warning messages from the last validation
- `bool IsValid` (in RuleValidationResult) - Indicates whether validation with this specific rule was successful
- `List<string> Errors` (in RuleValidationResult) - List of error messages from rule-specific validation
- `List<string> Warnings` (in RuleValidationResult) - List of warning messages from rule-specific validation
- `override string ToString()` - Returns a string representation of the engine showing rule count and validation status

---

## BadQueryFixturesTests

The `BadQueryFixturesTests` class contains correctness tests driven by SQL fixture files that verify the detection rules in `SqlPatternAnalyzer` correctly identify common SQL performance anti-patterns. Each test loads a known-bad query pattern from the `fixtures/` directory and asserts that the corresponding detection rule fires as expected. The `CleanFixture_DoesNotTripBadQueryRules` test verifies that a properly-structured query does not trigger any of the bad query rules.

### Usage Example

```csharp
// Create test instance
var fixturesTests = new BadQueryFixturesTests();

// Test that SELECT * pattern is detected
fixturesTests.SelectStarFixture_TripsSelectStarRule();

// Test that non-sargable predicates (functions on columns, leading wildcard LIKE) are detected
fixturesTests.MissingIndexFixture_TripsNonSargablePredicateRules();

// Test that implicit (cartesian) joins are detected
fixturesTests.CartesianJoinFixture_TripsImplicitJoinRule();

// Test that N+1 query patterns are detected
fixturesTests.NPlusOneFixture_TripsNPlusOneRule();

// Test that queries without WHERE clauses are detected
fixturesTests.MissingWhereFixture_TripsMissingWhereRule();

// Test that all bad fixtures produce at least one optimization recommendation
fixturesTests.EveryBadFixture_ProducesAtLeastOneRecommendation();

// Test that clean queries don't trigger any bad query rules
fixturesTests.CleanFixture_DoesNotTripBadQueryRules();
```

### Public Members

- `SelectStarFixture_TripsSelectStarRule()` - Verifies that the SELECT * detection rule correctly identifies queries with SELECT * patterns
- `MissingIndexFixture_TripsNonSargablePredicateRules()` - Verifies that non-sargable predicate detection rules (functions on columns and leading wildcard LIKE) correctly identify problematic patterns
- `CartesianJoinFixture_TripsImplicitJoinRule()` - Verifies that implicit (comma-separated) JOIN detection works correctly
- `NPlusOneFixture_TripsNPlusOneRule()` - Verifies that N+1 query pattern detection works correctly
- `MissingWhereFixture_TripsMissingWhereRule()` - Verifies that missing WHERE clause detection works correctly
- `EveryBadFixture_ProducesAtLeastOneRecommendation()` - Verifies that all known-bad fixtures produce optimization recommendations
- `CleanFixture_DoesNotTripBadQueryRules()` - Verifies that properly-structured queries don't trigger any bad query detection rules


---

## SqlQueryAnalyzerExceptionExtensions

The `SqlQueryAnalyzerExceptionExtensions` class provides extension methods for exception handling, formatting, and analysis of SQL Query Analyzer exceptions. It includes methods for creating formatted error messages, checking exception types, extracting error information, and generating detailed reports. These utilities help with consistent error handling, logging, and user-friendly error messages across the application.

### Usage Example

```csharp
// Example 1: Create a formatted error message for display
try
{
    var analyzer = new QueryAnalyzerService();
    await analyzer.AnalyzeQueryAsync("SELECT * FROM Users");
}
catch (SqlQueryAnalyzerException ex)
{
    // Format error message for user display
    var errorMessage = ex.ToErrorMessage();
    Console.WriteLine(errorMessage);
    
    // Check if it's a critical error
    if (ex.IsCriticalError())
    {
        Console.WriteLine("Critical error detected - halting processing");
    }
}

// Example 2: Extract error information for logging
try
{
    var analyzer = new QueryAnalyzerService();
    await analyzer.AnalyzeQueryAsync(invalidQuery);
}
catch (SqlQueryAnalyzerException ex)
{
    // Get error code for categorization
    var errorCode = ex.GetErrorCode();
    Console.WriteLine($"Error Code: {errorCode}");
    
    // Get error details
    var errorDetails = ex.GetErrorDetails();
    Console.WriteLine($"Details: {errorDetails}");
    
    // Check exception type
    if (ex.IsQueryValidationError())
    {
        Console.WriteLine("Query validation error detected");
    }
    else if (ex.IsDatabaseConnectionError())
    {
        Console.WriteLine("Database connection error detected");
    }
    else if (ex.IsQueryPlanError())
    {
        Console.WriteLine("Query plan error detected");
    }
}

// Example 3: Generate detailed exception report for debugging
try
{
    var analyzer = new QueryAnalyzerService();
    await analyzer.AnalyzeQueryAsync("SELECT * FROM LargeTable");
}
catch (SqlQueryAnalyzerException ex)
{
    // Generate comprehensive report for debugging
    var report = ex.GenerateExceptionReport();
    File.WriteAllText("exception_report.txt", report);
    
    // Create user-friendly summary for logging
    var summary = ex.ToUserFriendlySummary();
    _logger.LogError(summary);
}

// Example 4: Handle different exception types
try
{
    var analyzer = new QueryAnalyzerService();
    await analyzer.AnalyzeQueryAsync(query);
}
catch (InvalidQueryException ex) when (!string.IsNullOrEmpty(ex.Query))
{
    Console.WriteLine($"Invalid query at line {ex.LineNumber}: {ex.Query}");
    Console.WriteLine(ex.ToUserFriendlySummary());
}
catch (DatabaseConnectionException ex)
{
    Console.WriteLine($"Database connection failed: {ex.DatabaseName}");
    if (ex.IsCriticalError())
    {
        Environment.Exit(1);
    }
}
catch (SqlQueryAnalyzerException ex)
{
    Console.WriteLine(ex.ToErrorMessage());
}
```

### Public Members

- `ToErrorMessage(this SqlQueryAnalyzerException exception)` - Creates a formatted error message from the exception, including error code and details
- `IsQueryValidationError(this SqlQueryAnalyzerException exception)` - Determines if the exception represents a query validation error
- `IsDatabaseConnectionError(this SqlQueryAnalyzerException exception)` - Determines if the exception represents a database connection error
- `IsQueryPlanError(this SqlQueryAnalyzerException exception)` - Determines if the exception represents a query plan analysis error
- `GetErrorCode(this SqlQueryAnalyzerException exception)` - Safely extracts the error code from the exception if available
- `GetErrorDetails(this SqlQueryAnalyzerException exception)` - Safely extracts the error details from the exception if available
- `ToUserFriendlySummary(this SqlQueryAnalyzerException exception, bool includeStackTrace = false)` - Creates a user-friendly error summary suitable for logging or display
- `IsCriticalError(this SqlQueryAnalyzerException exception)` - Determines if the exception is a critical error that should halt processing
- `GenerateExceptionReport(this SqlQueryAnalyzerException exception)` - Creates a detailed exception report with all available information

---



---

## QueryNormalizerBenchmarksExtensions

The `QueryNormalizerBenchmarksExtensions` class provides extension methods for the `QueryNormalizerBenchmarks` type that simplify common benchmark scenarios. These methods combine setup and normalization operations into convenient one-call methods, making it easier to write realistic benchmarks without boilerplate code.

### Usage Example

```csharp
// Create a benchmarks instance
var benchmarks = new QueryNormalizerBenchmarks();

// Run setup and normalize a simple query
var simpleNormalized = benchmarks.RunSetupAndNormalizeSimple();
Console.WriteLine(simpleNormalized);

// Run setup and normalize a complex query with multiple JOINs
var complexNormalized = benchmarks.RunSetupAndNormalizeComplex();
Console.WriteLine(complexNormalized);

// Run setup and normalize a query while preserving string literals
var literalNormalized = benchmarks.RunSetupAndNormalizeWithLiterals();
Console.WriteLine(literalNormalized);

// Extract table names from a complex query
var tableNames = benchmarks.GetTableNames();
Console.WriteLine($"Tables found: {string.Join(", ", tableNames)}");

// Extract column names from a complex query
var columnNames = benchmarks.GetColumnNames();
Console.WriteLine($"Columns found: {string.Join(", ", columnNames)}");
```

### Public Members

- `RunSetupAndNormalizeSimple(this QueryNormalizerBenchmarks benchmarks)` - Executes the benchmark Setup step and then runs the simple normalization routine, returning the normalized SQL string
- `RunSetupAndNormalizeComplex(this QueryNormalizerBenchmarks benchmarks)` - Executes the benchmark Setup step and then runs the complex normalization routine with multiple JOINs, returning the normalized SQL string
- `RunSetupAndNormalizeWithLiterals(this QueryNormalizerBenchmarks benchmarks)` - Executes the benchmark Setup step and then runs the normalization routine that preserves literals, returning the normalized SQL string
- `GetTableNames(this QueryNormalizerBenchmarks benchmarks)` - Retrieves the table names extracted from a complex query as a read-only collection
- `GetColumnNames(this QueryNormalizerBenchmarks benchmarks)` - Retrieves the column names extracted from a complex query as a read-only collection

---

## QueryAnalysisPipelineBenchmarksExtensions

The `QueryAnalysisPipelineBenchmarksExtensions` class provides extension methods for the `QueryAnalysisPipelineBenchmarks` type that generate realistic SQL queries and perform common analysis operations. These methods are designed for benchmarking various SQL query parsing and analysis scenarios including CTEs, window functions, parameterized queries, join conditions, subqueries, CASE expressions, and date/time functions.

### Usage Example

```csharp
// Create a benchmarks instance
var benchmarks = new QueryAnalysisPipelineBenchmarks();

// Parse a query with Common Table Expressions (CTEs)
var cteQuery = benchmarks.ParseWithCteQuery();
Console.WriteLine($"CTE query parsed successfully: {cteQuery.Tables.Count} tables found");

// Parse a query with window functions
var windowQuery = benchmarks.ParseWithWindowFunctionsQuery();
Console.WriteLine($"Window functions query parsed: {windowQuery.Tables.Count} tables found");

// Hash a parameterized query for caching
var parameterizedHash = benchmarks.HashParameterizedQuery();
Console.WriteLine($"Parameterized query hash: {parameterizedHash}");

// Extract all join conditions from a complex query
var joinConditions = benchmarks.ExtractAllJoinConditions();
Console.WriteLine($"Found {joinConditions.Count} join conditions:");
foreach (var condition in joinConditions)
{
    Console.WriteLine($"- {condition}");
}

// Parse a query with subqueries
var subquery = benchmarks.ParseWithSubqueriesQuery();
Console.WriteLine($"Subquery parsed: {subquery.Subqueries.Count} subqueries detected");

// Parse a query with CASE expressions
var caseQuery = benchmarks.ParseWithCaseExpressionsQuery();
Console.WriteLine($"CASE expressions query parsed: {caseQuery.CaseExpressions.Count} CASE expressions found");

// Hash a query with date/time functions
var dateHash = benchmarks.HashDateTimeFunctionsQuery();
Console.WriteLine($"Date/time query hash: {dateHash}");

// Format join conditions for reporting
var formattedJoins = benchmarks.FormatJoinConditions(
    "SELECT * FROM Orders o JOIN Customers c ON o.CustomerId = c.CustomerId"
);
Console.WriteLine(formattedJoins);
```

### Public Members

- `ParseWithCteQuery(this QueryAnalysisPipelineBenchmarks benchmarks)` - Parses and returns a query with Common Table Expressions (CTEs) for benchmarking recursive CTE handling
- `ParseWithWindowFunctionsQuery(this QueryAnalysisPipelineBenchmarks benchmarks)` - Parses and returns a query with window functions (OVER clauses) for benchmarking window function detection
- `HashParameterizedQuery(this QueryAnalysisPipelineBenchmarks benchmarks)` - Parses a parameterized query and returns its hash for caching scenarios
- `ExtractAllJoinConditions(this QueryAnalysisPipelineBenchmarks benchmarks)` - Extracts join conditions from a complex query with multiple join types
- `ParseWithSubqueriesQuery(this QueryAnalysisPipelineBenchmarks benchmarks)` - Parses and returns a query with subqueries in SELECT and WHERE clauses
- `ParseWithCaseExpressionsQuery(this QueryAnalysisPipelineBenchmarks benchmarks)` - Parses and returns a query with CASE expressions and conditional logic
- `HashDateTimeFunctionsQuery(this QueryAnalysisPipelineBenchmarks benchmarks)` - Parses a query with date/time functions and returns its hash
- `FormatJoinConditions(this QueryAnalysisPipelineBenchmarks benchmarks, string queryText)` - Formats extracted join conditions as a readable string for documentation or reports

---

## HttpQueryAnalysisClientExtensions

The `HttpQueryAnalysisClientExtensions` class provides extension methods for the `HttpQueryAnalysisClient` HTTP client that simplify common query analysis operations. It offers convenient methods for analyzing single queries, batch queries, checking service health with retry logic, retrieving version information, timeout-based analysis, performance metrics calculation, and filtering queries by complexity level. These extensions make it easier to work with the HTTP client by providing higher-level abstractions and common patterns.

### Usage Example

```csharp
// Create an HttpQueryAnalysisClient instance
var client = new HttpQueryAnalysisClient(
    baseUrl: "https://api.sqlqueryanalyzer.com",
    apiKey: "your-api-key-here",
    timeoutSeconds: 30);

// Analyze a single query
var singleResult = await client.AnalyzeQueryAsync(
    "SELECT u.Name, COUNT(o.Id) as OrderCount FROM Users u LEFT JOIN Orders o ON u.Id = o.UserId WHERE u.Status = 'active' GROUP BY u.Name");
Console.WriteLine($"Single query performance score: {singleResult.PerformanceScore}");

// Analyze multiple queries in parallel
var queries = new string[]
{
    "SELECT * FROM Users WHERE Status = 'active'",
    "SELECT p.Name, p.Price FROM Products p WHERE p.Price > 100",
    "SELECT COUNT(*) FROM Orders WHERE Date > '2024-01-01'"
};

var batchResults = await client.AnalyzeQueriesAsync(queries, maxDegreeOfParallelism: 4);
foreach (var result in batchResults)
{
    Console.WriteLine($"Query: {result.QueryText.Substring(0, Math.Min(50, result.QueryText.Length))}...");
    Console.WriteLine($"  Score: {result.PerformanceScore}, Complexity: {result.Complexity}");
}

// Check service health with retry logic
bool isHealthy = await client.IsHealthyWithRetryAsync(maxRetries: 5, delayMs: 2000);
Console.WriteLine($"Service healthy: {isHealthy}");

// Get version information with fallback
var version = await client.GetVersionAsync(fallbackVersion: "2.0.0");
Console.WriteLine($"Analyzer version: {version}");

// Analyze queries with timeout
var timedResults = await client.AnalyzeWithTimeoutAsync(
    queries,
    timeout: TimeSpan.FromSeconds(15),
    maxDegreeOfParallelism: 2);

// Calculate performance metrics across multiple iterations
var metrics = await client.GetPerformanceMetricsAsync(
    queries,
    iterations: 5,
    maxDegreeOfParallelism: 4);

foreach (var metric in metrics)
{
    Console.WriteLine($"Query: {metric.Key.Substring(0, 40)}...");
    Console.WriteLine($"  Average performance score: {metric.Value:F2}");
}

// Filter queries by complexity level
var complexQueries = await client.FilterQueriesByComplexityAsync(
    queries,
    minComplexity: QueryComplexity.Medium,
    maxComplexity: QueryComplexity.High,
    maxDegreeOfParallelism: 4);

Console.WriteLine($"Found {complexQueries.Count} queries with medium to high complexity");
```

### Public Members

- `AnalyzeQueriesAsync(HttpQueryAnalysisClient, string[], int?)` - Analyzes multiple queries with a specified degree of parallelism and returns results in the same order as input queries
- `AnalyzeQueryAsync(HttpQueryAnalysisClient, string, Dictionary<string, string>?)` - Analyzes a single query with optional analysis options
- `IsHealthyWithRetryAsync(HttpQueryAnalysisClient, int, int)` - Checks if the remote analyzer service is healthy with retry logic and configurable delay between attempts
- `GetVersionAsync(HttpQueryAnalysisClient, string)` - Gets the version information from the remote analyzer with a fallback version if the call fails
- `AnalyzeWithTimeoutAsync(HttpQueryAnalysisClient, string[], TimeSpan, int?)` - Analyzes queries with timeout and throws if timeout is exceeded
- `AnalyzeWithTimeoutAsync(HttpQueryAnalysisClient, string[], int, int?)` - Analyzes queries with timeout specified as milliseconds
- `GetPerformanceMetricsAsync(HttpQueryAnalysisClient, string[], int, int?)` - Gets performance metrics for queries by analyzing them multiple times and calculating average performance scores
- `FilterQueriesByComplexityAsync(HttpQueryAnalysisClient, string[], QueryComplexity, QueryComplexity, int?)` - Filters queries by their complexity level after analysis

---

## HttpQueryAnalysisClientValidation

The `HttpQueryAnalysisClientValidation` class provides validation helpers for the `HttpQueryAnalysisClient` HTTP client. It offers extension methods for validating constructor arguments, method parameters, and internal state of HTTP query analysis client instances. The validation covers client instances, individual queries, query arrays for batch operations, analysis options dictionaries, maximum degree of parallelism settings, and timeout values in seconds. Methods are provided for both validation with error collection and exception-throwing validation.

### Usage Example

```csharp
// Create an HttpQueryAnalysisClient instance
var client = new HttpQueryAnalysisClient(
    baseUrl: "https://api.sqlqueryanalyzer.com",
    apiKey: "your-api-key-here",
    timeoutSeconds: 30);

// Validate the client instance
var clientErrors = client.Validate();
if (clientErrors.Count > 0)
{
    Console.WriteLine("Client validation errors:");
    foreach (var error in clientErrors)
    {
        Console.WriteLine($"- {error}");
    }
}
else
{
    Console.WriteLine("HttpQueryAnalysisClient is valid!");
}

// Check validity with IsValid extension
bool isValid = client.IsValid();
Console.WriteLine($"Is valid: {isValid}");

// Validate a SQL query before analysis
var query = "SELECT * FROM Users WHERE Status = 'active'";
var queryErrors = query.ValidateQuery();
if (queryErrors.Count > 0)
{
    Console.WriteLine("Query validation errors:");
    foreach (var error in queryErrors)
    {
        Console.WriteLine($"- {error}");
    }
}
else
{
    Console.WriteLine("Query is valid for analysis!");
}

// Validate batch queries
var queries = new string[]
{
    "SELECT * FROM Users",
    "SELECT Name FROM Products WHERE Price > 100",
    "SELECT COUNT(*) FROM Orders WHERE Date > '2024-01-01'"
};
var batchErrors = queries.ValidateQueries();
Console.WriteLine($"Batch validation errors: {batchErrors.Count}");

// Validate analysis options
var options = new Dictionary<string, string>
{
    { "format", "json" },
    { "includeExecutionPlan", "true" },
    { "maxDegreeOfParallelism", "4" }
};
var optionsErrors = options.ValidateOptions();
Console.WriteLine($"Options validation errors: {optionsErrors.Count}");

// Validate max degree of parallelism
int? maxDegree = 4;
var maxDegreeErrors = maxDegree.ValidateMaxDegreeOfParallelism();
Console.WriteLine($"Max degree validation errors: {maxDegreeErrors.Count}");

// Validate timeout
int timeout = 30;
var timeoutErrors = timeout.ValidateTimeoutSeconds();
Console.WriteLine($"Timeout validation errors: {timeoutErrors.Count}");

// Use EnsureValid to throw exceptions on validation failure
try
{
    client.EnsureValid();
    query.EnsureValidQuery();
    queries.EnsureValidQueries();
    options.EnsureValidOptions();
    maxDegree.EnsureValidMaxDegreeOfParallelism();
    timeout.EnsureValidTimeoutSeconds();
    Console.WriteLine("All validations passed - no exceptions thrown");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}
```

### Public Members

- `Validate(this HttpQueryAnalysisClient? value)` - Validates an HttpQueryAnalysisClient instance and returns a list of validation errors; empty if valid
- `IsValid(this HttpQueryAnalysisClient? value)` - Determines whether the specified HttpQueryAnalysisClient instance is valid
- `EnsureValid(this HttpQueryAnalysisClient? value)` - Ensures that the specified HttpQueryAnalysisClient instance is valid, throwing an exception if not
- `ValidateQuery(this string? query)` - Validates a SQL query string and returns a list of validation errors; empty if valid
- `IsValidQuery(this string? query)` - Determines whether the specified query is valid
- `EnsureValidQuery(this string? query)` - Ensures that the specified query is valid, throwing an exception if not
- `ValidateQueries(this string[]? queries)` - Validates an array of SQL queries and returns a list of validation errors; empty if valid
- `IsValidQueries(this string[]? queries)` - Determines whether the specified queries array is valid
- `EnsureValidQueries(this string[]? queries)` - Ensures that the specified queries array is valid, throwing an exception if not
- `ValidateOptions(this Dictionary<string, string>? options)` - Validates an options dictionary and returns a list of validation errors; empty if valid
- `IsValidOptions(this Dictionary<string, string>? options)` - Determines whether the specified options dictionary is valid
- `EnsureValidOptions(this Dictionary<string, string>? options)` - Ensures that the specified options dictionary is valid, throwing an exception if not
- `ValidateMaxDegreeOfParallelism(this int? maxDegreeOfParallelism)` - Validates the maximum degree of parallelism and returns a list of validation errors; empty if valid
- `IsValidMaxDegreeOfParallelism(this int? maxDegreeOfParallelism)` - Determines whether the specified max degree of parallelism is valid
- `EnsureValidMaxDegreeOfParallelism(this int? maxDegreeOfParallelism)` - Ensures that the specified max degree of parallelism is valid, throwing an exception if not
- `ValidateTimeoutSeconds(this int timeoutSeconds)` - Validates the timeout in seconds and returns a list of validation errors; empty if valid
- `IsValidTimeoutSeconds(this int timeoutSeconds)` - Determines whether the specified timeout in seconds is valid
- `EnsureValidTimeoutSeconds(this int timeoutSeconds)` - Ensures that the specified timeout in seconds is valid, throwing an exception if not

---

## QueryAnalysisExtensionsValidation

The `QueryAnalysisExtensionsValidation` class provides validation helpers for `QueryAnalysisResult` objects used by query analysis extension methods. It validates analysis results for null references, invalid values, and internal consistency issues, ensuring extension methods can be safely invoked and results are meaningful.

### Usage Example

```csharp
// Analyze a SQL query using the analyzer service
var analyzer = new QueryAnalyzerService();
var result = await analyzer.AnalyzeQueryAsync(
    "SELECT u.Name, COUNT(o.Id) as OrderCount FROM Users u LEFT JOIN Orders o ON u.Id = o.UserId WHERE u.Status = 'active' GROUP BY u.Name HAVING COUNT(o.Id) > 5 ORDER BY OrderCount DESC");

// Validate the analysis result
var validationErrors = result.Validate();
if (validationErrors.Count > 0)
{
    Console.WriteLine("Validation errors found:");
    foreach (var error in validationErrors)
    {
        Console.WriteLine($"- {error}");
    }
}
else
{
    Console.WriteLine("Analysis result is valid!");
}

// Check if the result is valid
bool isValid = result.IsValid();
Console.WriteLine($"Is valid: {isValid}");

// Ensure the result is valid (throws exception if not)
try
{
    result.EnsureValid();
    Console.WriteLine("Analysis result is valid - no exception thrown");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}

// Validate a collection of analysis results
var results = new List<QueryAnalysisResult> { result, /* additional results */ };
var collectionErrors = results.Validate();
if (collectionErrors.Count > 0)
{
    Console.WriteLine("Collection validation errors found:");
    foreach (var error in collectionErrors)
    {
        Console.WriteLine($"- {error}");
    }
}
else
{
    Console.WriteLine("All analysis results in collection are valid!");
}

// Ensure all results in collection are valid
try
{
    results.EnsureValid();
    Console.WriteLine("All analysis results are valid - no exceptions thrown");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Collection validation failed: {ex.Message}");
}
```

### Public Members

- `Validate(this QueryAnalysisResult value)` - Validates a query analysis result and returns a list of validation problems; empty if all validations pass
- `IsValid(this QueryAnalysisResult value)` - Checks if the query analysis result is valid (no validation problems)
- `EnsureValid(this QueryAnalysisResult value)` - Ensures the query analysis result is valid, throwing an exception if not
- `Validate(this IEnumerable<QueryAnalysisResult> values)` - Validates a collection of query analysis results and returns a list of validation problems; empty if all results are valid
- `IsValid(this IEnumerable<QueryAnalysisResult> values)` - Checks if all query analysis results in a collection are valid
- `EnsureValid(this IEnumerable<QueryAnalysisResult> values)` - Ensures all query analysis results in a collection are valid, throwing an exception if not

---

## CommandLineArgumentsExtensions

The `CommandLineArgumentsExtensions` class provides extension methods for the `CommandLineArguments` type, enhancing type safety and simplifying common operations when working with command-line arguments. It includes methods for determining output behavior, resolving file paths with appropriate extensions, checking feature flags, normalizing configuration values, and filtering analysis parameters. These utilities help ensure consistent behavior across the CLI while reducing boilerplate code.

### Usage Example

```csharp
// Parse command line arguments
var args = new CommandLineArguments(new[] { "--output", "analysis_results", "--format", "json", "--verbose" });

// Determine if output should be written to a file
bool shouldWriteToFile = args.ShouldWriteToFile();
Console.WriteLine($"Should write to file: {shouldWriteToFile}");

// Get output file path with appropriate extension
string? outputPath = args.GetOutputFilePathWithExtension();
if (outputPath != null)
{
    Console.WriteLine($"Output will be written to: {outputPath}");
}

// Check if verbose logging is enabled
bool isVerbose = args.IsVerboseEnabled();
Console.WriteLine($"Verbose logging enabled: {isVerbose}");

// Get effective database connection string
string? connectionString = args.GetEffectiveConnectionString();
Console.WriteLine($"Effective connection string: {connectionString ?? "Not specified"}");

// Check if caching is enabled
bool cacheEnabled = args.IsCacheEnabled();
Console.WriteLine($"Cache enabled: {cacheEnabled}");

// Get normalized SQL Server version
string sqlVersion = args.GetNormalizedSqlServerVersion();
Console.WriteLine($"SQL Server version: {sqlVersion}");

// Get normalized severity filter
var severityFilter = args.GetNormalizedSeverityFilter();
Console.WriteLine($"Severity filter: {string.Join(", ", severityFilter)}");

// Check if execution plan analysis should be performed
bool analyzePlan = args.ShouldAnalyzeExecutionPlan();
Console.WriteLine($"Analyze execution plan: {analyzePlan}");

// Get effective maximum results limit
int? maxResults = args.GetEffectiveMaxResults();
Console.WriteLine($"Max results: {maxResults?.ToString() ?? "Unlimited"}");

// Check if suggestions should be exported
bool exportSuggestions = args.ShouldExportSuggestions();
Console.WriteLine($"Export suggestions: {exportSuggestions}");
```

### Public Members

- `ShouldWriteToFile(this CommandLineArguments args)` - Determines if output should be written to a file based on the provided arguments
- `GetOutputFilePathWithExtension(this CommandLineArguments args)` - Gets the effective output file path with appropriate extension based on the output format
- `IsVerboseEnabled(this CommandLineArguments args)` - Determines if verbose logging should be enabled
- `GetEffectiveConnectionString(this CommandLineArguments args)` - Gets the effective database connection string, prioritizing explicit connection over config file
- `IsCacheEnabled(this CommandLineArguments args)` - Determines if caching should be enabled
- `GetNormalizedSqlServerVersion(this CommandLineArguments args)` - Gets the effective SQL Server version as a normalized version string
- `GetNormalizedSeverityFilter(this CommandLineArguments args)` - Gets the effective severity filter as a normalized collection of severity levels
- `ShouldAnalyzeExecutionPlan(this CommandLineArguments args)` - Determines if execution plan analysis should be performed
- `GetEffectiveMaxResults(this CommandLineArguments args)` - Gets the effective maximum results limit, ensuring it's within valid bounds
- `ShouldExportSuggestions(this CommandLineArguments args)` - Determines if suggestions should be exported based on the arguments

---


## AnalysisBuilderValidation

The `AnalysisBuilderValidation` class provides validation extension methods for the `AnalysisBuilder` type used to construct SQL query analysis requests. It offers methods for validating builder instances, checking validity, and ensuring validation with detailed error messages covering query text, application name, procedure name, module name, and execution plan XML requirements.

### Usage Example

```csharp
// Create an AnalysisBuilder instance
var builder = new AnalysisBuilder()
    .WithQueryText("SELECT u.Name, COUNT(o.Id) as OrderCount FROM Users u LEFT JOIN Orders o ON u.Id = o.UserId WHERE u.Status = 'active' GROUP BY u.Name")
    .WithApplicationName("OrderProcessingSystem")
    .WithProcedureName("GetCustomerOrders")
    .WithModuleName("CustomerModule")
    .WithExecutionPlanXml("<ShowPlanXML>...</ShowPlanXML>");

// Validate the builder instance
var validationErrors = builder.Validate();
if (validationErrors.Count > 0)
{
    Console.WriteLine("Validation errors found:");
    foreach (var error in validationErrors)
    {
        Console.WriteLine($"- {error}");
    }
}
else
{
    Console.WriteLine("AnalysisBuilder is valid!");
}

// Check if the builder is valid
bool isValid = builder.IsValid();
Console.WriteLine($"Is valid: {isValid}");

// Build the analysis request
var analysisRequest = builder.Build();
Console.WriteLine($"Analysis request created for: {analysisRequest.ApplicationName}");

// Use EnsureValid to throw exceptions on validation failure
try
{
    builder.EnsureValid();
    Console.WriteLine("AnalysisBuilder validation passed - no exception thrown");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}
```

### Public Members

- `Validate(this AnalysisBuilder value)` - Validates the AnalysisBuilder instance and returns a list of human-readable validation problems; empty if valid
- `IsValid(this AnalysisBuilder value)` - Determines whether the AnalysisBuilder instance is valid
- `EnsureValid(this AnalysisBuilder value)` - Validates the AnalysisBuilder instance and throws an ArgumentException if it is not valid, containing all validation errors

---

## AnalysisControllerValidation

The `AnalysisControllerValidation` class provides validation helpers for API request/response types used by the AnalysisController. It offers extension methods for validating `AnalysisRequest`, `BatchAnalysisRequest`, `ApiResponse<T>`, and `HealthStatus` instances, returning lists of validation errors or boolean validation status. Methods are provided for both validation with error collection and exception-throwing validation.

### Usage Example

```csharp
// Validate an AnalysisRequest
var request = new AnalysisRequest
{
    Query = "SELECT * FROM Users WHERE Status = 'active'",
    Options = new Dictionary<string, string> { { "format", "json" } }
};

// Validate and check results
var errors = request.Validate();
if (errors.Count > 0)
{
    Console.WriteLine("Validation errors found:");
    foreach (var error in errors)
    {
        Console.WriteLine($"- {error}");
    }
}
else
{
    Console.WriteLine("AnalysisRequest is valid!");
}

// Check validity with IsValid extension
bool isValid = request.IsValid();
Console.WriteLine($"Is valid: {isValid}");

// Validate a BatchAnalysisRequest
var batchRequest = new BatchAnalysisRequest
{
    Queries = new[] { 
        "SELECT * FROM Users",
        "SELECT Name FROM Products WHERE Price > 100"
    },
    MaxDegreeOfParallelism = 4
};

var batchErrors = batchRequest.Validate();
Console.WriteLine($"Batch validation errors: {batchErrors.Count}");

// Validate an ApiResponse
var response = new ApiResponse<string>
{
    StatusCode = 200,
    Message = "Analysis completed successfully",
    Data = "Analysis result",
    Timestamp = DateTime.UtcNow,
    Errors = new List<string>()
};

var responseErrors = response.Validate();
Console.WriteLine($"Response validation errors: {responseErrors.Count}");

// Validate a HealthStatus
var healthStatus = new HealthStatus
{
    Message = "All systems operational",
    Version = "1.0.0",
    Timestamp = DateTime.UtcNow,
    IsHealthy = true
};

var statusErrors = healthStatus.Validate();
Console.WriteLine($"Health status validation errors: {statusErrors.Count}");

// Use EnsureValid to throw exceptions on validation failure
try
{
    request.EnsureValid();
    Console.WriteLine("Request is valid - no exception thrown");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}
```

### Public Members

- `Validate(this AnalysisRequest? request)` - Validates an AnalysisRequest instance and returns a list of validation errors
- `Validate(this BatchAnalysisRequest? request)` - Validates a BatchAnalysisRequest instance and returns a list of validation errors
- `Validate<T>(this ApiResponse<T>? response)` - Validates an ApiResponse instance and returns a list of validation errors
- `Validate(this HealthStatus? status)` - Validates a HealthStatus instance and returns a list of validation errors
- `IsValid(this AnalysisRequest? request)` - Determines whether the specified AnalysisRequest is valid
- `IsValid(this BatchAnalysisRequest? request)` - Determines whether the specified BatchAnalysisRequest is valid
- `IsValid<T>(this ApiResponse<T>? response)` - Determines whether the specified ApiResponse is valid
- `IsValid(this HealthStatus? status)` - Determines whether the specified HealthStatus is valid
- `EnsureValid(this AnalysisRequest? request)` - Ensures that the specified AnalysisRequest is valid, throwing an exception if not
- `EnsureValid(this BatchAnalysisRequest? request)` - Ensures that the specified BatchAnalysisRequest is valid, throwing an exception if not
- `EnsureValid<T>(this ApiResponse<T>? response)` - Ensures that the specified ApiResponse is valid, throwing an exception if not
- `EnsureValid(this HealthStatus? status)` - Ensures that the specified HealthStatus is valid, throwing an exception if not

---

## ErrorHandlingMiddlewareValidation

The `ErrorHandlingMiddlewareValidation` class provides validation helpers for the `ErrorHandlingMiddleware` and related error handling types. It offers extension methods for validating middleware instances, error reports, and degradation strategies, returning validation errors or boolean validation status. Methods are provided for both validation with error collection and exception-throwing validation.

### Usage Example

```csharp
// Create and configure error handling middleware
var errorHandlingMiddleware = new ErrorHandlingMiddleware(
    loggerFactory: LoggerFactory.Create(builder => builder.AddConsole()),
    includeStackTrace: true,
    degradationStrategy: DegradationStrategy.GracefulDegradation
);

// Validate middleware instance
var middlewareErrors = errorHandlingMiddleware.Validate();
Console.WriteLine($"Middleware validation errors: {middlewareErrors.Count}");

// Check if middleware is valid
bool isMiddlewareValid = errorHandlingMiddleware.IsValid();
Console.WriteLine($"Middleware is valid: {isMiddlewareValid}");

// Validate an error report
var errorReport = new ErrorReport
{
    ErrorMessage = "Database connection failed",
    ErrorType = "DatabaseException",
    StackTrace = "at SqlQueryAnalyzer.DatabaseService.Connect() in DatabaseService.cs:line 42",
    Context = "Query analysis pipeline",
    Timestamp = DateTime.UtcNow,
    Suggestion = "Check database connection string and ensure server is available"
};

var reportErrors = errorReport.Validate();
if (reportErrors.Count > 0)
{
    Console.WriteLine("Error report validation errors:");
    foreach (var error in reportErrors)
    {
        Console.WriteLine($"- {error}");
    }
}
else
{
    Console.WriteLine("Error report is valid!");
}

// Check if error report is valid
bool isReportValid = errorReport.IsValid();
Console.WriteLine($"Error report is valid: {isReportValid}");

// Validate a degradation strategy
var strategy = DegradationStrategy.GracefulDegradation;
var strategyErrors = strategy.Validate();
Console.WriteLine($"Degradation strategy validation errors: {strategyErrors.Count}");

// Use EnsureValid to throw exceptions on validation failure
try
{
    errorHandlingMiddleware.EnsureValid();
    errorReport.EnsureValid();
    strategy.EnsureValid();
    Console.WriteLine("All validations passed successfully!");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}
```

### Public Members

- `Validate(this ErrorHandlingMiddleware value)` - Validates an ErrorHandlingMiddleware instance and returns a list of validation problems (empty if valid)
- `IsValid(this ErrorHandlingMiddleware value)` - Determines whether the specified ErrorHandlingMiddleware instance is valid
- `EnsureValid(this ErrorHandlingMiddleware value)` - Ensures that the specified ErrorHandlingMiddleware instance is valid, throwing an exception if not
- `Validate(this ErrorReport report)` - Validates an ErrorReport instance and returns a list of validation problems
- `IsValid(this ErrorReport report)` - Determines whether the specified ErrorReport is valid
- `EnsureValid(this ErrorReport report)` - Ensures that the specified ErrorReport is valid, throwing an exception if not
- `Validate(this DegradationStrategy strategy)` - Validates a DegradationStrategy instance and returns a list of validation problems (empty if valid)
- `IsValid(this DegradationStrategy strategy)` - Determines whether the specified DegradationStrategy is valid
- `EnsureValid(this DegradationStrategy strategy)` - Ensures that the specified DegradationStrategy is valid, throwing an exception if not

---

## AnalyzerSettingsValidation

The `AnalyzerSettingsValidation` class provides validation helpers for `AnalyzerSettings` configuration objects. It offers extension methods for validating analyzer settings, checking validity, and ensuring configuration correctness with detailed error messages covering database, analysis, cache, performance, and logging settings validation.

### Usage Example

```csharp
// Create analyzer settings with database configuration
var settings = new AnalyzerSettings
{
    Database = new DatabaseSettings
    {
        Provider = "SqlServer",
        ConnectionString = "Server=localhost;Database=Analytics;User Id=sa;Password=your_password;",
        ConnectionPoolSize = 10,
        ConnectionTimeoutSeconds = 30
    },
    Analysis = new AnalysisSettings
    {
        MaxThreads = 4,
        CriticalIssueSensitivity = 0.8,
        IndexSeverity = new IndexSeverityThresholds
        {
            InfoMaxRows = 1000,
            WarningMaxRows = 10000,
            InfoMaxCost = 100,
            WarningMaxCost = 1000
        },
        IgnorePatterns = new List<string> { "temp_*", "audit_*" }
    },
    Cache = new CacheSettings
    {
        Provider = "Redis",
        MaxEntries = 1000,
        MaxSizeBytes = 10485760, // 10 MB
        ExpirationSeconds = 3600,
        RedisConnectionString = "localhost:6379"
    },
    Performance = new PerformanceSettings
    {
        TimeoutSeconds = 60,
        MaxQueryLength = 10000,
        RateLimitQueriesPerSecond = 100,
        MaxConcurrentAnalysis = 8,
        BatchSize = 50
    },
    Logging = new LoggingSettings
    {
        MinimumLevel = "Information",
        LogMaxFileSizeBytes = 10485760, // 10 MB
        LogMaxBackupFiles = 5,
        FileLogging = true,
        LogFilePath = "/var/log/sql-query-analyzer/analyzer.log"
    }
};

// Validate the settings
var validationErrors = settings.Validate();
if (validationErrors.Count > 0)
{
    Console.WriteLine("Validation errors found:");
    foreach (var error in validationErrors)
    {
        Console.WriteLine($"- {error}");
    }
}
else
{
    Console.WriteLine("AnalyzerSettings are valid!");
}

// Check if settings are valid
bool isValid = settings.IsValid();
Console.WriteLine($"Is valid: {isValid}");

// Use EnsureValid to throw exceptions on validation failure
try
{
    settings.EnsureValid();
    Console.WriteLine("AnalyzerSettings validation passed - no exception thrown");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}
```

### Public Members

- `Validate(this AnalyzerSettings value)` - Validates the AnalyzerSettings instance and returns a list of human-readable validation errors; empty if valid
- `Validate(this DatabaseSettings value)` - Validates DatabaseSettings and returns validation errors for database configuration
- `Validate(this AnalysisSettings value)` - Validates AnalysisSettings and returns validation errors for analysis configuration
- `Validate(this CacheSettings value)` - Validates CacheSettings and returns validation errors for cache configuration
- `Validate(this PerformanceSettings value)` - Validates PerformanceSettings and returns validation errors for performance configuration
- `Validate(this LoggingSettings value)` - Validates LoggingSettings and returns validation errors for logging configuration
- `Validate(this IndexSeverityThresholds value)` - Validates IndexSeverityThresholds and returns validation errors for index severity thresholds
- `IsValid(this AnalyzerSettings value)` - Determines whether the specified AnalyzerSettings are valid
- `EnsureValid(this AnalyzerSettings value)` - Ensures that the specified AnalyzerSettings are valid, throwing an exception if not

---

## SqlQueryAnalyzerOptionsValidation

The `SqlQueryAnalyzerOptionsValidation` class provides validation helpers for `SqlQueryAnalyzerOptions` configuration objects. It offers extension methods for validating SQL query analyzer options, checking validity, and ensuring configuration correctness with detailed error messages covering database, analysis, cache, performance, and logging settings validation.

### Usage Example

```csharp
// Create SQL query analyzer options with database configuration
var options = new SqlQueryAnalyzerOptions
{
    Database = new DatabaseOptions
    {
        Provider = "SqlServer",
        ConnectionString = "Server=localhost;Database=Analytics;User Id=sa;Password=your_password;",
        ConnectionPoolSize = 10,
        ConnectionTimeoutSeconds = 30
    },
    Analysis = new AnalysisOptions
    {
        MaxThreads = 4,
        CriticalIssueSensitivity = 0.8,
        IndexSeverity = new IndexSeverityThresholdsOptions
        {
            InfoMaxRows = 1000,
            WarningMaxRows = 10000,
            InfoMaxCost = 100,
            WarningMaxCost = 1000
        },
        IgnorePatterns = new List<string> { "temp_*", "audit_*" }
    },
    Cache = new CacheOptions
    {
        Provider = "Redis",
        MaxEntries = 1000,
        MaxSizeBytes = 10485760, // 10 MB
        ExpirationSeconds = 3600,
        RedisConnectionString = "localhost:6379"
    },
    Performance = new PerformanceOptions
    {
        TimeoutSeconds = 60,
        MaxQueryLength = 10000,
        RateLimitQueriesPerSecond = 100,
        MaxConcurrentAnalysis = 8,
        BatchSize = 50
    },
    Logging = new LoggingOptions
    {
        MinimumLevel = "Information",
        LogMaxFileSizeBytes = 10485760, // 10 MB
        LogMaxBackupFiles = 5,
        FileLogging = true,
        LogFilePath = "/var/log/sql-query-analyzer/analyzer.log"
    }
};

// Validate the options
var validationErrors = options.Validate();
if (validationErrors.Count > 0)
{
    Console.WriteLine("Validation errors found:");
    foreach (var error in validationErrors)
    {
        Console.WriteLine($"- {error}");
    }
}
else
{
    Console.WriteLine("SqlQueryAnalyzerOptions are valid!");
}

// Check if options are valid
bool isValid = options.IsValid();
Console.WriteLine($"Is valid: {isValid}");

// Use EnsureValid to throw exceptions on validation failure
try
{
    options.EnsureValid();
    Console.WriteLine("SqlQueryAnalyzerOptions validation passed - no exception thrown");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}
```

### Public Members

- `Validate(this SqlQueryAnalyzerOptions value)` - Validates the SqlQueryAnalyzerOptions instance and returns a list of human-readable validation errors; empty if valid
- `Validate(this DatabaseOptions value)` - Validates DatabaseOptions and returns validation errors for database configuration
- `Validate(this AnalysisOptions value)` - Validates AnalysisOptions and returns validation errors for analysis configuration
- `Validate(this CacheOptions value)` - Validates CacheOptions and returns validation errors for cache configuration
- `Validate(this PerformanceOptions value)` - Validates PerformanceOptions and returns validation errors for performance configuration
- `Validate(this LoggingOptions value)` - Validates LoggingOptions and returns validation errors for logging configuration
- `Validate(this IndexSeverityThresholdsOptions value)` - Validates IndexSeverityThresholdsOptions and returns validation errors for index severity thresholds
- `IsValid(this SqlQueryAnalyzerOptions value)` - Determines whether the specified SqlQueryAnalyzerOptions are valid
- `EnsureValid(this SqlQueryAnalyzerOptions value)` - Ensures that the specified SqlQueryAnalyzerOptions are valid, throwing an exception if not

---

## PerformanceIssueDetectorServiceExtensions

The `PerformanceIssueDetectorServiceExtensions` class provides extension methods for the `PerformanceIssueDetectorService` type that enhance performance issue detection capabilities. It includes methods for detecting common SQL performance anti-patterns such as N+1 queries, join issues, and index opportunities, along with utility methods for filtering, grouping, and prioritizing detected issues.

### Usage Example

```csharp
// Create a performance issue detector service
var detector = new PerformanceIssueDetectorService(logger: null);

// Create sample database queries to analyze
var queries = new List<DatabaseQuery>
{
    new DatabaseQuery
    {
        QueryText = "SELECT * FROM Users u WHERE u.Status = 'active'",
        ReferencedTables = new HashSet<string> { "Users" }
    },
    new DatabaseQuery
    {
        QueryText = "SELECT o.OrderId, o.UserId FROM Orders o JOIN Users u ON o.UserId = u.UserId WHERE o.OrderDate > '2024-01-01'",
        ReferencedTables = new HashSet<string> { "Orders", "Users" }
    },
    new DatabaseQuery
    {
        QueryText = "SELECT * FROM Products p WHERE p.Price > 100",
        ReferencedTables = new HashSet<string> { "Products" }
    }
};

// Detect all performance issues across multiple queries
var allIssues = await detector.DetectIssuesAsync(queries);
Console.WriteLine($"Detected {allIssues.Count} total issues");

// Detect N+1 patterns specifically
var nPlusOneIssues = await detector.DetectNPlusOneAsync(queries, "Users");
Console.WriteLine($"N+1 issues for Users table: {nPlusOneIssues.Count}");

// Detect join issues
var joinIssues = await detector.DetectJoinIssuesAsync(queries);
Console.WriteLine($"Join issues detected: {joinIssues.Count}");

// Detect index opportunities
var indexIssues = await detector.DetectIndexOpportunitiesAsync(queries);
Console.WriteLine($"Index opportunities detected: {indexIssues.Count}");

// Filter issues by severity
var criticalIssues = nPlusOneIssues.FilterBySeverity(IssueSeverity.Critical);
Console.WriteLine($"Critical issues: {criticalIssues.Count()}");

// Group issues by type
var groupedIssues = allIssues.GroupByIssueType();
foreach (var group in groupedIssues)
{
    Console.WriteLine($"Issue Type: {group.Key} - {group.Value.Count} issues");
}

// Calculate total performance impact
var totalImpact = allIssues.CalculateTotalImpact();
Console.WriteLine($"Total estimated performance impact: {totalImpact:P}");

// Get prioritized list of recommended fixes
var prioritizedFixes = allIssues.GetPrioritizedFixes();
Console.WriteLine("Prioritized fixes:");
foreach (var fix in prioritizedFixes)
{
    Console.WriteLine($"- {fix}");
}
```

### Public Members

- `DetectIssuesAsync` - Detects performance issues across multiple queries and returns a combined report
- `DetectNPlusOneAsync` - Detects N+1 query patterns specifically for queries referencing the same table
- `DetectJoinIssuesAsync` - Detects join issues across multiple queries and returns a combined report
- `DetectIndexOpportunitiesAsync` - Detects index opportunities across multiple queries and returns a combined report
- `FilterBySeverity` - Filters detected issues by severity level
- `GroupByIssueType` - Groups performance issues by their type for easier analysis
- `CalculateTotalImpact` - Calculates total estimated performance impact across all issues
- `GetPrioritizedFixes` - Creates a prioritized list of recommended fixes based on detected issues

---


---

## QueryPlanAnalyzerServiceExtensions

The `QueryPlanAnalyzerServiceExtensions` class provides extension methods for the `QueryPlanAnalyzerService` type that add advanced query plan analysis capabilities. It includes methods for analyzing execution plans, identifying expensive operations, detecting table scans, calculating performance scores, and generating detailed analysis reports.

### Usage Example

```csharp
// Create a query plan analyzer service
var analyzer = new QueryPlanAnalyzerService();

// Create a sample query plan (typically parsed from XML execution plan)
var plan = new QueryPlan
{
    DatabaseName = "OrderProcessingDB",
    CapturedAt = DateTime.UtcNow,
    Format = "ShowPlanXML",
    IsEstimated = true,
    TotalEstimatedCost = 12.45,
    TotalEstimatedRows = 5000,
    TotalLogicalReads = 2500,
    TotalPhysicalReads = 150
};

// Get the top 5 most expensive operations in the plan
var expensiveOps = await analyzer.GetExpensiveOperationsAsync(plan, 5);
Console.WriteLine($"Found {expensiveOps.Count} expensive operations");

// Get all index operations (seeks, scans, lookups)
var indexOps = await analyzer.GetIndexOperationsAsync(plan);
Console.WriteLine($"Found {indexOps.Count} index operations");

// Get a summary of the execution plan
var planSummary = await analyzer.GetPlanSummaryAsync(plan);
Console.WriteLine($"Plan summary: {planSummary.Count} metrics");

// Get a performance score for the query plan (0-100, lower is better)
var score = await analyzer.GetPerformanceScoreAsync(plan);
Console.WriteLine($"Performance score: {score}/100");

// Get all high-impact table scans (scans with large row estimates)
var highImpactScans = await analyzer.GetHighImpactTableScansAsync(plan, minRowThreshold: 1000);
Console.WriteLine($"Found {highImpactScans.Count} high-impact table scans");

// Group performance issues by their type (requires issues collection)
var issues = new List<PerformanceIssue>(); // Populate with actual issues
var groupedIssues = analyzer.GroupByIssueType(issues);
foreach (var group in groupedIssues)
{
    Console.WriteLine($"Issue type {group.Key}: {group.Value.Count} issues");
}

// Calculate performance score (0-100, lower is better)
var finalScore = await analyzer.GetPerformanceScoreAsync(plan);
Console.WriteLine($"Performance score: {finalScore}/100");

// Generate a detailed analysis report
var report = await analyzer.GetAnalysisReportAsync(plan);
Console.WriteLine(report);

```

### Public Members

- `GetExpensiveOperationsAsync(QueryPlanAnalyzerService, QueryPlan, int)` - Gets the top N most expensive operations in the execution plan
- `GetIndexOperationsAsync(QueryPlanAnalyzerService, QueryPlan)` - Gets all index operations (seeks, scans, lookups) from the execution plan
- `GetPlanSummaryAsync(QueryPlanAnalyzerService, QueryPlan)` - Gets a summary report of the execution plan analysis
- `GroupByIssueType(QueryPlanAnalyzerService, IEnumerable<PerformanceIssue>)` - Groups performance issues by their type for better analysis
- `GetHighImpactTableScansAsync(QueryPlanAnalyzerService, QueryPlan, int)` - Gets all table scans with high estimated row counts (potential performance issues)
- `GetPerformanceScoreAsync(QueryPlanAnalyzerService, QueryPlan)` - Gets a performance score for the query plan (0-100, lower is better)
- `GetAnalysisReportAsync(QueryPlanAnalyzerService, QueryPlan, CultureInfo?)` - Gets a detailed analysis report as a formatted string

---

## SqlPatternAnalyzerTestsExtensions

The `SqlPatternAnalyzerTestsExtensions` class provides extension methods for the `SqlPatternAnalyzerTests` class that simplify execution of related test groups. These methods combine multiple test invocations into convenient one-call methods, making it easier to run specific categories of tests without boilerplate code. The extensions cover SELECT * pattern detection, N+1 pattern detection, readability score calculation, and optimization recommendations.

### Usage Example

```csharp
// Create a test instance
var tests = new SqlPatternAnalyzerTests();

// Execute all SELECT * related tests
SqlPatternAnalyzerTestsExtensions.ExecuteAllSelectStarTests(tests);

// Execute all N+1 pattern detection tests
SqlPatternAnalyzerTestsExtensions.ExecuteAllNPlusOneTests(tests);

// Execute all readability score calculation tests
SqlPatternAnalyzerTestsExtensions.ExecuteAllReadabilityTests(tests);

// Execute all pattern detection tests
SqlPatternAnalyzerTestsExtensions.ExecuteAllPatternDetectionTests(tests);

// Execute individual tests for specific scenarios
SqlPatternAnalyzerTestsExtensions.ExecuteSelectStarDetectionTest(tests);
SqlPatternAnalyzerTestsExtensions.ExecuteSelectStarWithColumnsTest(tests);
SqlPatternAnalyzerTestsExtensions.ExecuteOptimizationRecommendationsTest(tests);
```

### Public Members

- `ExecuteAllSelectStarTests(this SqlPatternAnalyzerTests tests)` - Executes all tests related to SELECT * pattern detection and optimization recommendations
- `ExecuteAllNPlusOneTests(this SqlPatternAnalyzerTests tests)` - Executes all tests related to N+1 pattern detection
- `ExecuteAllReadabilityTests(this SqlPatternAnalyzerTests tests)` - Executes all tests related to readability score calculation
- `ExecuteAllPatternDetectionTests(this SqlPatternAnalyzerTests tests)` - Executes all tests related to pattern detection for leading wildcard LIKE clauses
- `ExecuteSelectStarDetectionTest(this SqlPatternAnalyzerTests tests)` - Executes the test that verifies SELECT * detection works correctly
- `ExecuteSelectStarWithColumnsTest(this SqlPatternAnalyzerTests tests)` - Executes the test that verifies explicit column selection is detected correctly
- `ExecuteOptimizationRecommendationsTest(this SqlPatternAnalyzerTests tests)` - Executes the test that verifies optimization recommendations include column replacement advice

---

## QueryProfilerExtensionsValidation

The `QueryProfilerExtensionsValidation` class provides validation extension methods for query profiler-related data types (`ProfilerSettings`, `QueryProfilerReport`, `ExecutionStage`, `ProfilerMetric`, `ProfilerSuggestion`, `ProfileComparison`, and `MetricDelta`). These methods validate null references, required fields, value ranges, and internal consistency, returning validation errors or boolean validation status. The `EnsureValid` methods throw exceptions when validation fails, making it easy to enforce data integrity before using profiler results.

### Usage Example

```csharp
// Create profiler settings with validation
var settings = new ProfilerSettings
{
    QueryText = "SELECT * FROM Users WHERE Status = 'active'",
    MaxExecutionTimeMs = 1000,
    SampleRate = 1.0
};

// Validate settings before using them
var validationErrors = settings.Validate();
if (validationErrors.Count > 0)
{
    Console.WriteLine("Validation errors found:");
    foreach (var error in validationErrors)
    {
        Console.WriteLine($"- {error}");
    }
}
else
{
    Console.WriteLine("ProfilerSettings are valid!");
}

// Check validity with IsValid extension
bool isValid = settings.IsValid();
Console.WriteLine($"Is valid: {isValid}");

// Validate a QueryProfilerReport
var report = new QueryProfilerReport
{
    QueryId = "query-123",
    QueryText = "SELECT * FROM Users WHERE Status = 'active'",
    ProfiledAt = DateTime.UtcNow,
    PerformanceScore = 85.5,
    TotalProfilingDurationMs = 45.2,
    ExecutionStages = new List<ExecutionStage>
    {
        new ExecutionStage { Name = "Parse", DurationMs = 2.1 },
        new ExecutionStage { Name = "Execute", DurationMs = 40.5 }
    },
    Metrics = new List<ProfilerMetric>
    {
        new ProfilerMetric { Name = "CPU Time", Value = 15.3, Unit = "ms" },
        new ProfilerMetric { Name = "Reads", Value = 1250, Unit = "pages" }
    },
    Suggestions = new List<ProfilerSuggestion>
    {
        new ProfilerSuggestion
        {
            Priority = 1,
            Title = "Add index on Status column",
            Description = "The Status column is frequently filtered but has no index",
            Recommendation = "CREATE INDEX IX_Users_Status ON Users(Status)",
            EstimatedImpactPercent = 45.0,
            Severity = SuggestionSeverity.High,
            Category = SuggestionCategory.Indexing
        }
    }
};

// Validate the report
var reportErrors = report.Validate();
if (reportErrors.Count > 0)
{
    Console.WriteLine("Report validation errors:");
    foreach (var error in reportErrors)
    {
        Console.WriteLine($"- {error}");
    }
}
else
{
    Console.WriteLine("QueryProfilerReport is valid!");
}

// Ensure the report is valid (throws exception if not)
try
{
    report.EnsureValid();
    Console.WriteLine("Report validation passed - no exception thrown");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}

// Validate a collection of profiler reports
var reports = new List<QueryProfilerReport> { report };
var collectionErrors = reports.Validate();
Console.WriteLine($"Collection validation errors: {collectionErrors.Count}");

// Validate an ExecutionStage
var stage = new ExecutionStage { Name = "Parse", DurationMs = 2.1 };
var stageErrors = stage.Validate();
Console.WriteLine($"Stage validation errors: {stageErrors.Count}");
```

### Public Members

- `Validate(this ProfilerSettings? settings)` - Validates profiler settings and returns a list of validation errors; empty if valid
- `IsValid(this ProfilerSettings? settings)` - Determines whether the specified profiler settings are valid
- `EnsureValid(this ProfilerSettings? settings)` - Ensures that the specified profiler settings are valid, throwing an exception if not
- `Validate(this QueryProfilerReport? report)` - Validates a query profiler report and returns a list of validation errors; empty if valid
- `IsValid(this QueryProfilerReport? report)` - Determines whether the specified query profiler report is valid
- `EnsureValid(this QueryProfilerReport? report)` - Ensures that the specified query profiler report is valid, throwing an exception if not
- `Validate(this ExecutionStage? stage)` - Validates an execution stage and returns a list of validation errors; empty if valid
- `IsValid(this ExecutionStage? stage)` - Determines whether the specified execution stage is valid
- `EnsureValid(this ExecutionStage? stage)` - Ensures that the specified execution stage is valid, throwing an exception if not

---

## ExplainPlanParserServiceExtensions


The `ExplainPlanParserServiceExtensions` class provides extension methods for the `ExplainPlanParserService` type that simplify parsing and analysis of database execution plans from various database systems (SQL Server, PostgreSQL, MySQL). It offers convenient methods for parsing execution plans, extracting performance metrics, checking for performance issues, and identifying expensive operations in query plans.

### Usage Example

```csharp
// Create an ExplainPlanParserService instance
var planParser = new ExplainPlanParserService();

// Example SQL Server execution plan (XML format)
string sqlServerPlan = @"
<ShowPlanXML xmlns="http://schemas.microsoft.com/sqlserver/2004/07/showplan">
  <Batch>
    <Statements>
      <StmtSimple StatementText="SELECT u.Name, COUNT(o.Id) as OrderCount FROM Users u LEFT JOIN Orders o ON u.Id = o.UserId WHERE u.Status = 'active' GROUP BY u.Name" StatementId="1" StatementCompId="2" StatementType="SELECT" QueryHash="0xABC123" QueryPlanHash="0xDEF456">
        <QueryPlan>
          <RelOp NodeId="0" PhysicalOp="Hash Match" LogicalOp="Aggregate" EstimateRows="100" EstimateIO="0.01" EstimateCPU="0.001">
            <OutputList>
              <ColumnReference Column="Name" />
              <ColumnReference Column="OrderCount" />
            </OutputList>
            <Hash>
              <RelOp NodeId="1" PhysicalOp="Index Scan" LogicalOp="Index Scan" EstimateRows="1000" EstimateIO="0.1" EstimateCPU="0.01">
                <OutputList>
                  <ColumnReference Column="Name" />
                </OutputList>
                <IndexScan>
                  <Object Database="[MyDatabase]" Schema="[dbo]" Table="[Users]" Index="[IX_Users_Status]" />
                </IndexScan>
              </RelOp>
            </Hash>
          </RelOp>
        </QueryPlan>
      </StmtSimple>
    </Statements>
  </Batch>
</ShowPlanXML>
";

// Parse SQL Server execution plan
var sqlServerQueryPlan = await planParser.ParseSqlServerPlanAsync(sqlServerPlan);
Console.WriteLine($"SQL Server plan parsed: {sqlServerQueryPlan.DatabaseName}");

// Parse PostgreSQL EXPLAIN plan (JSON format)
string postgreSqlPlan = @"
{
  "Plan": {
    "Node Type": "Aggregate",
    "Actual Total Time": 0.123,
    "Actual Rows": 100,
    "Plans": [
      {
        "Node Type": "Seq Scan",
        "Relation Name": "users",
        "Actual Total Time": 0.045,
        "Actual Rows": 1000,
        "Startup Cost": 0.00,
        "Total Cost": 14.29,
        "Plan Rows": 1000,
        "Plan Width": 36
      }
    ]
  }
}
";

var postgreSqlQueryPlan = await planParser.ParsePostgreSqlPlanAsync(postgreSqlPlan);
Console.WriteLine($"PostgreSQL plan parsed: {postgreSqlQueryPlan.DatabaseName}");

// Parse MySQL EXPLAIN plan (JSON format)
string mySqlPlan = @"
{
  "query_block": {
    "select_id": 1,
    "table": {
      "table_name": "users",
      "access_type": "index",
      "possible_keys": ["PRIMARY"],
      "key": "PRIMARY",
      "rows": 1000,
      "filtered": 100.00,
      "Extra": "Using where"
    }
  }
}
";

var mySqlQueryPlan = await planParser.ParseMySqlPlanAsync(mySqlPlan);
Console.WriteLine($"MySQL plan parsed: {mySqlQueryPlan.DatabaseName}");

// Extract performance metrics from a query plan
var metrics = await planParser.ExtractPlanMetricsAsync(sqlServerQueryPlan);
Console.WriteLine($"Total cost: {metrics["totalCost"]}");
Console.WriteLine($"Estimated rows: {metrics["estimatedRows"]}");
Console.WriteLine($"Efficiency: {metrics["efficiency"]}");

// Get a simplified performance summary
var summary = await planParser.GetPlanSummaryAsync(sqlServerQueryPlan);
Console.WriteLine($"Plan summary - Database: {summary["database"]}, Format: {summary["format"]}");

// Check if the plan has performance issues
bool hasIssues = await planParser.HasPerformanceIssuesAsync(sqlServerQueryPlan);
Console.WriteLine($"Has performance issues: {hasIssues}");

// Get the most expensive operations in the plan
var expensiveOps = await planParser.GetMostExpensiveOperationsAsync(sqlServerQueryPlan, count: 3);
Console.WriteLine($"Most expensive operations: {expensiveOps.Count}");
foreach (var op in expensiveOps)
{
  Console.WriteLine($"- {op.NodeType} on {op.ObjectName} (cost: {op.EstimatedCost:F3})");
}
```

### Public Members

- `ParseSqlServerPlanAsync(this ExplainPlanParserService service, string xmlPlan)` - Parses a SQL Server execution plan from XML format and returns the query plan
- `ParsePostgreSqlPlanAsync(this ExplainPlanParserService service, string jsonPlan)` - Parses a PostgreSQL EXPLAIN plan from JSON format and returns the query plan
- `ParseMySqlPlanAsync(this ExplainPlanParserService service, string jsonPlan)` - Parses a MySQL EXPLAIN plan from JSON or tabular format and returns the query plan
- `ExtractPlanMetricsAsync(this ExplainPlanParserService service, QueryPlan plan)` - Extracts performance metrics from a query plan and returns them as a dictionary
- `GetPlanSummaryAsync(this ExplainPlanParserService service, QueryPlan plan)` - Parses a query plan and extracts a simplified performance summary
- `HasPerformanceIssuesAsync(this ExplainPlanParserService service, QueryPlan plan)` - Determines if a query plan has performance issues based on common bottlenecks
- `GetMostExpensiveOperationsAsync(this ExplainPlanParserService service, QueryPlan plan, int count = 5)` - Gets the most expensive operations in the query plan

---

## PerformanceIssueDetectorServiceExtensions
- `Validate(this ProfilerMetric? metric)` - Validates a profiler metric and returns a list of validation errors; empty if valid
- `IsValid(this ProfilerMetric? metric)` - Determines whether the specified profiler metric is valid
- `EnsureValid(this ProfilerMetric? metric)` - Ensures that the specified profiler metric is valid, throwing an exception if not
- `Validate(this ProfilerSuggestion? suggestion)` - Validates a profiler suggestion and returns a list of validation errors; empty if valid
- `IsValid(this ProfilerSuggestion? suggestion)` - Determines whether the specified profiler suggestion is valid
- `EnsureValid(this ProfilerSuggestion? suggestion)` - Ensures that the specified profiler suggestion is valid, throwing an exception if not
- `Validate(this ProfileComparison? comparison)` - Validates a profile comparison and returns a list of validation errors; empty if valid
- `IsValid(this ProfileComparison? comparison)` - Determines whether the specified profile comparison is valid
- `EnsureValid(this ProfileComparison? comparison)` - Ensures that the specified profile comparison is valid, throwing an exception if not
- `Validate(this MetricDelta? delta)` - Validates a metric delta and returns a list of validation errors; empty if valid
- `IsValid(this MetricDelta? delta)` - Determines whether the specified metric delta is valid
- `EnsureValid(this MetricDelta? delta)` - Ensures that the specified metric delta is valid, throwing an exception if not
- `Validate(this IEnumerable<QueryProfilerReport> values)` - Validates a collection of query profiler reports and returns a list of validation problems; empty if all results are valid
- `IsValid(this IEnumerable<QueryProfilerReport> values)` - Checks if all query profiler reports in a collection are valid
- `EnsureValid(this IEnumerable<QueryProfilerReport> values)` - Ensures all query profiler reports in a collection are valid, throwing an exception if not


## SampleQueryProviderJsonExtensions

The `SampleQueryProviderJsonExtensions` class provides static methods for serializing and deserializing sample query data to and from JSON. It supports conversion of sample queries organized by issue type, all samples, and random samples, enabling easy storage and transmission of test query data for SQL performance analysis scenarios.

### Usage Example

```csharp
// Serialize sample queries to JSON for storage or transmission
var json = SampleQueryProviderJsonExtensions.ToJson();
File.WriteAllText("sample_queries.json", json);

// Serialize with pretty printing for readability
var prettyJson = SampleQueryProviderJsonExtensions.ToJson(indented: true);
Console.WriteLine(prettyJson);

// Deserialize sample queries from JSON
string jsonData = File.ReadAllText("sample_queries.json");
var deserializedData = SampleQueryProviderJsonExtensions.FromJson(jsonData);

// Try to deserialize with error handling
if (SampleQueryProviderJsonExtensions.TryFromJson(jsonData, out var result))
{
    Console.WriteLine("Successfully deserialized sample queries");
}
else
{
    Console.WriteLine("Failed to deserialize sample queries");
}

// Access the actual sample data structure
var typedResult = SampleQueryProviderJsonExtensions.FromJson(jsonData) as JsonElement?;
if (typedResult?.Value.TryGetProperty("allSamples", out var allSamples) == true)
{
    foreach (var sample in allSamples.EnumerateObject())
    {
        Console.WriteLine($"Issue type '{sample.Name}': {sample.Value.GetRawText()}");
    }
}
```

### Public Members

- `ToJson(bool indented = false)` - Serializes sample queries to a JSON string, optionally formatted with indentation
- `FromJson(string json)` - Deserializes a JSON string to sample query data
- `TryFromJson(string json, out object? value)` - Attempts to deserialize a JSON string with error handling

---

## PlanVisualizationExtensions

The `PlanVisualizationExtensions` type provides extension methods for analyzing and visualizing query plan bottlenecks. It offers methods to calculate bottleneck costs, retrieve bottleneck annotations by various criteria (cost, depth, node type), analyze bottleneck distributions, and generate summary strings for plan visualization purposes.

Example usage:

```csharp
public class PlanVisualizationExtensionsExample
{
    public static void ExampleUsage()
    {
        // Calculate total bottleneck cost across all bottlenecks in the current plan
        double totalBottleneckCost = PlanVisualizationExtensions.GetTotalBottleneckCost();
        
        // Get the highest cost bottleneck in the current plan
        BottleneckAnnotation? highestCostBottleneck = PlanVisualizationExtensions.GetHighestCostBottleneck();
        
        // Calculate average bottleneck depth
        double averageBottleneckDepth = PlanVisualizationExtensions.GetAverageBottleneckDepth();
        
        // Get the percentage of query cost attributed to bottlenecks
        double bottleneckCostPercentage = PlanVisualizationExtensions.GetBottleneckCostPercentage();
        
        // Get the most common bottleneck node type
        string mostCommonNodeType = PlanVisualizationExtensions.GetMostCommonBottleneckNodeType();
        
        // Get the maximum bottleneck depth in the plan
        int maxBottleneckDepth = PlanVisualizationExtensions.GetMaxBottleneckDepth();
        
        // Get distribution of bottleneck node types
        IReadOnlyDictionary<string, int> nodeTypeDistribution = 
            PlanVisualizationExtensions.GetBottleneckNodeTypeDistribution();
        
        // Get all bottlenecks of a specific node type
        var indexBottlenecks = PlanVisualizationExtensions.GetBottlenecksByNodeType("Index");
        
        // Get high-cost bottlenecks (above threshold)
        var highCostBottlenecks = PlanVisualizationExtensions.GetHighCostBottlenecks();
        
        // Get bottlenecks at a specific depth level
        var depth5Bottlenecks = PlanVisualizationExtensions.GetBottlenecksAtDepth(5);
        
        // Generate a summary string for the current bottlenecks
        string summary = PlanVisualizationExtensions.ToSummaryString();
        
        Console.WriteLine(summary);
    }
}
```

### Public Members

- `GetTotalBottleneckCost()` - Calculates the total cost of all bottlenecks in the current query plan
- `GetHighestCostBottleneck()` - Returns the bottleneck annotation with the highest cost, or null if no bottlenecks exist
- `GetAverageBottleneckDepth()` - Calculates the average depth of all bottlenecks in the query plan
- `GetBottleneckCostPercentage()` - Returns the percentage of total query cost attributed to bottlenecks
- `GetMostCommonBottleneckNodeType()` - Identifies the most frequently occurring bottleneck node type
- `GetMaxBottleneckDepth()` - Returns the maximum depth at which bottlenecks occur in the plan
- `GetBottleneckNodeTypeDistribution()` - Returns a dictionary mapping node types to their bottleneck counts
- `GetBottlenecksByNodeType(string nodeType)` - Filters bottlenecks by the specified node type
- `GetHighCostBottlenecks()` - Returns bottlenecks exceeding a cost threshold (typically > 10% of total cost)
- `GetBottlenecksAtDepth(int depth)` - Returns bottlenecks occurring at the specified depth level
- `ToSummaryString()` - Generates a formatted summary string describing the bottleneck analysis results

## QueryRewriteExtensionsValidation

The `QueryRewriteExtensionsValidation` class provides validation extension methods for the `QueryRewriteExtensions` class. It validates `QueryRewriteSuggestion` collections and results from extension methods like `GetAutoApplicable()`, `GetNonBreaking()`, `OfType()`, `ForClause()`, `OrderByImpact()`, `GetTotalEstimatedImprovement()`, `GetAllIndexSuggestions()`, and `GetRewriteSummary()`. These validation methods ensure suggestions are properly structured, have valid values, and return expected results, helping to catch issues during query rewrite analysis and optimization workflows.

### Usage Example

```csharp
// Generate query rewrite suggestions from analysis results
var analyzer = new QueryAnalyzerService();
var analysis = await analyzer.AnalyzeQueryAsync(
    "SELECT * FROM Orders WHERE CustomerId = 123 AND Status = 'active'");

var suggestions = analysis.GetRewriteSuggestions()
    .Where(s => s.EstimatedImprovementPercent > 5)
    .ToList();

// Validate the suggestions collection
var validationProblems = suggestions.Validate();
if (validationProblems.Count > 0)
{
    Console.WriteLine("Validation errors found:");
    foreach (var problem in validationProblems)
    {
        Console.WriteLine($"- {problem}");
    }
    return;
}

// Validate specific extension method results
var autoApplicableProblems = suggestions.ValidateAutoApplicable();
var nonBreakingProblems = suggestions.ValidateNonBreaking();
var totalImprovement = suggestions.ValidateTotalEstimatedImprovement();

// Validate filtered suggestions by type
var indexSuggestions = suggestions.ValidateAllIndexSuggestions();
var selectStarFixes = suggestions.ValidateOfType(RewriteType.SelectStarToExplicitColumns);

// Validate suggestions for a specific clause
var whereClauseSuggestions = suggestions.ValidateForClause("WHERE");

// Validate ordered suggestions by impact
var orderedByImpact = suggestions.OrderByImpact();
var impactValidation = orderedByImpact.ValidateOrderByImpact();

// Get total estimated improvement across all suggestions
double totalImprovementPercent = suggestions.GetTotalEstimatedImprovement();
Console.WriteLine($"Total estimated improvement: {totalImprovementPercent:F1}%");

// Get rewrite summary
var summary = suggestions.GetRewriteSummary();
Console.WriteLine($"Rewrite summary: {summary}");

// Ensure suggestions are valid (throws exception if not)
try
{
    suggestions.EnsureValid();
    orderedByImpact.EnsureValid();
    Console.WriteLine("All validations passed successfully!");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}
```

### Public Members

- `Validate(this IEnumerable<QueryRewriteSuggestion> suggestions)` - Validates a collection of query rewrite suggestions and returns a list of validation problems; empty if valid
- `ValidateAutoApplicable(this IEnumerable<QueryRewriteSuggestion> suggestions)` - Validates results from `GetAutoApplicable()` and ensures all suggestions are marked as auto-applicable
- `ValidateNonBreaking(this IEnumerable<QueryRewriteSuggestion> suggestions)` - Validates results from `GetNonBreaking()` and ensures no suggestions are breaking changes
- `ValidateOfType(this IEnumerable<QueryRewriteSuggestion> suggestions, RewriteType rewriteType)` - Validates results from `OfType()` and ensures all suggestions have the specified rewrite type
- `ValidateForClause(this IEnumerable<QueryRewriteSuggestion> suggestions, string clause)` - Validates results from `ForClause()` and ensures all suggestions affect the specified clause
- `ValidateOrderByImpact(this IEnumerable<QueryRewriteSuggestion> suggestions)` - Validates results from `OrderByImpact()` and ensures suggestions are ordered by estimated improvement (descending)
- `ValidateTotalEstimatedImprovement(this IEnumerable<QueryRewriteSuggestion> suggestions)` - Validates the result from `GetTotalEstimatedImprovement()` and ensures it's a valid percentage (0-100)
- `ValidateAllIndexSuggestions(this IEnumerable<QueryRewriteSuggestion> suggestions)` - Validates results from `GetAllIndexSuggestions()` and ensures all index suggestions have valid names and positive performance gains
- `ValidateRewriteSummary(this IEnumerable<QueryRewriteSuggestion> suggestions)` - Validates results from `GetRewriteSummary()` and ensures it returns a non-empty summary string
- `IsValid(this IEnumerable<QueryRewriteSuggestion> suggestions)` - Checks if the suggestions collection is valid (no validation problems)
- `EnsureValid(this IEnumerable<QueryRewriteSuggestion> suggestions)` - Ensures the suggestions collection is valid, throwing an exception if not

---

## QueryProfilerExtensionsJsonExtensions

The `QueryProfilerExtensionsJsonExtensions` class provides static methods for serializing and deserializing query profiler-related data types to and from JSON. It supports conversion of `QueryProfilerReport`, `ProfileComparison`, `ProfilerBatchSummary`, and collections of `QueryProfilerReport` objects, enabling easy storage and transmission of profiling data.

### Usage Example

```csharp
// Serialize a QueryProfilerReport to JSON
var report = new QueryProfilerReport
{
    QueryText = "SELECT * FROM Users WHERE Status = 'active'",
    ExecutionTimeMs = 15.2,
    Reads = 1250,
    Writes = 0,
    CpuTimeMs = 8.7
};

string json = report.ToJson();
File.WriteAllText("report.json", json);

// Deserialize a QueryProfilerReport from JSON
string jsonData = File.ReadAllText("report.json");
QueryProfilerReport? deserializedReport = QueryProfilerReport.FromJson(jsonData);

// Try to deserialize with error handling
if (QueryProfilerReport.TryFromJson(jsonData, out var reportResult))
{
    Console.WriteLine($"Deserialized report: {reportResult.QueryText}");
}

// Serialize and deserialize a ProfileComparison
var comparison = new ProfileComparison
{
    Baseline = new QueryProfilerReport { /* baseline data */ },
    Comparison = new QueryProfilerReport { /* comparison data */ },
    DifferencePercentage = 12.5
};

string comparisonJson = comparison.ToJson();
ProfileComparison? comparisonResult = ProfileComparison.FromJsonToProfileComparison(comparisonJson);

// Serialize and deserialize a ProfilerBatchSummary
var batchSummary = new ProfilerBatchSummary
{
    TotalQueries = 100,
    TotalExecutionTimeMs = 1520.5,
    AverageExecutionTimeMs = 15.2,
    MaxExecutionTimeMs = 45.8,
    MinExecutionTimeMs = 2.1
};

string batchJson = batchSummary.ToJson();
ProfilerBatchSummary? batchResult = ProfilerBatchSummary.FromJsonToBatchSummary(batchJson);

// Serialize and deserialize a collection of QueryProfilerReport objects
var reports = new List<QueryProfilerReport> { report, /* additional reports */ };
string reportsJson = reports.ToJson();
IEnumerable<QueryProfilerReport>? deserializedReports = QueryProfilerReport.FromJsonToReports(reportsJson);
```

### Public Members

- `ToJson(this QueryProfilerReport report)` - Serializes a QueryProfilerReport to JSON
- `FromJson(string json)` - Deserializes a JSON string to a QueryProfilerReport
- `TryFromJson(string json, out QueryProfilerReport? report)` - Attempts to deserialize a JSON string to a QueryProfilerReport with error handling
- `ToJson(this ProfileComparison comparison)` - Serializes a ProfileComparison to JSON
- `FromJsonToProfileComparison(string json)` - Deserializes a JSON string to a ProfileComparison
- `TryFromJsonToProfileComparison(string json, out ProfileComparison? comparison)` - Attempts to deserialize a JSON string to a ProfileComparison with error handling
- `ToJson(this ProfilerBatchSummary summary)` - Serializes a ProfilerBatchSummary to JSON
- `FromJsonToBatchSummary(string json)` - Deserializes a JSON string to a ProfilerBatchSummary
- `TryFromJsonToBatchSummary(string json, out ProfilerBatchSummary? summary)` - Attempts to deserialize a JSON string to a ProfilerBatchSummary with error handling
- `ToJson(this IEnumerable<QueryProfilerReport> reports)` - Serializes a collection of QueryProfilerReport objects to JSON
- `FromJsonToReports(string json)` - Deserializes a JSON string to a collection of QueryProfilerReport objects
- `TryFromJsonToReports(string json, out IEnumerable<QueryProfilerReport>? reports)` - Attempts to deserialize a JSON string to a collection of QueryProfilerReport objects with error handling

---

## CommandLineArguments

The `CommandLineArguments` class represents the parsed command-line arguments for the SQL Query Analyzer. It supports various analysis modes including single query analysis, batch processing, configuration overrides, and output formatting. This type serves as the primary configuration container for CLI operations and can be used programmatically for integration scenarios.

### Usage Example

```csharp
// Create command-line arguments for single query analysis
var args = new CommandLineArguments
{
    Query = "SELECT * FROM Users WHERE Status = 'active'",
    OutputFormat = "json",
    Verbose = true,
    FilterBySeverity = "Warning",
    DatabaseConnection = "Server=localhost;Database=TestDB;User Id=sa;Password=your_password;"
};

// Validate arguments before use
try
{
    args.Validate();

    // Use with CLI application host
    var services = new ServiceCollection();
    services.AddLogging();
    services.AddQueryAnalyzerServices();

    var serviceProvider = services.BuildServiceProvider();
    var host = new CliApplicationHost(serviceProvider);

    int exitCode = await host.RunAsync(args);

    if (exitCode == 0)
    {
        Console.WriteLine("Analysis completed successfully!");
    }
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Invalid arguments: {ex.Message}");
}

// Batch mode example with multiple queries
var batchArgs = new CommandLineArguments
{
    BatchMode = true,
    ThreadCount = 4,
    OutputFormat = "csv",
    OutputPath = "analysis_results.csv",
    GenerateReport = true,
    EnableCache = true,
    CachePath = "./cache"
};

// Dry-run example (no actual analysis performed)
var dryRunArgs = new CommandLineArguments
{
    Query = "SELECT * FROM LargeTable WHERE Date > '2024-01-01'",
    DryRun = true,
    Verbose = true,
    ShowExecutionPlan = true
};
```

---

## WebhookNotificationServiceExtensions

The `WebhookNotificationServiceExtensions` class provides extension methods for managing webhook configurations in the SQL Query Analyzer. It enables bulk registration, filtering, enabling/disabling, and statistics gathering for webhook notifications, making it easier to manage multiple webhook endpoints for different event types.

### Usage Example

```csharp
// Configure webhook notifications for SQL query analysis results
var services = new ServiceCollection();
services.AddSingleton<WebhookNotificationService>();
var serviceProvider = services.BuildServiceProvider();
var webhookService = serviceProvider.GetRequiredService<WebhookNotificationService>();

// Register multiple webhooks at once
var webhookConfigs = new List<WebhookConfiguration>
{
    webhookService.CreateWebhookConfiguration("https://hooks.slack.com/services/xxx/yyy/zzz", "Slack Alerts", WebhookType.Slack),
    webhookService.CreateWebhookConfiguration("https://outlook.office.com/webhook/xxx/yyy/zzz", "Teams Notifications", WebhookType.MicrosoftTeams)
};
webhookService.RegisterWebhooks(webhookConfigs);

// Get all enabled webhooks for completion events
var enabledCompletionWebhooks = webhookService.GetWebhooksForEvent(typeof(AnalysisCompletedEvent))
                                              .Where(w => w.Enabled)
                                              .ToList();

// Get webhook statistics
var stats = webhookService.GetWebhookStatistics();
Console.WriteLine($"Total webhooks: {stats["Total"]}, Enabled: {stats["Enabled"]}");

// Check if we have webhooks for critical issues
if (webhookService.HasWebhookForEvent(typeof(CriticalIssueDetectedEvent)))
{
    Console.WriteLine("Critical issue notifications are configured");
}

// Disable webhooks for a specific pattern (e.g., all Slack webhooks)
int disabledCount = webhookService.DisableWebhooks(w => w.Type == WebhookType.Slack);
Console.WriteLine($"Disabled {disabledCount} Slack webhooks");

// Add custom headers to a specific webhook
var headers = new Dictionary<string, string>
{
    ["Authorization"] = "Bearer token123",
    ["X-Custom-Header"] = "custom-value"
};

int addedHeaders = webhookService.AddCustomHeaders("Slack Alerts", headers);
```

---

## AnalysisPipelineExtensions

The `AnalysisPipelineExtensions` class provides a set of fluent extension methods for the `AnalysisPipeline` class. These methods allow developers to easily configure a query analysis pipeline by adding various middleware—such as logging, validation, normalization, analysis, and optimization—and to execute the pipeline on single or batch SQL queries.

### Usage Example

```csharp
// Assuming you have an IQueryAnalyzerService registered in your DI container
var pipeline = new AnalysisPipeline();

// Use fluent API to configure standard middleware
pipeline.UseAllStandardMiddleware(myAnalyzerService);

// Analyze a single query
var result = await pipeline.AnalyzeQueryAsync("SELECT * FROM Users WHERE Status = 'active'");

// Check middleware count or clear for reconfiguration
Console.WriteLine($"Pipeline middleware count: {pipeline.GetMiddlewareCount()}");
pipeline.ClearMiddleware();
```

### Public Members

- `UseLogging` - Adds logging middleware to the pipeline
- `UseValidation` - Adds validation middleware to the pipeline
- `UseNormalization` - Adds query normalization middleware to the pipeline
- `UseAnalysis` - Adds analysis middleware to the pipeline
- `UseOptimization` - Adds optimization middleware to the pipeline
- `AnalyzeQueryAsync` - Executes the pipeline with the given query string and returns the analysis result
- `AnalyzeQueriesAsync` - Executes the pipeline with the given queries in parallel and returns all results
- `ClearMiddleware` - Clears all middleware from the pipeline, allowing it to be reconfigured
- `UseAllStandardMiddleware` - Adds all standard middleware (logging, validation, normalization, analysis, optimization) to the pipeline in the recommended order
- `GetMiddlewareCount` - Gets the count of middleware registered in the pipeline
- `ExecuteWithSuccessCheckAsync` - Executes the pipeline with the given context and returns whether execution completed successfully

---

## ErrorHandlingMiddlewareExtensionsValidation

The `ErrorHandlingMiddlewareExtensionsValidation` class provides validation helpers for the `ErrorHandlingMiddlewareExtensions` extension methods. It offers methods to validate parameters for error handling operations including error report creation, retry execution, and cache fallback execution. The validation ensures that error messages and contexts are properly provided, operations are not null, and retry counts are valid, returning validation errors or boolean validation status. The `EnsureValid` methods throw exceptions when validation fails.

### Usage Example

```csharp
// Validate error report creation parameters
var errorMessage = "Database connection failed";
var context = "Query analysis pipeline";

// Validate and check results
var validationErrors = ErrorHandlingMiddlewareExtensionsValidation.Validate(errorMessage, context);
if (validationErrors.Count > 0)
{
    Console.WriteLine("Validation errors found:");
    foreach (var error in validationErrors)
    {
        Console.WriteLine($"- {error}");
    }
}
else
{
    Console.WriteLine("Error report parameters are valid!");
}

// Check validity with IsValid extension
bool isValid = ErrorHandlingMiddlewareExtensionsValidation.IsValid(errorMessage, context);
Console.WriteLine($"Is valid: {isValid}");

// Validate retry operation parameters
Func<Task<string>> operation = async () => await DatabaseService.ConnectAsync();
var retryErrors = ErrorHandlingMiddlewareExtensionsValidation.Validate(
    operation, 
    "Database connection", 
    maxRetries: 3);
Console.WriteLine($"Retry validation errors: {retryErrors.Count}");

// Validate cache fallback operation parameters
Func<Task<int>> cacheOperation = async () => await GetCachedResultAsync();
Func<int> cachedResultProvider = () => GetDefaultResult();
var cacheErrors = ErrorHandlingMiddlewareExtensionsValidation.Validate(
    cacheOperation, 
    cachedResultProvider, 
    "Cache fallback operation");
Console.WriteLine($"Cache validation errors: {cacheErrors.Count}");

// Use EnsureValid to throw exceptions on validation failure
try
{
    ErrorHandlingMiddlewareExtensionsValidation.EnsureValid(errorMessage, context);
    ErrorHandlingMiddlewareExtensionsValidation.EnsureValid(
        operation, 
        "Database connection", 
        maxRetries: 3);
    ErrorHandlingMiddlewareExtensionsValidation.EnsureValid(
        cacheOperation, 
        cachedResultProvider, 
        "Cache fallback operation");
    Console.WriteLine("All validations passed - no exceptions thrown");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}

// Validate exception formatting parameters
var exception = new SqlQueryAnalyzerException("Query failed", "SELECT * FROM Users");
var formatErrors = ErrorHandlingMiddlewareExtensionsValidation.Validate(exception, "Query execution");
Console.WriteLine($"Format validation errors: {formatErrors.Count}");
```

### Public Members

- `Validate(string? errorMessage, string? context)` - Validates parameters for `ErrorHandlingMiddlewareExtensions.CreateErrorReport()` and returns a list of validation problems; empty if valid
- `IsValid(string? errorMessage, string? context)` - Checks if the specified error report parameters are valid
- `EnsureValid(string? errorMessage, string? context)` - Ensures the specified error report parameters are valid, throwing an exception if not
- `Validate<T>(Func<Task<T>>? operation, string? operationName, int maxRetries)` - Validates parameters for `ErrorHandlingMiddlewareExtensions.ExecuteWithRetryAsync<T>()` and returns a list of validation problems; empty if valid
- `IsValid<T>(Func<Task<T>>? operation, string? operationName, int maxRetries)` - Checks if the specified retry operation parameters are valid
- `EnsureValid<T>(Func<Task<T>>? operation, string? operationName, int maxRetries)` - Ensures the specified retry operation parameters are valid, throwing an exception if not
- `Validate(Exception? ex, string? context)` - Validates parameters for `ErrorHandlingMiddlewareExtensions.FormatErrorMessage()` and returns a list of validation problems; empty if valid
- `IsValid(Exception? ex, string? context)` - Checks if the specified exception formatting parameters are valid
- `EnsureValid(Exception? ex, string? context)` - Ensures the specified exception formatting parameters are valid, throwing an exception if not
- `Validate<T>(Func<Task<T>>? operation, Func<T>? cachedResultProvider, string? operationName)` - Validates parameters for `ErrorHandlingMiddlewareExtensions.ExecuteWithCacheFallbackAsync<T>()` and returns a list of validation problems; empty if valid
- `IsValid<T>(Func<Task<T>>? operation, Func<T>? cachedResultProvider, string? operationName)` - Checks if the specified cache fallback operation parameters are valid
- `EnsureValid<T>(Func<Task<T>>? operation, Func<T>? cachedResultProvider, string? operationName)` - Ensures the specified cache fallback operation parameters are valid, throwing an exception if not