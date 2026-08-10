param(
    [string]$OcctRoot = $env:OCCT_ROOT
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$RepoRoot = Split-Path -Parent $PSCommandPath
$BuildScript = Join-Path $RepoRoot "build.ps1"
$ContractPath = Join-Path $RepoRoot "bridge-contract.json"
$DistRoot = Join-Path $RepoRoot "dist\win-x64"
$Configuration = "Release"

function Assert-Path {
    param([Parameter(Mandatory = $true)][string]$Path)
    if (-not (Test-Path -LiteralPath $Path)) {
        throw "Required path was not found: $Path"
    }
}

function Invoke-Build {
    param([Parameter(Mandatory = $true)][string]$Target)

    $arguments = @($Target, $Configuration)
    if (-not [string]::IsNullOrWhiteSpace($OcctRoot)) {
        $arguments += @("-OcctRoot", $OcctRoot)
    }

    & $BuildScript @arguments
    if ($LASTEXITCODE -ne 0) {
        throw "Bridge $Target validation failed. Distribution was not updated."
    }
}

Assert-Path $BuildScript
Assert-Path $ContractPath

Write-Host "[dist] Validating Release build before publishing binaries..." -ForegroundColor Cyan
Invoke-Build "all"
Invoke-Build "test"
Invoke-Build "smoke"

$contract = Get-Content -LiteralPath $ContractPath -Raw -Encoding UTF8 | ConvertFrom-Json
$targetFramework = [string]$contract.dotnet.targetFramework

$files = [ordered]@{
    "OcctNative.dll" = Join-Path $RepoRoot "build\native\bin\Release\OcctNative.dll"
    "OcctNet.dll" = Join-Path $RepoRoot "src\OcctNet\bin\x64\Release\$targetFramework\OcctNet.dll"
    "OcctNet.WinForms.dll" = Join-Path $RepoRoot "src\OcctNet.WinForms\bin\x64\Release\$targetFramework\OcctNet.WinForms.dll"
    "OcctNet.Wpf.dll" = Join-Path $RepoRoot "src\OcctNet.Wpf\bin\x64\Release\$targetFramework\OcctNet.Wpf.dll"
    "OcctNet.Avalonia.dll" = Join-Path $RepoRoot "src\OcctNet.Avalonia\bin\x64\Release\$targetFramework\OcctNet.Avalonia.dll"
}

foreach ($source in $files.Values) {
    Assert-Path $source
}

Remove-Item -LiteralPath $DistRoot -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $DistRoot -Force | Out-Null

foreach ($entry in $files.GetEnumerator()) {
    Copy-Item -LiteralPath $entry.Value -Destination (Join-Path $DistRoot $entry.Key) -Force
}
Copy-Item -LiteralPath $ContractPath -Destination (Join-Path $DistRoot "bridge-contract.json") -Force

$sourceCommit = "unknown"
if (Get-Command git -ErrorAction SilentlyContinue) {
    $resolved = (& git -C $RepoRoot rev-parse HEAD 2>$null)
    if ($LASTEXITCODE -eq 0 -and -not [string]::IsNullOrWhiteSpace($resolved)) {
        $sourceCommit = $resolved.Trim()
    }
}

$manifestFiles = @()
foreach ($name in @($files.Keys) + @("bridge-contract.json")) {
    $path = Join-Path $DistRoot $name
    $manifestFiles += [ordered]@{
        name = $name
        sha256 = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
    }
}

$manifest = [ordered]@{
    schemaVersion = 1
    bridgeVersion = [string]$contract.bridgeVersion
    nativeAbiVersion = [int]$contract.nativeAbiVersion
    occtVersion = [string]$contract.occtVersion
    platform = [string]$contract.platform
    targetFramework = $targetFramework
    sdkVersion = [string]$contract.dotnet.sdkVersion
    languageVersion = [string]$contract.dotnet.languageVersion
    configuration = $Configuration
    sourceCommit = $sourceCommit
    files = $manifestFiles
}

$utf8 = [System.Text.UTF8Encoding]::new($false)
$manifestJson = $manifest | ConvertTo-Json -Depth 8
[System.IO.File]::WriteAllText((Join-Path $DistRoot "bridge-manifest.json"), $manifestJson + [Environment]::NewLine, $utf8)

Write-Host "[dist] Binary SDK updated: $DistRoot" -ForegroundColor Green
Write-Host "[dist] Commit dist/win-x64 only after this command completes successfully." -ForegroundColor Green
