# AnalysisRequestDtoValidation

Provides validation methods for `AnalysisRequestDto` instances.

## Purpose

This static class contains extension methods for validating `AnalysisRequestDto` objects, ensuring they meet the required constraints before processing. It offers three validation approaches:
- `Validate()`: Returns a list of error messages
- `IsValid()`: Returns a boolean indicating validity
- `EnsureValid()`: Throws an exception if invalid

## Members

### Validate(this AnalysisRequestDto? value)

Validates the specified analysis request DTO.

**Parameters:**
- `value`: The analysis request to validate.

**Returns:**
- An `IReadOnlyList<string>` containing validation error messages. Empty if the DTO is valid.

**Exceptions:**
- `ArgumentNullException`: Thrown when `value` is null.

**Validation Rules:**
- `QueryText` is required and cannot be empty or whitespace (max 1,000,000 characters)
- `ApplicationName` is optional but limited to 256 characters if provided
- `ProcedureName` is optional but limited to 256 characters if provided
- `ModuleName` is optional but limited to 256 characters if provided
- `ExecutionPlanXml` is optional but limited to 10,000,000 characters if provided

### IsValid(this AnalysisRequestDto? value)

Determines whether the specified analysis request DTO is valid.

**Parameters:**
- `value`: The analysis request to check.

**Returns:**
- `true` if the request is valid; otherwise `false`.

**Exceptions:**
- `ArgumentNullException`: Thrown when `value` is null.

### EnsureValid(this AnalysisRequestDto? value)

Ensures that the specified analysis request DTO is valid.

**Parameters:**
- `value`: The analysis request to validate.

**Exceptions:**
- `ArgumentNullException`: Thrown when `value` is null.
- `ArgumentException`: Thrown when the request is invalid, containing validation error messages in the message.

## Usage Example

```csharp
using SqlQueryAnalyzer.DTOs;

// Create a DTO to validate
var request = new AnalysisRequestDto
{
    QueryText = "SELECT * FROM Users WHERE Id = 1",
    ApplicationName = "MyApp",
    ProcedureName = "GetUserDetails",
    ModuleName = "UserManagement"
};

// Validate and get error messages
var errors = request.Validate();
if (errors.Count > 0)
{
    foreach (var error in errors)
    {
        Console.WriteLine($"Validation error: {error}");
    }
}

// Check if valid
bool isValid = request.IsValid();

// Throw exception if invalid (recommended for API validation)
request.EnsureValid(); // Throws ArgumentException if invalid
```

## Notes

- All string validations use `string.IsNullOrWhiteSpace()` for required fields and `string.IsNullOrEmpty()` for optional fields
- Length limits are designed to prevent excessive memory usage and potential DoS attacks
- The validation methods are implemented as extension methods for fluent usage
- Null checking is performed automatically by all methods