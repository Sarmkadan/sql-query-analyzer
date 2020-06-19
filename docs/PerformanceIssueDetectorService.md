# PerformanceIssueDetectorService

Analyzes SQL query execution plans and patterns to identify common performance bottlenecks such as N+1 queries, inefficient joins, and missing indexes.

## API

### `PerformanceIssueDetectorService()`

Initializes a new instance of the `PerformanceIssueDetectorService` with default configuration.

### `PerformanceIssueDetectorService(IQueryExecutionPlanProvider planProvider)`

Initializes a new instance of the `PerformanceIssueDetectorService` with a specified query execution plan provider.

Parameters:
- `planProvider`: Provides access to SQL query execution plans required for analysis.

### `async Task<List<PerformanceIssue>> DetectIssuesAsync()`

Detects all supported performance issues in the provided execution plans asynchronously.

Returns:
- A `Task` resolving to a list of `PerformanceIssue` objects representing detected issues.

Throws:
- `ArgumentNullException`: If the execution plan provider returns null.

### `ValueTask<List<PerformanceIssue>> DetectNPlusOneAsync()`

Detects N+1 query patterns in the execution plans asynchronously.

Returns:
- A `ValueTask` resolving to a list of `PerformanceIssue` objects representing detected N+1 issues.

Throws:
- `InvalidOperationException`: If the execution plans cannot be analyzed due to missing or invalid data.

### `ValueTask<List<PerformanceIssue>> DetectJoinIssuesAsync()`

Detects inefficient join operations in the execution plans asynchronously.

Returns:
- A `ValueTask` resolving to a list of `PerformanceIssue` objects representing detected join inefficiencies.

Throws:
- `InvalidOperationException`: If the execution plans cannot be analyzed due to missing or invalid data.

### `ValueTask<List<PerformanceIssue>> DetectIndexOpportunitiesAsync()`

Detects missing index opportunities in the execution plans asynchronously.

Returns:
- A `ValueTask` resolving to a list of `PerformanceIssue` objects representing potential index improvements.

Throws:
- `InvalidOperationException`: If the execution plans cannot be analyzed due to missing or invalid data.

## Usage
