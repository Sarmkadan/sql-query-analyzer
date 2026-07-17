# SampleQueryProviderJsonExtensions

Provides JSON serialization support for the sample query provider data model. The type exposes required properties for storing query samples and static helpers for converting instances to and from JSON representation.

## API

### AllSamples
**Purpose**  
Holds a mapping of sample identifiers to their JSON‑encoded query strings.

**Parameters**  
None.

**Return value**  
A `Dictionary<string, string>` containing the sample mappings. The property is required; assigning `null` will cause an `ArgumentNullException` when the object is used.

**Exceptions**  
- `ArgumentNullException` – if the property is set to `null`.

### RandomSample
**Purpose**  
Stores a single, randomly selected sample query as a JSON string for quick access.

**Parameters**  
None.

**Return value**  
A `string` representing the random sample. The property is required; assigning `null` will cause an `ArgumentNullException` when the object is used.

**Exceptions**  
- `ArgumentNullException` – if the property is set to `null`.

### SamplesByIssueType
**Purpose**  
Groups sample queries by issue type, mapping each issue type to a list of JSON‑encoded query strings.

**Parameters**  
None.

**Return value**  
A `Dictionary<string, List<string>>` where the key is the issue type and the value is a list of sample JSON strings. The property is required; assigning `null` will cause an `ArgumentNullException` when the object is used.

**Exceptions**  
- `ArgumentNullException` – if the property is set to `null`.

### ToJson
**Purpose**  
Serializes the current instance to a JSON string.

**Parameters**  
None.

**Return value**  
A `string` containing the JSON representation of the instance.

**Exceptions**  
- `InvalidOperationException` – if any required property has not been initialized before serialization.

### FromJson
**Purpose**  
Deserializes a JSON string into a new `SampleQueryProviderJsonExtensions` instance.

**Parameters**  
None.

**Return value**  
An `object?` that is the deserialized instance, or `null` if the JSON payload is invalid or does not match the expected type.

**Exceptions**  
- `JsonException` – if the JSON is malformed.

### TryFromJson
**Purpose**  
Attempts to deserialize a JSON string into a `SampleQueryProviderJsonExtensions` instance, indicating success via a Boolean return value.

**Parameters**  
None.

**Return value**  
`true` if deserialization succeeded; otherwise `false`. When `false` is returned, the output instance is `null`.

**Exceptions**  
None.

## Usage

```csharp
using System.Collections.Generic;

// Create and populate an instance.
var provider = new SampleQueryProviderJsonExtensions
{
    AllSamples = new Dictionary<string, string>
    {
        ["sample1"] = "{\"query\":\"SELECT * FROM Users\"}",
        ["sample2"] = "{\"query\":\"SELECT Name FROM Orders\"}"
    },
    RandomSample = "{\"query\":\"SELECT COUNT(*) FROM Logs\"}",
    SamplesByIssueType = new Dictionary<string, List<string>>
    {
        ["Performance"] = new List<string>
        {
            "{\"query\":\"SELECT * FROM LargeTable WHERE Id = 1\"}",
            "{\"query\":\"SELECT * FROM LargeTable ORDER BY Timestamp DESC LIMIT 10\"}"
        },
        ["Security"] = new List<string>
        {
            "{\"query\":\"SELECT * FROM Users WHERE Id = ' OR '1'='1\"}"
        }
    }
};

// Serialize to JSON.
string json = SampleQueryProviderJsonExtensions.ToJson();
// json now contains the full JSON representation of `provider`.

// Deserialize from JSON.
object? deserializedObj = SampleQueryProviderJsonExtensions.FromJson();
if (deserializedObj is SampleQueryProviderJsonExtensions restored)
{
    // Use restored instance.
    Console.WriteLine(restored.RandomSample);
}
```

```csharp
using System;

// Attempt deserialization with error handling.
bool success = SampleQueryProviderJsonExtensions.TryFromJson();
if (success)
{
    // Assuming TryFromJson populates a static holder or returns via an out parameter not shown.
    // Adjust usage according to actual implementation.
    Console.WriteLine("Deserialization succeeded.");
}
else
{
    Console.WriteLine("Deserialization failed; check the JSON payload.");
}
```

## Notes
- All three properties are **required**; setting any of them to `null` will result in an `ArgumentNullException` when the instance is accessed or serialized.
- The serialization methods (`ToJson`, `FromJson`, `TryFromJson`) operate on the current instance state; calling `ToJson` before initializing the required properties throws an `InvalidOperationException`.
- `FromJson` returns `null` for malformed JSON or when the payload does not map to the expected type; callers should verify the result before casting.
- `TryFromJson` provides a fallback that never throws; it returns `false` on failure, allowing callers to avoid exception handling paths.
- Instance members are not thread‑safe; concurrent reads and writes to the same instance require external synchronization.
- The static JSON helpers do not modify mutable static state; therefore they are safe to call concurrently from multiple threads, provided the instance being serialized is not being mutated simultaneously.
