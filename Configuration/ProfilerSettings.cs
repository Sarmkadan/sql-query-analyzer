#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;

namespace SqlQueryAnalyzer.Configuration;

/// <summary>
/// Configuration for the query profiler subsystem.
/// Can be used standalone when calling <c>AddQueryProfiler</c> or nested inside
/// <see cref="AnalyzerSettings"/> as a <c>Profiler</c> section in appsettings.json.
/// </summary>
public class ProfilerSettings
{
    // ── Default capture flags ────────────────────────────────────────────────

    /// <summary>
    /// When <c>true</c>, the profiler captures and embeds the execution plan in every report
    /// unless the caller explicitly overrides this via <c>ProfilerOptions</c>.
    /// </summary>
    public bool CaptureExecutionPlanByDefault { get; set; } = true;

    /// <summary>
    /// When <c>true</c>, wall-clock timings are recorded for each pipeline stage by default.
    /// </summary>
    public bool CaptureTimingsByDefault { get; set; } = true;

    /// <summary>
    /// When <c>true</c>, a process resource-usage snapshot is captured after each run by default.
    /// </summary>
    public bool CaptureResourceUsageByDefault { get; set; } = true;

    /// <summary>
    /// When <c>true</c>, an ASCII visualization of the plan tree is embedded in reports by default.
    /// Disable in high-throughput environments to reduce allocation pressure.
    /// </summary>
    public bool IncludePlanVisualizationByDefault { get; set; } = true;

    // ── Limits ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Maximum profiling wall-clock budget before the run is aborted, in milliseconds.
    /// Individual calls may override this via <c>ProfilerOptions.MaxDurationMs</c>.
    /// </summary>
    public int DefaultMaxDurationMs { get; set; } = 30_000;

    /// <summary>
    /// Maximum SQL text length the profiler will accept, in characters.
    /// Queries exceeding this limit are rejected with an <see cref="System.ArgumentException"/>.
    /// </summary>
    public int MaxQueryLengthChars { get; set; } = 1_048_576; // 1 MB

    /// <summary>
    /// Maximum number of queries accepted in a single <c>ProfileBatchAsync</c> call.
    /// Callers should split larger batches and submit them in chunks.
    /// </summary>
    public int MaxBatchSize { get; set; } = 100;

    // ── Comparison thresholds ─────────────────────────────────────────────────

    /// <summary>
    /// Minimum score decrease (candidate − baseline) required to flag a regression.
    /// A value of 5.0 means the candidate must score at least 5 points lower to be considered a regression.
    /// </summary>
    public double RegressionThreshold { get; set; } = 5.0;

    /// <summary>
    /// Minimum score increase (candidate − baseline) required to flag an improvement.
    /// A value of 3.0 means the candidate must score at least 3 points higher to be flagged.
    /// </summary>
    public double ImprovementThreshold { get; set; } = 3.0;

    /// <summary>
    /// Stage duration above which the profiler emits a slow-stage suggestion, in milliseconds.
    /// </summary>
    public double SlowStageThresholdMs { get; set; } = 100.0;

    /// <summary>
    /// Process working-set size above which the profiler emits a high-memory suggestion, in bytes.
    /// Defaults to 500 MB.
    /// </summary>
    public long HighMemoryThresholdBytes { get; set; } = 500L * 1024 * 1024;

    // ── Visualization ────────────────────────────────────────────────────────

    /// <summary>Settings forwarded to the <c>ExecutionPlanVisualizer</c> renderer.</summary>
    public VisualizationSettings Visualization { get; set; } = new();

    // ── Factory helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Creates a fully featured configuration suitable for development and debugging.
    /// Enables all capture options and uses detailed visualization settings.
    /// </summary>
    public static ProfilerSettings ForDevelopment() => new()
    {
        CaptureExecutionPlanByDefault = true,
        CaptureTimingsByDefault = true,
        CaptureResourceUsageByDefault = true,
        IncludePlanVisualizationByDefault = true,
        DefaultMaxDurationMs = 60_000,
        SlowStageThresholdMs = 50.0,
        Visualization = VisualizationSettings.Detailed()
    };

    /// <summary>
    /// Creates a low-overhead configuration for production environments.
    /// Plan capture and visualization are disabled by default; individual calls may opt in
    /// via <c>ProfilerOptions</c> for targeted on-demand profiling.
    /// </summary>
    public static ProfilerSettings ForProduction() => new()
    {
        CaptureExecutionPlanByDefault = false,
        CaptureResourceUsageByDefault = false,
        IncludePlanVisualizationByDefault = false,
        DefaultMaxDurationMs = 10_000,
        SlowStageThresholdMs = 200.0,
        Visualization = VisualizationSettings.Compact()
    };

    /// <summary>
    /// Validates these settings and returns a list of descriptive error messages.
    /// An empty list means the configuration is valid.
    /// </summary>
    public List<string> Validate()
    {
        var errors = new List<string>();

        if (DefaultMaxDurationMs < 100)
            errors.Add($"{nameof(DefaultMaxDurationMs)} must be at least 100 ms.");

        if (MaxBatchSize < 1)
            errors.Add($"{nameof(MaxBatchSize)} must be at least 1.");

        if (MaxQueryLengthChars < 1)
            errors.Add($"{nameof(MaxQueryLengthChars)} must be at least 1.");

        if (RegressionThreshold < 0)
            errors.Add($"{nameof(RegressionThreshold)} must be non-negative.");

        if (ImprovementThreshold < 0)
            errors.Add($"{nameof(ImprovementThreshold)} must be non-negative.");

        errors.AddRange(Visualization.Validate());
        return errors;
    }

    /// <summary>
    /// Returns a concise, informative string representation of the current settings.
    /// </summary>
    public override string ToString() => $"ProfilerSettings {{ CaptureExecutionPlanByDefault = {CaptureExecutionPlanByDefault}, CaptureTimingsByDefault = {CaptureTimingsByDefault}, CaptureResourceUsageByDefault = {CaptureResourceUsageByDefault}, IncludePlanVisualizationByDefault = {IncludePlanVisualizationByDefault}, DefaultMaxDurationMs = {DefaultMaxDurationMs}, MaxQueryLengthChars = {MaxQueryLengthChars} }}";
}

/// <summary>
/// Controls the rendering behaviour of the execution plan visualizer.
/// Used by <see cref="ProfilerSettings.Visualization"/> and forwarded to the renderer at construction time.
/// </summary>
public class VisualizationSettings
{
    /// <summary>
    /// Maximum tree depth to render. Nodes deeper than this value are omitted from the output.
    /// </summary>
    public int MaxDepth { get; set; } = 10;

    /// <summary>
    /// Maximum number of plan nodes to include in the rendered tree.
    /// A truncation notice is appended when this limit is exceeded.
    /// </summary>
    public int MaxNodes { get; set; } = 200;

    /// <summary>
    /// Character width of the relative-cost bar drawn alongside each node.
    /// Wider bars improve readability but increase line length.
    /// </summary>
    public int CostBarWidth { get; set; } = 20;

    /// <summary>
    /// When <c>true</c>, nodes whose estimated cost exceeds <see cref="BottleneckCostThreshold"/>
    /// are annotated with a "◄ BOTTLENECK" marker.
    /// </summary>
    public bool AnnotateBottlenecks { get; set; } = true;

    /// <summary>
    /// Estimated cost above which a plan node is considered a bottleneck.
    /// The unit matches the query optimizer's internal cost model.
    /// </summary>
    public double BottleneckCostThreshold { get; set; } = 5.0;

    /// <summary>When <c>true</c>, estimated row counts are shown next to each node label.</summary>
    public bool ShowRowCounts { get; set; } = true;

    /// <summary>
    /// When <c>true</c>, individual I/O and CPU cost components are shown per node.
    /// When <c>false</c>, only the total estimated cost is displayed.
    /// </summary>
    public bool ShowDetailedCosts { get; set; } = false;

    /// <summary>Creates verbose settings appropriate for development-time plan inspection.</summary>
    public static VisualizationSettings Detailed() => new()
    {
        MaxDepth = 20,
        MaxNodes = 500,
        CostBarWidth = 30,
        AnnotateBottlenecks = true,
        ShowRowCounts = true,
        ShowDetailedCosts = true,
        BottleneckCostThreshold = 2.0
    };

    /// <summary>Creates minimal settings appropriate for high-throughput production reporting.</summary>
    public static VisualizationSettings Compact() => new()
    {
        MaxDepth = 5,
        MaxNodes = 50,
        CostBarWidth = 10,
        AnnotateBottlenecks = true,
        ShowRowCounts = false,
        ShowDetailedCosts = false,
        BottleneckCostThreshold = 10.0
    };

    /// <summary>
    /// Validates these visualization settings.
    /// Returns descriptive error messages; an empty list means the settings are valid.
    /// </summary>
    internal List<string> Validate()
    {
        var errors = new List<string>();

        if (MaxDepth < 1)
            errors.Add($"Visualization.{nameof(MaxDepth)} must be at least 1.");

        if (MaxNodes < 1)
            errors.Add($"Visualization.{nameof(MaxNodes)} must be at least 1.");

        if (CostBarWidth < 5)
            errors.Add($"Visualization.{nameof(CostBarWidth)} must be at least 5 characters.");

        if (BottleneckCostThreshold < 0)
            errors.Add($"Visualization.{nameof(BottleneckCostThreshold)} must be non-negative.");

        return errors;
    }
}
