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

## StringExtensions

The `StringExtensions` class provides utility methods for string manipulation commonly used when processing SQL queries. It includes methods for normalizing whitespace, removing comments, truncating strings, checking for SQL keywords, converting between naming conventions, detecting suspicious patterns, extracting query types, splitting statements, and calculating text positions.

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