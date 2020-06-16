# RateLimitingMiddleware
The `RateLimitingMiddleware` class is designed to manage and enforce rate limiting on incoming queries, preventing excessive usage and potential denial-of-service attacks. It provides a set of methods to acquire and release slots, track query statistics, and determine system load, allowing for efficient and controlled query processing.

## API
* `public RateLimitingMiddleware`: The constructor for the `RateLimitingMiddleware` class, initializing a new instance.
* `public async Task AcquireSlotAsync`: Attempts to acquire a slot for query execution, returning a task that completes when the slot is acquired. Throws if the rate limit is exceeded.
* `public void ReleaseSlot`: Releases a previously acquired slot, allowing other queries to execute.
* `public double GetSystemLoad`: Returns the current system load, indicating the level of query processing activity.
* `public QueryRateLimitStats GetQueryStats`: Returns statistics about the query rate limit, including the number of requests and average interval between them.
* `public string QueryHash`: Gets the hash of the query being rate limited.
* `public int RequestCount`: Gets the number of requests made for the query.
* `public DateTime FirstRequestTime`: Gets the time of the first request made for the query.
* `public DateTime LastRequestTime`: Gets the time of the last request made for the query.
* `public double GetAverageInterval`: Returns the average interval between requests for the query.
* `public int TotalRequests`: Gets the total number of requests made for the query.
* `public DateTime LastRequestTime`: Gets the time of the last request made for the query.
* `public double AverageIntervalMs`: Gets the average interval between requests in milliseconds.
* `public bool IsThrottled`: Indicates whether the query is currently being throttled due to rate limiting.

## Usage
```csharp
// Example 1: Basic rate limiting
var middleware = new RateLimitingMiddleware();
await middleware.AcquireSlotAsync();
// Execute query
middleware.ReleaseSlot();
```

```csharp
// Example 2: Advanced rate limiting with statistics
var middleware = new RateLimitingMiddleware();
var stats = middleware.GetQueryStats();
Console.WriteLine($"Request Count: {stats.RequestCount}, Average Interval: {stats.AverageIntervalMs}ms");
if (middleware.IsThrottled)
{
    Console.WriteLine("Query is being throttled");
}
else
{
    await middleware.AcquireSlotAsync();
    // Execute query
    middleware.ReleaseSlot();
}
```

## Notes
The `RateLimitingMiddleware` class is designed to be thread-safe, allowing multiple threads to acquire and release slots concurrently. However, it is essential to ensure that the `AcquireSlotAsync` and `ReleaseSlot` methods are used correctly to avoid deadlocks or other synchronization issues. Additionally, the `GetSystemLoad` and `GetQueryStats` methods may return stale data if the system load or query statistics change rapidly, and should be used accordingly. Edge cases, such as an empty query hash or zero request count, should be handled carefully to avoid unexpected behavior.
