# AnalysisControllerExtensionsJsonExtensions

## Purpose
Provides JSON serialization and deserialization extensions for analysis controller responses.

## Public Members

### `ToJson<T>(this T value, bool indented = false)`
Serializes the specified value to a JSON string using camelCase property naming.

- **Type Parameters**
  - `T`: The type of the value to serialize.
- **Parameters**
  - `value`: The value to serialize.
  - `indented` (optional): Whether to format the JSON with indentation for readability. Defaults to `false`.
- **Returns**
  - A JSON string representation of the value.
- **Exceptions**
  - `ArgumentNullException`: Thrown when `value` is `null`.

### `FromJson<T>(this string json)`
Deserializes a JSON string into an instance of the specified type.

- **Type Parameters**
  - `T`: The type to deserialize into.
- **Parameters**
  - `json`: The JSON string to deserialize.
- **Returns**
  - An instance of type `T`.
- **Exceptions**
  - `ArgumentException`: Thrown when `json` is `null`, empty, or whitespace.
  - `JsonException`: Thrown when the JSON is invalid or cannot be deserialized into type `T`.

### `TryFromJson<T>(this string json, out T? value)`
Attempts to deserialize a JSON string into an instance of the specified type.

- **Type Parameters**
  - `T`: The type to deserialize into.
- **Parameters**
  - `json`: The JSON string to deserialize.
  - `value`: Receives the deserialized value if successful.
- **Returns**
  - `true` if deserialization succeeds; otherwise, `false`.
- **Exceptions**
  - `ArgumentException`: Thrown when `json` is `null`, empty, or whitespace.

## Usage Example

```csharp
using SqlQueryAnalyzer.API;

// Example model to serialize
public class AnalysisResult
{
    public string Query { get; set; } = string.Empty;
    public int RowCount { get; set; }
    public double ExecutionTimeMs { get; set; }
}

// Usage
var result = new AnalysisResult
{
    Query = "SELECT * FROM Users",
    RowCount = 42,
    ExecutionTimeMs = 15.5
};

// Serialize to JSON (camelCase, no indentation by default)
string json = result.ToJson();
// Output: {"query":"SELECT * FROM Users","rowCount":42,"executionTimeMs":15.5}

// Serialize with indentation for readability
string indentedJson = result.ToJson(indented: true);
/* Output:
{
  "query": "SELECT * FROM Users",
  "rowCount": 42,
  "executionTimeMs": 15.5
}
*/

// Deserialize from JSON
AnalysisResult? parsedResult = json.FromJson<AnalysisResult>();

// Safe deserialization that doesn't throw exceptions
if (json.TryFromJson<AnalysisResult>(out var safeResult))
{
    // Use safeResult
}
```