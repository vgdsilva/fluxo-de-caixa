<#
.SYNOPSIS
    Restaura, compila e (opcionalmente) testa a solução.
.EXAMPLE
    ./build.ps1                 # Debug, com testes
    ./build.ps1 -Configuration Release -SkipTests
#>
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',
    [switch]$SkipTests
)

$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

dotnet restore WebAPI.sln
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

dotnet build WebAPI.sln -c $Configuration --no-restore
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

if (-not $SkipTests) {
    dotnet test WebAPI.sln -c $Configuration --no-build
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

Write-Host "Build concluído ($Configuration)." -ForegroundColor Green
