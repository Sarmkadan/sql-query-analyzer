# BatchAnalysisProcessor

The `BatchAnalysisProcessor` class provides asynchronous facilities for analyzing collections of SQL queries, producing detailed analysis results for each query. It supports loading queries from files or delimited files, configuring parallelism, and reporting progress through read‑only properties.

## API

### AnalyzeBatchAsync
```csharp
public async Task<List<QueryAnalysisResult>> AnalyzeBatchAsync()
```
Analyzes the currently loaded batch of queries and returns a list of `QueryAnalysisResult` objects, one per query.  
- **Return value:** A `Task` that completes with a `List<QueryAnalysisResult>` containing the analysis outcome for each query.  
- **Exceptions:**  
  - `InvalidOperationException` if no query batch has been supplied prior to invocation.  
  - `ObjectDisposedException` if the processor has been disposed.  
  - Any exception thrown by the underlying analysis logic (e.g., parsing errors) is propagated.

### AnalyzeBatchFromFileAsync
```csharp
public async Task<List<QueryAnalysisResult>> AnalyzeBatchFromFileAsync()
```
Loads queries from a preset file location, analyzes them, and returns the results.  
- **Return value:** A `Task` that completes with a `List<QueryAnalysisResult>` for the queries read from the file.  
- **Exceptions:**  
  - `FileNotFoundException` if the designated file does not exist.  
  - `IOException` for general I/O failures (e.g., insufficient permissions, disk errors).  
  - `InvalidOperationException` if the file path has not been configured.  
  - Parsing or analysis exceptions are propagated as‑is.

### SetMaxParallel
```csharp
public void SetMaxParallel()
```
Configures the maximum degree of parallelism used by subsequent batch analysis operations.  
- **Return value:** None.  
- **Exceptions:**  
  - `ArgumentOutOfRangeException` if the internal parallelism setting would be invalid (e.g., less than 1).  
  - `InvalidOperationException` if called after an analysis operation has already started.

### AnalyzeBatchFromDelimitedFileAsync
```csharp
public async Task<List<QueryAnalysisResult>> AnalyzeBatchFromDelimitedFileAsync()
```
Reads queries from a preset delimited file (e.g., CSV or TSV), analyzes them, and returns the results.  
- **Return value:** A `Task` that completes with a `List<QueryAnalysisResult>` for the queries parsed from the delimited file.  
- **Exceptions:**  
  - `FileNotFoundException` if the delimited file cannot be located.  
  - `IOException` for I/O related problems.  
  - `InvalidOperationException` if the delimited file source has not been defined.  
  - `FormatException` if the file does not conform to the expected delimiter format.  
  - Any analysis‑specific exceptions are propagated.

### ProcessedCount
```csharp
public int ProcessedCount { get; }
```
Gets the number of queries that have been fully processed so far during the current analysis operation.  
- **Value range:** 0 to `TotalCount`. Updated incrementally as each query completes.

### TotalCount
```csharp
public int TotalCount { get; }
```
Gets the total number of queries in the batch being analyzed.  
- **Value:** Set when the batch is loaded and remains constant for the duration of the analysis.

### CurrentQueryIndex
```csharp
public int CurrentQueryIndex { get; }
```
Gets the zero‑based index of the query currently being processed.  
- **Value:** -1 when no analysis is active; otherwise ranges from 0 to `TotalCount‑1`.

### PercentComplete
```csharp
public double PercentComplete { get; }
```
Gets the completion percentage of the ongoing analysis operation.  
- **Value:** Calculated as `(ProcessedCount / (double)TotalCount) * 100`. Returns 0 when no analysis has started and 100 when all queries are processed.

### ToString
```csharp
public override string ToString()
```
Returns a string that represents the current state of the processor, typically including the batch size, processed count, and percent complete.  
- **Return value:** A human‑readable summary suitable for logging or debugging.

## Usage

### Example 1: Analyzing a batch from a text file
```csharp
using System;
using System.Threading.Tasks;
using SqlQueryAnalyzer; // namespace containing BatchAnalysisProcessor

class Program
{
    static async Task Main()
    {
        var processor = new BatchAnalysisProcessor();

        // Optionally adjust parallelism before starting
        processor.SetMaxParallel();

        try
        {
            var results = await processor.AnalyzeBatchFromFileAsync();
            Console.WriteLine($"Analysis complete. {results.Count} queries processed.");
            foreach (var res in results)
            {
                Console.WriteLine($"Query {res.QueryId}: {res.Status}");
            }
        }
        catch (FileNotFoundException ex)
        {
            Console.Error.WriteLine($"File not found: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Analysis failed: {ex.Message}");
        }
    }
}
```

### Example 2: Monitoring progress while analyzing a delimited file
```csharp
using System;
using System.Threading.Tasks;
using SqlQueryAnalyzer;

class Program
{
    static async Task Main()
    {
        var processor = new BatchAnalysisProcessor();
        processor.SetMaxParallel();

        // Start analysis
        var analysisTask = processor.AnalyzeBatchFromDelimitedFileAsync();

        // Simple progress reporting loop
        while (!analysisTask.IsCompleted)
        {
            Console.Write(
                $"\rProgress: {processor.PercentComplete:0.0}% " +
                $"({processor.ProcessedCount}/{processor.TotalCount})"
            );
            await Task.Delay(200);
        }

        await analysisTask; // ensure any exceptions are observed
        Console.WriteLine(); // newline after progress

        var results = await analysisTask;
        Console.WriteLine($"Finished with {results.Count} results.");
    }
}
```

## Notes
- The progress properties (`ProcessedCount`, `TotalCount`, `CurrentQueryIndex`, `PercentComplete`) are updated only while an analysis operation is active. Reading them before `Analyze*Async` is called or after it has completed yields stale values (typically zero for counts and false for completion).  
- `SetMaxParallel` must be invoked prior to starting any analysis; calling it after an analysis has begun throws an `InvalidOperationException`.  
- The class is not thread‑safe for concurrent invocation of the `Analyze*Async` methods. Simultaneous calls may result in undefined behavior or corrupted state. However, the read‑only progress properties may be safely queried from multiple threads while an analysis is running, as they are updated atomically.  
- If an analysis operation is cancelled externally (e.g., via a `CancellationToken` supplied internally), the operation throws an `OperationCanceledException` and the progress properties reflect the state at the point of cancellation.  
- The `ToString` override is intended for diagnostic purposes only; its exact format may change between versions and should not be parsed programmatically.
