# SqlQueryAnalyzerExceptionExtensions

Provides a set of static extension methods for analyzing and formatting exceptions thrown by the SQL Query Analyzer component. These helpers enable callers to classify error types, extract error codes and details, and produce user‑friendly or diagnostic strings without needing to know the internal structure of the exception types.

## API

### ToErrorMessage
```csharp
public static string ToErrorMessage(this Exception ex)
```
**Purpose** – Returns a concise, machine‑readable error message that combines the exception’s `Message` with any available error code.  
**Parameters** – `ex`: The exception to analyze.  
**Return Value** – A string containing the error message; if no error code is present, only the exception message is returned.  
**Exceptions** – Throws `ArgumentNullException` if `ex` is `null`.

### IsQueryValidationError
```csharp
public static bool IsQueryValidationError(this Exception ex)
```
**Purpose** – Determines whether the exception represents a query validation problem (e.g., malformed SQL, unsupported syntax).  
**Parameters** – `ex`: The exception to test.  
**Return Value** – `true` if the exception is a query validation error; otherwise `false`.  
**Exceptions** – Throws `ArgumentNullException` if `ex` is `null`.

### IsDatabaseConnectionError
```csharp
public static bool IsDatabaseConnectionError(this Exception ex)
```
**Purpose** – Determines whether the exception stems from a database connectivity issue (e.g., network failure, login timeout).  
**Parameters** – `ex`: The exception to test.  
**Return Value** – `true` if the exception is a database connection error; otherwise `false`.  
**Exceptions** – Throws `ArgumentNullException` if `ex` is `null`.

### IsQueryPlanError
```csharp
public static bool IsQueryPlanError(this Exception ex)
```
**Purpose** – Determines whether the exception relates to a problem generating or executing a query plan (e.g., missing statistics, plan cache corruption).  
**Parameters** – `ex`: The exception to test.  
**Return Value** – `true` if the exception is a query plan error; otherwise `false`.  
**Exceptions** – Throws `ArgumentNullException` if `ex` is `null`.

### GetErrorCode
```csharp
public static string? GetErrorCode(this Exception ex)
```
**Purpose** – Extracts the vendor‑specific error code associated with the exception, if one is present.  
**Parameters** – `ex`: The exception to inspect.  
**Return Value** – The error code as a string, or `null` when the exception does not expose an error code.  
**Exceptions** – Throws `ArgumentNullException` if `ex` is `null`.

### GetErrorDetails
```csharp
public static string? GetErrorDetails(this Exception ex)
```
**Purpose** – Retrieves additional details about the error (e.g., the offending SQL fragment, parameter values) when available.  
**Parameters** – `ex`: The exception to inspect.  
**Return Value** – A string with supplemental details, or `null` if no details are exposed.  
**Exceptions** – Throws `ArgumentNullException` if `ex` is `null`.

### ToUserFriendlySummary
```csharp
public static string ToUserFriendlySummary(this Exception ex)
```
**Purpose** – Produces a short, non‑technical description suitable for display to end users.  
**Parameters** – `ex`: The exception to summarize.  
**Return Value** – A user‑friendly string that avoids stack traces and internal identifiers.  
**Exceptions** – Throws `ArgumentNullException` if `ex` is `null`.

### IsCriticalError
```csharp
public static bool IsCriticalError(this Exception ex)
```
**Purpose** – Indicates whether the exception represents a critical failure that may require application shutdown or admin intervention (e.g., unrecoverable storage corruption).  
**Parameters** – `ex`: The exception to evaluate.  
**Return Value** – `true` for critical errors; otherwise `false`.  
**Exceptions** – Throws `ArgumentNullException` if `ex` is `null`.

### GenerateExceptionReport
```csharp
public static string GenerateExceptionReport(this Exception ex)
```
**Purpose** – Builds a comprehensive diagnostic report that includes the exception type, message, error code, details, and a formatted stack trace.  
**Parameters** – `ex`: The exception to report on.  
**Return Value** – A multi‑line string suitable for logging or sending to a support ticket system.  
**Exceptions** – Throws `ArgumentNullException` if `ex` is `null`.

## Usage

### Example 1: Classifying and presenting an error
```csharp
try
{
    var result = analyzer.ExecuteQuery(sql);
}
catch (Exception ex) when (ex.IsQueryValidationError())
{
    // Inform the user about a problem with their SQL syntax.
    string userMsg = ex.ToUserFriendlySummary();
    logger.Warn($"Validation error: {userMsg}");
}
catch (Exception ex) when (ex.IsDatabaseConnectionError())
{
    // Attempt a retry or alert an operator.
    if (ex.IsCriticalError())
    {
        logger.Fatal(ex.GenerateExceptionReport());
        Environment.FailFast("Database unreachable");
    }
    else
    {
        logger.Error(ex.ToErrorMessage());
    }
}
```

### Example 2: Producing a diagnostic bundle for support
```csharp
try
{
    analyzer.PrepareStatement(sql);
}
catch (Exception ex)
{
    string report = ex.GenerateExceptionReport();
    // Attach the report to a support ticket or write to a diagnostics folder.
    File.WriteAllText(Path.Combine(logFolder, $"error-{DateTime.UtcNow:yyyyMMddHHmmss}.txt"), report);
}
```

## Notes

- All extension methods are **pure** with respect to the exception instance**; they do not modify the supplied `Exception` object.  
- Passing `null` for the `ex` parameter results in an `ArgumentNullException` from every member, as the methods rely on accessing members of the exception instance.  
- Because the methods contain no static state, they are **thread‑safe** and may be invoked concurrently from multiple threads without additional synchronization.  
- The boolean classifiers (`IsQueryValidationError`, `IsDatabaseConnectionError`, `IsQueryPlanError`, `IsCriticalError`) are based on the exception’s type and any embedded error code; future versions of the library may add new error categories without changing the method signatures.  
- `GetErrorCode` and `GetErrorDetails` may return `null` when the underlying exception does not expose the corresponding information; callers should handle the `null` case appropriately.  
- `ToUserFriendlySummary` deliberately omits stack traces and technical identifiers; for detailed diagnostics, use `GenerateExceptionReport` or combine `ToErrorMessage` with `GetErrorDetails`.  
- The methods do **not** perform any I/O, logging, or environment checks beyond inspecting the exception, making them suitable for use in performance‑sensitive paths such as middleware or asynchronous continuations.
