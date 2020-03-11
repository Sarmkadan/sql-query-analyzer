// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.2.0] - 2026-05-04

### Added
- **PostgreSQL Query Plan Support**: Parse EXPLAIN (ANALYZE, BUFFERS) output
- **Real-time Analysis Web Dashboard**: Live query analysis with WebSocket updates
- **Webhook Notifications**: Integrate with Slack/Teams for critical issues
- **Batch Processing API**: Analyze 1000+ queries in single request
- **Performance Baselines**: Track performance over time and detect regressions
- **Custom Issue Rules**: Define organization-specific detection patterns
- **Multi-language Output**: French, German, Spanish, Chinese translations
- **Query Fingerprinting**: Group identical queries across different parameters

### Changed
- Performance improvement: 40% faster analysis on large queries
- Index suggestion algorithm now considers write/read ratios
- Improved N+1 detection accuracy from 92% to 98%
- Documentation expanded with 50+ code examples
- CI/CD pipeline now runs on .NET 10 stable release

### Fixed
- Fixed false positive "LeadingWildcard" detection for ESCAPE clause
- Corrected fragmentation calculation for partitioned indexes
- Resolved memory leak in batch processing for 10K+ query sets
- Fixed edge case in subquery optimization detection with CTEs

### Deprecated
- `GetAnalysisHistoryAsync()` with string parameter (use DatabaseQuery overload)
- XML-based plan parsing (transitioning to native database APIs)

## [1.1.0] - 2026-04-15

### Added
- **MySQL 8.0 Execution Plan Support**: Full analysis of MySQL EXPLAIN output
- **Docker Compose Profiles**: Separate configs for SQL Server/PostgreSQL/MySQL
- **Performance Metrics Dashboard**: Visual charts and trend analysis
- **Index Maintenance Script Generation**: Auto-REBUILD/REORGANIZE SQL
- **Query Normalization**: Group similar queries for pattern analysis
- **Extended Statistics**: Detailed metrics on CPU, I/O, row counts
- **Health Check Endpoint**: Kubernetes readiness/liveness probes

### Changed
- Unified database abstraction layer for multi-vendor support
- Service registration moved to extension methods for cleaner DI
- Logging now uses structured logging with semantic properties
- Report generation refactored to pluggable formatter system

### Fixed
- Resolved index suggestion conflicts on overlapping columns
- Fixed query parser to handle nested parentheses correctly
- Corrected estimated cost calculations for APPLY operations
- Fixed rare crash when parsing malformed execution plans

## [1.0.0] - 2026-03-20

### Added
- **Core Query Analysis**: Performance scoring and complexity assessment
- **Issue Detection (15 types)**: Patterns for common SQL anti-patterns
- **Index Analysis**: Fragmentation, unused index, and suggestion engine
- **Execution Plan Parsing**: SQL Server XML plan analysis with cost breakdown
- **Report Generation**: Text, HTML, JSON, CSV output formats
- **Caching System**: Performance optimization with configurable TTL
- **Docker Support**: Complete containerization with docker-compose
- **CLI Interface**: Command-line tool for batch processing
- **REST API**: ASP.NET Core endpoints for integration
- **Background Processing**: Queue-based batch analysis service
- **Configuration Management**: Environment variables and appsettings.json
- **Error Handling**: Comprehensive exception hierarchy and logging
- **Unit Tests**: 150+ test cases covering core functionality
- **Documentation**: Complete API reference and usage examples

### Security
- Input validation on all query text
- SQL injection detection analyzer
- Connection string encryption in configuration
- Rate limiting middleware for API endpoints

## [0.5.0] - 2026-02-20

### Added
- Beta release for testing
- Core analysis engine (incomplete)
- Basic issue detection (8 types)
- SQL Server support only
- CLI prototype

### Known Issues
- High false positive rate in N+1 detection
- Index suggestions not optimized for write-heavy workloads
- Memory usage grows with query complexity
- No support for parameterized queries

---

## Version Legend

- **Major (X.0.0)**: Breaking changes to API or significant features
- **Minor (0.X.0)**: New features without breaking existing API
- **Patch (0.0.X)**: Bug fixes and performance improvements

## Upgrade Guide

### From 1.1.0 → 1.2.0

```csharp
// Old code (still works)
var history = await analyzer.GetAnalysisHistoryAsync("query_text", 10);

// New recommended approach
var query = new DatabaseQuery { QueryText = "..." };
var history = await analyzer.GetAnalysisHistoryAsync(query, 10);
```

### From 1.0.0 → 1.1.0

No breaking changes. All existing code continues to work.

```csharp
// New features
services.AddAnalyzerWithMetrics();  // Includes metrics and dashboard

var metrics = analyzer.GetPerformanceMetrics();
var charts = new PerformanceCharts(metrics);
```

---

## Planned Features

### 2.0.0 (Q3 2026)
- GraphQL API for flexible query results
- Machine learning-based issue prediction
- Multi-tenant SaaS support
- Advanced query optimization suggestions
- Distributed tracing integration

### 2.1.0 (Q4 2026)
- Oracle Database full support
- Query recommendation engine
- Performance baseline management
- Integration with SSMS/Azure Data Studio
- Cost estimation for cloud databases

---

**Last Updated**: 2026-05-04
