<#
.SYNOPSIS
    Builds the .NET solution and the React client.
.EXAMPLE
    .\scripts\build.ps1
    .\scripts\build.ps1 -Configuration Release
#>
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug'
)

. "$PSScriptRoot/_common.ps1"

Write-Step 'Restoring .NET tools'
Invoke-Native 'dotnet' @('tool', 'restore')

Write-Step "Building .NET solution ($Configuration)"
Invoke-Native 'dotnet' @('build', 'ClinicSpace.slnx', '--configuration', $Configuration)

Install-ClientPackages

Write-Step 'Building client'
Invoke-Native 'npm' @('run', 'build') $ClientDir

Write-Host ''
Write-Host 'Build succeeded.' -ForegroundColor Green
