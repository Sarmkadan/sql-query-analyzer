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