# QueryCacheKeyGeneratorExtensions

Utility class providing extension methods for generating and manipulating cache keys for SQL query caching. These methods standardize the creation, comparison, and validation of cache keys used by the SQL query analyzer to ensure consistent and efficient caching behavior across query executions.

## API

### `CreateCompositeKey`

Creates a composite cache key by combining a base key with a set of parameter values. The resulting key is deterministic and suitable for use as a cache lookup identifier.

- **Parameters**
  - `baseKey` (string): The base identifier for the query structure (e.g., normalized SQL).
  - `parameters` (IEnumerable<KeyValuePair<string, object?>>): A collection of parameter names and their values used in the query.
- **Returns**
  - `string`: A composite key formed by concatenating the base key with sorted parameter values.
- **Throws**
  - `ArgumentNullException`: If `baseKey` is `null`.
  - `ArgumentNullException`: If `parameters` is `null`.

---

### `AreKeysForSameQuery`

Determines whether two cache keys represent the same logical query, ignoring differences in parameter formatting or ordering.

- **Parameters**
  - `key1` (string): The first cache key to compare.
  - `key2` (string): The second cache key to compare.
- **Returns**
  - `bool`: `true` if the keys represent the same query regardless of parameter formatting; otherwise, `false`.
- **Throws**
  - `ArgumentNullException`: If either `key1` or `key2` is `null`.

---

### `ExtractParametersFromMetadataKey`

Parses a metadata-style cache key to extract the original parameter values. Useful for reconstructing query parameters from a stored key.

- **Parameters**
  - `metadataKey` (string): The cache key generated via `GenerateParameterizedQueryKey` or similar.
- **Returns**
  - `Dictionary<string, string>?`: A dictionary mapping parameter names to their stringified values, or `null` if the key format is invalid or does not contain parameters.
- **Throws**
  - `ArgumentNullException`: If `metadataKey` is `null`.

---

### `GenerateParameterizedQueryKey`

Generates a parameterized cache key for a SQL query by combining a normalized query string with its parameter values. The key is deterministic and suitable for cache lookups.

- **Parameters**
  - `normalizedQuery` (string): The normalized SQL query string (e.g., with whitespace and formatting standardized).
  - `parameters` (IEnumerable<KeyValuePair<string, object?>>): A collection of parameter names and their values.
- **Returns**
  - `string`: A composite cache key incorporating both the query structure and parameter values.
- **Throws**
  - `ArgumentNullException`: If `normalizedQuery` is `null`.
  - `ArgumentNullException`: If `parameters` is `null`.

---
### `IsCacheKeyExpired`

Determines whether a cache key has expired based on an optional expiration policy embedded in the key or external configuration.

- **Parameters**
  - `cacheKey` (string): The cache key to evaluate.
  - `currentTimestamp` (DateTimeOffset): The current timestamp used for expiration comparison.
  - `expirationPolicy` (TimeSpan?, optional): The maximum age of a valid key. If `null`, keys are considered non-expiring.
- **Returns**
  - `bool`: `true` if the key is expired or malformed; otherwise, `false`.
- **Throws**
  - `ArgumentNullException`: If `cacheKey` is `null`.

---
### `FormatCacheKey`

Formats a raw cache key into a human-readable string, typically for logging or debugging purposes. Preserves the internal structure but improves readability.

- **Parameters**
  - `cacheKey` (string): The raw cache key to format.
- **Returns**
  - `string`: A formatted version of the cache key with improved readability (e.g., separated components, consistent casing).
- **Throws**
  - `ArgumentNullException`: If `cacheKey` is `null`.

## Usage
