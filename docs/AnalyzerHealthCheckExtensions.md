# AnalyzerHealthCheckExtensions

## Purpose
Provides extension methods for the `AnalyzerHealthCheck` class, adding convenient helpers for evaluating health status, running comprehensive self-healing workflows, and producing human-readable status reports.

## Members

### IsHealthyAsync(analyzer)
Asynchronously checks whether the analyzer is currently healthy.

- **Parameters**:
  - `analyzer`: The `AnalyzerHealthCheck` instance to evaluate.
- **Returns**: `true` if the analyzer's status is `HealthStatus.Healthy`; otherwise, `false`.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `analyzer` is null.

### PerformComprehensiveHealAsync(analyzer)
Runs a full health check and, if the analyzer is not healthy, attempts self-healing.

- **Parameters**:
  - `analyzer`: The `AnalyzerHealthCheck` instance to evaluate.
- **Returns**: A `SelfHealResult` describing the healing attempt when the analyzer is not healthy; otherwise, `null` when the analyzer is already healthy.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `analyzer` is null.

### GetStatusReportAsync(analyzer)
Asynchronously produces a string report summarizing the analyzer's health status and component details.

- **Parameters**:
  - `analyzer`: The `AnalyzerHealthCheck` instance to evaluate.
- **Returns**: A string report containing the overall health status and per-component details.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `analyzer` is null.
  - `InvalidOperationException`: Thrown when `AnalyzerHealthCheck.CheckHealthAsync` returns null.

## Usage Example
```csharp
using System;
using System.Threading.Tasks;
using SqlQueryAnalyzer.Diagnostics;

public class HealthMonitor
{
    private readonly AnalyzerHealthCheck _healthCheck;

    public HealthMonitor(AnalyzerHealthCheck healthCheck)
    {
        _healthCheck = healthCheck;
    }

    public async Task MonitorAsync()
    {
        // Quick health check
        bool isHealthy = await _healthCheck.IsHealthyAsync();
        Console.WriteLine($"Analyzer healthy: {isHealthy}");

        // Comprehensive check with self-healing
        var healResult = await _healthCheck.PerformComprehensiveHealAsync();
        if (healResult != null)
        {
            Console.WriteLine($"Self-heal success: {healResult.Success}");
            foreach (var action in healResult.ActionsPerformed)
            {
                Console.WriteLine($"Action taken: {action}");
            }
        }

        // Full status report
        string report = await _healthCheck.GetStatusReportAsync();
        Console.WriteLine(report);
    }
}
```