// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Complete API Reference

Comprehensive reference for all public interfaces and methods in SQL Query Analyzer.

## IQueryAnalyzerService

Main interface for query analysis.

### Methods

#### AnalyzeQueryAsync(string queryText)

```csharp
Task<QueryAnalysisResult> AnalyzeQueryAsync(string queryText);
```

**Parameters**:
- `queryText` (string): SQL query to analyze

**Returns**: `QueryAnalysisResult` containing analysis findings

**Example**:
```csharp
var result = await analyzer.AnalyzeQueryAsync("SELECT * FROM Orders");
Console.WriteLine($"Score: {result.PerformanceScore}");
```

#### AnalyzeQueryAsync(DatabaseQuery query)

```csharp
Task<QueryAnalysisResult> AnalyzeQueryAsync(DatabaseQuery query);
```

**Parameters**:
- `query` (DatabaseQuery): Query object with metadata

**Returns**: `QueryAnalysisResult` with enhanced analysis

**Example**:
```csharp
var dbQuery = new DatabaseQuery 
{ 
    QueryText = "SELECT * FROM Orders",
    ApplicationName = "MyApp",
    ExecutionTimeMs = 150
};
var result = await analyzer.AnalyzeQueryAsync(dbQuery);
```

#### CalculatePerformanceScoreAsync(QueryAnalysisResult analysis, ScoringWeights weights)

```csharp
Task<double> CalculatePerformanceScoreAsync(
    QueryAnalysisResult analysis, 
    ScoringWeights? weights = null);
```

**Parameters**:
- `analysis` (QueryAnalysisResult): Analysis result to score
- `weights` (ScoringWeights): Custom scoring weights (optional)

**Returns**: Performance score (0-100)

**Example**:
```csharp
var result = await analyzer.AnalyzeQueryAsync(query);
var customWeights = new ScoringWeights 
{ 
    CriticalPenalty = 15,
    WarningPenalty = 3
};
var score = await analyzer.CalculatePerformanceScoreAsync(result, customWeights);
```

#### DetermineComplexityAsync(DatabaseQuery query)

```csharp
Task<QueryComplexity> DetermineComplexityAsync(DatabaseQuery query);
```

**Returns**: QueryComplexity enum (Simple, Moderate, Complex)

#### AnalyzeQueriesAsync(List<DatabaseQuery> queries)

```csharp
Task<List<QueryAnalysisResult>> AnalyzeQueriesAsync(
    List<DatabaseQuery> queries);
```

**Parameters**:
- `queries` (List<DatabaseQuery>): Multiple queries to analyze

**Returns**: List of analysis results

#### GetAnalysisHistoryAsync(string queryHash, int limit)

```csharp
Task<List<QueryAnalysisResult>> GetAnalysisHistoryAsync(
    string queryHash, 
    int limit = 10);
```

**Parameters**:
- `queryHash` (string): Query fingerprint/hash
- `limit` (int): Number of results to return

**Returns**: Historical analysis results

---

## IIndexAnalyzerService

Interface for index analysis and optimization.

### Methods

#### AnalyzeIndexesAsync(string tableName)

```csharp
Task<List<IndexSuggestion>> AnalyzeIndexesAsync(string tableName);
```

**Returns**: List of suggested indexes for the table

**Example**:
```csharp
var suggestions = await indexAnalyzer.AnalyzeIndexesAsync("Orders");
foreach (var suggestion in suggestions)
{
    Console.WriteLine($"CREATE INDEX {suggestion.SuggestedIndexName}");
}
```

#### GetFragmentedIndexesAsync(double threshold)

```csharp
Task<List<Index>> GetFragmentedIndexesAsync(double threshold = 10.0);
```

**Parameters**:
- `threshold` (double): Fragmentation percentage threshold

**Returns**: List of fragmented indexes

**Example**:
```csharp
var fragmented = await indexAnalyzer.GetFragmentedIndexesAsync(20.0);
foreach (var idx in fragmented)
{
    Console.WriteLine($"{idx.TableName}.{idx.IndexName}: {idx.FragmentationPercent}%");
}
```

#### GetUnusedIndexesAsync()

```csharp
Task<List<Index>> GetUnusedIndexesAsync();
```

**Returns**: Indexes with zero seeks/scans/lookups

#### GenerateMaintenanceScriptsAsync()

```csharp
Task<List<string>> GenerateMaintenanceScriptsAsync();
```

**Returns**: SQL scripts for index maintenance (REBUILD/REORGANIZE)

#### CalculateIndexRoiAsync(IndexSuggestion suggestion)

```csharp
Task<double> CalculateIndexRoiAsync(IndexSuggestion suggestion);
```

**Returns**: ROI percentage for the suggested index

---

## IPerformanceIssueDetectorService

Detects performance problems and anti-patterns.

### Methods

#### DetectIssuesAsync(DatabaseQuery query)

```csharp
Task<List<PerformanceIssue>> DetectIssuesAsync(DatabaseQuery query);
```

**Returns**: All detected performance issues

#### DetectNPlusOneAsync(List<DatabaseQuery> queries)

```csharp
Task<List<PerformanceIssue>> DetectNPlusOneAsync(
    List<DatabaseQuery> queries);
```

**Returns**: N+1 pattern issues

**Example**:
```csharp
var queries = new List<DatabaseQuery>
{
    new() { QueryText = "SELECT * FROM Orders" },
    new() { QueryText = "SELECT * FROM OrderDetails WHERE OrderId = 1" },
    // ... more similar queries
};
var nPlusOneIssues = await detector.DetectNPlusOneAsync(queries);
```

#### DetectJoinIssuesAsync(DatabaseQuery query)

```csharp
Task<List<PerformanceIssue>> DetectJoinIssuesAsync(DatabaseQuery query);
```

**Returns**: JOIN-related issues (CROSS JOIN, OR conditions, etc.)

#### DetectIndexOpportunitiesAsync(DatabaseQuery query)

```csharp
Task<List<PerformanceIssue>> DetectIndexOpportunitiesAsync(
    DatabaseQuery query);
```

**Returns**: Missing index opportunities

---

## IQueryPlanAnalyzerService

Analyzes SQL execution plans.

### Methods

#### ParsePlanAsync(string planXml)

```csharp
Task<QueryPlan> ParsePlanAsync(string planXml);
```

**Parameters**:
- `planXml` (string): XML execution plan

**Returns**: Parsed QueryPlan object

**Example**:
```csharp
var planXml = File.ReadAllText("execution-plan.xml");
var plan = await planAnalyzer.ParsePlanAsync(planXml);
Console.WriteLine($"Total Cost: {plan.TotalCost}");
```

#### AnalyzePlanAsync(QueryPlan plan)

```csharp
Task<List<PerformanceIssue>> AnalyzePlanAsync(QueryPlan plan);
```

**Returns**: Issues found in the execution plan

#### GetOperationsByTotalCostAsync(QueryPlan plan)

```csharp
Task<List<PlanOperation>> GetOperationsByTotalCostAsync(QueryPlan plan);
```

**Returns**: Operations sorted by cost (descending)

#### FindBottlenecksAsync(QueryPlan plan)

```csharp
Task<List<PlanBottleneck>> FindBottlenecksAsync(QueryPlan plan);
```

**Returns**: Identified performance bottlenecks

---

## IExplainPlanParserService

Multi-database execution plan parsing.

### Methods

#### ParseSqlServerPlanAsync(string xmlPlan)

```csharp
Task<QueryPlan> ParseSqlServerPlanAsync(string xmlPlan);
```

#### ParsePostgreSqlPlanAsync(string textPlan)

```csharp
Task<QueryPlan> ParsePostgreSqlPlanAsync(string textPlan);
```

#### ParseMySqlPlanAsync(string jsonPlan)

```csharp
Task<QueryPlan> ParseMySqlPlanAsync(string jsonPlan);
```

---

## Data Models

### QueryAnalysisResult

```csharp
public class QueryAnalysisResult
{
    public Guid Id { get; set; }
    public string QueryText { get; set; }
    public double PerformanceScore { get; set; }
    public QueryComplexity Complexity { get; set; }
    public List<PerformanceIssue> Issues { get; set; }
    public List<IndexSuggestion> IndexSuggestions { get; set; }
    public QueryStatistics Statistics { get; set; }
    public DateTime AnalyzedAt { get; set; }
    
    public string GetSummary();
    public List<PerformanceIssue> GetIssuesByServerity(IssueSeverity severity);
}
```

### PerformanceIssue

```csharp
public class PerformanceIssue
{
    public string IssueType { get; set; }
    public IssueSeverity Severity { get; set; }
    public string Description { get; set; }
    public string RecommendedFix { get; set; }
    public int ImpactScore { get; set; }  // 1-10
    public int LineNumber { get; set; }
    public int ColumnNumber { get; set; }
}

public enum IssueSeverity
{
    Critical,
    Warning,
    Info
}
```

### IndexSuggestion

```csharp
public class IndexSuggestion
{
    public string SuggestedIndexName { get; set; }
    public string TableName { get; set; }
    public List<string> Columns { get; set; }
    public List<string>? IncludedColumns { get; set; }
    public double Roi { get; set; }
    public long EstimatedSizeKB { get; set; }
    public int EstimatedImprovementPercent { get; set; }
    
    public string ToCreateIndexSql();
}
```

### QueryPlan

```csharp
public class QueryPlan
{
    public PlanOperation RootOperation { get; set; }
    public double TotalCost { get; set; }
    public long EstimatedRows { get; set; }
    public double EstimatedIO { get; set; }
    public double EstimatedCPU { get; set; }
    public string PlanSource { get; set; }  // SQL Server, PostgreSQL, MySQL
    
    public List<PlanOperation> GetOperationsByTotalCost();
    public List<PlanOperation> FindOperations(string operationType);
}
```

### DatabaseQuery

```csharp
public class DatabaseQuery
{
    public Guid Id { get; set; }
    public string QueryText { get; set; }
    public string? ApplicationName { get; set; }
    public string? DatabaseName { get; set; }
    public long? EstimatedRows { get; set; }
    public int? ExecutionTimeMs { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### QueryStatistics

```csharp
public class QueryStatistics
{
    public double EstimatedExecutionTimeMs { get; set; }
    public double EstimatedLogicalIO { get; set; }
    public double EstimatedPhysicalIO { get; set; }
    public double EstimatedCpuCost { get; set; }
    public long EstimatedRows { get; set; }
    public long EstimatedRowsRead { get; set; }
    public int JoinCount { get; set; }
    public int SubqueryCount { get; set; }
    public bool HasUnionAll { get; set; }
    public bool HasCTE { get; set; }
}
```

---

## Utility Classes

### ReportGenerator

```csharp
public static class ReportGenerator
{
    public static string GenerateTextReport(QueryAnalysisResult result);
    public static string GenerateHtmlReport(QueryAnalysisResult result);
    public static string GenerateJsonReport(QueryAnalysisResult result);
    public static string GenerateCsvReport(QueryAnalysisResult result);
}
```

### QueryValidator

```csharp
public static class QueryValidator
{
    public static bool IsValid(string query);
    public static ValidationResult Validate(string query);
    public static bool ContainsSqlInjectionPattern(string query);
}
```

### PerformanceMetricsCalculator

```csharp
public static class PerformanceMetricsCalculator
{
    public static double CalculateScore(QueryAnalysisResult result);
    public static double CalculateScore(QueryAnalysisResult result, ScoringWeights weights);
    public static QueryComplexity DetermineComplexity(string query);
}
```

---

**API Version**: 1.0  
**Last Updated**: 2026-05-04
