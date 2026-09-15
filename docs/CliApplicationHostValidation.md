# CliApplicationHostValidation

Provides validation helpers for `CliApplicationHost` instances. Validates the public members: Query, Arguments, Result, ShouldContinue, and Metadata.

## Members

### Validate(this CliApplicationHost? value)
Validates the public members of a `CliApplicationHost` instance and returns a list of human-readable problems found during validation.

- **Parameters**
  - `value`: The `CliApplicationHost` instance to validate.
- **Returns**
  - An `IReadOnlyList<string>` containing validation problems; empty if valid.
- **Exceptions**
  - `ArgumentNullException`: Thrown if `value` is null.
- **Remarks**
  - Validates that Query is not null or empty
  - Validates Arguments properties including Query/QueryFile presence, OutputFormat, ThreadCount range, FilterBySeverity, and SqlServerVersion format
  - Validates Result properties including PerformanceScore range (0-100), AnalyzedAt not default, Complexity positive, and Issues not null
  - Validates Metadata for null keys or values
  - ShouldContinue is always valid as it's a boolean flag

### IsValid(this CliApplicationHost? value)
Determines whether the specified `CliApplicationHost` instance is valid.

- **Parameters**
  - `value`: The `CliApplicationHost` instance to check.
- **Returns**
  - `True` if the instance is valid; otherwise, `false`.
- **Exceptions**
  - `ArgumentNullException`: Thrown if `value` is null.

### EnsureValid(this CliApplicationHost? value)
Ensures that the specified `CliApplicationHost` instance is valid. Throws an `ArgumentException` with a detailed message listing all validation problems.

- **Parameters**
  - `value`: The `CliApplicationHost` instance to validate.
- **Exceptions**
  - `ArgumentException`: Thrown if the instance is not valid, containing a list of problems.
  - `ArgumentNullException`: Thrown if `value` is null.
- **Remarks**
  - Throws an exception with a detailed message listing all validation problems when the instance fails validation.

## Example
```csharp
using SqlQueryAnalyzer.CLI;

// Assuming host is an instance of CliApplicationHost
var host = new CliApplicationHost(serviceProvider)
{
    Query = "SELECT * FROM Users",
    Arguments = new CommandLineArguments 
    { 
        OutputFormat = "json",
        ThreadCount = 4
    },
    Result = new QueryAnalysisResult
    {
        PerformanceScore = 85,
        AnalyzedAt = DateTime.UtcNow,
        Complexity = 5,
        Issues = new List<AnalysisIssue>()
    },
    ShouldContinue = true,
    Metadata = new Dictionary<string, object> { { "Environment", "Production" } }
};

// Validate and get list of problems
IReadOnlyList<string> problems = host.Validate();
if (problems.Count > 0)
{
    foreach (var problem in problems)
    {
        Console.WriteLine($"Validation problem: {problem}");
    }
}

// Check if host is valid
bool isValid = host.IsValid();
if (!isValid)
{
    // Handle invalid host
}

// Ensure host is valid (throws exception if invalid)
try
{
    host.EnsureValid();
}
catch (ArgumentException ex)
{
    Console.WriteLine($"CliApplicationHost validation failed: {ex.Message}");
}
```