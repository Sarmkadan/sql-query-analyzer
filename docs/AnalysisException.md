# AnalysisException

**Namespace:** `SqlQueryAnalyzer.Exceptions`
**Assembly:** `SqlQueryAnalyzer`
**Source:** `Exceptions/AnalysisException.cs`
**Base type:** `SqlQueryAnalyzerException` (derives from `System.Exception`)

`AnalysisException` is the base exception for analysis-related failures in SQL Query Analyzer. It adds a machine-readable error code and an optional details string on top of the standard exception message. Specific failure kinds derive from it (see [Derived exceptions](#derived-exceptions)), so callers can catch `AnalysisException` to handle any analyzer error, or catch a derived type for a specific case.

## Declaration

```csharp
public class AnalysisException : SqlQueryAnalyzerException
```

## Properties

| Property | Type | Accessors | Description |
|---|---|---|---|
| `ErrorCode` | `string?` | get, set | Machine-readable code such as `INVALID_QUERY` or `DB_CONNECTION_ERROR`. `null` when the exception was created without a code. |
| `ErrorDetails` | `string?` | get, set | Free-form extra information about the failure, for example the pipeline stage or the inner error text. `null` when not supplied. |

Both properties are plain auto-properties. They can be assigned after construction, but the constructors are the normal way to set them.

The base class `SqlQueryAnalyzerException` contributes no extra public members beyond the standard `Exception` members. It overrides `ToString()` to list the `ErrorCode`, `ErrorDetails`, `Query`, `LineNumber`, `ColumnNumber`, and `ConnectionString` properties that exist on the runtime type.

## Constructors

| Signature | Sets `ErrorCode` | Sets `ErrorDetails` | Typical use |
|---|---|---|---|
| `AnalysisException(string message)` | No | No | Generic analysis failure with no code. |
| `AnalysisException(string message, Exception innerException)` | No | No | Wraps a lower-level exception. The inner exception is preserved in `InnerException`. |
| `AnalysisException(string message, string errorCode)` | Yes | No | Failure with a known code but no extra details. |
| `AnalysisException(string message, string errorCode, string errorDetails)` | Yes | Yes | Failure with a code and extra context. |

Every constructor passes `message` to the base `Exception`, so `Message` always returns the value you supply.

### Example

```csharp
throw new AnalysisException("Query analysis failed", "ANALYSIS_FAILED", "Stage: ExplainPlan");
```

## Error codes

The derived types set these codes. Code-based checks such as `ErrorCode == "INVALID_QUERY"` match the same failures as the matching type checks.

| Code | Type |
|---|---|
| `INVALID_QUERY` | `InvalidQueryException` |
| `DB_CONNECTION_ERROR` | `DatabaseConnectionException` |
| `PLAN_ERROR` | `QueryPlanException` |
| `INDEX_ANALYSIS_ERROR` | `IndexAnalysisException` |
| `CONFIGURATION_ERROR` | `ConfigurationException` |
| `REPOSITORY_ERROR` | `RepositoryException` |
| `VALIDATION_ERROR` | `ValidationException` |
| `INTEGRATION_ERROR` | `IntegrationException` |
| `QUERY_TOO_LARGE` | `QueryTooLargeException` |

## Derived exceptions

All of these derive from `AnalysisException` and are declared in the same file unless noted.

| Type | Thrown when |
|---|---|
| `InvalidQueryException` | The SQL text is empty or fails validation. |
| `DatabaseConnectionException` | A database connection cannot be opened or used. |
| `QueryPlanException` | An execution plan cannot be obtained or parsed. |
| `IndexAnalysisException` | Index analysis fails for a specific index or table. |
| `ConfigurationException` | A configuration value is missing or invalid. |
| `RepositoryException` | A repository operation fails. |
| `ValidationException` | An input object or result fails validation. |
| `IntegrationException` | An external service integration fails. |
| `QueryTooLargeException` | The query text exceeds the maximum allowed length (`Exceptions/QueryTooLargeException.cs`). |

## When it is thrown

`AnalysisException` is the type to catch when you want to handle any analyzer failure in one place. In the code base:

- **Directly:** production code rarely constructs the base type. Tests and the sample in `examples/` use it, and so do some callers that need a generic failure.
- **Through derived types:** most specific failures are thrown as one of the subtypes above, such as `InvalidQueryException` from `Utilities/QueryValidator.cs` or `ConfigurationException` for connection-string problems. Each subtype is still an `AnalysisException`, so a `catch (AnalysisException)` block handles them.
- **Argument errors:** the library's argument checks (for example `ArgumentNullException.ThrowIfNull`) throw standard .NET exceptions, not `AnalysisException`.

`ErrorHandlingMiddleware` (`Middleware/ErrorHandlingMiddleware.cs`) handles it in two ways:

- It retries the operation when the error looks transient. The check is a case-insensitive search of `Message` for `connection`, `timeout`, or `unavailable`. After the final attempt, the exception is rethrown.
- It logs and rethrows every other `AnalysisException` without retrying.

## Handling

```csharp
try
{
    var result = await analyzer.AnalyzeQueryAsync(query);
}
catch (InvalidQueryException ex)
{
    // Report the bad SQL to the user.
}
catch (AnalysisException ex) when (ex.ErrorCode == "PLAN_ERROR")
{
    // Plan-specific fallback.
}
catch (AnalysisException ex)
{
    logger.LogError("Analysis failed: {Code} {Details}", ex.ErrorCode, ex.ErrorDetails);
}
```

Helper extension methods in `Exceptions/SqlQueryAnalyzerExceptionExtensions.cs` read the same properties. `ToErrorMessage()` formats `ErrorCode` and `ErrorDetails` into a report, and `IsQueryValidationError()`, `IsDatabaseConnectionError()`, and `IsQueryPlanError()` check the code.

## Related

- `SqlQueryAnalyzerException` - root type for all library exceptions.
- `SqlQueryAnalyzerExceptionExtensions` - formatting and classification helpers.
- `ErrorHandlingMiddleware` - retry and logging behavior.
