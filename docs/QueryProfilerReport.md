# QueryProfilerReport

Models defined in `Models/QueryProfilerReport.cs`. The file holds the profiler's per-run configuration (`ProfilerOptions`), the report it produces (`QueryProfilerReport`), and the supporting types that the report aggregates: stages, metrics, resource snapshot, suggestions, and before/after comparison.

This page documents `ProfilerOptions`, the input side of a profiling run. The report types are listed at the end for orientation.

## ProfilerOptions

Per-invocation configuration for a profiler run. It is passed as the optional `ProfilerOptions? options` argument of `ProfileQueryAsync` in `Services/QueryProfilerService.cs`. When no instance is supplied, the service builds one from `ProfilerSettings` defaults.

`ProfilerOptions` is a `sealed` class with no base type and no `ToString()` override, so it prints as its type name. To inspect a configuration, read the properties directly or use `GetActiveFeatures()` / `Validate()` (see [Extension and validation helpers](#extension-and-validation-helpers)).

### Properties

| Property | Type | Default | Purpose |
|---|---|---|---|
| `CaptureExecutionPlan` | `bool` | `true` | Capture the execution plan and embed it in the report. |
| `CaptureTimings` | `bool` | `true` | Measure wall-clock time for each pipeline stage. |
| `CaptureResourceUsage` | `bool` | `true` | Take a CPU and memory snapshot during the run. |
| `MaxDurationMs` | `int` | `30_000` | Wall-clock budget for the run, in milliseconds. |
| `WarmUpIterations` | `int` | `0` | Iterations executed before metrics are collected. |
| `MeasurementIterations` | `int` | `1` | Iterations whose results are averaged. |
| `IncludePlanVisualization` | `bool` | `true` | Include an ASCII tree of the execution plan in the report. |

#### `CaptureExecutionPlan`
When `true`, the execution plan is captured and, if it is available and parseable, stored in `QueryProfilerReport.ExecutionPlan`. The service reads this flag only when the analysis result carries an execution plan.

#### `CaptureTimings`
When `true`, each pipeline stage is timed and recorded in `QueryProfilerReport.ExecutionStages`. `TotalProfilingDurationMs` is always reported on the report; this flag controls the per-stage timings.

#### `CaptureResourceUsage`
When `true`, a `ResourceUsage` snapshot is stored in `QueryProfilerReport.ResourceUsage` after the analysis run.

#### `MaxDurationMs`
Intended wall-clock budget for a profiling run, in milliseconds. Valid values are `1` to `3_600_000` (one hour). The default is seeded from `ProfilerSettings.DefaultMaxDurationMs` when the service creates the options. `Validate()` rejects values outside this range.

Note: `ProfileQueryAsync` does not currently read this value, so it does not abort a run that exceeds it.

#### `WarmUpIterations`
Number of unmeasured executions run before metrics are collected, to reduce cold-start effects. Must be `0` or greater. When `0`, no warm-up runs.

#### `MeasurementIterations`
Number of measured iterations intended for metric averaging. Valid values are `1` to `1_000`. Higher values reduce noise and increase total run time.

Note: `ProfileQueryAsync` does not currently read this value.

#### `IncludePlanVisualization`
When `true`, a text rendering of the plan is stored in `QueryProfilerReport.PlanVisualization`. This only takes effect when `CaptureExecutionPlan` is also `true` and a plan was captured.

### Defaults and sources

- A new `ProfilerOptions` instance has the defaults in the table above.
- The service's private `BuildDefaultOptions()` copies `CaptureExecutionPlan`, `CaptureTimings`, `CaptureResourceUsage`, `MaxDurationMs`, and `IncludePlanVisualization` from `ProfilerSettings`. `WarmUpIterations` and `MeasurementIterations` are not set there and keep the class defaults.
- A caller-supplied instance is used as-is, so its values replace the settings defaults entirely.

## Extension and validation helpers

These live in other files but operate on `ProfilerOptions`:

- `ProfilerOptionsExtensions.IsHighPrecision(this ProfilerOptions)`: `true` when `WarmUpIterations > 0` and `MeasurementIterations > 1`.
- `ProfilerOptionsExtensions.GetActiveFeatures(this ProfilerOptions)`: yields the names of the enabled `Capture*` and `IncludePlanVisualization` flags.
- `ProfilerOptionsValidation.Validate(this ProfilerOptions)`: returns a list of problem messages (empty when valid). Checks `MaxDurationMs`, `WarmUpIterations`, and `MeasurementIterations` against the ranges above.
- `ProfilerOptionsValidation.IsValid(this ProfilerOptions)`: `true` when `Validate()` returns no errors.
- `ProfilerOptionsValidation.EnsureValid(this ProfilerOptions)`: throws `ArgumentException` listing all problems.

## Related types in the same file

- `QueryProfilerReport`: the complete result of a profiling run. It carries `QueryId`, `QueryText`, `ProfiledAt`, the `Options` used, `PerformanceScore` (0–100), `TotalProfilingDurationMs`, error fields (`HasError`, `ErrorMessage`), sub-reports (`AnalysisResult`, `ExecutionPlan`, `PlanVisualization`), and collections (`ExecutionStages`, `ResourceUsage`, `Metrics`, `Suggestions`). Its `GetSummary()` and `ToMetricsDictionary()` methods produce log and export views.
- `ExecutionStage`: a timed pipeline stage. `ToString()` returns `Name: 12.3ms` or `Name: ERROR (message)`.
- `ProfilerMetric`: a scalar metric with `Name`, `Value`, `Unit`, and `MetricCategory`. `ToString()` returns `Name=Value Unit`.
- `MetricCategory`: `Timing`, `Quality`, `Resource`, `Plan`.
- `ResourceUsage`: working-set and CPU snapshot. `WorkingSetMb` converts bytes to megabytes.
- `ProfilerSuggestion`: a ranked recommendation with `Severity`, `Category`, `Priority`, and `EstimatedImpactPercent`. `ToString()` returns `[P1][Critical][IndexOptimization] Title (~40% impact)`.
- `SuggestionSeverity`: `Info`, `Warning`, `Critical`. `SuggestionCategory`: `QueryStructure`, `IndexOptimization`, `ExecutionPlan`, `Runtime`, `ResourceUsage`, `Schema`.
- `ProfileComparison` and `MetricDelta`: baseline versus candidate comparison. `MetricDelta.ToString()` formats the change as `Name: base → candidate (+x.x%) unit`.

## Usage

```csharp
var options = new ProfilerOptions
{
    CaptureExecutionPlan = true,
    CaptureTimings = true,
    CaptureResourceUsage = false,
    MaxDurationMs = 10_000,
    WarmUpIterations = 2,
    MeasurementIterations = 5,
    IncludePlanVisualization = false
};

options.EnsureValid();

// Pass to the profiler service (IQueryProfilerService)
var report = await profiler.ProfileQueryAsync(queryText, options);
```
