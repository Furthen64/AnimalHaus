# Requires: PowerShell 5+ or PowerShell Core
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

# Resolve script directory and switch to it
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $ScriptDir

$Project = "src/tools/AdministrationApp/AdministrationApp.csproj"
$ProjectPath = Join-Path $ScriptDir $Project

if (-not (Test-Path $ProjectPath)) {
    throw "AdministrationApp project is missing: $Project"
}

Write-Host "=== Launching AdministrationApp ==="
dotnet run --project $Project
