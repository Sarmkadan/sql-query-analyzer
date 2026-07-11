# PerformanceMetricsCalculator

`PerformanceMetricsCalculator` is a static utility class that provides a set of helper methods for evaluating the performance characteristics of SQL queries. The methods compute scores, estimates, and predictions based on query execution metrics, index statistics, and historical data, enabling callers to quantify optimization potential, complexity, maintenance effort, and trends without exposing internal calculation logic.

## API

### CalculateCombinedScore
**Purpose:** Computes a normalized score that combines multiple performance factors (e.g., cost, index usage, execution time) into a single double value for easy comparison.  
**Parameters:**  
- `queryMetrics` (`QueryMetrics`) – An object containing the raw metrics for the query to evaluate.  
**Return value:** A double representing the combined score; higher values indicate better overall performance.  
**Exceptions:**  
- `ArgumentNullException` if `queryMetrics` is null.  
- `InvalidOperationException` if required metric fields are missing or contain invalid values.

### EstimateTotalOptimization
**Purpose:** Estimates the total possible optimization gain (as a percentage) achievable by applying recommended index and query‑plan changes.  
**Parameters:**  
- `currentPlan` (`ExecutionPlan`) – The current execution plan of the query.  
- `recommendations` (`IEnumerable<OptimizationRecommendation>`) – A collection of suggested optimizations.  
**Return value:** A double between 0 and 100 indicating the estimated percentage reduction in cost or execution time.  
**Exceptions:**  
- `ArgumentNullException` if either parameter is null.  
- `ArgumentException` if the recommendations collection is empty.

### CalculateComplexityScore
**Purpose:** Calculates an integer score reflecting the syntactic and structural complexity of a SQL query (e.g., number of joins, subqueries, conditional logic).  
**Parameters:**  
- `sqlText` (`string`) – The raw SQL query string.  
**Return value:** An integer where higher values denote greater complexity.  
**Exceptions:**  
- `ArgumentNullException` if `sqlText` is null.  
- `FormatException` if the string does not contain a parsable SQL statement.

### CalculateIndexUsageScore
**Purpose:** Determines how effectively existing indexes are used by a query, expressed as a score from 0 to 1.  
**Parameters:**  
- `indexUsage` (`IDictionary<string, IndexUsageInfo>`) – Mapping of index names to usage statistics (seeks, scans, lookups).  
**Return value:** A double representing the proportion of optimal index usage; 1.0 indicates ideal usage.  
**Exceptions:**  
- `ArgumentNullException` if `indexUsage` is null.  
- `InvalidOperationException` if the dictionary lacks required usage fields.

### CalculateMaintenanceEffort
**Purpose:** Estimates the ongoing maintenance effort (in arbitrary units) required to keep the current set of indexes effective for the query workload.  
**Parameters:**  
- `workload` (`WorkloadInfo`) – Information about query frequency and patterns.  
- `indexes` (`IEnumerable<IndexInfo>`) – The indexes under consideration.  
**Return value:** An integer score; higher values suggest greater maintenance overhead.  
**Exceptions:**  
- `ArgumentNullException` if either parameter is null.  
- `ArgumentException` if the workload or indexes collections are empty.

### GetPerformanceTrend
**Purpose:** Returns a categorical description of the query’s performance trend over time (e.g., “Improving”, “Stable”, “Degrading”).  
**Parameters:**  
- `historicalMetrics` (`IEnumerable<QueryMetricSnapshot>`) – Time‑ordered snapshots of the query’s performance data.  
**Return value:** A string representing the trend.  
**Exceptions:**  
- `ArgumentNullException` if `historicalMetrics` is null.  
- `InvalidOperationException` if fewer than two snapshots are supplied.

### CalculateExecutionTimeDistribution
**Purpose:** Builds a histogram of execution times, grouping observed durations into buckets and returning the count per bucket.  
**Parameters:**  
- `executionTimes` (`IEnumerable<TimeSpan>`) – Collected execution times for the query.  
- `bucketSize` (`TimeSpan`) – The width of each histogram bucket.  
**Return value:** A dictionary where the key is the bucket’s lower bound (formatted as a string) and the value is the number of observations falling into that bucket.  
**Exceptions:**  
- `ArgumentNullException` if either parameter is null.  
- `ArgumentOutOfRangeException` if `bucketSize` is less than or equal to zero.

### CalculateIndexROI
**Purpose:** Computes the return on investment for a specific index by comparing the performance gain it provides against its maintenance cost.  
**Parameters:**  
- `indexInfo` (`IndexInfo`) – Details of the index to evaluate.  
- `queryStats` (`QueryStatistics`) – Aggregated statistics of queries that use the index.  
**Return value:** A double representing the ROI ratio; values > 1 indicate a net benefit.  
**Exceptions:**  
- `ArgumentNullException` if either parameter is null.  
- `InvalidOperationException` if essential fields (e.g., usage count, cost) are missing or zero.

### PredictExecutionTime
**Purpose:** Predicts the future execution time of a query based on its current plan and recent performance history.  
**Parameters:**  
- `currentPlan` (`ExecutionPlan`) – The plan to be used for prediction.  
- `recentHistory` (`IQueryable<QueryMetricSnapshot>`) – Recent metric snapshots, ordered chronologically.  
**Return value:** A `TimeSpan` representing the predicted execution duration.  
**Exceptions:**  
- `ArgumentNullException` if either parameter is null.  
- `InvalidOperationException` if the recent history does not contain sufficient data points for a reliable prediction.

## Usage

```csharp
using SqlQueryAnalyzer.Metrics;
using System;
using System.Collections.Generic;

// Example 1: Calculate a combined score and decide whether to re‑index.
var metrics = QueryMetricsCollector.Gather(queryId);
double combinedScore = PerformanceMetricsCalculator.CalculateCombinedScore(metrics);

if (combinedScore < 0.4)
{
    var recommendations = IndexAdvisor.GetRecommendations(queryId);
    double optGain = PerformanceMetricsCalculator.EstimateTotalOptimization(
                        ExecutionPlanRetriever.GetCurrentPlan(queryId),
                        recommendations);
    Console.WriteLine($"Estimated optimization gain: {optGain:F1}%");
}

// Example 2: Build an execution‑time distribution histogram for reporting.
List<TimeSpan> times = ExecutionLogReader.GetExecutionTimes(queryId);
var histogram = PerformanceMetricsCalculator.CalculateExecutionTimeDistribution(
                    times, TimeSpan.FromSeconds(0.5));

foreach (var bucket in histogram)
{
    Console.WriteLine($"{bucket.Key}s – {bucket.Value} occurrences");
}
```

## Notes

- All methods are **stateless** and rely solely on their input arguments; therefore they are thread‑safe for concurrent invocation as long as the supplied objects are not mutated during the call.  
- Passing `null` for any reference‑type parameter will consistently result in an `ArgumentNullException`.  
- Methods that accept collections will throw if the collection is empty or lacks the required elements, as they cannot produce a meaningful result without data.  
- Numeric outputs (scores, percentages, ratios) are defined to be non‑negative; callers should treat any negative value as an indication of an internal error, though such values are not expected under normal operation.  
- The class does not maintain any internal caches or static state, so successive calls with identical inputs will recompute the result each time. If caching is desired, it should be implemented at the caller level.
