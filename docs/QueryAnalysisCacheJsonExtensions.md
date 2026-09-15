# QueryAnalysisCacheJsonExtensions

Provides System.Text.Json serialization and deserialization extensions for `QueryAnalysisCache`.

## Purpose

Enables JSON serialization of a `QueryAnalysisCache` instance using camelCase property naming, plus safe deserialization back to a `QueryAnalysisCache` instance. Useful for persisting or transferring cache state.

## Members

### ToJson

```csharp
public static string ToJson(this QueryAnalysisCache value, bool indented = false)
```

Serializes the `QueryAnalysisCache` instance to a JSON string.

#### Parameters
- `value`: The cache instance to serialize.
- `indented`: Whether to format the JSON with indentation for readability. Default is `false`.

#### Returns
A JSON string representation of the cache.

#### Exceptions
- `ArgumentNullException`: Thrown when `value` is `null`.

### FromJson

```csharp
public static QueryAnalysisCache? FromJson(string json)
```

Deserializes a JSON string to a `QueryAnalysisCache` instance.

#### Parameters
- `json`: The JSON string to deserialize.

#### Returns
A `QueryAnalysisCache` instance, or `null` if the JSON is invalid or whitespace-only.

#### Exceptions
- `ArgumentException`: Thrown when `json` is `null` or empty.

### TryFromJson

```csharp
public static bool TryFromJson(string json, out QueryAnalysisCache? value)
```

Attempts to deserialize a JSON string to a `QueryAnalysisCache` instance.

#### Parameters
- `json`: The JSON string to deserialize.
- `value`: Receives the deserialized instance if successful, otherwise `null`.

#### Returns
`true` if deserialization succeeded; otherwise, `false`.

#### Exceptions
- `ArgumentException`: Thrown when `json` is `null` or empty.

## Example

```csharp
using SqlQueryAnalyzer.Caching;

// Serialize to JSON (compact)
string json = cache.ToJson();

// Serialize to JSON (indented for readability)
string indentedJson = cache.ToJson(indented: true);

// Deserialize from JSON
QueryAnalysisCache? deserialized = QueryAnalysisCacheJsonExtensions.FromJson(json);

// Try deserialization (safe)
if (QueryAnalysisCacheJsonExtensions.TryFromJson(json, out var cacheFromTry))
{
    // Use cacheFromTry
}
```