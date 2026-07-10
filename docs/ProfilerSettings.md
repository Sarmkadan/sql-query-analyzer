# ProfilerSettings
The `ProfilerSettings` type in the `sql-query-analyzer` project provides a set of configuration options for profiling SQL queries. It allows users to customize the behavior of the profiler, such as capturing execution plans, timings, and resource usage, as well as setting thresholds for slow queries and high memory usage. This type is essential for fine-tuning the profiling process to meet specific requirements and optimize query performance.

## API
The `ProfilerSettings` type has the following public members:
* `CaptureExecutionPlanByDefault`: A boolean indicating whether to capture execution plans by default.
* `CaptureTimingsByDefault`: A boolean indicating whether to capture timings by default.
* `CaptureResourceUsageByDefault`: A boolean indicating whether to capture resource usage by default.
* `IncludePlanVisualizationByDefault`: A boolean indicating whether to include plan visualization by default.
* `DefaultMaxDurationMs`: An integer representing the default maximum duration in milliseconds.
* `MaxQueryLengthChars`: An integer representing the maximum query length in characters.
* `MaxBatchSize`: An integer representing the maximum batch size.
* `RegressionThreshold`: A double representing the regression threshold.
* `ImprovementThreshold`: A double representing the improvement threshold.
* `SlowStageThresholdMs`: A double representing the slow stage threshold in milliseconds.
* `HighMemoryThresholdBytes`: A long representing the high memory threshold in bytes.
* `Visualization`: A `VisualizationSettings` object representing the visualization settings.
* `ForDevelopment`: A static `ProfilerSettings` instance for development environments.
* `ForProduction`: A static `ProfilerSettings` instance for production environments.
* `Validate`: A list of strings representing validation settings.
* `MaxDepth`: An integer representing the maximum depth.
* `MaxNodes`: An integer representing the maximum number of nodes.
* `CostBarWidth`: An integer representing the cost bar width.
* `AnnotateBottlenecks`: A boolean indicating whether to annotate bottlenecks.
* `BottleneckCostThreshold`: A double representing the bottleneck cost threshold.

## Usage
Here are two examples of using the `ProfilerSettings` type in C#:
```csharp
// Example 1: Creating a custom ProfilerSettings instance
var settings = new ProfilerSettings
{
    CaptureExecutionPlanByDefault = true,
    CaptureTimingsByDefault = true,
    DefaultMaxDurationMs = 1000,
    MaxQueryLengthChars = 10000,
    Visualization = new VisualizationSettings { MaxDepth = 10 }
};

// Example 2: Using the ForDevelopment and ForProduction instances
var developmentSettings = ProfilerSettings.ForDevelopment;
var productionSettings = ProfilerSettings.ForProduction;

developmentSettings.CaptureResourceUsageByDefault = true;
productionSettings.SlowStageThresholdMs = 500;
```

## Notes
When using the `ProfilerSettings` type, consider the following edge cases and thread-safety remarks:
* The `ForDevelopment` and `ForProduction` instances are static and shared across the application. Modifying these instances can affect all parts of the application that use them.
* The `Validate` list and `Visualization` object are mutable. When sharing instances of `ProfilerSettings`, consider creating defensive copies to avoid unintended modifications.
* The `MaxDepth`, `MaxNodes`, and `CostBarWidth` properties control the visualization of query plans. Setting these properties too high can lead to performance issues or excessive memory usage.
* The `BottleneckCostThreshold` property controls the annotation of bottlenecks in query plans. Setting this property too low can lead to excessive annotation, while setting it too high can hide important performance issues.
