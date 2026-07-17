# ErrorHandlingMiddlewareValidation

The `ErrorHandlingMiddlewareValidation` class provides a static utility API for validating configuration or state related to error handling middleware within the SQL Query Analyzer pipeline. It exposes a consistent pattern of validation methods that allow callers to retrieve a list of validation errors, check a boolean validity flag, or enforce validity by throwing an exception if the current state is invalid. This type is designed to be used during the initialization or configuration phases of the analysis pipeline to ensure that error handling components are correctly set up before execution begins.

## API

### `Validate`
```csharp
public static IReadOnlyList<string> Validate()
```
Executes the validation logic against the current error handling middleware configuration.
*   **Purpose**: To retrieve a comprehensive list of validation error messages.
*   **Return Value**: An `IReadOnlyList<string>` containing human-readable error descriptions. If the configuration is valid, the list is empty.
*   **Throws**: This method does not throw exceptions for validation failures; it returns them as strings.

### `IsValid`
```csharp
public static bool IsValid()
```
Checks whether the current error handling middleware configuration meets all required criteria.
*   **Purpose**: To perform a quick boolean check without generating error message strings.
*   **Return Value**: `true` if the configuration is valid; `false` otherwise.
*   **Throws**: This method does not throw exceptions for validation failures.

### `EnsureValid`
```csharp
public static void EnsureValid()
```
Validates the current configuration and enforces correctness by throwing an exception if invalid.
*   **Purpose**: To halt execution immediately if the middleware configuration is incorrect, typically used at startup boundaries.
*   **Return Value**: `void`. Returns normally only if the configuration is valid.
*   **Throws**: Throws an exception (typically `InvalidOperationException` or a custom validation exception) if the configuration is invalid. The exception message usually aggregates the errors found during validation.

## Usage

### Example 1: Pre-flight Check with Detailed Error Reporting
This example demonstrates how to use `Validate` to collect all potential configuration issues before starting the analysis pipeline, allowing for a user-friendly error report.

```csharp
using System;
using System.Linq;
using SqlQueryAnalyzer.Validation;

public class StartupValidator
{
    public void ConfigureMiddleware()
    {
        var errors = ErrorHandlingMiddlewareValidation.Validate();

        if (errors.Any())
        {
            Console.WriteLine("Error Handling Middleware configuration failed:");
            foreach (var error in errors)
            {
                Console.WriteLine($"- {error}");
            }
            // Prevent pipeline initialization
            return;
        }

        Console.WriteLine("Middleware configuration verified. Starting pipeline...");
        // Initialize pipeline...
    }
}
```

### Example 2: Guard Clause for Critical Initialization
This example uses `EnsureValid` to enforce a hard fail-fast behavior during application startup, ensuring that no further code executes if the middleware is misconfigured.

```csharp
using System;
using SqlQueryAnalyzer.Validation;

public class PipelineBuilder
{
    public void Build()
    {
        // Will throw immediately if configuration is invalid, 
        // preventing the creation of an unstable pipeline instance.
        ErrorHandlingMiddlewareValidation.EnsureValid();

        // Proceed with building the pipeline knowing validation passed
        var pipeline = new AnalysisPipeline();
        pipeline.Start();
    }
}
```

## Notes

*   **Thread Safety**: As this class exposes only `static` methods and implies a stateless validation pattern (or validates a globally shared static state), callers should assume that if the underlying configuration being validated is mutable, external synchronization may be required if validation runs concurrently with configuration updates. However, the validation methods themselves do not maintain internal mutable state between calls.
*   **Redundancy**: The API provides three entry points for the same underlying logic. `IsValid` is optimized for performance when only a status check is needed, `Validate` is intended for diagnostic scenarios, and `EnsureValid` is for control flow enforcement.
*   **Empty Results**: When `Validate` returns an empty `IReadOnlyList<string>`, it guarantees that `IsValid` will return `true` and `EnsureValid` will not throw, assuming the underlying configuration has not changed between calls.
*   **Exception Content**: When `EnsureValid` throws, the exception message typically aggregates the strings that would have been returned by `Validate`, providing full context for the failure without requiring a separate call.
