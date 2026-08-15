param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Test-TrackedPath {
    param([Parameter(Mandatory = $true)][string]$RelativePath)
    $normalized = $RelativePath.Replace('\', '/')
    $tracked = @(& git -C $RepositoryRoot ls-files -- $normalized "$normalized/**" 2>$null)
    if ($LASTEXITCODE -ne 0) {
        throw "Unable to inspect tracked repository paths with git ls-files."
    }
    return $tracked.Count -gt 0
}

function Get-TrackedSourceText {
    param(
        [Parameter(Mandatory = $true)][string]$RelativeRoot,
        [Parameter(Mandatory = $true)][string[]]$Extensions
    )

    $normalizedRoot = $RelativeRoot.Replace('\', '/').TrimEnd('/')
    $tracked = @(& git -C $RepositoryRoot ls-files -- "$normalizedRoot/**" 2>$null)
    if ($LASTEXITCODE -ne 0) {
        throw "Unable to inspect tracked repository files with git ls-files: $RelativeRoot"
    }

    $parts = @()
    foreach ($relativePath in $tracked) {
        if ([System.IO.Path]::GetExtension($relativePath) -notin $Extensions) { continue }
        $fullPath = Join-Path $RepositoryRoot $relativePath
        if (-not (Test-Path $fullPath -PathType Leaf)) {
            throw "Tracked source file is missing from the working tree: $relativePath"
        }
        $parts += [System.IO.File]::ReadAllText($fullPath)
    }
    return $parts -join "`n"
}

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
if ([string]$contract.api.policy -ne "abi5-only") {
    throw "bridge-contract.json api.policy must be 'abi5-only'."
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
    "tests/compatibility",
    "tests/compatibility/OcctNet.LegacyCompatibilityTests.csproj",
    "tests/compatibility/Program.cs"
)) {
    if (Test-TrackedPath $retiredPath) {
        throw "Retired ABI4 compatibility artifact must not be tracked: $retiredPath"
    }
}

$nativeText = Get-TrackedSourceText "src/OcctNative" @('.h', '.hpp', '.hxx', '.cpp', '.cxx')
foreach ($retiredToken in @(
    "OcctModelHandle",
    "modelOf("
)) {
    if ($nativeText.Contains($retiredToken)) {
        throw "Retired pre-ABI5 modeling implementation token remains in tracked Native source: $retiredToken"
    }
}

Write-Host "[abi5] ABI 5 is the only supported native ABI; no ABI4 compatibility artifacts or legacy modeling handles are tracked." -ForegroundColor Green
