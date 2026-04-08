#!/bin/bash

# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

# CLI Usage Examples for SQL Query Analyzer
# This script demonstrates common command-line usage patterns

set -e

echo "SQL Query Analyzer - CLI Usage Examples"
echo "======================================"
echo ""

# Color output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Example 1: Simple query analysis
echo -e "${BLUE}Example 1: Analyze a simple query${NC}"
echo "Command: dotnet run \"SELECT * FROM Orders\""
echo ""

# Example 2: Batch file analysis
echo -e "${BLUE}Example 2: Analyze queries from file${NC}"
echo "Command: dotnet run --file queries.txt --format json"
echo ""

# Example 3: With specific database
echo -e "${BLUE}Example 3: Analyze against specific database${NC}"
echo "Command:"
echo "  export DB_SERVER=prod-server"
echo "  export DB_NAME=ProductionDB"
echo "  dotnet run \"SELECT * FROM Orders\""
echo ""

# Example 4: Generate report
echo -e "${BLUE}Example 4: Generate HTML report${NC}"
echo "Command: dotnet run --query \"SELECT * FROM Orders\" --output-html report.html"
echo ""

# Example 5: Batch processing with output
echo -e "${BLUE}Example 5: Batch analysis with CSV export${NC}"
echo "Command: dotnet run --file queries.csv --output-csv results.csv --format verbose"
echo ""

# Example 6: API server mode
echo -e "${BLUE}Example 6: Start API server${NC}"
echo "Command: dotnet run --api --port 5000"
echo "Then access: http://localhost:5000"
echo ""

# Example 7: Index analysis
echo -e "${BLUE}Example 7: Analyze indexes${NC}"
echo "Command: dotnet run --analyze-indexes --table Orders"
echo ""

# Example 8: Execution plan analysis
echo -e "${BLUE}Example 8: Parse and analyze execution plan${NC}"
echo "Command: dotnet run --plan-file execution-plan.xml --analyze-plan"
echo ""

# Example 9: Performance profiling
echo -e "${BLUE}Example 9: Analyze with performance profiling${NC}"
echo "Command: dotnet run --query \"SELECT * FROM Orders\" --profile --timing"
echo ""

# Example 10: Multiple output formats
echo -e "${BLUE}Example 10: Generate all report formats${NC}"
echo "Command:"
echo "  dotnet run --query \"SELECT * FROM Orders\" --output-all ./reports/"
echo "  ls ./reports/"
echo "  # Outputs: report.txt, report.html, report.json, report.csv"
echo ""

# Practical examples
echo -e "${BLUE}=== PRACTICAL EXAMPLES ===${NC}"
echo ""

# Example: Docker usage
echo -e "${BLUE}Using Docker${NC}"
echo "1. Start services:"
echo "   docker-compose up"
echo ""
echo "2. Run analysis in container:"
echo "   docker exec sql-query-analyzer-app dotnet SqlQueryAnalyzer.dll --help"
echo ""

# Example: Schedule regular analysis
echo -e "${BLUE}Schedule Regular Analysis (cron)${NC}"
echo "Add to crontab:"
echo "0 2 * * * /app/analyze-scheduled.sh > /var/log/analyzer.log 2>&1"
echo ""

# Example: CI/CD Integration
echo -e "${BLUE}CI/CD Integration (GitHub Actions)${NC}"
echo "In .github/workflows/sql-check.yml:"
echo ""
cat << 'EOF'
- name: Analyze SQL Queries
  run: |
    dotnet run --file ./sql/queries.sql \
               --format json \
               --output results.json

    # Fail if critical issues found
    if grep '"IssueType":"CrossJoin"' results.json; then
      echo "Critical issues found!"
      exit 1
    fi
EOF
echo ""

# Environment variables reference
echo -e "${BLUE}=== ENVIRONMENT VARIABLES ===${NC}"
echo ""
echo "Database Configuration:"
echo "  DB_SERVER=localhost"
echo "  DB_PORT=1433"
echo "  DB_NAME=QueryAnalyzer"
echo "  DB_USER=sa"
echo "  DB_PASSWORD=YourPassword123!"
echo "  DB_TIMEOUT=30"
echo ""
echo "Analyzer Configuration:"
echo "  ANALYZER_LOG_LEVEL=Information"
echo "  ANALYZER_ENABLE_CACHE=true"
echo "  ANALYZER_CACHE_TTL=3600"
echo "  ANALYZER_MAX_QUERY_SIZE=100000"
echo ""
echo "Feature Flags:"
echo "  ANALYZER_DETECT_NPLUS_ONE=true"
echo "  ANALYZER_SUGGEST_INDEXES=true"
echo "  ANALYZER_PARSE_PLANS=true"
echo ""

echo -e "${GREEN}✓ For more examples, see: examples/ and docs/ directories${NC}"
