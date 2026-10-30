# QueryTooLargeException

**Namespace:** `SqlQueryAnalyzer.Exceptions`
**Assembly:** `SqlQueryAnalyzer`
**Source:** `Exceptions/QueryTooLargeException.cs`
**Base type:** `AnalysisException` (derives from `SqlQueryAnalyzerException` and `System.Exception`)

`QueryTooLargeException` reports that a SQL query's text is longer than the analyzer accepts. It is a specific kind of [`AnalysisException`](AnalysisException.md), so a `catch (AnalysisException)` block also handles it. It always sets the error code `QUERY_TOO_LARGE`.

## Declaration

```csharp
public class QueryTooLargeException : AnalysisException
```

## Properties

| Property | Type | Accessors | Description |
|---|---|---|---|
| `Query` | `string` | get | The query text that exceeded the limit. Defaults to `string.Empty` when the exception is created without a query. |

`Query` is the only member this type declares. It is read-only after construction. Its value is set only by the two-argument constructor.

Inherited from `AnalysisException`:

| Property | Value for this type | Notes |
|---|---|---|
| `ErrorCode` | `"QUERY_TOO_LARGE"` | Set by the base constructor in both overloads. |
| `ErrorDetails` | `null` | Not set by this type. Can be assigned after construction. |

Because the base `SqlQueryAnalyzerException.ToString()` reflects over the `Query` property, the full query text appears in `ToString()` output (and so in logs that write the exception with `ToString()`). The query can be large, so be careful about logging it.

## Constructors

| Signature | Sets `Query` | Typical use |
|---|---|---|
| `QueryTooLargeException(string message)` | No (`string.Empty`) | Size failure where the query text is not available to attach. |
| `QueryTooLargeException(string message, string query)` | Yes | Preferred form. Carries the offending query so callers can report or log it. |

Both constructors pass `message` to the base class and set `ErrorCode` to `QUERY_TOO_LARGE`.

### Example

```csharp
throw new QueryTooLargeException(
    $"Query text exceeds maximum allowed length of {MaxQueryLength} characters.",
    queryText);
```

## Error code

| Code | Type |
|---|---|
| `QUERY_TOO_LARGE` | `QueryTooLargeException` |

See the full code table in [AnalysisException](AnalysisException.md#error-codes).

## When it is thrown

In the current code base, the exception is thrown from one place:

- **`DatabaseQuery.Parse()`** (`Models/DatabaseQuery.cs`). Before normalizing or parsing, `Parse()` checks `QueryText.Length` against a private constant, `MaxQueryLength = 100 * 1024` (102,400). If the text is longer, it throws `QueryTooLargeException` with the query text attached. The check runs after `ArgumentException.ThrowIfNullOrEmpty(QueryText)`, so an empty query fails with `ArgumentException` instead.

The limit is measured in UTF-16 characters (`string.Length`), not bytes. The message in the exception says "characters", which matches.

### Limits that do not throw this exception

Several other size settings exist. None of them throws `QueryTooLargeException`:

| Setting | Default | Behavior when exceeded |
|---|---|---|
| `DatabaseQuery` limit (hardcoded) | 102,400 characters | Throws `QueryTooLargeException`. |
| `AnalyzerSettings.MaxQueryLength` | 1,048,576 | Validated for a minimum of 1 only. Not enforced by `DatabaseQuery.Parse()`. |
| `SqlQueryAnalyzerOptions.Performance.MaxQueryLength` | 1,048,576 | `GetMaxQueryLength()` returns at least 1,024. Not enforced by `DatabaseQuery.Parse()`. |
| `ProfilerSettings.MaxQueryLengthChars` | 1,048,576 | `QueryProfilerService` throws a plain `ArgumentException` with its own message before it calls `Parse()`. |

So a query between 102,400 and 1,048,576 characters passes the profiler's own check but is then rejected by `DatabaseQuery.Parse()` with `QueryTooLargeException`, even though the configured limit is higher. Check the `DatabaseQuery` path first when a long query is unexpectedly rejected.

## Handling

```csharp
try
{
    query.Parse();
}
catch (QueryTooLargeException ex)
{
    // Tell the user the query is too long. ex.Query holds the rejected text.
    logger.LogWarning("Rejected query of {Length} characters", ex.Query.Length);
}
catch (AnalysisException ex) when (ex.ErrorCode == "QUERY_TOO_LARGE")
{
    // Same failure, matched by code.
}
```

Avoid logging `ex.Query` in full. It can be up to the size of the input, and the exception's `ToString()` includes it.

## Related

- [`AnalysisException`](AnalysisException.md) - base type, error codes, and retry behavior.
- [`SqlQueryAnalyzerException`](SqlQueryAnalyzerException.md) - root type for all library exceptions.
- [`SqlQueryAnalyzerExceptionExtensions`](SqlQueryAnalyzerExceptionExtensions.md) - formatting and classification helpers.
