@echo off
setlocal enabledelayedexpansion

echo ====================================
echo  Migration SQL Script Generator
echo ====================================
echo.

cd /d "%~dp0\.."

echo [1/3] Checking dotnet ef tools...
dotnet ef --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: dotnet ef tools not found
    echo Installing dotnet-ef...
    dotnet tool install --global dotnet-ef
    if errorlevel 1 (
        echo ERROR: Failed to install dotnet-ef
        exit /b 1
    )
)
echo dotnet ef tools found
echo.

echo [2/3] Creating SQL output directory...
if not exist "src\Infrastructure\SQL" (
    mkdir "src\Infrastructure\SQL"
    echo Created src\Infrastructure\SQL directory
) else (
    echo Directory already exists
)
echo.

echo [3/3] Generating SQL scripts for all migrations...
echo.

REM Generate complete migration script (all migrations)
echo Generating complete migration script...
dotnet ef migrations script --idempotent --output "src\Infrastructure\SQL\00_complete_migration.sql" --project ecommerce-backend.csproj --context PostgresqlContext
if errorlevel 1 (
    echo ERROR: Failed to generate complete migration script
    exit /b 1
)
echo Created: src\Infrastructure\SQL\00_complete_migration.sql
echo.

REM Generate individual migration scripts
echo Generating individual migration scripts...

REM First, get the list and save migration names to an array
dotnet ef migrations list --project ecommerce-backend.csproj --context PostgresqlContext --no-connect > temp_migrations.txt 2>&1

REM Count migrations and store them
set migration_count=0
for /f "usebackq tokens=*" %%a in ("temp_migrations.txt") do (
    set line=%%a
    REM Look for lines with timestamps (14 digits followed by underscore)
    echo !line! | findstr /R "^[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]_" >nul
    if not errorlevel 1 (
        set /a migration_count+=1
        set "migration_!migration_count!=!line!"
    )
)

echo Found !migration_count! migration(s)
echo.

REM Now generate SQL for each migration
set counter=1
:loop_migrations
if !counter! GTR !migration_count! goto end_loop

set "current_migration=!migration_%counter%!"
echo [!counter!/!migration_count!] Processing: !current_migration!

REM Generate SQL script
if !counter! EQU 1 (
    REM First migration - from start
    dotnet ef migrations script 0 "!current_migration!" --output "src\Infrastructure\SQL\!counter!_!current_migration!.sql" --project ecommerce-backend.csproj --context PostgresqlContext
) else (
    REM Subsequent migrations - from previous to current
    set /a prev_index=!counter!-1
    set "previous_migration=!migration_%prev_index%!"
    dotnet ef migrations script "!previous_migration!" "!current_migration!" --output "src\Infrastructure\SQL\!counter!_!current_migration!.sql" --project ecommerce-backend.csproj --context PostgresqlContext
)

if errorlevel 1 (
    echo WARNING: Failed to generate script for !current_migration!
) else (
    echo Created: src\Infrastructure\SQL\!counter!_!current_migration!.sql
)
echo.

set /a counter+=1
goto loop_migrations

:end_loop

del temp_migrations.txt

echo ====================================
echo  SQL Scripts Generated Successfully
echo ====================================
echo.
echo Location: src\Infrastructure\SQL\
echo.

pause
