# UnboundedOrderByPlugin

`UnboundedOrderByPlugin` is an analysis plugin that flags `ORDER BY` clauses that are not limited by pagination. It lives in `Plugins/UnboundedOrderByPlugin.cs`, derives from `AnalysisPluginBase`, and implements `IDetectorPlugin`. It is therefore usable both in the `IAnalysisPlugin` pipeline (through `ProcessAsync`) and as a standalone rule (through `Analyze`).

An `ORDER BY` without `TOP`, `LIMIT`, or `OFFSET ... FETCH` asks the database to sort the entire result set before returning rows. On large tables this can be expensive, so the plugin reports it as a potential full-sort operation. The finding is informational, not a defect.

## Detection logic

From the type's XML documentation, the plugin:

- Flags `ORDER BY` clauses that have no `TOP`, `LIMIT`, or `OFFSET ... FETCH` clause.
- Reports each such clause as a potential full-sort operation.

The implementation works as follows:

1. It finds every `ORDER BY` in `DatabaseQuery.QueryText` (or `QueryAnalysisResult.Query` in `ProcessAsync`) with the regex `ORDER\s+BY\s+([^;]*?)(?=\s*(?:;|$))`, case-insensitive and singleline. The captured text runs to the next `;` or the end of the query.
2. It skips clauses whose captured text is empty or whitespace.
3. It treats the clause as bounded, and reports nothing, if `HasPagination` returns `true`. Pagination is detected when any of the following holds:
   - A `TOP n` appears before the `ORDER BY`.
   - A `ROW_NUMBER(` appears before the `ORDER BY` (the usual subquery pattern).
   - A `LIMIT n` appears anywhere in the query.
   - An `OFFSET n ROWS FETCH NEXT n ROWS ONLY` or a `FETCH NEXT n ROWS ONLY` appears anywhere in the query.

Only the `TOP` and `ROW_NUMBER` checks are restricted to the text before the `ORDER BY`. The `LIMIT` and `OFFSET`/`FETCH` checks look at the whole query, because some dialects place them after the `ORDER BY`.

Each unbounded clause adds one `PerformanceIssue` with:

- `IssueType`: `IssueType.LargeResultSet`
- `Severity`: `IssueSeverity.Info`
- `AffectedClause`: `"ORDER BY"`
- `EstimatedPerformanceImpact`: `20.0`
- `LineNumber`: derived from the number of newlines before the match
- `Metadata` entries: `rule_id` (`"unbounded-orderby"`), `orderby_clause`, `impact_reason`, `best_practice`, `pattern`, and `subsumes_rules` (`"missing-where-limit"`)

The `subsumes_rules` entry marks this finding as covering the same span as a `missing-where-limit` finding. According to the code comment, this prevents the two from being counted twice.

If `IsEnabled` is `false`, or the query text is null or whitespace, `ProcessAsync` returns the result unchanged. A null argument to `Analyze` throws `ArgumentNullException`.

## Public members

### `UnboundedOrderByPlugin(ILogger<UnboundedOrderByPlugin>? logger = null)`
Creates the plugin. The logger is optional. When it is `null`, the plugin does not log.

### `PluginId`
`public override string PluginId`

Returns `"unbounded-orderby-detection"`.

### `Name`
`public override string Name`

Returns `"Unbounded ORDER BY Detection Plugin"`.

### `Version`
`public override Version Version`

Returns `1.0.0`.

### `RuleId`
`public string RuleId`

Returns `"unbounded-orderby"`. This is the `IDetectorPlugin` rule identifier, and it is also written to the `rule_id` metadata of each issue.

### `Analyze(DatabaseQuery query)`
`public IEnumerable<PerformanceIssue> Analyze(DatabaseQuery query)`

Implements `IDetectorPlugin.Analyze`. It returns one issue per unbounded `ORDER BY` in `query.QueryText`, or an empty sequence when there are none.

### `ProcessAsync(QueryAnalysisResult result)`
`public override async Task<QueryAnalysisResult> ProcessAsync(QueryAnalysisResult result)`

Runs `Analyze` on `result.Query` and appends the issues to `result.Issues`. It returns the same `result` instance.

### `IsEnabled` (inherited)
`public bool IsEnabled { get; set; }` from `AnalysisPluginBase`. It defaults to `true`. When it is `false`, `ProcessAsync` makes no changes.

## Example

```csharp
using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Constants;
using SqlQueryAnalyzer.Models;
using SqlQueryAnalyzer.Plugins;

var plugin = new UnboundedOrderByPlugin();

var result = new QueryAnalysisResult
{
    Query = "SELECT Id, CreatedAt FROM Orders ORDER BY CreatedAt",
    QueryId = "orders-unbounded-1"
};

var processed = await plugin.ProcessAsync(result);

// processed.Issues contains one issue:
//   IssueType = IssueType.LargeResultSet
//   Severity  = IssueSeverity.Info
//   Metadata["rule_id"] = "unbounded-orderby"
```

Adding `OFFSET 0 ROWS FETCH NEXT 100 ROWS ONLY` to the query removes the issue, because the `OFFSET ... FETCH` pattern is detected.
