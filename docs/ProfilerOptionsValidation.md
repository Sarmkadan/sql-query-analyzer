# ProfilerOptionsValidation

Provides validation helpers for `ProfilerOptions` instances.

## Purpose

This static class contains validation methods for the `ProfilerOptions` model, allowing to:
- Validate an instance and get a list of error messages.
- Check if an instance is valid.
- Ensure an instance is valid, throwing an exception if not.

## Members

### Validate(this ProfilerOptions value)

Validates the specified `ProfilerOptions` instance and returns a list of human-readable problems.

**Parameters**
- `value`: The options instance to validate.

**Returns**
- An empty list if the instance is valid; otherwise, a list of validation error messages.

**Exceptions**
- `ArgumentNullException`: Thrown when `value` is `null`.

**Example**
```csharp
var options = new ProfilerOptions { MaxDurationMs = -1 };
var errors = options.Validate(); // Returns ["MaxDurationMs must be positive, but was -1."]
```

### IsValid(this ProfilerOptions value)

Determines whether the specified `ProfilerOptions` instance is valid.

**Parameters**
- `value`: The options instance to check.

**Returns**
- `true` if the instance is valid; otherwise, `false`.

**Example**
```csharp
var options = new ProfilerOptions { MaxDurationMs = 1000 };
bool isValid = options.IsValid(); // Returns true
```

### EnsureValid(this ProfilerOptions value)

Ensures that the specified `ProfilerOptions` instance is valid, throwing an `ArgumentException` with a detailed message listing all validation problems if it is not.

**Parameters**
- `value`: The options instance to validate.

**Exceptions**
- `ArgumentNullException`: Thrown when `value` is `null`.
- `ArgumentException`: Thrown when the instance is invalid, containing a list of all validation problems.

**Example**
```csharp
var options = new ProfilerOptions { MaxDurationMs = -1 };
options.EnsureValid(); // Throws ArgumentException with message:
// "ProfilerOptions is invalid. Problems:
// 
// - MaxDurationMs must be positive, but was -1."
```