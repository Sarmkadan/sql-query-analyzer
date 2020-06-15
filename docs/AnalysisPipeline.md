# AnalysisPipeline

The `AnalysisPipeline` class orchestrates a sequence of middleware components that process a SQL query string. It allows registering, clearing, and executing middleware in a defined order, providing extensibility points for logging, validation, normalization, analysis, and optimization steps.

## API

### AnalysisPipeline()
Creates a new, empty pipeline instance. No parameters are required. The pipeline is ready to accept middleware registrations via `RegisterMiddleware`. Throws no exceptions under normal operation.

### void RegisterMiddleware(Middleware middleware)
Registers a middleware component to be executed when the pipeline runs.

- **Parameters**  
  - `middleware`: The middleware instance to add to the pipeline. Must not be `null`.

- **Return value**  
  - `None`.

- **Exceptions**  
  - `ArgumentNullException` if `middleware` is `null`.  
  - `InvalidOperationException` if the pipeline is currently executing a query (i.e., an `ExecuteAsync` call is in progress).

### void Clear()
Removes all previously registered middleware, returning the pipeline to its initial empty state.

- **Parameters**  
  - `None`.

- **Return value**  
  - `None`.

- **Exceptions**  
  - `InvalidOperationException` if the pipeline is currently executing a query.

### LoggingMiddleware LoggingMiddleware { get; }
Provides access to the built‑in logging middleware that can be inserted into the pipeline.

- **Parameters**  
  - `None`.

- **Return value**  
  - An instance of `LoggingMiddleware` ready for registration.

- **Exceptions**  
  - None.

### ValidationMiddleware ValidationMiddleware { get; }
Provides access to the built‑in validation middleware.

- **Parameters**  
  - `None`.

- **Return value**  
  - An instance of `ValidationMiddleware`.

- **Exceptions**  
  - None.

### QueryNormalizationMiddleware QueryNormalizationMiddleware { get; }
Provides access to the built‑in query normalization middleware.

- **Parameters**  
  - `None`.

- **Return value**  
  - An instance of `QueryNormalizationMiddleware`.

- **Exceptions**  
  - None.

### AnalysisMiddleware AnalysisMiddleware { get; }
Provides access to the built‑in analysis middleware.

- **Parameters**  
  - `None`.

- **Return value**  
  - An instance of `AnalysisMiddleware`.

- **Exceptions**  
  - None.

### OptimizationMiddleware OptimizationMiddleware { get; }
Provides access to the built‑in optimization middleware.

- **Parameters**  
  - `None`.

- **Return value**  
  - An instance of `OptimizationMiddleware`.

- **Exceptions**  
  - None.

### Task ExecuteAsync(string query)
Executes the registered middleware pipeline against the supplied SQL query asynchronously.

- **Parameters**  
  - `query`: The SQL query string to process. Must not be `null` or whitespace.

- **Return value**  
  - A `Task` that completes when all middleware have finished processing the query.

- **Exceptions**  
  - `ArgumentException` if `query` is `null` or consists only of whitespace.  
  - `InvalidOperationException` if no middleware has been registered.

### Task ExecuteAsync(string query, CancellationToken cancellationToken)
Executes the pipeline asynchronously with support for cancellation.

- **Parameters**  
  - `query`: The SQL query string to process. Must not be `null` or whitespace.  
  - `cancellationToken`: A token to observe for cancellation requests.

- **Return value**  
  - A `Task` that completes when processing finishes or is canceled.

- **Exceptions**  
  - `ArgumentException` if `query` is `null` or whitespace.  
  - `OperationCanceledException` if the token is triggered before completion.  
  - `InvalidOperationException` if no middleware is registered.

### async Task ExecuteAsync(string query)
An alternative asynchronous entry point that returns a `Task` awaiting internal asynchronous operations.

- **Parameters**  
  - `query`: The SQL query string to process. Must not be `null` or whitespace.

- **Return value**  
  - A `Task` representing the asynchronous operation.

- **Exceptions**  
  - Same as the synchronous `ExecuteAsync(string query)` overload.

### async Task ExecuteAsync(string query, CancellationToken cancellationToken)
Asynchronous execution with cancellation support, returning a `Task` that awaits internal async work.

- **Parameters**  
  - `query`: The SQL query string to process. Must not be `null` or whitespace.  
  - `cancellationToken`: A token to observe for cancellation requests.

- **Return value**  
  - A `Task` representing the asynchronous operation.

- **Exceptions**  
  - Same as the `ExecuteAsync(string query, CancellationToken)` overload.

## Usage

### Basic pipeline construction and execution
```csharp
using SqlQueryAnalyzer.Middleware;

// Create the pipeline
var pipeline = new AnalysisPipeline();

// Register built‑in middleware in the desired order
pipeline.RegisterMiddleware(pipeline.LoggingMiddleware);
pipeline.RegisterMiddleware(pipeline.ValidationMiddleware);
pipeline.RegisterMiddleware(pipeline.QueryNormalizationMiddleware);
pipeline.RegisterMiddleware(pipeline.AnalysisMiddleware);
pipeline.RegisterMiddleware(pipeline.OptimizationMiddleware);

// Execute a query
string sql = "SELECT * FROM Orders WHERE CustomerId = 42;";
await pipeline.ExecuteAsync(sql);
```

### Executing with a cancellation token
```csharp
using System.Threading;
using System.Threading.Tasks;
using SqlQueryAnalyzer.Middleware;

var pipeline = new AnalysisPipeline();
pipeline.RegisterMiddleware(pipeline.LoggingMiddleware);
pipeline.RegisterMiddleware(pipeline.ValidationMiddleware);
// ... other registrations ...

var cts = new CancellationTokenSource();
try
{
    await pipeline.ExecuteAsync("UPDATE Accounts SET Balance = Balance + 100;", cts.Token);
}
catch (OperationCanceledException)
{
    // Handle cancellation
}
finally
{
    cts.Dispose();
}
```

## Notes
- The pipeline is **not thread‑safe** for concurrent modifications. Calling `RegisterMiddleware` or `Clear` while an `ExecuteAsync` operation is in progress will result in an `InvalidOperationException`.  
- Multiple concurrent calls to `ExecuteAsync` are permitted **only after** all middleware registration has completed and no further modifications are made to the pipeline.  
- If no middleware is registered, any call to `ExecuteAsync` will throw an `InvalidOperationException`.  
- The built‑in middleware properties return ready‑to‑use instances; registering the same instance multiple times will cause it to be invoked multiple times in the order of registration.  
- The `ExecuteAsync` overloads that return a non‑`async` `Task` are provided for compatibility with synchronous‑looking APIs; they internally perform asynchronous work and should be awaited.  
- Cancellation is cooperative; middleware must observe the supplied `CancellationToken` to honor cancellation requests promptly.  
- After calling `Clear`, the pipeline can be reused by registering new middleware instances.
