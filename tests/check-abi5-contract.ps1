param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$contractPath = Join-Path $RepositoryRoot "bridge-contract.json"
if (-not (Test-Path $contractPath -PathType Leaf)) {
    throw "Bridge contract file was not found: bridge-contract.json"
}

$contract = Get-Content $contractPath -Raw -Encoding UTF8 | ConvertFrom-Json
if ([int]$contract.schemaVersion -ne 3) {
    throw "ABI5-only Bridge 3 contract must use schemaVersion 3."
}
if ([int]$contract.nativeAbi.current -ne 5 -or [int]$contract.nativeAbi.minimumSupported -ne 5) {
    throw "Bridge 3 must expose ABI 5 only."
}

$contractText = [System.IO.File]::ReadAllText($contractPath)
foreach ($retired in @(
    '"legacy"',
    '"compatibility"',
    'legacyAbi4Exports',
    'compatibilityExtensions',
    'compatibilityExtensionNames',
    'plannedRemoval'
)) {
    if ($contractText.Contains($retired)) {
        throw "ABI4 compatibility metadata must not exist in bridge-contract.json: $retired"
    }
}

foreach ($retiredPath in @(
    "tests/contracts/abi4-exports.txt",
    "tests/check-abi-compatibility.ps1",
    "tests/compatibility/OcctNet.LegacyCompatibilityTests.csproj",
    "tests/compatibility/Program.cs"
)) {
    if (Test-Path (Join-Path $RepositoryRoot $retiredPath)) {
        throw "Retired ABI4 compatibility artifact must not exist: $retiredPath"
    }
}

Write-Host "[abi5] ABI 5 is the only supported native ABI; ABI4 compatibility artifacts are absent." -ForegroundColor Green
