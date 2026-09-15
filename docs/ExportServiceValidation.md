# ExportServiceValidation

## Purpose
Provides validation helpers for `ExportService` instances, ensuring proper configuration and state before use.

## Members

### Validate(value)
Validates an `ExportService` instance.

- **Parameters**:
  - `value`: The export service instance to validate.
- **Returns**: A list of validation errors; empty if valid.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `value` is null.

### IsValid(value)
Determines whether an `ExportService` instance is valid.

- **Parameters**:
  - `value`: The export service instance to check.
- **Returns**: `true` if the instance is valid; otherwise, `false`.

### EnsureValid(value)
Ensures that an `ExportService` instance is valid, throwing an exception if not.

- **Parameters**:
  - `value`: The export service instance to validate.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `value` is null.
  - `ArgumentException`: Thrown when the instance is not valid, containing validation errors.

## Usage Example
```csharp
using System;
using SqlQueryAnalyzer.Export;

public class ExportServiceValidator
{
    public void ValidateService(ExportService service)
    {
        // Quick validation check
        if (!service.IsValid())
        {
            Console.WriteLine("Export service is not valid");
            var errors = service.Validate();
            foreach (var error in errors)
            {
                Console.WriteLine($"Validation error: {error}");
            }
            return;
        }

        // Ensuring validity with exception on failure
        try
        {
            service.EnsureValid();
            Console.WriteLine("Export service is valid and ready for use");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Export service validation failed: {ex.Message}");
        }
    }
}
```