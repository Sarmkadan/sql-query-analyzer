# DtoMapperValidation

Provides validation methods for `QueryDetailDto` instances.

## Purpose

This static class contains extension methods for validating `QueryDetailDto` objects, ensuring they meet the required constraints before processing. It offers three validation approaches:
- `Validate()`: Returns a list of error messages
- `IsValid()`: Returns a boolean indicating validity
- `EnsureValid()`: Throws an exception if invalid

## Members

### Validate(this QueryDetailDto? value)

Validates the specified query detail DTO.

**Parameters:**
- `value`: The query detail DTO to validate.

**Returns:**
- An `IReadOnlyList<string>` containing validation error messages. Empty if the DTO is valid.

**Exceptions:**
- `ArgumentNullException`: Thrown when `value` is null.

**Validation Rules:**
- `QueryId` is required and cannot be null or whitespace
- `QueryText` is required and cannot be null or whitespace
- `QueryType` is required and must be one of: SELECT, INSERT, UPDATE, DELETE, MERGE (case-insensitive)
- `TableCount` must be non-negative
- `Tables` collection must not be null and its count must match `TableCount`
- Each item in `Tables` collection must not be null or whitespace
- `JoinCount` must be non-negative
- `ParameterCount` must be non-negative
- `LineCount` must be non-negative

### IsValid(this QueryDetailDto? value)

Determines whether the specified query detail DTO is valid.

**Parameters:**
- `value`: The query detail DTO to check.

**Returns:**
- `true` if the DTO is valid; otherwise `false`.

**Exceptions:**
- `ArgumentNullException`: Thrown when `value` is null.

### EnsureValid(this QueryDetailDto? value)

Ensures that the specified query detail DTO is valid.

**Parameters:**
- `value`: The query detail DTO to validate.

**Exceptions:**
- `ArgumentNullException`: Thrown when `value` is null.
- `ArgumentException`: Thrown when the DTO is invalid, containing validation error messages in the message.

## Usage Example

```csharp
using SqlQueryAnalyzer.DTOs;

// Create a DTO to validate
var queryDetail = new QueryDetailDto
{
    QueryId = "q123",
    QueryText = "SELECT * FROM Users WHERE Id = @Id",
    QueryType = "SELECT",
    TableCount = 1,
    Tables = new List<string> { "Users" },
    JoinCount = 0,
    ParameterCount = 1,
    LineCount = 1
};

// Validate and get error messages
var errors = queryDetail.Validate();
if (errors.Count > 0)
{
    foreach (var error in errors)
    {
        Console.WriteLine($"Validation error: {error}");
    }
}

// Check if valid
bool isValid = queryDetail.IsValid();

// Throw exception if invalid (recommended for API validation)
queryDetail.EnsureValid(); // Throws ArgumentException if invalid
```

## Notes

- All string validations use `string.IsNullOrWhiteSpace()` for required fields
- Query type validation is case-insensitive and accepts: SELECT, INSERT, UPDATE, DELETE, MERGE
- The validation methods are implemented as extension methods for fluent usage
- Null checking is performed automatically by all methods
- Collection validations ensure referential integrity between `TableCount` and `Tables` collection