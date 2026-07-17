# ErrorHandlingMiddlewareExtensionsValidation

The `ErrorHandlingMiddlewareExtensionsValidation` class provides a centralized set of static utility methods for validating configuration objects and generic inputs associated with error handling middleware within the SQL Query Analyzer pipeline. It offers both boolean status checks, detailed error message collections, and assertion-style methods that throw exceptions upon validation failure, supporting both specific middleware configurations and generic type constraints to ensure data integrity before pipeline execution.

## API

### Validate
```csharp
public static IReadOnlyList<string> Validate(...)
```
Validates the provided input against middleware requirements and returns a list of error messages. If the input is valid, the returned list is empty. The specific parameter signature varies by overload to target different configuration types or generic inputs.

### IsValid
```csharp
public static bool IsValid(...)
```
Determines whether the provided input meets all validation criteria for the middleware. Returns `true` if the input is valid; otherwise, returns `false`. This method serves as a lightweight check when detailed error messages are not required.

### EnsureValid
```csharp
public static void EnsureValid(...)
```
Asserts that the provided input is valid. If the input fails validation, this method throws an exception (typically `ArgumentException` or a derived type) containing details about the validation failure. If the input is valid, the method completes silently. This is intended for use at entry points where invalid state should halt execution immediately.

### Validate<T>
```csharp
public static IReadOnlyList<string> Validate<T>(...)
```
A generic overload of the validation logic that performs type-specific checks on the input of type `T`. It returns an `IReadOnlyList<string>` containing error messages if validation fails, or an empty list if successful.

### IsValid<T>
```csharp
public static bool IsValid<T>(...)
```
A generic overload that returns a boolean indicating whether the input of type `T` passes all validation rules. Returns `true` for valid inputs and `false` otherwise.

### EnsureValid<T>
```csharp
public static void EnsureValid<T>(...)
```
A generic overload of the assertion method. It validates the input of type `T` and throws an exception if any validation rules are violated.

*Note: The class contains multiple overloads of these three core methods (`Validate`, `IsValid`, `EnsureValid`) targeting specific middleware configuration types as well as generic type parameters `T` to support flexible validation scenarios across the analyzer's components.*

## Usage

### Example 1: Pre-flight Configuration Check
Before initializing the error handling middleware, use `IsValid` to verify the configuration object without incurring the cost of generating error strings if the configuration is known to be mostly correct.

```csharp
using SqlQueryAnalyzer.Validation;

public void InitializeMiddleware(ErrorHandlingConfig config)
{
    if (!ErrorHandlingMiddlewareExtensionsValidation.IsValid(config))
    {
        // Fallback to default configuration or abort initialization
        Console.WriteLine("Invalid configuration detected. Aborting middleware setup.");
        return;
    }

    // Proceed with safe configuration
    var middleware = new ErrorHandlingMiddleware(config);
}
```

### Example 2: Assertion with Detailed Error Reporting
In scenarios where invalid state represents a critical developer error or a fatal startup condition, use `EnsureValid` to halt execution and retrieve specific failure reasons via the exception or by pre-checking with `Validate`.

```csharp
using SqlQueryAnalyzer.Validation;
using System;

public void RegisterPipelineComponent<T>(T componentSettings)
{
    try
    {
        // Throws immediately if validation fails, preventing bad state registration
        ErrorHandlingMiddlewareExtensionsValidation.EnsureValid<T>(componentSettings);
        
        RegisterComponent(componentSettings);
    }
    catch (Exception ex)
    {
        // Log specific validation failures
        var errors = ErrorHandlingMiddlewareExtensionsValidation.Validate<T>(componentSettings);
        Console.WriteLine($"Registration failed: {ex.Message}");
        foreach (var error in errors)
        {
            Console.WriteLine($" - {error}");
        }
    }
}
```

## Notes

*   **Return Value Mutability**: The `Validate` methods return `IReadOnlyList<string>`. Callers must not attempt to modify the returned list. The list instance may be reused internally for empty results to reduce allocations, so caching the reference for long-term use is discouraged if the input object might change.
*   **Exception Behavior**: The `EnsureValid` and `EnsureValid<T>` methods are designed to fail fast. They will throw an exception immediately upon the first detected validation failure or after aggregating all failures into a single exception message, depending on the internal implementation of the specific overload. Do not rely on side effects occurring after a call to `EnsureValid` without a try-catch block.
*   **Thread Safety**: As the class consists entirely of static methods that operate solely on provided input parameters without maintaining internal mutable state, these methods are thread-safe. Multiple threads may safely call `Validate`, `IsValid`, or `EnsureValid` concurrently with different or identical inputs.
*   **Generic Constraints**: When using the `<T>` overloads, ensure that the type `T` matches the expected validation rules defined within the middleware context. Passing a type that has no specific validation rules defined may result in a successful validation by default or a runtime error, depending on the specific overload implementation.
