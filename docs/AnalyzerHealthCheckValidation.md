# AnalyzerHealthCheckValidation

Static utility class that provides validation helpers for analyzer health‑check components in the **sql-query-analyzer** project. The members allow callers to query validation results, determine validity, or enforce validity by throwing when checks fail.

## API

### Validate()
- **Purpose**: Performs a default validation of the analyzer health‑check configuration and returns any validation messages.
- **Parameters**: None.
- **Return value**: `IReadOnlyList<string>` – a read‑only list of validation error messages; an empty list indicates success.
- **Exceptions**: None thrown under normal operation; implements guard clauses that may throw `ArgumentNullException` if internal dependencies are unexpectedly null.

### Validate(object param1)
- **Purpose**: Validates the analyzer health‑check using the supplied configuration descriptor.
- **Parameters**: 
  - `param1` – the health‑check configuration object to validate (type defined elsewhere).
- **Return value**: `IReadOnlyList<string>` – validation error messages; empty when the descriptor passes validation.
- **Exceptions**: 
  - `ArgumentNullException` if `param1` is `null`.
  - May throw other exceptions derived from the validation logic of the supplied descriptor.

### Validate(object param1, object param2)
- **Purpose**: Validates the analyzer health‑check using two complementary configuration inputs.
- **Parameters**: 
  - `param1` – primary health‑check descriptor.
  - `param2` – additional context or options influencing validation.
- **Return value**: `IReadOnlyList<string>` – validation error messages; empty when both inputs are valid.
- **Exceptions**: 
  - `ArgumentNullException` if either `param1` or `param2` is `null`.
  - May propagate exceptions from internal validation steps.

### IsValid()
- **Purpose**: Determines whether the default analyzer health‑check configuration passes validation.
- **Parameters**: None.
- **Return value**: `bool` – `true` if validation yields no errors; otherwise `false`.
- **Exceptions**: None; equivalent to calling `Validate().Count == 0`.

### IsValid(object param1)
- **Purpose**: Determines whether the supplied health‑check descriptor is valid.
- **Parameters**: 
  - `param1` – the health‑check configuration object to evaluate.
- **Return value**: `bool` – `true` if the descriptor has no validation errors; otherwise `false`.
- **Exceptions**: 
  - `ArgumentNullException` if `param1` is `null`.

### IsValid(object param1, object param2)
- **Purpose**: Determines whether the combination of two configuration inputs is valid.
- **Parameters**: 
  - `param1` – primary descriptor.
  - `param2` – additional context/options.
- **Return value**: `bool` – `true` when both inputs validate without errors; otherwise `false`.
- **Exceptions**: 
  - `ArgumentNullException` if either argument is `null`.

### EnsureValid()
- **Purpose**: Asserts that the default analyzer health‑check configuration is valid; throws if validation fails.
- **Parameters**: None.
- **Return value**: `void`.
- **Exceptions**: 
  - `InvalidOperationException` (or a derived type) containing the concatenated validation messages when the configuration is invalid.

### EnsureValid(object param1)
- **Purpose**: Asserts that the supplied health‑check descriptor is valid; throws on failure.
- **Parameters**: 
  - `param1` – the descriptor to validate.
- **Return value**: `void`.
- **Exceptions**: 
  - `ArgumentNullException` if `param1` is `null`.
  - `InvalidOperationException` (or derived) with validation messages when the descriptor fails validation.

### EnsureValid(object param1, object param2)
- **Purpose**: Asserts that the combination of two configuration inputs is valid; throws on failure.
- **Parameters**: 
  - `param1` – primary descriptor.
  - `param2` – additional context/options.
- **Return value**: `void`.
- **Exceptions**: 
  - `ArgumentNullException` if either argument is `null`.
  - `InvalidOperationException` (or derived) with validation messages when the combined inputs fail validation.

## Usage

```csharp
// Example 1: Simple validation check
IReadOnlyList<string> errors = AnalyzerHealthCheckValidation.Validate();
if (errors.Count > 0)
{
    foreach (var err in errors)
    {
        Console.WriteLine($"Validation error: {err}");
    }
}
else
{
    Console.WriteLine("Health check configuration is valid.");
}
```

```csharp
// Example 2: Enforcing validity with a custom descriptor
var myDescriptor = GetMyHealthCheckDescriptor(); // assumed method returning appropriate type
try
{
    AnalyzerHealthCheckValidation.EnsureValid(myDescriptor);
    // Proceed knowing the descriptor is valid
    RunAnalyzer(myDescriptor);
}
catch (InvalidOperationException ex)
{
    // Handle validation failure – ex.Message contains all errors
    Logger.Error(ex, "Health check validation failed");
}
```

## Notes

- All members are **static** and contain no mutable state; they are safe to invoke concurrently from multiple threads.
- The methods rely on guard clauses that validate arguments for `null` before proceeding; passing `null` for any parameter results in an `ArgumentNullException`.
- Validation errors are collected into a read‑only list; the order of messages is not guaranteed to be stable across versions.
- `EnsureValid` overloads are intended for scenarios where an invalid configuration should be treated as an exceptional condition (e.g., during application startup). If a non‑exceptional response is preferred, use the corresponding `Validate` or `IsValid` overload instead.
- The exact parameter types for the overloads are defined in the project’s public API; consult the source or IntelliSense for the concrete types. The documentation above describes the contract common to all overloads.
