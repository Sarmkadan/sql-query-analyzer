# AnalysisRequestDtoExtensions

Extension methods for `AnalysisRequestDto` that provide analysis configuration and context utilities.

## API

### `GetContextIdentifier`
Gets a unique identifier for the analysis context derived from the request.

- **Parameters**
  - `AnalysisRequestDto request`: The request containing analysis parameters.
- **Returns**
  - `string`: A context identifier string.
- **Throws**
  - `ArgumentNullException`: If `request` is `null`.

### `ShouldAnalyzePlan`
Determines whether the query plan should be analyzed based on the request configuration.

- **Parameters**
  - `AnalysisRequestDto request`: The request containing analysis parameters.
- **Returns**
  - `bool`: `true` if plan analysis is enabled; otherwise, `false`.
- **Throws**
  - `ArgumentNullException`: If `request` is `null`.

### `ShouldIncludeIndexSuggestions`
Determines whether index suggestions should be included in the analysis results.

- **Parameters**
  - `AnalysisRequestDto request`: The request containing analysis parameters.
- **Returns**
  - `bool`: `true` if index suggestions are enabled; otherwise, `false`.
- **Throws**
  - `ArgumentNullException`: If `request` is `null`.

### `ShouldAnalyzeFragmentation`
Determines whether index fragmentation analysis should be performed.

- **Parameters**
  - `AnalysisRequestDto request`: The request containing analysis parameters.
- **Returns**
  - `bool`: `true` if fragmentation analysis is enabled; otherwise, `false`.
- **Throws**
  - `ArgumentNullException`: If `request` is `null`.

### `GetContextDisplayName`
Gets a human-readable display name for the analysis context derived from the request.

- **Parameters**
  - `AnalysisRequestDto request`: The request containing analysis parameters.
- **Returns**
  - `string`: A display name for the context.
- **Throws**
  - `ArgumentNullException`: If `request` is `null`.

### `GetConfigurationSummary`
Generates a summary of the analysis configuration from the request.

- **Parameters**
  - `AnalysisRequestDto request`: The request containing analysis parameters.
- **Returns**
  - `string`: A summary of the configuration.
- **Throws**
  - `ArgumentNullException`: If `request` is `null`.

### `Clone`
Creates a deep copy of the `AnalysisRequestDto` with all properties copied.

- **Parameters**
  - `AnalysisRequestDto request`: The request to clone.
- **Returns**
  - `AnalysisRequestDto`: A new instance with identical property values.
- **Throws**
  - `ArgumentNullException`: If `request` is `null`.

## Usage

```csharp
// Example 1: Checking analysis flags before processing
var request = new AnalysisRequestDto
{
    AnalyzePlan = true,
    IncludeIndexSuggestions = true,
    AnalyzeFragmentation = false
};

if (AnalysisRequestDtoExtensions.ShouldAnalyzePlan(request))
{
    Console.WriteLine("Plan analysis is enabled.");
}

if (AnalysisRequestDtoExtensions.ShouldIncludeIndexSuggestions(request))
{
    Console.WriteLine("Index suggestions will be generated.");
}
```

```csharp
// Example 2: Cloning a request for modification
var original = new AnalysisRequestDto
{
    ContextIdentifier = "query-123",
    AnalyzePlan = true
};

var modified = AnalysisRequestDtoExtensions.Clone(original);
modified.AnalyzePlan = false; // Modify the clone without affecting the original
```

## Notes

- All methods validate parameters and throw `ArgumentNullException` if `request` is `null`.
- The `Clone` method performs a deep copy, ensuring that mutable properties are not shared between the original and cloned instances.
- These methods are thread-safe for read-only access to the `AnalysisRequestDto` instance. If the instance is modified concurrently, behavior is undefined.
