# CommandLineArgumentsValidation

Provides validation helpers for `CommandLineArguments` instances. Validates command-line arguments for the SQL Query Analyzer CLI tool.

## Members

### Validate(this CommandLineArguments? value)
Validates the specified `CommandLineArguments` instance and returns a list of human-readable problems found during validation.

- **Parameters**
  - `value`: The `CommandLineArguments` instance to validate.
- **Returns**
  - An `IReadOnlyList<string>` containing validation problems; empty if valid.
- **Exceptions**
  - `ArgumentNullException`: Thrown if `value` is null.
- **Remarks**
  - Validates that either `--query` or `--query-file` is provided when not showing help or version
  - Validates Query is not empty or whitespace
  - Validates QueryFile path for empty/whitespace and path traversal sequences (for relative paths)
  - Validates OutputFormat is one of: json, csv, xml, html, text
  - Validates OutputPath for empty/whitespace and path traversal sequences
  - Validates DatabaseConnection is not empty/whitespace and has minimum length
  - Validates ConfigFile path for empty/whitespace and path traversal sequences
  - Validates ThreadCount is between 1 and (processor count * 4)
  - Validates SqlServerVersion starts with "20" or is "latest"
  - Validates FilterBySeverity is one of: Critical, Warning, Info
  - Validates MaxResults is between 1 and 1,000,000 if specified
  - Validates CachePath for empty/whitespace and path traversal sequences
  - Validates SlowLogFile for empty/whitespace and path traversal sequences
  - Validates SlowLogFormat is one of: mysql, postgres, sqlserver, oracle

### IsValid(this CommandLineArguments? value)
Determines whether the specified `CommandLineArguments` instance is valid.

- **Parameters**
  - `value`: The `CommandLineArguments` instance to check.
- **Returns**
  - `True` if the instance is valid; otherwise, `false`.
- **Exceptions**
  - `ArgumentNullException`: Thrown if `value` is null.
- **Remarks**
  - This method returns `false` if validation fails, but does not throw exceptions.
  - Use `EnsureValid` to throw an exception on validation failure.

### EnsureValid(this CommandLineArguments? value)
Validates the specified `CommandLineArguments` instance and throws an `ArgumentException` if validation fails.

- **Parameters**
  - `value`: The `CommandLineArguments` instance to validate.
- **Exceptions**
  - `ArgumentNullException`: Thrown if `value` is null.
  - `ArgumentException`: Thrown if validation fails, containing a list of all validation problems.
- **Remarks**
  - This method calls `Validate` and throws an `ArgumentException` with a formatted message
    containing all validation problems if any are found.

## Example
```csharp
using SqlQueryAnalyzer.CLI;

// Assuming args is an instance of CommandLineArguments
var args = new CommandLineArguments 
{ 
    Query = "SELECT * FROM Users",
    OutputFormat = "json",
    ThreadCount = 4
};

// Validate and get list of problems
IReadOnlyList<string> problems = args.Validate();
if (problems.Count > 0)
{
    foreach (var problem in problems)
    {
        Console.WriteLine($"Validation problem: {problem}");
    }
}

// Check if args is valid
bool isValid = args.IsValid();
if (!isValid)
{
    // Handle invalid arguments
}

// Ensure args is valid (throws exception if invalid)
try
{
    args.EnsureValid();
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Command line arguments validation failed: {ex.Message}");
}
```