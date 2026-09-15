# ProfilerSettingsValidation

The `ProfilerSettingsValidation` class provides static methods for validating the configuration of a SQL query profiler. It exposes three overloads of the `Validate` method, each targeting a different aspect of the settings (profiler settings and visualization settings), a quick `IsValid` check, and a throwing `EnsureValid` method. All validation logic is centralized here to ensure that profiler settings are consistent and complete before execution.

## API

### `Validate` (3 overloads)

Each overload returns an `IReadOnlyList<string>` containing zero or more error messages. An empty list indicates that the corresponding settings are valid. The three overloads cover the following areas:

| Overload | Description |
|----------|-------------|
| 1 | Validates the core profiler settings (limits, thresholds, capture flags). |
| 2 | Validates the visualization settings (depth, node limits, cost bar width). |
| 3 | Validates the profiler settings including nested visualization settings. |

All overloads are static and thread‑safe with respect to the settings objects passed in (provided those objects are not mutated concurrently).

### `IsValid`

`public static bool IsValid`

Returns `true` if the last call to any `Validate` overload (on the same thread) returned an empty list; otherwise `false`. The value is reset to `false` when a new validation is performed. This property is not thread‑safe and should be used only from a single thread after a validation call.

### `EnsureValid`

`public static void EnsureValid`

Throws an `ArgumentException` if the last validation (on the current thread) produced any error messages. The exception message contains the concatenated error list. If no validation has been performed yet, it throws with a message indicating that validation has not been run. This method is not thread‑safe and should be called immediately after a `Validate` call on the same thread.

## Usage

### Example 1: Validating and checking settings before profiling

```csharp
using SqlQueryAnalyzer.Configuration;

var settings = new ProfilerSettings
{
    DefaultMaxDurationMs = 30_000,
    MaxBatchSize = 100,
    MaxQueryLengthChars = 1_048_576,
    RegressionThreshold = 5.0,
    ImprovementThreshold = 3.0,
    SlowStageThresholdMs = 100.0,
    HighMemoryThresholdBytes = 500L * 1024 * 1024,
    Visualization = new VisualizationSettings
    {
        MaxDepth = 10,
        MaxNodes = 200,
        CostBarWidth = 20,
        BottleneckCostThreshold = 5.0
    }
};

// Validate all relevant aspects
var errors = settings.Validate();
if (errors.Count > 0)
{
    Console.WriteLine("Settings are invalid:");
    foreach (var error in errors)
        Console.WriteLine($"  - {error}");
    return;
}

// Quick check (same result as above)
bool ok = settings.IsValid();
Console.WriteLine($"Settings valid: {ok}");

// Proceed with profiling
var profiler = new QueryProfiler(settings);
var result = profiler.ProfileBatch("SELECT * FROM Users");
```

### Example 2: Using EnsureValid to fail fast

```csharp
using SqlQueryAnalyzer.Configuration;

var settings = new ProfilerSettings
{
    DefaultMaxDurationMs = 50,  // too low
    MaxBatchSize = 0,           // invalid
    MaxQueryLengthChars = 1_048_576,
    RegressionThreshold = -1.0, // negative
    ImprovementThreshold = 3.0,
    SlowStageThresholdMs = 100.0,
    HighMemoryThresholdBytes = 500L * 1024 * 1024,
    Visualization = new VisualizationSettings
    {
        MaxDepth = 0,           // too low
        MaxNodes = 200,
        CostBarWidth = 20,
        BottleneckCostThreshold = 5.0
    }
};

// Validate and immediately throw if anything is wrong
settings.Validate();
settings.EnsureValid();  // throws ArgumentException

// This line is never reached
var profiler = new QueryProfiler(settings);
```

## Notes

- **Null arguments:** All `Validate` overloads throw `ArgumentNullException` if the provided settings object is `null`.
- **Empty settings:** An empty or default settings object may produce multiple validation errors (e.g., invalid limits, negative thresholds).
- **Thread safety:** The `Validate` methods are safe to call concurrently on different settings instances. However, `IsValid` and `EnsureValid` rely on thread‑local state and are not safe for concurrent use. Always call them on the same thread that performed the validation.
- **Validation granularity:** Use the appropriate overload to validate only the settings you intend to change. Over‑validation (e.g., calling all three overloads) is harmless but may produce duplicate errors.
- **Error messages:** All error messages are human‑readable and include the name of the invalid setting and the reason (e.g., "DefaultMaxDurationMs must be at least 100 ms.").