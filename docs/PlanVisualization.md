# PlanVisualization

Represents a fully rendered, human-readable visualization of a SQL query execution plan. It aggregates the textual tree representation, cost breakdowns, identified bottlenecks, runtime statistics, and actionable tuning recommendations into a single structured object. Instances are typically produced by the plan analysis pipeline and are intended for display in diagnostic tools, logs, or reports.

## API

### `TextTree` : `string`

A multi-line string containing the hierarchical, indented tree view of the execution plan operators. Each line corresponds to a plan node, showing its type, object name, and estimated cost. This is the primary visual representation used for quick manual inspection.

### `CostDistribution` : `string`

A formatted string describing how the total estimated cost is distributed across the major operators in the plan. Typically includes percentage breakdowns and highlights the most expensive subtrees. Useful for identifying cost hotspots at a glance.

### `Bottlenecks` : `List<BottleneckAnnotation>`

A list of `BottleneckAnnotation` objects, each describing a specific performance problem detected in the plan. Annotations include the offending node identifier, a severity level, a description of the issue, and a suggested mitigation. The list may be empty if no bottlenecks were found.

### `Stats` : `Dictionary<string, object>`

A dictionary of supplementary statistics gathered during plan analysis. Keys are metric names (e.g., `"TotalSubtreeCost"`, `"EstimatedRows"`, `"MissingIndexCount"`), and values are the corresponding numeric or string data. Consumers should check for key existence before accessing, as the set of available statistics depends on the plan source and analysis depth.

### `RenderedAt` : `DateTime`

The UTC timestamp at which this visualization was generated. Set at the moment the analysis pipeline completes rendering. Can be used to determine the freshness of the visualization relative to the underlying plan data.

### `ToCompactReport` : `string`

Returns a condensed, single-line summary string suitable for logging or tabular display. The format includes the most critical information: node identifier, operator type, estimated cost, and the primary recommendation if one exists. The exact format is subject to change across versions; consumers should treat it as a human-readable label, not a machine-parseable contract.

### `NodeId` : `string`

The unique identifier of the root node of the execution plan from which this visualization was generated. Corresponds to the `NodeId` property of the top-level plan operator.

### `NodeType` : `string`

The operator type of the root node (e.g., `"SELECT"`, `"INSERT"`, `"MERGE"`). This is the logical operation classification as reported by the query optimizer.

### `ObjectName` : `string`

The name of the primary database object targeted by the root operation, if applicable. For statements operating on tables, views, or indexed structures, this contains the schema-qualified name. May be `null` or empty for plans that do not target a specific object (e.g., ad-hoc constant scans).

### `EstimatedCost` : `double`

The optimizer-estimated cost of the entire plan, as reported for the root node. This value is dimensionless and relative to the optimizer’s internal cost model. Higher values indicate more expensive plans.

### `Depth` : `int`

The maximum depth of the operator tree, measured as the number of edges from the root node to the deepest leaf. A depth of zero indicates a plan consisting of only the root node.

### `Recommendation` : `string`

A human-readable tuning recommendation for the entire plan, synthesized from the detected bottlenecks and cost analysis. If no actionable recommendation can be produced, this property is an empty string. It never returns `null`.

### `ToString` : `override string`

Returns the same value as `TextTree`. Overrides `object.ToString()` to enable convenient display in debuggers, consoles, and logging frameworks that implicitly call `ToString()` on objects.

## Usage

### Example 1: Rendering a Plan to Console with Bottleneck Details

```csharp
// Assume 'analyzer' is an initialized PlanAnalyzer and 'rawPlan' is an execution plan XML string.
PlanVisualization visualization = analyzer.AnalyzeAndRender(rawPlan);

Console.WriteLine(visualization.TextTree);
Console.WriteLine();
Console.WriteLine("Cost Distribution:");
Console.WriteLine(visualization.CostDistribution);

if (visualization.Bottlenecks.Count > 0)
{
    Console.WriteLine("Bottlenecks Detected:");
    foreach (var bottleneck in visualization.Bottlenecks)
    {
        Console.WriteLine($"  [{bottleneck.Severity}] {bottleneck.Description}");
        Console.WriteLine($"  Suggestion: {bottleneck.Mitigation}");
    }
}
else
{
    Console.WriteLine("No bottlenecks identified.");
}

Console.WriteLine($"Rendered at: {visualization.RenderedAt:u}");
```

### Example 2: Logging Compact Reports for a Batch of Plans

```csharp
// 'planBatch' is a collection of execution plan strings to be analyzed in bulk.
var reportLines = new List<string>();

foreach (var planXml in planBatch)
{
    PlanVisualization vis = analyzer.AnalyzeAndRender(planXml);
    
    // Log a one-line summary for each plan.
    reportLines.Add(vis.ToCompactReport);
    
    // Optionally store statistics for later aggregation.
    if (vis.Stats.TryGetValue("TotalSubtreeCost", out var costObj) && costObj is double totalCost)
    {
        AggregateCost(totalCost);
    }
}

File.WriteAllLines(@"C:\Reports\plan_summaries.txt", reportLines);
```

## Notes

- **Nullability**: `ObjectName` may be `null` or empty for plans without a direct object reference. `Recommendation` returns an empty string when no recommendation is available, never `null`. All other string properties return non-null values; `TextTree` and `CostDistribution` are guaranteed to contain at least minimal content for any successfully rendered plan.
- **Thread Safety**: `PlanVisualization` is an immutable data transfer object once constructed. All public members are read-only. Concurrent reads from multiple threads are safe without synchronization. The `List<BottleneckAnnotation>` and `Dictionary<string, object>` properties return references to internal collections that are not defensively copied; callers must not modify them. Modification by a caller introduces thread-safety risks and may corrupt shared state.
- **Statistics Dictionary**: The keys present in `Stats` depend on the capabilities of the analysis pipeline and the richness of the input plan. Always guard access with `TryGetValue` or a key-existence check. Values may be boxed numeric types (`int`, `long`, `double`) or strings; casting without type checking can throw `InvalidCastException`.
- **Timestamp Precision**: `RenderedAt` uses `DateTime.UtcNow` precision, which on most platforms is limited to approximately 10–15 milliseconds. Do not rely on it for high-resolution ordering of visualizations generated in rapid succession.
- **`ToString` Behavior**: Because `ToString` delegates to `TextTree`, embedding a `PlanVisualization` in string interpolation or passing it to logging frameworks that call `ToString` will output the full tree. For compact logging, explicitly call `ToCompactReport` instead.
