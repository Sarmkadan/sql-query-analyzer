# ErrorHandlingMiddlewareExtensions

Provides a set of static utility methods for structured error handling, retry logic with exponential backoff, cache fallback strategies, and standardized error reporting within the SQL query analysis pipeline. These extensions centralize exception management patterns to ensure consistent logging, user-facing error formatting, and resilient execution flows across middleware components.

## API

### ExecuteWithErrorHandlingAsync

```csharp
public static async Task<bool> ExecuteWithErrorHandlingAsync(
    Func<Task> action,
    string operationContext,
    ILogger logger,
    Action<ErrorReport>? onError = null)
```

Executes an asynchronous action within a try-catch boundary, logs structured error details, and optionally invokes a callback with a full `ErrorReport`. Returns `true` if the action completes without throwing; returns `false` if any exception is caught and handled.

**Parameters:**
- `action` — The asynchronous operation to guard.
- `operationContext` — A human-readable label identifying the operation (e.g., `"QueryParsing"`) for log correlation.
- `logger` — The `ILogger` instance used to write error entries.
- `onError` — Optional callback that receives the generated `ErrorReport` when an exception occurs.

**Returns:** `true` on success, `false` when an exception is intercepted.

**Throws:** Never throws; all exceptions are caught and suppressed.

---

### CreateErrorReport

```csharp
public static ErrorReport CreateErrorReport(
    Exception exception,
    string operationContext,
    DateTime timestamp,
    string? correlationId = null)
```

Constructs a normalized `ErrorReport` object from an exception, attaching contextual metadata suitable for logging, diagnostics, and downstream error display.

**Parameters:**
- `exception` — The captured exception.
- `operationContext` — The operation name or phase where the error originated.
- `timestamp` — The UTC or local time at which the error was recorded.
- `correlationId` — Optional identifier linking the error to a specific request or session.

**Returns:** A populated `ErrorReport` instance.

**Throws:** `ArgumentNullException` if `exception` or `operationContext` is `null`.

---

### ExecuteWithRetryAsync\<T\>

```csharp
public static async Task<T> ExecuteWithRetryAsync<T>(
    Func<Task<T>> action,
    int maxRetries = 3,
    TimeSpan? baseDelay = null,
    ILogger? logger = null)
```

Attempts to execute a function returning `Task<T>`, retrying on failure up to `maxRetries` times with exponential backoff. Each retry delay is calculated as `baseDelay * 2^attempt` (default base delay: 1 second). If all attempts are exhausted, the last captured exception is rethrown.

**Parameters:**
- `action` — The asynchronous function to execute.
- `maxRetries` — Maximum number of retry attempts (default 3).
- `baseDelay` — Initial delay between retries; scales exponentially (default 1 second).
- `logger` — Optional logger for recording retry attempts and failures.

**Returns:** The result of type `T` from the first successful execution.

**Throws:** The last exception encountered after all retries are exhausted. `ArgumentOutOfRangeException` if `maxRetries` is less than 0.

---

### FormatErrorMessage

```csharp
public static string FormatErrorMessage(
    ErrorReport report,
    bool includeStackTrace = false,
    bool includeTimestamp = true)
```

Produces a human-readable, optionally detailed error string from an `ErrorReport`. Designed for user-facing messages or diagnostic output where control over verbosity is required.

**Parameters:**
- `report` — The error report to format.
- `includeStackTrace` — When `true`, appends the full stack trace to the output.
- `includeTimestamp` — When `true`, prefixes the message with the timestamp from the report.

**Returns:** A formatted error string.

**Throws:** `ArgumentNullException` if `report` is `null`.

---

### ExecuteWithCacheFallbackAsync\<T\>

```csharp
public static async Task<T> ExecuteWithCacheFallbackAsync<T>(
    Func<Task<T>> primaryAction,
    Func<Task<T>> fallbackAction,
    string cacheKey,
    ILogger? logger = null)
```

Executes a primary asynchronous operation. If it fails, logs the failure and attempts a fallback operation (typically retrieving a cached or stale result). If both fail, the exception from the primary action is rethrown.

**Parameters:**
- `primaryAction` — The preferred asynchronous operation.
- `fallbackAction` — The fallback operation invoked when the primary action throws.
- `cacheKey` — A key identifying the cached resource, used for logging correlation.
- `logger` — Optional logger for recording the fallback event.

**Returns:** The result from `primaryAction` if successful; otherwise the result from `fallbackAction`.

**Throws:** The exception from `primaryAction` if `fallbackAction` also throws. `ArgumentNullException` if `primaryAction`, `fallbackAction`, or `cacheKey` is `null`.

## Usage

### Example 1: Parsing with Full Error Handling Pipeline

```csharp
ILogger logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger("QueryParser");

ErrorReport? lastError = null;

bool success = await ErrorHandlingMiddlewareExtensions.ExecuteWithErrorHandlingAsync(
    async () =>
    {
        await ParseSqlQueryAsync(userInput);
    },
    operationContext: "SqlParsing",
    logger: logger,
    onError: report =>
    {
        lastError = report;
    });

if (!success && lastError != null)
{
    string userMessage = ErrorHandlingMiddlewareExtensions.FormatErrorMessage(
        lastError,
        includeStackTrace: false,
        includeTimestamp: true);
    
    Console.WriteLine($"Parsing failed: {userMessage}");
}
```

### Example 2: Resilient Query Execution with Cache Fallback

```csharp
ILogger logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger("QueryExecutor");

AnalysisResult result = await ErrorHandlingMiddlewareExtensions.ExecuteWithCacheFallbackAsync(
    primaryAction: async () => await ExecuteLiveQueryAsync(query),
    fallbackAction: async () => await GetCachedResultAsync(query.Id),
    cacheKey: $"query_result_{query.Id}",
    logger: logger);

// Optionally wrap the entire flow with retry logic for transient failures
AnalysisResult robustResult = await ErrorHandlingMiddlewareExtensions.ExecuteWithRetryAsync(
    async () => await ExecuteLiveQueryAsync(query),
    maxRetries: 5,
    baseDelay: TimeSpan.FromMilliseconds(500),
    logger: logger);
```

## Notes

- **Thread Safety:** All methods are static and operate on their parameters without shared mutable state. They are safe to call concurrently from multiple threads, provided the supplied `action` delegates and `ILogger` implementations are themselves thread-safe.
- **Exception Propagation:** `ExecuteWithErrorHandlingAsync` suppresses all exceptions and returns a boolean status, making it suitable for fire-and-forget or optional operations where failure should not abort the entire pipeline. In contrast, `ExecuteWithRetryAsync` and `ExecuteWithCacheFallbackAsync` ultimately rethrow if all recovery paths fail, preserving the original exception for upstream handlers.
- **Retry Backoff:** The exponential backoff in `ExecuteWithRetryAsync` uses the formula `baseDelay * 2^attempt`. With the default base delay of 1 second, delays are 1s, 2s, 4s, etc. Setting `maxRetries` to 0 executes the action exactly once with no retry. Negative `maxRetries` values throw `ArgumentOutOfRangeException`.
- **Cache Fallback Semantics:** `ExecuteWithCacheFallbackAsync` does not itself populate the cache; it assumes the fallback delegate retrieves a previously stored value. If the fallback delegate returns a default or null value for reference types, that value is returned to the caller without additional validation.
- **ErrorReport Creation:** `CreateErrorReport` captures the exception type, message, stack trace, and any inner exceptions at the moment of creation. Callers should pass a meaningful `correlationId` to enable tracing across distributed operations.
- **FormatErrorMessage Output:** When `includeTimestamp` is `true`, the timestamp is formatted using the invariant culture. Stack traces, when included, preserve their original formatting. The method does not truncate long messages or traces; callers must handle length constraints if displaying in constrained UI surfaces.
