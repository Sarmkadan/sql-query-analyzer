# QueryRewriteService

**Namespace:** `SqlQueryAnalyzer.Services`
**Assembly:** `SqlQueryAnalyzer`
**Source:** `Services/QueryRewriteService.cs`
**Interface:** `IQueryRewriteService` (declared in the same file)

`QueryRewriteService` is the pattern-based engine that inspects a parsed [`DatabaseQuery`](DatabaseQuery.md) and produces two kinds of output:

1. **Rewrite suggestions**: a [`QueryRewriteSuggestion`](QueryRewriteSuggestion.md) per applicable textual rewrite (for example `NOT IN` to `NOT EXISTS`, or `OR` to `UNION ALL`), with a rationale and an estimated improvement.
2. **Index recommendations**: [`IndexSuggestion`](IndexSuggestion.md) objects derived from the WHERE and JOIN structure, with generated `CREATE` and `DROP` scripts.

The service uses regular expressions and clause-level inspection of `DatabaseQuery`. It does not parse the SQL grammar and does not connect to a database, so it never checks the schema, existing indexes, or real statistics. Treat the suggestions as candidates to review, not as changes to apply automatically. Every rewrite suggestion is created with `IsAutoApplicable = false`.

The class is stateless apart from its injected logger. It is safe to register as a singleton.

## Declaration

```csharp
public interface IQueryRewriteService
public class QueryRewriteService : IQueryRewriteService
```

## Constructor

```csharp
public QueryRewriteService(ILogger<QueryRewriteService> logger)
```

| Parameter | Type | Description |
|---|---|---|
| `logger` | `ILogger<QueryRewriteService>` | Receives information logs for each call and error logs for failed generation. The constructor does not check for `null`. |

## Public members

### `GenerateRewriteSuggestionsAsync`

```csharp
public Task<List<QueryRewriteSuggestion>> GenerateRewriteSuggestionsAsync(
    DatabaseQuery query,
    CancellationToken cancellationToken = default)
```

Runs every rewrite rule against `query` and returns the suggestions that apply.

| Parameter | Description |
|---|---|
| `query` | The parsed query to analyse. Must not be `null`. |
| `cancellationToken` | Checked once before the rules run. Cancellation is not checked between rules. |

**Returns:** A `Task` whose result is a `List<QueryRewriteSuggestion>`, possibly empty. The list is ordered by `Priority` ascending (lower values first), then by `EstimatedImprovementPercent` descending.

**Rules evaluated:**

| `RewriteType` | Applies when | Priority | Breaking change |
|---|---|---|---|
| `ExplicitColumnSelection` | The text contains `SELECT *` (case-insensitive). | 3 | No |
| `OrToUnionAll` | `WhereConditions` is not empty and contains ` OR `. | 2 | No |
| `FunctionSargability` | `WhereConditions` is not empty and the text calls `UPPER`, `LOWER`, `CONVERT`, `CAST`, `DATEPART`, `YEAR`, `MONTH`, `DAY`, `LTRIM`, `RTRIM`, or `ISNULL`. | 1 | No |
| `SubqueryToJoin` | The text matches `IN (SELECT`. | 2 | No |
| `NotInToNotExists` | The text matches `NOT IN (SELECT`. | 2 | No |
| `UnionToUnionAll` | The text contains `UNION` that is not followed by `ALL`. | 3 | Yes |
| `ResultSetPagination` | The text starts with `SELECT` and has no `TOP n`, `FETCH NEXT`, or `LIMIT`. | 2 | Yes |

Only suggestions where `IsValid()` returns `true` are returned. That means the rewritten text is non-blank, the rationale is non-blank, and the rewritten text differs from the original.

**Exceptions:**

- `ArgumentNullException` if `query` is `null`.
- `OperationCanceledException` if `cancellationToken` is cancelled before the rules run.
- Any exception raised by a rule is logged at error level and rethrown.

### `GenerateIndexRecommendationsAsync`

```csharp
public Task<List<IndexSuggestion>> GenerateIndexRecommendationsAsync(
    DatabaseQuery query,
    CancellationToken cancellationToken = default)
```

Derives index candidates from the WHERE and JOIN clauses of `query`.

| Parameter | Description |
|---|---|
| `query` | The parsed query to analyse. |
| `cancellationToken` | Checked once before the work starts. |

**Returns:** A `Task` whose result is a `List<IndexSuggestion>`, possibly empty. Results are not sorted. WHERE-clause suggestions come first, then JOIN-column suggestions.

**How the indexes are built:**

- **WHERE clause.** If the query has WHERE conditions and referenced tables, the service takes the column names that appear before `=`, `>`, `<`, `>=`, `<=`, `LIKE`, or `IN`. Column names are de-duplicated and must be longer than one character. It creates one `NONCLUSTERED` suggestion per referenced table, using the first two columns as key columns and the next three as included columns. The same column list is used for every table, because the regex does not know which table each column belongs to.
- **JOIN clause.** For each join condition of the form `a.x = b.y`, it creates one single-column `NONCLUSTERED` suggestion for each side whose table is in `ReferencedTables` (case-insensitive). Conditions that do not match this form are skipped.

Each suggestion has `GenerateIndexName()`, `GenerateCreateScript()`, and `GenerateDropScript()` already called, so `GeneratedCreateScript` and `GeneratedDropScript` are populated.

**Exceptions:**

- `OperationCanceledException` if `cancellationToken` is cancelled before the work starts.
- Any exception raised while building suggestions is logged at error level and rethrown.
- `query` is not null-checked. Passing `null` throws `NullReferenceException` when the log message reads `query.QueryId`.

## Behavior notes

- **Related index suggestions are not populated.** The interface summary for `GenerateRewriteSuggestionsAsync` says each rewrite is paired with index recommendations. The current implementation does not set `QueryRewriteSuggestion.RelatedIndexSuggestions`, so it stays empty. To get index candidates, call `GenerateIndexRecommendationsAsync` separately.
- **Logging.** Successful calls log an information message with the query ID and the number of results.
- **Input dependence.** Results depend on what `DatabaseQuery.Parse()` extracted. If `ReferencedTables`, `WhereConditions`, or `JoinConditions` are empty, the matching rules return nothing. Some rules inspect `QueryText` directly, so they still work without a `Parse()` call.

## Usage

### Example: Get rewrites and index candidates for a query

```csharp
using Microsoft.Extensions.Logging.Abstractions;
using SqlQueryAnalyzer.Models;
using SqlQueryAnalyzer.Services;

var query = new DatabaseQuery
{
    QueryText = """
        SELECT * FROM Orders o
        JOIN Customers c ON o.CustomerId = c.Id
        WHERE UPPER(c.Email) = 'USER@EXAMPLE.COM'
        """
};
query.Parse();

IQueryRewriteService service =
    new QueryRewriteService(NullLogger<QueryRewriteService>.Instance);

var rewrites = await service.GenerateRewriteSuggestionsAsync(query);
foreach (var rewrite in rewrites)
{
    Console.WriteLine(
        $"[P{rewrite.Priority}] {rewrite.RewriteType}: " +
        $"~{rewrite.EstimatedImprovementPercent}% - {rewrite.Rationale}");
}

var indexes = await service.GenerateIndexRecommendationsAsync(query);
foreach (var index in indexes)
{
    Console.WriteLine(index.GeneratedCreateScript);
}
```

In an application, resolve `IQueryRewriteService` from the DI container instead of creating the instance by hand. Register it with the `AddQueryRewriteService()` extension method in `Extensions/QueryRewriteExtensions.cs`, which adds the service as a singleton.

## See also

- [`QueryRewriteSuggestion`](QueryRewriteSuggestion.md) describes the returned rewrite objects.
- [`IndexSuggestion`](IndexSuggestion.md) describes the returned index objects.
- [`DatabaseQuery`](DatabaseQuery.md) describes the input and the `Parse()` method.
