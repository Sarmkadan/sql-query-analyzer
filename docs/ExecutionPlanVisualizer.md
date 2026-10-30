# ExecutionPlanVisualizer

**Namespace:** `SqlQueryAnalyzer.Visualization`
**Assembly:** `SqlQueryAnalyzer`
**Source:** `Visualization/ExecutionPlanVisualizer.cs`
**Implements:** [`IExecutionPlanVisualizer`](IExecutionPlanVisualizer.md)

`ExecutionPlanVisualizer` turns a parsed `QueryPlan` into fixed-width text. It draws an ASCII tree of the plan nodes with a relative cost bar per node, a horizontal cost-distribution chart per table access, bottleneck annotations with recommendations, and a dictionary of summary statistics. The output works in terminals, log files, CI consoles, and HTML `<pre>` elements.

The class is `sealed`. Rendering options (depth and node limits, cost-bar width, bottleneck threshold, and display flags) come from the `Visualization` section of `ProfilerSettings`, which is read once in the constructor. Changing the settings object after construction has no effect on an existing instance.

## Declaration

```csharp
public sealed class ExecutionPlanVisualizer : IExecutionPlanVisualizer
```

## Constructor

```csharp
public ExecutionPlanVisualizer(ProfilerSettings profilerSettings)
```

Copies `profilerSettings.Visualization` into a private field.

| Parameter | Type | Description |
|---|---|---|
| `profilerSettings` | `ProfilerSettings` | Source of the `Visualization` settings. Must not be `null`. |

**Exceptions**
- `ArgumentNullException`: `profilerSettings` is `null`.

The class is registered as a singleton for `IExecutionPlanVisualizer` by `AddQueryProfiler` in `Extensions/QueryProfilerExtensions.cs`, so most applications get an instance through dependency injection.

## Public members

| Member | Signature | Returns | Description |
|---|---|---|---|
| `Render` | `PlanVisualization Render(QueryPlan plan)` | `PlanVisualization` | Builds the full result: `TextTree`, `CostDistribution`, `Bottlenecks`, `Stats`, and `RenderedAt` (UTC now). |
| `RenderTree` | `string RenderTree(QueryPlan plan)` | `string` | Renders only the header and ASCII node tree. |
| `RenderCostDistribution` | `string RenderCostDistribution(QueryPlan plan)` | `string` | Renders only the per-table-access cost bar chart. |

All three methods throw `ArgumentNullException` when `plan` is `null`.

### `Render`

Calls `RenderTree`, `RenderCostDistribution`, the bottleneck search, and the statistics builder, then returns them in one `PlanVisualization`. Use it when you need more than one of the text outputs.

### `RenderTree`

Produces this layout:

1. A header with the plan format, optimizer cost, estimated rows, total node count, and capture time.
2. A separator line, then one line per rendered node. Each line shows the connector, node type, object name in parentheses when present, cost, estimated rows (when `ShowRowCounts` is on), a cost bar, and `◄ BOTTLENECK` when the node's cost is above `BottleneckCostThreshold` and `AnnotateBottlenecks` is on.
3. A note such as `... (N additional node(s) not shown …)` when `MaxDepth` or `MaxNodes` cut the tree.

If `plan.RootNode` is `null`, the method returns `(empty plan — no root node available)`.

With `ShowDetailedCosts` on, each node shows its I/O, CPU, and total costs. Otherwise it shows only the total.

### `RenderCostDistribution`

Lists the plan's `TableAccesses` in descending order of `EstimatedCost`. Each entry shows `TableName [AccessMethod]`, a bar scaled to the largest cost (`█` filled, `·` empty), and the cost. With `ShowRowCounts` on, an extra line shows `~rows`. Returns `(no table access operations recorded in this plan)` when the list is empty.

## Bottleneck detection

`Render` fills `PlanVisualization.Bottlenecks` with up to 10 nodes from `plan.AllNodes` whose `EstimatedCost` is above `BottleneckCostThreshold`, ordered by cost descending. Each `BottleneckAnnotation` has a recommendation chosen by `NodeType`:

| `NodeType` | Recommendation focus |
|---|---|
| `Table Scan` | Covering index on the filtered columns |
| `Index Scan` | A more selective index to allow a seek |
| `Hash Match` | Join column indexes, data type match, statistics |
| `Nested Loops` | Index on the inner-side join column |
| `Merge Join` | Sorted inputs on the join key |
| `Sort` | Index aligned with `ORDER BY` or `GROUP BY` |
| `Key Lookup` | Include the missing columns in the non-clustered index |
| `Parallelism` | MAXDOP and whether parallelism helps |
| `Spool` | Materialize the subquery or CTE into a temp table |
| anything else | General review of query structure and statistics |

## Stats dictionary

`Render` also fills `PlanVisualization.Stats` with these keys:

| Key | Value type | Source |
|---|---|---|
| `format` | `string` | `plan.Format` |
| `capturedAt` | `string` | `plan.CapturedAt` in `u` format |
| `isEstimated` | `bool` | `plan.IsEstimated` |
| `totalCost`, `totalIoCost`, `totalCpuCost` | `double` | Plan totals |
| `totalRows` | `int` | `plan.TotalEstimatedRows` |
| `nodeCount`, `tableAccessCount`, `joinCount` | `int` | Collection sizes |
| `tableScanCount`, `indexSeekCount`, `indexScanCount`, `sortCount`, `hashJoinCount` | `int` | Node counts by type; hash joins match `JoinType` containing "Hash" |
| `seekToScanRatio` | `string` | `"seeks/scans"` when there are table accesses, otherwise `"n/a"` |

Counts come from `plan.AllNodes`, so the statistics do not depend on `MaxNodes` or `MaxDepth`.

## Settings that affect output

These come from `VisualizationSettings` (`Configuration/ProfilerSettings.cs`):

| Setting | Default | Effect |
|---|---|---|
| `MaxDepth` | `10` | Nodes deeper than this are not rendered. |
| `MaxNodes` | `200` | Maximum nodes rendered in the tree. |
| `CostBarWidth` | `20` | Width of the cost bars. |
| `AnnotateBottlenecks` | `true` | Shows the `◄ BOTTLENECK` marker. |
| `BottleneckCostThreshold` | `5.0` | Cost above which a node is a bottleneck. |
| `ShowRowCounts` | `true` | Shows row estimates. |
| `ShowDetailedCosts` | `false` | Shows I/O and CPU cost components. |

`VisualizationSettings.Detailed()` and `VisualizationSettings.Compact()` provide presets for development and high-volume reporting.

## Usage

### Example: render a plan to the console and to a log

```csharp
using SqlQueryAnalyzer.Configuration;
using SqlQueryAnalyzer.Models;
using SqlQueryAnalyzer.Visualization;

var settings = new ProfilerSettings
{
    Visualization = VisualizationSettings.Compact()
};
var visualizer = new ExecutionPlanVisualizer(settings);

var plan = new QueryPlan
{
    Format = PlanFormat.Unknown,
    IsEstimated = true,
    TotalEstimatedCost = 12.0,
    TotalEstimatedRows = 1000,
    RootNode = new PlanNode
    {
        NodeType = "Nested Loops",
        EstimatedCost = 12.0,
        EstimatedRows = 1000,
        Children =
        {
            new PlanNode
            {
                NodeType = "Table Scan",
                ObjectName = "dbo.Orders",
                EstimatedCost = 11.0,
                EstimatedRows = 50000
            }
        }
    }
};
plan.Initialize(); // fills AllNodes, TableAccesses and Joins from RootNode

PlanVisualization result = visualizer.Render(plan);

Console.WriteLine(result.TextTree);
Console.WriteLine(result.CostDistribution);

foreach (var bottleneck in result.Bottlenecks)
{
    Console.WriteLine($"{bottleneck.NodeType} {bottleneck.ObjectName}: {bottleneck.Recommendation}");
}

// Or, as a single block for logs or CI output:
Console.WriteLine(result.ToCompactReport());
```

`Compact()` sets `BottleneckCostThreshold` to `10.0`, so both `Nested Loops` (12.0) and `Table Scan` (11.0) are marked `◄ BOTTLENECK` and listed in `result.Bottlenecks`, highest cost first. The `Table Scan` entry gets the covering-index recommendation.

## Notes

- Rendering does not change instance state, so one instance can be shared, as the DI registration does. Do not change the `ProfilerSettings` object while renders are running.
- The text formats are meant for people. Do not parse them in automated tests or other code; use `Stats`, `Bottlenecks`, or the `QueryPlan` model instead.
- `Render` does not modify the plan.
- `ProfilerSettings` is read once in the constructor, so create a new instance to use different settings.
- See also: [`IExecutionPlanVisualizer`](IExecutionPlanVisualizer.md), [`PlanVisualization`](PlanVisualization.md).
