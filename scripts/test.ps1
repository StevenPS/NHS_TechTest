<#
.SYNOPSIS
    Runs the .NET tests and lints the client. No database server is needed (tests use SQLite in-memory).
.EXAMPLE
    .\scripts\test.ps1
    .\scripts\test.ps1 -SkipClient
#>
param(
    [switch]$SkipClient
)

. "$PSScriptRoot/_common.ps1"

Write-Step 'Running .NET tests'
Invoke-Native 'dotnet' @('test', '--solution', 'ClinicSpace.slnx')

if (-not $SkipClient) {
    Install-ClientPackages

    Write-Step 'Linting client'
    Invoke-Native 'npm' @('run', 'lint') $ClientDir

    Write-Step 'Type-checking client'
    Invoke-Native 'npx' @('tsc', '-b') $ClientDir
}

Write-Host ''
Write-Host 'All checks passed.' -ForegroundColor Green
