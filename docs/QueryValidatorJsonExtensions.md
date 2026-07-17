# QueryValidatorJsonExtensions

Provides extension methods for serializing and deserializing `QueryAnalysisResult`, `PerformanceIssue`, and `IndexSuggestion` objects to and from JSON. This class centralizes JSON conversion logic for the core analysis types, ensuring consistent formatting and error handling across the application.

## API

### QueryAnalysisResult Serialization

#### `ToJson(this QueryAnalysisResult result)`
Serializes a `QueryAnalysisResult` instance to its JSON string representation.

| Parameter | Type | Description |
|-----------|------|-------------|
| `result` | `QueryAnalysisResult` | The analysis result to serialize. |

**Returns:** `string` — A JSON string representing the analysis result.

**Throws:** `ArgumentNullException` when `result` is `null`.

---

#### `FromJsonToAnalysisResult(this string json)`
Deserializes a JSON string into a `QueryAnalysisResult` instance.

| Parameter | Type | Description |
|-----------|------|-------------|
| `json` | `string` | The JSON string to deserialize. |

**Returns:** `QueryAnalysisResult` — The deserialized analysis result.

**Throws:** `ArgumentNullException` when `json` is `null`. `JsonException` when the JSON is malformed or cannot be mapped to the target type.

---

#### `TryFromJson(this string json, out QueryAnalysisResult result)`
Attempts to deserialize a JSON string into a `QueryAnalysisResult` without throwing on failure.

| Parameter | Type | Description |
|-----------|------|-------------|
| `json` | `string` | The JSON string to deserialize. |
| `result` | `out QueryAnalysisResult?` | When successful, contains the deserialized object; otherwise `null`. |

**Returns:** `bool` — `true` if deserialization succeeded; `false` otherwise.

**Throws:** Does not throw. Returns `false` for `null` input, empty strings, or invalid JSON.

---

### `PerformanceIssue` Serialization

#### `ToJson(this PerformanceIssue issue)`
Serializes a `PerformanceIssue` instance to its JSON string representation.

| Parameter | Type | Description |
|-----------|------|-------------|
| `issue` | `PerformanceIssue` | The performance issue to serialize. |

**Returns:** `string` — A JSON representation of the performance issue.

**Throws:** `ArgumentNullException` when `issue` is `null`.

---

#### `FromJsonToPerformanceIssue(string json)`
Parses a JSON string into a `PerformanceIssue` instance.

| Parameter | Type | Description |
|-----------|------|-------------|
| `json` | `string` | The JSON string to deserialize. |

**Returns:** `PerformanceIssue` — The deserialized performance issue.

**Throws:** `ArgumentNullException` when `json` is `null`. `JsonException` when the JSON is malformed or incompatible.

---

#### `TryFromJson(this string json, out PerformanceIssue? issue)`
Attempts to deserialize a JSON string into a `PerformanceIssue` without throwing.

| Parameter | Type | Description |
|-----------|------|-------------|
| `json` | `string` | The JSON string to deserialize. |
| `issue` | `out PerformanceIssue?` | On success, the deserialized object; otherwise `null`. |

**Returns:** `bool` — `true` if deserialization succeeded; `false` otherwise.

**Throws:** No exceptions. Returns `false` for `null` or invalid input.

---

### `IndexSuggestion` Serialization

#### `ToJson(this IndexSuggestion suggestion)`
Serializes an `IndexSuggestion` instance to its JSON string representation.

| Parameter | Type | Description |
|-----------|------|-------------|
| `suggestion` | `IndexSuggestion` | The index suggestion to serialize. |

**Returns:** `string` — A JSON representation of the index suggestion.

**Throws:** `ArgumentNullException` when `suggestion` is `null`.

---

#### `FromJsonToIndexSuggestion(string json)`
Parses a JSON string into an `IndexSuggestion` instance.

| Parameter | Type | Description |
|-----------|------|-------------|
| `json` | `string` | The JSON string to deserialize. |

**Returns:** `IndexSuggestion` — The deserialized index suggestion.

**Throws:** `ArgumentNullException` when `json` is `null`. `JsonException` when the JSON is malformed or malformed.

---

#### `TryFromJson(this string json, out IndexSuggestion? suggestion)`
Attempts to deserialize a JSON string into an `IndexSuggestion` without throwing.

| Parameter | Type | Description |
|-----------|------|-------------|
| `json` | `string` | The JSON string to deserialize. |
| `suggestion` | `out IndexSuggestion?` | On success, the deserialized object; otherwise `null`. |

**Returns:** `bool` — `true` if deserialization succeeded; `false` otherwise.

**Throws:** No exceptions.

---

## Usage

### Example 1: Round-Trip Serialization of an Analysis Result

```csharp
using SqlQueryAnalyzer;

// Obtain an analysis result from the analyzer
QueryAnalysisResult original = analyzer.Analyze("SELECT * FROM Orders WHERE CustomerId = 5");

// Serialize to JSON for storage or transmission
string json = original.ToJson();

// Deserialize back to an object
QueryAnalysisResult restored = json.FromJsonToAnalysisResult();

// Verify round-trip fidelity
Console.WriteLine($"Issues count: {restored.Issues.Count}");
```

### Example 2: Safe Deserialization with TryFromJson

```csharp
using SqlQueryAnalyzer;

string jsonFromFile = File.ReadAllText("cached_analysis.json");

// Attempt deserialization without risk of exceptions
if (jsonFromFile.TryFromJson(out QueryAnalysisResult? result) && result != null)
{
    foreach (var issue in result.Issues)
    {
        Console.WriteLine($"Severity: {issue.Severity}, Message: {issue.Description}");
    }
}
else
{
    Console.WriteLine("Failed to parse cached analysis. Re-running analysis...");
    // Fall back to re-analysis
}
```

## Notes

- **Null handling:** All `ToJson` methods throw `ArgumentNullException` when passed a `null` argument. The `FromJson` methods throw `ArgumentNullException` for `null` JSON strings. The `TryFromJson` methods return `false` for `null` input without throwing.
- **Empty and whitespace strings**: `TryFromJson` returns `false` for empty or whitespace-only strings. `FromJson` methods will throw `JsonException` in these cases.
- **Type mismatch**: If the JSON structure does not correspond to the expected type, `FromJson` throws `JsonException`. `TryFromJson` returns `false`.
- **Thread safety**: All methods are static and operate on immutable string inputs or produce new objects. They do not mutate shared state and are safe to call concurrently from multiple threads.
- **Round-trip fidelity**: Serialization via `ToJson` followed by the corresponding `FromJson` or `TryFromJson` produces an object that is semantically equivalent to the original, assuming no custom serialization settings interfere with the default contract.
- **Extension method syntax**: All `ToJson` and `TryFromJson` methods are designed to be called as extension methods on their respective types. The `FromJson` methods are static methods called on the string type.
