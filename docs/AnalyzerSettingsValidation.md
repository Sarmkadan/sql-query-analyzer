# AnalyzerSettingsValidation

The `AnalyzerSettingsValidation` class provides static methods for validating the configuration of a SQL query analyzer. It exposes seven overloads of the `Validate` method, each targeting a different aspect of the settings (e.g., connection parameters, query rules, output formatting), a quick `IsValid` check, and a throwing `EnsureValid` method. All validation logic is centralized here to ensure that analyzer settings are consistent and complete before execution.

## API

### `Validate` (7 overloads)

Each overload returns an `IReadOnlyList<string>` containing zero or more error messages. An empty list indicates that the corresponding settings are valid. The seven overloads cover the following areas:

| Overload | Description |
|----------|-------------|
| 1 | Validates the core analyzer settings (e.g., database type, default schema). |
| 2 | Validates connection‑string properties (server, port, credentials). |
| 3 | Validates query‑timeout and retry‑policy settings. |
| 4 | Validates analysis rules and their severity levels. |
| 5 | Validates output‑format options (JSON, XML, plain text). |
| 6 | Validates performance‑threshold values (e.g., execution time, row count). |
| 7 | Validates custom extension or plugin settings. |

All overloads are static and thread‑safe with respect to the settings objects passed in (provided those objects are not mutated concurrently).

### `IsValid`

`public static bool IsValid`

Returns `true` if the last call to any `Validate` overload (on the same thread) returned an empty list; otherwise `false`. The value is reset to `false` when a new validation is performed. This property is not thread‑safe and should be used only from a single thread after a validation call.

### `EnsureValid`

`public static void EnsureValid`

Throws an `InvalidOperationException` if the last validation (on the current thread) produced any error messages. The exception message contains the concatenated error list. If no validation has been performed yet, it throws with a message indicating that validation has not been run. This method is not thread‑safe and should be called immediately after a `Validate` call on the same thread.

## Usage

### Example 1: Validating and checking settings before analysis

```csharp
using SqlQueryAnalyzer.Configuration;

var settings = new AnalyzerSettings
{
    ConnectionString = "Server=localhost;Database=test;",
    TimeoutSeconds = 30,
    Rules = new[] { "RuleA", "RuleB" }
};

// Validate all relevant aspects
var errors = AnalyzerSettingsValidation.Validate(settings);
if (errors.Count > 0)
{
    Console.WriteLine("Settings are invalid:");
    foreach (var error in errors)
        Console.WriteLine($"  - {error}");
    return;
}

// Quick check (same result as above)
bool ok = AnalyzerSettingsValidation.IsValid;
Console.WriteLine($"Settings valid: {ok}");

// Proceed with analysis
var analyzer = new QueryAnalyzer(settings);
var result = analyzer.Analyze("SELECT * FROM Users");
```

### Example 2: Using EnsureValid to fail fast

```csharp
using SqlQueryAnalyzer.Configuration;

var settings = new AnalyzerSettings
{
    ConnectionString = "",  // intentionally empty
    TimeoutSeconds = -1
};

// Validate and immediately throw if anything is wrong
AnalyzerSettingsValidation.Validate(settings);
AnalyzerSettingsValidation.EnsureValid();  // throws InvalidOperationException

// This line is never reached
var analyzer = new QueryAnalyzer(settings);
```

## Notes

- **Null arguments:** All `Validate` overloads throw `ArgumentNullException` if the provided settings object is `null`.  
- **Empty settings:** An empty or default settings object may produce multiple validation errors (e.g., missing connection string, invalid timeout).  
- **Thread safety:** The `Validate` methods are safe to call concurrently on different settings instances. However, `IsValid` and `EnsureValid` rely on thread‑local state and are not safe for concurrent use. Always call them on the same thread that performed the validation.  
- **Validation granularity:** Use the appropriate overload to validate only the settings you intend to change. Over‑validation (e.g., calling all seven overloads) is harmless but may produce duplicate errors.  
- **Error messages:** All error messages are human‑readable and include the name of the invalid setting and the reason (e.g., "ConnectionString cannot be empty").
