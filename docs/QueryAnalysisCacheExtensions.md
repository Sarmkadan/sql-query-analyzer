# QueryAnalysisCacheExtensions

Extension methods for `QueryAnalysisCache` that provide convenient caching patterns and diagnostic utilities.

## Purpose

Provides extension methods that simplify common caching operations such as get-or-add patterns, cache capacity checking, and detailed statistics reporting for the `QueryAnalysisCache` class.

## Members

### `public static QueryAnalysisResult GetOrAdd(this QueryAnalysisCache cache, string query, Func<string, QueryAnalysisResult> valueFactory)`

Gets the cached result for a query, or adds it if not present using the provided factory function.

- **Parameters**
  - `cache`: The cache instance to operate on.
  - `query`: The SQL query string to look up or cache.
  - `valueFactory`: A function that generates the analysis result when the query is not found in cache.

- **Returns**
  - The cached or newly generated `QueryAnalysisResult`.

- **Exceptions**
  - `ArgumentNullException`: Thrown when `cache` or `valueFactory` is null.
  - `ArgumentException`: Thrown when `query` is null or empty.

### `public static bool IsFull(this QueryAnalysisCache cache)`

Checks if the cache has reached its maximum capacity.

- **Parameters**
  - `cache`: The cache instance to check.

- **Returns**
  - `true` if the cache is at maximum capacity; otherwise, `false`.

- **Exceptions**
  - `ArgumentNullException`: Thrown when `cache` is null.

### `public static string GetSummary(this QueryAnalysisCache cache)`

Returns a detailed string summary of the cache statistics.

- **Parameters**
  - `cache`: The cache instance to summarize.

- **Returns**
  - A formatted string containing cache statistics including entry count, hit rate, hits/misses, average accesses, and oldest entry age.

- **Exceptions**
  - `ArgumentNullException`: Thrown when `cache` is null.

## Usage

### Example 1: Get-or-add pattern

```csharp
// Instead of manually checking and setting:
// var cache = new QueryAnalysisCache(logger, keyGenerator);
// if (!cache.TryGetResult(query, out var result))
// {
//     result = AnalyzeQuery(query);
//     cache.Set(query, result);
// }

// Use the extension method:
var result = cache.GetOrAdd(query, AnalyzeQuery);
```

### Example 2: Checking cache capacity

```csharp
if (cache.IsFull())
{
    // Cache is at maximum capacity, consider clearing or increasing size
    logger.LogWarning("Query analysis cache is full ({Count}/{Max} entries)",
        cache.Count, cache.GetStatistics().MaxEntries);
}
```

### Example 3: Getting detailed cache summary

```csharp
// For logging or debugging purposes
var summary = cache.GetSummary();
// Returns something like: "Cache Summary: 42/1000 entries, Hit Rate: 85.2%, Hits: 240, Misses: 42, Avg Accesses: 5.7, Oldest Entry: 3600s ago"
logger.LogDebug("Cache status: {Summary}", summary);
```