param(
    [string]$GamePath = "C:\Program Files (x86)\Steam\steamapps\common\GalMaster",
    [string]$Version = "1.0.0",
    [string]$OutputDirectory = "release"
)

$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
$stage = Join-Path ([IO.Path]::GetTempPath()) ("GalMasterAccess-" + [Guid]::NewGuid().ToString("N"))
$output = Join-Path $root $OutputDirectory

if (-not (Test-Path (Join-Path $GamePath "GAL PRO MASTER.exe"))) {
    throw "Game executable not found: $GamePath"
}

New-Item -ItemType Directory -Path $stage -Force | Out-Null
New-Item -ItemType Directory -Path $output -Force | Out-Null

# These are the install/runtime files outside the game itself. UserData is
# included because the game's hidden-console accessibility setting is stored there.
$directories = @("MelonLoader", "Mods", "Plugins", "UserLibs", "UserData")
foreach ($directory in $directories) {
    $source = Join-Path $GamePath $directory
    if (Test-Path $source) {
        Copy-Item $source (Join-Path $stage $directory) -Recurse -Force
    }
}

$rootFiles = @(
    "byctrl-x64.dll", "byctrl.conf", "GameConsoleMode", "nvdaControllerClient64.dll",
    "SAAPI64.dll", "Tolk.dll", "Tolk.exp", "Tolk.lib", "ZDSRAPI_x64.dll", "ZDSRAPI.ini",
    "version.dll"
)
foreach ($file in $rootFiles) {
    $source = Join-Path $GamePath $file
    if (Test-Path $source) { Copy-Item $source (Join-Path $stage $file) -Force }
}

# Preserve game-root configuration files shipped alongside the loader.
$rootConfigFiles = Get-ChildItem $GamePath -File | Where-Object {
    $_.Extension -in @(".conf", ".ini", ".cfg", ".json", ".config", ".txt", ".xml") -or
    [string]::IsNullOrEmpty($_.Extension)
}
foreach ($file in $rootConfigFiles) {
    Copy-Item $file.FullName (Join-Path $stage $file.Name) -Force
}

$archive = Join-Path $output ("GalMasterAccess-v" + $Version + ".zip")
if (Test-Path $archive) { Remove-Item $archive -Force }
Compress-Archive -Path (Join-Path $stage "*") -DestinationPath $archive -CompressionLevel Optimal
Remove-Item $stage -Recurse -Force
Write-Host "Release package: $archive"
