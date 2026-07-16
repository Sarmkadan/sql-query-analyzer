# Quickstart & Rule Catalog

A focused entry point for SQL Query Analyzer. The full API reference lives in
[`README.md`](../README.md); this page is the "get running in five minutes"
version plus the catalog of detection rules.

## Install & build

```bash
git clone https://github.com/sarmkadan/sql-query-analyzer.git
cd sql-query-analyzer
dotnet build -c Release
```

Requires the .NET 10.0 SDK (see `global.json`).

## CLI in 30 seconds

```bash
# Analyze one query
sqlanalyzer --query "SELECT * FROM Orders WHERE CustomerId = 1"

# Analyze a file and emit a JSON report
sqlanalyzer --query-file queries.sql --report --format json

# Batch mode across many files
sqlanalyzer --batch --threads 4 --cache-path ./cache

# Only surface the worst offenders
sqlanalyzer --query-file query.sql --severity Critical --export-suggestions
```

## Library in 30 seconds

```csharp
var services = new ServiceCollection()
    .AddScoped<IQueryAnalyzerService, QueryAnalyzerService>()
    .BuildServiceProvider();

var analyzer = services.GetRequiredService<IQueryAnalyzerService>();

var result = await analyzer.AnalyzeQueryAsync(
    "SELECT * FROM Orders WHERE CustomerId = 1");

Console.WriteLine($"Performance Score: {result.PerformanceScore:F1}/100");
foreach (var issue in result.Issues)
    Console.WriteLine($"[{issue.Severity}] {issue.IssueType}: {issue.Description}");
```

For pure pattern checks without the service layer, the static
`SqlPatternAnalyzer` (in `SqlQueryAnalyzer.Utilities`) exposes each rule directly:

```csharp
SqlPatternAnalyzer.HasSelectStar("SELECT * FROM users");            // true
SqlPatternAnalyzer.HasImplicitJoin("SELECT ... FROM a, b WHERE ..."); // true
SqlPatternAnalyzer.GenerateOptimizationRecommendations(query);        // string[]
```

## Rule catalog

Each rule maps to a method on `SqlPatternAnalyzer` and to a fixture under
`tests/sql-query-analyzer.Tests/fixtures/` that pins its behavior.

| Rule | Method | Why it matters | Fixture |
| :--- | :--- | :--- | :--- |
| SELECT * | `HasSelectStar` | Ships unused columns and defeats covering indexes. | `select-star.sql` |
| Missing WHERE / LIMIT | `HasMissingWhereClause` | Unbounded scan streams the whole table. | `missing-where.sql` |
| Implicit / cartesian join | `HasImplicitJoin` | Comma-joined tables with no join key multiply rows. | `cartesian-join.sql` |
| Function on column | `HasFunctionOnColumn` | Wrapping an indexed column (`UPPER(col)`) blocks an index seek. | `missing-index.sql` |
| Leading-wildcard LIKE | `HasLeadingWildcardLike` | `LIKE '%x'` cannot use a b-tree range. | `missing-index.sql` |
| N+1 access pattern | `DetectNPlusOnePattern` | Same table hit once per parent row instead of a set-based fetch. | `n-plus-one.sql` |
| Excess OR conditions | `CountOrConditions` | Many ORs in WHERE often beat the optimizer; consider UNION ALL. | - |
| Subquery in FROM | `HasSubquery` | Derived tables can block predicate pushdown; a JOIN may be cheaper. | - |
| DISTINCT without ORDER BY | `HasDistinctWithoutOrder` | Deduplication with no defined order is usually a modeling smell. | - |

The control fixture `clean-query.sql` is asserted to trip *none* of the
bad-query rules, guarding against false positives.

## Example output

```text
Performance Score: 62.0/100

[High]     SelectStar        Replace SELECT * with specific column names
[High]     CartesianJoin     Replace implicit JOIN (comma-separated tables) with explicit JOIN syntax
[Medium]   NonSargable       Move functions to right side of comparison or use computed columns with indexes
[Medium]   LeadingWildcard   Use full-text search instead of LIKE with leading wildcard

Recommendations: 4  |  Readability: 55/100
```

## Running the tests

```bash
dotnet test --filter "FullyQualifiedName~BadQueryFixturesTests"
```

The fixture-driven suite feeds each known-bad query above through the analyzer
and asserts the matching rule fires.
