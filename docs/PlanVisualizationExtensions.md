# PlanVisualizationExtensions

Provides extension methods for visualizing and analyzing SQL query plan bottlenecks, including cost aggregation, depth analysis, and node type distribution.

## API

### `GetTotalBottleneckCost(IEnumerable<BottleneckAnnotation> bottlenecks)`
Calculates the cumulative cost of all bottlenecks in the provided collection.

- **Parameters**: `bottlenecks` – An enumerable of `BottleneckAnnotation` objects representing query plan bottlenecks.
- **Returns**: The sum of all bottleneck costs as a `double`.
- **Throws**: `ArgumentNullException` if `bottlenecks` is `null`.

### `GetHighestCostBottleneck(IEnumerable<BottleneckAnnotation> bottlenecks)`
Retrieves the bottleneck annotation with the highest cost in the collection.

- **Parameters**: `bottlenecks` – An enumerable of `BottleneckAnnotation` objects.
- **Returns**: The `BottleneckAnnotation` with the highest cost, or `null` if the collection is empty or contains only zero-cost bottlenecks.
- **Throws**: `ArgumentNullException` if `bottlenecks` is `null`.

### `GetAverageBottleneckDepth(IEnumerable<BottleneckAnnotation> bottlenecks)`
Computes the average depth of bottlenecks in the collection.

- **Parameters**: `bottlenecks` – An enumerable of `BottleneckAnnotation` objects.
- **Returns**: The average depth as a `double`, or `0.0` if the collection is empty.
- **Throws**: `ArgumentNullException` if `bottlenecks` is `null`.

### `GetBottleneckCostPercentage(IEnumerable<BottleneckAnnotation> bottlenecks, double totalQueryCost)`
Calculates the percentage of total query cost represented by the bottlenecks.

- **Parameters**:
  - `bottlenecks` – An enumerable of `BottleneckAnnotation` objects.
  - `totalQueryCost` – The total cost of the query plan as a `double`.
- **Returns**: The percentage of `totalQueryCost` attributable to bottlenecks, or `0.0` if `totalQueryCost` is zero or negative.
- **Throws**: `ArgumentNullException` if `bottlenecks` is `null`; `ArgumentOutOfRangeException` if `totalQueryCost` is negative.

### `GetMostCommonBottleneckNodeType(IEnumerable<BottleneckAnnotation> bottlenecks)`
Determines the most frequently occurring node type among the bottlenecks.

- **Parameters**: `bottlenecks` – An enumerable of `BottleneckAnnotation` objects.
- **Returns**: The node type name as a `string`, or `null` if the collection is empty.
- **Throws**: `ArgumentNullException` if `bottlenecks` is `null`.

### `GetMaxBottleneckDepth(IEnumerable<BottleneckAnnotation> bottlenecks)`
Finds the maximum depth value among the bottlenecks.

- **Parameters**: `bottlenecks` – An enumerable of `BottleneckAnnotation` objects.
- **Returns**: The highest depth as an `int`, or `0` if the collection is empty.
- **Throws**: `ArgumentNullException` if `bottlenecks` is `null`.

### `GetBottleneckNodeTypeDistribution(IEnumerable<BottleneckAnnotation> bottlenecks)`
Generates a frequency distribution of node types among the bottlenecks.

- **Parameters**: `bottlenecks` – An enumerable of `BottleneckAnnotation` objects.
- **Returns**: An `IReadOnlyDictionary<string, int>` mapping node type names to their occurrence counts. Returns an empty dictionary if the collection is empty.
- **Throws**: `ArgumentNullException` if `bottlenecks` is `null`.

### `GetBottlenecksByNodeType(IEnumerable<BottleneckAnnotation> bottlenecks, string nodeType)`
Filters bottlenecks by a specific node type.

- **Parameters**:
  - `bottlenecks` – An enumerable of `BottleneckAnnotation` objects.
  - `nodeType` – The node type name to filter by.
- **Returns**: An `IEnumerable<BottleneckAnnotation>` containing only bottlenecks with the specified node type. Returns an empty sequence if no matches are found.
- **Throws**: `ArgumentNullException` if `bottlenecks` is `null` or if `nodeType` is `null`.

### `GetHighCostBottlenecks(IEnumerable<BottleneckAnnotation> bottlenecks, double threshold)`
Filters bottlenecks exceeding a given cost threshold.

- **Parameters**:
  - `bottlenecks` – An enumerable of `BottleneckAnnotation` objects.
  - `threshold` – The minimum cost value to consider a bottleneck "high cost."
- **Returns**: An `IEnumerable<BottleneckAnnotation>` containing bottlenecks with cost greater than `threshold`. Returns an empty sequence if no bottlenecks meet the threshold.
- **Throws**: `ArgumentNullException` if `bottlenecks` is `null`; `ArgumentOutOfRangeException` if `threshold` is negative.

### `GetBottlenecksAtDepth(IEnumerable<BottleneckAnnotation> bottlenecks, int depth)`
Filters bottlenecks occurring at a specific depth.

- **Parameters**:
  - `bottlenecks` – An enumerable of `BottleneckAnnotation` objects.
  - `depth` – The target depth level to filter by.
- **Returns**: An `IEnumerable<BottleneckAnnotation>` containing only bottlenecks at the specified depth. Returns an empty sequence if no bottlenecks match.
- **Throws**: `ArgumentNullException` if `bottlenecks` is `null`; `ArgumentOutOfRangeException` if `depth` is negative.

### `ToSummaryString(IEnumerable<BottleneckAnnotation> bottlenecks)`
Generates a human-readable summary of the bottlenecks.

- **Parameters**: `bottlenecks` – An enumerable of `BottleneckAnnotation` objects.
- **Returns**: A `string` containing a formatted summary, including total cost, average depth, and bottleneck count. Returns `"No bottlenecks found."` if the collection is empty.
- **Throws**: `ArgumentNullException` if `bottlenecks` is `null`.

## Usage

### Example 1: Analyzing a Query Plan
