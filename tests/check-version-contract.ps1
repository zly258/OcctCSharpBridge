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
$expectedTargetFramework = [string]$contract.dotnet.targetFramework
$expectedSdkVersion = [string]$contract.dotnet.sdkVersion
$expectedLanguageVersion = [string]$contract.dotnet.languageVersion
$expectedNativeCount = [int]$contract.api.nativeExports
$expectedManagedCount = [int]$contract.api.managedPInvokes
$expectedPublicTypeCount = [int]$contract.api.publicNetTypes
$expectedViewerCount = [int]$contract.api.viewer
$expectedModelingCount = [int]$contract.api.modeling

foreach ($entry in ([ordered]@{
    bridgeVersion = $expectedVersion
    occtVersion = $expectedOcctVersion
    cmakeMinimumVersion = $expectedCmakeVersion
    targetFramework = $expectedTargetFramework
    sdkVersion = $expectedSdkVersion
    languageVersion = $expectedLanguageVersion
}).GetEnumerator()) {
    if ([string]::IsNullOrWhiteSpace([string]$entry.Value)) {
        throw "Bridge contract value is missing: $($entry.Key)"
    }
}

foreach ($entry in ([ordered]@{
    nativeAbiVersion = $expectedAbiVersion
    nativeExports = $expectedNativeCount
    managedPInvokes = $expectedManagedCount
    publicNetTypes = $expectedPublicTypeCount
    viewer = $expectedViewerCount
    modeling = $expectedModelingCount
}).GetEnumerator()) {
    if ([int]$entry.Value -le 0) {
        throw "Bridge contract numeric value must be positive: $($entry.Key)"
    }
}

if (($expectedViewerCount + $expectedModelingCount) -ne $expectedNativeCount) {
    throw "Viewer + Modeling API counts must equal nativeExports."
}
if ($expectedNativeCount -ne $expectedManagedCount) {
    throw "Native export and managed P/Invoke counts must stay equal."
}

function Read-Text {
    param([Parameter(Mandatory = $true)][string]$RelativePath)

    $path = Join-Path $RepositoryRoot $RelativePath
    if (-not (Test-Path $path -PathType Leaf)) {
        throw "Version contract file was not found: $RelativePath"
    }
    return [System.IO.File]::ReadAllText($path)
}

function Get-ProjectProperty {
    param(
        [Parameter(Mandatory = $true)][xml]$Project,
        [Parameter(Mandatory = $true)][string]$Name
    )

    $node = $Project.SelectSingleNode("/Project/PropertyGroup/$Name[normalize-space(.) != '']")
    if ($null -eq $node) { return $null }
    return [string]$node.InnerText
}

$nativeEngine = Read-Text "src/OcctNative/OcctEngine.cpp"
if (-not $nativeEngine.Contains("return `"$expectedVersion`";")) {
    throw "Native bridge version differs from bridge-contract.json."
}
if (-not $nativeEngine.Contains("return $expectedAbiVersion;")) {
    throw "Native ABI version differs from bridge-contract.json."
}

$bridgeInfo = Read-Text "src/OcctNet/OcctBridgeInfo.cs"
if (-not $bridgeInfo.Contains("ExpectedAbiVersion = $expectedAbiVersion")) {
    throw "Managed ABI expectation differs from bridge-contract.json."
}
if (-not $bridgeInfo.Contains("ManagedVersion = `"$expectedVersion`"")) {
    throw "Managed bridge version differs from bridge-contract.json."
}

$projectFiles = @(
    "src/OcctNet/OcctNet.csproj",
    "src/OcctNet.WinForms/OcctNet.WinForms.csproj",
    "src/OcctNet.Wpf/OcctNet.Wpf.csproj",
    "src/OcctNet.Avalonia/OcctNet.Avalonia.csproj",
    "tests/OcctNet.ManagedTests/OcctNet.ManagedTests.csproj",
    "tests/OcctNet.Smoke/OcctNet.Smoke.csproj"
)
foreach ($relativePath in $projectFiles) {
    [xml]$project = Read-Text $relativePath
    $targetFramework = Get-ProjectProperty $project "TargetFramework"
    $platformTarget = Get-ProjectProperty $project "PlatformTarget"
    if ($targetFramework -ne $expectedTargetFramework) {
        throw "$relativePath target framework is '$targetFramework'; expected '$expectedTargetFramework'."
    }
    if ($platformTarget -ne "x64") {
        throw "$relativePath PlatformTarget is '$platformTarget'; expected 'x64'."
    }
}

$globalJsonPath = Join-Path $RepositoryRoot "global.json"
try {
    $globalJson = Get-Content $globalJsonPath -Raw -Encoding UTF8 | ConvertFrom-Json
}
catch {
    throw "global.json is not valid JSON: $($_.Exception.Message)"
}
if ([string]$globalJson.sdk.version -ne $expectedSdkVersion) {
    throw "global.json SDK differs from bridge-contract.json."
}

[xml]$directoryProps = Read-Text "Directory.Build.props"
$languageVersion = Get-ProjectProperty $directoryProps "LangVersion"
if ($languageVersion -ne $expectedLanguageVersion) {
    throw "Directory.Build.props LangVersion differs from bridge-contract.json."
}

$nativeCmake = Read-Text "src/OcctNative/CMakeLists.txt"
foreach ($token in @(
    "cmake_minimum_required(VERSION $expectedCmakeVersion)",
    "bridge-contract.json",
    "string(JSON BRIDGE_VERSION",
    "string(JSON BRIDGE_ABI_VERSION",
    "string(JSON REQUIRED_OCCT_VERSION",
    "OcctCSharpBridge requires exactly OCCT `${REQUIRED_OCCT_VERSION}"
)) {
    if (-not $nativeCmake.Contains($token)) {
        throw "Native CMake version contract is missing: $token"
    }
}

$contractText = [System.IO.File]::ReadAllText($contractPath)
if ($contractText.Contains("compatibilityPublicNetTypes")) {
    throw "Compatibility API accounting must not be reintroduced into the new library contract."
}

Write-Host ("[version] Bridge {0}, ABI {1}, OCCT {2}, SDK {3}, target {4}, C# {5}, API {6}/{7}, public types {8}, viewer/modeling {9}/{10}." -f
    $expectedVersion,
    $expectedAbiVersion,
    $expectedOcctVersion,
    $expectedSdkVersion,
    $expectedTargetFramework,
    $expectedLanguageVersion,
    $expectedNativeCount,
    $expectedManagedCount,
    $expectedPublicTypeCount,
    $expectedViewerCount,
    $expectedModelingCount) -ForegroundColor Green
