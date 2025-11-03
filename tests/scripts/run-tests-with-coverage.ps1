# Run tests with code coverage
param(
    [string]$Configuration = "Debug",
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"

Write-Host "Running tests with code coverage..." -ForegroundColor Cyan
Write-Host "Configuration: $Configuration" -ForegroundColor Gray

$verbosity = if ($Verbose) { "detailed" } else { "normal" }

try {
    # Navigate to test project directory
    $testProjectPath = Join-Path $PSScriptRoot ".."
    Push-Location $testProjectPath

    # Clean previous results
    if (Test-Path "TestResults") {
        Write-Host "Cleaning previous test results..." -ForegroundColor Gray
        Remove-Item -Recurse -Force "TestResults"
    }

    # Run tests with coverage
    Write-Host "Running tests..." -ForegroundColor Gray
    dotnet test `
        --configuration $Configuration `
        --verbosity $verbosity `
        --collect:"XPlat Code Coverage" `
        --settings test.runsettings

    if ($LASTEXITCODE -eq 0) {
        Write-Host "`n✓ Tests completed successfully!" -ForegroundColor Green

        # Find coverage file
        $coverageFile = Get-ChildItem -Path "TestResults" -Filter "coverage.cobertura.xml" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1

        if ($coverageFile) {
            Write-Host "`nCoverage report generated at:" -ForegroundColor Cyan
            Write-Host $coverageFile.FullName -ForegroundColor Gray
            Write-Host "`nTo generate HTML report, run: .\generate-coverage-report.ps1" -ForegroundColor Yellow
        }
    } else {
        Write-Host "`n✗ Some tests failed!" -ForegroundColor Red
        exit $LASTEXITCODE
    }
}
catch {
    Write-Host "`n✗ Error running tests: $_" -ForegroundColor Red
    exit 1
}
finally {
    Pop-Location
}
