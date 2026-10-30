# HtmlPlanVisualizer

**Namespace:** `SqlQueryAnalyzer.Visualization`
**Assembly:** `SqlQueryAnalyzer`
**Source:** `Visualization/HtmlPlanVisualizer.cs`
**Implements:** `IHtmlPlanVisualizer`

`HtmlPlanVisualizer` turns a `QueryPlan` into HTML. It renders a collapsible plan tree in which each node shows a cost bar colored by its relative cost, a table of table accesses, a list of bottleneck highlights with recommendations, and a grid of summary statistics. `RenderHtml` returns a complete, self-contained document with inline CSS. `RenderHtmlFragment` returns only the body markup, for embedding in a larger report.

The class is `sealed`. It reads `profilerSettings.Visualization` once in the constructor, so later changes to the settings object have no effect on an existing instance. Only `MaxNodes` and `BottleneckCostThreshold` are read by this class; `MaxDepth`, `CostBarWidth`, `AnnotateBottlenecks`, `ShowRowCounts`, and `ShowDetailedCosts` are ignored here.

`AddQueryProfiler` is not involved. `Program.cs` registers the class directly as a singleton: `services.AddSingleton<IHtmlPlanVisualizer, HtmlPlanVisualizer>();`.

## Declaration

```csharp
public sealed class HtmlPlanVisualizer : IHtmlPlanVisualizer
```

## Constructor

```csharp
public HtmlPlanVisualizer(ProfilerSettings profilerSettings)
```

Copies `profilerSettings.Visualization` into a private field.

| Parameter | Type | Description |
|---|---|---|
| `profilerSettings` | `ProfilerSettings` | Source of the `Visualization` settings. Must not be `null`. |

**Exceptions**
- `ArgumentNullException`: `profilerSettings` is `null`.

## Public members

| Member | Signature | Returns | Description |
|---|---|---|---|
| `RenderHtml` | `string RenderHtml(QueryPlan plan)` | `string` | Returns a full `<!DOCTYPE html>` page. It embeds the output of `RenderHtmlFragment` in `<body>` and adds a `<title>`, a viewport meta tag, and an inline `<style>` block. |
| `RenderHtmlFragment` | `string RenderHtmlFragment(QueryPlan plan)` | `string` | Returns the body markup only, without a page wrapper or styles. |

Both methods throw `ArgumentNullException` when `plan` is `null`.

The class also implements `IHtmlPlanVisualizer`, so callers can depend on the interface and receive an instance through dependency injection.

### `RenderHtml`

Calls `RenderHtmlFragment` and wraps the result in a page with `lang="en"` and UTF-8 charset. The page styles cover the body, headings, the collapsible tree, the cost bars (`cost-low`, `cost-medium`, `cost-high`), tables, the statistic cards, and the empty-state box. The page has no scripts and loads no external resources, so it can be saved to a file or opened offline.

### `RenderHtmlFragment`

Produces these sections, in order:

1. **Summary Statistics.** Cards for Format, Total Cost (`F2`), Estimated Rows (`N0`), Plan Nodes, Table Accesses, and Captured At (UTC, `u` format).
2. **Plan Tree.** A nested `<details>` element per node. The root is open by default. Each `<summary>` shows the node type, the object name in parentheses when present, the depth (or `Root`), estimated rows, estimated cost, and a bar whose width is the node's cost divided by the plan's maximum cost. Bar color is `cost-high` at 66% or more of the maximum, `cost-medium` at 33% or more, and `cost-low` otherwise. Bars are at least 2% wide. Only the first `MaxNodes` children of each node are rendered.
3. **Table Accesses.** A table with Table, Access Method, Cost, and Rows columns, sorted by cost descending. It shows a single row stating that no table accesses were recorded when the list is empty.
4. **Bottleneck Highlights.** A bulleted list of up to 10 nodes from `plan.AllNodes` whose `EstimatedCost` is greater than or equal to `BottleneckCostThreshold`, sorted by cost descending. Each item carries a recommendation chosen by `NodeType`: `Table Scan`, `Index Scan`, `Hash Match`, and `Sort` have specific advice; any other type gets a general review note. When nothing reaches the threshold, the section shows an empty-state message.

If `plan.RootNode` is `null`, `RenderHtmlFragment` returns a section with an empty-state message and skips the other sections.

All text from the plan (node types, object names, table names, access methods, and recommendations) is HTML-encoded with `WebUtility.HtmlEncode`. Numbers are formatted with `InvariantCulture`.

## Notes

- **Lazy initialization.** If `RootNode` is set but `AllNodes`, `TableAccesses`, and `Joins` are all empty, the renderer calls `plan.Initialize()` before rendering. That call rebuilds those lists from the tree and replaces their contents. Plans that already have populated lists are not touched.
- **Threshold comparison.** The bottleneck check uses `>=`. The text renderer, `ExecutionPlanVisualizer`, uses `>`.
- **Bottleneck recommendations.** This class covers four node types with specific advice. `ExecutionPlanVisualizer` covers more types, so the two outputs can differ for the same plan.
- **Maximum cost.** The cost-bar scale uses the larger of `TotalEstimatedCost` and the highest node cost. If that value is zero or less, the scale is 1.

## Example

Render a small plan to a file. The plan has a sort over a table scan, and `RootNode` is the only structure the caller sets. The renderer fills in the flat lists itself.

```csharp
using SqlQueryAnalyzer.Configuration;
using SqlQueryAnalyzer.Models;
using SqlQueryAnalyzer.Visualization;

var plan = new QueryPlan
{
    DatabaseName = "Sales",
    Format = PlanFormat.SqlServer,
    TotalEstimatedCost = 12.5,
    TotalEstimatedRows = 4200,
    RootNode = new PlanNode
    {
        NodeType = "Sort",
        EstimatedCost = 8.0,
        EstimatedRows = 4200,
        Depth = 0,
        Children =
        {
            new PlanNode
            {
                NodeType = "Table Scan",
                ObjectName = "dbo.Orders",
                EstimatedCost = 6.5,
                EstimatedRows = 120000,
                Depth = 1
            }
        }
    }
};

var visualizer = new HtmlPlanVisualizer(ProfilerSettings.ForDevelopment());

// Full page for a standalone report
File.WriteAllText("plan.html", visualizer.RenderHtml(plan));

// Body markup only, for embedding in another page
string fragment = visualizer.RenderHtmlFragment(plan);
```

With `ProfilerSettings.ForDevelopment()`, the visualization threshold is `VisualizationSettings.Detailed()`'s value of 2.0. The `Sort` node (cost 8.0) and the `Table Scan` node (cost 6.5) both meet it, so both appear under Bottleneck Highlights with `Sort` first. `Initialize` also adds a `TableAccess` entry for the `Table Scan` node, so Table Accesses shows one row for `dbo.Orders`.
