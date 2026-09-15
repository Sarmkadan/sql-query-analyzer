# QueryAnalysisCacheValidation

## Purpose
Provides validation helpers for `QueryAnalysisCache` instances. Validates cache state, statistics, and entry data for correctness and consistency.

## Members

### Validate(this QueryAnalysisCache? value)
Validates the cache instance and returns a list of human-readable problems.

- **Parameters**
  - `value`: The cache instance to validate.
- **Returns**
  - An `IReadOnlyList<string>` containing validation problems; empty if valid.
- **Exceptions**
  - `ArgumentNullException`: Thrown if `value` is null.
- **Remarks**
  - Delegates all validation logic to `ValidateCacheStatistics` which handles comprehensive validation of cache statistics including entry counts, hit rate, access patterns, and age metrics.

### IsValid(this QueryAnalysisCache? value)
Determines whether the cache instance is valid.

- **Parameters**
  - `value`: The cache instance to check.
- **Returns**
  - `True` if valid; otherwise, `false`.
- **Exceptions**
  - `ArgumentNullException`: Thrown if `value` is null.

### EnsureValid(this QueryAnalysisCache? value)
Ensures the cache instance is valid, throwing an `ArgumentException` with details if any validation problems are found.

- **Parameters**
  - `value`: The cache instance to validate.
- **Exceptions**
  - `ArgumentException`: Thrown if `value` is invalid.
  - `ArgumentNullException`: Thrown if `value` is null.
- **Remarks**
  - Throws an exception with a detailed message listing all validation problems when the cache fails validation.

## Example
```csharp
using SqlQueryAnalyzer.Caching;

// Assuming cache is an instance of QueryAnalysisCache
var cache = new QueryAnalysisCache();

// Validate and get list of problems
IReadOnlyList<string> problems = cache.Validate();
if (problems.Count > 0)
{
    foreach (var problem in problems)
    {
        Console.WriteLine($"Validation problem: {problem}");
    }
}

// Check if cache is valid
bool isValid = cache.IsValid();
if (!isValid)
{
    // Handle invalid cache
}

// Ensure cache is valid (throws exception if invalid)
try
{
    cache.EnsureValid();
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Cache validation failed: {ex.Message}");
}
```