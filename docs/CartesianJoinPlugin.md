# CartesianJoinPlugin

`CartesianJoinPlugin` is an analysis plugin that flags Cartesian products in SQL queries. It lives in `Plugins/CartesianJoinPlugin.cs`, derives from `AnalysisPluginBase`, and runs in the `IAnalysisPlugin` pipeline through `ProcessAsync`.

A Cartesian product pairs every row of one table with every row of another. It usually comes from a missing join condition, and on real tables it multiplies row counts and produces very large intermediate results. The plugin reports two forms:

- **Implicit CROSS JOIN**: several tables listed in a `FROM` clause separated by commas, with no join condition.
- **Explicit CROSS JOIN**: the `CROSS JOIN` keyword.

Both are reported as `IssueType.CrossJoin` with `IssueSeverity.Critical`.

## Detection logic

From the type's XML documentation, the plugin:

- Flags `FROM` clauses that list multiple tables separated by commas, without proper join conditions, as implicit cross joins.
- Flags explicit `CROSS JOIN` syntax as a potential performance issue.

The implementation works as follows:

1. It finds every `FROM` clause in `QueryAnalysisResult.Query` with the regex `FROM\s+(.*?)(?:\s+WHERE|\s+GROUP|\s+ORDER|\s+HAVING|\s+LIMIT|\s*;|$)`, case-insensitive and singleline. The captured text runs up to the next `WHERE`, `GROUP`, `ORDER`, `HAVING`, `LIMIT`, `;`, or the end of the query.
2. It skips clauses whose captured text is empty or whitespace.
3. **Implicit check:** it removes SQL comments and normalizes whitespace in the captured clause (shared utilities), then splits on `,`. If there are two or more non-empty parts, it adds an implicit cross join issue.
4. **Explicit check:** it searches the query text from the `FROM` match position to the end for `CROSS\s+JOIN`. If found, it adds an explicit cross join issue.

Because the explicit check runs for each `FROM` match, a `CROSS JOIN` that follows several `FROM` keywords is reported once per match. The line number of an explicit issue is an estimate based on character offset (about 50 characters per line), not an exact position.

Each detected pattern adds one `PerformanceIssue` with:

| Pattern | `IssueType` | `Severity` | `AffectedClause` | `EstimatedPerformanceImpact` | `Metadata["pattern"]` |
|---|---|---|---|---|---|
| Implicit | `IssueType.CrossJoin` | `IssueSeverity.Critical` | `"FROM"` | `90.0` | `"implicit-cross-join"` |
| Explicit | `IssueType.CrossJoin` | `IssueSeverity.Critical` | `"FROM"` | `85.0` | `"explicit-cross-join"` |

Each issue also carries these metadata entries:

- `impact_reason`: why the pattern is expensive.
- `best_practice`: the recommended join style.
- `from_clause` (implicit issues only): the captured `FROM` clause text.

The recommended fix for both forms is `FROM Table1 t1 INNER JOIN Table2 t2 ON t1.Id = t2.Table1Id`.

If `IsEnabled` is `false`, or the query is null or whitespace, `ProcessAsync` returns the result unchanged. If the query has no `FROM` keyword, nothing is reported.

## Public members

### `CartesianJoinPlugin(ILogger<CartesianJoinPlugin>? logger = null)`
Creates the plugin. The logger is optional. When it is `null`, the plugin does not log.

### `PluginId`
`public override string PluginId`

Returns `"cartesian-join-detection"`.

### `Name`
`public override string Name`

Returns `"Cartesian Join Detection Plugin"`.

### `Version`
`public override Version Version`

Returns `1.0.0`.

### `ProcessAsync(QueryAnalysisResult result)`
`public override async Task<QueryAnalysisResult> ProcessAsync(QueryAnalysisResult result)`

Scans `result.Query` and appends any detected cross join issues to `result.Issues`. It returns the same `result` instance.

### `IsEnabled` (inherited)
`public bool IsEnabled { get; set; }` from `AnalysisPluginBase`. It defaults to `true`. When it is `false`, `ProcessAsync` makes no changes.

## Example

```csharp
using SqlQueryAnalyzer.Models;
using SqlQueryAnalyzer.Plugins;

var plugin = new CartesianJoinPlugin();

var result = new QueryAnalysisResult
{
    Query = "SELECT o.Id, c.Name FROM Orders o, Customers c WHERE o.Id > 0",
    QueryId = "orders-customers-1"
};

var processed = await plugin.ProcessAsync(result);

// processed.Issues contains one issue:
//   IssueType = IssueType.CrossJoin
//   Severity  = IssueSeverity.Critical
//   Metadata["pattern"] = "implicit-cross-join"
```

Rewriting the query with an explicit join condition removes the issue:

```sql
SELECT o.Id, c.Name FROM Orders o INNER JOIN Customers c ON c.Id = o.CustomerId WHERE o.Id > 0
```
