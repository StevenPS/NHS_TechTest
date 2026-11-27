# Shared helpers, dot-sourced by the other scripts.
$ErrorActionPreference = 'Stop'

$RepoRoot = Split-Path -Parent $PSScriptRoot
$ClientDir = Join-Path $RepoRoot 'client'
$ApiProject = Join-Path $RepoRoot 'src/ClinicSpace.Api'
$InfrastructureProject = Join-Path $RepoRoot 'src/ClinicSpace.Infrastructure'

function Write-Step([string]$Message) {
    Write-Host ''
    Write-Host "==> $Message" -ForegroundColor Cyan
}

# Runs a native command and stops the script if it fails
# (PowerShell does not stop on a non-zero exit code by itself).
function Invoke-Native([string]$Command, [string[]]$Arguments = @(), [string]$WorkingDirectory = $RepoRoot) {
    Push-Location $WorkingDirectory
    try {
        & $Command @Arguments
        if ($LASTEXITCODE -ne 0) {
            throw "'$Command $($Arguments -join ' ')' failed with exit code $LASTEXITCODE."
        }
    }
    finally {
        Pop-Location
    }
}

# Installs client packages if node_modules is missing (or always, with -Force).
function Install-ClientPackages([switch]$Force) {
    if ($Force -or -not (Test-Path (Join-Path $ClientDir 'node_modules'))) {
        Write-Step 'Installing client packages (npm ci)'
        Invoke-Native 'npm' @('ci') $ClientDir
    }
}
