// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Getting Started with SQL Query Analyzer

This guide will walk you through installing, configuring, and using SQL Query Analyzer for your first query analysis.

## Prerequisites

- **.NET 10 SDK** or later ([Download](https://dotnet.microsoft.com/download/dotnet/10.0))
- **SQL Server 2016+** or **PostgreSQL 12+** (optional for database features)
- **Git** for cloning the repository
- **Docker** (optional, if using containerized setup)

## Installation (5 minutes)

### Step 1: Clone Repository

```bash
git clone https://github.com/sarmkadan/sql-query-analyzer.git
cd sql-query-analyzer
```

### Step 2: Verify .NET Installation

```bash
dotnet --version
# Output should show 10.x.x or higher
```

### Step 3: Restore and Build

```bash
dotnet restore
dotnet build
```

### Step 4: Verify Installation

```bash
dotnet run
# Should output: Starting SQL Query Analyzer v1.0.0
```

## Quick Start (10 minutes)

### Basic Analysis

Create a file `analyze-query.cs`:

```csharp
using SqlQueryAnalyzer.Services;
using SqlQueryAnalyzer.Models;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection()
    .AddScoped<IQueryAnalyzerService, QueryAnalyzerService>()
    .BuildServiceProvider();

var analyzer = services.GetRequiredService<IQueryAnalyzerService>();

var result = await analyzer.AnalyzeQueryAsync(
    "SELECT * FROM Orders WHERE CustomerId = 1"
);

Console.WriteLine($"Score: {result.PerformanceScore:F1}/100");
Console.WriteLine($"Issues: {result.Issues.Count}");
foreach (var issue in result.Issues)
{
    Console.WriteLine($"  - {issue.IssueType}: {issue.Description}");
}
```

Run it:

```bash
dotnet run analyze-query.cs
```

### Expected Output

```
Score: 65.0/100
Issues: 1
  - SelectStar: SELECT * should specify columns
```

## Configuration Setup

### Environment Variables

Set database connection via environment:

```bash
# SQL Server
export DB_SERVER=localhost
export DB_PORT=1433
export DB_USER=sa
export DB_PASSWORD=YourPassword123!
export DB_NAME=QueryAnalyzer

# Or PostgreSQL
export DB_PORT=5432
```

### Configuration File (Optional)

Create `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=QueryAnalyzer;User Id=sa;Password=YourPassword123!"
  },
  "AnalyzerSettings": {
    "EnableNPlusOneDetection": true,
    "EnableIndexAnalysis": true,
    "CacheEnabled": true,
    "LogLevel": "Information"
  }
}
```

## Docker Quick Start

### Using Docker Compose (Easiest)

```bash
docker-compose up
```

This starts:
- SQL Server database container
- Analyzer application

### Custom Database

```bash
# PostgreSQL
docker-compose -f docker-compose.yml -f docker-compose.postgres.yml up

# MySQL
docker-compose -f docker-compose.yml -f docker-compose.mysql.yml up
```

## Common First Tasks

### Task 1: Analyze a Complex Query

```csharp
var query = @"
    SELECT o.Id, o.OrderDate, c.Name, COUNT(*) as ItemCount
    FROM Orders o
    LEFT JOIN Customers c ON o.CustomerId = c.Id
    LEFT JOIN OrderItems oi ON o.Id = oi.OrderId
    WHERE o.OrderDate > GETDATE() - 30
    AND c.Country IN ('USA', 'Canada')
    GROUP BY o.Id, o.OrderDate, c.Name
    HAVING COUNT(*) > 5
";

var result = await analyzer.AnalyzeQueryAsync(query);
PrintAnalysisReport(result);
```

### Task 2: Check Index Health

```csharp
var indexAnalyzer = services.GetRequiredService<IIndexAnalyzerService>();

var fragmented = await indexAnalyzer.GetFragmentedIndexesAsync();
Console.WriteLine($"Fragmented indexes: {fragmented.Count}");

foreach (var idx in fragmented.Where(x => x.FragmentationPercent > 30))
{
    Console.WriteLine($"{idx.TableName}.{idx.IndexName}: {idx.FragmentationPercent:F1}%");
    Console.WriteLine($"Action: REBUILD");
}
```

### Task 3: Generate Report

```csharp
var result = await analyzer.AnalyzeQueryAsync(queryText);

// Save multiple formats
var textReport = ReportGenerator.GenerateTextReport(result);
await File.WriteAllTextAsync("report.txt", textReport);

var htmlReport = ReportGenerator.GenerateHtmlReport(result);
await File.WriteAllTextAsync("report.html", htmlReport);

var jsonReport = ReportGenerator.GenerateJsonReport(result);
await File.WriteAllTextAsync("report.json", jsonReport);

Console.WriteLine("Reports generated:");
Console.WriteLine("  - report.txt (human-readable)");
Console.WriteLine("  - report.html (interactive)");
Console.WriteLine("  - report.json (machine-readable)");
```

## Troubleshooting

### Issue: "Cannot find .NET SDK"

```bash
# Install .NET 10
# Windows: Download from https://dotnet.microsoft.com/download/dotnet/10.0
# macOS: brew install dotnet
# Linux: Follow OS-specific instructions
```

### Issue: "Cannot connect to database"

```bash
# Test connection
dotnet tool install -g dotnet-sql-cli
sqlcmd -S localhost -U sa -P YourPassword123! -Q "SELECT 1"

# If fails, check firewall
# SQL Server: Port 1433
# PostgreSQL: Port 5432
```

### Issue: "Permission denied" on Linux

```bash
chmod +x ./SqlQueryAnalyzer.dll
sudo usermod -aG docker $USER
newgrp docker
```

## Next Steps

1. **Read Architecture Guide**: [architecture.md](./architecture.md)
2. **Explore API Reference**: [api-reference.md](./api-reference.md)
3. **View Examples**: [../examples/](../examples/)
4. **Check FAQ**: [faq.md](./faq.md)

## Support

- **Issues**: [GitHub Issues](https://github.com/sarmkadan/sql-query-analyzer/issues)
- **Discussions**: [GitHub Discussions](https://github.com/sarmkadan/sql-query-analyzer/discussions)
- **Email**: Contact via repository

---

**Happy analyzing!** 🚀
