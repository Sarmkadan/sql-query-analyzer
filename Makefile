# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

.PHONY: help build clean restore test run docker-build docker-up docker-down lint format publish

DOTNET := dotnet
DOCKER := docker
DOCKER_COMPOSE := docker-compose
PROJECT_FILE := sql-query-analyzer.csproj
OUTPUT_DIR := ./bin/Release
PUBLISH_DIR := ./publish

help:
	@echo "SQL Query Analyzer - Available Commands"
	@echo "======================================"
	@echo ""
	@echo "Development:"
	@echo "  make build              - Build the project"
	@echo "  make clean              - Clean build artifacts"
	@echo "  make restore            - Restore NuGet packages"
	@echo "  make test               - Run unit tests"
	@echo "  make run                - Run the application"
	@echo ""
	@echo "Code Quality:"
	@echo "  make lint               - Run code analysis"
	@echo "  make format             - Format code"
	@echo "  make format-check       - Check code formatting"
	@echo ""
	@echo "Docker:"
	@echo "  make docker-build       - Build Docker image"
	@echo "  make docker-up          - Start Docker services"
	@echo "  make docker-down        - Stop Docker services"
	@echo "  make docker-logs        - View Docker logs"
	@echo ""
	@echo "Deployment:"
	@echo "  make publish            - Publish release build"
	@echo "  make package            - Create NuGet package"
	@echo ""
	@echo "Maintenance:"
	@echo "  make all                - Build, test, and lint"
	@echo "  make clean-all          - Full cleanup"
	@echo ""

restore:
	@echo "Restoring NuGet packages..."
	$(DOTNET) restore

build: restore
	@echo "Building project..."
	$(DOTNET) build --configuration Release

debug: restore
	@echo "Building project (Debug)..."
	$(DOTNET) build --configuration Debug

clean:
	@echo "Cleaning build artifacts..."
	$(DOTNET) clean
	rm -rf $(OUTPUT_DIR) $(PUBLISH_DIR) bin obj

test: build
	@echo "Running tests..."
	$(DOTNET) test --configuration Release --no-build --verbosity normal

test-verbose: build
	@echo "Running tests with verbose output..."
	$(DOTNET) test --configuration Release --no-build --verbosity detailed

run: build
	@echo "Running application..."
	$(DOTNET) run --configuration Release

run-debug: debug
	@echo "Running application (Debug)..."
	$(DOTNET) run --configuration Debug

lint:
	@echo "Running code analysis..."
	$(DOTNET) build /p:EnforceCodeStyleInBuild=true
	@echo "✓ Code analysis complete"

format:
	@echo "Formatting code..."
	$(DOTNET) format

format-check:
	@echo "Checking code formatting..."
	$(DOTNET) format --verify-no-changes --verbosity diagnostic

publish: clean
	@echo "Publishing Release build..."
	$(DOTNET) publish --configuration Release --output $(PUBLISH_DIR)
	@echo "✓ Published to $(PUBLISH_DIR)"

package: build
	@echo "Creating NuGet package..."
	$(DOTNET) pack --configuration Release --output $(PUBLISH_DIR)
	@echo "✓ Package created"

docker-build:
	@echo "Building Docker image..."
	$(DOCKER) build -t sql-query-analyzer:latest .
	@echo "✓ Image built successfully"

docker-up:
	@echo "Starting Docker services..."
	$(DOCKER_COMPOSE) up -d
	@echo "✓ Services started"
	@echo "  API: http://localhost:5000"

docker-down:
	@echo "Stopping Docker services..."
	$(DOCKER_COMPOSE) down
	@echo "✓ Services stopped"

docker-logs:
	@echo "Showing Docker logs..."
	$(DOCKER_COMPOSE) logs -f

docker-clean:
	@echo "Cleaning Docker resources..."
	$(DOCKER_COMPOSE) down -v
	$(DOCKER) rmi sql-query-analyzer:latest
	@echo "✓ Cleaned"

all: clean restore build lint test
	@echo "✓ Build, test, and lint complete"

clean-all: clean docker-clean
	@echo "✓ Full cleanup complete"

.DEFAULT_GOAL := help
