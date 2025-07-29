# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

# Multi-stage build for optimized Docker image
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder

WORKDIR /build

# Copy project file and restore dependencies
COPY sql-query-analyzer.csproj .
RUN dotnet restore

# Copy source code
COPY . .

# Build release configuration
RUN dotnet build --configuration Release --no-restore

# Publish application
RUN dotnet publish --configuration Release --no-build -o /app

# Runtime image
FROM mcr.microsoft.com/dotnet/runtime:10.0

WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application from builder
COPY --from=builder /app .

# Create non-root user for security
RUN useradd -m -u 1000 analyzer && chown -R analyzer:analyzer /app
USER analyzer

# Environment configuration
ENV DOTNET_RunAsUser=0
ENV ASPNETCORE_ENVIRONMENT=Production

# Default to SQL Server (can be overridden)
ENV DB_SERVER=localhost
ENV DB_PORT=1433
ENV DB_USER=sa
ENV DB_PASSWORD=YourPassword123!
ENV DB_NAME=QueryAnalyzer
ENV ANALYZER_LOG_LEVEL=Information

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:5000/health || exit 1

# Port exposure
EXPOSE 5000

# Run application
ENTRYPOINT ["dotnet", "SqlQueryAnalyzer.dll"]
CMD ["--help"]
