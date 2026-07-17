# SqlInjectionDetectorJsonExtensions

Provides JSON serialization and deserialization helpers for the `SqlInjectionDetector` type, enabling easy persistence or transmission of detector state as JSON strings.

## API

### `public static string ToJson(this SqlInjectionDetector detector)`
Serializes a `SqlInjectionDetector` instance to a JSON string.

- **Parameters**  
  - `detector`: The detector to serialize. Must not be `null`.

- **Return value**  
  A JSON‑encoded string representing the detector.

- **Exceptions**  
  - `ArgumentNullException` if `detector` is `null`.  
  - `JsonException` if serialization fails (e.g., due to unsupported member types).

### `public static string ToJson(this SqlInjectionDetector detector, JsonSerializerOptions options)`
Serializes a `SqlInjectionDetector` instance to a JSON string using the supplied `JsonSerializerOptions`.

- **Parameters**  
  - `detector`: The detector to serialize. Must not be `null`.  
  - `options`: Serialization options that control formatting, encoding, etc. May be `null` to use defaults.

- **Return value**  
  A JSON‑encoded string representing the detector.

- **Exceptions**  
  - `ArgumentNullException` if `detector` is `null`.  
  - `JsonException` if serialization fails.

### `public static SqlInjectionDetector? FromJson(this string json)`
Deserializes a JSON string into a `SqlInjectionDetector` instance.

- **Parameters**  
  - `json`: The JSON string to parse. Must not be `null`.

- **Return value**  
  The deserialized `SqlInjectionDetector`, or `null` if the JSON represents a null value.

- **Exceptions**  
  - `ArgumentNullException` if `json` is `null`.  
  - `JsonException` if the JSON is malformed or does not correspond to a `SqlInjectionDetector`.

### `public static SqlInjectionDetector? FromJson(this string json, JsonSerializerOptions options)`
Deserializes a JSON string into a `SqlInjectionDetector` instance using the supplied `JsonSerializerOptions`.

- **Parameters**  
  - `json`: The JSON string to parse. Must not be `null`.  
  - `options`: Deserialization options that control property naming, type handling, etc. May be `null` to use defaults.

- **Return value**  
  The deserialized `SqlInjectionDetector`, or `null` if the JSON represents a null value.

- **Exceptions**  
  - `ArgumentNullException` if `json` is `null`.  
  - `JsonException` if the JSON is malformed or does not correspond to a `SqlInjectionDetector`.

### `public static SqlInjectionIssue? FromJsonToSqlInjectionIssue(this string json)`
Deserializes a JSON string into a `SqlInjectionIssue` instance (the issue representation produced by the detector).

- **Parameters**  
  - `json`: The JSON string to parse. Must not be `null`.

- **Return value**  
  The deserialized `SqlInjectionIssue`, or `null` if the JSON represents a null value.

- **Exceptions**  
  - `ArgumentNullException` if `json` is `null`.  
  - `JsonException` if the JSON is malformed or does not correspond to a `SqlInjectionIssue`.

### `public static bool TryFromJson(this string json, out SqlInjectionDetector? detector)`
Attempts to deserialize a JSON string into a `SqlInjectionDetector` without throwing exceptions on failure.

- **Parameters**  
  - `json`: The JSON string to parse. Must not be `null`.  
  - `detector`: When the method returns `true`, contains the deserialized detector; otherwise `null`.

- **Return value**  
  `true` if `json` was successfully deserialized; otherwise `false`.

- **Exceptions**  
  - `ArgumentNullException` if `json` is `null`.

### `public static bool TryFromJson(this string json, JsonSerializerOptions options, out SqlInjectionDetector? detector)`
Attempts to deserialize a JSON string into a `SqlInjectionDetector` using the supplied options, without throwing exceptions on failure.

- **Parameters**  
  - `json`: The JSON string to parse. Must not be `null`.  
  - `options`: Deserialization options that control property naming, type handling, etc. May be `null` to use defaults.  
  - `detector`: When the method returns `true`, contains the deserialized detector; otherwise `null`.

- **Return value**  
  `true` if `json` was successfully deserialized; otherwise `false`.

- **Exceptions**  
  - `ArgumentNullException` if `json` is `null`.

## Usage

```csharp
using System.Text.Json;
using SqlQueryAnalyzer.Detection;

// Create a detector (example initialization)
var detector = new SqlInjectionDetector { /* configure properties */ };

// Serialize to JSON
string json = detector.ToJson();
// Or with custom options
var options = new JsonSerializerOptions { WriteIndented = true };
string prettyJson = detector.ToJson(options);

// Deserialize from JSON
SqlInjectionDetector? restored = json.FromJson();
// Or with options
SqlInjectionDetector? restoredWithOptions = json.FromJson(options);

// Safe deserialization that does not throw
if (json.TryFromJson(out SqlInjectionDetector? safeDetect))
{
    // Use safeDetect
}
else
{
    // Handle invalid JSON
}
```

```csharp
using System.Text.Json;
using SqlQueryAnalyzer.Detection;

// Suppose you have JSON that represents a SqlInjectionIssue
string issueJson = @"{ ""Message"": ""Potential injection detected"", ""Severity"": ""High"" }";

// Convert directly to SqlInjectionIssue
SqlInjectionIssue? issue = issueJson.FromJsonToSqlInjectionIssue();

// Try‑parse pattern for issue (if such overload existed, similar to detector)
// bool success = issueJson.TryFromJson(out SqlInjectionIssue? issue);
```

## Notes

- All extension methods are **static** and contain no mutable state; they are thread‑safe and can be invoked concurrently from multiple threads.
- Passing `null` for the input JSON string or the detector instance results in an `ArgumentNullException` for the non‑try variants; the `TryFromJson` variants return `false` and set the output parameter to `null` in those cases (except for the null‑check itself, which still throws).
- Invalid JSON payloads cause a `JsonException` in the throwing variants; the try variants simply return `false`.
- The methods rely on `System.Text.Json`; therefore, any custom converters or polymorphic types used in `SqlInjectionDetector` or `SqlInjectionIssue` must be registered via `JsonSerializerOptions` if the default behavior is insufficient.
- The `FromJsonToSqlInjectionIssue` helper is provided for scenarios where the serialized form represents only the issue data rather than the full detector state. It follows the same null‑ and exception‑handling rules as the detector‑focused overloads.
