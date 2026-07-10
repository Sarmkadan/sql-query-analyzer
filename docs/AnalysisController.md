# AnalysisController

The `AnalysisController` serves as the primary interface for performing SQL query analysis within the `sql-query-analyzer` framework, offering methods for both single-query evaluation and parallelized batch analysis. It utilizes asynchronous operations to ensure non-blocking execution, providing diagnostic results wrapped in structured `ApiResponse` objects to facilitate robust error handling and status reporting.

## API

### AnalysisController

| Member | Description |
| :--- | :--- |
| `public AnalysisController()` | Initializes a new instance of the `AnalysisController` class. |
| `public override string ToString()` | Returns a string representation of the controller instance. |

### Methods

| Method | Parameters | Return Type | Description |
| :--- | :--- | :--- | :--- |
| `AnalyzeAsync` | `string Query`, `Dictionary<string, string>? Options` | `Task<ApiResponse<QueryAnalysisResult>>` | Performs analysis on a single SQL query with optional configuration parameters. |
| `AnalyzeBatchAsync` | `string[] Queries`, `int? MaxDegreeOfParallelism`, `Dictionary<string, string>? Options` | `Task<ApiResponse<List<QueryAnalysisResult>>>` | Performs analysis on multiple SQL queries in parallel, respecting the specified degree of parallelism and options. |
| `GetHealthAsync` | None | `Task<ApiResponse<HealthStatus>>` | Retrieves the current health and status information of the analyzer service. |

### Supporting Structures

#### ApiResponse&lt;T&gt;
Generic response wrapper for all controller operations.

*   `bool Success`: Indicates whether the operation was successful.
*   `T? Data`: The result data of the operation, if successful.
*   `string Message`: A descriptive message regarding the outcome.
*   `int StatusCode`: The HTTP-equivalent status code of the operation.
*   `List<string> Errors`: A list of error messages if the operation failed.
*   `DateTime Timestamp`: The time at which the response was generated.

#### HealthStatus
Diagnostic information provided by `GetHealthAsync`.

*   `bool IsHealthy`: Indicates if the service is currently operating normally.
*   `string Message`: Status summary message.
*   `string Version`: The current version of the service.
*   `DateTime Timestamp`: The time at which the health status was checked.
*   `Dictionary<string, object>? Details`: Additional diagnostic details.

## Usage

### Single Query Analysis
```csharp
var controller = new AnalysisController();
var options = new Dictionary<string, string> { { "Timeout", "30" } };

var response = await controller.AnalyzeAsync("SELECT * FROM Users", options);

if (response.Success)
{
    var result = response.Data;
    // Process analysis result
}
else
{
    foreach (var error in response.Errors)
    {
        Console.WriteLine($"Error: {error}");
    }
}
```

### Batch Query Analysis
```csharp
var controller = new AnalysisController();
string[] queries = { "SELECT * FROM Users", "SELECT * FROM Orders" };

// Analyze queries in parallel with a maximum degree of parallelism set to 2
var response = await controller.AnalyzeBatchAsync(queries, 2, null);

if (response.Success)
{
    foreach (var result in response.Data)
    {
        // Process individual batch results
    }
}
```

## Notes

*   **Thread Safety:** The `AnalysisController` is designed to be thread-safe, allowing multiple calls to `AnalyzeAsync` or `AnalyzeBatchAsync` concurrently.
*   **Exceptions:** Methods are designed to return an `ApiResponse` object containing error information rather than throwing exceptions directly for expected validation or analysis failures. Exceptional service failures (e.g., infrastructure issues) may still throw underlying exceptions.
*   **Parallelism:** When using `AnalyzeBatchAsync`, if `MaxDegreeOfParallelism` is not specified (`null`), the controller will utilize a default internal parallelization strategy.
*   **Validation:** Input queries must be well-formed SQL. Invalid queries or invalid configuration options passed via the `Dictionary` may result in a `Success` value of `false` and corresponding entries in the `Errors` list.
