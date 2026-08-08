param(
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"
$project = Join-Path $PSScriptRoot "..\GalMasterAccess.csproj"

dotnet build $project --configuration $Configuration
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed."
    exit $LASTEXITCODE
}

$dll = Join-Path $PSScriptRoot "..\bin\$Configuration\net472\GalMasterAccess.dll"
Write-Host "Build succeeded: $dll"
