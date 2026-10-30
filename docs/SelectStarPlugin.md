# SelectStarPlugin

`SelectStarPlugin` is an analysis plugin that detects `SELECT *` (and `table.*`) in a query and recommends an explicit column list. It lives in `Plugins/SelectStarPlugin.cs` and derives from `AnalysisPluginBase`, so it is part of the `IAnalysisPlugin` pipeline.

The plugin reports `SELECT *` because it returns every column, including unused ones, which increases I/O, memory use, and network traffic. Explicit column lists also make the query's intent clearer.

## Detection logic

From the type's XML documentation, the plugin:

- Flags `SELECT *` patterns case-insensitively.
- Ignores `COUNT(*)`, because the select clause is `COUNT(*)` and not a bare star.
- Ignores stars inside SQL comments. Comments are removed from the select clause before the check runs.

In `ProcessAsync`, the plugin runs the regex `SELECT\s+(.*?)\s+FROM` (case-insensitive, singleline) over `QueryAnalysisResult.Query` and checks each captured select clause. A clause is flagged when, after comment removal and whitespace normalization, it is either:

- `*` (matched by `^\*$`),
- `table.*` (matched by `^\w+\.\*$`), or
- a comma-separated list in which at least one item is `*` or `table.*`.

Each match adds one `PerformanceIssue` to `result.Issues`. The issue has:

- `IssueType`: `IssueType.SelectStar`
- `Severity`: `IssueSeverity.Warning`
- `EstimatedPerformanceImpact`: `15.0`
- `LineNumber`: derived from the number of newlines before the match
- `Metadata` entries: `select_clause`, `impact_reason`, and `best_practice`

If `IsEnabled` is `false`, or the query is null or whitespace, `ProcessAsync` returns the result unchanged.

## Public members

### `SelectStarPlugin(ILogger<SelectStarPlugin>? logger = null)`
Creates the plugin. The logger is optional. When it is `null`, the plugin does not log.

### `PluginId`
`public override string PluginId`

Returns `"select-star-detection"`.

### `Name`
`public override string Name`

Returns `"SELECT * Detection Plugin"`.

### `Version`
`public override Version Version`

Returns `1.0.0`.

### `ProcessAsync(QueryAnalysisResult result)`
`public override Task<QueryAnalysisResult> ProcessAsync(QueryAnalysisResult result)`

Scans `result.Query` for `SELECT *` patterns and appends an issue for each one found. It returns the same `result` instance that was passed in, wrapped in a completed `Task`.

### `IsEnabled` (inherited)
`public bool IsEnabled { get; set; }` from `AnalysisPluginBase`. It defaults to `true`. When it is `false`, `ProcessAsync` makes no changes.

## Example

```csharp
using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Constants;
using SqlQueryAnalyzer.Models;
using SqlQueryAnalyzer.Plugins;

var plugin = new SelectStarPlugin();

var result = new QueryAnalysisResult
{
    Query = "SELECT * FROM Orders",
    QueryId = "orders-1"
};

var processed = await plugin.ProcessAsync(result);

// processed.Issues contains one issue:
//   IssueType = IssueType.SelectStar
//   Severity  = IssueSeverity.Warning
//   Description starts with "SELECT * detected"
```
