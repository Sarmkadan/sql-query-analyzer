// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Installation Guide

Complete instructions for installing SQL Query Analyzer on various platforms.

## Quick Start (2 minutes)

### Docker Compose (Recommended)

```bash
git clone https://github.com/sarmkadan/sql-query-analyzer.git
cd sql-query-analyzer
docker-compose up
# Access at http://localhost:5000
```

### From Source (5 minutes)

```bash
git clone https://github.com/sarmkadan/sql-query-analyzer.git
cd sql-query-analyzer
dotnet restore
dotnet build --configuration Release
dotnet run
```

---

## Detailed Installation

### Prerequisites

- **Operating System**: Windows 10+, macOS 10.15+, or Linux (any distro)
- **.NET 10 SDK**: Download from https://dotnet.microsoft.com/download/dotnet/10.0
- **Git**: For cloning the repository
- **Database** (optional): SQL Server 2016+, PostgreSQL 12+, or MySQL 5.7+

### Step 1: Install .NET 10 SDK

#### Windows

```powershell
# Option 1: Download and run installer
# https://dotnet.microsoft.com/download/dotnet/10.0

# Option 2: Using Chocolatey
choco install dotnet-sdk-10.0

# Verify installation
dotnet --version  # Should show 10.x.x
```

#### macOS

```bash
# Using Homebrew (recommended)
brew install dotnet

# Or download installer
# https://dotnet.microsoft.com/download/dotnet/10.0

# Verify
dotnet --version
```

#### Linux (Ubuntu/Debian)

```bash
# Add Microsoft repository
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Install .NET SDK
sudo apt-get update
sudo apt-get install -y dotnet-sdk-10.0

# Verify
dotnet --version
```

#### Linux (CentOS/RHEL)

```bash
sudo dnf install dotnet-sdk-10.0
dotnet --version
```

### Step 2: Clone Repository

```bash
git clone https://github.com/sarmkadan/sql-query-analyzer.git
cd sql-query-analyzer
```

### Step 3: Restore Dependencies

```bash
dotnet restore
```

If you encounter network issues:

```bash
# Clear cache and retry
dotnet nuget locals all --clear
dotnet restore
```

### Step 4: Build the Project

**Development Build**:
```bash
dotnet build
```

**Release Build**:
```bash
dotnet build --configuration Release
```

**Production Build with Trimming**:
```bash
dotnet build --configuration Release -p:PublishTrimmed=true
```

### Step 5: Run the Application

```bash
dotnet run
```

Or run compiled executable:

```bash
# Windows
./bin/Release/net10.0/SqlQueryAnalyzer.exe

# Linux/macOS
./bin/Release/net10.0/SqlQueryAnalyzer
```

---

## Installation Methods

### Method 1: Docker (Production Ready)

**Advantages**: Isolated, reproducible, includes database

**Requirements**: Docker 20.10+, Docker Compose 2.0+

```bash
# Clone repository
git clone https://github.com/sarmkadan/sql-query-analyzer.git
cd sql-query-analyzer

# Check Docker installation
docker --version
docker-compose --version

# Start services
docker-compose up -d

# Verify services running
docker ps
docker logs sql-query-analyzer-app

# Access application
curl http://localhost:5000/health
```

**Environment Files**:

Create `.env` file:
```
DB_PASSWORD=YourSecurePassword123!
ANALYZER_LOG_LEVEL=Information
```

### Method 2: Source Installation

**Advantages**: Full control, easy development, native performance

**Requirements**: .NET 10 SDK

```bash
# Clone and build
git clone https://github.com/sarmkadan/sql-query-analyzer.git
cd sql-query-analyzer

# Build in Release mode
dotnet build --configuration Release

# Run
dotnet run --configuration Release

# Or publish and run
dotnet publish --configuration Release
./bin/Release/net10.0/publish/SqlQueryAnalyzer
```

### Method 3: Package (NuGet)

**Advantages**: Use as library in your project

**Requirements**: .NET 10 SDK

```bash
# Create new project
dotnet new console -n MyAnalyzer
cd MyAnalyzer

# Add package reference
dotnet add package SqlQueryAnalyzer

# Use in code
var analyzer = new QueryAnalyzerService();
var result = await analyzer.AnalyzeQueryAsync("SELECT * FROM Orders");
```

### Method 4: Kubernetes

**Advantages**: Enterprise deployment, auto-scaling, service mesh ready

**Requirements**: Kubernetes cluster, kubectl

See [docs/deployment.md](docs/deployment.md) for Kubernetes manifests.

### Method 5: Cloud Platforms

#### Azure App Service

```bash
az webapp create --resource-group mygroup --plan myplan --name analyzer
```

See [docs/deployment.md](docs/deployment.md) for detailed Azure deployment.

#### AWS Elastic Container Service (ECS)

```bash
# Use provided task definition
aws ecs create-service --cluster analyzer-cluster ...
```

#### Google Cloud Run

```bash
gcloud run deploy sql-query-analyzer \
  --source . \
  --region us-central1
```

---

## Configuration

### Database Configuration

Set environment variables:

```bash
# SQL Server
export DB_SERVER=localhost
export DB_PORT=1433
export DB_USER=sa
export DB_PASSWORD=YourPassword123!
export DB_NAME=QueryAnalyzer

# PostgreSQL
export DB_SERVER=localhost
export DB_PORT=5432
export DB_USER=postgres
export DB_PASSWORD=postgres
export DB_NAME=query_analyzer

# MySQL
export DB_SERVER=localhost
export DB_PORT=3306
export DB_USER=root
export DB_PASSWORD=password
export DB_NAME=query_analyzer
```

Or create `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=QueryAnalyzer;User Id=sa;Password=YourPassword123!;Encrypt=false"
  },
  "AnalyzerSettings": {
    "EnableCache": true,
    "CacheTtl": 3600,
    "LogLevel": "Information"
  }
}
```

### Advanced Configuration

Create `.env` file or environment variables:

```bash
# Caching
ANALYZER_ENABLE_CACHE=true
ANALYZER_CACHE_TTL=3600
ANALYZER_MAX_CACHE_SIZE=10000

# Feature flags
ANALYZER_DETECT_NPLUS_ONE=true
ANALYZER_SUGGEST_INDEXES=true
ANALYZER_PARSE_PLANS=true

# Performance
ANALYZER_MAX_QUERY_SIZE=100000
ANALYZER_TIMEOUT_SECONDS=30

# Logging
ANALYZER_LOG_LEVEL=Information
```

---

## Verification

### 1. Test Installation

```bash
# Run unit tests
dotnet test

# Expected output: All tests passed
```

### 2. Test Database Connection

```bash
# If using database features
export DB_SERVER=localhost
export DB_USER=sa
export DB_PASSWORD=YourPassword123!

dotnet run
# Should connect successfully
```

### 3. Test API Endpoint

```bash
# If running as API
curl -X POST http://localhost:5000/api/analyze \
  -H "Content-Type: application/json" \
  -d '{"queryText":"SELECT * FROM Orders"}'

# Expected: JSON response with analysis result
```

---

## Troubleshooting Installation

### Issue: "dotnet: command not found"

```bash
# Check if .NET is installed
which dotnet

# If not found, install:
# Follow Step 1 (Install .NET 10 SDK) above

# Or add to PATH
export PATH="/usr/local/share/dotnet:$PATH"
```

### Issue: "Project file not found"

```bash
# Verify you're in correct directory
pwd
ls sql-query-analyzer.csproj

# If not found, navigate to project root
cd sql-query-analyzer/
```

### Issue: "NuGet restore fails"

```bash
# Clear cache
dotnet nuget locals all --clear

# Try again
dotnet restore

# If behind proxy, configure:
dotnet nuget add source https://api.nuget.org/v3/index.json \
  -n nuget.org \
  --username <username> \
  --password <password> \
  --store-password-in-clear-text
```

### Issue: "Build fails with errors"

```bash
# Clean previous builds
dotnet clean

# Rebuild
dotnet build --verbosity diagnostic

# Update .NET SDK
dotnet sdk update

# Verify .NET version
dotnet --version  # Should show 10.x.x
```

---

## Next Steps

After installation:

1. **Read Documentation**
   - [Getting Started](docs/getting-started.md)
   - [Usage Examples](README.md#📖-usage-examples)

2. **Run Examples**
   - `dotnet run examples/BasicAnalyzer.cs`
   - `dotnet run examples/BatchAnalyzer.cs`

3. **Configure Database**
   - Set up SQL Server/PostgreSQL connection
   - Run your first analysis

4. **Explore Features**
   - Index analysis
   - Execution plan parsing
   - Report generation

---

## Getting Help

If you encounter issues:

1. **Check [FAQ](docs/faq.md)** for common questions
2. **Review [Troubleshooting Guide](docs/troubleshooting.md)**
3. **Search [GitHub Issues](https://github.com/sarmkadan/sql-query-analyzer/issues)**
4. **Open new issue** with:
   - Error message
   - Steps to reproduce
   - Environment details (`dotnet --info`)

---

**Installation Guide Version**: 1.0  
**Last Updated**: 2026-05-04
