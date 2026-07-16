# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Analyzer correctness suite backed by SQL fixtures (`tests/.../fixtures/`) that
  feeds known-bad queries (SELECT *, missing WHERE, cartesian/implicit join,
  N+1 access pattern, leading-wildcard LIKE, function-on-column) and asserts each
  detection rule fires.
- Focused quickstart and rule catalog documentation under `docs/QUICKSTART.md`.

## [2.0.2] - 2026-05-04

### Fixed
- Explain-plan parser no longer chokes on the PostgreSQL 17 output format.
- Added a regression test covering the PostgreSQL 17 plan shape.

## [2.0.1] - 2026-04-20

### Changed
- Enabled nullable reference types across the codebase and applied code-quality
  cleanups.
- Stopped treating compiler warnings as errors so downstream builds are less
  brittle.

### Fixed
- Hardened edge-case handling and input validation in the pattern analyzer.

## [2.0.0] - 2026-03-15

### Added
- Major v2.0 feature work: service layer, middleware, and external integrations.
- Dependency-injection wiring for the analyzer services.
- Docker support and container health-check endpoints.
- CLI, API, background workers, caching, and export surfaces.

### Changed
- Retargeted the project to .NET 10.0 and refreshed dependencies.

### Migration
- See the v2.0 migration guide and Docker documentation in `docs/`.

## [1.1.0] - 2020-10-02

### Added
- BenchmarkDotNet benchmark project covering the analyzer hot paths.

### Performance
- Optimized hot paths: source-generated regexes (`[GeneratedRegex]`) replace
  per-call `Regex` construction, `FrozenSet` used for O(1) keyword membership,
  and table extraction collapsed into a single regex pass.

## [1.0.0] - 2020-05-07

### Added
- Initial public release: core SQL query analysis engine.
- SQL pattern detection (SELECT *, implicit joins, leading-wildcard LIKE,
  function-on-column, subqueries, N+1 access patterns).
- Explain-plan parsing and index recommendation engine.
- Unit tests, examples, documentation, and community files.
- CI/CD workflows and NuGet packaging configuration.

[Unreleased]: https://github.com/sarmkadan/sql-query-analyzer/compare/v2.0.2...HEAD
[2.0.2]: https://github.com/sarmkadan/sql-query-analyzer/compare/v2.0.1...v2.0.2
[2.0.1]: https://github.com/sarmkadan/sql-query-analyzer/compare/v2.0.0...v2.0.1
[2.0.0]: https://github.com/sarmkadan/sql-query-analyzer/compare/v1.1.0...v2.0.0
[1.1.0]: https://github.com/sarmkadan/sql-query-analyzer/compare/v1.0.0...v1.1.0
[1.0.0]: https://github.com/sarmkadan/sql-query-analyzer/releases/tag/v1.0.0
