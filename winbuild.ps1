# Requires: PowerShell 5+ or PowerShell Core
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

# Resolve script directory and switch to it
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $ScriptDir

$APP_VERSION = "0.1.0"
$BUILD_DATE  = (Get-Date).ToUniversalTime().ToString("yyyy-MM-dd")
$BUILD_CONFIGURATION = if ($env:BUILD_CONFIGURATION) { $env:BUILD_CONFIGURATION } else { "Release" }

Write-Host "=== Building AnimalHaus ==="
Write-Host ("    Version:    {0}" -f $APP_VERSION)
Write-Host ("    Build date: {0}" -f $BUILD_DATE)
Write-Host ("    Config:     {0}" -f $BUILD_CONFIGURATION)

$SystemProjects = @(
    "src/systems/AnimalHaus.Pigpen/AnimalHaus.Pigpen.csproj",
    "src/systems/AnimalHaus.Barn/AnimalHaus.Barn.csproj",
    "src/systems/AnimalHaus.Tractor/AnimalHaus.Tractor.csproj"
)

foreach ($project in $SystemProjects) {
    if (-not (Test-Path $project)) {
        Write-Error "Required system project is missing: $project"
        exit 1
    }
}

dotnet build "AnimalHaus.sln" -c $BUILD_CONFIGURATION

Write-Host "=== Build complete ==="