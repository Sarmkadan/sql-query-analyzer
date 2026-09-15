# ProfilerSettingsExtensions

## Purpose
Provides extension methods for the `ProfilerSettings` class, enabling validation, configuration checks, and diagnostic information retrieval.

## Members

### ValidateOrThrow(settings)
Validates the settings and throws if any errors are found.

- **Parameters**:
  - `settings`: The settings to validate.
- **Returns**: None.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `settings` is null.
  - `InvalidOperationException`: Thrown if validation fails with a message containing all validation errors.

### GetHighMemoryThresholdMb(settings)
Gets the high memory threshold in megabytes.

- **Parameters**:
  - `settings`: The settings to query.
- **Returns**: The high memory threshold in MB.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `settings` is null.

### IsProductionConfig(settings)
Determines if the settings are configured for low-overhead production usage.

- **Parameters**:
  - `settings`: The settings to check.
- **Returns**: <see langword="true"/> if configured for production; otherwise, <see langword="false"/>.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `settings` is null.

### ToDiagnosticString(settings)
Generates a brief diagnostic summary string of the settings.

- **Parameters**:
  - `settings`: The settings to summarize.
- **Returns**: A culture-invariant string summary containing max duration, batch size, and production mode flag.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `settings` is null.

## Usage Example
```csharp
using SqlQueryAnalyzer.Configuration;

// Create profiler settings
var settings = new ProfilerSettings
{
    DefaultMaxDurationMs = 5000,
    MaxBatchSize = 1000,
    HighMemoryThresholdBytes = 100 * 1024 * 1024, // 100 MB
    CaptureExecutionPlanByDefault = false,
    CaptureResourceUsageByDefault = false,
    IncludePlanVisualizationByDefault = false
};

// Validate settings (throws if invalid)
settings.ValidateOrThrow();

// Get high memory threshold in MB
double highMemoryThresholdMb = settings.GetHighMemoryThresholdMb();
Console.WriteLine($"High memory threshold: {highMemoryThresholdMb} MB");

// Check if configured for production
bool isProduction = settings.IsProductionConfig();
Console.WriteLine($"Is production config: {isProduction}");

// Get diagnostic string
string diagnostic = settings.ToDiagnosticString();
Console.WriteLine(diagnostic);
```