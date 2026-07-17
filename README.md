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