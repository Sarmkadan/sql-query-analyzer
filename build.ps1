# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [switch]$Test,
    [switch]$Pack,
    [switch]$Publish
)

$ErrorActionPreference = "Stop"

Write-Host "SQL Query Analyzer - Build Script (PowerShell)" -ForegroundColor Cyan
Write-Host "=============================================="
Write-Host ""

# Check .NET SDK
$dotnetVersion = dotnet --version
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ .NET SDK not found" -ForegroundColor Red
    exit 1
}

Write-Host "✓ .NET SDK $dotnetVersion" -ForegroundColor Green

# Validate project file
if (-not (Test-Path "sql-query-analyzer.csproj")) {
    Write-Host "✗ Project file not found" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Project file found" -ForegroundColor Green
Write-Host ""

# Step 1: Restore
Write-Host "Step 1: Restoring dependencies..." -ForegroundColor Cyan
dotnet restore
if ($LASTEXITCODE -ne 0) { exit 1 }
Write-Host "✓ Restore complete" -ForegroundColor Green
Write-Host ""

# Step 2: Build
Write-Host "Step 2: Building project..." -ForegroundColor Cyan
dotnet build --configuration $Configuration --no-restore
if ($LASTEXITCODE -ne 0) { exit 1 }
Write-Host "✓ Build complete" -ForegroundColor Green
Write-Host ""

# Step 3: Test
if ($Test -or (Test-Path "./Tests")) {
    Write-Host "Step 3: Running tests..." -ForegroundColor Cyan
    dotnet test --configuration $Configuration --no-build --verbosity minimal
    if ($LASTEXITCODE -ne 0) { exit 1 }
    Write-Host "✓ Tests passed" -ForegroundColor Green
    Write-Host ""
}

# Step 4: Pack
if ($Pack) {
    Write-Host "Step 4: Creating NuGet package..." -ForegroundColor Cyan
    dotnet pack --configuration $Configuration --no-build --output ./publish
    if ($LASTEXITCODE -ne 0) { exit 1 }
    Write-Host "✓ Package created" -ForegroundColor Green
    Write-Host ""
}

# Step 5: Publish
if ($Publish) {
    Write-Host "Step 5: Publishing..." -ForegroundColor Cyan
    dotnet publish --configuration $Configuration --output ./publish
    if ($LASTEXITCODE -ne 0) { exit 1 }
    Write-Host "✓ Publish complete" -ForegroundColor Green
    Write-Host ""
}

# Summary
Write-Host "Build Summary:" -ForegroundColor Cyan
Write-Host "  Configuration: $Configuration"
Write-Host "  Output: ./bin/$Configuration"
Write-Host "  Framework: net10.0"
Write-Host ""

Write-Host "✓ Build successful!" -ForegroundColor Green
