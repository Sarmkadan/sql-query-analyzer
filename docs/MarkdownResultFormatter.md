# MarkdownResultFormatter

**Namespace:** `SqlQueryAnalyzer.Formatters`
**Assembly:** `SqlQueryAnalyzer`
**Source:** `Formatters/MarkdownResultFormatter.cs`
**Implements:** `IResultFormatter`

`MarkdownResultFormatter` turns analysis results into a Markdown report. The report has a header with the query ID, score, complexity and analysis time, followed by a summary table, an issues table, index suggestions, the SQL text in a fenced block, and a table of additional metrics. `FormatBatch` produces a combined report for several results. The formatter is stateless and has no constructor parameters, so one instance can be reused.

Use it when a human will read the output in a Markdown viewer, a pull request comment, or a wiki page. For machine-readable output, use `JsonResultFormatter`.

## Declaration

```csharp
public class MarkdownResultFormatter : IResultFormatter
```

## Public members

| Member | Signature | Returns | Description |
|---|---|---|---|
| `Format` | `string Format(QueryAnalysisResult result)` | Markdown document | Renders one result as a full report titled `# SQL Query Analysis Report`. |
| `FormatBatch` | `string FormatBatch(IEnumerable<QueryAnalysisResult> results)` | Markdown document | Renders several results as one report titled `# SQL Query Analysis - Batch Report`. |
| `GetFormatType` | `string GetFormatType()` | `"md"` | Returns the format identifier. |

The class has no other public members. It has no constructor parameters and no public properties.

### `Format`

Produces these sections, in this order:

1. **Header.** Query ID, score (`PerformanceScore`, one decimal, `/100`), `Complexity`, and `AnalyzedAt` in `yyyy-MM-dd HH:mm:ss UTC`.
2. **Summary.** A metric table with total issues, critical, warning and info counts, index suggestion count, and `TotalOptimizationPotential` (one decimal, `%`).
3. **Issues Found.** A table of `Severity | Rule | Message`, sorted by severity (highest first), then by `EstimatedPerformanceImpact` (highest first). If there are no issues, the section shows a single line saying so.
4. **Index Suggestions.** Shown only when `IndexSuggestions` is not empty. Sorted by `EstimatedPerformanceGain`, highest first. A null `ColumnName` is shown as `N/A`.
5. **SQL Query.** `Query` inside a ```` ```sql ```` fence, written as-is.
6. **Additional Information.** `ComplexityScore`, `HasCriticalIssues`, and `EstimatedExecutionTime` in whole milliseconds.

Sections 2 to 6 are always present, except for section 4.

Two details to know:

- In issue messages and index table cells, a `|` character is replaced with `‖` (U+2016), so it does not break the table.
- The `Query` text is not escaped. A query that contains a line of three backticks would close the code fence early.

### `FormatBatch`

Produces these sections:

1. **Header.** Generation time (`DateTime.UtcNow`) and the number of results.
2. **Summary Statistics.** Issue counts across all results, total index suggestions, and the sum of `TotalOptimizationPotential` across results. Note that this is a plain sum, so it can exceed 100%.
3. **Query Details.** One `###` subsection per result. The heading is the first 20 characters of `QueryId`. Each subsection has a metric table, and when the result has issues, a **Top Issues** table with at most five rows. Those rows are sorted by `EstimatedPerformanceImpact`.

`FormatBatch` does not include the SQL text, the index suggestions, or the per-query issue descriptions. Use `Format` for a per-query report.

### `GetFormatType`

Returns `"md"`. Use this value to choose a file extension or a content-type mapping.

## Fields not rendered

`Format` and `FormatBatch` do not output these `QueryAnalysisResult` members: `ExecutionPlan`, `Statistics`, `Metadata`, `Diagnostics`, `IsPartial`, and `PerformanceIssue` details such as `RecommendedFix` and `ExampleFix`. Use a different formatter or a custom one if you need them.

## Input handling

The formatter does not validate its arguments. Passing `null` throws `NullReferenceException`. Results with a `null` `QueryId` also fail in `FormatBatch`, because it calls `Substring` on it.

## Example

This example formats a result with one critical table scan, one informational `SELECT *` issue, and one index suggestion. The output below is the exact text returned by `Format` for this input.

```csharp
using SqlQueryAnalyzer.Constants;
using SqlQueryAnalyzer.Formatters;
using SqlQueryAnalyzer.Models;

var result = new QueryAnalysisResult
{
    QueryId = "q-2026-001",
    Query = "SELECT * FROM Orders o JOIN Customers c ON c.Id = o.CustomerId WHERE o.Status = 'open'",
    AnalyzedAt = new DateTime(2026, 10, 30, 12, 0, 0, DateTimeKind.Utc),
    Complexity = QueryComplexity.Medium,
    PerformanceScore = 62.5,
    EstimatedExecutionTime = TimeSpan.FromMilliseconds(840),
    Issues =
    {
        new PerformanceIssue
        {
            IssueType = IssueType.TableScan,
            Severity = IssueSeverity.Critical,
            Description = "Full table scan on Orders | filter on Status",
            EstimatedPerformanceImpact = 45.0,
        },
        new PerformanceIssue
        {
            IssueType = IssueType.SelectStar,
            Severity = IssueSeverity.Info,
            Description = "SELECT * returns unneeded columns",
            EstimatedPerformanceImpact = 5.0,
        },
    },
    IndexSuggestions =
    {
        new IndexSuggestion
        {
            TableName = "Orders",
            ColumnName = "Status",
            EstimatedPerformanceGain = 38.0,
        },
    },
};

var formatter = new MarkdownResultFormatter();
string markdown = formatter.Format(result);
Console.WriteLine(markdown);
```

Output:

```markdown
# SQL Query Analysis Report

**Query ID:** `q-2026-001`
**Score:** `62.5/100`
**Complexity:** `Medium`
**Analyzed:** `2026-10-30 12:00:00 UTC`

## Summary

| Metric | Value |
|--------|-------|
| Total Issues | 2 |
| Critical Issues | 1 |
| Warning Issues | 0 |
| Info Issues | 1 |
| Index Suggestions | 1 |
| Optimization Potential | 38.0% |

## Issues Found

| Severity | Rule | Message |
|----------|------|---------|
| 🔴 Critical | `TableScan` | Full table scan on Orders ‖ filter on Status |
| ℹ️ Info | `SelectStar` | SELECT * returns unneeded columns |

## Index Suggestions

| Table | Column | Estimated Gain |
|-------|--------|----------------|
| `Orders` | `Status` | 38.0% |

## SQL Query

```sql
SELECT * FROM Orders o JOIN Customers c ON c.Id = o.CustomerId WHERE o.Status = 'open'
```

## Additional Information

| Metric | Value |
|--------|-------|
| Complexity Score | 7 |
| Has Critical Issues | True |
| Estimated Execution Time | 840 ms |
```

Notes on the output:

- The pipe in `"Orders | filter"` is rendered as `‖`.
- The critical issue is listed before the info issue because severity sorts first.
- `Optimization Potential` is `38.0%` because it comes from `TotalOptimizationPotential`, which is derived from the index suggestion.
- `Complexity Score` is computed by `QueryComplexityScorer`, not set by the caller.

## Related types

- [`IResultFormatter`](IResultFormatter.md): the interface this class implements.
- `JsonResultFormatter` and `SqlFormatter` in the same folder are the other formatters.
- `QueryAnalysisResult`, `PerformanceIssue`, and `IndexSuggestion` in `Models/` are the inputs.
