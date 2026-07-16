# Architecture Guide

This file previously described a design that diverged from the codebase: it
referenced components that were never implemented (`IAnalysisStrategy`,
`ReportFormatterBase`, `CachedQueryAnalyzer`/`LoggedQueryAnalyzer` decorators,
SQL Server/PostgreSQL repository implementations) and claimed scoped DI
lifetimes where `Program.cs` registers singletons.

The accurate, code-grounded architecture document now lives at
**[ARCHITECTURE.md](ARCHITECTURE.md)** (same directory). In short:

- Single .NET 10 console app; `Program.cs` is the composition root and
  registers everything as singletons.
- Analysis is static and regex-based (`DatabaseQuery.Parse`,
  `SqlPatternAnalyzer`, `PerformanceIssueDetectorService`); no live database
  connection and no SQL grammar parser.
- Repositories, cache, queue, and rate limiter are in-memory only; the
  interfaces are the seam for future persistent implementations.
- Extension points: `IAnalysisPlugin`, `IResultFormatter`, the repository
  interfaces, `AnalysisEventPublisher`, `ValidationRuleEngine`.

See [ARCHITECTURE.md](ARCHITECTURE.md) for the full module breakdown, data
flow, design rationale, and known limitations.
