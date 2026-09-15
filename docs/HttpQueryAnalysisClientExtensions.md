# HttpQueryAnalysisClientExtensions

## Purpose
Provides extension methods for the `HttpQueryAnalysisClient` class that add convenient functionality for batch query analysis, retry logic, timeout handling, performance metrics, and complexity-based filtering.

## Members

### AnalyzeQueriesAsync(client, queries, maxDegreeOfParallelism)
Analyzes multiple queries with a specified degree of parallelism.

- **Parameters**:
  - `client`: The `HttpQueryAnalysisClient` instance.
  - `queries`: The queries to analyze.
  - `maxDegreeOfParallelism`: Maximum degree of parallelism for analysis.
- **Returns**: Read-only list of analysis results in the same order as input queries.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `client` or `queries` is null.
  - `ArgumentException`: Thrown when `queries` is empty.
  - `InvalidOperationException`: Thrown when batch analysis returns unexpected result count.

### AnalyzeQueryAsync(client, query, options)
Analyzes a single query with optional analysis options.

- **Parameters**:
  - `client`: The `HttpQueryAnalysisClient` instance.
  - `query`: The SQL query to analyze.
  - `options`: Optional analysis options (e.g., timeout, rules to apply).
- **Returns**: The analysis result.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `client` is null.
  - `ArgumentException`: Thrown when `query` is empty or whitespace.

### IsHealthyWithRetryAsync(client, maxRetries, delayMs)
Checks if the remote analyzer service is healthy with retry logic.

- **Parameters**:
  - `client`: The `HttpQueryAnalysisClient` instance.
  - `maxRetries`: Maximum number of retry attempts.
  - `delayMs`: Initial delay in milliseconds between retries.
- **Returns**: True if service is healthy; otherwise false.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `client` is null.

### GetVersionAsync(client, fallbackVersion)
Gets the version information from the remote analyzer with fallback.

- **Parameters**:
  - `client`: The `HttpQueryAnalysisClient` instance.
  - `fallbackVersion`: Version string to return if remote call fails.
- **Returns**: The version string or the fallback version.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `client` is null.

### AnalyzeWithTimeoutAsync(client, queries, timeout, maxDegreeOfParallelism)
Analyzes queries with timeout and returns results or throws if timeout is exceeded.

- **Parameters**:
  - `client`: The `HttpQueryAnalysisClient` instance.
  - `queries`: The queries to analyze.
  - `timeout`: Timeout for the analysis operation.
  - `maxDegreeOfParallelism`: Maximum degree of parallelism.
- **Returns**: Read-only list of analysis results.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `client` or `queries` is null.
  - `ArgumentException`: Thrown when `queries` is empty.
  - `TimeoutException`: Thrown when analysis exceeds the specified timeout.

### AnalyzeWithTimeoutAsync(client, queries, timeoutMs, maxDegreeOfParallelism)
Analyzes queries with timeout specified as milliseconds.

- **Parameters**:
  - `client`: The `HttpQueryAnalysisClient` instance.
  - `queries`: The queries to analyze.
  - `timeoutMs`: Timeout in milliseconds for the analysis operation.
  - `maxDegreeOfParallelism`: Maximum degree of parallelism.
- **Returns**: Read-only list of analysis results.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `client` or `queries` is null.
  - `ArgumentException`: Thrown when `queries` is empty.
  - `TimeoutException`: Thrown when analysis exceeds the specified timeout.

### GetPerformanceMetricsAsync(client, queries, iterations, maxDegreeOfParallelism)
Gets performance metrics for a collection of queries by analyzing them multiple times and calculating average performance scores.

- **Parameters**:
  - `client`: The `HttpQueryAnalysisClient` instance.
  - `queries`: The queries to analyze.
  - `iterations`: Number of analysis iterations to perform.
  - `maxDegreeOfParallelism`: Maximum degree of parallelism.
- **Returns**: Dictionary mapping queries to their average performance metrics.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `client` or `queries` is null.
  - `ArgumentException`: Thrown when `queries` is empty or `iterations` is less than 1.

### FilterQueriesByComplexityAsync(client, queries, minComplexity, maxComplexity, maxDegreeOfParallelism)
Filters queries by their complexity level after analysis.

- **Parameters**:
  - `client`: The `HttpQueryAnalysisClient` instance.
  - `queries`: The queries to analyze.
  - `minComplexity`: Minimum complexity level to include.
  - `maxComplexity`: Maximum complexity level to include.
  - `maxDegreeOfParallelism`: Maximum degree of parallelism.
- **Returns**: Read-only list of queries that match the complexity filter.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `client` or `queries` is null.
  - `ArgumentException`: Thrown when `queries` is empty.

## Usage Example
```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SqlQueryAnalyzer.Integration;
using SqlQueryAnalyzer.Constants;

public class QueryAnalyzer
{
    private readonly HttpQueryAnalysisClient _client;

    public QueryAnalyzer(HttpQueryAnalysisClient client)
    {
        _client = client;
    }

    public async Task AnalyzeAndReportAsync()
    {
        string[] queries = {
            "SELECT * FROM customers WHERE id = 1",
            "SELECT o.*, c.name FROM orders o JOIN customers c ON o.customer_id = c.id",
            "SELECT product_id, COUNT(*) FROM order_items GROUP BY product_id HAVING COUNT(*) > 5"
        };

        // Check service health with retry logic
        bool isHealthy = await _client.IsHealthyWithRetryAsync(maxRetries: 3, delayMs: 1000);
        Console.WriteLine($"Service healthy: {isHealthy}");

        if (isHealthy)
        {
            // Analyze queries with parallelism
            var results = await _client.AnalyzeQueriesAsync(queries, maxDegreeOfParallelism: 5);
            
            // Get performance metrics across multiple iterations
            var metrics = await _client.GetPerformanceMetricsAsync(queries, iterations: 3);
            
            // Filter only simple to medium complexity queries
            var simpleQueries = await _client.FilterQueriesByComplexityAsync(
                queries, 
                minComplexity: QueryComplexity.Simple, 
                maxComplexity: QueryComplexity.Medium);

            Console.WriteLine($"Analyzed {results.Count} queries");
            Console.WriteLine($"Found {simpleQueries.Count} simple/medium complexity queries");
            
            foreach (var query in simpleQueries)
            {
                if (metrics.TryGetValue(query, out double avgScore))
                {
                    Console.WriteLine($"Query: {query.Substring(0, Math.Min(50, query.Length))}... | Avg Score: {avgScore:F2}");
                }
            }
        }
    }

    public async Task<QueryAnalysisResult> AnalyzeSingleQueryWithTimeoutAsync(string query)
    {
        // Analyze with 30-second timeout
        var results = await _client.AnalyzeWithTimeoutAsync(
            new[] { query }, 
            timeoutMs: 30000);
        
        return results[0];
    }

    public async Task<string> GetServiceVersionAsync()
    {
        // Get version with fallback
        return await _client.GetVersionAsync(fallbackVersion: "unknown");
    }
}
```