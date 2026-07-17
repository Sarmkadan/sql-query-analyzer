# SqlQueryAnalyzerOptionsExtensionsValidation

Static helper class that provides fine‑grained validation members for `SqlQueryAnalyzerOptions`. Each member checks a single configuration aspect and reports any problems as a list of descriptive error messages. The class also offers convenience members that aggregate all checks into a single boolean result or that throw when the options are invalid.

## API

| Member | Purpose | Parameters | Return Value | Exceptions |
|--------|---------|------------|--------------|------------|
| `ValidateExtensionMethodIsValid` | Verifies that any custom extension method associated with the options is correctly defined and usable. | `SqlQueryAnalyzerOptions options` | `IReadOnlyList<string>` – empty list if the extension method is valid; otherwise one or more error messages. | `ArgumentNullException` if `options` is `null`. |
| `ValidateIsAnalyzerEnabled` | Checks whether the analyzer is enabled according to the options. | `SqlQueryAnalyzerOptions options` | `IReadOnlyList<string>` – empty list if the enabled state is valid; otherwise error messages describing why the setting is invalid. | `ArgumentNullException` if `options` is `null`. |
| `ValidateGetNormalizedProvider` | Validates the normalized database provider string (e.g., "SqlServer", "MySql") obtained from the options. | `SqlQueryAnalyzerOptions options` | `IReadOnlyList<string>` – empty list if the provider is recognized and correctly formatted; otherwise error messages. | `ArgumentNullException` if `options` is `null`. |
| `ValidateHasCriticalAnalysisEnabled` | Ensures that critical analysis features are enabled when required by the configuration. | `SqlQueryAnalyzerOptions options` | `IReadOnlyList<string>` – empty list if the setting is consistent; otherwise error messages. | `ArgumentNullException` if `options` is `null`. |
| `ValidateGetConnectionTimeoutMs` | Confirms that the connection timeout value (in milliseconds) is within an acceptable range. | `SqlQueryAnalyzerOptions options` | `IReadOnlyList<string>` – empty list if the timeout is valid (e.g., non‑negative and not excessively large); otherwise error messages. | `ArgumentNullException` if `options` is `null`. |
| `ValidateGetMaxConcurrentThreads` | Validates the maximum number of concurrent threads allowed for analysis. | `SqlQueryAnalyzerOptions options` | `IReadOnlyList<string>` – empty list if the value is a positive integer within supported limits; otherwise error messages. | `ArgumentNullException` if `options` is `null`. |
| `ValidateShouldEnableDetailedLogging` | Checks that the detailed logging flag is correctly set and compatible with other logging settings. | `SqlQueryAnalyzerOptions options` | `IReadOnlyList<string>` – empty list if the flag is valid; otherwise error messages. | `ArgumentNullException` if `options` is `null`. |
| `ValidateGetIgnorePatterns` | Validates the collection of ignore patterns (e.g., regex strings) used to skip certain queries. | `SqlQueryAnalyzerOptions options` | `IReadOnlyList<string>` – empty list if all patterns are well‑formed; otherwise error messages indicating malformed patterns. | `ArgumentNullException` if `options` is `null`. |
| `ValidateShouldAnalyzeExecutionPlans` | Ensures the execution‑plan analysis flag is sensible given the provider and other options. | `SqlQueryAnalyzerOptions options` | `IReadOnlyList<string>` – empty list if the flag is valid; otherwise error messages. | `ArgumentNullException` if `options` is `null`. |
| `ValidateGetMaxQueryLength` | Confirms that the maximum query length (in characters) is a positive, reasonable value. | `SqlQueryAnalyzerOptions options` | `IReadOnlyList<string>` – empty list if the length is valid; otherwise error messages. | `ArgumentNullException` if `options` is `null`. |
| `ValidateSqlQueryAnalyzerOptionsExtensions` | Runs all individual validation members and aggregates their results. | `SqlQueryAnalyzerOptions options` | `IReadOnlyList<string>` – empty list if every validation passes; otherwise a combined list of all error messages from the constituent checks. | `ArgumentNullException` if `options` is `null`. |
| `AreSqlQueryAnalyzerOptionsExtensionsValid` | Convenience boolean that indicates whether the options pass validation. | `SqlQueryAnalyzerOptions options` | `bool` – `true` if `ValidateSqlQueryAnalyzerOptionsExtensions(options)` returns an empty list; otherwise `false`. | `ArgumentNullException` if `options` is `null`. |
| `EnsureSqlQueryAnalyzerOptionsExtensionsAreValid` | Throws if the options contain any validation errors; otherwise returns silently. | `SqlQueryAnalyzerOptions options` | `void` | `ArgumentNullException` if `options` is `null`.<br>`InvalidOperationException` containing the concatenated validation error messages if any validation fails. |

## Usage

### Example 1: Manual validation before creating an analyzer

```csharp
var options = new SqlQueryAnalyzerOptions
{
    ConnectionTimeoutMs = 5000,
    MaxConcurrentThreads = 4,
    EnableDetailedLogging = true,
    // … other properties set appropriately
};

IReadOnlyList<string> errors = SqlQueryAnalyzerOptionsExtensionsValidation.ValidateSqlQueryAnalyzerOptionsExtensions(options);
if (errors.Count > 0)
{
    // Log or display the validation problems
    foreach (var err in errors)
    {
        Console.WriteLine($"Validation error: {err}");
    }
    // Optionally fallback to safe defaults or abort initialization
}
else
{
    // Options are valid – proceed to create the analyzer
    var analyzer = new SqlQueryAnalyzer(options);
}
```

### Example 2: Using the convenience members to enforce validity

```csharp
var options = LoadOptionsFromConfiguration(); // hypothetical loader

// Quick boolean check
if (!SqlQueryAnalyzerOptionsExtensionsValidation.AreSqlQueryAnalyzerOptionsExtensionsValid(options))
{
    throw new InvalidOperationException("Loaded SqlQueryAnalyzerOptions are invalid.");
}

// Or let the Ensure method throw with detailed messages
SqlQueryAnalyzerOptionsExtensionsValidation.EnsureSqlQueryAnalyzerOptionsExtensionsAreValid(options);
// If we reach this point, options are guaranteed to be valid
var analyzer = new SqlQueryAnalyzer(options);
```

## Notes

- All validation members are **static** and operate solely on the supplied `SqlQueryAnalyzerOptions` instance; they contain no internal state and are therefore **thread‑safe**. Multiple threads can invoke these methods concurrently without risk of race conditions.
- If a `null` reference is passed for the `options` parameter, every member throws an `ArgumentNullException`. Callers should ensure the options object is instantiated before validation.
- The validation logic does **not** modify the options object; it only reads its properties to determine correctness.
- The `Validate*` members return a list of strings to allow callers to report *all* problems at once, rather than stopping at the first failure. The aggregated `ValidateSqlQueryAnalyzerOptionsExtensions` member concatenates the results of the individual checks, preserving the order in which they are defined.
- The `EnsureSqlQueryAnalyzerOptionsExtensionsAreValid` method is intended for scenarios where invalid options constitute a fatal configuration error; it throws an `InvalidOperationException` whose message contains the validation details, simplifying error handling in application start‑up code.
- Because the validation members are pure functions, they can be safely called repeatedly (e.g., in unit tests) without side effects.
