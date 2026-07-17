# AnalysisPipelineExtensions

The `AnalysisPipelineExtensions` class provides a set of extension methods for the `AnalysisPipeline` type, enabling the fluent construction and execution of a query analysis pipeline. These methods allow you to register middleware components (logging, validation, normalization, analysis, optimization), execute analysis asynchronously on one or more SQL queries, inspect the pipeline state, and perform a success-checked execution. The class is designed to simplify common pipeline configurations and to integrate with the `sql-query-analyzer` framework.

## API

### `UseLogging`
```csharp
public static AnalysisPipeline UseLogging(this AnalysisPipeline pipeline)
```
Adds a logging middleware component to the pipeline.  
**Parameters:**  
- `pipeline` – The current pipeline instance.  

**Returns:** The same `AnalysisPipeline` instance with logging middleware appended.  

**Throws:** `ArgumentNullException` if `pipeline` is `null`.

### `UseValidation`
```csharp
public static AnalysisPipeline UseValidation(this AnalysisPipeline pipeline)
```
Adds a validation middleware component to the pipeline.  
**Parameters:**  
- `pipeline` – The current pipeline instance.  

**Returns:** The same `AnalysisPipeline` instance with validation middleware appended.  

**Throws:** `ArgumentNullException` if `pipeline` is `null`.

### `UseNormalization`
```csharp
public static AnalysisPipeline UseNormalization(this AnalysisPipeline pipeline)
```
Adds a normalization middleware component to the pipeline.  
**Parameters:**  
- `pipeline` – The current pipeline instance.  

**Returns:** The same `AnalysisPipeline` instance with normalization middleware appended.  

**Throws:** `ArgumentNullException` if `pipeline` is `null`.

### `UseAnalysis`
```csharp
public static AnalysisPipeline UseAnalysis(this AnalysisPipeline pipeline)
```
Adds an analysis middleware component to the pipeline.  
**Parameters:**  
- `pipeline` – The current pipeline instance.  

**Returns:** The same `AnalysisPipeline` instance with analysis middleware appended.  

**Throws:** `ArgumentNullException` if `pipeline` is `null`.

### `UseOptimization`
```csharp
public static AnalysisPipeline UseOptimization(this AnalysisPipeline pipeline)
```
Adds an optimization middleware component to the pipeline.  
**Parameters:**  
- `pipeline` – The current pipeline instance.  

**Returns:** The same `AnalysisPipeline` instance with optimization middleware appended.  

**Throws:** `ArgumentNullException` if `pipeline` is `null`.

### `AnalyzeQueryAsync`
```csharp
public static async Task<QueryAnalysisResult> AnalyzeQueryAsync(this AnalysisPipeline pipeline, string query)
```
Executes the pipeline asynchronously on a single SQL query and returns the analysis result.  
**Parameters:**  
- `pipeline` – The pipeline instance to execute.  
- `query` – The SQL query string to analyze.  

**Returns:** A `Task<QueryAnalysisResult>` representing the asynchronous operation, with a result containing the analysis output.  

**Throws:**  
- `ArgumentNullException` if `pipeline` or `query` is `null`.  
- `InvalidOperationException` if the pipeline has no middleware registered.

### `AnalyzeQueriesAsync`
```csharp
public static async Task<IReadOnlyList<QueryAnalysisResult>> AnalyzeQueriesAsync(this AnalysisPipeline pipeline, IEnumerable<string> queries)
```
Executes the pipeline asynchronously on a collection of SQL queries and returns a list of analysis results, one per query.  
**Parameters:**  
- `pipeline` – The pipeline instance to execute.  
- `queries` – A collection of SQL query strings to analyze.  

**Returns:** A `Task<IReadOnlyList<QueryAnalysisResult>>` representing the asynchronous operation, with a result containing a read-only list of analysis results in the same order as the input queries.  

**Throws:**  
- `ArgumentNullException` if `pipeline` or `queries` is `null`.  
- `InvalidOperationException` if the pipeline has no middleware registered.

### `ClearMiddleware`
```csharp
public static AnalysisPipeline ClearMiddleware(this AnalysisPipeline pipeline)
```
Removes all middleware components from the pipeline.  
**Parameters:**  
- `pipeline` – The current pipeline instance.  

**Returns:** The same `AnalysisPipeline` instance with an empty middleware chain.  

**Throws:** `ArgumentNullException` if `pipeline` is `null`.

### `UseAllStandardMiddleware`
```csharp
public static AnalysisPipeline UseAllStandardMiddleware(this AnalysisPipeline pipeline)
```
Adds all standard middleware components (logging, validation, normalization, analysis, optimization) to the pipeline in a predefined order.  
**Parameters:**  
- `pipeline` – The current pipeline instance.  

**Returns:** The same `AnalysisPipeline` instance with the full set of standard middleware appended.  

**Throws:** `ArgumentNullException` if `pipeline` is `null`.

### `GetMiddlewareCount`
```csharp
public static int GetMiddlewareCount(this AnalysisPipeline pipeline)
```
Returns the number of middleware components currently registered in the pipeline.  
**Parameters:**  
- `pipeline` – The pipeline instance to inspect.  

**Returns:** An `int` representing the count of middleware components.  

**Throws:** `ArgumentNullException` if `pipeline` is `null`.

### `ExecuteWithSuccessCheckAsync`
```csharp
public static async Task<bool> ExecuteWithSuccessCheckAsync(this AnalysisPipeline pipeline, string query)
```
Executes the pipeline asynchronously on a single SQL query and returns `true` if the analysis indicates success (e.g., no errors or warnings), otherwise `false`.  
**Parameters:**  
- `pipeline` – The pipeline instance to execute.  
- `query` – The SQL query string to analyze.  

**Returns:** A `Task<bool>` representing the asynchronous operation, with a result of `true` if the analysis succeeded, `false` otherwise.  

**Throws:**  
- `ArgumentNullException` if `pipeline` or `query` is `null`.  
- `InvalidOperationException` if the pipeline has no middleware registered.

## Usage

### Example 1: Building a custom pipeline and analyzing a single query

```csharp
using SqlQueryAnalyzer;

var pipeline = new AnalysisPipeline()
    .UseLogging()
    .UseValidation()
    .UseNormalization()
    .UseAnalysis();

string query = "SELECT * FROM Orders WHERE OrderDate > '2023-01-01'";
QueryAnalysisResult result = await pipeline.AnalyzeQueryAsync(query);

Console.WriteLine($"Analysis completed. Issues found: {result.Issues.Count}");
```

### Example 2: Using all standard middleware and analyzing multiple queries with success check

```csharp
using SqlQueryAnalyzer;

var pipeline = new AnalysisPipeline()
    .UseAllStandardMiddleware();

var queries = new[]
{
    "SELECT Id, Name FROM Customers",
    "UPDATE Products SET Price = Price * 1.1 WHERE Category = 'Electronics'",
    "DELETE FROM Logs WHERE CreatedAt < '2020-01-01'"
};

IReadOnlyList<QueryAnalysisResult> results = await pipeline.AnalyzeQueriesAsync(queries);

for (int i = 0; i < queries.Length; i++)
{
    bool success = await pipeline.ExecuteWithSuccessCheckAsync(queries[i]);
    Console.WriteLine($"Query {i + 1}: {(success ? "Passed" : "Failed")}");
}
```

## Notes

- All extension methods throw `ArgumentNullException` if the `pipeline` argument is `null`. Methods that accept a query string or collection also throw `ArgumentNullException` if those arguments are `null`.
- The `AnalyzeQueryAsync`, `AnalyzeQueriesAsync`, and `ExecuteWithSuccessCheckAsync` methods throw `InvalidOperationException` if the pipeline has no middleware registered, because an empty pipeline cannot produce a meaningful result.
- The `AnalysisPipeline` instance is mutable; middleware is appended in the order the extension methods are called. The `ClearMiddleware` method resets the pipeline to an empty state.
- Thread safety is not guaranteed. Concurrent modifications to the same `AnalysisPipeline` instance (e.g., adding middleware from multiple threads) may result in inconsistent state. It is recommended to build the pipeline once and reuse it for multiple analysis calls, or to create separate instances per thread.
- The `UseAllStandardMiddleware` method adds middleware in a fixed order: logging, validation, normalization, analysis, optimization. This order is considered the standard sequence for typical query analysis workflows.
- The `ExecuteWithSuccessCheckAsync` method’s definition of “success” depends on the implementation of the middleware components. By default, it likely checks that no errors or critical warnings were produced during analysis.
