#!/bin/bash

# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

set -e

echo "SQL Query Analyzer - Docker Quick Start"
echo "========================================"
echo ""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Check Docker installation
if ! command -v docker &> /dev/null; then
    echo -e "${RED}✗ Docker is not installed${NC}"
    echo "Install Docker from https://docs.docker.com/get-docker/"
    exit 1
fi

echo -e "${GREEN}✓${NC} Docker found"

# Check Docker Compose installation
if ! command -v docker-compose &> /dev/null; then
    echo -e "${RED}✗ Docker Compose is not installed${NC}"
    echo "Install Docker Compose from https://docs.docker.com/compose/install/"
    exit 1
fi

echo -e "${GREEN}✓${NC} Docker Compose found"

# Check if running in project directory
if [ ! -f "docker-compose.yml" ]; then
    echo -e "${RED}✗ docker-compose.yml not found${NC}"
    echo "Please run this script from the project root directory"
    exit 1
fi

echo ""
echo "Starting SQL Query Analyzer stack..."
echo ""

# Start services
docker-compose up -d

# Wait for services to be ready
echo "Waiting for services to be ready..."
sleep 10

# Check SQL Server health
echo "Checking SQL Server connectivity..."
for i in {1..30}; do
    if docker-compose exec -T sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourPassword123!' -Q "SELECT 1" &>/dev/null; then
        echo -e "${GREEN}✓${NC} SQL Server is ready"
        break
    fi
    if [ $i -eq 30 ]; then
        echo -e "${RED}✗ SQL Server failed to start${NC}"
        docker-compose logs sqlserver
        exit 1
    fi
    echo "  Retry $i/30..."
    sleep 2
done

# Check analyzer service
echo "Checking Analyzer service..."
for i in {1..20}; do
    if curl -s http://localhost:5000/health &>/dev/null; then
        echo -e "${GREEN}✓${NC} Analyzer is ready"
        break
    fi
    if [ $i -eq 20 ]; then
        echo -e "${YELLOW}⚠${NC} Analyzer may not be fully ready yet"
    fi
    echo "  Retry $i/20..."
    sleep 1
done

echo ""
echo -e "${GREEN}✓ All services started successfully!${NC}"
echo ""
echo "Access information:"
echo "  - Analyzer API: http://localhost:5000"
echo "  - SQL Server: localhost:1433"
echo "    User: sa"
echo "    Password: YourPassword123!"
echo ""
echo "View logs:"
echo "  docker-compose logs -f analyzer"
echo "  docker-compose logs -f sqlserver"
echo ""
echo "Stop services:"
echo "  docker-compose down"
echo ""
echo "Next steps:"
echo "  1. Read the README.md for usage examples"
echo "  2. Check docs/ directory for detailed guides"
echo "  3. Run example programs in examples/ directory"
echo ""
