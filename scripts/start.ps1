<#
.SYNOPSIS
    Starts the API and the client dev server together. Press Ctrl+C to stop both.
.DESCRIPTION
    1. Applies EF Core migrations to the database (skip with -SkipMigrations).
    2. Starts the API on https://localhost:7196 in the background.
    3. Starts the Vite dev server on http://localhost:5173 in the foreground (it proxies /api to the API).
.EXAMPLE
    .\scripts\start.ps1
    .\scripts\start.ps1 -SkipMigrations
#>
param(
    [switch]$SkipMigrations
)

. "$PSScriptRoot/_common.ps1"

Write-Step 'Restoring .NET tools'
Invoke-Native 'dotnet' @('tool', 'restore')

if (-not $SkipMigrations) {
    Write-Step 'Applying database migrations'
    Invoke-Native 'dotnet' @('ef', 'database', 'update', '--project', $InfrastructureProject, '--startup-project', $ApiProject)
}

Install-ClientPackages

Write-Step 'Starting API (https://localhost:7196)'
$api = Start-Process -FilePath 'dotnet' `
    -ArgumentList @('run', '--project', "`"$ApiProject`"", '--launch-profile', 'https') `
    -WorkingDirectory $RepoRoot -NoNewWindow -PassThru

try {
    Write-Step 'Starting client (http://localhost:5173), press Ctrl+C to stop'
    Push-Location $ClientDir
    try {
        & npm run dev
    }
    finally {
        Pop-Location
    }
}
finally {
    Write-Step 'Stopping API'
    if (-not $api.HasExited) {
        # 'dotnet run' launches the app as a child process, so stop the whole process tree.
        if ($IsWindows -or $env:OS -eq 'Windows_NT') {
            & taskkill /PID $api.Id /T /F | Out-Null
        }
        else {
            Stop-Process -Id $api.Id -Force
        }
    }
}
