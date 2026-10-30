# IndexRecommendationEngine

`IndexRecommendationEngine` is the default implementation of `IIndexRecommendationEngine`. It inspects SQL text with regular expressions, collects the columns used in `WHERE`, `JOIN ... ON`, `ORDER BY` and `GROUP BY`, and turns them into `IndexRecommendation` objects with a heuristic impact score. It can also rank a list of recommendations and report overlapping index candidates.

The engine is a text heuristic, not a SQL parser. It does not read schema, statistics or execution plans, so its output is a set of candidates to review, not a verified index design.

Namespace: `SqlQueryAnalyzer.Services`
Assembly: `SqlQueryAnalyzer`

## API

### Constructors

**`public IndexRecommendationEngine(ILogger<IndexRecommendationEngine> logger)`**
**Purpose:** Creates an engine with default analyzer settings from `AnalyzerSettingsFactory.CreateDefault()`.
**Parameters:** `logger` - logger used for diagnostics.
**Throws:** `ArgumentNullException` if `logger` is `null`.

**`public IndexRecommendationEngine(ILogger<IndexRecommendationEngine> logger, AnalyzerSettings settings)`**
**Purpose:** Creates an engine with explicit analyzer settings.
**Parameters:**
- `logger` - logger used for diagnostics.
- `settings` - `AnalyzerSettings` instance the engine is configured with.

**Throws:** `ArgumentNullException` if `logger` or `settings` is `null`.

### Methods

**`public Task<List<IndexRecommendation>> RecommendAsync(string queryText)`**
**Purpose:** Analyzes a SQL query and returns ranked index recommendations. The method is not `async`; it returns an already completed task.
**Parameters:** `queryText` - the SQL text to analyze.
**Return value:** A `Task` holding the ranked `List<IndexRecommendation>`. If `queryText` is empty or whitespace, the list is empty.
**Throws:** Any exception raised during analysis is logged and rethrown. Because the method is not `async`, the exception is thrown from the call itself, not stored in the returned task.

The method runs these steps:
1. Maps table aliases to table names from `FROM` and `JOIN` clauses.
2. Builds single-column candidates from each clause, with the scores below.
3. For every table with more than one distinct candidate column, adds one composite candidate.
4. Ranks the result with `RankRecommendations`, then calls `DetectRedundancies` and logs the number of overlaps found.

| Source (`RecommendationSource`) | Clause | ImpactScore |
|---|---|---|
| `WhereClause` | `WHERE` predicates (`=`, `<>`, `!=`, `>=`, `<=`, `>`, `<`, `LIKE`, `IN`, `BETWEEN`, `IS`) | 70 |
| `JoinCondition` | `JOIN ... ON` equality pairs, both sides | 65 |
| `OrderBy` | `ORDER BY` columns (`ASC`/`DESC` removed) | 50 |
| `GroupBy` | `GROUP BY` columns | 45 |
| `Composite` | All distinct columns of one table | `min(95, 75 + 5 × columnCount)` |

Only the `WHERE`, `JOIN`, `ORDER BY` and `GROUP BY` clauses are read. The `SELECT` list is not analyzed, so `INCLUDE` columns are never filled in.

**`public List<IndexRecommendation> RankRecommendations(List<IndexRecommendation> recommendations)`**
**Purpose:** Removes duplicates and sorts recommendations from most to least impactful. Two recommendations are duplicates when they have the same table, key columns (in order) and source; the one with the highest `ImpactScore` is kept.
**Parameters:** `recommendations` - the candidates to rank. The list is not modified.
**Return value:** A new `List<IndexRecommendation>` ordered by `ImpactScore` descending, then composites before single-column candidates, then by more key columns first.
**Throws:** `ArgumentNullException` if `recommendations` is `null`.

**`public List<string> DetectRedundancies(List<IndexRecommendation> recommendations)`**
**Purpose:** Finds candidates on the same table where one candidate's key columns are a leading prefix of another's. For example, an index on `(Status)` overlaps with one on `(Status, CustomerId)`.
**Parameters:** `recommendations` - the candidates to compare. The list is not modified.
**Return value:** One message per overlapping pair, in the form `<table>: overlapping candidates [<cols>] and [<cols>]`. Duplicate messages are removed. Returns an empty list when there is no overlap.
**Throws:** `ArgumentNullException` if `recommendations` is `null`.

## Usage

The example analyzes a query with a `WHERE` predicate, a join and an `ORDER BY`, then prints the ranked candidates and the overlap report.

```csharp
using Microsoft.Extensions.Logging.Abstractions;
using SqlQueryAnalyzer.Services;

var engine = new IndexRecommendationEngine(NullLogger<IndexRecommendationEngine>.Instance);

const string sql = """
    SELECT o.Id, c.Name
    FROM Orders o
    JOIN Customers c ON o.CustomerId = c.Id
    WHERE o.Status = 'Open'
    ORDER BY o.CreatedAt DESC
    """;

var recommendations = await engine.RecommendAsync(sql);

foreach (var r in recommendations)
{
    Console.WriteLine($"{r.Source,-14} {r.TableName}({string.Join(", ", r.KeyColumns)}) score={r.ImpactScore}");
}

// RecommendAsync does not return redundancies; ask for them explicitly.
foreach (var overlap in engine.DetectRedundancies(recommendations))
{
    Console.WriteLine(overlap);
}
```

Expected output (traced from the implementation):

```text
Composite      Orders(Status, CustomerId, CreatedAt) score=90
WhereClause    Orders(Status) score=70
JoinCondition  Orders(CustomerId) score=65
JoinCondition  Customers(Id) score=65
OrderBy        Orders(CreatedAt) score=50
Orders: overlapping candidates [Status, CustomerId, CreatedAt] and [Status]
```

## Notes

- **Heuristic matching:** Aliases are resolved with regular expressions. Subqueries, CTEs, `OR` conditions, and unqualified columns in multi-table queries may be misattributed or skipped. Unqualified columns (no `alias.` prefix) are attributed only when the alias map holds exactly one entry, which in practice means a single table with no alias. With an alias such as `FROM Orders o`, the map holds two entries, so unqualified columns are dropped.
- **Redundancy is not returned by `RecommendAsync`:** the method only logs the number of overlaps. Call `DetectRedundancies` to get the messages.
- **Settings are not used yet:** the `AnalyzerSettings` passed to the second constructor is stored but never read. The scores are constants in the implementation.
- **Thread safety:** the engine holds only a logger and settings reference and keeps no per-call state, so instances can be shared. The class is not documented as thread-safe for concurrent mutation of the lists passed to `RankRecommendations` or `DetectRedundancies`.
- **Logging:** each call logs at `Information` level, and logs the query text itself. Be careful with sensitive literals in production logs.
