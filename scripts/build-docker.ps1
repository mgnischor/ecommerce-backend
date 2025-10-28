#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot\..

Write-Host "====================================" -ForegroundColor Cyan
Write-Host " Docker Build and Run Script" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "[1/5] Cleaning up existing containers..." -ForegroundColor Yellow
docker rm -f ecommerce-backend-dev 2>$null
docker rm -f ecommerce-backend-prod 2>$null
docker rm -f ecommerce-postgres 2>$null
Write-Host ""

Write-Host "[2/5] Creating Docker network..." -ForegroundColor Yellow
$networkExists = docker network ls --filter name=ecommerce-network --format "{{.Name}}"
if ($networkExists -eq "ecommerce-network") {
    Write-Host "Network already exists" -ForegroundColor Green
} else {
    docker network create ecommerce-network
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Network created successfully" -ForegroundColor Green
    } else {
        Write-Host "ERROR: Failed to create network" -ForegroundColor Red
        exit 1
    }
}
Write-Host ""

Write-Host "[3/5] Starting PostgreSQL container..." -ForegroundColor Yellow
docker run -d `
  --name ecommerce-postgres `
  --network ecommerce-network `
  -e POSTGRES_USER=ecommerce `
  -e POSTGRES_PASSWORD=ecommerce `
  -e POSTGRES_DB=ecommerce `
  -p 5432:5432 `
  postgres:16-alpine

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to start PostgreSQL container" -ForegroundColor Red
    exit 1
}

Write-Host "Waiting for PostgreSQL to be ready..." -ForegroundColor Cyan
Start-Sleep -Seconds 5
Write-Host "PostgreSQL is ready" -ForegroundColor Green
Write-Host ""

Write-Host "[4/5] Building Docker images..." -ForegroundColor Yellow
Write-Host "Building development image..." -ForegroundColor Cyan
docker build --target development --build-arg BUILD_DEVELOPMENT=1 -t ecommerce-backend:dev .
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Development build failed" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Building production image..." -ForegroundColor Cyan
docker build --target production --build-arg BUILD_DEVELOPMENT=0 -t ecommerce-backend:prod .
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Production build failed" -ForegroundColor Red
    exit 1
}
Write-Host ""

Write-Host "[5/6] Starting application containers..." -ForegroundColor Yellow
Write-Host "Starting development container..." -ForegroundColor Cyan
docker run -d `
  --name ecommerce-backend-dev `
  --network ecommerce-network `
  -e ASPNETCORE_ENVIRONMENT=Development `
  -e "ConnectionStrings__DefaultConnection=Host=ecommerce-postgres;Database=ecommerce;Username=ecommerce;Password=ecommerce;Port=5432" `
  -e OpenTelemetry__ServiceName=ECommerce.Backend.Dev `
  -e OpenTelemetry__OtlpEndpoint=http://localhost:4317 `
  -p 5049:5049 `
  ecommerce-backend:dev

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to start development container" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Starting production container..." -ForegroundColor Cyan
docker run -d `
  --name ecommerce-backend-prod `
  --network ecommerce-network `
  -e ASPNETCORE_ENVIRONMENT=Production `
  -e "ConnectionStrings__DefaultConnection=Host=ecommerce-postgres;Database=ecommerce;Username=ecommerce;Password=ecommerce;Port=5432" `
  -e OpenTelemetry__ServiceName=ECommerce.Backend `
  -e OpenTelemetry__OtlpEndpoint=http://localhost:4317 `
  -p 8080:80 `
  ecommerce-backend:prod

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to start production container" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "[6/6] Applying database migrations..." -ForegroundColor Yellow
Write-Host "Waiting for containers to be fully ready..." -ForegroundColor Cyan
Start-Sleep -Seconds 3

Write-Host "Creating temporary container for migrations..." -ForegroundColor Cyan
docker run -d `
  --name ecommerce-migration-temp `
  --network ecommerce-network `
  --entrypoint tail `
  ecommerce-backend:dev `
  -f /dev/null

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to create temporary container" -ForegroundColor Red
    exit 1
}

Write-Host "Applying migrations to PostgreSQL via temporary container..." -ForegroundColor Cyan
docker exec ecommerce-migration-temp dotnet ef database update `
  --project /src/ecommerce-backend.csproj `
  --context PostgresqlContext `
  --connection "Host=ecommerce-postgres;Database=ecommerce;Username=ecommerce;Password=ecommerce;Port=5432"

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "WARNING: Failed to apply database migrations" -ForegroundColor Yellow
    Write-Host "Cleaning up temporary container..." -ForegroundColor Yellow
    docker rm -f ecommerce-migration-temp 2>$null
    Write-Host ""
    Write-Host "The containers are running but database update failed." -ForegroundColor Yellow
    Write-Host "You can try running migrations manually with:" -ForegroundColor White
    Write-Host "  docker run -d --name ecommerce-migration-temp --network ecommerce-network --entrypoint tail ecommerce-backend:dev -f /dev/null" -ForegroundColor Gray
    Write-Host "  docker exec ecommerce-migration-temp dotnet ef database update --project /src/ecommerce-backend.csproj --context PostgresqlContext --connection 'Host=ecommerce-postgres;Database=ecommerce;Username=ecommerce;Password=ecommerce;Port=5432'" -ForegroundColor Gray
    Write-Host "  docker rm -f ecommerce-migration-temp" -ForegroundColor Gray
    Write-Host ""
} else {
    Write-Host "Database migrations applied successfully!" -ForegroundColor Green
    Write-Host "Cleaning up temporary container..." -ForegroundColor Cyan
    docker rm -f ecommerce-migration-temp 2>$null
    Write-Host ""
    Write-Host "Restarting backend containers to apply seeders..." -ForegroundColor Cyan
    docker restart ecommerce-backend-dev ecommerce-backend-prod
    Write-Host ""
    Write-Host "Waiting for containers to restart..." -ForegroundColor Cyan
    Start-Sleep -Seconds 5
    Write-Host ""
}
Write-Host "====================================" -ForegroundColor Green
Write-Host " Containers started successfully!" -ForegroundColor Green
Write-Host "====================================" -ForegroundColor Green
Write-Host ""
Write-Host "Running containers:" -ForegroundColor Cyan
docker ps --filter "name=ecommerce"
Write-Host ""
Write-Host "Endpoints:" -ForegroundColor Cyan
Write-Host "  PostgreSQL:        " -NoNewline -ForegroundColor White
Write-Host "localhost:5432" -ForegroundColor Yellow
Write-Host "  Development API:   " -NoNewline -ForegroundColor White
Write-Host "http://localhost:5049" -ForegroundColor Yellow
Write-Host "  Production API:    " -NoNewline -ForegroundColor White
Write-Host "http://localhost:8080" -ForegroundColor Yellow
Write-Host ""
Write-Host "Useful commands:" -ForegroundColor Cyan
Write-Host "  View dev logs:     " -NoNewline -ForegroundColor White
Write-Host "docker logs -f ecommerce-backend-dev" -ForegroundColor Gray
Write-Host "  View prod logs:    " -NoNewline -ForegroundColor White
Write-Host "docker logs -f ecommerce-backend-prod" -ForegroundColor Gray
Write-Host "  View DB logs:      " -NoNewline -ForegroundColor White
Write-Host "docker logs -f ecommerce-postgres" -ForegroundColor Gray
Write-Host "  Stop all:          " -NoNewline -ForegroundColor White
Write-Host "docker stop ecommerce-backend-dev ecommerce-backend-prod ecommerce-postgres" -ForegroundColor Gray
Write-Host "  Remove all:        " -NoNewline -ForegroundColor White
Write-Host "docker rm -f ecommerce-backend-dev ecommerce-backend-prod ecommerce-postgres" -ForegroundColor Gray
Write-Host ""
