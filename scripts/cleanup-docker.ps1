#!/usr/bin/env pwsh

Write-Host "====================================" -ForegroundColor Cyan
Write-Host " Docker Cleanup Script" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Stopping containers..." -ForegroundColor Yellow
docker stop ecommerce-backend-dev ecommerce-backend-prod ecommerce-postgres 2>$null

Write-Host "Removing containers..." -ForegroundColor Yellow
docker rm -f ecommerce-backend-dev ecommerce-backend-prod ecommerce-postgres 2>$null

Write-Host "Removing network..." -ForegroundColor Yellow
docker network rm ecommerce-network 2>$null

Write-Host ""
Write-Host "Cleanup completed!" -ForegroundColor Green
Write-Host ""
