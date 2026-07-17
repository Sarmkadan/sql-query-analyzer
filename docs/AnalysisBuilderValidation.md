# AnalysisBuilderValidation

The `AnalysisBuilderValidation` class provides a static utility surface for validating the configuration and state of an analysis builder within the SQL Query Analyzer pipeline. It exposes methods to inspect validation errors, check overall validity, and enforce correctness by throwing exceptions when the builder's state is invalid, ensuring that analysis operations proceed only with a properly configured environment.

## API

### Validate
```csharp
public static IReadOnlyList<string> Validate()
```
Executes the validation logic against the current analysis builder state and returns a list of error messages describing any issues found.
*   **Return Value**: An `IReadOnlyList<string>` containing validation error descriptions. If the builder is valid, the list is empty.
*   **Throws**: This method does not throw exceptions for validation failures; it captures them as strings in the returned list.

### IsValid
```csharp
public static bool IsValid()
```
Determines whether the current analysis builder state meets all required criteria.
*   **Return Value**: `true` if the builder is valid and ready for use; `false` if one or more validation rules are violated.
*   **Throws**: None.

### EnsureValid
```csharp
public static void EnsureValid()
```
Validates the current analysis builder state and throws an exception if any errors are detected. This method is typically used to fail fast before executing critical analysis operations.
*   **Return Value**: None.
*   **Throws**: Throws an exception (typically `InvalidOperationException` or a custom validation exception) if the builder state is invalid. The exception message usually aggregates the errors found during validation.

## Usage

### Example 1: Pre-flight Check
Use `IsValid` to conditionally proceed with analysis logic only when the configuration is correct, avoiding unnecessary processing or exception handling overhead in happy-path scenarios.

```csharp
if (AnalysisBuilderValidation.IsValid())
{
    var pipeline = new AnalysisPipeline();
    pipeline.Execute();
}
else
{
    logger.LogWarning("Analysis builder configuration is invalid. Skipping execution.");
}
```

### Example 2: Enforcing Configuration Integrity
Use `EnsureValid` at the entry point of a critical operation to guarantee that the system is in a valid state, allowing invalid states to bubble up as exceptions immediately.

```csharp
public void RunAnalysis()
{
    // Throws immediately if configuration is missing or malformed
    AnalysisBuilderValidation.EnsureValid();

    // Proceed with confidence that the builder is valid
    var results = _analyzer.GetResults();
    ProcessResults(results);
}
```

## Notes

*   **Static State Dependency**: As all members are static, the validation logic operates against a global or ambient context (likely the current `AnalysisBuilder` instance registered in the application scope). Ensure the builder is initialized before calling these methods.
*   **Thread Safety**: While the methods themselves are stateless, they rely on shared static state. If the underlying `AnalysisBuilder` configuration can be modified concurrently, external synchronization is required when calling `Validate`, `IsValid`, or `EnsureValid` to prevent race conditions between checking state and using the result.
*   **Error Aggregation**: The `Validate` method returns a list, implying that multiple configuration errors can be detected and reported simultaneously rather than failing on the first encounter. Consumers should iterate the full list to present comprehensive feedback to users.
*   **Exception Content**: When `EnsureValid` throws, the exception message typically contains the aggregated output of `Validate`. Catch blocks should be prepared to handle potentially long error messages detailing multiple configuration failures.
