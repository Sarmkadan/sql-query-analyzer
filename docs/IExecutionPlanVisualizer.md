# IExecutionPlanVisualizer

The `IExecutionPlanVisualizer` interface defines a contract for components that generate visual representations of SQL execution plans. Implementations of this interface process query execution plans and produce structured visualizations suitable for analysis, debugging, or presentation.

## API

### `ExecutionPlanVisualizer`

```csharp
public ExecutionPlanVisualizer()
```

Constructs a new instance of the execution plan visualizer.

**Parameters**
None.

**Return value**
None.

**Exceptions**
None.

---

### `PlanVisualization Render`

```csharp
public PlanVisualization Render(ExecutionPlan plan)
```

Generates a structured visualization of the provided execution plan.

**Parameters**
- `plan` – The execution plan to visualize. Must not be `null`.

**Return value**
A `PlanVisualization` object containing the rendered visualization data.

**Exceptions**
- `ArgumentNullException` – Thrown if `plan` is `null`.

---

### `string RenderTree`

```csharp
public string RenderTree(PlanVisualization visualization)
```

Renders the execution plan as a hierarchical tree structure in a textual format.

**Parameters**
- `visualization` – The `PlanVisualization` object produced by `Render`. Must not be `null`.

**Return value**
A string representing the tree structure of the execution plan.

**Exceptions**
- `ArgumentNullException` – Thrown if `visualization` is `null`.

---
### `string RenderCostDistribution`

```csharp
public string RenderCostDistribution(PlanVisualization visualization)
```

Renders the execution plan as a cost distribution analysis in a textual format.

**Parameters**
- `visualization` – The `PlanVisualization` object produced by `Render`. Must not be `null`.

**Return value**
A string representing the cost distribution of the execution plan.

**Exceptions**
- `ArgumentNullException` – Thrown if `visualization` is `null`.

## Usage

### Example 1: Basic Visualization

```csharp
var plan = new ExecutionPlan(/* ... */);
var visualizer = new ExecutionPlanVisualizer();
var visualization = visualizer.Render(plan);
var tree = visualizer.RenderTree(visualization);
var cost = visualizer.RenderCostDistribution(visualization);

Console.WriteLine("Tree View:");
Console.WriteLine(tree);

Console.WriteLine("\nCost Distribution:");
Console.WriteLine(cost);
```

### Example 2: Integration with a Query Analyzer

```csharp
public class QueryAnalyzer
{
    private readonly IExecutionPlanVisualizer _visualizer;

    public QueryAnalyzer(IExecutionPlanVisualizer visualizer)
    {
        _visualizer = visualizer;
    }

    public void Analyze(ExecutionPlan plan)
    {
        if (plan == null) throw new ArgumentNullException(nameof(plan));

        var visualization = _visualizer.Render(plan);
        var tree = _visualizer.RenderTree(visualization);
        var cost = _visualizer.RenderCostDistribution(visualization);

        // Log or display the results
        StoreVisualization(tree, cost);
    }
}
```

## Notes

- Implementations must ensure thread safety if shared across multiple threads. The interface itself does not enforce thread safety, so callers must handle synchronization if needed.
- The `Render` method may throw additional exceptions if the execution plan contains invalid or unsupported elements, but these are implementation-specific and not part of the contract.
- The textual output formats (`RenderTree` and `RenderCostDistribution`) are designed for human readability and may change between implementations. Do not rely on exact formatting in automated tests.
