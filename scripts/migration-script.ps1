#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot\..

Write-Host "====================================" -ForegroundColor Cyan
Write-Host " Migration SQL Script Generator" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "[1/3] Checking dotnet ef tools..." -ForegroundColor Yellow
try {
    $efVersion = dotnet ef --version 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "dotnet ef tools found: $efVersion" -ForegroundColor Green
    } else {
        throw "dotnet ef not found"
    }
} catch {
    Write-Host "dotnet ef not found. Installing..." -ForegroundColor Cyan
    dotnet tool install --global dotnet-ef
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Failed to install dotnet-ef" -ForegroundColor Red
        exit 1
    }
    Write-Host "dotnet ef installed successfully" -ForegroundColor Green
}
Write-Host ""

Write-Host "[2/3] Creating SQL output directory..." -ForegroundColor Yellow
$sqlDir = "src\Infrastructure\SQL"
if (-not (Test-Path $sqlDir)) {
    New-Item -ItemType Directory -Path $sqlDir -Force | Out-Null
    Write-Host "Created $sqlDir directory" -ForegroundColor Green
} else {
    Write-Host "Directory already exists" -ForegroundColor Green
}
Write-Host ""

Write-Host "[3/3] Generating SQL scripts for migrations..." -ForegroundColor Yellow
Write-Host ""

# Generate complete migration script (all migrations, idempotent)
Write-Host "Generating complete idempotent migration script..." -ForegroundColor Cyan
$completeScript = "$sqlDir\00_complete_migration.sql"
dotnet ef migrations script --idempotent --output $completeScript --project ecommerce-backend.csproj --context PostgresqlContext

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Created: $completeScript" -ForegroundColor Green
} else {
    Write-Host "✗ ERROR: Failed to generate complete migration script" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Get list of migrations
Write-Host "Retrieving migration list..." -ForegroundColor Cyan
$migrationsOutput = dotnet ef migrations list --project ecommerce-backend.csproj --context PostgresqlContext --no-connect 2>&1
$migrations = @()

foreach ($line in $migrationsOutput) {
    # Match lines that contain migration names (timestamp_MigrationName format)
    if ($line -match '^\d{14}_\w+') {
        $migrationName = $line.Trim()
        $migrations += $migrationName
    } elseif ($line -match 'Migration_v\d+_\d+_\d+') {
        # Also match Migration_v0_0_X format
        $migrationName = $line.Trim()
        $migrations += $migrationName
    }
}

if ($migrations.Count -eq 0) {
    Write-Host "No migrations found" -ForegroundColor Yellow
    exit 0
}

Write-Host "Found $($migrations.Count) migration(s)" -ForegroundColor Green
Write-Host ""

# Generate individual migration scripts
Write-Host "Generating individual migration scripts..." -ForegroundColor Cyan
Write-Host ""

$counter = 1
$previousMigration = $null

foreach ($migration in $migrations) {
    Write-Host "[$counter/$($migrations.Count)] Processing: $migration" -ForegroundColor Yellow
    
    # Extract clean name for filename
    $cleanName = $migration -replace '^\d{14}_', ''
    $outputFile = "$sqlDir\$counter" + "_$cleanName.sql"
    
    # Generate SQL script from previous migration to current
    if ($previousMigration) {
        # Generate incremental script (only changes for this migration)
        dotnet ef migrations script $previousMigration $migration --output $outputFile --project ecommerce-backend.csproj --context PostgresqlContext
    } else {
        # First migration - generate from beginning
        dotnet ef migrations script 0 $migration --output $outputFile --project ecommerce-backend.csproj --context PostgresqlContext
    }
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✓ Created: $outputFile" -ForegroundColor Green
    } else {
        Write-Host "  ✗ WARNING: Failed to generate script for $migration" -ForegroundColor Yellow
    }
    
    $previousMigration = $migration
    $counter++
    Write-Host ""
}

Write-Host "====================================" -ForegroundColor Green
Write-Host " SQL Scripts Generated Successfully" -ForegroundColor Green
Write-Host "====================================" -ForegroundColor Green
Write-Host ""
Write-Host "Location: $sqlDir\" -ForegroundColor Cyan
Write-Host ""
Write-Host "Files generated:" -ForegroundColor White
Get-ChildItem -Path $sqlDir -Filter "*.sql" | ForEach-Object {
    $size = [math]::Round($_.Length / 1KB, 2)
    Write-Host "  - $($_.Name) ($size KB)" -ForegroundColor Gray
}
Write-Host ""
