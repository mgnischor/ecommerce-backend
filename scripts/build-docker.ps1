#!/usr/bin/env pwsh

param(
    [switch]$Dev,
    [switch]$NoBuild
)

$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot\..

$composeFile = if ($Dev) { "docker-compose.dev.yml" } else { "docker-compose.yml" }
$stack = if ($Dev) { "desenvolvimento" } else { "produção" }

Write-Host "====================================" -ForegroundColor Cyan
Write-Host " Comex Docker Build ($stack)" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host ""

if (-not $NoBuild) {
    Write-Host "Building images..." -ForegroundColor Yellow
    docker compose -f $composeFile build
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Docker build failed" -ForegroundColor Red
        exit 1
    }
    Write-Host ""
}

Write-Host "Starting stack ($stack)..." -ForegroundColor Yellow
docker compose -f $composeFile up -d
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to start stack" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "====================================" -ForegroundColor Green
Write-Host " Stack ($stack) is running!" -ForegroundColor Green
Write-Host "====================================" -ForegroundColor Green
Write-Host ""
Write-Host "Endpoints:" -ForegroundColor Cyan
Write-Host "  Frontend:         " -NoNewline -ForegroundColor White
Write-Host "http://localhost:8080" -ForegroundColor Yellow
Write-Host "  Backend API:      " -NoNewline -ForegroundColor White
Write-Host "http://localhost:5049" -ForegroundColor Yellow
Write-Host "  PostgreSQL:       " -NoNewline -ForegroundColor White
Write-Host "localhost:5432" -ForegroundColor Yellow
Write-Host "  Jaeger UI:        " -NoNewline -ForegroundColor White
Write-Host "http://localhost:16686" -ForegroundColor Yellow
Write-Host ""
Write-Host "Useful commands:" -ForegroundColor Cyan
Write-Host "  View logs:      " -NoNewline -ForegroundColor White
Write-Host "docker compose -f $composeFile logs -f" -ForegroundColor Gray
Write-Host "  Stop stack:     " -NoNewline -ForegroundColor White
Write-Host "docker compose -f $composeFile down" -ForegroundColor Gray
Write-Host "  Stop + volumes: " -NoNewline -ForegroundColor White
Write-Host "docker compose -f $composeFile down -v" -ForegroundColor Gray
Write-Host ""
Write-Host "Cleanup script: .\scripts\cleanup-docker.ps1" -ForegroundColor Cyan
