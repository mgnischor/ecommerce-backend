@echo off
setlocal enabledelayedexpansion

cd ..

echo ====================================
echo  Ecommerce Backend Build Script
echo  (Uses LOCAL PostgreSQL)
echo ====================================
echo.

echo [1/5] Reading version...
if not exist version.txt (
    echo 0.1.0 > version.txt
)

set /p VERSION=<version.txt
echo Current version: %VERSION%
echo.

echo [2/5] Incrementing build version...
for /f "tokens=1,2,3 delims=." %%a in ("%VERSION%") do (
    set MAJOR=%%a
    set MINOR=%%b
    set /a BUILD=%%c+1
)
set NEW_VERSION=%MAJOR%.%MINOR%.%BUILD%
echo %NEW_VERSION% > version.txt
echo New version: %NEW_VERSION%
echo.

echo [3/5] Checking dotnet ef tools...
dotnet ef --version >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo dotnet ef not found. Installing...
    dotnet tool install --global dotnet-ef
    if %ERRORLEVEL% NEQ 0 (
        echo ERROR: Failed to install dotnet-ef
        echo Reverting version...
        echo %VERSION% > version.txt
        exit /b 1
    )
    echo dotnet ef installed successfully
) else (
    echo dotnet ef is already installed
)
echo.

echo [4/5] Creating migration...
set MIGRATION_NAME=Migration_v%MAJOR%_%MINOR%_%BUILD%
echo Migration name: %MIGRATION_NAME%

dotnet ef migrations add %MIGRATION_NAME% ^
    --project ECommerce.Backend.csproj ^
    --output-dir src\Infrastructure\Migrations ^
    --context PostgresqlContext

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERROR: Failed to create migration
    echo Reverting version...
    echo %VERSION% > version.txt
    exit /b 1
)
echo.

echo [5/8] Building project...
echo Building Release configuration...
dotnet build -c Release --no-incremental
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Release build failed
    exit /b 1
)

echo Building Debug configuration...
dotnet build -c Debug --no-incremental
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Debug build failed
    exit /b 1
)
echo.

echo [6/8] Publishing project...
echo Publishing Release configuration...
dotnet publish -c Release --no-build
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Release publish failed
    exit /b 1
)

echo Publishing Debug configuration...
dotnet publish -c Debug --no-build
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Debug publish failed
    exit /b 1
)
echo.

echo [7/8] Exporting migration SQL scripts...
echo Running migration script generator...
if exist "%~dp0migration-script.cmd" (
    call "%~dp0migration-script.cmd" --no-pause
    echo Migration SQL scripts exported successfully!
) else (
    echo WARNING: migration-script.cmd not found
)
echo.

echo [8/8] Applying database migrations...
echo Updating database with latest migrations...
dotnet ef database update ^
    --project ECommerce.Backend.csproj ^
    --context PostgresqlContext

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo WARNING: Failed to apply database migrations
    echo The build was successful but database update failed.
    echo Please check your database connection and run: dotnet ef database update
    echo.
) else (
    echo Database updated successfully!
    echo.
)

echo ====================================
echo  Build completed successfully!
echo  Version: %NEW_VERSION%
echo  Migration: %MIGRATION_NAME%
echo ====================================

endlocal
