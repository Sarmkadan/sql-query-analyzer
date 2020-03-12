[![Build](https://github.com/sarmkadan/sql-query-analyzer/actions/workflows/build.yml/badge.svg)](https://github.com/sarmkadan/sql-query-analyzer/actions/workflows/build.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

# SQL Query Analyzer

**Enterprise-grade SQL query analysis tool for detecting performance issues, optimizing queries, and preventing database bottlenecks before they impact production.**

SQL Query Analyzer is a comprehensive .NET 10 tool that provides deep insights into SQL query performance. It detects anti-patterns, suggests index optimizations, analyzes execution plans, and generates actionable recommendations to keep your database running at peak efficiency.

## Key Features

### Query Performance Analysis
- **Comprehensive Scoring**: Rate queries on a 0-100 scale with detailed breakdowns
- **Complexity Assessment**: Classify queries as simple, moderate, or complex
- **Performance Metrics**: Track estimated execution time, logical I/O, CPU cost, and row counts
- **Trend Analysis**: Monitor performance changes over time

### Issue Detection (18+ Issue Types)
- **N+1 Query Patterns**: Detect repeated query patterns in batch operations
- **Missing Indexes**: Identify columns that would benefit from indexing
- **Table Scans**: Find queries that bypass indexes and scan entire tables
- **Join Inefficiencies**: Detect CROSS JOINs, OR conditions in joins, and improper join conditions
- **Implicit Conversions**: Catch type mismatches that prevent index usage
- **Non-SARGABLE Predicates**: Identify WHERE clauses that can't use indexes
- **SELECT * Analysis**: Flag queries selecting all columns without specification
- **Function-on-Column**: Detect functions applied to columns in WHERE clauses
- **LIKE Wildcards**: Identify LIKE patterns with leading wildcards
- **Subquery Optimization**: Find subqueries that could be rewritten as JOINs
- **Index Fragmentation**: Monitor and recommend index maintenance
- **Unused Indexes**: Find redundant indexes consuming resources
- ...and more!

### Index Analysis & Optimization
- **Fragmentation Detection**: Identify indexes with high fragmentation
- **Unused Index Discovery**: Find indexes that are never or rarely used
- **Suggestion Engine**: Automatic index creation recommendations with ROI
- **Maintenance Script Generation**: Auto-generate REBUILD/REORGANIZE scripts
- **Usage Analytics**: Track seeks, scans, lookups, and updates per index

### Execution Plan Analysis
- **Plan Parsing**: Parse and analyze SQL Server, PostgreSQL, and MySQL execution plans
- **Cost Analysis**: Break down estimated costs by operation
- **Bottleneck Identification**: Pinpoint expensive operations
- **Table Access Patterns**: Understand how tables are accessed
- **Join Efficiency**: Evaluate join strategies and costs

### Report Generation (Multiple Formats)
- **Text Reports**: Human-readable analysis with clear recommendations
- **HTML Reports**: Interactive, styled reports viewable in any browser
- **JSON Exports**: Machine-readable format for tool integration
- **CSV Summaries**: Spreadsheet-compatible summaries for trend analysis

---

## Architecture

### System Design

```
┌─────────────────────────────────────────────────────────────┐
│                    SQL Query Analyzer                        │
│                                                              │
│  ┌────────────────────────────────────────────────────────┐ │
│  │                  Query Input Layer                      │ │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐ │ │
│  │  │ Query Text   │  │ Execution    │  │ Query        │ │ │
│  │  │ (String)     │  │ Plans (XML)  │  │ Objects      │ │ │
│  │  └──────────────┘  └──────────────┘  └──────────────┘ │ │
│  └────────────────────────────────────────────────────────┘ │
│                              ↓                                │
│  ┌────────────────────────────────────────────────────────┐ │
│  │              Analysis Engine (Services)                │ │
│  │                                                        │ │
│  │  ┌─────────────────────────────────────────────────┐ │ │
│  │  │  Query Analyzer Service (Orchestrator)          │ │ │
│  │  └─────────────────────────────────────────────────┘ │ │
│  │  ┌─────────────────────────────────────────────────┐ │ │
│  │  │  Pattern Detector       │  Index Analyzer      │ │ │
│  │  │  (18+ Issue Types)      │  (Fragmentation)     │ │ │
│  │  └─────────────────────────────────────────────────┘ │ │
│  │  ┌─────────────────────────────────────────────────┐ │ │
│  │  │  Execution Plan Parser  │  Performance Scorer  │ │ │
│  │  │  (Multi-DB)             │  (Cost Calculator)   │ │ │
│  │  └─────────────────────────────────────────────────┘ │ │
│  └────────────────────────────────────────────────────────┘ │
│                              ↓                                │
│  ┌────────────────────────────────────────────────────────┐ │
│  │           Result Models & Data Structures             │ │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐ │ │
│  │  │ Analysis     │  │ Performance  │  │ Index        │ │ │
│  │  │ Result       │  │ Issues       │  │ Suggestions  │ │ │
│  │  └──────────────┘  └──────────────┘  └──────────────┘ │ │
│  └────────────────────────────────────────────────────────┘ │
│                              ↓                                │
│  ┌────────────────────────────────────────────────────────┐ │
│  │              Output Layer (Export)                      │ │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐ │ │
│  │  │ Text Reports │  │ HTML Reports │  │ JSON/CSV     │ │ │
│  │  └──────────────┘  └──────────────┘  └──────────────┘ │ │
│  └────────────────────────────────────────────────────────┘ │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

### Component Details

**Models Layer** (`/Models`)
- `QueryAnalysisResult`: Complete analysis output with all findings
- `PerformanceIssue`: Individual issue with type, severity, and recommendations
- `IndexSuggestion`: Index creation proposal with ROI estimates
- `QueryPlan`: Parsed execution plan tree with cost metrics
- `DatabaseQuery`: SQL statement with metadata and statistics
- `Index`: Database index with health and usage data
- `QueryStatistics`: Execution metrics (CPU, I/O, row counts)

**Services Layer** (`/Services`)
- `IQueryAnalyzerService`: Main orchestrator, coordinates all analysis
- `IIndexAnalyzerService`: Index health, fragmentation, usage analysis
- `IQueryPlanAnalyzerService`: Execution plan parsing and interpretation
- `IPerformanceIssueDetectorService`: Pattern matching for 18+ issue types
- `IExplainPlanParserService`: Multi-database plan format parsing

**Data Access Layer** (`/Repositories`)
- `IQueryRepository`: Query storage, retrieval, and history
- `IAnalysisRepository`: Result persistence and trend analysis
- `IIndexRepository`: Index suggestions and recommendations

**Utilities** (`/Utilities`)
- `QueryValidator`: Input validation and sanitization
- `PerformanceMetricsCalculator`: Score computation and metric aggregation
- `SqlPatternAnalyzer`: Regex-based SQL pattern recognition
- `ReportGenerator`: Multi-format report generation
- `QueryNormalizer`: Query normalization and standardization
- `SqlInjectionDetector`: Security analysis and threat detection

### Data Flow

```
SQL Input → Validation → Analysis Engine → Detection Pipeline 
  ↓            ↓              ↓                    ↓
Query Text  Validators   Pattern Match      Issue Detection
            Config Mgmt   Cost Analysis      Index Analysis
            DB Connection Exec Plan Parse    Scoring

                            ↓
                    Result Aggregation
                    ↓
                Report Generation
                ↓
            Multiple Output Formats
```

---

## Installation Guide

### System Requirements
- **Runtime**: .NET 10 SDK or later
- **Database**: SQL Server 2016+ OR PostgreSQL 12+ OR MySQL 5.7+
- **RAM**: 512 MB minimum (1 GB recommended)
- **Disk**: 100 MB for installation

### Method 1: From Source (Development)

```bash
# Clone the repository
git clone https://github.com/sarmkadan/sql-query-analyzer.git
cd sql-query-analyzer

# Verify .NET 10 installation
dotnet --version

# Restore NuGet packages
dotnet restore

# Build in Release configuration
dotnet build --configuration Release

# Verify build
dotnet ./bin/Release/net10.0/SqlQueryAnalyzer.dll --help
```

### Method 2: Using Docker Compose

```bash
git clone https://github.com/sarmkadan/sql-query-analyzer.git
cd sql-query-analyzer

# Build and run with default SQL Server
docker-compose up --build

# Or run with PostgreSQL
docker-compose -f docker-compose.yml -f docker-compose.postgres.yml up
```

### Method 3: Building Custom Docker Image

```bash
docker build -t sql-query-analyzer:latest .
docker run -e DB_SERVER=host.docker.internal sql-query-analyzer:latest
```

### Method 4: Package Installation

```bash
# Build NuGet package (if published to nuget.org)
dotnet add package SqlQueryAnalyzer

# In your project file
<ItemGroup>
    <PackageReference Include="SqlQueryAnalyzer" Version="1.0.0" />
</ItemGroup>
```

### Verify Installation

```bash
# Run quick test
dotnet run --project sql-query-analyzer.csproj

# Expected output:
# Starting SQL Query Analyzer v1.0.0
# Analyzing: SELECT * FROM Orders o JOIN Customers c...
# Issues found: 2
#   - SelectStar: SELECT * should specify columns
#   - MissingIndex: Consider index on Orders.CustomerId
```

---

## Usage Examples

### Example 1: Analyze a Single Query

```csharp
using SqlQueryAnalyzer.Services;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection()
    .AddScoped<IQueryAnalyzerService, QueryAnalyzerService>()
    .BuildServiceProvider();

var analyzer = services.GetRequiredService<IQueryAnalyzerService>();

// Analyze a problematic query
var query = @"
    SELECT * FROM Orders o
    WHERE o.CustomerId IN (
        SELECT c.Id FROM Customers c 
        WHERE c.Country = 'USA' AND YEAR(c.CreatedDate) = 2024
    )
";

var result = await analyzer.AnalyzeQueryAsync(query);

Console.WriteLine($"Performance Score: {result.PerformanceScore:F1}/100");
Console.WriteLine($"Complexity: {result.Complexity}");
Console.WriteLine($"Issues Found: {result.Issues.Count}");

foreach (var issue in result.Issues.OrderByDescending(x => x.Severity))
{
    Console.WriteLine($"\n[{issue.Severity}] {issue.IssueType}");
    Console.WriteLine($"Description: {issue.Description}");
    Console.WriteLine($"Fix: {issue.RecommendedFix}");
    Console.WriteLine($"Impact: {issue.ImpactScore}/10");
}
```

**Output:**
```
Performance Score: 52.5/100
Complexity: Complex
Issues Found: 4

[Critical] FunctionOnColumn
Description: YEAR(c.CreatedDate) in WHERE clause prevents index usage
Fix: Use a date range comparison instead: c.CreatedDate >= '2024-01-01'
Impact: 8/10

[Warning] SelectStar
Description: SELECT * includes unnecessary columns
Fix: Specify only needed columns: SELECT o.Id, o.OrderDate, o.CustomerId
Impact: 4/10

[Warning] SubqueryOptimization
Description: IN subquery could be rewritten as JOIN
Fix: Rewrite as: JOIN Customers c ON o.CustomerId = c.Id WHERE c.Country = 'USA'
Impact: 5/10

[Info] MissingIndex
Description: No index on Orders.CustomerId for join condition
Fix: CREATE INDEX ix_orders_customerid ON Orders(CustomerId);
Impact: 6/10
```

### Example 2: Batch Analysis of Multiple Queries

```csharp
var analyzer = services.GetRequiredService<IQueryAnalyzerService>();

var queries = new[]
{
    "SELECT * FROM Orders",
    "SELECT o.*, c.* FROM Orders o JOIN Customers c ON o.CustomerId = c.Id",
    "SELECT COUNT(*) FROM OrderItems WHERE ProductId = 1",
};

var results = new List<QueryAnalysisResult>();

foreach (var query in queries)
{
    var result = await analyzer.AnalyzeQueryAsync(query);
    results.Add(result);
}

// Summarize results
var avgScore = results.Average(r => r.PerformanceScore);
var criticalCount = results.Sum(r => r.Issues.Count(i => i.Severity == IssueSeverity.Critical));

Console.WriteLine($"Average Score: {avgScore:F1}/100");
Console.WriteLine($"Total Critical Issues: {criticalCount}");
Console.WriteLine($"Queries Analyzed: {results.Count}");
```

### Example 3: Index Analysis and Suggestions

```csharp
var indexAnalyzer = services.GetRequiredService<IIndexAnalyzerService>();

// Find fragmented indexes
var fragmented = await indexAnalyzer.GetFragmentedIndexesAsync();
Console.WriteLine($"Fragmented Indexes: {fragmented.Count}");

foreach (var idx in fragmented.Where(x => x.FragmentationPercent > 30))
{
    Console.WriteLine($"  {idx.TableName}.{idx.IndexName}: {idx.FragmentationPercent:F1}%");
    Console.WriteLine($"    Action: REBUILD (fragmentation > 30%)");
}

// Get unused indexes
var unused = await indexAnalyzer.GetUnusedIndexesAsync();
Console.WriteLine($"\nUnused Indexes: {unused.Count}");

foreach (var idx in unused)
{
    Console.WriteLine($"  {idx.TableName}.{idx.IndexName}");
    Console.WriteLine($"    Consider: DROP INDEX if not needed for queries");
}

// Get index suggestions
var suggestions = await indexAnalyzer.AnalyzeIndexesAsync("Orders");
Console.WriteLine($"\nSuggested Indexes for Orders table: {suggestions.Count}");

foreach (var sugg in suggestions.OrderByDescending(x => x.Roi))
{
    Console.WriteLine($"  CREATE INDEX {sugg.SuggestedIndexName}");
    Console.WriteLine($"  ON {sugg.TableName}({string.Join(", ", sugg.Columns)})");
    Console.WriteLine($"  ROI: {sugg.Roi:F1}% | Est. Size: {sugg.EstimatedSizeKB} KB");
}
```

### Example 4: Generate Multiple Report Formats

```csharp
var analyzer = services.GetRequiredService<IQueryAnalyzerService>();
var result = await analyzer.AnalyzeQueryAsync(queryText);

// Generate text report
var textReport = ReportGenerator.GenerateTextReport(result);
await File.WriteAllTextAsync("analysis.txt", textReport);

// Generate HTML report with styling
var htmlReport = ReportGenerator.GenerateHtmlReport(result);
await File.WriteAllTextAsync("analysis.html", htmlReport);

// Generate JSON for programmatic use
var jsonReport = ReportGenerator.GenerateJsonReport(result);
await File.WriteAllTextAsync("analysis.json", jsonReport);

// Generate CSV for spreadsheet analysis
var csvReport = ReportGenerator.GenerateCsvReport(result);
await File.WriteAllTextAsync("analysis.csv", csvReport);

Console.WriteLine("Reports generated:");
Console.WriteLine("  - analysis.txt (text format)");
Console.WriteLine("  - analysis.html (interactive)");
Console.WriteLine("  - analysis.json (machine-readable)");
Console.WriteLine("  - analysis.csv (spreadsheet)");
```

### Example 5: Execution Plan Analysis

```csharp
var planAnalyzer = services.GetRequiredService<IQueryPlanAnalyzerService>();

// Parse SQL Server execution plan XML
var planXml = @"<ShowPlanXML>...</ShowPlanXML>";
var plan = await planAnalyzer.ParsePlanAsync(planXml);

Console.WriteLine($"Total Cost: {plan.TotalCost:F4}");
Console.WriteLine($"Estimated Rows: {plan.EstimatedRows}");
Console.WriteLine($"Root Operation: {plan.RootOperation}");

// Analyze operations by cost
var operations = plan.GetOperationsByTotalCost()
    .Take(5)
    .ToList();

Console.WriteLine("\nMost Expensive Operations:");
foreach (var op in operations)
{
    Console.WriteLine($"  {op.OperationType}: {op.TotalCost:F4}");
    Console.WriteLine($"    Est. Rows: {op.EstimatedRows}");
    Console.WriteLine($"    Est. IO: {op.EstimatedIO:F4}");
}
```

### Example 6: Web API Integration

```csharp
// Startup.cs / Program.cs
services
    .AddControllers()
    .AddScoped<IQueryAnalyzerService, QueryAnalyzerService>();

// Controllers/AnalysisController.cs
[ApiController]
[Route("api/[controller]")]
public class AnalysisController : ControllerBase
{
    private readonly IQueryAnalyzerService _analyzer;

    [HttpPost("analyze")]
    public async Task<IActionResult> AnalyzeQuery([FromBody] AnalysisRequest request)
    {
        var result = await _analyzer.AnalyzeQueryAsync(request.QueryText);
        return Ok(result);
    }

    [HttpPost("analyze-batch")]
    public async Task<IActionResult> AnalyzeBatch([FromBody] string[] queries)
    {
        var results = new List<QueryAnalysisResult>();
        foreach (var query in queries)
        {
            results.Add(await _analyzer.AnalyzeQueryAsync(query));
        }
        return Ok(results);
    }
}

// Usage
var client = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };
var response = await client.PostAsJsonAsync(
    "/api/analysis/analyze",
    new { queryText = "SELECT * FROM Orders" }
);
var result = await response.Content.ReadAsAsync<QueryAnalysisResult>();
```

### Example 7: N+1 Query Detection

```csharp
var detector = services.GetRequiredService<IPerformanceIssueDetectorService>();

// Simulate N+1 pattern
var queries = new List<DatabaseQuery>
{
    new() { QueryText = "SELECT * FROM Orders WHERE CustomerId = 1" },
    new() { QueryText = "SELECT * FROM OrderDetails WHERE OrderId = 1" },
    new() { QueryText = "SELECT * FROM OrderDetails WHERE OrderId = 2" },
    new() { QueryText = "SELECT * FROM OrderDetails WHERE OrderId = 3" },
    // ... 1000 more similar queries
};

var nplusOneIssues = await detector.DetectNPlusOneAsync(queries);

if (nplusOneIssues.Any())
{
    Console.WriteLine($"N+1 Pattern Detected: {nplusOneIssues.Count} issues");
    foreach (var issue in nplusOneIssues)
    {
        Console.WriteLine($"  Query: {issue.Description}");
        Console.WriteLine($"  Fix: {issue.RecommendedFix}");
    }
}
```

---

## Configuration Reference

### Environment Variables

```bash
# Database Configuration
DB_SERVER=localhost              # SQL Server: localhost, [hostname\instance]
DB_PORT=1433                     # SQL Server port (default 1433)
DB_NAME=YourDatabase             # Database name
DB_USER=sa                       # Username
DB_PASSWORD=YourPassword123!     # Password
DB_TIMEOUT=30                    # Connection timeout (seconds)

# PostgreSQL Configuration
DB_PORT=5432                     # PostgreSQL port (default 5432)
DB_USER=postgres                 # PostgreSQL user
DB_PASSWORD=postgres             # PostgreSQL password

# Analyzer Configuration
ANALYZER_LOG_LEVEL=Information   # Logging level: Debug, Info, Warning, Error
ANALYZER_ENABLE_CACHE=true       # Enable result caching
ANALYZER_CACHE_TTL=3600          # Cache time-to-live (seconds)

# Advanced Configuration
ANALYZER_MAX_QUERY_SIZE=100000   # Maximum query size (bytes)
ANALYZER_DETECT_NPLUS_ONE=true   # Enable N+1 detection
ANALYZER_SUGGEST_INDEXES=true    # Enable index suggestions
ANALYZER_PARSE_PLANS=true        # Enable execution plan parsing
```

### Configuration File (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=YourDB;User Id=sa;Password=YourPassword"
  },
  "AnalyzerSettings": {
    "EnableNPlusOneDetection": true,
    "EnableIndexAnalysis": true,
    "EnablePlanAnalysis": true,
    "MaxQuerySize": 100000,
    "CacheEnabled": true,
    "CacheTtlSeconds": 3600,
    "LogLevel": "Information"
  },
  "Database": {
    "Type": "SqlServer",
    "Server": "localhost",
    "Name": "QueryAnalyzer",
    "User": "sa",
    "Password": "YourPassword123!",
    "Timeout": 30
  }
}
```

---

## Performance Scoring System

The analyzer uses a weighted scoring algorithm:

| Component | Weight | Details |
|-----------|--------|---------|
| Critical Issues | -10 pts each | Blocking problems (cross joins, table scans) |
| Warnings | -5 pts each | Significant issues (missing indexes, N+1 patterns) |
| Info Issues | -2 pts each | Minor suggestions (SELECT *, leading wildcards) |
| Optimization Potential | +0.1 per 1% | Improvement from implementing suggestions |
| Base Score | 100 pts | Starting point before deductions |

### Score Interpretation

| Score | Rating | Status | Action Required |
|-------|--------|--------|-----------------|
| 90-100 | Excellent | Optimal performance | Monitor only |
| 75-89 | Good | Minor optimizations available | Review suggestions |
| 60-74 | Acceptable | Moderate improvements needed | Implement recommendations |
| 40-59 | Poor | Significant issues present | Schedule optimization |
| 0-39 | Critical | Severe performance risk | Immediate action needed |

---

## Issue Types Reference

### Critical Severity
- **CrossJoin**: Missing join condition creates Cartesian product
- **TableScan**: Query scans entire table bypassing indexes
- **ImplicitConversion**: Type mismatch prevents index usage

### Warning Severity
- **NPlusOne**: N+1 query pattern detected
- **MissingIndex**: Column could benefit from indexing
- **UnusedIndex**: Index not used but maintained
- **NonSargable**: WHERE clause prevents index utilization
- **IneffectiveJoin**: Join condition not optimized
- **SubqueryOptimization**: Subquery could be JOIN

### Info Severity
- **SelectStar**: SELECT * without column specification
- **LeadingWildcard**: LIKE pattern with leading wildcard
- **FunctionOnColumn**: Function applied to column in WHERE
- **OrCondition**: OR in WHERE prevents index use
- **IndexFragmentation**: Index fragmentation level high

---

## Docker Deployment

### Using Docker Compose (Recommended)

```bash
# SQL Server (default)
docker-compose up

# PostgreSQL
docker-compose -f docker-compose.yml -f docker-compose.postgres.yml up

# MySQL
docker-compose -f docker-compose.yml -f docker-compose.mysql.yml up
```

### Standalone Docker

```bash
# Build image
docker build -t sql-query-analyzer:1.0.0 .

# Run with SQL Server
docker run -e DB_SERVER=sqlserver -e DB_USER=sa sql-query-analyzer:1.0.0

# Run with environment file
docker run --env-file .env sql-query-analyzer:1.0.0
```

---

## Troubleshooting

### Connection Issues

**Problem**: "Cannot connect to database"

```bash
# Check connection string
echo $DB_SERVER $DB_USER

# Test connectivity
dotnet tool install -g dotnet-sqlserver
sqlserver-test localhost 1433

# Verify firewall
netstat -an | grep 1433  # SQL Server
netstat -an | grep 5432  # PostgreSQL
```

### Performance Issues

**Problem**: "Analysis is slow"

- Increase cache TTL: `ANALYZER_CACHE_TTL=7200`
- Enable result caching: `ANALYZER_ENABLE_CACHE=true`
- Reduce query batch size in batch processing

### Memory Issues

**Problem**: "Out of memory on large queries"

```bash
# Increase available memory
docker run -m 2g sql-query-analyzer:latest

# Or in docker-compose.yml
services:
  analyzer:
    mem_limit: 2g
```

### Missing Issues

**Problem**: "Not detecting expected issues"

```bash
# Enable all detectors
ANALYZER_DETECT_NPLUS_ONE=true
ANALYZER_SUGGEST_INDEXES=true
ANALYZER_PARSE_PLANS=true

# Check log level
ANALYZER_LOG_LEVEL=Debug
```

---

## Complete API Reference

### Main Service Interface

```csharp
public interface IQueryAnalyzerService
{
    // Analyze raw query text
    Task<QueryAnalysisResult> AnalyzeQueryAsync(string queryText);
    
    // Analyze DatabaseQuery object with metadata
    Task<QueryAnalysisResult> AnalyzeQueryAsync(DatabaseQuery query);
    
    // Calculate performance score with custom weights
    Task<double> CalculatePerformanceScoreAsync(QueryAnalysisResult analysis, 
        ScoringWeights? weights = null);
    
    // Determine query complexity level
    Task<QueryComplexity> DetermineComplexityAsync(DatabaseQuery query);
    
    // Batch analyze multiple queries
    Task<List<QueryAnalysisResult>> AnalyzeQueriesAsync(List<DatabaseQuery> queries);
    
    // Get analysis history for a query
    Task<List<QueryAnalysisResult>> GetAnalysisHistoryAsync(string queryHash, 
        int limit = 10);
}
```

### Index Analyzer Interface

```csharp
public interface IIndexAnalyzerService
{
    // Analyze all indexes on a table
    Task<List<IndexSuggestion>> AnalyzeIndexesAsync(string tableName);
    
    // Get indexes with high fragmentation
    Task<List<Index>> GetFragmentedIndexesAsync(double threshold = 10.0);
    
    // Find indexes never used
    Task<List<Index>> GetUnusedIndexesAsync();
    
    // Generate maintenance scripts
    Task<List<string>> GenerateMaintenanceScriptsAsync();
    
    // Calculate index ROI
    Task<double> CalculateIndexRoiAsync(IndexSuggestion suggestion);
}
```

---

## Performance & Benchmarks

SQL Query Analyzer is designed to be fast enough to run in CI pipelines and development workflows without impacting throughput.

| Workload | Result |
|----------|--------|
| Single query analysis (simple) | < 5ms |
| Single query analysis (complex, with plan parsing) | < 50ms |
| Batch analysis — 1,000 queries | < 2 seconds |
| Sustained throughput (single core) | ~10,000 queries/sec |
| Memory footprint (typical workload) | < 50 MB RSS |
| Cold startup time (.NET 10, trimmed) | < 200ms |

Measurements taken on a 4-core laptop (Intel i7-1265U) with in-memory caching enabled. Results scale linearly with additional cores when using `BatchAnalysisProcessor` with `MaxDegreeOfParallelism`.

---

## Related Projects

- [dotnet-micro-orm](https://github.com/sarmkadan/dotnet-micro-orm) - High-performance micro-ORM for .NET - compiled expressions, batch operations, change tracking, multi-DB support

### Integration Examples

**Analyze queries emitted by dotnet-micro-orm before executing them:**

```csharp
// Wire the analyzer as a query interceptor in your micro-ORM pipeline
var analyzer = serviceProvider.GetRequiredService<IQueryAnalyzerService>();

// Intercept the query built by the ORM and check it before execution
string sql = ormQueryBuilder.ToSql();
var analysis = await analyzer.AnalyzeQueryAsync(sql);

if (analysis.PerformanceScore < 60)
{
    logger.LogWarning("Low-quality query detected (score {Score}): {Issues}",
        analysis.PerformanceScore,
        string.Join(", ", analysis.Issues.Select(i => i.IssueType)));
}

// Proceed to execute only if acceptable, or surface the warning to the developer
await ormQueryBuilder.ExecuteAsync();
```

**Batch-analyze slow queries captured from ORM change tracking:**

```csharp
// Pull recent queries recorded by the micro-ORM's diagnostics listener
IReadOnlyList<string> recentQueries = ormDiagnostics.GetRecentStatements(limit: 200);

var processor = serviceProvider.GetRequiredService<BatchAnalysisProcessor>();
IReadOnlyList<QueryAnalysisResult> results = await processor.ProcessBatchAsync(
    recentQueries.Select(q => new DatabaseQuery { QueryText = q }).ToList());

var report = ReportGenerator.GenerateTextReport(results.First());
Console.WriteLine(report);
```

---

## Contributing

This is an actively maintained open-source project. Contributions are welcome!

### How to Contribute

1. **Fork** the repository
2. **Create** a feature branch: `git checkout -b feature/your-feature`
3. **Commit** changes: `git commit -am 'Add new feature'`
4. **Push** to branch: `git push origin feature/your-feature`
5. **Submit** a pull request with detailed description

See [CONTRIBUTING.md](CONTRIBUTING.md) for detailed guidelines.

---

## License

MIT License - Copyright © 2026 Vladyslav Zaiets

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software.

See [LICENSE](LICENSE) file for complete details.

---

## Support & Resources

- **Documentation**: [docs/](./docs/)
- **Examples**: [examples/](./examples/)
- **Issues**: [GitHub Issues](https://github.com/sarmkadan/sql-query-analyzer/issues)
- **Discussions**: [GitHub Discussions](https://github.com/sarmkadan/sql-query-analyzer/discussions)

---

**Built by [Vladyslav Zaiets](https://sarmkadan.com) - CTO & Software Architect**

[Portfolio](https://sarmkadan.com) | [GitHub](https://github.com/Sarmkadan) | [Telegram](https://t.me/sarmkadan)
