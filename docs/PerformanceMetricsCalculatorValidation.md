# PerformanceMetricsCalculatorValidation

Provides static validation methods for performance metrics calculation inputs and results. This type ensures that all parameters passed to the performance metrics pipeline are within acceptable ranges and that computed metrics meet expected quality thresholds before they are consumed by downstream analysis components.

## API

### Validate

```csharp
public static IReadOnlyList<string> Validate(QueryPlanAnalysisResult analysisResult)
public static IReadOnlyList<string> Validate(ExecutionStatistics statistics)
public static IReadOnlyList<string> Validate(IndexUsageMetrics indexMetrics)
public static IReadOnlyList<string> Validate(WaitStatistics waitStats)
public static IReadOnlyList<string> Validate(IOMetrics ioMetrics)
public static IReadOnlyList<string> Validate(CompilationMetrics compilationMetrics)
public static IReadOnlyList<string> Validate(RuntimeMetrics runtimeMetrics)
public static IReadOnlyList<string> Validate(MemoryGrantMetrics memoryMetrics)
public static IReadOnlyList<string> Validate(ParallelismMetrics parallelismMetrics)
```

Validates the specified performance metric object and returns a read-only list of validation error messages. Each overload targets a distinct metric type produced during query analysis.

**Parameters**
- `analysisResult` — The complete query plan analysis result to validate.
- `statistics` — Execution statistics including row counts, execution count, and timing data.
- `indexMetrics` — Index seek, scan, and lookup metrics extracted from the plan.
- `waitStats` — Wait type durations and counts observed during execution.
- `ioMetrics` — Logical and physical I/O measurements.
- `compilationMetrics` — Query compilation and optimization time data.
- `runtimeMetrics` — CPU time, elapsed time, and degree-of-parallelism data.
- `memoryMetrics` — Memory grant, spill, and workspace memory figures.
- `parallelism` — Parallelism-related metrics including DOP and thread distribution.

**Returns**  
`IReadOnlyList<string>` — An empty list if validation passes; otherwise, each entry describes a specific validation failure.

**Remarks**  
All overloads are thread-safe and do not modify the input objects. The returned list is a snapshot; subsequent calls with the same input produce identical results.

---

### IsValid

```csharp
public static bool IsValid(PerformancePlanAnalysisResult analysisResult)
public static bool IsValid(ExecutionStatistics statistics)
public static bool IsValid(IndexedMetrics indexMetrics)
public static bool IsValid(WaitStatistics waitStats)
public static bool IsValid(IOMetrics ioMetrics)
public static bool IsValid(CompilationMetrics compilationMetrics)
public static bool IsValid(RuntimeMetrics runtimeMetrics)
public static bool IsValid(MemoryMetrics memoryMetrics)
public static bool IsValid(ParallelismMetrics parallelism)
```

Performs a quick validity check on the specified metrics object without collecting individual error messages. Returns `true` when all validation rules pass; `false` otherwise.

**Parameters**  
Same set of metric types as the `Validate` overloads.

**Return**  
`bool` — `true` if the metrics are valid according to all applicable rules; `false` if any rule is violated.

**Remarks**  
These methods short-circuit on the first failure for efficiency. Use `Validate` when you need the full list of violations.

---

### EnsureValid

```csharp
public static void EnsureValid(PerformancePlanAnalysisResult analysisResult)
public static void EnsureValid(ExecutionStatistics statistics)
```

Performs validation and throws an exception if the metrics are invalid. These are guard methods intended for use at API boundaries where invalid data must halt processing immediately.

**Parameters**  
- `analysisResult` — The analysis result to validate.
- `statistics` — Execution statistics to validate.

**Exceptions**  
- `ValidationException` — Thrown when one or more validation rules fail. The exception message aggregates all failure descriptions.

**Remarks**  
These methods internally call the corresponding `Validate` overload and throw if the returned list is non-empty. They are safe to call from multiple threads concurrently.

## Usage

**Example 1: Collecting all validation errors for reporting**

```csharp
var ioMetrics = new IOMetrics
{
    LogicalReads = 150_000,
    PhysicalReads = 12_000,
    ReadAheadReads = 8_000
};

IReadOnlyList<string> errors = PerformanceMetricsCalculatorValidation.Validate(ioMetrics);

if (errors.Count > 0)
{
    foreach (string error in errors)
    {
        Console.WriteLine($"[IO Validation] {error}");
    }
}
```

**Example 2: Guard clause in a processing pipeline**

```csharp
public void ProcessQueryAnalysis(ExecutionStatistics statistics)
{
    // Halt immediately if statistics are invalid
    PerformanceMetricsCalculatorValidation.EnsureValid(statistics);

    // Proceed with processing
    var score = CalculatePerformanceScore(statistics);
    StoreResults(score);
}
```

## Notes

- All methods are static and stateless; no instance construction is required or possible.
- The `Validate` overloads return an empty list on success, never `null`. Callers can safely iterate over the result without null checks.
- `IsValid` is optimized for boolean checks and short-circuits on the first violation. Use it in hot paths where you only need a pass/fail decision.
- `EnsureValid` is intended for defensive programming at API boundaries. The thrown `ValidationException` includes all accumulated error messages, not just the first failure.
- Input objects are not mutated by any validation method. The same metrics instance can be validated multiple times safely.
- All public members are thread-safe. Multiple threads may call any combination of `Validate`, `IsValid`, and `EnsureValid` concurrently without external synchronization.
- When a metrics object contains multiple violations, `Validate` returns all of them in a single list. The order of messages is deterministic but not guaranteed to correspond to any particular field order.
