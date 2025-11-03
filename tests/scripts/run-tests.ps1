# Run all unit tests
param(
    [string]$Configuration = "Debug",
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"

Write-Host "Running unit tests..." -ForegroundColor Cyan
Write-Host "Configuration: $Configuration" -ForegroundColor Gray

$verbosity = if ($Verbose) { "detailed" } else { "normal" }

try {
    # Navigate to test project directory
    $testProjectPath = Join-Path $PSScriptRoot ".."
    Push-Location $testProjectPath

    # Run tests
    dotnet test `
        --configuration $Configuration `
        --verbosity $verbosity `
        --no-build

    if ($LASTEXITCODE -eq 0) {
        Write-Host "`n✓ All tests passed!" -ForegroundColor Green
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
