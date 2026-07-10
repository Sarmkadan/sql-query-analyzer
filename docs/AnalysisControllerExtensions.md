# AnalysisControllerExtensions

The `AnalysisControllerExtensions` class provides a set of static extension methods designed to streamline the creation of analysis requests and the safe execution of query analysis operations within the `sql-query-analyzer` project. By encapsulating request instantiation and error-handling logic, these utilities allow controllers to delegate SQL analysis tasks efficiently while ensuring that unexpected exceptions during asynchronous processing do not disrupt the application flow, returning null results instead of propagating failures.

## API

### `CreateAnalysisRequest`
Constructs a new instance of an `AnalysisRequest` object based on the provided input parameters. This method serves as a factory for single-query analysis operations, ensuring that the request object is properly initialized before being passed to the analysis engine.
*   **Parameters**: Accepts the necessary arguments to define the query string and associated analysis context (specific signature details depend on the `AnalysisRequest` constructor requirements).
*   **Return Value**: Returns a populated `AnalysisRequest` instance ready for processing.
*   **Throws**: May throw `ArgumentNullException` if required input parameters are null; otherwise, it performs no I/O and does not throw under normal validation conditions.

### `CreateBatchRequest`
Initializes a `BatchAnalysisRequest` object intended for processing multiple SQL queries in a single operation. This method aggregates individual query inputs into a structured batch container, optimizing throughput for scenarios requiring bulk analysis.
*   **Parameters**: Accepts a collection of queries or batch-specific configuration options.
*   **Return Value**: Returns a `BatchAnalysisRequest` instance containing the aggregated data.
*   **Throws**: Throws `ArgumentException` if the provided collection is empty or invalid; throws `ArgumentNullException` if the input collection itself is null.

### `AnalyzeQuerySafelyAsync`
Executes an asynchronous analysis of a given query with built-in exception suppression. This method wraps the core analysis logic to catch unforeseen errors (such as timeout exceptions, parser failures, or infrastructure issues) and returns a null result instead of crashing the calling thread, facilitating graceful degradation.
*   **Parameters**: Requires an `AnalysisRequest` object and optionally a `CancellationToken`.
*   **Return Value**: Returns a `Task<QueryAnalysisResult?>`. The task completes with a valid `QueryAnalysisResult` upon success, or `null` if an exception occurred during processing.
*   **Throws**: This method is designed not to throw exceptions to the caller; all internal exceptions are caught and translated into a `null` return value.

### `AnalyzeBatchSafelyAsync` (Inferred from signature fragment)
Performs asynchronous analysis on a batch of queries with the same safety guarantees as the single-query variant. It processes the `BatchAnalysisRequest` and handles internal errors gracefully.
*   **Parameters**: Requires a `BatchAnalysisRequest` object and optionally a `CancellationToken`.
*   **Return Value**: Returns a `Task` containing the batch analysis results (or null if the entire batch operation fails critically).
*   **Throws**: Like its single-query counterpart, this method suppresses internal exceptions and returns null or a partial result set rather than propagating errors.

## Usage

### Single Query Analysis with Safety
The following example demonstrates how to create a single analysis request and execute it safely. If the analysis engine encounters a syntax error or a transient database connection issue, the method returns `null` instead of throwing, allowing the controller to handle the absence of data gracefully.

```csharp
using SqlQueryAnalyzer.Extensions;

public async Task<IActionResult> AnalyzeSingleQuery(string sqlQuery)
{
    // Create the request object using the extension helper
    var request = AnalysisControllerExtensions.CreateAnalysisRequest(sqlQuery);

    // Execute safely; result will be null if an internal exception occurs
    var result = await AnalysisControllerExtensions.AnalyzeQuerySafelyAsync(request);

    if (result == null)
    {
        // Handle failure case (log error internally via structured logging)
        return StatusCode(500, "Analysis failed due to an internal error.");
    }

    return Ok(result);
}
```

### Batch Processing
This example illustrates the creation and execution of a batch request. This pattern is suitable for endpoints accepting multiple SQL statements, leveraging the batch-specific extension method to manage the lifecycle of the operation.

```csharp
using SqlQueryAnalyzer.Extensions;

public async Task<IActionResult> AnalyzeQueryBatch(List<string> queries)
{
    if (queries == null || !queries.Any())
    {
        return BadRequest("No queries provided.");
    }

    // Initialize the batch request
    var batchRequest = AnalysisControllerExtensions.CreateBatchRequest(queries);

    // Execute the batch analysis safely
    // Note: Assuming the fourth member follows the pattern of returning Task<BatchResult?>
    var results = await AnalysisControllerExtensions.AnalyzeBatchSafelyAsync(batchRequest);

    if (results == null)
    {
        return StatusCode(500, "Batch analysis could not be completed.");
    }

    return Ok(results);
}
```

## Notes

*   **Exception Suppression**: The `Analyze...SafelyAsync` methods explicitly swallow exceptions. While this prevents application crashes, it also obscures the specific root cause of failures from the immediate caller. Consumers should rely on the project's structured logging system (recently enhanced in the codebase) to diagnose why a `null` result was returned.
*   **Thread Safety**: As the class consists entirely of static methods that do not maintain internal mutable state, `AnalysisControllerExtensions` is thread-safe. Multiple concurrent requests can invoke these methods without risk of race conditions, provided the underlying `AnalysisRequest` and `BatchAnalysisRequest` objects are not shared across threads during mutation.
*   **Null Handling**: Callers must strictly check for `null` return values from the `Analyze...SafelyAsync` methods before accessing properties on the result object. Failure to do so will result in a `NullReferenceException` at the call site, defeating the purpose of the safe wrapper.
*   **Cancellation**: While the specific signature fragment for the batch method was truncated, the single-query method supports standard `CancellationToken` propagation. Long-running batch operations should ensure tokens are passed through to prevent resource leaks during application shutdowns.
