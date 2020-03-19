# Migration Guide to v2.0

## Breaking Changes

### Removed Deprecated Methods
* The following methods have been removed in v2.0:
  - `IQueryAnalyzerService.OldAnalyzeMethod()` - replaced with enhanced analysis engine
  - `IQueryAnalyzerService.LegacyIndexSuggestion()` - replaced with new IndexSuggestion API
  - `QueryValidator.SimpleValidation()` - replaced with enhanced validation system

### Configuration Changes
* Default database connection configuration has been updated to support multiple database types
* The configuration system now uses a new provider model that supports SQL Server, PostgreSQL, and MySQL

### API Changes
* The `/analyze` endpoint now requires authentication by default
* Rate limiting has been implemented with a new default of 100 requests per minute
* The response format has been updated to include execution plan visualization data

---

## New v2.0 Features Overview

### Query Profiler with Execution Plan Visualization
The major new feature in v2.0 is the advanced query profiler that provides:
- Execution plan parsing and visualization for all major database systems
- Real-time query performance suggestions
- Enhanced index suggestion algorithms
- Advanced statistics collection

### Enhanced Index Analysis
- New fragmentation detection algorithms
- Advanced index suggestion engine with ROI calculations
- Unused index detection with cost analysis

### Performance Improvements
- 5x faster query analysis
- Improved caching system with configurable TTL
- Parallel processing for batch operations

### Security Enhancements
- Built-in API key authentication
- Enhanced input validation
- Rate limiting at the infrastructure level

---

## Step-by-Step Migration Process

### 1. Update Dependencies
Update your project references to use the new v2.0 packages:

```xml
<PackageReference Include="SqlQueryAnalyzer" Version="2.0.0" />
```

### 2. Update Configuration
Replace your configuration with the new provider model:

**Before (v1.x)**:
```json
{
  "AnalyzerSettings": {
    "EnableNPlusOneDetection": true,
    "EnableIndexAnalysis": true,
    "MaxQuerySize": 100000
  }
}
```

**After (v2.0)**:
```json
{
  "AnalyzerSettings": {
    "EnableNPlusOneDetection": true,
    "EnableIndexAnalysis": true,
    "EnablePlanAnalysis": true,
    "MaxQuerySize": 100000,
    "EnableExecutionPlanVisualization": true
  }
}
```

### 3. Update Connection Strings
Update your connection strings to support the new multi-database configuration:

**SQL Server**:
```
Server=your-server;Database=your-db;User Id=your-user;Password=your-password;
```

**PostgreSQL**:
```
Host=your-host;Port=5432;Database=your-db;Username=your-user;Password=your-password;
```

**MySQL**:
```
Server=your-server;Database=your-db;Uid=your-user;Pwd=your-password;
```

### 4. Update Code References

**Before (v1.x)**:
```csharp
var result = await analyzer.AnalyzeQueryAsync("SELECT * FROM Orders");
Console.WriteLine($"Score: {result.PerformanceScore}");
```

**After (v2.0)**:
```csharp
var services = new ServiceCollection()
    .AddScoped<IQueryAnalyzerService, QueryAnalyzerService>()
    .AddScoped<IQueryPlanAnalyzerService, QueryPlanAnalyzerService>()
    .BuildServiceProvider();

var analyzer = services.GetRequiredService<IQueryAnalyzerService>();
var planAnalyzer = services.GetRequiredService<IQueryPlanAnalyzerService>();

var result = await analyzer.AnalyzeQueryAsync("SELECT * FROM Orders");
var plan = await planAnalyzer.ParsePlanAsync(result.ExecutionPlanXml);

Console.WriteLine($"Score: {result.PerformanceScore}/100");
Console.WriteLine($"Plan Visualization: {plan.Visualization}");
```

### 5. API Endpoint Changes

**Before (v1.x)**:
```
POST /api/analyze
{
  "query": "SELECT * FROM Orders WHERE CustomerId = 1"
}
```

**After (v2.0)**:
```
POST /api/v2/analyze
Headers: 
  Authorization: <your-api-key>
  Content-Type: application/json

{
  "query": "SELECT * FROM Orders WHERE CustomerId = 1",
  "databaseType": "SqlServer",
  "includeExecutionPlan": true,
  "includeVisualization": true
}
```

### 6. Response Format Changes

**Before (v1.x)**:
```json
{
  "performanceScore": 85.5,
  "issues": [
    {
      "type": "MissingIndex",
      "description": "No index on Orders.CustomerId",
      "severity": "Warning"
    }
  ]
}
```

**After (v2.0)**:
```json
{
  "performanceScore": 85.5,
  "complexity": "Moderate",
  "issues": [
    {
      "type": "MissingIndex",
      "description": "No index on Orders.CustomerId",
      "severity": "Warning",
      "recommendedFix": "CREATE INDEX IX_Orders_CustomerId ON Orders(CustomerId)",
      "impactScore": 7
    }
  ],
  "indexSuggestions": [
    {
      "name": "IX_Orders_CustomerId",
      "columns": ["CustomerId"],
      "roi": 85.5
    }
  ],
  "executionPlan": {
    "totalCost": 15.2,
    "operations": []
  },
  "visualization": "graphical-plan-data-here"
}
```

---

## Code Examples: Old vs New API

### Basic Usage

**v1.0 API**:
```csharp
var analyzer = new QueryAnalyzer();
var result = analyzer.Analyze("SELECT * FROM Orders");
Console.WriteLine($"Issues: {result.Issues.Count}");
```

**v2.0 API**:
```csharp
// v2.0 implementation
var analyzer = new QueryAnalyzerService();
var result = await analyzer.AnalyzeQueryAsync("SELECT * FROM Orders");
var plan = await analyzer.PlanAnalyzer.ParsePlanAsync(result.QueryPlanXml);
Console.WriteLine($"Score: {result.PerformanceScore}/100");
Console.WriteLine($"Visualization: {plan.Visualization}");
```

### Advanced Configuration

**v1.0 Configuration**:
```csharp
var config = new AnalyzerConfiguration 
{
  EnableNPlusOneDetection = true,
  MaxQuerySize = 100000
};
```

**v2.0 Configuration**:
```csharp
var config = new AnalyzerConfiguration 
{
  EnableNPlusOneDetection = true,
  EnableIndexAnalysis = true,
  EnablePlanAnalysis = true,
  MaxQuerySize = 100000,
  EnableExecutionPlanVisualization = true,
  DatabaseProvider = DatabaseProvider.SqlServer
};
```

---

## Configuration Changes

### New Settings in v2.0

1. **EnableExecutionPlanVisualization** - Enable the new execution plan visualization feature
2. **DatabaseProvider** - Specify which database provider to use (SQL Server, PostgreSQL, MySQL)
3. **MaxDegreeOfParallelism** - Control parallel processing for batch operations
4. **EnableAdvancedIndexing** - Enable advanced index suggestion algorithms

### Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `ANALYZER_ENABLE_PLAN_VISUALIZATION` | true | Enable execution plan visualization |
| `ANALYZER_DATABASE_PROVIDER` | `SQLServer` | Database provider selection |
| `ANALYZER_MAX_PARALLELISM` | 4 | Maximum degree of parallelism |
| `ANALYZER_ENABLE_ADVANCED_INDEXING` | true | Enable advanced indexing |

### Updated Response Format

The new API provides enhanced response data including execution plan visualization:

```json
{
  "queryId": "550e8400-e29b-41d4-a716-446655440000",
  "queryText": "SELECT * FROM Orders WHERE CustomerId = 1",
  "performanceScore": 85.5,
  "complexity": "Simple",
  "issues": [
    {
      "type": "SelectStar",
      "severity": "Info",
      "description": "Query uses SELECT *",
      "lineNumber": 1,
      "columnNumber": 8,
      "recommendedFix": "Specify only needed columns: SELECT Id, OrderDate, CustomerId FROM Orders"
    }
  ],
  "indexSuggestions": [
    {
      "name": "IX_Orders_CustomerId",
      "columns": ["CustomerId"],
      "estimatedImprovement": "25%",
      "roi": 7.2
    }
  ],
  "executionPlan": {
    "totalCost": 15.2,
    "estimatedRows": 1000,
    "visualization": "base64-encoded-visualization-data"
  }
}
```

---

## Summary

v2.0 represents a major architectural upgrade with enhanced features for query analysis, including:
- Native support for multiple database systems (SQL Server, PostgreSQL, MySQL)
- Execution plan visualization features
- Enhanced performance issue detection
- Improved index for the new execution plan visualization
- Breaking changes to method signatures and configuration options
- New response formats with detailed execution plan data

For any issues with the migration, please consult the documentation or reach out to our support team.