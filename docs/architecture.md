// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Architecture Guide

Deep dive into the architecture, design patterns, and internal structure of SQL Query Analyzer.

## High-Level Architecture

```
┌─────────────────────────────────────────────┐
│         Input Processing Layer              │
│  - Query Text Validation                    │
│  - Plan XML Parsing                         │
│  - Configuration Loading                    │
└─────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────┐
│      Analysis Engine (Services)             │
│  ┌───────────────────────────────────────┐  │
│  │  Query Analyzer Service               │  │
│  │  (Main Orchestrator)                  │  │
│  │                                       │  │
│  │  Coordinates:                         │  │
│  │  - Pattern Detection                  │  │
│  │  - Index Analysis                     │  │
│  │  - Plan Parsing                       │  │
│  │  - Score Calculation                  │  │
│  └───────────────────────────────────────┘  │
└─────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────┐
│       Analysis Result Models                │
│  - QueryAnalysisResult                      │
│  - PerformanceIssue                         │
│  - IndexSuggestion                          │
│  - QueryStatistics                          │
└─────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────┐
│        Report Generation Layer              │
│  - Text Formatter                           │
│  - HTML Formatter                           │
│  - JSON Serializer                          │
│  - CSV Exporter                             │
└─────────────────────────────────────────────┘
```

## Core Components

### 1. Models Layer (`/Models`)

**Purpose**: Domain objects representing data structures

**Key Classes**:

- **QueryAnalysisResult**: Complete analysis output
  - Properties: `PerformanceScore`, `Issues`, `IndexSuggestions`, `Statistics`
  - Methods: `GetSummary()`, `GetDetailedReport()`

- **PerformanceIssue**: Individual detected problem
  - Properties: `IssueType`, `Severity`, `Description`, `RecommendedFix`, `ImpactScore`
  - Types: SelectStar, MissingIndex, NPlusOne, TableScan, etc.

- **IndexSuggestion**: Index optimization recommendation
  - Properties: `SuggestedIndexName`, `Columns`, `EstimatedSizeKB`, `Roi`
  - Methods: `ToCreateIndexSql()`, `GetROI()`

- **QueryPlan**: Parsed execution plan
  - Properties: `TotalCost`, `RootOperation`, `EstimatedRows`
  - Methods: `GetOperationsByTotalCost()`, `FindExpensiveOperations()`

**Design Pattern**: Value Objects and Aggregates

### 2. Services Layer (`/Services`)

**Purpose**: Business logic and analysis algorithms

**Architecture Pattern**: Strategy Pattern with Dependency Injection

#### IQueryAnalyzerService (Main Orchestrator)
```
┌─────────────────────────────────────┐
│  IQueryAnalyzerService              │
│                                     │
│  AnalyzeQueryAsync(query)           │
│    ↓                                │
│    1. Validate input                │
│    2. Create DatabaseQuery model    │
│    3. Call pattern detectors        │
│    4. Call index analyzer           │
│    5. Calculate performance score   │
│    6. Aggregate results             │
│    7. Return QueryAnalysisResult    │
└─────────────────────────────────────┘
```

#### Specialized Services

- **IPerformanceIssueDetectorService**: 18+ detection patterns
  - Pattern matching using regex and SQL parsing
  - Returns `List<PerformanceIssue>`

- **IIndexAnalyzerService**: Index health and suggestions
  - Fragmentation analysis
  - Unused index detection
  - Suggestion generation with ROI

- **IQueryPlanAnalyzerService**: Execution plan analysis
  - XML parsing and tree traversal
  - Cost calculation and breakdown
  - Operation classification

- **IExplainPlanParserService**: Multi-database support
  - SQL Server: XML format
  - PostgreSQL: EXPLAIN (ANALYZE, BUFFERS)
  - MySQL: EXPLAIN EXTENDED

### 3. Data Access Layer (`/Repositories`)

**Pattern**: Repository Pattern with abstraction

```csharp
public interface IQueryRepository
{
    Task<DatabaseQuery> GetByIdAsync(Guid id);
    Task<List<DatabaseQuery>> GetByTextAsync(string text);
    Task SaveAsync(DatabaseQuery query);
    Task<List<DatabaseQuery>> GetRecentAsync(int count);
}
```

**Implementations**:
- In-memory (for development)
- SQL Server (production)
- PostgreSQL (multi-DB support)

### 4. Utilities & Helpers

**Purpose**: Cross-cutting concerns and helper functions

- **QueryValidator**: Input validation
  - Query syntax check
  - Size limits
  - Injection detection

- **PerformanceMetricsCalculator**: Score computation
  - Weighted scoring algorithm
  - Metric aggregation
  - Baseline comparison

- **QueryNormalizer**: Query standardization
  - Whitespace normalization
  - Case standardization
  - Fingerprinting for pattern matching

- **ReportGenerator**: Multi-format output
  - Template-based generation
  - Format-specific rendering
  - Styling and markup

## Data Flow Example

**Analyzing a Query**:

```
User Input: "SELECT * FROM Orders WHERE CustomerId = 1"
    ↓
QueryAnalyzerService.AnalyzeQueryAsync()
    ↓
1. QueryValidator.ValidateQuery()
   - Check syntax, size, injection
    ↓
2. Create DatabaseQuery model
    ↓
3. IPerformanceIssueDetectorService
   - Regex: /SELECT\s*\*/ → SelectStar issue
   - Pattern: WHERE on non-indexed column → MissingIndex issue
    ↓
4. IIndexAnalyzerService
   - Query database metadata
   - Analyze index fragmentation
   - Generate suggestions
    ↓
5. PerformanceMetricsCalculator
   - SelectStar: -5 points
   - MissingIndex: -10 points
   - Base: 100 points
   - Score: 85/100
    ↓
6. Aggregate Results
   - Combine all issues
   - Sort by severity
   - Calculate statistics
    ↓
Return: QueryAnalysisResult
  {
    PerformanceScore: 85.0,
    Issues: [SelectStar, MissingIndex],
    Suggestions: [CREATE INDEX ix_orders_customerid],
    Statistics: {...}
  }
```

## Design Patterns Used

### 1. Dependency Injection (DI)
```csharp
services.AddScoped<IQueryAnalyzerService, QueryAnalyzerService>();
services.AddScoped<IIndexAnalyzerService, IndexAnalyzerService>();
```

### 2. Strategy Pattern
Different analysis strategies for different issue types:
```csharp
private readonly List<IAnalysisStrategy> _strategies;

foreach (var strategy in _strategies)
{
    issues.AddRange(await strategy.AnalyzeAsync(query));
}
```

### 3. Repository Pattern
Abstract data access:
```csharp
var query = await _queryRepository.GetByTextAsync(queryText);
```

### 4. Decorator Pattern
Add functionality to core analysis:
```csharp
var cachedAnalyzer = new CachedQueryAnalyzer(_baseAnalyzer);
var loggedAnalyzer = new LoggedQueryAnalyzer(cachedAnalyzer);
```

### 5. Template Method Pattern
Report generation:
```csharp
public abstract class ReportFormatterBase
{
    public string Generate(QueryAnalysisResult result)
    {
        var header = GenerateHeader();
        var body = GenerateBody(result);
        var footer = GenerateFooter();
        return $"{header}\n{body}\n{footer}";
    }
    
    protected abstract string GenerateHeader();
    protected abstract string GenerateBody(QueryAnalysisResult result);
}
```

## Performance Considerations

### Caching Strategy
```csharp
// Query fingerprint-based caching
var fingerprint = QueryNormalizer.GetFingerprint(query);
var cached = await _cache.GetAsync(fingerprint);
if (cached != null) return cached;

var result = await AnalyzeAsync(query);
await _cache.SetAsync(fingerprint, result, TimeSpan.FromHours(1));
return result;
```

### Batch Processing
```csharp
// Parallel analysis for independent queries
var tasks = queries.Select(q => AnalyzeQueryAsync(q));
var results = await Task.WhenAll(tasks);
```

### Database Connection Pooling
```csharp
// Configured in SqlServerConfiguration
"Connection Pooling=true;Max Pool Size=100"
```

## Extension Points

### Adding Custom Issue Detectors

```csharp
public class CustomIssueDetector : IAnalysisStrategy
{
    public async Task<List<PerformanceIssue>> AnalyzeAsync(DatabaseQuery query)
    {
        if (query.QueryText.Contains("YOUR_PATTERN"))
        {
            return new List<PerformanceIssue>
            {
                new PerformanceIssue
                {
                    IssueType = "CustomIssueType",
                    Severity = IssueSeverity.Warning,
                    Description = "Description",
                    RecommendedFix = "Fix recommendation"
                }
            };
        }
        return new List<PerformanceIssue>();
    }
}

// Register in DI
services.AddScoped<IAnalysisStrategy, CustomIssueDetector>();
```

### Custom Report Format

```csharp
public class CustomReportFormatter : ReportFormatterBase
{
    public override string Format(QueryAnalysisResult result)
    {
        // Custom formatting logic
        return CustomFormat(result);
    }
}
```

## Testing Architecture

### Unit Tests
- Isolated service testing
- Mock repositories and configuration
- Pattern matching verification

### Integration Tests
- Real database connections
- Full analysis pipeline
- Report generation validation

### Performance Tests
- Large query set analysis
- Memory usage profiling
- Throughput benchmarking

---

**Architecture Version**: 1.0  
**Last Updated**: 2026-05-04
