# Contributing to SQL Query Analyzer

## Prerequisites

- **.NET SDK 10.0** (pinned to `10.0.100` in `global.json`, `rollForward: latestMinor`, so any 10.0.x SDK works)
- Git
- Docker (optional, only for `make docker-*` targets)

## Build

```bash
dotnet restore
dotnet build --configuration Release    # or: make build
```

The main project is `sql-query-analyzer.csproj`; the solution is `sql-query-analyzer.sln`.
The build must finish with no new warnings. `Nullable` is enabled, so new code must compile cleanly under nullable reference types.

## Run tests

```bash
dotnet test --configuration Release    # or: make test
```

- Test project: `tests/sql-query-analyzer.Tests/` (xUnit 2.9, FluentAssertions 8, Moq 4.20).
- Single test class:
  `dotnet test --filter "FullyQualifiedName~CartesianJoinPluginTests"`
- Benchmarks (`benchmarks/sql-query-analyzer.Benchmarks/`) are not part of the test run.

## Coding conventions

- Follow `.editorconfig`: 4-space indent, Allman braces, `System` usings first, separate import groups, final newline.
- Run `make format-check` before opening a PR, and `make format` to fix. `make lint` runs the analyzers.
- Every `.cs` file starts with `#nullable enable` and the author header block.
- Namespace `SqlQueryAnalyzer.<Folder>` with file-scoped namespaces.
- Interfaces are prefixed `I`; services end in `Service`, plugins in `Plugin`, repositories in `Repository`.
- Test names follow `Method_Scenario_ExpectedResult`, one test class per type (`FooTests`).
- Public types get XML doc comments.
- Do not commit `bin/`, `obj/`, `postgres-data/`, or `.aider*`.

## Branch naming

- `feature/<short-description>` for new functionality
- `bugfix/<issue-number>` or `bugfix/<short-description>` for fixes
- Branch from and target `main`.

## Commit messages

Use [Conventional Commits](https://www.conventionalcommits.org/), as in the existing history:

```
feat(models): add buffer cache hit ratio extension
fix: add null guards to ErrorHandlingMiddleware
docs: add CONTRIBUTING.md
```

Use the imperative mood in the subject line, and keep it under about 72 characters.

## Pull requests

1. Open a PR against `main`.
2. CI must pass. The `CI` workflow (`.github/workflows/ci.yml`) runs restore, build, and test on .NET 10; `CodeQL` also runs on PRs.
3. Get one approval from a maintainer.
4. Describe what changed and why, and link any related issue.

By contributing, you agree that your contributions are licensed under the MIT License (see `LICENSE`).
