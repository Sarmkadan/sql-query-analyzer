# CachingMiddleware

The `CachingMiddleware` class is an `IAnalysisMiddleware` stage that checks the `QueryAnalysisCache` before analysis runs. When a cached `QueryAnalysisResult` exists for the query, the middleware places it on the `AnalysisContext` and stops the pipeline, so the analyzer is skipped. On a cache miss it does nothing and lets the remaining stages run. Caching can be bypassed per analysis through command-line arguments or context metadata. Cache errors are logged and never fail the pipeline.

Storing new results is not done by this middleware. After the analysis stage, `AnalysisPipeline` writes the result to `QueryAnalysisCache.Instance` when that cache is registered.

## API

### Constructors

* `public CachingMiddleware(QueryAnalysisCache cache, ILogger<CachingMiddleware> logger)`: Initializes a new instance of the `CachingMiddleware` class with the cache to read from and the logger used for diagnostics.

### Methods

* `public Task ExecuteAsync(AnalysisContext context)`: Implements `IAnalysisMiddleware.ExecuteAsync`. Behavior:
  * If caching is disabled for this analysis (see [Bypassing the cache](#bypassing-the-cache)), it returns without touching the cache.
  * If `QueryAnalysisCache.TryGetResult(context.Query, ...)` finds a non-null result, it sets `context.Result` to that result and sets `context.ShouldContinue` to `false`.
  * On a miss, it returns without changing the context.
  * Any exception is caught and logged as an error. The context is left unchanged, so the pipeline continues without cached data.

### Private Members

* `private bool IsCachingDisabled(AnalysisContext context)`: Returns `true` when caching must be skipped. This is the case when `context.Arguments` is a `CommandLineArguments` with `EnableCache` set to `false`, or when `context.Metadata["bypassCache"]` is a `bool` with value `true`.

### Properties

The class exposes no public properties. Its dependencies are held in private read-only fields.

## Bypassing the cache

Caching is on by default (`CommandLineArguments.EnableCache` defaults to `true`). It can be disabled in either of two ways:

* Set `CommandLineArguments.EnableCache` to `false` on the `AnalysisContext.Arguments` object.
* Add `Metadata["bypassCache"] = true` to the `AnalysisContext.Metadata` dictionary.

## Usage

The following example runs the middleware on its own. It assumes the cache and key generator are created the same way the application does.

```csharp
using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Caching;
using SqlQueryAnalyzer.CLI;
using SqlQueryAnalyzer.Middleware;
using SqlQueryAnalyzer.Models;
using SqlQueryAnalyzer.Utilities;

var cache = new QueryAnalysisCache(
    NullLogger<QueryAnalysisCache>.Instance,
    new QueryCacheKeyGenerator());

var middleware = new CachingMiddleware(cache, NullLogger<CachingMiddleware>.Instance);

// Store a result from an earlier analysis (QueryAnalysisResult comes from the analyzer)
cache.Set("SELECT * FROM Orders", previousResult);

// Cache hit: the result is attached and the pipeline is told to stop
var context = new AnalysisContext { Query = "SELECT * FROM Orders" };
await middleware.ExecuteAsync(context);

if (!context.ShouldContinue && context.Result is not null)
{
    Console.WriteLine("Served from cache");
}

// Bypass: the cache is not consulted at all
var bypassContext = new AnalysisContext { Query = "SELECT * FROM Orders" };
bypassContext.Metadata["bypassCache"] = true;
await middleware.ExecuteAsync(bypassContext);
```
