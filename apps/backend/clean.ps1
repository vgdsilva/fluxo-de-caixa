<#
.SYNOPSIS
    Remove as pastas bin/ e obj/ de todos os projetos.
#>
$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

Get-ChildItem -Path . -Recurse -Directory -Include bin, obj |
    Where-Object { $_.FullName -notmatch '\\node_modules\\' } |
    ForEach-Object {
        Write-Host "Removendo $($_.FullName)"
        Remove-Item $_.FullName -Recurse -Force
    }

Write-Host "Limpeza concluída." -ForegroundColor Green
