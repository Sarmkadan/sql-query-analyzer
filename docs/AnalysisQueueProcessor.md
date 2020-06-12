# AnalysisQueueProcessor

The `AnalysisQueueProcessor` is a core component of the `sql-query-analyzer` project responsible for managing the asynchronous execution of SQL query analysis tasks. It implements a producer-consumer pattern, allowing analysis requests to be enqueued and processed concurrently up to a defined limit. The processor tracks the lifecycle of each task from creation to completion, maintains real-time statistics on queue depth and throughput, and provides mechanisms for retrieving results or handling errors via callbacks.

## API

### Constructor
*   **`public AnalysisQueueProcessor()`**
    *   Initializes a new instance of the `AnalysisQueueProcessor`. Default configuration values (such as `MaxConcurrency`) are applied unless exposed via specific setters (not listed in public members).

### Queue Management
*   **`public string EnqueueAnalysis(string query)`**
    *   Submits a new SQL query string to the analysis queue.
    *   **Parameters:**
        *   `query`: The SQL query string to be analyzed.
    *   **Returns:** A unique `string` identifier (`TaskId`) for the queued task.
    *   **Throws:** May throw an exception if the queue is in a stopped state or if the input `query` is invalid (e.g., null or empty), depending on internal validation logic.

*   **`public void Start()`**
    *   Initiates the background processing loop. Once started, the processor begins dequeuing items and executing analysis tasks up to the `MaxConcurrency` limit.
    *   **Throws:** May throw if called when the processor is already running.

*   **`public async Task StopAsync()`**
    *   Gracefully shuts down the processor. This method waits for all currently active tasks to complete before returning. No new tasks will be processed after this method is invoked.
    *   **Returns:** A `Task` representing the asynchronous operation.

### Task Inspection
*   **`public AnalysisTask? GetTaskStatus(string taskId)`**
    *   Retrieves the current status and details of a specific task.
    *   **Parameters:**
        *   `taskId`: The unique identifier returned by `EnqueueAnalysis`.
    *   **Returns:** An `AnalysisTask` object containing status details if the task exists; otherwise, `null`.

*   **`public QueueStatistics GetStatistics()`**
    *   Returns a snapshot of the current queue performance and load metrics.
    *   **Returns:** A `QueueStatistics` object containing counts for queued, active, and processed items.

### AnalysisTask Properties
The `AnalysisTask` object returned by `GetTaskStatus` (or represented internally) exposes the following members:

*   **`public string TaskId`**
    *   Gets the unique identifier for this specific analysis task.

*   **`public string Query`**
    *   Gets the original SQL query string submitted for analysis.

*   **`public AnalysisTaskStatus Status`**
    *   Gets the current state of the task (e.g., Queued, Running, Completed, Failed).

*   **`public DateTime CreatedAt`**
    *   Gets the timestamp when the task was initially enqueued.

*   **`public DateTime? StartedAt`**
    *   Gets the timestamp when the task began execution. Returns `null` if the task has not yet started.

*   **`public DateTime? CompletedAt`**
    *   Gets the timestamp when the task finished (successfully or with error). Returns `null` if the task is still pending or running.

*   **`public QueryAnalysisResult? Result`**
    *   Gets the analysis output if the task completed successfully. Returns `null` if the task is incomplete or failed.

*   **`public string? ErrorMessage`**
    *   Gets the error message if the task failed. Returns `null` if the task is successful or incomplete.

*   **`public Action<QueryAnalysisResult>? OnComplete`**
    *   Gets or sets a callback delegate invoked automatically when the task completes successfully.

*   **`public TimeSpan GetElapsedTime()`**
    *   Calculates and returns the total duration of the task. If the task is running, it calculates the time from `StartedAt` to the current moment. If completed, it calculates the difference between `CompletedAt` and `StartedAt`.

### Configuration and Metrics
*   **`public int QueuedCount`**
    *   Gets the number of tasks currently waiting in the queue to be processed.

*   **`public int ActiveCount`**
    *   Gets the number of tasks currently being executed.

*   **`public int MaxConcurrency`**
    *   Gets or sets the maximum number of tasks allowed to run simultaneously.

*   **`public int TotalProcessed`**
    *   Gets the cumulative count of tasks that have been fully processed (completed or failed) since the processor started.

## Usage

### Example 1: Basic Lifecycle Management
This example demonstrates initializing the processor, submitting a query, and waiting for completion.

```csharp
using SqlQueryAnalyzer;

// Initialize and start the processor
var processor = new AnalysisQueueProcessor();
processor.MaxConcurrency = 4; // Configure parallelism
processor.Start();

// Enqueue a query
string taskId = processor.EnqueueAnalysis("SELECT * FROM Users WHERE IsActive = 1");

// Poll for status
AnalysisTask? task = null;
while (task == null || task.Status == AnalysisTaskStatus.Queued || task.Status == AnalysisTaskStatus.Running)
{
    await Task.Delay(500);
    task = processor.GetTaskStatus(taskId);
}

if (task.Status == AnalysisTaskStatus.Completed && task.Result != null)
{
    Console.WriteLine($"Analysis complete. Found {task.Result.Issues.Count} issues.");
}
else if (task.Status == AnalysisTaskStatus.Failed)
{
    Console.WriteLine($"Analysis failed: {task.ErrorMessage}");
}

// Graceful shutdown
await processor.StopAsync();
```

### Example 2: Event-Driven Processing with Statistics
This example utilizes the `OnComplete` callback for immediate result handling and monitors queue statistics.

```csharp
using SqlQueryAnalyzer;

var processor = new AnalysisQueueProcessor();
processor.OnComplete = (result) => 
{
    // Global completion handler (if supported by implementation) or per-task
    Console.WriteLine($"Task {result.TaskId} finished with severity: {result.Severity}");
};

processor.Start();

// Submit multiple queries
var queries = new[] { "SELECT * FROM Orders", "UPDATE Logs SET Read=1" };
foreach (var q in queries)
{
    var id = processor.EnqueueAnalysis(q);
    var task = processor.GetTaskStatus(id);
    
    // Set specific callback for this task
    if (task != null)
    {
        task.OnComplete = (res) => 
        {
            Console.WriteLine($"Specific task {id} completed.");
        };
    }
}

// Monitor live statistics
var stats = processor.GetStatistics();
Console.WriteLine($"Queue Depth: {stats.QueuedCount}, Active: {stats.ActiveCount}, Total Done: {stats.TotalProcessed}");

await processor.StopAsync();
```

## Notes

*   **Thread Safety:** The `AnalysisQueueProcessor` is designed for concurrent access. Methods like `EnqueueAnalysis`, `GetTaskStatus`, and property accessors on the processor instance are thread-safe. However, modifications to individual `AnalysisTask` properties (such as assigning `OnComplete`) should ideally occur before the task transitions out of the `Queued` state to avoid race conditions where the task completes before the handler is attached.
*   **Lifecycle Constraints:** Calling `EnqueueAnalysis` after `StopAsync` has been invoked (or before `Start` is called) may result in exceptions or the task being silently dropped, depending on the internal state machine implementation. Always ensure the processor is in the `Started` state before submitting work.
*   **Resource Cleanup:** The `StopAsync` method is critical for application shutdown. It ensures that in-flight analysis operations are allowed to finish rather than being abruptly terminated, preventing resource leaks or partial data writes.
*   **Nullability:** Consumers must handle nullable return types carefully. `GetTaskStatus` returns `null` for unknown IDs. `StartedAt`, `CompletedAt`, `Result`, and `ErrorMessage` are conditional based on the `Status` enum; accessing `Result` on a failed task or `ErrorMessage` on a successful task will yield `null`.
*   **Elapsed Time Calculation:** The `GetElapsedTime` method on `AnalysisTask` performs dynamic calculation if the task is still running. Frequent polling of this property during active execution may incur minor overhead, though it is generally negligible.
