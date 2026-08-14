#!/usr/bin/env pwsh

param(
    [switch]$Dev,
    [switch]$Volumes
)

$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot\..

$composeFile = if ($Dev) { "docker-compose.dev.yml" } else { "docker-compose.yml" }
$stack = if ($Dev) { "desenvolvimento" } else { "produção" }
$volumesArg = if ($Volumes) { "-v" } else { "" }

Write-Host "====================================" -ForegroundColor Cyan
Write-Host " Docker Cleanup ($stack)" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Stopping and removing stack ($stack)..." -ForegroundColor Yellow
docker compose -f $composeFile down $volumesArg

Write-Host ""
if ($Volumes) {
    Write-Host "Volumes removed." -ForegroundColor Green
}
Write-Host "Cleanup completed!" -ForegroundColor Green
