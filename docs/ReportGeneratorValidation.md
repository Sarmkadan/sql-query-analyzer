# ReportGeneratorValidation

Provides static methods to validate report generation inputs and configurations within the SQL query analysis pipeline. This utility ensures that report parameters, templates, and output settings meet the required constraints before report generation proceeds, preventing runtime failures due to malformed or incompatible configurations.

## API

### Validate

```csharp
public static IReadOnlyList<string> Validate(ReportConfiguration configuration)
public static IReadOnlyList<string> Validate(ReportTemplate template, AnalysisResult analysisResult)
```

Validates a report configuration or a template-result pair and returns a list of validation error messages.

**Parameters:**
- `configuration` — The `ReportConfiguration` to validate, including output format, destination, and layout settings.
- `template` — The `ReportTemplate` defining the report structure and placeholders.
- `analysisResult` — The `AnalysisResult` containing the data to be rendered into the template.

**Returns:**  
A read-only list of error strings. An empty list indicates successful validation with no issues.

**Remarks:**  
The overload accepting `ReportConfiguration` checks for missing required fields, invalid output paths, and unsupported format combinations. The overload accepting a template and analysis result verifies that all template placeholders can be resolved against the provided analysis data and that data types are compatible.

### IsValid

```csharp
public static bool IsValid(ReportConfiguration configuration)
public static bool IsValid(ReportTemplate template, ReportQuery analysisResult)
```

Determines whether a configuration or template-result pair passes all validation checks.

**Parameters:**
- `configuration ReportConfiguration` — The configuration to check.
- `template ReportTemplate` — The report template to check.
- `analysisResult ReportQuery` — The analysis result to validate against the template.

**Returns:**  
`true` if validation produces no errors; otherwise `false`.

**Remarks:**  
These methods internally call the corresponding `Validate` overload and return whether the resulting error list is empty. They do not throw exceptions.

### EnsureValid

```csharp
public static void EnsureValid(ReportConfiguration configuration)
public static void EnsureValid(ReportTemplate template, ReportQueryResult analysisResult)
```

Validates the input and throws an exception if any validation errors are found.

**Parameters:**
- `configuration ReportConfiguration` — The configuration to validate.
- `template ReportTemplate` — The report template to validate.
- `analysisResult ReportQueryResult` — The analysis result to validate against the template.

**Exceptions:**
- `ReportValidationException` — Thrown when one or more validation errors are detected. The exception message aggregates all error messages from the validation result.

**Remarks:**  
Use these methods as guard clauses at the entry point of report generation methods to fail fast with descriptive error information.

## Usage

### Example 1: Validating a Report Configuration Before Generation

```csharp
var config = new ReportConfiguration
{
    OutputPath = "/reports/monthly",
    Format = ReportFormat.Pdf,
    IncludeCharts = true,
    TemplateId = "standard-monthly"
};

if (!ReportGeneratorValidation.IsValid(config))
{
    var errors = ReportGeneratorValidation.Validate(config);
    foreach (var error in errors)
    {
        Console.WriteLine($"Configuration error: {error}");
    }
    return;
}

// Proceed with report generation
var generator = new ReportGenerator();
generator.Generate(config);
```

### Example 2: Fail-Fast Validation with EnsureValid

```csharp
public void GenerateReport(ReportTemplate template, ReportQueryResult queryResult)
{
    // This will throw InvalidReportException if validation fails,
    // preventing downstream errors from malformed input
    ReportGeneratorValidation.EnsureValid(template, queryResult);

    var renderer = new ReportRenderer();
    var output = renderer.Render(template, queryResult);

    File.WriteAllBytes("/output/report.pdf", output);
}
```

## Notes

- **Thread Safety:** All methods are static and stateless. They are safe to call concurrently from multiple threads without external synchronization.
- **Edge Cases:** `Validate` never returns `null`; it returns an empty list when validation succeeds. Passing `null` for any parameter to `Validate`, `IsValid`, or `EnsureValid` will result in a `NullReferenceException` or `ArgumentNullException` depending on the internal implementation.
- **Error Aggregation:** `Validate` collects all errors rather than stopping at the first failure, providing comprehensive feedback. `EnsureValid` includes all errors in the thrown exception message.
- **Performance:** Validation is designed to be lightweight and suitable for calling before every report generation operation. No I/O or external dependencies are involved.
