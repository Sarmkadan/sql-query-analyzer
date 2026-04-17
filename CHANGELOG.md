// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.0] - 2026-02-22

### Added
- **Query Profiler**: Execution plan visualization and performance suggestions
- **Docker Support**: Multi-stage builds with health check endpoints (/health, /health/ready)
- **Integration Test Suite**: Comprehensive xUnit test coverage
- **Migration Guide**: v2.0 migration guide and Docker documentation

### Changed
- Upgraded to .NET 10.0
- Modern C# features (records, primary constructors)
- Improved API consistency across all services

### Fixed
- Various edge cases found through integration testing

## [0.9.0] - 2025-11-17

### Added
- **SQL Injection Detector**: Pattern-based security analysis on query input
- **Input Validation Layer**: `QueryValidator` rejects malformed or oversized input
- **Webhook Notifications**: `WebhookNotificationService` posts critical issues to external URLs
- **Export Formats**: CSV and JSON export via `ExportService`
- **`BatchAnalysisProcessor`**: Parallel processing with configurable `MaxDegreeOfParallelism`

### Changed
- `IQueryAnalyzerService` interface finalized — no further breaking changes planned
- Analysis pipeline hardened against null inputs and empty query strings
- Logging now emits structured properties for all severity levels

### Fixed
- `PerformanceMetricsCalculator` division by zero on empty query lists
- `ReportGenerator` HTML output escaped special characters incorrectly
- Configuration binding ignored `CacheTtlSeconds` when set via environment variable

## [0.8.0] - 2025-10-13

### Added
- **N+1 Query Detection**: `DetectNPlusOneAsync` identifies repeated parameterized patterns
- **Extended Issue Types**: 18 total — added `OrCondition`, `ImplicitConversion`, `IneffectiveJoin`
- **Query Normalization**: `QueryNormalizer` strips literals for pattern grouping and cache keying
- **`StatisticsAggregator`**: Summarize issue frequency across batches
- **`PerformanceMetricCollector`**: Collects CPU, I/O, and row-count metrics per analysis run

### Changed
- `SqlPatternAnalyzer` regex patterns reviewed for correctness and performance
- Issue severity thresholds tuned: cross-joins now always Critical
- `AnalyzerSettings` supports environment variable overrides for all properties

### Fixed
- `IndexAnalyzerService` returned duplicate suggestions for compound indexes
- N+1 detection fired on single-query inputs with no repetition context

## [0.7.0] - 2025-09-01

### Added
- **Caching Layer**: `QueryAnalysisCache` with configurable TTL avoids redundant analysis
- **`QueryCacheKeyGenerator`**: Deterministic fingerprint from normalized query text
- **Rate Limiting**: Sliding-window middleware for HTTP API endpoints
- **`AnalysisBuilder`**: Fluent API for constructing analysis requests programmatically
- **`DtoMapper`**: Clean separation between internal models and API response DTOs

### Changed
- `IQueryAnalyzerService.AnalyzeQueryAsync` overload accepts both `string` and `DatabaseQuery`
- Dependency injection wiring moved to extension methods (`AddSqlQueryAnalyzer()`)
- Reduced allocations in hot-path pattern matching by pre-compiling regexes

### Fixed
- Cache TTL was always read as zero when loaded from `appsettings.json`
- `AnalysisPipeline` middleware swallowed exceptions instead of re-throwing

## [0.6.0] - 2025-07-28

### Added
- **CLI Interface**: `CommandLineParser` and `CliApplicationHost` for terminal-driven analysis
- **`SampleQueryProvider`**: Built-in test queries for demos and integration testing
- **`DatabaseConnectionValidator`**: Validate connection strings before running analysis
- **`StringExtensions`**: Shared helpers used across formatters and validators

### Changed
- `ReportGenerator` split into pluggable `IResultFormatter` implementations
- Configuration consolidated into `AnalyzerSettings` loaded from `appsettings.json`
- Dockerfile updated to multi-stage build; image size reduced by 60%

### Fixed
- CLI argument parser crashed on unknown flags instead of printing usage
- HTML report contained unescaped `<` and `>` in query text blocks

## [0.5.0] - 2025-06-09

### Added
- **Report Generation**: Text, HTML, JSON, and CSV output via `ReportGenerator`
- **Execution Plan Analysis**: `QueryPlanAnalyzerService` and `ExplainPlanParserService` parse SQL Server XML plans
- **`QueryPlan` model**: Typed representation of plan nodes with cost and row estimates
- **Issue Severity Enum**: Critical / Warning / Info classification on all detected issues

### Changed
- `QueryAnalysisResult` extended with `PerformanceScore` (0-100) and `Complexity` enum
- `PerformanceIssue` records now include `ImpactScore` and `RecommendedFix`
- Analysis result serialization uses camelCase for JSON consumers

### Fixed
- `ExplainPlanParserService` threw on plans with missing `EstimateRows` attributes
- Execution cost totals were summed incorrectly for plans with nested loop joins

## [0.4.0] - 2025-04-28

### Added
- **Index Suggestions**: `IndexAnalyzerService` proposes indexes with ROI estimates
- **Fragmentation Detection**: Identify indexes above configurable thresholds
- **Unused Index Discovery**: Surface indexes with zero seek/scan activity
- **`IndexSuggestion` model**: Captures table, columns, estimated size, and ROI
- **`Index` model**: Tracks usage stats — seeks, scans, lookups, updates, fragmentation

### Changed
- `IIndexAnalyzerService` interface introduced; `IndexAnalyzerService` becomes injectable
- `PerformanceIssue.IssueType` changed from `string` to typed enum (`IssueTypes` constants)

### Fixed
- Index analysis returned suggestions for system tables (`sys.*`)
- Fragmentation threshold comparison used `>` instead of `>=`

## [0.3.0] - 2025-03-24

### Added
- **8 Issue Detectors**: `SelectStar`, `MissingIndex`, `TableScan`, `LeadingWildcard`, `FunctionOnColumn`, `SubqueryOptimization`, `CrossJoin`, `NonSargable`
- **Performance Scoring**: Weighted deductions per issue type and severity
- **`SqlPatternAnalyzer`**: Regex-driven pattern matching for all supported issue types
- **`PerformanceMetricsCalculator`**: Score computation and metric aggregation utilities
- **Complexity Classification**: Simple / Moderate / Complex based on join count and subqueries

### Changed
- `QueryAnalysisResult` now aggregates all issue lists into a single `Issues` collection
- `PerformanceIssue` includes `Description`, `RecommendedFix`, and `AffectedColumns`

### Fixed
- Pattern detector triggered on SQL comments containing keywords
- Complexity score overflowed on deeply nested subqueries

## [0.2.0] - 2025-02-17

### Added
- **`QueryValidator`**: Rejects empty strings, oversized queries, and obviously invalid SQL
- **`QueryNormalizer`**: Strips whitespace and normalizes keyword casing for comparison
- **`DatabaseQuery` model**: Wraps query text with metadata (database, user, timestamp)
- **`QueryStatistics` model**: Placeholder for execution metrics (CPU, I/O, row counts)
- Repository interfaces: `IQueryRepository`, `IAnalysisRepository`

### Changed
- Project restructured into layered folders: `Models/`, `Services/`, `Utilities/`, `Repositories/`
- `IQueryAnalyzerService` interface extracted; concrete implementation becomes injectable

## [0.1.0] - 2025-01-20

### Added
- Initial project scaffold: .NET 10 console application
- `QueryAnalysisResult` and `PerformanceIssue` core models
- `IQueryAnalyzerService` skeleton with `AnalyzeQueryAsync` stub
- Basic `SELECT *` detection proof-of-concept
- MIT license and initial README

---

## Version Legend

- **Major (X.0.0)**: Breaking changes to API or significant architectural shifts
- **Minor (0.X.0)**: New features without breaking existing API
- **Patch (0.0.X)**: Bug fixes and performance improvements

---

**Last Updated**: 2025-12-22
