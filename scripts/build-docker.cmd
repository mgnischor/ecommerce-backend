@echo off
setlocal

set COMPOSE_FILE=docker-compose.yml
set STACK=producao
if /i "%1"=="dev" (
    set COMPOSE_FILE=docker-compose.dev.yml
    set STACK=desenvolvimento
    shift
)

set NO_BUILD=
if /i "%1"=="nobuild" (
    set NO_BUILD=1
)

cd /d "%~dp0\.."

echo ====================================
echo  Comex Docker Build (%STACK%)
echo ====================================
echo.

if "%NO_BUILD%"=="" (
    echo Building images...
    docker compose -f %COMPOSE_FILE% build
    if errorlevel 1 (
        echo ERROR: Docker build failed
        exit /b 1
    )
    echo.
)

echo Starting stack (%STACK%)...
docker compose -f %COMPOSE_FILE% up -d
if errorlevel 1 (
    echo ERROR: Failed to start stack
    exit /b 1
)

echo.
echo ====================================
echo  Stack (%STACK%) is running!
echo ====================================
echo.
echo Endpoints:
echo   Frontend:         http://localhost:8080
echo   Backend API:      http://localhost:5049
echo   PostgreSQL:       localhost:5432
echo   Jaeger UI:        http://localhost:16686
echo.
echo Useful commands:
echo   View logs:      docker compose -f %COMPOSE_FILE% logs -f
echo   Stop stack:     docker compose -f %COMPOSE_FILE% down
echo   Stop + volumes: docker compose -f %COMPOSE_FILE% down -v
echo.
echo Cleanup script: .\scripts\cleanup-docker.cmd

endlocal
