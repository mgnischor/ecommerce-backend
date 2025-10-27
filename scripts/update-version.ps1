#!/usr/bin/env pwsh

param(
    [string]$VersionFile = "version.txt",
    [string]$CsprojFile = "ecommerce-backend.csproj"
)

$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot\..

if (-not (Test-Path $VersionFile)) {
    Write-Host "ERROR: Version file not found: $VersionFile" -ForegroundColor Red
    exit 1
}

$version = (Get-Content $VersionFile -Raw).Trim()

if (-not (Test-Path $CsprojFile)) {
    Write-Host "ERROR: Project file not found: $CsprojFile" -ForegroundColor Red
    exit 1
}

Write-Host "Updating version in $CsprojFile to $version..." -ForegroundColor Cyan

[xml]$csproj = Get-Content $CsprojFile
$propertyGroup = $csproj.Project.PropertyGroup | Where-Object { $_.Version -ne $null } | Select-Object -First 1

if ($null -eq $propertyGroup) {
    $propertyGroup = $csproj.Project.PropertyGroup | Select-Object -First 1
}

if ($null -ne $propertyGroup) {
    $propertyGroup.Version = $version
    $propertyGroup.AssemblyVersion = "$version.0"
    $propertyGroup.FileVersion = "$version.0"
    
    if ($version.Contains("-")) {
        $propertyGroup.InformationalVersion = $version
    } else {
        $propertyGroup.InformationalVersion = "$version-dev"
    }
    
    $csproj.Save((Resolve-Path $CsprojFile).Path)
    Write-Host "Version updated successfully!" -ForegroundColor Green
} else {
    Write-Host "ERROR: Could not find PropertyGroup in project file" -ForegroundColor Red
    exit 1
}
