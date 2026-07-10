# IQueryAnalyzerService

Provides asynchronous analysis of SQL queries, delivering performance scores, complexity classifications, and detailed analysis results. The service is designed to be lightweight and suitable for integration into diagnostic tools, query editors, or monitoring pipelines.

## API

### QueryAnalyzerService()
Initializes a new instance of the analyzer service. The constructor requires no parameters and prepares any internal state needed for subsequent analysis operations.

### AnalyzeQueryAsync(...)
Analyzes a supplied query and returns a `QueryAnalysisResult` containing structural and semantic information.  
- **Purpose:** Produces a comprehensive breakdown of the query, including tables, columns, predicates, and estimated cost metrics.  
- **Parameters:** The overloads accept either a raw SQL string or a parsed query model; the exact type varies between overloads.  
- **Return Value:** A `Task<QueryAnalysisResult>` that completes with the analysis outcome.  
- **Exceptions:** Throws `ArgumentNullException` if the query argument is null; may throw `InvalidOperationException` if the query cannot be parsed; propagates any `OperationCanceledException` associated with a supplied cancellation token.

### AnalyzeQueryAsync(...)
Second overload of the query analysis method, offering an alternative input format (e.g., a pre‑parsed query object) while providing the same result type as the first overload.  
- **Purpose:** Allows callers to avoid re‑parsing when a query model is already available.  
- **Parameters:** Accepts a query model or similar representation; see the first overload for semantic equivalence.  
- **Return Value:** A `Task<QueryAnalysisResult>` with the analysis details.  
- **Exceptions:** Same as the first overload.

### CalculatePerformanceScoreAsync(...)
Computes a numeric performance score for a previously analyzed query.  
- **Purpose:** Provides a relative measure (higher is better) that can be used for ranking or alerting.  
- **Parameters:** Takes the `QueryAnalysisResult` produced by `AnalyzeQueryAsync`.  
- **Return Value:** A `ValueTask<double>` yielding the score when awaited.  
- **Exceptions:** Throws `ArgumentNullException` if the analysis result is null; may throw `InvalidOperationException` if the result lacks required metrics.

### DetermineComplexityAsync(...)
Classifies the syntactic and semantic complexity of a query.  
- **Purpose:** Returns a `QueryComplexity` enum value (e.g., Simple, Moderate, Complex) to aid in UI gating or resource planning.  
- **Parameters:** Accepts the `QueryAnalysisResult` from an analysis operation.  
- **Return Value:** A `ValueTask<QueryComplexity>` that completes with the complexity level.  
- **Exceptions:** Throws `ArgumentNullException` for a null analysis result.

## Usage

```csharp
using System.Threading.Tasks;
using SqlQueryAnalyzer;

// Assume analyzer is obtained via DI or direct instantiation
var analyzer = new QueryAnalyzerService();

string sql = @"SELECT o.OrderID, c.CustomerName
               FROM Orders o
               JOIN Customers c ON o.CustomerID = c.CustomerID
               WHERE o.OrderDate >= '2023-01-01'";

QueryAnalysisResult result = await analyzer.AnalyzeQueryAsync(sql);
double score = await analyzer.CalculatePerformanceScoreAsync(result);
QueryComplexity complexity = await analyzer.DetermineComplexityAsync(result);

Console.WriteLine($"Score: {score:F2}, Complexity: {complexity}");
```

```csharp
using System.Threading.Tasks;
using SqlQueryAnalyzer;

// When a query model is already available (e.g., from a parser)
var analyzer = new QueryAnalyzerService();
ParsedQuery model = Parser.Parse(sqlString); // hypothetical parser

QueryAnalysisResult result = await analyzer.AnalyzeQueryAsync(model);
if (await analyzer.DetermineComplexityAsync(result) == QueryComplexity.Complex)
{
    // Trigger advisory or logging
    Logger.Warn("Complex query detected");
}
```

## Notes

- The service does not maintain mutable state after construction; instances are safe to invoke concurrently from multiple threads.  
- All analysis methods are asynchronous and should be awaited to avoid blocking threads.  
- Passing null to any method that expects a query or analysis result will result in an `ArgumentNullException`.  
- Extremely large queries may cause the analyzer to consume significant memory or time; callers should consider applying size limits or timeouts where appropriate.  
- The numeric performance score is relative and not tied to any specific execution environment; it is intended for comparative purposes only.  
- Implementations may throw additional domain‑specific exceptions (e.g., for unsupported SQL dialects); callers should catch broadly if they need to handle all failure modes gracefully.
