param(
    [string]$BridgeBranch = 'main',
    [string]$OcctRoot = ''
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$root = $PSScriptRoot
$syncBridge = Join-Path $root 'tools\sync-bridge.ps1'
$bridgeSdk = Join-Path $root 'external\OcctCSharpBridge\win-x64'

if (-not (Test-Path -LiteralPath $syncBridge -PathType Leaf)) {
    throw "Bridge synchronization script was not found: $syncBridge"
}

Write-Host "[sync] Updating OcctCSharpBridge origin/$BridgeBranch..." -ForegroundColor Cyan
$parameters = @{
    SourceBranch = $BridgeBranch
    ForceRebuild = $true
}
if (-not [string]::IsNullOrWhiteSpace($OcctRoot)) { $parameters.OcctRoot = $OcctRoot }
& $syncBridge @parameters
if (-not $?) { throw 'Bridge synchronization failed.' }

$manifestPath = Join-Path $bridgeSdk 'bridge-manifest.json'
if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
    throw "Bridge manifest was not found after synchronization: $manifestPath"
}
$manifest = Get-Content -LiteralPath $manifestPath -Raw -Encoding UTF8 | ConvertFrom-Json
Write-Host "[sync] Bridge branch: $BridgeBranch"
Write-Host "[sync] Bridge source: $($manifest.sourceCommit)"
Write-Host "[sync] Binary SDK:    $bridgeSdk"
Write-Host '[sync] Completed.' -ForegroundColor Green
