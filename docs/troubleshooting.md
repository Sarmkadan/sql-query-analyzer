// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Troubleshooting Guide

Solutions to common issues with SQL Query Analyzer.

## Installation Issues

### Issue: ".NET SDK not found"

**Problem**: Command `dotnet` is not recognized

**Solutions**:

```bash
# 1. Check if .NET is installed
dotnet --version

# 2. Windows: Use .NET installer
# Download from https://dotnet.microsoft.com/download/dotnet/10.0

# 3. macOS: Use Homebrew
brew install dotnet

# 4. Linux: Package manager
# Ubuntu/Debian:
sudo apt-get install dotnet-sdk-10.0

# CentOS/RHEL:
sudo yum install dotnet-sdk-10.0
```

### Issue: "dotnet restore" fails with network error

**Problem**: Cannot reach NuGet package sources

**Solutions**:

```bash
# 1. Check internet connection
ping api.nuget.org

# 2. Configure proxy (if behind corporate proxy)
dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org --configfile nuget.config

# 3. Clear NuGet cache
dotnet nuget locals all --clear

# 4. Try restore again
dotnet restore
```

### Issue: "Project file not found"

**Problem**: Cannot locate project file

**Solutions**:

```bash
# 1. Check current directory
pwd
ls -la sql-query-analyzer.csproj

# 2. Navigate to project root
cd sql-query-analyzer/

# 3. Verify file exists
file sql-query-analyzer.csproj
```

## Build Issues

### Issue: "Build failed with compilation errors"

**Problem**: C# syntax errors or missing dependencies

**Solutions**:

```bash
# 1. Clean previous builds
dotnet clean

# 2. Restore dependencies again
dotnet restore

# 3. Build with verbose output
dotnet build --verbosity detailed

# 4. Check for compatibility
dotnet sdk check

# 5. Update .NET SDK
dotnet sdk update
```

### Issue: "Target framework not supported"

**Problem**: .NET version mismatch

**Solutions**:

```bash
# 1. Check required framework
grep TargetFramework sql-query-analyzer.csproj
# Should show: net10.0

# 2. Verify installed frameworks
dotnet --list-runtimes

# 3. Install required framework
# Download and install .NET 10 from https://dotnet.microsoft.com/download/dotnet/10.0
```

### Issue: "NuGet package version conflict"

**Problem**: Dependency version mismatch

**Solutions**:

```bash
# 1. Check dependencies
dotnet list package

# 2. Update all packages
dotnet package update

# 3. Restore specific version
dotnet add package PackageName --version 10.0.0

# 4. Clear cache
dotnet nuget locals all --clear && dotnet restore
```

## Database Connection Issues

### Issue: "Cannot connect to database"

**Problem**: Database connection fails

**Solutions**:

```bash
# 1. Verify connection string
echo $DB_SERVER $DB_USER $DB_NAME

# 2. Test basic connectivity
ping $DB_SERVER

# 3. SQL Server: Check if running
# Windows
Get-Service MSSQLSERVER | Select-Object Status

# Linux
sudo systemctl status mssql-server

# 4. Check firewall
netstat -an | grep 1433  # SQL Server
netstat -an | grep 5432  # PostgreSQL

# 5. Test with native tools
sqlcmd -S localhost -U sa -P 'password' -Q "SELECT 1"  # SQL Server
psql -h localhost -U postgres -c "SELECT 1"  # PostgreSQL
```

### Issue: "Login failed for user 'sa'"

**Problem**: Authentication error

**Solutions**:

```bash
# 1. Verify credentials
echo "User: $DB_USER"
echo "Server: $DB_SERVER"
echo "Database: $DB_NAME"

# 2. Check SQL Server is accepting connections
sqlcmd -S localhost

# 3. Reset sa password (SQL Server)
# Windows CMD as Administrator:
sqlcmd -S . -Q "ALTER LOGIN sa WITH PASSWORD = 'NewPassword123!'"

# 4. Check user permissions
sqlcmd -S localhost -U sa -P 'password' -Q "SELECT SUSER_NAME(), DB_NAME()"
```

### Issue: "Connection timeout"

**Problem**: Connection takes too long

**Solutions**:

```bash
# 1. Increase timeout in config
export DB_TIMEOUT=60  # 60 seconds instead of default 30

# 2. Check server load
# SQL Server
sqlcmd -S localhost -Q "SELECT @@CPU_BUSY, @@IO_BUSY"

# 3. Verify network connectivity
ping -c 5 $DB_SERVER
tracert $DB_SERVER  # Windows
traceroute $DB_SERVER  # Linux/macOS

# 4. Reduce batch size
# Analyze fewer queries at once
```

### Issue: "Database 'QueryAnalyzer' does not exist"

**Problem**: Missing database

**Solutions**:

```bash
# 1. Create database (SQL Server)
sqlcmd -S localhost -U sa -P 'password' -Q "CREATE DATABASE QueryAnalyzer"

# 2. Create database (PostgreSQL)
psql -U postgres -c "CREATE DATABASE query_analyzer"

# 3. Verify database exists
sqlcmd -S localhost -U sa -P 'password' -Q "SELECT * FROM sys.databases WHERE name = 'QueryAnalyzer'"

psql -l | grep query_analyzer
```

## Docker Issues

### Issue: "Docker daemon not running"

**Problem**: Docker service is stopped

**Solutions**:

```bash
# Start Docker
# Windows: Start Docker Desktop

# macOS
sudo /usr/local/bin/dockerd

# Linux
sudo systemctl start docker

# Verify
docker --version
docker ps
```

### Issue: "Cannot connect to Docker daemon"

**Problem**: Permission denied error

**Solutions**:

```bash
# Linux: Add user to docker group
sudo usermod -aG docker $USER
newgrp docker

# Test
docker ps

# Or use sudo
sudo docker ps
```

### Issue: "docker-compose: command not found"

**Problem**: Docker Compose not installed

**Solutions**:

```bash
# Install Docker Compose v2
docker run --rm -v /usr/local/bin:/output docker:cli plugin install docker/compose:latest

# Or use docker compose (v2 syntax)
docker compose up  # Instead of docker-compose up

# Verify installation
docker compose --version
```

### Issue: "Port 1433 already in use"

**Problem**: SQL Server port is taken

**Solutions**:

```bash
# 1. Find what's using the port
netstat -ano | findstr :1433  # Windows
lsof -i :1433  # macOS/Linux

# 2. Stop conflicting service
# Windows
Get-Process | Where-Object {$_.Id -eq 12345} | Stop-Process

# Linux
sudo kill -9 <PID>

# 3. Use different port in docker-compose.yml
ports:
  - "1434:1433"  # Map to 1434 instead
```

### Issue: "Out of memory" in Docker

**Problem**: Docker container crashes

**Solutions**:

```bash
# 1. Increase Docker memory limit
docker update --memory 2g sql-query-analyzer-app

# 2. Or in docker-compose.yml
services:
  analyzer:
    mem_limit: 2g

# 3. Check current usage
docker stats sql-query-analyzer-app

# 4. Reduce batch size or disable caching
ANALYZER_ENABLE_CACHE=false
ANALYZER_MAX_CACHE_SIZE=1000
```

## Performance Issues

### Issue: "Analysis is very slow"

**Problem**: Long analysis times

**Solutions**:

```bash
# 1. Enable caching
export ANALYZER_ENABLE_CACHE=true
export ANALYZER_CACHE_TTL=3600

# 2. Disable unused features
export ANALYZER_DETECT_NPLUS_ONE=false
export ANALYZER_PARSE_PLANS=false

# 3. Use async processing
# Run in parallel but with limits
var semaphore = new SemaphoreSlim(5);

# 4. Check system resources
free -h  # Memory
df -h    # Disk
top      # Process list
```

### Issue: "High memory usage"

**Problem**: Process uses too much RAM

**Solutions**:

```bash
# 1. Monitor memory
# Windows
Get-Process SqlQueryAnalyzer | Select-Object WorkingSet

# Linux
ps aux | grep SqlQueryAnalyzer

# 2. Reduce cache size
export ANALYZER_MAX_CACHE_SIZE=1000

# 3. Process in smaller batches
# Instead of 10,000 queries at once, do 100 at a time

# 4. Disable caching if not needed
export ANALYZER_ENABLE_CACHE=false
```

## Analysis Issues

### Issue: "False positives - detecting issues that aren't real"

**Problem**: Incorrect issue detection

**Solutions**:

```bash
# 1. Report the query to GitHub
# Include: exact query, expected vs actual result

# 2. Disable specific detector temporarily
# Modify code to skip detector or file issue

# 3. Use try-catch to handle specific cases
try
{
    var result = await analyzer.AnalyzeQueryAsync(query);
    // Filter out known false positives
    result.Issues = result.Issues
        .Where(i => i.IssueType != "KnownFalsePositive")
        .ToList();
}
catch (Exception ex)
{
    logger.LogError(ex, "Analysis failed");
}
```

### Issue: "Not detecting expected issues"

**Problem**: Missed issue detection

**Solutions**:

```bash
# 1. Verify detectors are enabled
export ANALYZER_DETECT_NPLUS_ONE=true
export ANALYZER_SUGGEST_INDEXES=true
export ANALYZER_PARSE_PLANS=true

# 2. Enable debug logging
export ANALYZER_LOG_LEVEL=Debug

# 3. Check log output
# Should show detection attempts

# 4. Report missing detection
# Open GitHub issue with: exact query, why it should be detected
```

### Issue: "Reports not being generated"

**Problem**: Report files not created

**Solutions**:

```bash
# 1. Check directory permissions
ls -la ./reports/
chmod 755 ./reports/

# 2. Verify output directory exists
mkdir -p ./reports

# 3. Check disk space
df -h  # macOS/Linux
dir C:  # Windows

# 4. Try manual report generation
var report = ReportGenerator.GenerateHtmlReport(result);
File.WriteAllText("output.html", report);

# 5. Check for exceptions in logs
ANALYZER_LOG_LEVEL=Debug
```

## API Issues

### Issue: "HTTP 500 error when calling API"

**Problem**: Server error in API endpoint

**Solutions**:

```bash
# 1. Check server logs
docker logs sql-query-analyzer-app

# 2. Verify request format
curl -X POST http://localhost:5000/api/analyze \
  -H "Content-Type: application/json" \
  -d '{"queryText":"SELECT * FROM Orders"}'

# 3. Check database connectivity
# Ensure DB_SERVER, DB_USER, DB_PASSWORD are set

# 4. Restart service
docker-compose restart analyzer
```

### Issue: "CORS error when calling from web app"

**Problem**: Cross-Origin Resource Sharing blocked

**Solutions**:

```csharp
// In Program.cs, add CORS configuration
services.AddCors(options =>
{
    options.AddPolicy("AllowWeb", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

app.UseCors("AllowWeb");
```

## Logging & Debugging

### Enable Detailed Logging

```bash
# Maximum verbosity
export ANALYZER_LOG_LEVEL=Debug

# Save logs to file
export LOGGING__FILE__PATH=/var/log/analyzer.log
```

### Check Logs

```bash
# Docker
docker logs -f sql-query-analyzer-app

# Local
dotnet run 2>&1 | tee analyzer.log
```

### Get Support

If issues persist:

1. **Collect Information**:
   - Logs (with `ANALYZER_LOG_LEVEL=Debug`)
   - Environment variables
   - System information (`dotnet --info`)
   - Reproduction steps

2. **Report on GitHub**:
   - Title: Brief description
   - Steps to reproduce
   - Logs (sanitized)
   - Expected vs actual behavior

3. **Contact Maintainers**:
   - Repository discussions
   - GitHub issues

---

**Last Updated**: 2026-05-04  
**Version**: 1.0
