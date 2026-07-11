# AnalyzerHealthCheck

The `AnalyzerHealthCheck` class serves as the primary diagnostic and recovery interface for the SQL Query Analyzer service, encapsulating the current operational state of critical subsystems including caching, rate limiting, metrics collection, and database connectivity. It provides mechanisms to asynchronously evaluate system health, aggregate component-specific statuses into a unified report, and execute automated self-healing procedures when degradations are detected, ensuring high availability and immediate visibility into service integrity.

## API

### Constructors

#### `public AnalyzerHealthCheck()`
Initializes a new instance of the `AnalyzerHealthCheck` class. This constructor sets up the internal state required to track component health and prepares the instance for execution of health checks or self-healing routines.

### Methods

#### `public async Task<HealthCheckResult> CheckHealthAsync()`
Asynchronously evaluates the health status of all dependent components (Cache, RateLimiter, Metrics, Database) and aggregates the results.
*   **Return Value**: Returns a `HealthCheckResult` object containing the overall `Status`, a summary `Message`, a boolean `Success` indicator, and a list of specific `Errors` encountered during the evaluation.
*   **Exceptions**: May throw exceptions if the underlying diagnostic probes for any component fail catastrophically or if the execution context is cancelled.

#### `public async Task<SelfHealResult> AttemptSelfHealAsync()`
Attempts to automatically resolve identified health issues based on the current state of the system. This method analyzes failed components and executes predefined recovery actions.
*   **Return Value**: Returns a `SelfHealResult` object indicating whether the operation was successful (`Success`), detailing the `ActionsPerformed` during the attempt, and providing an `Error` message if the healing process itself failed.
*   **Exceptions**: May throw exceptions if the recovery logic encounters an unrecoverable state or if external resources required for healing are unavailable.

#### `public override string ToString()`
Returns a string representation of the current health check instance, typically summarizing the overall status and the timestamp of the check.

### Properties

#### `public DateTime CheckTime`
Gets the precise date and time when the health check was last executed or updated.

#### `public HealthStatus Status`
Gets the aggregated health status of the analyzer (e.g., Healthy, Degraded, Unhealthy). Note: This property appears in both the main class context and the result object context; in the main class, it reflects the current live state.

#### `public ComponentHealth CacheHealth`
Gets the specific health status and details for the caching subsystem.

#### `public ComponentHealth RateLimiterHealth`
Gets the specific health status and details for the rate-limiting subsystem.

#### `public ComponentHealth MetricsHealth`
Gets the specific health status and details for the metrics collection subsystem.

#### `public ComponentHealth DatabaseHealth`
Gets the specific health status and details for the database connectivity subsystem.

#### `public List<string> Errors`
Gets a collection of error messages describing failures detected during the most recent health check.

#### `public string Component`
Gets the name or identifier of the specific component associated with this health record (relevant when viewing individual component results within the aggregate).

#### `public string Message`
Gets a human-readable description of the current health state or the result of the last operation.

#### `public bool Success`
Gets a boolean value indicating whether the last health check or self-heal operation completed successfully without critical failures.

#### `public List<string> ActionsPerformed`
Gets a list of descriptive strings detailing the specific recovery steps taken during the last `AttemptSelfHealAsync` execution.

#### `public string? Error`
Gets a single string containing the primary error message if the last operation failed, or `null` if the operation was successful.

## Usage

### Example 1: Performing a Routine Health Check
This example demonstrates how to instantiate the checker, run an asynchronous health evaluation, and handle the resulting status and error collection.

```csharp
using System;
using System.Threading.Tasks;
using SqlQueryAnalyzer.Diagnostics;

public class MonitoringService
{
    public async Task MonitorAnalyzerStatus()
    {
        var healthCheck = new AnalyzerHealthCheck();
        
        // Execute the health check
        var result = await healthCheck.CheckHealthAsync();

        Console.WriteLine($"Check Time: {healthCheck.CheckTime}");
        Console.WriteLine($"Overall Status: {result.Status}");
        Console.WriteLine($"Success: {result.Success}");

        if (!result.Success)
        {
            Console.WriteLine("Detected issues:");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"- {error}");
            }

            // Inspect specific subsystems
            if (healthCheck.DatabaseHealth.Status != HealthStatus.Healthy)
            {
                Console.WriteLine("Database subsystem is degraded.");
            }
        }
    }
}
```

### Example 2: Automated Self-Healing Workflow
This example illustrates a workflow where a failed health check triggers an automatic attempt to repair the system, logging the actions taken.

```csharp
using System;
using System.Threading.Tasks;
using SqlQueryAnalyzer.Diagnostics;

public class RecoveryOrchestrator
{
    public async Task RunRecoveryIfNeeded()
    {
        var healthCheck = new AnalyzerHealthCheck();
        var initialResult = await healthCheck.CheckHealthAsync();

        if (!initialResult.Success)
        {
            Console.WriteLine("Health check failed. Attempting self-heal...");
            
            try 
            {
                var healResult = await healthCheck.AttemptSelfHealAsync();

                if (healResult.Success)
                {
                    Console.WriteLine("Self-heal completed successfully.");
                    foreach (var action in healResult.ActionsPerformed)
                    {
                        Console.WriteLine($"Action taken: {action}");
                    }
                }
                else
                {
                    Console.WriteLine($"Self-heal failed: {healResult.Error}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Critical failure during self-heal: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("System is healthy; no action required.");
        }
    }
}
```

## Notes

*   **Thread Safety**: The `AnalyzerHealthCheck` instance methods (`CheckHealthAsync`, `AttemptSelfHealAsync`) are designed for asynchronous execution. While the `async` pattern suggests non-blocking behavior, the class does not explicitly guarantee thread safety for concurrent calls on the same instance. It is recommended to treat instances as scoped per-request or ensure external synchronization if multiple coroutines access the same instance simultaneously to update state properties like `Errors` or `Status`.
*   **State Mutability**: Properties such as `CheckTime`, `Status`, `Errors`, and `ActionsPerformed` are mutable and reflect the state of the *last* executed operation. Accessing these properties immediately after construction but before calling `CheckHealthAsync` or `AttemptSelfHealAsync` may yield default or uninitialized values.
*   **Component Dependencies**: The `ComponentHealth` properties (`CacheHealth`, `DatabaseHealth`, etc.) are populated during the execution of `CheckHealthAsync`. If a specific subsystem is not configured or is optional, its health status should be verified before assuming a failure condition based solely on the aggregate `Status`.
*   **Self-Heal Idempotency**: The `AttemptSelfHealAsync` method should be considered potentially side-effecting. Repeated invocations without an intervening state change may result in redundant `ActionsPerformed` entries or unnecessary resource consumption, depending on the internal implementation of the healing strategies.
