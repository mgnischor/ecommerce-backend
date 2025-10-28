@echo off

cd ..

echo ====================================
echo  Docker Build and Run Script
echo ====================================
echo.

echo [1/5] Cleaning up existing containers...
docker rm -f ecommerce-backend-dev 2>nul
docker rm -f ecommerce-backend-prod 2>nul
docker rm -f ecommerce-postgres 2>nul
echo.

echo [2/5] Creating Docker network...
docker network create ecommerce-network 2>nul
if %ERRORLEVEL% EQU 0 (
    echo Network created successfully
) else (
    echo Network already exists
)
echo.

echo [3/5] Starting PostgreSQL container...
docker run -d ^
  --name ecommerce-postgres ^
  --network ecommerce-network ^
  -e POSTGRES_USER=ecommerce ^
  -e POSTGRES_PASSWORD=ecommerce ^
  -e POSTGRES_DB=ecommerce ^
  -p 5432:5432 ^
  postgres:16-alpine

echo Waiting for PostgreSQL to be ready...
timeout /t 5 /nobreak >nul
echo PostgreSQL is ready
echo.

echo [4/5] Building Docker images...
echo Building development image...
docker build --target development --build-arg BUILD_DEVELOPMENT=1 -t ecommerce-backend:dev .
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Development build failed
    exit /b 1
)

echo.
echo Building production image...
docker build --target production --build-arg BUILD_DEVELOPMENT=0 -t ecommerce-backend:prod .
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Production build failed
    exit /b 1
)
echo.

echo [5/6] Starting application containers...
echo Starting development container...
docker run -d ^
  --name ecommerce-backend-dev ^
  --network ecommerce-network ^
  -e ASPNETCORE_ENVIRONMENT=Development ^
  -e ConnectionStrings__DefaultConnection="Host=ecommerce-postgres;Database=ecommerce;Username=ecommerce;Password=ecommerce;Port=5432" ^
  -e OpenTelemetry__ServiceName=ECommerce.Backend.Dev ^
  -e OpenTelemetry__OtlpEndpoint=http://localhost:4317 ^
  -p 5049:5049 ^
  ecommerce-backend:dev

echo.
echo Starting production container...
docker run -d ^
  --name ecommerce-backend-prod ^
  --network ecommerce-network ^
  -e ASPNETCORE_ENVIRONMENT=Production ^
  -e ConnectionStrings__DefaultConnection="Host=ecommerce-postgres;Database=ecommerce;Username=ecommerce;Password=ecommerce;Port=5432" ^
  -e OpenTelemetry__ServiceName=ECommerce.Backend ^
  -e OpenTelemetry__OtlpEndpoint=http://localhost:4317 ^
  -p 8080:80 ^
  ecommerce-backend:prod

echo.
echo [6/6] Applying database migrations...
echo Waiting for containers to be fully ready...
timeout /t 3 /nobreak >nul

echo Creating temporary container for migrations...
docker run -d ^
  --name ecommerce-migration-temp ^
  --network ecommerce-network ^
  --entrypoint tail ^
  ecommerce-backend:dev ^
  -f /dev/null

echo Applying migrations to PostgreSQL via temporary container...
docker exec ecommerce-migration-temp dotnet ef database update ^
  --project /src/ecommerce-backend.csproj ^
  --context PostgresqlContext ^
  --connection "Host=ecommerce-postgres;Database=ecommerce;Username=ecommerce;Password=ecommerce;Port=5432"

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo WARNING: Failed to apply database migrations
    echo Cleaning up temporary container...
    docker rm -f ecommerce-migration-temp 2>nul
    echo.
    echo The containers are running but database update failed.
    echo You can try running migrations manually with:
    echo   docker run -d --name ecommerce-migration-temp --network ecommerce-network --entrypoint tail ecommerce-backend:dev -f /dev/null
    echo   docker exec ecommerce-migration-temp dotnet ef database update --project /src/ecommerce-backend.csproj --context PostgresqlContext --connection "Host=ecommerce-postgres;Database=ecommerce;Username=ecommerce;Password=ecommerce;Port=5432"
    echo   docker rm -f ecommerce-migration-temp
    echo.
) else (
    echo Database migrations applied successfully!
    echo Cleaning up temporary container...
    docker rm -f ecommerce-migration-temp 2>nul
    echo.
    echo Restarting backend containers to apply seeders...
    docker restart ecommerce-backend-dev ecommerce-backend-prod
    echo.
    echo Waiting for containers to restart...
    timeout /t 5 /nobreak >nul
    echo.
)
echo ====================================
echo  Containers started successfully!
echo ====================================
echo.
echo Running containers:
docker ps --filter "name=ecommerce"
echo.
echo Endpoints:
echo   PostgreSQL:        localhost:5432
echo   Development API:   http://localhost:5049
echo   Production API:    http://localhost:8080
echo.
echo Useful commands:
echo   View dev logs:     docker logs -f ecommerce-backend-dev
echo   View prod logs:    docker logs -f ecommerce-backend-prod
echo   View DB logs:      docker logs -f ecommerce-postgres
echo   Stop all:          docker stop ecommerce-backend-dev ecommerce-backend-prod ecommerce-postgres
echo   Remove all:        docker rm -f ecommerce-backend-dev ecommerce-backend-prod ecommerce-postgres
echo.
