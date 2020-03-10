# SQL Query Analyzer

A comprehensive .NET 10 tool for analyzing SQL queries and detecting performance issues, missing indexes, and optimization opportunities.

## Features

- **Query Performance Analysis**: Evaluate query complexity and performance scoring
- **Issue Detection**: Identify common SQL anti-patterns and performance problems
  - N+1 query detection
  - Missing index analysis
  - Table scan detection
  - Inefficient join patterns
  - Function-on-column issues
  - LIKE with leading wildcard detection
  - SELECT * analysis
  - AND more!

- **Index Analysis**: Comprehensive index health and usage analysis
  - Fragmentation detection and maintenance recommendations
  - Unused index identification
  - Index suggestion engine
  - Automatic SQL script generation

- **Execution Plan Analysis**: Parse and analyze SQL Server execution plans
  - Cost analysis
  - Performance bottleneck identification
  - Table access pattern analysis
  - Join efficiency evaluation

- **Report Generation**: Multiple output formats
  - Text reports
  - HTML reports
  - JSON exports
  - CSV summaries

## Architecture

### Project Structure

```
sql-query-analyzer/
├── Models/                 # Domain entities and data models
├── Services/              # Business logic and analysis engines
├── Repositories/          # Data access layer
├── Configuration/         # Database connection management
├── Constants/            # Enums, constants, and defaults
├── Exceptions/           # Custom exception types
├── DTOs/                 # Data transfer objects
├── Utilities/            # Helper classes and extensions
└── Program.cs            # Application entry point
```

### Core Components

#### Models
- **QueryAnalysisResult**: Complete analysis output with issues and suggestions
- **PerformanceIssue**: Detected performance problems with severity and impact
- **IndexSuggestion**: Recommended index creation with ROI calculation
- **QueryPlan**: Parsed execution plan tree with cost metrics
- **DatabaseQuery**: SQL query with metadata and analysis
- **Index**: Database index with health and usage metrics
- **QueryStatistics**: Execution statistics and performance data

#### Services
- **IQueryAnalyzerService**: Main analysis orchestration
- **IIndexAnalyzerService**: Index health and optimization analysis
- **IQueryPlanAnalyzerService**: Execution plan parsing and analysis
- **IPerformanceIssueDetectorService**: Pattern-based issue detection
- **IExplainPlanParserService**: Multi-database plan parsing

#### Repositories
- **IQueryRepository**: Query storage and retrieval
- **IAnalysisRepository**: Analysis result persistence
- **IIndexRepository**: Index and suggestion management

#### Utilities
- **QueryValidator**: Query and configuration validation
- **PerformanceMetricsCalculator**: Score and metric calculations
- **SqlPatternAnalyzer**: Pattern recognition and analysis
- **ReportGenerator**: Multi-format report generation
- **StringExtensions**: SQL string operations

## Getting Started

### Prerequisites
- .NET 10 SDK or later
- SQL Server 2016+ or PostgreSQL 12+ (for database features)

### Installation

```bash
# Clone the repository
git clone https://github.com/sarmkadan/sql-query-analyzer.git
cd sql-query-analyzer

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run
```

### Basic Usage

```csharp
// Analyze a query
var analyzer = serviceProvider.GetRequiredService<IQueryAnalyzerService>();
var result = await analyzer.AnalyzeQueryAsync("SELECT * FROM Orders WHERE CustomerId = 1");

// Check results
Console.WriteLine($"Score: {result.PerformanceScore:F1}/100");
Console.WriteLine($"Issues: {result.Issues.Count}");
Console.WriteLine($"Suggestions: {result.IndexSuggestions.Count}");

// Get detailed recommendations
foreach (var issue in result.Issues)
{
    Console.WriteLine($"- {issue.IssueType}: {issue.Description}");
    Console.WriteLine($"  Fix: {issue.RecommendedFix}");
}
```

### Configuration

Set database connection via environment variables:

```bash
# SQL Server
export DB_SERVER=localhost
export DB_NAME=QueryAnalyzer
export DB_USER=sa
export DB_PASSWORD=YourPassword123!
export DB_TIMEOUT=30

# PostgreSQL
export DB_SERVER=localhost
export DB_NAME=query_analyzer
export DB_PORT=5432
export DB_USER=postgres
export DB_PASSWORD=postgres
```

## API Overview

### QueryAnalyzerService

```csharp
// Analyze query text
Task<QueryAnalysisResult> AnalyzeQueryAsync(string queryText);

// Analyze DatabaseQuery object
Task<QueryAnalysisResult> AnalyzeQueryAsync(DatabaseQuery query);

// Calculate performance score
Task<double> CalculatePerformanceScoreAsync(QueryAnalysisResult analysis);

// Determine complexity
Task<QueryComplexity> DetermineComplexityAsync(DatabaseQuery query);
```

### IndexAnalyzerService

```csharp
// Analyze table indexes
Task<List<IndexSuggestion>> AnalyzeIndexesAsync(string tableName);

// Get fragmented indexes
Task<List<Index>> GetFragmentedIndexesAsync();

// Get unused indexes
Task<List<Index>> GetUnusedIndexesAsync();

// Generate maintenance scripts
Task<List<string>> GenerateMaintenanceScriptsAsync();
```

### PerformanceIssueDetectorService

```csharp
// Detect all issues
Task<List<PerformanceIssue>> DetectIssuesAsync(DatabaseQuery query);

// Detect N+1 patterns
Task<List<PerformanceIssue>> DetectNPlusOneAsync(List<DatabaseQuery> queries);

// Analyze joins
Task<List<PerformanceIssue>> DetectJoinIssuesAsync(DatabaseQuery query);

// Find index opportunities
Task<List<PerformanceIssue>> DetectIndexOpportunitiesAsync(DatabaseQuery query);
```

## Performance Scoring

The analyzer calculates a 0-100 performance score based on:

- **Critical Issues**: -10 points each
- **Warnings**: -5 points each
- **Info Issues**: -2 points each
- **Optimization Potential**: +0.1 per percentage point

### Score Ranges

| Score | Rating | Status |
|-------|--------|--------|
| 90-100 | Excellent | Optimal performance |
| 75-89 | Good | Minor optimizations available |
| 60-74 | Acceptable | Review recommendations |
| 40-59 | Poor | Significant improvements needed |
| 0-39 | Critical | Immediate attention required |

## Issue Types

- **TableScan**: Tables accessed without proper indexes
- **NPlusOne**: N+1 query patterns detected
- **MissingIndex**: Index opportunity identified
- **UnusedIndex**: Redundant indexes consuming resources
- **ImplicitConversion**: Type mismatch in comparisons
- **NonSargable**: Predicate prevents index usage
- **IneffectiveJoin**: Join condition not optimized
- **CrossJoin**: Missing or incomplete join conditions
- **OrCondition**: OR prevents index usage
- **SubqueryOptimization**: Subquery could be optimized
- **IndexFragmentation**: Index fragmentation level high
- **SelectStar**: SELECT * without column specification
- **LeadingWildcard**: LIKE with leading wildcard
- **FunctionOnColumn**: Function applied to column in WHERE
- ...and more

## Report Formats

### Text Report
Human-readable analysis with issue details and recommendations.

### HTML Report
Interactive browser-viewable report with styling and organization.

### JSON Export
Machine-readable format for integration with other tools.

### CSV Summary
Tabular format for spreadsheet analysis and trending.

## Examples

### Analyze Single Query

```csharp
var result = await analyzer.AnalyzeQueryAsync(
    "SELECT * FROM Orders WHERE OrderDate > GETDATE() - 30"
);

Console.WriteLine(result.GetSummary());
// Output: Score: 65.0/100 | Issues: 2 (0 critical, 2 warnings, 0 info) | Optimization: 25.0%
```

### Batch Analysis

```csharp
var builder = new BatchAnalysisBuilder()
    .AddQueries(queries)
    .WithApplication("MyApp")
    .AnalyzePatterns(true)
    .WithTimeout(300);

var request = builder.Build();
var batchResult = await batchAnalyzer.AnalyzeBatchAsync(request);
```

### Generate Reports

```csharp
// Text report
var textReport = ReportGenerator.GenerateTextReport(result);
File.WriteAllText("analysis.txt", textReport);

// HTML report
var htmlReport = ReportGenerator.GenerateHtmlReport(result);
File.WriteAllText("analysis.html", htmlReport);

// JSON export
var jsonReport = ReportGenerator.GenerateJsonReport(result);
File.WriteAllText("analysis.json", jsonReport);
```

## Performance Metrics

The analyzer tracks:

- **Execution Time**: Estimated vs. actual performance
- **Logical I/O**: Page reads required for execution
- **Physical I/O**: Disk reads from buffer cache misses
- **CPU Cost**: Estimated processor utilization
- **Row Counts**: Result set sizes and estimation accuracy
- **Index Usage**: Seeks, scans, lookups, and updates

## Database Support

### Tested & Supported
- SQL Server 2016, 2017, 2019, 2022
- PostgreSQL 12, 13, 14, 15
- MySQL 5.7, 8.0

### Partial Support
- Oracle Database
- SQLite (limited features)

## Building & Testing

```bash
# Build in Release mode
dotnet build --configuration Release

# Run with verbose logging
dotnet run --verbosity diagnostic

# Format code
dotnet format

# Static analysis
dotnet sonaranalyzer
```

## Contributing

This is a solo-authored project. For contributions, feature requests, or bug reports, please visit the repository.

## License

MIT License - Copyright © 2026 Vladyslav Zaiets

See LICENSE file for details.

## Author

**Vladyslav Zaiets**
- CTO & Software Architect
- Website: https://sarmkadan.com
- Email: rutova2@gmail.com

---

For more information and documentation, visit the repository wiki.
