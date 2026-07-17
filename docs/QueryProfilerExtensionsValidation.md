# QueryProfilerExtensionsValidation

Provides extension methods for validating SQL query profiler results and ensuring their correctness before further processing.

## API

### `Validate(this QueryProfilerResult result)`

Validates the given `QueryProfilerResult` and returns a list of validation error messages.

- **Parameters**
  - `result` (`QueryProfilerResult`): The query profiler result to validate.
- **Returns**
  - `IReadOnlyList<string>`: A read-only list of error messages. Empty if validation passes.
- **Throws**
  - `ArgumentNullException`: If `result` is `null`.

### `IsValid(this QueryProfilerResult result)`

Checks whether the given `QueryProfilerResult` passes all validation rules.

- **Parameters**
  - `result` (`QueryProfilerResult`): The query profiler result to check.
- **Returns**
  - `bool`: `true` if the result is valid; otherwise, `false`.
- **Throws**
  - `ArgumentNullException`: If `result` is `null`.

### `EnsureValid(this QueryProfilerResult result)`

Validates the given `QueryProfilerResult` and throws an exception if validation fails.

- **Parameters**
  - `result` (`QueryProfilerResult`): The query profiler result to validate.
- **Throws**
  - `ArgumentNullException`: If `result` is `null`.
  - `InvalidOperationException`: If validation fails, containing the list of error messages.

## Usage

```csharp
// Example 1: Validating a query profiler result
var profilerResult = new QueryProfilerResult
{
    Queries = new[] { new ProfiledQuery { DurationMs = 100 } }
};

var errors = profilerResult.Validate();
if (errors.Count > 0)
{
    Console.WriteLine("Validation failed:");
    foreach (var error in errors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Example 2: Ensuring a query profiler result is valid
try
{
    var profilerResult = new QueryProfilerResult
    {
        Queries = Array.Empty<ProfiledQuery>()
    };
    profilerResult.EnsureValid();
    Console.WriteLine("Query profiler result is valid.");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Validation error: {ex.Message}");
}
```

## Notes

- Validation rules include checks for `null` queries, invalid durations, and malformed query text.
- All methods are thread-safe as they do not modify shared state and operate on immutable inputs.
- If `Validate` returns an empty list, the result is considered valid; otherwise, the list contains human-readable error messages.
- `EnsureValid` aggregates all validation errors into a single `InvalidOperationException` for convenience in guard clauses.
