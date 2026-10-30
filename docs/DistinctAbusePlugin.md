# DistinctAbusePlugin

`DistinctAbusePlugin` is an analysis plugin that flags `SELECT DISTINCT` queries whose `FROM` clause contains joins. It lives in `Plugins/DistinctAbusePlugin.cs`, derives from `AnalysisPluginBase`, and runs in the `IAnalysisPlugin` pipeline through `ProcessAsync`.

`DISTINCT` combined with joins often hides a join-fanout problem. The database must materialize the full join result before it removes duplicates, which costs I/O and memory. Duplicates frequently come from join keys that are not constrained, not from a real need for deduplication. The plugin reports the pattern as a warning so the join conditions can be reviewed.

## Detection logic

From the type's XML documentation, the plugin:

- Flags `SELECT DISTINCT` combined with `JOIN`s in the `FROM` clause as a potential join-fanout smell.
- Suggests verifying that join keys properly constrain the result, to avoid unnecessary duplicate elimination.

The implementation works as follows:

1. It finds every match of `SELECT\s+DISTINCT\s+(.*?)\s+FROM` in `QueryAnalysisResult.Query`, case-insensitive and singleline. The captured group is the select list.
2. It skips matches whose select list is empty or whitespace.
3. It takes the text after the match (from the end of `FROM` to the end of the query) and passes it to the private `HasJoinClauses` check. A match is reported when that check returns `true`. `HasJoinClauses` removes comments and normalizes whitespace, then looks for either:
   - An explicit join: `(INNER|LEFT|RIGHT|FULL|CROSS)\s+JOIN\b`.
   - A comma join: `,\s*\w+`.

Because the comma check runs over the whole remaining query, a comma later in the statement (for example, in a `WHERE ... IN (1, 2)` list or a following `SELECT` list) also counts as a join.

Each reported match adds one `PerformanceIssue` with:

- `IssueType`: `IssueType.LargeResultSet`
- `Severity`: `IssueSeverity.Warning`
- `AffectedClause`: `"SELECT DISTINCT"`
- `ColumnNumber`: `1`
- `EstimatedPerformanceImpact`: `40.0`
- `RecommendedFix` and `ExampleFix`: guidance on constraining joins or removing `DISTINCT`
- `Metadata` entries: `select_clause`, `impact_reason`, `best_practice`, `pattern` (`"distinct-with-joins"`), and `join_verification_check`

`LineNumber` is computed by counting newlines in the captured select list, not in the full query. Treat it as approximate.

If `IsEnabled` is `false`, or the query text is null or whitespace, `ProcessAsync` returns the result unchanged. The plugin does not implement `IDetectorPlugin`, so it has no `Analyze` method.

## Public members

### `DistinctAbusePlugin(ILogger<DistinctAbusePlugin>? logger = null)`
Creates the plugin. The logger is optional. When it is `null`, the plugin does not log.

### `PluginId`
`public override string PluginId`

Returns `"distinct-abuse-detection"`.

### `Name`
`public override string Name`

Returns `"DISTINCT Abuse Detection Plugin"`.

### `Version`
`public override Version Version`

Returns `1.0.0`.

### `ProcessAsync(QueryAnalysisResult result)`
`public override async Task<QueryAnalysisResult> ProcessAsync(QueryAnalysisResult result)`

Scans `result.Query` for `SELECT DISTINCT ... FROM` patterns that contain joins and appends one issue per match to `result.Issues`. It returns the same `result` instance, with no changes when nothing matches.

### `IsEnabled` (inherited)
`public bool IsEnabled { get; set; }` from `AnalysisPluginBase`. It defaults to `true`. When it is `false`, `ProcessAsync` makes no changes.

## Example

```csharp
using SqlQueryAnalyzer.Constants;
using SqlQueryAnalyzer.Models;
using SqlQueryAnalyzer.Plugins;

var plugin = new DistinctAbusePlugin();

var result = new QueryAnalysisResult
{
    Query = "SELECT DISTINCT o.Id, c.Name FROM Orders o INNER JOIN Customers c ON c.Id = o.CustomerId",
    QueryId = "orders-distinct-1"
};

var processed = await plugin.ProcessAsync(result);

// processed.Issues contains one issue:
//   IssueType = IssueType.LargeResultSet
//   Severity  = IssueSeverity.Warning
//   AffectedClause = "SELECT DISTINCT"
//   Metadata["pattern"] = "distinct-with-joins"
//   Metadata["select_clause"] = "o.Id, c.Name"
```

Removing the `DISTINCT` keyword, or removing the join, stops the issue from being reported.
