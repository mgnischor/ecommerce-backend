#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot\..

Write-Host "====================================" -ForegroundColor Cyan
Write-Host " Ecommerce Backend Build Script" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "[1/5] Reading version..." -ForegroundColor Yellow
$versionFile = "version.txt"

if (-not (Test-Path $versionFile)) {
    "0.1.0" | Out-File -FilePath $versionFile -Encoding utf8 -NoNewline
}

$version = Get-Content $versionFile -Raw
$version = $version.Trim()
Write-Host "Current version: $version" -ForegroundColor Green
Write-Host ""

Write-Host "[2/5] Incrementing build version..." -ForegroundColor Yellow
$versionParts = $version.Split('.')
$major = [int]$versionParts[0]
$minor = [int]$versionParts[1]
$build = [int]$versionParts[2] + 1

$newVersion = "$major.$minor.$build"
$newVersion | Out-File -FilePath $versionFile -Encoding utf8 -NoNewline
Write-Host "New version: $newVersion" -ForegroundColor Green

Write-Host "Updating .csproj file..." -ForegroundColor Cyan
& "$PSScriptRoot\update-version.ps1" -VersionFile $versionFile -CsprojFile "ecommerce-backend.csproj"
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to update .csproj file" -ForegroundColor Red
    $version | Out-File -FilePath $versionFile -Encoding utf8 -NoNewline
    exit 1
}
Write-Host ""

Write-Host "[3/6] Checking dotnet ef tools..." -ForegroundColor Yellow
try {
    $efVersion = dotnet ef --version 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "dotnet ef is already installed: $efVersion" -ForegroundColor Green
    } else {
        throw "dotnet ef not found"
    }
} catch {
    Write-Host "dotnet ef not found. Installing..." -ForegroundColor Cyan
    dotnet tool install --global dotnet-ef
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Failed to install dotnet-ef" -ForegroundColor Red
        Write-Host "Reverting version..." -ForegroundColor Yellow
        $version | Out-File -FilePath $versionFile -Encoding utf8 -NoNewline
        exit 1
    }
    Write-Host "dotnet ef installed successfully" -ForegroundColor Green
}
Write-Host ""

Write-Host "[4/6] Creating migration..." -ForegroundColor Yellow
$migrationName = "Migration_v$($major)_$($minor)_$($build)"
Write-Host "Migration name: $migrationName" -ForegroundColor Green

try {
    dotnet ef migrations add $migrationName `
        --project ecommerce-backend.csproj `
        --output-dir src\Infrastructure\Migrations `
        --context PostgresqlContext
    
    if ($LASTEXITCODE -ne 0) {
        throw "Migration creation failed"
    }
} catch {
    Write-Host ""
    Write-Host "ERROR: Failed to create migration" -ForegroundColor Red
    Write-Host "Reverting version..." -ForegroundColor Yellow
    $version | Out-File -FilePath $versionFile -Encoding utf8 -NoNewline
    exit 1
}
Write-Host ""

Write-Host "[5/8] Building project..." -ForegroundColor Yellow
Write-Host "Building Release configuration..." -ForegroundColor Cyan
dotnet build -c Release --no-incremental
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Release build failed" -ForegroundColor Red
    exit 1
}

Write-Host "Building Debug configuration..." -ForegroundColor Cyan
dotnet build -c Debug --no-incremental
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Debug build failed" -ForegroundColor Red
    exit 1
}
Write-Host ""

Write-Host "[6/8] Publishing project..." -ForegroundColor Yellow
Write-Host "Publishing Release configuration..." -ForegroundColor Cyan
dotnet publish -c Release --no-build
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Release publish failed" -ForegroundColor Red
    exit 1
}

Write-Host "Publishing Debug configuration..." -ForegroundColor Cyan
dotnet publish -c Debug --no-build
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Debug publish failed" -ForegroundColor Red
    exit 1
}
Write-Host ""

Write-Host "[7/8] Exporting migration SQL scripts..." -ForegroundColor Yellow
Write-Host "Running migration script generator..." -ForegroundColor Cyan

$migrationScriptPath = Join-Path $PSScriptRoot "migration-script.ps1"
if (Test-Path $migrationScriptPath) {
    try {
        & $migrationScriptPath
        Write-Host "Migration SQL scripts exported successfully!" -ForegroundColor Green
    } catch {
        Write-Host "WARNING: Failed to export migration SQL scripts" -ForegroundColor Yellow
        Write-Host "Error: $_" -ForegroundColor Yellow
    }
} else {
    Write-Host "WARNING: migration-script.ps1 not found at $migrationScriptPath" -ForegroundColor Yellow
}
Write-Host ""

Write-Host "[8/8] Applying database migrations..." -ForegroundColor Yellow
Write-Host "Updating database with latest migrations..." -ForegroundColor Cyan
dotnet ef database update `
    --project ecommerce-backend.csproj `
    --context PostgresqlContext

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "WARNING: Failed to apply database migrations" -ForegroundColor Yellow
    Write-Host "The build was successful but database update failed." -ForegroundColor Yellow
    Write-Host "Please check your database connection and run: dotnet ef database update" -ForegroundColor White
    Write-Host ""
} else {
    Write-Host "Database updated successfully!" -ForegroundColor Green
    Write-Host ""
}

Write-Host "====================================" -ForegroundColor Green
Write-Host " Build completed successfully!" -ForegroundColor Green
Write-Host " Version: $newVersion" -ForegroundColor Green
Write-Host " Migration: $migrationName" -ForegroundColor Green
Write-Host "====================================" -ForegroundColor Green
