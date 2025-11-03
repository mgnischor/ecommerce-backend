# Generate HTML coverage report from coverage data
param(
    [switch]$Open
)

$ErrorActionPreference = "Stop"

Write-Host "Generating HTML coverage report..." -ForegroundColor Cyan

try {
    # Navigate to test project directory
    $testProjectPath = Join-Path $PSScriptRoot ".."
    Push-Location $testProjectPath

    # Check if coverage data exists
    if (-not (Test-Path "TestResults")) {
        Write-Host "No test results found. Run tests with coverage first:" -ForegroundColor Yellow
        Write-Host "  .\run-tests-with-coverage.ps1" -ForegroundColor Gray
        exit 1
    }

    # Find coverage files
    $coverageFiles = Get-ChildItem -Path "TestResults" -Filter "coverage.cobertura.xml" -Recurse

    if ($coverageFiles.Count -eq 0) {
        Write-Host "No coverage files found. Run tests with coverage first:" -ForegroundColor Yellow
        Write-Host "  .\run-tests-with-coverage.ps1" -ForegroundColor Gray
        exit 1
    }

    # Check if reportgenerator tool is installed
    $reportGeneratorInstalled = dotnet tool list -g | Select-String "dotnet-reportgenerator-globaltool"

    if (-not $reportGeneratorInstalled) {
        Write-Host "Installing reportgenerator tool..." -ForegroundColor Gray
        dotnet tool install -g dotnet-reportgenerator-globaltool
    }

    # Generate report
    $reportPath = Join-Path $testProjectPath "TestResults\CoverageReport"

    Write-Host "Generating report to: $reportPath" -ForegroundColor Gray

    reportgenerator `
        "-reports:TestResults\*\coverage.cobertura.xml" `
        "-targetdir:$reportPath" `
        "-reporttypes:Html;TextSummary"

    if ($LASTEXITCODE -eq 0) {
        Write-Host "`n✓ Coverage report generated successfully!" -ForegroundColor Green
        Write-Host "`nReport location:" -ForegroundColor Cyan
        Write-Host (Join-Path $reportPath "index.html") -ForegroundColor Gray

        # Display summary
        $summaryFile = Join-Path $reportPath "Summary.txt"
        if (Test-Path $summaryFile) {
            Write-Host "`nCoverage Summary:" -ForegroundColor Cyan
            Get-Content $summaryFile | Write-Host -ForegroundColor Gray
        }

        # Open report in browser if requested
        if ($Open) {
            Write-Host "`nOpening report in browser..." -ForegroundColor Gray
            Start-Process (Join-Path $reportPath "index.html")
        }
    } else {
        Write-Host "`n✗ Failed to generate report!" -ForegroundColor Red
        exit $LASTEXITCODE
    }
}
catch {
    Write-Host "`n✗ Error generating report: $_" -ForegroundColor Red
    exit 1
}
finally {
    Pop-Location
}
