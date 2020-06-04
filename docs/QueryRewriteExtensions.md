# QueryRewriteExtensions

Static extension class that provides helper methods for configuring and analyzing query rewrite suggestions within the SQL Query Analyzer pipeline. The members enable service registration, filtering, ordering, and summarization of rewrite suggestions, as well as extraction of associated index recommendations.

## API

### AddQueryRewriteService(IServiceCollection services)

**Purpose**  
Registers the query rewrite analysis services with the dependency‑injection container.

**Parameters**  
- `services`: The `IServiceCollection` to which the rewrite services are added.

**Return value**  
The same `IServiceCollection` instance, allowing fluent chaining.

**Exceptions**  
- `ArgumentNullException` if `services` is `null`.

### GetAutoApplicable(IEnumerable<QueryRewriteSuggestion> suggestions)

**Purpose**  
Filters the supplied suggestions to those that can be applied automatically without user intervention.

**Parameters**  
- `suggestions`: The source collection of rewrite suggestions to evaluate.

**Return value**  
A new `List<QueryRewriteSuggestion>` containing only the auto‑applicable suggestions.

**Exceptions**  
- `ArgumentNullException` if `suggestions` is `null`.

### GetNonBreaking(IEnumerable<QueryRewriteSuggestion> suggestions)

**Purpose**  
Returns suggestions that are guaranteed not to break existing query semantics.

**Parameters**  
- `suggestions`: The source collection of rewrite suggestions to evaluate.

**Return value**  
A new `List<QueryRewriteSuggestion>` containing only the non‑breaking suggestions.

**Exceptions**  
- `ArgumentNullException` if `suggestions` is `null`.

### OfType<T>(IEnumerable<QueryRewriteSuggestion> suggestions) where T : QueryRewriteSuggestion

**Purpose**  
Selects suggestions of a specific concrete type from the source collection.

**Parameters**  
- `suggestions`: The source collection of rewrite suggestions.  
- `T`: The desired suggestion type to filter by.

**Return value**  
A new `List<QueryRewriteSuggestion>` containing only elements whose runtime type matches `T`.

**Exceptions**  
- `ArgumentNullException` if `suggestions` is `null`.

### ForClause(IEnumerable<QueryRewriteSuggestion> suggestions, ClauseType clause)

**Purpose**  
Filters suggestions that target a particular SQL clause (e.g., SELECT, WHERE, JOIN).

**Parameters**  
- `suggestions`: The source collection of rewrite suggestions.  
- `clause`: The `ClauseType` indicating the clause to match.

**Return value**  
A new `List<QueryRewriteSuggestion>` containing only suggestions relevant to the specified clause.

**Exceptions**  
- `ArgumentNullException` if `suggestions` is `null`.  
- `ArgumentOutOfRangeException` if `clause` is not a defined `ClauseType`.

### OrderByImpact(IEnumerable<QueryRewriteSuggestion> suggestions)

**Purpose**  
Orders suggestions by their estimated performance impact, descending.

**Parameters**  
- `suggestions`: The source collection of rewrite suggestions.

**Return value**  
An `IOrderedEnumerable<QueryRewriteSuggestion>` sorted from highest to lowest impact.

**Exceptions**  
- `ArgumentNullException` if `suggestions` is `null`.

### GetTotalEstimatedImprovement(IEnumerable<QueryRewriteSuggestion> suggestions)

**Purpose**  
Calculates the cumulative estimated improvement percentage across all supplied suggestions.

**Parameters**  
- `suggestions`: The source collection of rewrite suggestions.

**Return value**  
A `double` representing the sum of individual improvement estimates (e.g., 23.5 for 23.5 %).

**Exceptions**  
- `ArgumentNullException` if `suggestions` is `null`.

### GetAllIndexSuggestions(IEnumerable<QueryRewriteSuggestion> suggestions)

**Purpose**  
Extracts all index‑related suggestions embedded within the rewrite suggestions.

**Parameters**  
- `suggestions`: The source collection of rewrite suggestions.

**Return value**  
A new `List<IndexSuggestion>` containing every index suggestion found.

**Exceptions**  
- `ArgumentNullException` if `suggestions` is `null`.

### GetRewriteSummary(IEnumerable<QueryRewriteSuggestion> suggestions)

**Purpose**  
Produces a human‑readable summary of the rewrite suggestions, including counts and impact totals.

**Parameters**  
- `suggestions`: The source collection of rewrite suggestions.

**Return value**  
A `string` summarizing the suggestions (e.g., “5 suggestions, 2 auto‑applicable, estimated improvement 12.3%”).

**Exceptions**  
- `ArgumentNullException` if `suggestions` is `null`.

## Usage

### Registering the rewrite service in an ASP.NET Core application

```csharp
using Microsoft.Extensions.DependencyInjection;
using SqlQueryAnalyzer.Extensions; // namespace containing QueryRewriteExtensions

public void ConfigureServices(IServiceCollection services)
{
    // Adds the required query rewrite analysis services.
    services.AddQueryRewriteService();

    // Other service registrations...
}
```

### Analyzing a query and reporting applicable rewrites

```csharp
using System.Linq;
using SqlQueryAnalyzer.Core;
using SqlQueryAnalyzer.Extensions;

public void AnalyzeAndReport(string sql)
{
    // Obtain raw suggestions from the analyzer (hypothetical API).
    IEnumerable<QueryRewriteSuggestion> rawSuggestions = QueryAnalyzer.GetSuggestions(sql);

    // Filter to safe, auto‑applicable rewrites.
    var safeAuto = rawSuggestions
                    .GetNonBreaking()
                    .GetAutoApplicable();

    // Order by expected impact.
    var ordered = safeAuto.OrderByImpact();

    // Compute total estimated improvement.
    double totalImprovement = ordered.GetTotalEstimatedImprovement();

    // Produce a summary for logging or UI display.
    string summary = ordered.GetRewriteSummary();

    Console.WriteLine($"Analysis complete: {summary}");
    Console.WriteLine($"Total estimated improvement: {totalImprovement}%");
}
```

## Notes

- All extension methods are **pure**; they do not modify the input collection and produce new instances for their results.
- Passing `null` for any enumerable or service collection argument results in an `ArgumentNullException`. Callers should validate or guard against null inputs when the source may be undefined.
- The methods are stateless and thread‑safe; multiple threads can invoke them concurrently on separate or shared input data without side effects, provided the input collections themselves are not mutated during enumeration.
- If the source enumeration is empty, the methods return empty collections or default values (e.g., `0.0` for `GetTotalEstimatedImprovement`, an empty string for `GetRewriteSummary`) rather than throwing.
- The `OfType<T>` method relies on runtime type checking; if no suggestions match the supplied type, an empty list is returned.
- `OrderByImpact` uses the `EstimatedImpact` property of `QueryRewriteSuggestion`; suggestions lacking a defined impact are treated as having zero impact and appear at the end of the ordered sequence.
