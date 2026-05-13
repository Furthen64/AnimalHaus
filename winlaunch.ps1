# Requires: PowerShell 5+ or PowerShell Core
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

# Resolve script directory and switch to it
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $ScriptDir

Write-Host "=== Launching AnimalHaus systems ==="

$Systems = @(
    @{ Name = "Pigpen"; Project = "src/systems/AnimalHaus.Pigpen/AnimalHaus.Pigpen.csproj" },
    @{ Name = "Barn"; Project = "src/systems/AnimalHaus.Barn/AnimalHaus.Barn.csproj" },
    @{ Name = "Tractor"; Project = "src/systems/AnimalHaus.Tractor/AnimalHaus.Tractor.csproj" },
    @{ Name = "MarketPlace"; Project = "src/systems/AnimalHaus.MarketPlace/AnimalHaus.MarketPlace.csproj" }
)

$Processes = @()

try {
    foreach ($System in $Systems) {
        $ProjectPath = Join-Path $ScriptDir $System.Project

        if (-not (Test-Path $ProjectPath)) {
            throw "Required system project is missing: $($System.Project)"
        }

        Write-Host "Starting $($System.Name) ($($System.Project))"
        $Process = Start-Process -FilePath "dotnet" -ArgumentList @("run", "--project", $System.Project) -WorkingDirectory $ScriptDir -NoNewWindow -PassThru
        $Processes += $Process
    }

    Write-Host "All systems started. Press Ctrl+C to stop them."
    while ($true) {
        $RunningProcesses = @($Processes | Where-Object { -not $_.HasExited })
        if ($RunningProcesses.Count -eq 0) {
            break
        }

        Start-Sleep -Seconds 1
    }
}
finally {
    Write-Host ""
    Write-Host "=== Stopping AnimalHaus systems ==="

    foreach ($Process in $Processes) {
        if ($null -ne $Process) {
            try {
                if (-not $Process.HasExited) {
                    Stop-Process -Id $Process.Id -ErrorAction SilentlyContinue
                }
            }
            catch {
                # Process may already have exited.
            }
        }
    }
}
