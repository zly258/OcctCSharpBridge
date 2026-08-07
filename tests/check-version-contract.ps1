param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$expectedVersion = "2.5.0"
$expectedNativeCount = 339
$expectedPublicTypeCount = 75

$contracts = [ordered]@{
    "src/OcctNative/OcctEngine.cpp" = @("occt_bridge_version()", $expectedVersion)
    "src/OcctNet/OcctBridgeInfo.cs" = @("ManagedVersion", $expectedVersion)
    "README.md" = @("Bridge version:", $expectedVersion)
    "README.zh-CN.md" = @("Bridge 版本：", $expectedVersion)
    "docs/API_COVERAGE.md" = @(
        "Native exports:",
        [string]$expectedNativeCount,
        "Managed P/Invoke declarations:",
        "Public .NET types:",
        [string]$expectedPublicTypeCount,
        "Native bridge version:",
        $expectedVersion
    )
    "docs/API_COVERAGE.zh-CN.md" = @(
        "Native exports:",
        [string]$expectedNativeCount,
        "Managed P/Invoke declarations:",
        "Public .NET types:",
        [string]$expectedPublicTypeCount,
        "原生桥接版本：",
        $expectedVersion
    )
}

foreach ($contract in $contracts.GetEnumerator()) {
    $path = Join-Path $RepositoryRoot $contract.Key
    if (-not (Test-Path $path -PathType Leaf)) {
        throw "Version contract file was not found: $($contract.Key)"
    }
    $content = [System.IO.File]::ReadAllText($path)
    foreach ($token in $contract.Value) {
        if (-not $content.Contains([string]$token)) {
            throw "Version contract is stale in $($contract.Key): $token"
        }
    }
}

foreach ($path in @("build.ps1", "src/OcctNative/CMakeLists.txt")) {
    $content = [System.IO.File]::ReadAllText((Join-Path $RepositoryRoot $path))
    if ($content -match '(?i)D:[\\/]tools[\\/]occt') {
        throw "A machine-specific OCCT path remains in $path."
    }
}

Write-Host "[version] Bridge $expectedVersion, ABI 2 and API inventory counts validated." -ForegroundColor Green
