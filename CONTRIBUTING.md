// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Contributing to SQL Query Analyzer

Thank you for your interest in contributing! This document provides guidelines and instructions for contributing to the project.

## Code of Conduct

All contributors are expected to:
- Be respectful and inclusive
- Provide constructive feedback
- Focus on the code, not the person
- Help create a welcoming environment

## Getting Started

### Prerequisites
- .NET 10 SDK
- Git
- A GitHub account
- A SQL Server/PostgreSQL instance (optional)

### Setup Development Environment

```bash
# Fork and clone
git clone https://github.com/YOUR_USERNAME/sql-query-analyzer.git
cd sql-query-analyzer

# Restore and build
dotnet restore
dotnet build

# Run tests
dotnet test

# View available commands
make help
```

## Making Changes

### Branch Naming Convention

```
feature/brief-description    # New features
bugfix/issue-number-name     # Bug fixes
docs/description             # Documentation
refactor/description         # Code refactoring
```

Example:
```bash
git checkout -b feature/n-plus-one-detection
git checkout -b bugfix/false-positive-selectstar
```

### Code Style

Follow .NET standard conventions:
- **Naming**: PascalCase for classes/methods, camelCase for variables
- **Formatting**: See `.editorconfig` for rules
- **Comments**: Only when WHY is non-obvious

```csharp
// Good: Explains the business logic
if (fragmentationPercent > 30)
{
    // Physical index reorganization is more efficient than rebuild
    // for fragmentation between 10-30%
    scripts.Add(GenerateReorganizeScript(index));
}

// Bad: Restates what the code does
if (fragmentationPercent > 30)
{
    // Rebuild the index
    scripts.Add(GenerateRebuildScript(index));
}
```

### File Headers

Every .cs file must include this header:

```csharp
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================
```

### Code Examples

```csharp
// Naming conventions
public class PerformanceIssueDetector  // PascalCase for classes
{
    private readonly ILogger _logger;  // camelCase with underscore for fields
    
    public async Task<List<PerformanceIssue>> DetectAsync(DatabaseQuery query)
    {
        var issues = new List<PerformanceIssue>();
        var queryText = query.QueryText;  // camelCase for local variables
        
        if (DetectsSelectStar(queryText))
        {
            issues.Add(new PerformanceIssue
            {
                IssueType = "SelectStar",
                Severity = IssueSeverity.Info,
                Description = "SELECT * should specify columns"
            });
        }
        
        return issues;
    }
    
    private bool DetectsSelectStar(string query)
    {
        return Regex.IsMatch(query, @"SELECT\s+\*");
    }
}
```

## Testing Requirements

### Unit Tests
Every feature should have corresponding tests:

```csharp
[TestClass]
public class PerformanceIssueDetectorTests
{
    [TestMethod]
    public async Task DetectAsync_WithSelectStar_ReturnsIssue()
    {
        // Arrange
        var detector = new PerformanceIssueDetector();
        var query = new DatabaseQuery { QueryText = "SELECT * FROM Orders" };
        
        // Act
        var issues = await detector.DetectAsync(query);
        
        // Assert
        Assert.IsTrue(issues.Any(i => i.IssueType == "SelectStar"));
    }
}
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter TestClass=PerformanceIssueDetectorTests

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverageFormat=lcov
```

### Minimum Coverage
Aim for:
- **Services**: 80%+ coverage
- **Utilities**: 75%+ coverage
- **Models**: 60%+ coverage (often simple)

## Submitting Changes

### 1. Commit Messages

Use clear, descriptive messages:

```
Feature: Add MySQL execution plan parser

- Implement parser for MySQL EXPLAIN FORMAT=JSON output
- Add unit tests for common operations
- Update documentation with MySQL support details

Closes #123
```

Good format:
```
[Type]: Brief description (50 chars max)

Detailed explanation if needed (72 chars per line)
- Bullet points for multiple changes
- Each point should be meaningful

References: #123, #456
Closes: #789
```

Types: `Feature`, `Fix`, `Docs`, `Refactor`, `Test`, `Chore`

### 2. Create Pull Request

```bash
# Push your branch
git push origin feature/my-feature

# Create PR on GitHub
# Title: Feature: Add MySQL execution plan parser
# Description: (use template provided)
```

### PR Checklist

- [ ] Tests added/updated
- [ ] Code formatted (`make format`)
- [ ] No linting errors (`make lint`)
- [ ] Documentation updated
- [ ] Changelog updated
- [ ] Commit messages are clear

## Documentation

### Update README.md
If adding new features, update the README with:
- Feature description
- Usage example
- API reference

### Add Example Code
For significant features, add an example in `/examples`:

```csharp
// examples/MyFeatureExample.cs
public class MyFeatureExample
{
    static async Task Main()
    {
        // Complete, runnable example
        var analyzer = GetAnalyzer();
        var result = await analyzer.AnalyzeQueryAsync("SELECT ...");
        
        Console.WriteLine(result.PerformanceScore);
    }
}
```

### Update Changelog
Add entries to `CHANGELOG.md`:

```markdown
## [1.3.0] - 2026-06-01

### Added
- **Feature Name**: Description
  - Sub-feature 1
  - Sub-feature 2

### Fixed
- Fixed bug #123: Description
```

## Architecture & Design

### Adding New Detectors

```csharp
// 1. Create detector class
public class MyIssueDetector : IAnalysisStrategy
{
    public async Task<List<PerformanceIssue>> AnalyzeAsync(DatabaseQuery query)
    {
        // Detection logic
        return issues;
    }
}

// 2. Register in DI (Program.cs or extension method)
services.AddScoped<IAnalysisStrategy, MyIssueDetector>();

// 3. Add tests
[TestClass]
public class MyIssueDetectorTests { }

// 4. Document in API reference
```

### Adding Database Support

1. Create parser in `/Services`
2. Implement `IExplainPlanParserService` interface
3. Add database-specific handling
4. Update docker-compose.yml
5. Add integration tests
6. Update documentation

## Performance Considerations

When contributing, consider:

1. **Time Complexity**: Keep O(n) or O(n log n)
2. **Memory Usage**: Avoid loading entire queries into memory
3. **Caching**: Leverage existing caching where possible
4. **Parallelization**: Use Task.WhenAll for independent operations

```csharp
// Good: Parallel processing
var tasks = queries.Select(q => AnalyzeAsync(q));
var results = await Task.WhenAll(tasks);

// Bad: Sequential processing of independent items
foreach (var query in queries)
{
    await AnalyzeAsync(query);  // Slow!
}
```

## Security Considerations

- **Input Validation**: Always validate user input
- **SQL Injection**: Never concatenate user input into SQL
- **Secrets**: Never commit credentials or API keys
- **Dependencies**: Keep packages updated

### Before Committing

```bash
# Check for secrets
git secrets --scan

# Update dependencies
dotnet list package --outdated

# Run security analyzer
dotnet tool install -g dotnet-sonaranalyzer
```

## Debugging Tips

### Enable Verbose Logging

```bash
ANALYZER_LOG_LEVEL=Debug dotnet run
```

### Attach Debugger

```csharp
System.Diagnostics.Debugger.Launch();  // Or use IDE's breakpoints
```

### Test Specific Issue Type

```csharp
var query = new DatabaseQuery { QueryText = "SELECT * FROM Orders" };
var detector = new SelectStarDetector();
var issues = await detector.DetectAsync(query);
```

## Reviewers & Maintainers

- All PRs require review
- Address review feedback with commits (don't force-push)
- Be patient - reviews take time

## Release Process

Releases follow [Semantic Versioning](https://semver.org/):
- **MAJOR**: Breaking changes
- **MINOR**: New features (backwards compatible)
- **PATCH**: Bug fixes

## Questions?

- **Issues**: [GitHub Issues](https://github.com/sarmkadan/sql-query-analyzer/issues)
- **Discussions**: [GitHub Discussions](https://github.com/sarmkadan/sql-query-analyzer/discussions)
- **Email**: Check repository for contact info

## Thank You!

Your contributions make this project better. Thank you for investing time to improve SQL Query Analyzer!

---

**Last Updated**: 2026-05-04
