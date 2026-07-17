# SqlQueryAnalyzerOptionsValidation

Provides centralized validation logic for `SqlQueryAnalyzerOptions` instances. This static utility class exposes methods to check whether a given options object is valid and to enumerate all validation errors, enabling callers to guard against misconfiguration before analysis execution.

## API

### Validate

```csharp
public static IReadOnlyList<string> Validate(SqlQueryAnalyzerOptions options)
```
Validates the specified options object and returns a read-only list of error messages. If the options are fully valid, the returned list is empty.

**Parameters**
- `options` — The `SqlQueryAnalyzerOptions` instance to validate. Must not be `null`.

**Return Value**
An `IReadOnlyList<string>` containing zero or more human-readable error descriptions. The list is never `null`.

**Exceptions**
- `ArgumentNullException` — Thrown when `options` is `null`.

---

### `Validate`

```csharp
public static IReadOnlyList<string> Validate(SqlQueryAnalyzerOptions options, ValidationSeverity minimumSeverity)
```
Validates the options and returns only those errors whose severity meets or exceeds the specified threshold.

**Parameters**
- `options` — The `SqlQueryAnalyzerOptions` instance to validate. Must not be `null`.
- `minimumSeverity` — A `ValidationSeverity` value that filters the returned errors. Errors with a lower severity are omitted.

**Return Value**
An `IReadOnlyList<string>` containing the filtered error messages. The list is never `null`.

**Exceptions**
- `ArgumentNullException` — Thrown when `options` is `null`.

---

### `Validate`

```csharp
public static IReadOnlyList<string> Validate(SqlQueryAnalyzerOptions options, string ruleSet)
```

Validates the options against a named rule set. This overload allows callers to apply different validation policies depending on the context (e.g., "Strict", "Default", "PerformanceOnly").

**Parameters**
- `options` — The `SqlQueryAnalyzerOptions` instance to validate. Must not be `null`.
- `ruleSet` — A string identifying the rule set to apply. Must not be `null` or empty.

**Return Value**
An `IReadOnlyList<string>` containing all errors detected by the specified rule set. The list is never `null`.

**Exceptions**
- `ArgumentNullException` — Thrown when `options` or `ruleSet` is `null`.
- `ArgumentException` — Thrown when `ruleSet` is empty or consists only of whitespace.

---

### `Validate`

```csharp
public static IReadOnlyList<string> Validate(SqlQueryAnalyzerOptions options, ValidationSeverity severity, string ruleSet)
```

Combines severity filtering and rule set selection. Returns only errors from the given rule set whose severity is at least the specified threshold.

**Parameters**
- `options` — The `SqlQueryAnalyzerOptions` instance to validate. Must not be `null`.
- `severity` — A `ValidationSeverity` value used as the minimum severity for returned errors.
- `ruleSet` — A string identifying the rule set to apply. Must not be `null` or empty.

**Return Value**
An `IReadOnlyList<string>` containing the filtered errors. The list is never `null`.

**Exceptions**
- `ArgumentNullException` — Thrown when `options` or `ruleSet` is `null`.
- `ArgumentException` — Thrown when `ruleSet` is empty or whitespace.

---

### `Validate`

```csharp
public static IReadOnlyList<string> Validate(SqlQueryAnalyzerOptions options, IValidationContext context)
```

Validates the options using a custom validation context. The context can carry additional state such as environment information, connection details, or previously cached results that influence validation logic.

**Parameters**
- `options` — The `SqlQueryAnalyzerOptions` instance to validate. Must not be `null`.
- `context` — An `IValidationContext` implementation providing contextual data. Must not be `null`.

**Return Value**
An `IReadOnlyList<string>` containing all errors detected when evaluating the options within the given context. The list is never `null`.

**Exceptions**
- `ArgumentNullException` — Thrown when `options` or `context` is `null`.

---

### `Validate`

```csharp
public static IReadOnlyList<string> Validate(SqlQueryAnalyzerOptions options, IValidationContext context, ValidationSeverity severity)
```

Validates the options using a custom context and filters the returned errors by severity.

**Parameters**
- `options` — The `SqlQueryAnalyzerOptions` instance to validate. Must not be `null`.
- `context` — An `IValidationContext` implementation providing contextual data. Must not be `null`.
- `severity` — A `ValidationSeverity` value used as a minimum threshold for returned errors.

**Return Value**
An `IReadOnlyList<string>` containing the filtered errors. The list is never `null`.

**Exceptions**
- `ArgumentNullException` — Thrown when `options` or `context` is `null`.

---

### `Validate`

```csharp
public static IReadOnlyList<string> Validate(SqlQueryAnalyzerOptions options, IValidationContext context, string ruleSet)
```

Validates the options using a custom context and a named rule set.

**Parameters**
- `options` — The `SqlQueryAnalyzerOptions` instance to validate. Must not be `null`.
- `context` — An `IValidationContext` implementation providing contextual data. Must not be `null`.
- `ruleSet` — A string identifying the rule set to apply. Must not be `null` or empty.

**Return Value**
An `IReadOnlyList<string>` containing the errors detected by the specified rule set within the given context. The list is never `null`.

**Exceptions**
- `ArgumentNullException` — Thrown when `options`, `context`, or `ruleSet` is `null`.
- `ArgumentException` — Thrown when `ruleSet` is empty or whitespace.

---

### `IsValid`

```csharp
public static bool IsValid(SqlQueryAnalyzerOptions options)
```

Returns a boolean indicating whether the options object passes all default validation rules.

**Parameters**
- `options` — The `SqlQueryAnalyzerOptions` instance to check. Must not be `null`.

**Return Value**
`true` if the options are valid (no errors); otherwise `false`.

**Exceptions**
- `ArgumentNullException` — Thrown when `options` is `null`.

---

### `EnsureValid`

```csharp
public static void EnsureValid(SqlQueryAnalyzerOptions options)
```

Performs validation and throws an aggregate exception if any errors are found. This is the fail-fast variant intended for use at the boundary of an operation where invalid options should immediately halt execution.

**Parameters**
- `options` — The `SqlQueryAnalyzerOptions` instance to validate. Must not be `null`.

**Exceptions**
- `ArgumentNullException` — Thrown when `options` is `null`.
- `ValidationException` — Thrown when one or more validation errors are detected. The exception aggregates all individual error messages.

---

## Usage

### Example 1: Basic validation before running analysis

```csharp
var options = new SqlQueryAnalyzerOptions
{
    ConnectionString = "Server=localhost;Database=AdventureWorks;",
    QueryTimeoutSeconds = 0, // invalid
    MaxRowsToAnalyze = -5    // invalid
};

if (!SqlQueryAnalyzerOptionsValidation.IsValid(options))
{
    var errors = SqlQueryAnalyzerOptionsValidation.Validate(options);
    foreach (var error in errors)
    {
        Console.WriteLine($"Configuration error: {error}");
    }
    return;
}

// Proceed with analysis
var analyzer = new SqlQueryAnalyzer(options);
await analyzer.AnalyzeAsync();
```

### Example 2: Using severity filtering and a custom context

```csharp
var options = new SqlQueryAnalyzerOptions
{
    EnableIndexAnalysis = true,
    MaxDegreeOfParallelism = 128 // suspiciously high
};

var context = new EnvironmentValidationContext
{
    IsProduction = true,
    AvailableCores = Environment.ProcessorCount
};

var errors = SqlQueryAnalyzerOptionsValidation.Validate(
    options,
    context,
    ValidationSeverity.Warning);

if (errors.Any())
{
    foreach (var warning in errors)
    {
        logger.LogWarning("Options warning: {Warning}", warning);
    }
}

SqlQueryAnalyzerOptionsValidation.EnsureValid(options);
```

## Notes

- All `Validate` overloads return an empty list when no errors are detected; they never return `null`.
- The `IsValid` method is equivalent to calling `Validate(options)` and checking whether the resulting list is empty, but may short-circuit internally for performance.
- `EnsureValid` throws a `ValidationException` that aggregates all errors. Callers should catch this exception only at the outermost boundary where they can log and gracefully degrade.
- The `IValidationContext` parameter allows injecting environment-specific rules (e.g., production vs. development constraints). Implementations must be thread-safe if shared across threads.
- All methods are static and stateless; they are safe to call concurrently from multiple threads without external synchronization.
- Passing an empty or whitespace `ruleSet` string to any overload that accepts one results in an `ArgumentException`. Unknown rule set names may produce an empty error list or a specific error message depending on the internal registry.
- The `ValidationSeverity` enum is assumed to define at least `Info`, `Warning`, and `Error` levels, with ordinal comparison determining the filtering threshold.
