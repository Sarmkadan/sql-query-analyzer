# Architecture

This document describes the actual structure of the codebase as it exists today.
Everything below is grounded in the source; where the design has gaps or the
implementation is a stub, that is stated explicitly.

## Overview

SQL Query Analyzer is a single .NET 10 console application (`sql-query-analyzer.csproj`,
`OutputType=Exe`, assembly `SqlQueryAnalyzer`) that performs **static, regex-based
analysis** of SQL text. It detects common anti-patterns (SELECT *, implicit joins,
leading-wildcard LIKE, functions on columns in predicates, N+1 access patterns),
scores the query 0-100, and produces index and rewrite suggestions.

There is no ASP.NET Core host and no live database connection in the main flow:
`Program.Main` wires a `ServiceCollection` by hand, resolves `IQueryAnalyzerService`,
and runs three hard-coded sample queries. Command-line arguments are currently
ignored by `Main` (see Known Limitations). The `System.Data.SqlClient`/`Npgsql`
package references exist for `DatabaseConnectionValidator` and configuration
types, not for query execution during analysis.

Two sibling projects live in the same repo and are excluded from the main
compile glob (`<Compile Remove>` in the csproj):

- `tests/sql-query-analyzer.Tests` - xUnit tests (parsers, normalizer, pattern analyzer, visualizer)
- `benchmarks/sql-query-analyzer.Benchmarks` - BenchmarkDotNet benchmarks
- `examples/` - standalone usage samples, also excluded from compilation

## Composition Root and Data Flow

`Program.cs` is the composition root:

```
ConfigurationBuilder (appsettings.json, env vars prefixed SQA_)
  → AddOptions<SqlQueryAnalyzerOptions>().Bind(...).ValidateDataAnnotations().ValidateOnStart()
  → ServiceCollection: everything registered as Singleton
  → BuildServiceProvider()
  → IQueryAnalyzerService.AnalyzeQueryAsync(sampleQuery)
```

All services are **singletons** - correct for a short-lived console process
holding only in-memory state. (Older docs claimed `AddScoped`; that was never
what `Program.cs` does.)

Analysis of one query flows as follows (`QueryAnalyzerService.AnalyzeQueryAsync`):

1. `DatabaseQuery.Parse()` - normalizes the text (strips comments/whitespace),
   counts statements/lines, detects the query type from the leading keyword,
   extracts referenced tables (FROM/JOIN/INTO/UPDATE, excluding CTE aliases)
   and join conditions. All regex-based; there is no real SQL grammar parser.
2. `DetermineComplexityAsync` - heuristic on line count, table count, join count.
3. `IPerformanceIssueDetectorService.DetectIssuesAsync` - runs the individual
   detectors (SELECT *, join issues, leading wildcards, functions on columns,
   index opportunities, implicit conversions) and sorts issues by severity.
4. `IIndexAnalyzerService.AnalyzeIndexesAsync(table)` per referenced table -
   produces `IndexSuggestion`s from heuristics (no catalog metadata is read).
5. `CalculatePerformanceScoreAsync` - starts at 100, subtracts 10/5/2 per
   Critical/Warning/Info issue, clamps to [0, 100].
6. `EstimateExecutionTime` - synthetic estimate from complexity + issue impact;
   it is a heuristic label, not a measurement.
7. `IAnalysisRepository.SaveAnalysisAsync(result)` - persists to the in-memory
   repository.

The result is a `QueryAnalysisResult` (score, `Issues`, `IndexSuggestions`,
`Statistics`, complexity, estimated time).

## Module Breakdown

| Directory | What is actually there |
|---|---|
| `Models/` | Domain types: `DatabaseQuery` (with `Parse()`), `QueryAnalysisResult`, `PerformanceIssue`, `IndexSuggestion`, `IndexRecommendation`, `QueryPlan`, `SlowQueryEntry`, `QueryStatistics`, `PlanVisualization`, plus per-type `*Extensions`/`*Validation`/`*JsonExtensions` helper files |
| `Services/` | The analysis engine: `QueryAnalyzerService` (orchestrator), `PerformanceIssueDetectorService`, `IndexAnalyzerService`, `QueryPlanAnalyzerService`, `ExplainPlanParserService`, `IndexRecommendationEngine`, `SlowQueryLogParser`, `QueryProfilerService`, `QueryRewriteService`, `AnalysisBuilder` |
| `Repositories/` | `IQueryRepository`/`IAnalysisRepository`/`IIndexRepository` with **in-memory, lock-guarded List implementations only**. There are no SQL Server or PostgreSQL repository implementations |
| `Utilities/` | `SqlPatternAnalyzer` (static, source-generated regexes), `QueryNormalizer`, `QueryValidator`, `SqlInjectionDetector`, `QueryCacheKeyGenerator`, `PerformanceMetricsCalculator`, `StatisticsAggregator`, `ReportGenerator`, `BatchAnalysisProcessor`, `DatabaseConnectionValidator` |
| `Middleware/` | Not ASP.NET middleware - plain classes composed manually: `AnalysisPipeline`, `RateLimitingMiddleware` (in-process token/window limiter), `ErrorHandlingMiddleware` |
| `API/` | `AnalysisController` - a framework-free controller-shaped class returning `ApiResponse<T>` objects. The project has no ASP.NET Core reference; nothing routes HTTP to it |
| `CLI/` | `CommandLineParser`, `CommandLineArguments`, `CliApplicationHost`. Implemented but **not invoked from `Program.Main`** |
| `Caching/` | `QueryAnalysisCache` - in-memory dictionary with TTL + LRU eviction, keyed by `QueryCacheKeyGenerator` fingerprints. Not registered in DI; consumers construct it directly |
| `Configuration/` | `SqlQueryAnalyzerOptions` (bound from the `SqlQueryAnalyzer` section, DataAnnotations-validated), `AnalyzerSettings`, `ProfilerSettings`, `IConnectionConfiguration` + `SqlServerConfiguration` |
| `Visualization/` | `ExecutionPlanVisualizer` (ASCII tree) and `HtmlPlanVisualizer` (self-contained HTML report) |
| `Export/`, `Integration/` | `ExportService` (JSON/CSV/HTML/Markdown), `HttpQueryAnalysisClient`, `WebhookNotificationService` |
| `BackgroundWorkers/` | `AnalysisQueueProcessor` - in-process queue consumer |
| `Events/`, `Plugins/`, `Formatters/`, `Validation/`, `Scoring/`, `Diagnostics/`, `Testing/`, `Constants/` | `AnalysisEventPublisher`, `IAnalysisPlugin`, `IResultFormatter`, `ValidationRuleEngine`, `QueryComplexityScorer`, `AnalyzerHealthCheck`, `SampleQueryProvider`, and `AnalyzerDefaults`/`IssueTypes` constants |

A pervasive convention: most public types have companion `*Extensions.cs`,
`*Validation.cs`, and `*JsonExtensions.cs` files providing fluent helpers,
guard-clause validation, and `System.Text.Json` (de)serialization respectively.

## Key Design Decisions

**Regex-based parsing instead of a SQL grammar.**
`DatabaseQuery.Parse`, `SqlPatternAnalyzer`, and the detectors all use regular
expressions. Trade-off: fast, dependency-free, works on any dialect fragment;
but it cannot resolve aliases, quoted identifiers, or nested subquery scope, so
detections are heuristic with known false positives/negatives. CTE aliases are
explicitly excluded from table extraction to reduce false N+1 hits.

**Source-generated regexes on hot paths.**
`SqlPatternAnalyzer` and `PerformanceIssueDetectorService` use `[GeneratedRegex]`
(compile-time state machines, zero per-call allocation) and `FrozenSet<string>`
for keyword membership. The benchmark project exists to keep this honest.

**Everything in-memory, interfaces first.**
Repositories, cache, queue, and rate limiter are all in-process. The interfaces
(`IQueryRepository`, `IAnalysisRepository`, ...) are the seam where a real
database-backed implementation would plug in; none ships today.

**Singleton lifetimes throughout.**
No scoped/transient services and no captive-dependency risk: a console app with
stateless services and lock-guarded in-memory stores. If this were hosted in
ASP.NET Core, the in-memory repositories would become shared cross-request
state by design.

**Sync core, async surface.**
Detector internals are synchronous; public members return `Task`/`ValueTask`
(`ValueTask.FromResult` where the value is already computed) so the contracts
won't break when I/O-bound implementations appear.

## Extension Points

- **`Plugins/IAnalysisPlugin`** - implement to contribute additional issues to
  an analysis pass.
- **`Formatters/IResultFormatter`** - implement for new output formats;
  `ExportService` covers JSON/CSV/HTML/Markdown today.
- **Repository interfaces** - swap the in-memory implementations for persistent
  ones by changing the registrations in `Program.cs`.
- **`Events/AnalysisEventPublisher`** - subscribe to analysis lifecycle events.
- **`Validation/ValidationRuleEngine`** - add custom validation rules.

Note: there is no `IAnalysisStrategy` abstraction, no `ReportFormatterBase`
template class, and no `CachedQueryAnalyzer`/`LoggedQueryAnalyzer` decorators -
earlier revisions of the architecture doc described these, but they were never
implemented.

## Known Limitations

- `Program.Main` ignores `args` and runs hard-coded sample queries; the `CLI/`
  types are wired for real argument handling but nothing calls them yet.
- `AnalysisController` has no HTTP host; it is a library-shaped facade.
- Index suggestions are heuristic - no database catalog is consulted.
- `EstimatedExecutionTime` is a synthetic label derived from the heuristics,
  not a measurement.
- Only the in-memory repositories exist; results do not survive the process.
- The regex parser does not handle quoted/bracketed identifiers, schema-qualified
  names (`dbo.Orders` extracts `dbo`), or dialect-specific syntax edge cases.

## Testing

`tests/sql-query-analyzer.Tests` (xUnit + FluentAssertions) covers the pure
logic: `QueryNormalizer`, `QueryValidator`, `SqlPatternAnalyzer`,
`ExplainPlanParser`, `SlowQueryLogParser`, `QueryPlanAnalyzer`,
`IndexRecommendationEngine`, `HtmlPlanVisualizer`, and bad-query fixtures.
There are no integration tests against live databases.
