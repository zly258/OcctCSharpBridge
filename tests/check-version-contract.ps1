param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$contractPath = Join-Path $RepositoryRoot "bridge-contract.json"
if (-not (Test-Path $contractPath -PathType Leaf)) {
    throw "Bridge contract file was not found: bridge-contract.json"
}

try {
    $contract = Get-Content $contractPath -Raw -Encoding UTF8 | ConvertFrom-Json
}
catch {
    throw "bridge-contract.json is not valid JSON: $($_.Exception.Message)"
}

$expectedVersion = [string]$contract.bridgeVersion
$expectedAbiVersion = [int]$contract.nativeAbiVersion
$expectedOcctVersion = [string]$contract.occtVersion
$expectedCmakeVersion = [string]$contract.cmakeMinimumVersion
$expectedSdkVersion = [string]$contract.dotnet.sdkVersion
$expectedLanguageVersion = [string]$contract.dotnet.languageVersion
$expectedNativeCount = [int]$contract.api.nativeExports
$expectedManagedCount = [int]$contract.api.managedPInvokes
$expectedPublicTypeCount = [int]$contract.api.publicNetTypes

foreach ($entry in [ordered]@{
    bridgeVersion = $expectedVersion
    occtVersion = $expectedOcctVersion
    cmakeMinimumVersion = $expectedCmakeVersion
    dotnetSdkVersion = $expectedSdkVersion
    languageVersion = $expectedLanguageVersion
}.GetEnumerator()) {
    if ([string]::IsNullOrWhiteSpace([string]$entry.Value)) {
        throw "Bridge contract value is missing: $($entry.Key)"
    }
}

foreach ($entry in [ordered]@{
    nativeAbiVersion = $expectedAbiVersion
    nativeExports = $expectedNativeCount
    managedPInvokes = $expectedManagedCount
    publicNetTypes = $expectedPublicTypeCount
}.GetEnumerator()) {
    if ([int]$entry.Value -le 0) {
        throw "Bridge contract numeric value must be positive: $($entry.Key)"
    }
}

$contracts = [ordered]@{
    "src/OcctNative/OcctEngine.cpp" = @(
        "occt_bridge_version()",
        "return `"$expectedVersion`";",
        "occt_bridge_abi_version()",
        "return $expectedAbiVersion;"
    )
    "src/OcctNet/OcctBridgeInfo.cs" = @(
        "ExpectedAbiVersion = $expectedAbiVersion",
        "ManagedVersion = `"$expectedVersion`""
    )
    "README.md" = @($expectedVersion, $expectedOcctVersion, $expectedCmakeVersion)
    "README.zh-CN.md" = @($expectedVersion, $expectedOcctVersion, $expectedCmakeVersion)
    "docs/API_COVERAGE.md" = @(
        "Native exports:",
        [string]$expectedNativeCount,
        "Managed P/Invoke declarations:",
        [string]$expectedManagedCount,
        "Public .NET types:",
        [string]$expectedPublicTypeCount,
        "Native bridge version:",
        $expectedVersion
    )
    "docs/API_COVERAGE.zh-CN.md" = @(
        "Native exports:",
        [string]$expectedNativeCount,
        "Managed P/Invoke declarations:",
        [string]$expectedManagedCount,
        "Public .NET types:",
        [string]$expectedPublicTypeCount,
        "原生桥接版本：",
        $expectedVersion
    )
    "src/OcctNative/CMakeLists.txt" = @(
        "cmake_minimum_required(VERSION $expectedCmakeVersion)",
        "requires exactly OCCT $expectedOcctVersion"
    )
    "global.json" = @($expectedSdkVersion)
    "Directory.Build.props" = @("<LangVersion>$expectedLanguageVersion</LangVersion>")
}

foreach ($contractEntry in $contracts.GetEnumerator()) {
    $path = Join-Path $RepositoryRoot $contractEntry.Key
    if (-not (Test-Path $path -PathType Leaf)) {
        throw "Version contract file was not found: $($contractEntry.Key)"
    }
    $content = [System.IO.File]::ReadAllText($path)
    foreach ($token in $contractEntry.Value) {
        if (-not $content.Contains([string]$token)) {
            throw "Version contract is stale in $($contractEntry.Key): $token"
        }
    }
}

foreach ($path in @("build.ps1", "src/OcctNative/CMakeLists.txt")) {
    $content = [System.IO.File]::ReadAllText((Join-Path $RepositoryRoot $path))
    if ($content -match '(?i)D:[\\/]tools[\\/]occt') {
        throw "A machine-specific OCCT path remains in $path."
    }
}

Write-Host ("[version] Bridge {0}, ABI {1}, OCCT {2}, SDK {3}, API {4}/{5}, public types {6}." -f
    $expectedVersion,
    $expectedAbiVersion,
    $expectedOcctVersion,
    $expectedSdkVersion,
    $expectedNativeCount,
    $expectedManagedCount,
    $expectedPublicTypeCount) -ForegroundColor Green
