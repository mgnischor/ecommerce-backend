@echo off
setlocal

set COMPOSE_FILE=docker-compose.yml
if /i "%1"=="dev" (
    set COMPOSE_FILE=docker-compose.dev.yml
    shift
)

set VOLUMES=
if /i "%1"=="volumes" (
    set VOLUMES=-v
)

cd /d "%~dp0\.."

echo ====================================
echo  Docker Cleanup
echo ====================================
echo.

echo Stopping and removing stack...
docker compose -f %COMPOSE_FILE% down %VOLUMES%

echo.
if defined VOLUMES (
    echo Volumes removed.
)
echo Cleanup completed!

endlocal
