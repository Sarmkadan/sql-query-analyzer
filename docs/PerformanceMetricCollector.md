# PerformanceMetricCollector

`PerformanceMetricCollector` is a utility class designed to gather, track, and report performance metrics related to SQL query analysis operations. It records key indicators such as execution time, cache behavior, success rates, and throughput, enabling performance evaluation and diagnostics over multiple analysis cycles. The class supports aggregation of metrics across multiple executions and provides percentile-based timing insights.

## API

### `public PerformanceMetricCollector()`

Initializes a new instance of the `PerformanceMetricCollector` with default values for all metrics. The initial state reflects zero analyses, no cache hits or misses, and default timing values.

### `public void RecordAnalysisMetric(double executionTimeMs, bool cacheHit, bool successful)`

Records a single analysis metric with the provided execution time, cache hit status, and success status.

- **Parameters**:
  - `executionTimeMs`: The execution time of the analysis in milliseconds.
  - `cacheHit`: A boolean indicating whether the analysis result was retrieved from cache.
  - `successful`: A boolean indicating whether the analysis completed successfully.
- **Throws**: `ArgumentOutOfRangeException` if `executionTimeMs` is negative.

### `public double GetAverageExecutionTimeMs()`

Calculates and returns the average execution time in milliseconds across all recorded analyses.

- **Returns**: The average execution time in milliseconds. Returns `0.0` if no analyses have been recorded.
- **Throws**: None.

### `public double GetCacheHitRatio()`

Calculates and returns the ratio of cache hits to total cache accesses (hits + misses).

- **Returns**: The cache hit ratio as a value between `0.0` and `1.0`. Returns `0.0` if no cache accesses have been recorded.
- **Throws**: None.

### `public double GetSuccessRate()`

Calculates and returns the ratio of successful analyses to total analyses.

- **Returns**: The success rate as a value between `0.0` and `1.0`. Returns `0.0` if no analyses have been recorded.
- **Throws**: None.

### `public double GetThroughput()`

Calculates and returns the number of analyses completed per second based on the total execution time.

- **Returns**: The throughput in analyses per second. Returns `0.0` if no analyses have been recorded or total execution time is zero.
- **Throws**: None.

### `public double GetPercentileExecutionTime(double percentile)`

Calculates and returns the execution time at the specified percentile across all recorded analyses.

- **Parameters**:
  - `percentile`: The percentile to compute (e.g., `0.95` for the 95th percentile).
- **Returns**: The execution time in milliseconds at the specified percentile. Returns `0.0` if no analyses have been recorded or if `percentile` is outside the range `[0.0, 1.0]`.
- **Throws**: `ArgumentOutOfRangeException` if `percentile` is less than `0.0` or greater than `1.0`.

### `public PerformanceReport GetReport()`

Generates and returns a comprehensive performance report summarizing all recorded metrics.

- **Returns**: A `PerformanceReport` object containing aggregated metrics such as average execution time, success rate, cache hit ratio, throughput, and percentile execution times.
- **Throws**: None.

### `public void Reset()`

Resets all recorded metrics to their initial values, clearing all analysis history and counters.

- **Returns**: None.
- **Throws**: None.

### `public string QueryId`

Gets or sets the identifier for the query being analyzed. This value is used to associate metrics with a specific query.

- **Returns**: The current query identifier as a string. Can be `null` if not set.
- **Throws**: None.

### `public double ExecutionTimeMs`

Gets or sets the execution time of the most recent analysis in milliseconds.

- **Returns**: The execution time of the last recorded analysis. Returns `0.0` if no analysis has been recorded.
- **Throws**: None.

### `public int IssueCount`

Gets or sets the number of issues detected during the most recent analysis.

- **Returns**: The number of issues detected. Returns `0` if no issues have been recorded.
- **Throws**: None.

### `public bool CacheHit`

Gets or sets a boolean indicating whether the most recent analysis result was retrieved from cache.

- **Returns**: `true` if the result was a cache hit; otherwise, `false`. Returns `false` if no analysis has been recorded.
- **Throws**: None.

### `public bool Successful`

Gets or sets a boolean indicating whether the most recent analysis completed successfully.

- **Returns**: `true` if the analysis was successful; otherwise, `false`. Returns `false` if no analysis has been recorded.
- **Throws**: None.

### `public DateTime Timestamp`

Gets or sets the timestamp of the most recent analysis.

- **Returns**: The timestamp of the last recorded analysis. Returns `DateTime.MinValue` if no analysis has been recorded.
- **Throws**: None.

### `public int TotalAnalyses`

Gets the total number of analyses recorded.

- **Returns**: The total number of analyses. Returns `0` if no analyses have been recorded.
- **Throws**: None.

### `public int SuccessfulAnalyses`

Gets the total number of successful analyses recorded.

- **Returns**: The number of successful analyses. Returns `0` if no analyses have been recorded.
- **Throws**: None.

### `public int FailedAnalyses`

Gets the total number of failed analyses recorded.

- **Returns**: The number of failed analyses. Returns `0` if no analyses have been recorded.
- **Throws**: None.

### `public int CacheHits`

Gets the total number of cache hits recorded.

- **Returns**: The number of cache hits. Returns `0` if no cache accesses have been recorded.
- **Throws**: None.

### `public int CacheMisses`

Gets the total number of cache misses recorded.

- **Returns**: The number of cache misses. Returns `0` if no cache accesses have been recorded.
- **Throws**: None.

## Usage

### Example 1: Recording and Reporting Metrics in a Loop
