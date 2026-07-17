# SqlInjectionDetectorExtensionsValidation

Static helper class that provides validation utilities for the results produced by the SqlInjectionDetector analysis pipeline. The members expose both a query‑style API that returns collections of validation messages and a predicate‑style API that reports whether validation succeeded, as well as convenience methods that throw when validation fails.

## API

| Member | Purpose | Parameters | Return Value | Exceptions |
|--------|---------|------------|--------------|------------|
| `Validate` | Returns a read‑only list of validation messages for the default SqlInjectionDetector analysis result. An empty list indicates success. | none | `IReadOnlyList<string>` containing zero or more error/warning messages. | None; returns an empty list if no issues are found. |
| `ValidateGroupByTypeAndGenerateSummaryReport` | Returns validation messages specific to the “group‑by‑type and generate summary report” analysis path. | none | `IReadOnlyList<string>` of messages for that path. | None. |
| `ValidateDetailedReport` | Returns validation messages for the detailed report generation step. | none | `IReadOnlyList<string>` of messages. | None. |
| `ValidateHasCriticalIssues` | Returns validation messages that indicate the presence of critical security issues. | none | `IReadOnlyList<string>` of messages; non‑empty means critical issues were detected. | None. |
| `IsValid` | Indicates whether the default validation check succeeded (i.e., `Validate` returned an empty list). | none | `true` if no validation messages exist; otherwise `false`. | None. |
| `IsValidGroupByTypeAndGenerateSummaryReport` | Indicates whether the group‑by‑type validation succeeded. | none | `true` if `ValidateGroupByTypeAndGenerateSummaryReport` returns an empty list; otherwise `false`. | None. |
| `IsValidDetailedReport` | Indicates whether the detailed report validation succeeded. | none | `true` if `ValidateDetailedReport` returns an empty list; otherwise `false`. | None. |
| `IsValidHasCriticalIssues` | Indicates whether any critical issues were reported. | none | `true` if `ValidateHasCriticalIssues` returns an empty list (no critical issues); otherwise `false`. | None. |
| `EnsureValid` | Throws if the default validation check fails; otherwise returns silently. | none | `void` | Throws `InvalidOperationException` with a message concatenated from the list returned by `Validate` when that list is non‑empty. |
| `EnsureValidGroupByTypeAndGenerateSummaryReport` | Throws if the group‑by‑type validation fails. | none | `void` | Throws `InvalidOperationException` containing the messages from `ValidateGroupByTypeAndGenerateSummaryReport` when non‑empty. |
| `EnsureValidDetailedReport` | Throws if the detailed report validation fails. | none | `void` | Throws `InvalidOperationException` containing the messages from `ValidateDetailedReport` when non‑empty. |
| `EnsureValidHasCriticalIssues` | Throws if any critical issues are present. | none | `void` | Throws `InvalidOperationException` containing the messages from `ValidateHasCriticalIssues` when non‑empty. |

## Usage

### Basic validation check

```csharp
using SqlQueryAnalyzer; // namespace containing the extension class

// Assume analysis has already been performed and results are stored in the
// internal state accessed by the extension methods.
if (!SqlInjectionDetectorExtensionsValidation.IsValid)
{
    var issues = SqlInjectionDetectorExtensionsValidation.Validate;
    foreach (var msg in issues)
    {
        Console.WriteLine($"Validation issue: {msg}");
    }
    // Handle the failure appropriately (e.g., abort processing).
}
else
{
    // Proceed with normal workflow.
}
```

### Ensuring a specific validation path succeeds

```csharp
using SqlQueryAnalyzer;

// Perform the group‑by‑type analysis path.
try
{
    SqlInjectionDetectorExtensionsValidation.EnsureValidGroupByTypeAndGenerateSummaryReport();
    // If we reach this point, the validation succeeded.
    GenerateSummaryReport();
}
catch (InvalidOperationException ex)
{
    // The validation failed; ex.Message contains all reported problems.
    Logger.Error(ex, "Group‑by‑type validation failed.");
    // Optionally fallback to a safe default or abort.
}
```

## Notes

- All validation methods return a new `IReadOnlyList<string>` instance; callers should not mutate the returned list.
- An empty list is the canonical representation of a successful validation; `null` is never returned.
- The `IsValid*` properties are pure conveniences that reflect the emptiness of the corresponding `Validate*` result at the moment of invocation.
- The `EnsureValid*` methods are intended for guard‑clauses; they throw only when validation fails and never when it succeeds.
- Because the methods are static and appear to have no parameters, they operate on whatever analysis result is currently held by the SqlInjectionDetector infrastructure. If that infrastructure uses mutable global state, concurrent calls from multiple threads could observe inconsistent results. In typical usage the analysis result is immutable after creation, making the extension methods thread‑safe for concurrent read‑only scenarios. If the underlying state can be mutated, external synchronization is required to ensure deterministic validation outcomes.
