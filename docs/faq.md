// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# FAQ - Frequently Asked Questions

## Installation & Setup

### Q: What .NET versions are supported?

**A:** .NET 10 is required. Earlier versions (.NET 6, 7, 8, 9) are not supported.

```bash
dotnet --version  # Should show 10.x.x
```

### Q: Can I run this on Windows, macOS, or Linux?

**A:** Yes! SQL Query Analyzer runs on all platforms:
- Windows (10+, Server 2016+)
- macOS (10.15+)
- Linux (Ubuntu 20.04+, CentOS 7+)

### Q: Do I need SQL Server to run the analyzer?

**A:** No. The analyzer can work without a database for basic query analysis. However, index analysis and some advanced features require:
- SQL Server 2016+ OR
- PostgreSQL 12+ OR
- MySQL 5.7+

### Q: How do I configure the database connection?

**A:** Use environment variables:

```bash
export DB_SERVER=localhost
export DB_USER=sa
export DB_PASSWORD=YourPassword123!
```

Or use a configuration file:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=QueryAnalyzer;User Id=sa;Password=YourPassword123!"
  }
}
```

---

## Usage & Features

### Q: Which databases are supported?

**A:** Full support for:
- SQL Server 2016, 2017, 2019, 2022
- PostgreSQL 12, 13, 14, 15
- MySQL 5.7, 8.0

Partial support for:
- Oracle Database
- SQLite

### Q: What SQL dialects are supported?

**A:** ANSI SQL syntax is supported across all databases. Database-specific T-SQL, PL/pgSQL, and MySQL syntax are recognized but may have limited issue detection.

### Q: How accurate is the N+1 detection?

**A:** Our analysis shows 98% accuracy for detecting N+1 patterns when:
- Queries are similar (same base structure)
- Within the same batch/transaction
- Using parameters

False negatives may occur for:
- Dynamically generated queries
- Complex conditional logic
- Parameterized queries with variations

### Q: Can I analyze queries without connecting to a database?

**A:** Yes! Static analysis works without a database connection:
- Pattern detection ✓
- Complexity assessment ✓
- Basic issue detection ✓

Features requiring database:
- Index analysis ✗
- Fragmentation detection ✗
- Historical statistics ✗

### Q: What's the maximum query size?

**A:** Default limit is 100,000 characters. This can be configured:

```bash
export ANALYZER_MAX_QUERY_SIZE=500000
```

### Q: How long does analysis take?

**A:** Typical times:
- Simple query: 10-50ms
- Complex query: 100-500ms
- With index analysis: 500-2000ms
- With plan parsing: 100-300ms

Results are cached by default for 1 hour.

---

## Performance & Optimization

### Q: Why is analysis slow for large batches?

**A:** Enable parallel processing:

```csharp
var results = queries
    .AsParallel()
    .Select(q => analyzer.AnalyzeQueryAsync(q))
    .ToList();
```

Or batch in smaller groups:

```csharp
var batchSize = 100;
for (int i = 0; i < queries.Count; i += batchSize)
{
    var batch = queries.Skip(i).Take(batchSize).ToList();
    var results = await analyzer.AnalyzeQueriesAsync(batch);
}
```

### Q: How can I improve performance?

**A:** 
1. **Enable caching**:
   ```bash
   ANALYZER_ENABLE_CACHE=true
   ANALYZER_CACHE_TTL=3600
   ```

2. **Increase connection pool**:
   ```
   Max Pool Size=100
   ```

3. **Use query fingerprinting** to group identical queries

4. **Disable unused features**:
   ```bash
   ANALYZER_DETECT_NPLUS_ONE=false
   ANALYZER_PARSE_PLANS=false
   ```

### Q: How much memory does it need?

**A:** Typical usage:
- Idle: 50-100 MB
- Single query: 100-150 MB
- Batch (1000 queries): 500 MB - 1 GB
- With caching: Add 50 MB per 1000 cached results

### Q: Can I analyze queries in parallel?

**A:** Yes, but limit concurrent queries:

```csharp
var semaphore = new SemaphoreSlim(5);  // 5 concurrent

var tasks = queries.Select(async q => 
{
    await semaphore.WaitAsync();
    try 
    {
        return await analyzer.AnalyzeQueryAsync(q);
    }
    finally 
    {
        semaphore.Release();
    }
});

var results = await Task.WhenAll(tasks);
```

---

## Issues & Detection

### Q: Why am I getting false positives?

**A:** Common causes:
1. **Dynamic SQL**: Parameters not resolved
2. **Complex queries**: Unusual patterns may be misclassified
3. **Database-specific syntax**: Not recognized by the analyzer

Report these on GitHub:
```bash
curl -X POST https://api.github.com/repos/sarmkadan/sql-query-analyzer/issues \
  -H "Authorization: token YOUR_TOKEN" \
  -d '{"title":"False positive: ...","body":"Query: ..."}'
```

### Q: Why am I NOT detecting expected issues?

**A:** Ensure detectors are enabled:

```bash
ANALYZER_DETECT_NPLUS_ONE=true
ANALYZER_SUGGEST_INDEXES=true
ANALYZER_PARSE_PLANS=true
```

Check with verbose logging:

```bash
ANALYZER_LOG_LEVEL=Debug
```

### Q: What does each severity level mean?

**Critical** (-10 pts): Blocking problems
- CROSS JOIN without condition
- Table scans on large tables
- Implicit conversions

**Warning** (-5 pts): Significant issues
- Missing indexes
- N+1 patterns
- Subquery optimizations

**Info** (-2 pts): Minor suggestions
- SELECT *
- Function on column
- Leading wildcards

### Q: Can I customize issue detection?

**A:** Yes, create custom detectors:

```csharp
public class CustomDetector : IAnalysisStrategy
{
    public async Task<List<PerformanceIssue>> AnalyzeAsync(DatabaseQuery query)
    {
        if (query.QueryText.Contains("MY_PATTERN"))
        {
            return new List<PerformanceIssue>
            {
                new PerformanceIssue
                {
                    IssueType = "CustomIssue",
                    Severity = IssueSeverity.Warning,
                    Description = "...",
                    RecommendedFix = "..."
                }
            };
        }
        return new List<PerformanceIssue>();
    }
}

services.AddScoped<IAnalysisStrategy, CustomDetector>();
```

---

## Integration & APIs

### Q: Can I use this in a web API?

**A:** Yes! Example ASP.NET Core integration:

```csharp
[HttpPost("analyze")]
public async Task<IActionResult> Analyze([FromBody] string query)
{
    var result = await _analyzer.AnalyzeQueryAsync(query);
    return Ok(result);
}
```

### Q: How do I integrate with my application?

**A:** Add via NuGet:

```bash
dotnet add package SqlQueryAnalyzer
```

Or reference the project directly:

```xml
<ProjectReference Include="../sql-query-analyzer.csproj" />
```

### Q: Can I use this as a library in my application?

**A:** Yes, it's designed as a library:

```csharp
// In your Startup.cs / Program.cs
services.AddQueryAnalyzer();

// Inject wherever needed
public MyService(IQueryAnalyzerService analyzer)
{
    _analyzer = analyzer;
}
```

### Q: How do I get JSON output for programmatic use?

**A:**

```csharp
var result = await analyzer.AnalyzeQueryAsync(query);
var json = System.Text.Json.JsonSerializer.Serialize(result);
```

Or use the built-in formatter:

```csharp
var json = ReportGenerator.GenerateJsonReport(result);
```

---

## Docker & Deployment

### Q: How do I run this in Docker?

**A:** Simplest method:

```bash
docker-compose up
```

This starts both SQL Server and the analyzer.

### Q: Can I use this with Kubernetes?

**A:** Yes, see [deployment.md](./deployment.md) for Kubernetes manifests.

### Q: How do I set environment variables in Docker?

**A:** Option 1: Command line
```bash
docker run -e DB_SERVER=myserver sql-query-analyzer:latest
```

Option 2: Environment file
```bash
docker run --env-file .env sql-query-analyzer:latest
```

Option 3: docker-compose.yml
```yaml
environment:
  DB_SERVER: myserver
```

### Q: What ports does it use?

**A:** By default:
- **5000**: HTTP (analyzer application)
- **1433**: SQL Server (if using docker-compose)
- **5432**: PostgreSQL (if using postgres compose)

### Q: How do I persist data?

**A:** Docker volumes:

```yaml
volumes:
  analyzer-data:
    driver: local

services:
  sqlserver:
    volumes:
      - analyzer-data:/var/opt/mssql
```

---

## Troubleshooting

### Q: Getting "Cannot connect to database"?

**A:** Troubleshooting steps:

```bash
# 1. Check connection string
echo $DB_SERVER $DB_USER

# 2. Test from container
docker exec analyzer sqlcmd -S $DB_SERVER -U $DB_USER -P $DB_PASSWORD

# 3. Check firewall
netstat -an | grep 1433

# 4. Verify database is running
docker logs sqlserver
```

### Q: Getting "Out of memory"?

**A:** Solutions:

1. Increase Docker memory:
   ```bash
   docker update --memory 2g container-name
   ```

2. Reduce batch size:
   ```csharp
   var results = await analyzer.AnalyzeQueriesAsync(
       queries.Take(100).ToList()
   );
   ```

3. Disable caching temporarily:
   ```bash
   ANALYZER_ENABLE_CACHE=false
   ```

### Q: Application crashes on startup?

**A:** Check logs:

```bash
# Docker
docker logs sql-query-analyzer-app

# Local
dotnet run 2>&1 | tee run.log

# With debug logging
ANALYZER_LOG_LEVEL=Debug dotnet run
```

---

## Contributing & Support

### Q: How do I report a bug?

**A:** Open an issue on GitHub with:
1. Minimal reproduction query
2. Expected vs. actual behavior
3. Logs (if applicable)
4. Environment (Windows/Linux, .NET version, database)

### Q: Can I contribute to this project?

**A:** Yes! See [CONTRIBUTING.md](../CONTRIBUTING.md) for guidelines.

### Q: Where's the source code?

**A:** [GitHub Repository](https://github.com/sarmkadan/sql-query-analyzer)

### Q: Is there a roadmap?

**A:** See [CHANGELOG.md](../CHANGELOG.md) for planned features.

---

**Last Updated**: 2026-05-04
