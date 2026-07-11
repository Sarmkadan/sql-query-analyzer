# QueryAnalysisResultExtensions

Provides extension methods for `QueryAnalysisResult` that offer common operations such as performance assessment, severity classification, serialization, and data manipulation.

## API

### `IsHighPerformance(QueryAnalysisResult result)`

Determines whether the analyzed query is considered high performance based on internal heuristics.

- **Parameters**
  - `result` – The `QueryAnalysisResult` instance to evaluate.
- **Return value**
  - `true` if the query meets high-performance criteria; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `result` is `null`.

### `NeedsOptimization(QueryAnalysisResult result)`

Indicates whether the analyzed query requires optimization due to detected inefficiencies.

- **Parameters**
  - `result` – The `QueryAnalysisResult` instance to evaluate.
- **Return value**
  - `true` if optimization is recommended; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `result` is `null`.

### `GetSeverityLevel(QueryAnalysisResult result)`

Returns a human-readable severity level describing the query’s performance impact.

- **Parameters**
  - `result` – The `QueryAnalysisResult` instance to evaluate.
- **Return value**
  - A string representing the severity level (e.g., "High", "Medium", "Low").
- **Exceptions**
  - Throws `ArgumentNullException` if `result` is `null`.

### `DeepCopy(QueryAnalysisResult result)`

Creates a deep copy of the `QueryAnalysisResult` instance, including all nested data.

- **Parameters**
  - `result` – The `QueryAnalysisResult` instance to copy.
- **Return value**
  - A new `QueryAnalysisResult` instance with identical values.
- **Exceptions**
  - Throws `ArgumentNullException` if `result` is `null`.

### `FormatSummary(QueryAnalysisResult result)`

Generates a formatted summary string describing key aspects of the query analysis.

- **Parameters**
  - `result` – The `QueryAnalysisResult` instance to summarize.
- **Return value**
  - A formatted string containing analysis highlights.
- **Exceptions**
  - Throws `ArgumentNullException` if `result` is `null`.

### `ToJsonString(QueryAnalysisResult result)`

Serializes the `QueryAnalysisResult` instance to a JSON-formatted string.

- **Parameters**
  - `result` – The `QueryAnalysisResult` instance to serialize.
- **Return value**
  - A JSON string representation of the object.
- **Exceptions**
  - Throws `ArgumentNullException` if `result` is `null`.

## Usage

```csharp
// Example 1: Evaluate query performance and log severity
var analysis = new QueryAnalysisResult
{
    ExecutionTimeMs = 150,
    CpuUsage = 85,
    Reads = 2500
};

if (!QueryAnalysisResultExtensions.IsHighPerformance(analysis))
{
    var severity = QueryAnalysisResultExtensions.GetSeverityLevel(analysis);
    Console.WriteLine($"Query requires attention. Severity: {severity}");
}

// Example 2: Serialize analysis to JSON for storage
var copy = QueryAnalysisResultExtensions.DeepCopy(analysis);
var json = QueryAnalysisResultExtensions.ToJsonString(copy);
File.WriteAllText("analysis.json", json);
```

## Notes

- All methods are thread-safe with respect to their input parameters; however, the returned objects (e.g., from `DeepCopy`) are not inherently thread-safe unless explicitly synchronized by the caller.
- Passing `null` to any method results in an immediate `ArgumentNullException`; no defensive checks are performed beyond this.
- The severity level returned by `GetSeverityLevel` is derived from thresholds that may change between versions; treat the value as advisory only.
