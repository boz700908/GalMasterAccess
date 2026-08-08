param(
    [string]$Configuration = "Debug",
    [string]$GamePath = "C:\Program Files (x86)\Steam\steamapps\common\GalMaster"
)

$ErrorActionPreference = "Stop"
& (Join-Path $PSScriptRoot "Build-Mod.ps1") -Configuration $Configuration
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed; deployment skipped."
    exit $LASTEXITCODE
}

$dll = Join-Path $PSScriptRoot "..\bin\$Configuration\net472\GalMasterAccess.dll"
$targetDir = Join-Path $GamePath "Mods"
if (-not (Test-Path $targetDir)) {
    Write-Error "Mods directory not found: $targetDir"
    exit 1
}

Copy-Item $dll $targetDir -Force
Write-Host "Deployed GalMasterAccess.dll to $targetDir"
