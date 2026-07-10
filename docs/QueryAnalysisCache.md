# QueryAnalysisCache

A thread-safe in-memory cache that stores `QueryAnalysisResult` objects keyed by SQL query strings, providing fast lookup, statistics, and automatic expiration of stale entries. It is designed to reduce redundant query parsing and analysis in tools like SQL query analyzers by reusing previously computed results.

## API

### `public QueryAnalysisCache(int maxEntries = 1000)`

Initializes a new cache with the specified maximum number of entries. Older or less recently used entries are evicted when the limit is reached.

- **Parameters**
  - `maxEntries`: The maximum number of entries the cache can hold. Must be positive.

- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `maxEntries` is less than or equal to zero.

---

### `public bool TryGetResult(string key, [NotNullWhen(true)] out QueryAnalysisResult? result)`

Attempts to retrieve a cached `QueryAnalysisResult` by its query string key.

- **Parameters**
  - `key`: The SQL query string used as the lookup key.
  - `result`: When this method returns `true`, contains the cached `QueryAnalysisResult`; otherwise, `null`.

- **Return Value**
  - `true` if the key was found and the result is valid; otherwise, `false`.

- **Exceptions**
  - Throws `ArgumentNullException` if `key` is `null`.

---

### `public void Set(string key, QueryAnalysisResult result)`

Stores a `QueryAnalysisResult` in the cache under the specified query string key, updating metadata such as access count and timestamps.

- **Parameters**
  - `key`: The SQL query string to use as the cache key.
  - `result`: The analysis result to cache.

- **Exceptions**
  - Throws `ArgumentNullException` if `key` or `result` is `null`.

---

### `public void Invalidate(string key)`

Removes the cached entry associated with the specified query string key, if it exists.

- **Parameters**
  - `key`: The SQL query string of the entry to remove.

- **Exceptions**
  - Throws `ArgumentNullException` if `key` is `null`.

---

### `public void Clear()`

Removes all entries from the cache.

---

### `public void RemoveExpiredEntries()`

Removes entries that have exceeded their maximum age (if an expiration policy is configured). This method is idempotent and safe to call frequently.

---

### `public CacheStatistics GetStatistics()`

Returns a snapshot of current cache statistics, including hit/miss counts, entry count, and performance metrics.

- **Return Value**
  - A `CacheStatistics` object containing current cache metrics.

---

### `public string Key`

Gets the SQL query string that serves as the unique identifier for this cache entry.

- **Return Value**
  - The query string key.

---

### `public QueryAnalysisResult Result`

Gets the cached analysis result.

- **Return Value**
  - The cached `QueryAnalysisResult`.

---

### `public DateTime CreatedAt`

Gets the timestamp when this entry was first added to the cache.

- **Return Value**
  - The creation timestamp.

---

### `public DateTime LastAccessedAt`

Gets the timestamp of the most recent access (read or write) to this entry.

- **Return Value**
  - The last access timestamp.

---
### `public int AccessCount`

Gets the number of times this entry has been accessed (read or written).

- **Return Value**
  - The access count.

---
### `public int TotalEntries`

Gets the total number of entries currently stored in the cache.

- **Return Value**
  - The current entry count.

---
### `public int MaxEntries`

Gets the maximum number of entries this cache can hold.

- **Return Value**
  - The maximum allowed entries.

---
### `public long Hits`

Gets the total number of successful lookups (cache hits) since the cache was created.

- **Return Value**
  - The hit count.

---
### `public long Misses`

Gets the total number of failed lookups (cache misses) since the cache was created.

- **Return Value**
  - The miss count.

---
### `public double HitRate`

Gets the ratio of cache hits to total lookups (hits + misses), expressed as a value between 0.0 and 1.0.

- **Return Value**
  - The hit rate.

---
### `public double AverageAccessCount`

Gets the average number of accesses per entry across all entries currently in the cache.

- **Return Value**
  - The average access count.

---
### `public double OldestEntryAge`

Gets the age in seconds of the oldest entry currently in the cache.

- **Return Value**
  - The age of the oldest entry.

---
### `public override string ToString()`

Returns a human-readable summary of the cache state, including entry count, hit rate, and oldest entry age.

- **Return Value**
  - A formatted string with cache statistics.

---

## Usage

### Example 1: Basic Caching and Retrieval
