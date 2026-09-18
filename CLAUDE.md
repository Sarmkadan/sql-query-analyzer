# CLAUDE.md

SQL Query Analyzer: .NET 10 console app + library that analyzes SQL queries, detects performance issues (table scans, missing indexes, N+1, cartesian joins, SELECT *), and produces index/rewrite recommendations.

## Build

- SDK: .NET 10.0.100 (`global.json`, `rollForward: latestMinor`)
- `dotnet restore && dotnet build --configuration Release` (or `make build`)
- `dotnet run` runs `Program.cs` sample analysis (or `make run`)
- `make publish` -> `./publish`; `make package` -> NuGet pack
- Docker: `make docker-build`, `make docker-up` (`docker-compose.yml`, plus `docker-compose.postgres.yml` / `docker-compose.mysql.yml`)
- Cross-platform scripts: `build.sh`, `build.ps1`

## Tests

- Test project: `tests/sql-query-analyzer.Tests/` (xUnit 2.9, FluentAssertions 8, Moq 4.20)
- `dotnet test --configuration Release` (or `make test`, `make test-verbose`)
- Single test: `dotnet test --filter "FullyQualifiedName~CartesianJoinPluginTests"`
- SQL fixtures: `tests/sql-query-analyzer.Tests/fixtures/**/*.sql` (copied to output)
- Benchmarks: `benchmarks/sql-query-analyzer.Benchmarks/` (BenchmarkDotNet), `dotnet run -c Release --project benchmarks/sql-query-analyzer.Benchmarks`
- CI: `.github/workflows/` runs restore/build/test on push and PR to `main`
- Main csproj excludes `tests/**`, `benchmarks/**`, `examples/**` from compile; `InternalsVisibleTo` for the test project and Moq

## Lint / Format

- `make lint` = `dotnet build /p:EnforceCodeStyleInBuild=true`
- `make format` / `make format-check` = `dotnet format` (`--verify-no-changes`)
- Style rules in `.editorconfig`: 4-space indent, Allman braces, `System` usings first, separate import groups, final newline
- `Nullable` and `ImplicitUsings` enabled; `TreatWarningsAsErrors=false`

## Key Directories

- `Program.cs` - entry point; DI container setup (Microsoft.Extensions.DependencyInjection) and sample run
- `Services/` - core analysis: `IQueryAnalyzerService`, `AnalysisBuilder`, `ExplainPlanParserService`, index/plan analyzers, detector plugins
- `Plugins/` - `IAnalysisPlugin` / `AnalysisPluginBase` and concrete detectors (CartesianJoin, SelectStar, DistinctAbuse, UnboundedOrderBy)
- `Models/` - domain types (`QueryAnalysisResult`, `PerformanceIssue`, `QueryPlan`, `IndexRecommendation`, ...)
- `DTOs/` - API-facing DTOs and `IDtoMapper`
- `API/` - `AnalysisController`
- `CLI/` - `CliApplicationHost`, `CommandLineParser`, `CommandLineArguments`
- `Repositories/` - `IQueryRepository`, `IAnalysisRepository`, `IIndexRepository`
- `Configuration/` - `IConnectionConfiguration`, `SqlServerConfiguration`, `SqlQueryAnalyzerOptions`, `ProfilerSettings`
- `Caching/` - `QueryAnalysisCache`, `QueryCacheKeyGenerator`
- `Middleware/`, `Events/`, `Validation/`, `Scoring/`, `Export/`, `Formatters/`, `Visualization/`, `Diagnostics/`, `Integration/`, `BackgroundWorkers/`, `Utilities/`, `Extensions/`, `Constants/`, `Exceptions/`
- `Testing/` - `SampleQueryProvider` (test data helpers shipped in main assembly)
- `docs/` - one markdown page per class/feature, plus `faq.md`, `troubleshooting.md`, `docker-guide.md`
- `appsettings.example.json` - configuration template

## Conventions

- Namespace `SqlQueryAnalyzer.<Folder>`; assembly `SqlQueryAnalyzer`; file-scoped namespaces
- Every `.cs` file starts with `#nullable enable` and the author header block
- Interfaces prefixed `I`; services end in `Service`, plugins in `Plugin`, repositories in `Repository`
- Companion-file pattern per type: `Foo.cs`, `FooExtensions.cs`, `FooJsonExtensions.cs`, `FooValidation.cs`
- Async methods end in `Async`; DI registrations are singletons in `Program.cs`
- XML doc comments (`GenerateDocumentationFile=true`) on public types
- Regex-based detectors use `const string` patterns; see `REGEX_HARDENING_IMPLEMENTATION.md` and `verify_hardening.sh` for ReDoS rules
- Test naming: `Method_Scenario_ExpectedResult`; one test class per type (`FooTests`)
- Do not commit `bin/`, `obj/`, `postgres-data/`, `.aider*`
