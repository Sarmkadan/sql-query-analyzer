# QueryProfilerExtensionsJsonExtensions

Provides System.Text.Json serialization extensions for query profiler types. Enables serialization and deserialization of `QueryProfilerReport`, `ProfileComparison`, `ProfilerBatchSummary`, and collections of `QueryProfilerReport` to and from JSON strings with camelCase property naming and null value handling.

## API

### ToJson(this QueryProfilerReport value, bool indented = false)

Serializes a `QueryProfilerReport` to a JSON string.

- **Parameters:**
  - `value`: The profiler report to serialize.
  - `indented`: Whether to format the JSON with indentation for readability.
- **Returns:** A JSON string representation of the profiler report.
- **Exceptions:**
  - Throws `ArgumentNullException` when `value` is `null`.

### FromJson(string json)

Deserializes a JSON string into a `QueryProfilerReport`.

- **Parameters:**
  - `json`: The JSON string to deserialize.
- **Returns:** A profiler report, or `null` if the JSON is empty or whitespace.
- **Exceptions:**
  - Throws `ArgumentException` when `json` is `null`, empty, or consists only of whitespace.
  - Throws `JsonException` when the JSON is invalid or cannot be deserialized.

### TryFromJson(string json, out QueryProfilerReport? value)

Attempts to deserialize a JSON string into a `QueryProfilerReport`.

- **Parameters:**
  - `json`: The JSON string to deserialize.
  - `value`: Receives the deserialized report if successful; otherwise, `null`.
- **Returns:** `true` if deserialization succeeds; otherwise, `false`.
- **Exceptions:**
  - Throws `ArgumentException` when `json` is `null` or empty.

### ToJson(this ProfileComparison value, bool indented = false)

Serializes a `ProfileComparison` to a JSON string.

- **Parameters:**
  - `value`: The profile comparison to serialize.
  - `indented`: Whether to format the JSON with indentation for readability.
- **Returns:** A JSON string representation of the profile comparison.
- **Exceptions:**
  - Throws `ArgumentNullException` when `value` is `null`.

### FromJsonToProfileComparison(string json)

Deserializes a JSON string into a `ProfileComparison`.

- **Parameters:**
  - `json`: The JSON string to deserialize.
- **Returns:** A profile comparison, or `null` if the JSON is empty or whitespace.
- **Exceptions:**
  - Throws `ArgumentException` when `json` is `null`, empty, or consists only of whitespace.
  - Throws `JsonException` when the JSON is invalid or cannot be deserialized.

### TryFromJsonToProfileComparison(string json, out ProfileComparison? value)

Attempts to deserialize a JSON string into a `ProfileComparison`.

- **Parameters:**
  - `json`: The JSON string to deserialize.
  - `value`: Receives the deserialized comparison if successful; otherwise, `null`.
- **Returns:** `true` if deserialization succeeds; otherwise, `false`.
- **Exceptions:**
  - Throws `ArgumentException` when `json` is `null` or empty.

### ToJson(this IEnumerable<QueryProfilerReport> value, bool indented = false)

Serializes a collection of `QueryProfilerReport` to a JSON string.

- **Parameters:**
  - `value`: The collection of profiler reports to serialize.
  - `indented`: Whether to format the JSON with indentation for readability.
- **Returns:** A JSON string representation of the profiler reports.
- **Exceptions:**
  - Throws `ArgumentNullException` when `value` is `null`.

### FromJsonToReports(string json)

Deserializes a JSON string into a collection of `QueryProfilerReport`.

- **Parameters:**
  - `json`: The JSON string to deserialize.
- **Returns:** A collection of profiler reports, or `null` if the JSON is empty or whitespace.
- **Exceptions:**
  - Throws `ArgumentException` when `json` is `null`, empty, or consists only of whitespace.
  - Throws `JsonException` when the JSON is invalid or cannot be deserialized.

### TryFromJsonToReports(string json, out IEnumerable<QueryProfilerReport>? value)

Attempts to deserialize a JSON string into a collection of `QueryProfilerReport`.

- **Parameters:**
  - `json`: The JSON string to deserialize.
  - `value`: Receives the deserialized reports if successful; otherwise, `null`.
- **Returns:** `true` if deserialization succeeds; otherwise, `false`.
- **Exceptions:**
  - Throws `ArgumentException` when `json` is `null` or empty.

### ToJson(this ProfilerBatchSummary value, bool indented = false)

Serializes a `ProfilerBatchSummary` to a JSON string.

- **Parameters:**
  - `value`: The batch summary to serialize.
  - `indented`: Whether to format the JSON with indentation for readability.
- **Returns:** A JSON string representation of the batch summary.
- **Exceptions:**
  - Throws `ArgumentNullException` when `value` is `null`.

### FromJsonToBatchSummary(string json)

Deserializes a JSON string into a `ProfilerBatchSummary`.

- **Parameters:**
  - `json`: The JSON string to deserialize.
- **Returns:** A batch summary, or `null` if the JSON is empty or whitespace.
- **Exceptions:**
  - Throws `ArgumentException` when `json` is `null`, empty, or consists only of whitespace.
  - Throws `JsonException` when the JSON is invalid or cannot be deserialized.

### TryFromJsonToBatchSummary(string json, out ProfilerBatchSummary? value)

Attempts to deserialize a JSON string into a `ProfilerBatchSummary`.

- **Parameters:**
  - `json`: The JSON string to deserialize.
  - `value`: Receives the deserialized batch summary if successful; otherwise, `null`.
- **Returns:** `true` if deserialization succeeds; otherwise, `false`.
- **Exceptions:**
  - Throws `ArgumentException` when `json` is `null` or empty.

## Usage

### Serializing and deserializing a QueryProfilerReport

```csharp
using SqlQueryAnalyzer.Extensions;
using SqlQueryAnalyzer.Models;

var report = new QueryProfilerReport
{
    Query = "SELECT * FROM Users WHERE Id = @id",
    ExecutionTimeMs = 12.5,
    CpuTimeMs = 8.2,
    Reads = 42,
    Writes = 0,
    DurationMs = 15.3
};

// Serialize to compact JSON
string json = report.ToJson();
Console.WriteLine(json);

// Deserialize back
var deserialized = QueryProfilerExtensionsJsonExtensions.FromJson(json);
Console.WriteLine(deserialized?.ExecutionTimeMs); // 12.5
```

### Working with collections of reports

```csharp
using SqlQueryAnalyzer.Extensions;
using SqlQueryAnalyzer.Models;

var reports = new List<QueryProfilerReport>
{
    new QueryProfilerReport { Query = "SELECT * FROM Users", ExecutionTimeMs = 10.1 },
    new QueryProfilerReport { Query = "SELECT * FROM Orders", ExecutionTimeMs = 8.7 }
};

// Serialize collection
string json = reports.ToJson(indented: true);
Console.WriteLine(json);

// Deserialize collection
var deserializedReports = QueryProfilerExtensionsJsonExtensions.FromJsonToReports(json);
foreach (var r in deserializedReports ?? Enumerable.Empty<QueryProfilerReport>())
{
    Console.WriteLine($"{r.Query}: {r.ExecutionTimeMs}ms");
}
```

## Notes

- All serialization methods use camelCase property naming via `JsonNamingPolicy.CamelCase`.
- Null values are omitted from serialized JSON due to `DefaultIgnoreCondition.WhenWritingNull`.
- The shared `_jsonOptions` instance uses `ReferenceHandler.IgnoreCycles` to prevent infinite loops during serialization of object graphs.
- Deserialization methods return `null` for empty or whitespace JSON strings rather than throwing.
- Try methods return `false` for invalid JSON rather than throwing exceptions, making them suitable for defensive programming scenarios.
- Serialization methods throw `ArgumentNullException` for `null` input values, enforcing non-null constraints at the API boundary.
- The `indented` parameter controls JSON formatting but does not affect the underlying serialization behavior or options.
- All methods are thread-safe as they only read from shared, immutable `JsonSerializerOptions` and perform no shared state mutation.
- Deserialization failures due to JSON structure mismatches throw `JsonException`, allowing callers to handle type-specific errors explicitly.