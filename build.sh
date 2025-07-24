#!/bin/bash

# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
PROJECT_FILE="sql-query-analyzer.csproj"
CONFIG=${1:-Release}
OUTPUT_DIR="./bin/${CONFIG}"

echo -e "${BLUE}SQL Query Analyzer - Build Script${NC}"
echo "=================================="
echo ""

# Check .NET SDK
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}✗ .NET SDK not found${NC}"
    echo "Install from: https://dotnet.microsoft.com/download"
    exit 1
fi

DOTNET_VERSION=$(dotnet --version)
echo -e "${GREEN}✓${NC} .NET SDK ${DOTNET_VERSION}"

# Check project file exists
if [ ! -f "$PROJECT_FILE" ]; then
    echo -e "${RED}✗ Project file not found: $PROJECT_FILE${NC}"
    exit 1
fi

echo -e "${GREEN}✓${NC} Project file found"
echo ""

# Step 1: Restore
echo -e "${BLUE}Step 1: Restoring dependencies...${NC}"
dotnet restore
echo -e "${GREEN}✓${NC} Restore complete"
echo ""

# Step 2: Build
echo -e "${BLUE}Step 2: Building project...${NC}"
dotnet build --configuration "$CONFIG" --no-restore
echo -e "${GREEN}✓${NC} Build complete"
echo ""

# Step 3: Run tests (if in Debug mode, or if tests exist)
if [ -d "./Tests" ] || [ -d "./tests" ]; then
    echo -e "${BLUE}Step 3: Running tests...${NC}"
    dotnet test --configuration "$CONFIG" --no-build --verbosity minimal
    echo -e "${GREEN}✓${NC} Tests passed"
    echo ""
fi

# Step 4: Code formatting check (optional, non-blocking)
if command -v dotnet-format &> /dev/null; then
    echo -e "${BLUE}Step 4: Checking code formatting...${NC}"
    if dotnet format --verify-no-changes --verbosity quiet 2>/dev/null; then
        echo -e "${GREEN}✓${NC} Code formatting is correct"
    else
        echo -e "${YELLOW}⚠${NC} Code formatting issues found"
        echo "  Run: dotnet format"
    fi
    echo ""
fi

# Step 5: Summary
echo -e "${BLUE}Build Summary:${NC}"
echo "  Configuration: $CONFIG"
echo "  Output: $OUTPUT_DIR"
echo "  Framework: net10.0"
echo ""

# Step 6: Next steps
echo -e "${BLUE}Next steps:${NC}"
if [ "$CONFIG" = "Release" ]; then
    echo "  • Publish: dotnet publish --configuration Release"
    echo "  • Create package: dotnet pack --configuration Release"
else
    echo "  • Run: dotnet run --configuration Debug"
    echo "  • Test: dotnet test --configuration Debug"
fi
echo "  • More: make help"
echo ""

echo -e "${GREEN}✓ Build successful!${NC}"
