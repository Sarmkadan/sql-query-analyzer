# QueryPlanExtensions

Utility class providing extension methods for analyzing SQL query execution plans represented as `PlanNode` trees. These methods help identify performance bottlenecks, expensive operations, and overall plan efficiency by exposing cost metrics, node filtering, and summary statistics.

## API

### `GetCostPercentage(PlanNode node)`
Calculates the percentage of total plan cost represented by a specific node.
- **Parameters**:
  - `node`: The `PlanNode` whose cost percentage is to be calculated.
- **Returns**: A `double` between 0 and 100 representing the cost percentage.
- **Throws**: `ArgumentNullException` if `node` is `null`.

### `GetNodesAboveThreshold(double threshold)`
Filters nodes in the plan whose cost exceeds a specified threshold.
- **Parameters**:
  - `threshold`: The minimum cost value (inclusive) to include in the result.
- **Returns**: A `List<PlanNode>` containing all nodes with cost ≥ `threshold`, ordered by descending cost.
- **Throws**: `ArgumentOutOfRangeException` if `threshold` is negative.

### `CalculateCumulativeCost(PlanNode node)`
Computes the total cumulative cost of a node and all its descendants in the plan tree.
- **Parameters**:
  - `node`: The root `PlanNode` from which to calculate cumulative cost.
- **Returns**: A `double` representing the sum of costs for the node and all its children.
- **Throws**: `ArgumentNullException` if `node` is `null`.

### `GetMostExpensiveTableAccess()`
Identifies the most expensive table access operation in the query plan.
- **Returns**: The `TableAccess` node with the highest cost, or `null` if no table access nodes exist.
- **Throws**: Does not throw.

### `GetMostExpensiveJoin()`
Identifies the most expensive join operation in the query plan.
- **Returns**: The `Join` node with the highest cost, or `null` if no join nodes exist.
- **Throws**: Does not throw.

### `HasTableScans()`
Determines whether the query plan contains any table scan operations.
- **Returns**: `true` if at least one `TableAccess` node with a scan type exists; otherwise, `false`.
- **Throws**: Does not throw.

### `GetFilteringNodes()`
Retrieves all nodes in the plan that perform filtering operations.
- **Returns**: A `List<PlanNode>` of nodes where the operation type indicates filtering (e.g., `Filter`, `Predicate`, etc.), ordered by cost descending.
- **Throws**: Does not throw.

### `GetCpuToIoCostRatio()`
Calculates the ratio of CPU cost to I/O cost across the entire query plan.
- **Returns**: A `double` representing CPU cost divided by I/O cost. Returns `double.NaN` if I/O cost is zero.
- **Throws**: Does not throw.

### `GetSortingNodes()`
Retrieves all nodes in the plan that perform sorting operations.
- **Returns**: A `List<PlanNode>` of nodes where the operation type indicates sorting (e.g., `Sort`, `TopN`, etc.), ordered by cost descending.
- **Throws**: Does not throw.

### `GetPerformanceSummary()`
Generates a dictionary summarizing key performance metrics of the query plan.
- **Returns**: A `Dictionary<string, object>` with keys such as `"TotalCost"`, `"CpuCost"`, `"IoCost"`, `"MaxNodeCost"`, `"TableScans"`, and `"JoinCount"`, mapping to their respective values.
- **Throws**: Does not throw.

### `IsEfficient(double threshold)`
Determines whether the query plan is considered efficient based on a cost threshold.
- **Parameters**:
  - `threshold`: The maximum acceptable total cost for the plan to be considered efficient.
- **Returns**: `true` if the total plan cost is ≤ `threshold`; otherwise, `false`.
- **Throws**: `ArgumentOutOfRangeException` if `threshold` is negative.

### `GetNodesForTable(string tableName)`
Filters nodes in the plan that reference a specific table.
- **Parameters**:
  - `tableName`: The name of the table to match against node references.
- **Returns**: A `List<PlanNode>` of nodes that reference `tableName`, ordered by cost descending.
- **Throws**: `ArgumentException` if `tableName` is `null` or whitespace.

## Usage
