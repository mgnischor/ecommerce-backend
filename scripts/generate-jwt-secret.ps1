# Generates a cryptographically strong JWT signing secret.
# Usage: .\generate-jwt-secret.ps1
# Output: A Base64-encoded 384-bit secret. Configure it via the Jwt:SecretKey setting
#         or the Jwt__SecretKey environment variable (never commit it to source control).

$ErrorActionPreference = "Stop"

$projectDir = Split-Path -Parent $PSScriptRoot
Push-Location $projectDir
try {
    $secret = dotnet run --project ECommerce.Backend.csproj -- --generate-jwt-secret
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to generate JWT secret (exit code $LASTEXITCODE)."
    }

    Write-Host ""
    Write-Host "JWT signing secret generated successfully:"
    Write-Host ""
    Write-Host "  $secret" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Configure it in production via the environment variable:" -ForegroundColor Cyan
    Write-Host '  $env:Jwt__SecretKey = "<your-secret>"' -ForegroundColor Cyan
} finally {
    Pop-Location
}
