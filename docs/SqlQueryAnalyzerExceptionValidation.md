# SqlQueryAnalyzerExceptionValidation

Provides validation helpers for `SqlQueryAnalyzerException` and its derived types, ensuring exception instances meet required constraints before use.

## Members

### Validate(this SqlQueryAnalyzerException value)
Validates an exception instance and returns a list of human-readable validation problems.

- **Parameters**:
  - `value`: The exception to validate.
- **Returns**: An enumerable of validation problems; empty if the exception is valid.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `value` is null.

### IsValid(this SqlQueryAnalyzerException value)
Determines whether an exception instance is valid.

- **Parameters**:
  - `value`: The exception to check.
- **Returns**: True if the exception is valid; otherwise, false.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `value` is null.

### EnsureValid(this SqlQueryAnalyzerException value)
Ensures that an exception instance is valid, throwing an `ArgumentException` with a detailed message if it is not.

- **Parameters**:
  - `value`: The exception to validate.
- **Exceptions**:
  - `ArgumentException`: Thrown when `value` is invalid.
  - `ArgumentNullException`: Thrown when `value` is null.

## Usage Example
```csharp
using System;
using System.Collections.Generic;
using SqlQueryAnalyzer.Exceptions;

public class ExceptionValidator
{
    public void ValidateExceptions()
    {
        // Create an invalid exception (missing query)
        var invalidException = new InvalidQueryException("Query is invalid");
        
        // Check if valid
        bool isValid = invalidException.IsValid(); // Returns false
        
        // Get validation problems
        IReadOnlyList<string> problems = invalidException.Validate();
        // problems contains: "InvalidQueryException.Query must not be null, empty, or whitespace."
        
        // Throw if invalid (alternative approach)
        try
        {
            invalidException.EnsureValid();
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Validation failed: {ex.Message}");
            // Output: Validation failed: The exception is invalid:
            // InvalidQueryException.Query must not be null, empty, or whitespace.
        }
        
        // Create a valid exception
        var validException = new InvalidQueryException("Query is invalid", "SELECT * FROM", 1, 5);
        
        // Validate the valid exception
        bool validIsValid = validException.IsValid(); // Returns true
        IReadOnlyList<string> validProblems = validException.Validate(); // Empty list
    }
}
```