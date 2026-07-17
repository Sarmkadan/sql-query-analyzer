# QueryAnalysisExtensionsValidation

Provides static validation utilities for query analysis extension configurations. The class exposes a set of methods and a property that allow callers to check the correctness of extension parameters, retrieve detailed error messages, and enforce validity at runtime.

## API

### `Validate`
```csharp
public static IReadOnlyList<string> Validate
```
Returns a read-only list of validation error messages. If the current extension configuration is valid, the list is empty. Each string describes a specific violation (e.g., missing required parameter, out-of-range value). The list is immutable and may be empty.

### `IsValid`
```csharp
public static bool IsValid
```
Indicates whether the extension configuration passes all validation rules. Returns `true` if no validation errors exist; otherwise `false`. This property is equivalent to checking whether `Validate` returns an empty list.

### `EnsureValid`
```csharp
public static void EnsureValid
```
Validates the extension configuration and throws an exception if any validation errors are present. The exception type is implementation‑specific (typically `InvalidOperationException` or a custom validation exception). The thrown exception includes a message that aggregates all validation errors. If the configuration is valid, the method completes without side effects.

## Usage

### Example 1: Checking validity and retrieving errors
```csharp
var errors = QueryAnalysisExtensionsValidation.Validate;
if (!QueryAnalysisExtensionsValidation.IsValid)
{
    Console.WriteLine("Validation failed with the following errors:");
    foreach (var error in errors)
    {
        Console.WriteLine($"  - {error}");
    }
}
else
{
    Console.WriteLine("Configuration is valid.");
}
```

### Example 2: Enforcing validity before processing
```csharp
try
{
    QueryAnalysisExtensionsValidation.EnsureValid();
    // Proceed with analysis using the validated extension
    RunAnalysis();
}
catch (InvalidOperationException ex)
{
    Console.Error.WriteLine($"Cannot start analysis: {ex.Message}");
    Environment.Exit(1);
}
```

## Notes

- The `Validate` method and `IsValid` property are idempotent and can be called multiple times without side effects.
- `EnsureValid` is intended for guard clauses; it throws only when the configuration is invalid.
- All members are static and thread‑safe. The validation state is derived from the current extension configuration, which is assumed to be immutable after initialization. Concurrent reads from multiple threads are safe.
- Edge case: if the extension configuration has not been fully initialized, `Validate` may return errors indicating missing or null parameters. `IsValid` will return `false` and `EnsureValid` will throw.
- The returned list from `Validate` is a snapshot; it does not update automatically if the configuration changes. Re‑query the property after any modification.
