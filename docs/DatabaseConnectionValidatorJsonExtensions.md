# DatabaseConnectionValidatorJsonExtensions

Provides JSON serialization and deserialization extension methods for `DatabaseConnectionValidator`, `ConnectionValidationResult`, and `ConnectionTestResult` types. These methods enable converting instances to their JSON string representations and reconstructing them from JSON, with both exception-throwing and safe-try variants.

## API

### ToJson (DatabaseConnectionValidator)

```csharp
public static string ToJson(this DatabaseConnectionValidator validator)
```

Serializes a `DatabaseConnectionValidator` instance to its JSON string representation.

**Parameters:**
- `validator` — The `DatabaseConnectionValidator` instance to serialize.

**Return Value:**
A JSON string representing the validator.

**Exceptions:**
Throws `ArgumentNullException` if `validator` is `null`.

---

### FromJson (to DatabaseConnectionValidator)

```csharp
public static DatabaseConnectionValidator? FromJson(this string json)
```

Deserializes a JSON string into a `DatabaseConnectionValidator` instance.

**Parameters:**
- `json` — The JSON string to deserialize.

**Return Value:**
A `DatabaseConnectionValidator` instance, or `null` if the JSON is invalid or the string is `null`.

**Exceptions:**
Throws `System.Text.Json.JsonException` if the JSON is malformed and cannot be parsed at all.

---

### TryFromJson (DatabaseConnectionValidator)

```csharp
public static bool TryFromJson(this string json, out DatabaseConnectionValidator? validator)
```

Attempts to deserialize a JSON string into a `DatabaseConnectionValidator` without throwing on failure.

**Parameters:**
- `json` — The JSON string to deserialize.
- `validator` — When this method returns `true`, contains the deserialized `DatabaseConnectionValidator`; otherwise `null`.

**Return Value:**
`true` if deserialization succeeded; `false` otherwise.

**Exceptions:**
None. All parsing errors are caught internally.

---

### ToJson (ConnectionValidationResult)

```csharp
public static string ToJson(this ConnectionValidationResult result)
```

Serializes a `ConnectionValidationResult` instance to its JSON string representation.

**Parameters:**
- `result` — The `ConnectionValidationResult` instance to serialize.

**Return Value:**
A JSON string representing the validation result.

**Exceptions:**
- `System.ArgumentNullException` if `result` is `null`.

---

### FromJsonToConnectionValidationResult

```csharp
public static ConnectionValidationResult? FromJsonToConnectionValidationResult(this string json)
```

Deserializes a JSON string into a `ConnectionValidationResult` instance.

**Parameters:**
- `json` — The JSON string to deserialize.

**Return Value:**
A `ConnectionValidationResult` instance, or `null` if the JSON is invalid or the string is `null`.

**Exceptions:**
Throws `System.Text.Json.JsonException` if the JSON is malformed.

---

### TryFromJson (ConnectionValidationResult)

```csharp
public static bool TryFromJson(this string json, out ConnectionValidationResult? result)
```

Attempts to deserialize a JSON string into a `ConnectionValidationResult` without throwing on failure.

**Parameters:**
- `json` — The JSON string to deserialize.
- `result` — When this method returns `true`, contains the deserialized `ConnectionValidationResult`; otherwise `null`.

**Return Value:**
`true` if deserialization succeeded; `false` otherwise.

**Exceptions:**
None.

---

### ToJson (ConnectionTestResult)

```csharp
public static string ToJson(this ConnectionTestResult result)
```

Serializes a `ConnectionTestResult` instance to its JSON string representation.

**Parameters:**
- `result` — The `ConnectionTestResult` instance to serialize.

**Return Value:**
A JSON string representing the test result.

**Exceptions:**
- `System.ArgumentNullException` if `result` is `null`.

---

### FromJsonToConnectionTestResult

```csharp
public static ConnectionTestResult? FromJsonToConnectionTestResult(this string json)
```

Deserializes a JSON string into a `ConnectionTestResult` instance.

**Parameters:**
- `json` — The JSON string to deserialize.

**Return Value:**
A `ConnectionTestResult` instance, or `null` if the JSON is invalid or the string is `null`.

**Exceptions:**
Throws `System.Text.Json.JsonException` if the JSON is malformed.

---

### TryFromJson (ConnectionTestResult)

```csharp
public static bool TryFromJson(this string json, out ConnectionTestResult? result)
```

Attempts to deserialize a JSON string into a `ConnectionTestResult` without throwing on failure.

**Parameters:**
- `json` — The JSON string to deserialize.
- `result` — When this method returns `true`, contains the deserialized `ConnectionTestResult`; otherwise `null`.

**Return Value:**
`true` if deserialization succeeded; `false` otherwise.

**Exceptions:**
None.

## Usage

### Example 1: Round-tripping a ConnectionValidationResult

```csharp
var validator = new DatabaseConnectionValidator();
var validationResult = validator.Validate(connectionString);

// Serialize to JSON for logging or storage
string json = validationResult.ToJson();
Console.WriteLine(json);

// Deserialize later using safe try-pattern
if (json.TryFromJson(out ConnectionValidationResult? restored))
{
    Console.WriteLine($"Restored result: {restored.IsValid}");
}
else
{
    Console.WriteLine("Failed to parse validation result JSON.");
}
```

### Example 2: Persisting and restoring a ConnectionTestResult

```csharp
var testResult = new ConnectionTestResult
{
    Success = true,
    LatencyMs = 42,
    ServerVersion = "SQL Server 2022"
};

// Serialize to JSON
string json = testResult.ToJson();
File.WriteAllText("connection_test.json", json);

// Later: restore from file
string loadedJson = File.ReadAllText("connection_test.json");
ConnectionTestResult? restored = loadedJson.FromJsonToConnectionTestResult();

if (restored != null)
{
    Console.WriteLine($"Latency: {restored.LatencyMs}ms, Version: {restored.ServerVersion}");
}
```

## Notes

- All `ToJson` methods throw on `null` input; pass a valid object reference to avoid `ArgumentNullException`.
- The `FromJson` and `FromJsonTo*` methods return `null` for `null` or empty input strings, but throw `JsonException` for structurally invalid JSON. Use the `TryFromJson` overloads when the source data may be untrusted or malformed.
- These methods are extension methods and must be called with instance-method syntax on the appropriate types.
- Thread safety depends on the underlying JSON serializer configuration. If a custom `JsonSerializerOptions` instance is used internally, it should be configured for thread-safe access. The methods themselves hold no mutable shared state and are safe to call concurrently.
- The `TryFromJson` methods never throw; all deserialization exceptions are caught and converted to a `false` return with a `null` output parameter.
