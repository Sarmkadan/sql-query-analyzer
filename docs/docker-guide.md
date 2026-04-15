# Docker Guide for SQL Query Analyzer v2.0

## Quick Start with Docker

SQL Query Analyzer v2.0 provides official Docker images for easy deployment across different environments. This guide covers all aspects of running the analyzer in containers.

---

## Table of Contents

- [Quick Start with Docker](#quick-start-with-docker)
- [Docker Compose Usage](#docker-compose-usage)
- [Environment Variables Reference](#environment-variables-reference)
- [Production Deployment Checklist](#production-deployment-checklist)
- [Database Configuration](#database-configuration)
- [Volume Mounts](#volume-mounts)
- [Health Checks](#health-checks)
- [Network Configuration](#network-configuration)
- [Security Considerations](#security-considerations)
- [Troubleshooting](#troubleshooting)

---

## Quick Start with Docker

### Prerequisites

- Docker Engine 20.10+ or Docker Desktop 4.0+
- Docker Compose v2+ (recommended for production)
- At least 2GB of available RAM
- Ports 8080 (HTTP) and 8081 (metrics) available

### One-Command Setup

```bash
# Clone the repository
git clone https://github.com/sarmkadan/sql-query-analyzer.git
cd sql-query-analyzer

# Build and start the application with SQL Server
docker-compose up --build

# Wait for startup (~30-60 seconds)
# Access the web interface at http://localhost:8080
```

### Verify Installation

```bash
# Check container status
docker ps

# Expected output:
# CONTAINER ID   IMAGE                     COMMAND                  PORTS                    NAMES
# abc12345678   sql-query-analyzer:latest  "dotnet SqlQueryAnaly..."   0.0.0.0:8080->80/tcp   sql-query-analyzer

# Test API endpoint
curl http://localhost:8080/api/health
# Expected: {"status":"healthy"}
```

---

## Docker Compose Usage

### Basic Setup (SQL Server)

```yaml
# docker-compose.yml
version: '3.8'

services:
  sql-query-analyzer:
    image: sql-query-analyzer:latest
    build: .
    ports:
      - "8080:80"
      - "8081:8081"  # Metrics endpoint
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - DB_SERVER=sqlserver
      - DB_NAME=QueryAnalyzerDB
      - DB_USER=sa
      - DB_PASSWORD=YourStrongPassword123!
      - DB_TIMEOUT=30
    depends_on:
      sqlserver:
        condition: service_healthy
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:80/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      SA_PASSWORD: "YourStrongPassword123!"
      ACCEPT_EULA: "Y"
      MSSQL_PID: "Developer"
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql
    healthcheck:
      test: ["CMD", "/opt/mssql-tools/bin/sqlcmd", "-S", "localhost", "-U", "sa", "-P", "YourStrongPassword123!", "-Q", "SELECT 1"]
      interval: 10s
      timeout: 5s
      retries: 10
      start_period: 10s

volumes:
  sqlserver_data:
```

### PostgreSQL Configuration

```yaml
# docker-compose.postgres.yml
version: '3.8'

services:
  sql-query-analyzer:
    environment:
      - DB_TYPE=PostgreSQL
      - DB_SERVER=postgres
      - DB_PORT=5432
      - DB_NAME=query_analyzer
      - DB_USER=postgres
      - DB_PASSWORD=postgres
    depends_on:
      postgres:
        condition: service_healthy

  postgres:
    image: postgres:15-alpine
    environment:
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: query_analyzer
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 5s
      timeout: 5s
      retries: 5

volumes:
  postgres_data:
```

### MySQL Configuration

```yaml
# docker-compose.mysql.yml
version: '3.8'

services:
  sql-query-analyzer:
    environment:
      - DB_TYPE=MySQL
      - DB_SERVER=mysql
      - DB_PORT=3306
      - DB_NAME=query_analyzer
      - DB_USER=root
      - DB_PASSWORD=mysql
    depends_on:
      mysql:
        condition: service_healthy

  mysql:
    image: mysql:8.0
    environment:
      MYSQL_ROOT_PASSWORD: mysql
      MYSQL_DATABASE: query_analyzer
    ports:
      - "3306:3306"
    volumes:
      - mysql_data:/var/lib/mysql
    healthcheck:
      test: ["CMD", "mysqladmin", "ping", "-h", "localhost"]
      interval: 5s
      timeout: 5s
      retries: 5

volumes:
  mysql_data:
```

### Combined Setup

```bash
# Run with SQL Server (default)
docker-compose up

# Run with PostgreSQL
docker-compose -f docker-compose.yml -f docker-compose.postgres.yml up

# Run with MySQL
docker-compose -f docker-compose.yml -f docker-compose.mysql.yml up
```

---

## Environment Variables Reference

### Database Configuration

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `DB_TYPE` | No | `SqlServer` | Database type: `SqlServer`, `PostgreSQL`, or `MySQL` |
| `DB_SERVER` | Yes | - | Database server hostname or IP |
| `DB_PORT` | No | `1433` (SQL Server), `5432` (PostgreSQL), `3306` (MySQL) | Database port |
| `DB_NAME` | Yes | - | Database name |
| `DB_USER` | Yes | - | Database username |
| `DB_PASSWORD` | Yes | - | Database password |
| `DB_TIMEOUT` | No | `30` | Connection timeout in seconds |
| `DB_CONNECTION_STRING` | No | - | Full connection string (overrides individual settings) |

### Application Configuration

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `ASPNETCORE_ENVIRONMENT` | No | `Development` | ASP.NET Core environment: `Development`, `Staging`, or `Production` |
| `ASPNETCORE_URLS` | No | `http://+:80` | Server URLs |
| `ANALYZER_LOG_LEVEL` | No | `Information` | Logging level: `Debug`, `Information`, `Warning`, `Error` |
| `ANALYZER_ENABLE_CACHE` | No | `true` | Enable result caching |
| `ANALYZER_CACHE_TTL` | No | `3600` | Cache time-to-live in seconds |
| `ANALYZER_MAX_QUERY_SIZE` | No | `100000` | Maximum query size in bytes |
| `ANALYZER_DETECT_NPLUS_ONE` | No | `true` | Enable N+1 query detection |
| `ANALYZER_SUGGEST_INDEXES` | No | `true` | Enable index suggestions |
| `ANALYZER_PARSE_PLANS` | No | `true` | Enable execution plan parsing |
| `ANALYZER_WEBHOOK_URL` | No | - | Webhook URL for notifications |
| `ANALYZER_WEBHOOK_SECRET` | No | - | Webhook secret for authentication |

### Security Configuration

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `ANALYZER_API_KEY` | No | - | API key for authentication |
| `ANALYZER_RATE_LIMIT` | No | `100` | Maximum requests per minute |
| `ANALYZER_CORS_ORIGINS` | No | `*` | Allowed CORS origins (comma-separated) |
| `ANALYZER_CORS_METHODS` | No | `GET,POST,PUT,DELETE` | Allowed CORS methods |

### Performance Configuration

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `ANALYZER_MAX_DEGREE_OF_PARALLELISM` | No | `4` | Maximum parallelism for batch analysis |
| `ANALYZER_BATCH_SIZE` | No | `100` | Default batch size for bulk operations |
| `ANALYZER_MAX_ANALYSIS_TIME_MS` | No | `5000` | Maximum analysis time per query in milliseconds |

### Example .env File

```bash
# Database Configuration
DB_TYPE=SqlServer
DB_SERVER=sqlserver.database.windows.net
DB_PORT=1433
DB_NAME=QueryAnalyzerDB
DB_USER=analyzer_user
DB_PASSWORD=ComplexPassword123!@#
DB_TIMEOUT=45

# Application Configuration
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:80;http://+:8081
ANALYZER_LOG_LEVEL=Warning
ANALYZER_ENABLE_CACHE=true
ANALYZER_CACHE_TTL=7200

# Security
ANALYZER_API_KEY=your-secure-api-key-here
ANALYZER_RATE_LIMIT=200

# Performance
ANALYZER_MAX_DEGREE_OF_PARALLELISM=8
```

---

## Production Deployment Checklist

### 1. Database Configuration ✓

- [ ] Database server is secure (firewall configured)
- [ ] Database user has limited permissions (read-only recommended)
- [ ] Database password is complex and stored securely
- [ ] Connection string is tested and working
- [ ] Database is backed up regularly


### 2. Application Configuration ✓


- [ ] Environment set to `Production`
- [ ] API keys are generated and configured
- [ ] Rate limiting is enabled
- [ ] CORS is restricted to known domains
- [ ] Logging level is set appropriately
- [ ] Cache TTL is configured for your workload


### 3. Security ✓


- [ ] HTTPS is enabled (use reverse proxy like Nginx or Traefik)
- [ ] API key authentication is enabled
- [ ] Rate limiting is configured
- [ ] Database credentials are not hardcoded
- [ ] Secrets are managed via Docker secrets or environment variables
- [ ] Regular security updates are applied

### 4. Performance ✓


- [ ] Resource limits are set (CPU and memory)
- [ ] Health checks are configured
- [ ] Auto-restart policies are set
- [ ] Batch analysis settings are optimized for your workload
- [ ] Parallelism is configured based on available cores

### 5. Monitoring ✓


- [ ] Health check endpoint is accessible
- [ ] Metrics endpoint is enabled
- [ ] Logging is configured for your monitoring system
- [ ] Alerts are set up for critical issues
- [ ] Container logs are persisted

### 6. Backup & Recovery ✓


- [ ] Container volumes are backed up
- [ ] Database backups are configured
- [ ] Disaster recovery plan is documented
- [ ] Rollback procedure is tested


### 7. Network Configuration ✓


- [ ] Ports are properly exposed
- [ ] Network policies are configured
- [ ] DNS/hostname is set up
- [ ] Load balancer is configured (if needed)


---

## Database Configuration


### SQL Server Configuration


**Recommended**: Use Azure SQL Database or SQL Server on a managed VM


```bash
# SQL Server connection examples
docker run -e DB_TYPE=SqlServer \
  -e DB_SERVER=your-sql-server.database.windows.net \
  -e DB_NAME=QueryAnalyzer \
  -e DB_USER=analyzer_user \
  -e DB_PASSWORD=ComplexPassword123! \
  sql-query-analyzer:latest
```

**Connection String Format**:
```
Server=tcp:your-sql-server.database.windows.net,1433;Database=QueryAnalyzer;User ID=analyzer_user;Password=ComplexPassword123!;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

### PostgreSQL Configuration


**Recommended**: Use Azure Database for PostgreSQL or managed PostgreSQL


```bash
# PostgreSQL connection
docker run -e DB_TYPE=PostgreSQL \
  -e DB_SERVER=your-postgres-server.postgres.database.azure.com \
  -e DB_PORT=5432 \
  -e DB_NAME=query_analyzer \
  -e DB_USER=analyzer_user \
  -e DB_PASSWORD=ComplexPassword123! \
  sql-query-analyzer:latest
```

**Connection String Format**:
```
Host=your-postgres-server.postgres.database.azure.com;Port=5432;Database=query_analyzer;Username=analyzer_user;Password=ComplexPassword123!;SSL Mode=Require;Trust Server Certificate=true;Pooling=true;Minimum Pool Size=0;Maximum Pool Size=100;Connection Lifetime=0;
```

### MySQL Configuration


**Recommended**: Use Azure Database for MySQL or managed MySQL


```bash
# MySQL connection
docker run -e DB_TYPE=MySQL \
  -e DB_SERVER=your-mysql-server.mysql.database.azure.com \
  -e DB_PORT=3306 \
  -e DB_NAME=query_analyzer \
  -e DB_USER=analyzer_user \
  -e DB_PASSWORD=ComplexPassword123! \
  sql-query-analyzer:latest
```

**Connection String Format**:
```
Server=your-mysql-server.mysql.database.azure.com;Database=query_analyzer;Uid=analyzer_user;Pwd=ComplexPassword123!;Port=3306;SslMode=Preferred;
```


---

## Volume Mounts

### Persistent Storage


```yaml
services:
  sql-query-analyzer:
    volumes:
      - ./config:/app/config:ro
      - ./logs:/app/logs
      - ./reports:/app/reports
      - analyzer_cache:/app/cache

volumes:
  analyzer_cache:
```

### Configuration Files

```bash
# Mount custom configuration
volumes:
  - ./appsettings.Production.json:/app/appsettings.Production.json:ro
```

### Logs

```bash
# Persist logs
volumes:
  - ./logs:/app/logs
```

### Reports

```bash
# Store generated reports
volumes:
  - ./reports:/app/reports
```

### Cache

```bash
# Persistent cache for better performance
volumes:
  - analyzer_cache:/app/cache
```

---

## Health Checks

### Built-in Health Endpoints

- **Liveness**: `GET /health/live` - Basic container health
- **Readiness**: `GET /health/ready` - Application ready to serve traffic
- **Startup**: `GET /health/startup` - Application startup status
- **Metrics**: `GET /metrics` - Prometheus metrics

### Docker Healthcheck Configuration

```yaml
services:
  sql-query-analyzer:
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:80/health/ready"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s
```

### Prometheus Metrics

```bash
# Enable metrics endpoint
ASPNETCORE_URLS=http://+:80;http://+:8081

# Access metrics at http://localhost:8081/metrics
```

**Example Prometheus scrape configuration**:
```yaml
scrape_configs:
  - job_name: 'sql-query-analyzer'
    scrape_interval: 15s
    static_configs:
      - targets: ['sql-query-analyzer:8081']
```

---

## Network Configuration

### Port Mapping

```yaml
services:
  sql-query-analyzer:
    ports:
      - "8080:80"      # HTTP API
      - "8081:8081"    # Metrics
      - "8443:443"     # HTTPS (if configured)
```

### Network Modes

**Bridge (default)**:
```yaml
network_mode: bridge
```

**Host (for performance)**:
```yaml
network_mode: host
```

**Custom network**:
```bash
# Create network first
docker network create sql-analyzer-network

# Then use it
services:
  sql-query-analyzer:
    networks:
      - sql-analyzer-network
```

### DNS Configuration

```yaml
services:
  sql-query-analyzer:
    dns:
      - 8.8.8.8
      - 8.8.4.4
    dns_search:
      - example.com
```

---

## Security Considerations

### Database Security

- **Use SSL/TLS**: Always enable SSL for database connections
- **Least privilege**: Database user should have read-only access
- **Network isolation**: Place database in private subnet
- **Firewall rules**: Restrict database port access

### Application Security

- **HTTPS**: Use reverse proxy (Nginx, Traefik, or cloud load balancer)
- **API keys**: Rotate keys regularly
- **Rate limiting**: Protect against brute force attacks
- **Input validation**: Validate all query inputs
- **Secrets management**: Use Docker secrets or vault solutions

### Container Security

- **Non-root user**: Run as non-root user
- **Read-only filesystem**: Where possible
- **Resource limits**: Set CPU and memory limits
- **Image scanning**: Scan images for vulnerabilities
- **Regular updates**: Keep base images updated

### Example Secure Configuration

```yaml
services:
  sql-query-analyzer:
    user: "1000:1000"
    read_only: true
    tmpfs:
      - /tmp
    cap_drop:
      - ALL
    cap_add:
      - NET_BIND_SERVICE
    mem_limit: 2g
    mem_reservation: 1g
    cpus: 2
```

---

## Troubleshooting

### Common Issues

#### 1. Connection Refused

**Symptoms**: `Cannot connect to database` errors

**Solutions**:
```bash
# Check database is running
docker ps | grep sqlserver

# Test database connectivity from container
docker exec -it sql-query-analyzer bash
nc -zv sqlserver 1433
```

#### 2. Authentication Failed

**Symptoms**: Login failed errors

**Solutions**:
```bash
# Verify credentials
# Check special characters in password are properly escaped

# Use connection string instead
export DB_CONNECTION_STRING="Server=...;Database=...;User ID=...;Password=...;"
```

#### 3. Out of Memory

**Symptoms**: Container crashes or OOM errors

**Solutions**:
```bash
# Increase memory limit
docker run -m 4g sql-query-analyzer:latest

# Or in docker-compose.yml
services:
  sql-query-analyzer:
    mem_limit: 4g
    mem_reservation: 2g
```

#### 4. Slow Performance

**Symptoms**: Analysis takes too long

**Solutions**:
```bash
# Increase parallelism
export ANALYZER_MAX_DEGREE_OF_PARALLELISM=8

# Increase cache TTL
export ANALYZER_CACHE_TTL=7200

# Reduce batch size for large analyses
export ANALYZER_BATCH_SIZE=50
```

#### 5. Port Conflicts

**Symptoms**: Container fails to start

**Solutions**:
```bash
# Check port usage
netstat -tuln | grep 8080

# Use different port
ports:
  - "8082:80"
```

### Debugging Commands

```bash
# View container logs
docker logs sql-query-analyzer

# Follow logs in real-time
docker logs -f sql-query-analyzer

# View last 100 lines
docker logs --tail=100 sql-query-analyzer

# Enter container shell
docker exec -it sql-query-analyzer bash

# Check environment variables
docker exec sql-query-analyzer env

# Test database connection manually
# (inside container)
dotnet SqlQueryAnalyzer.dll --test-connection
```

### Health Check Debugging

```bash
# Check health endpoint manually
curl http://localhost:80/health/ready

# Expected response: {"status":"healthy"}

# Check metrics
curl http://localhost:8081/metrics
```

---

## Advanced Topics

### Docker Secrets (for production)

```yaml
services:
  sql-query-analyzer:
    secrets:
      - db_password
      - api_key

secrets:
  db_password:
    external: true
  api_key:
    external: true
```

### Custom Configuration Files

```bash
# Mount custom appsettings.json
volumes:
  - ./config/appsettings.Production.json:/app/appsettings.Production.json:ro
```

### Multi-stage Builds

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
USER 1000
ENTRYPOINT ["dotnet", "SqlQueryAnalyzer.dll"]
```

### CI/CD Integration

**Example GitHub Actions workflow**:
```yaml
name: Docker Build and Push
on: [push]
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Login to Docker Hub
        uses: docker/login-action@v2
        with:
          username: ${{ secrets.DOCKER_HUB_USERNAME }}
          password: ${{ secrets.DOCKER_HUB_TOKEN }}
      - name: Build and push
        uses: docker/build-push-action@v3
        with:
          push: true
          tags: sqlqueryanalyzer/sql-query-analyzer:latest
```

---

## Performance Tuning

### Resource Allocation

**Development (minimum)**:
```yaml
mem_limit: 1g
mem_reservation: 512m
cpus: 1
```

**Production (recommended)**:
```yaml
mem_limit: 4g
mem_reservation: 2g
cpus: 4
```

### Batch Analysis Optimization

```bash
# Increase for large workloads
export ANALYZER_MAX_DEGREE_OF_PARALLELISM=8
export ANALYZER_BATCH_SIZE=200

# Adjust based on your database capacity
```

### Caching Strategy

```bash
# Longer TTL for production
export ANALYZER_CACHE_TTL=7200

# Disable cache for testing
export ANALYZER_ENABLE_CACHE=false
```

---

## Best Practices

### 1. Use Docker Compose for Development
✓ Simplifies setup with multiple services
✓ Easy to modify and test
✓ Reproducible environments


### 2. Use Environment Files for Production
✓ Keep secrets out of version control
✓ Easy to manage different environments
✓ Simple to update configurations


### 3. Monitor Resource Usage
✓ Set appropriate memory and CPU limits
✓ Configure health checks
✓ Monitor logs and metrics


### 4. Secure Your Deployment
✓ Use HTTPS (via reverse proxy)
✓ Enable authentication
✓ Restrict database access
✓ Regularly update images


### 5. Backup Regularly
✓ Backup container volumes
✓ Backup database
✓ Test restore procedures
✓ Document recovery steps


---

## Support & Resources

- **Documentation**: [docs/docker-guide.md](./docker-guide.md)
- **Examples**: [examples/docker-deployment/](./examples/docker-deployment/)
- **Issues**: [GitHub Issues](https://github.com/sarmkadan/sql-query-analyzer/issues)
- **Discussions**: [GitHub Discussions](https://github.com/sarmkadan/sql-query-analyzer/discussions)


---

## Version History


| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-05-18 | Initial v2.0 release with query profiler |
| 1.5.0 | 2025-11-15 | Multi-database support |
| 1.4.0 | 2025-08-20 | Health check endpoints |
| 1.3.0 | 2025-05-10 | Initial Docker support |

---

**Last Updated**: 2026-05-18
**Author**: Vladyslav Zaiets | https://sarmkadan.com
