# ProfilerOptionsExtensions

Provides extension methods for `ProfilerOptions` to simplify common configuration checks and feature enumeration.

## Purpose

This static class contains extension methods that enhance the usability of the `ProfilerOptions` model by providing:
- A method to check if the profiler is configured for high-precision measurement
- A method to enumerate active profiling features based on boolean flags

## Members

### IsHighPrecision(ProfilerOptions options)

Determines if the profiler is configured for high-precision measurement.

**Parameters**
- `options`: The profiler options instance to evaluate.

**Returns**
- `true` if `WarmUpIterations` is greater than 0 and `MeasurementIterations` is greater than 1; otherwise, `false`.

**Exceptions**
- `ArgumentNullException`: Thrown when `options` is `null`.

**Example**
```csharp
var options = new ProfilerOptions { WarmUpIterations = 5, MeasurementIterations = 10 };
bool isHighPrecision = options.IsHighPrecision(); // Returns true
```

### GetActiveFeatures(ProfilerOptions options)

Returns an enumerable of active profiling feature names based on the boolean configuration flags.

**Parameters**
- `options`: The profiler options instance to evaluate.

**Returns**
- An `IEnumerable<string>` containing the names of enabled features. Each feature name corresponds to the property name of a boolean flag that is set to `true`.

**Exceptions**
- `ArgumentNullException`: Thrown when `options` is `null`.

**Example**
```csharp
var options = new ProfilerOptions 
{ 
    CaptureExecutionPlan = true,
    CaptureTimings = false,
    CaptureResourceUsage = true,
    IncludePlanVisualization = false 
};

IEnumerable<string> activeFeatures = options.GetActiveFeatures();
// Returns: ["CaptureExecutionPlan", "CaptureResourceUsage"]
```