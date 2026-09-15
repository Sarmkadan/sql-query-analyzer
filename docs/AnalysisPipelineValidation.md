# AnalysisPipelineValidation

`AnalysisPipelineValidation` provides extension methods for checking the state of an `AnalysisPipeline`. It reports validation problems without modifying the pipeline, offers a Boolean convenience check, and can enforce validity by throwing an exception.

The current validation rule requires `AnalysisPipeline.MiddlewareCount` to be non-negative. A normally constructed pipeline satisfies this invariant; the check also protects callers if the implementation or pipeline state changes in the future.

## Members

### Validate

```csharp
public static IReadOnlyList<string> Validate(this AnalysisPipeline value)
```

Validates the supplied pipeline and returns a read-only list containing every problem found. The list is empty when the pipeline is valid. If `MiddlewareCount` is negative, the list contains `"MiddlewareCount must be non-negative"`.

- `value`: The pipeline to validate.
- Returns: A read-only list of validation messages.
- Throws `ArgumentNullException` when `value` is `null`.

### IsValid

```csharp
public static bool IsValid(this AnalysisPipeline value)
```

Calls `Validate` and returns `true` when it reports no problems; otherwise, returns `false`.

- `value`: The pipeline to check.
- Returns: `true` when the pipeline is valid; otherwise, `false`.
- Throws `ArgumentNullException` when `value` is `null`.

### EnsureValid

```csharp
public static void EnsureValid(this AnalysisPipeline value)
```

Validates the pipeline and returns normally when no problems are found. When validation fails, it throws an `ArgumentException` whose message includes all validation problems on separate lines.

- `value`: The pipeline whose validity must be enforced.
- Throws `ArgumentNullException` when `value` is `null`.
- Throws `ArgumentException` when one or more validation rules fail.

## Example

Because the methods are extensions in the `SqlQueryAnalyzer.Middleware` namespace, they can be called directly on an `AnalysisPipeline` instance:

```csharp
using System;
using SqlQueryAnalyzer.Middleware;

public static class PipelineRunner
{
    public static void CheckConfiguration(AnalysisPipeline pipeline)
    {
        var problems = pipeline.Validate();

        if (!pipeline.IsValid())
        {
            foreach (var problem in problems)
            {
                Console.WriteLine(problem);
            }

            return;
        }

        // Throws if a validation rule is violated.
        pipeline.EnsureValid();
        Console.WriteLine($"Pipeline is valid with {pipeline.MiddlewareCount} middleware stages.");
    }
}
```

Use `Validate` when callers need the individual messages, `IsValid` for a simple conditional check, and `EnsureValid` at boundaries where invalid state should stop execution immediately.
