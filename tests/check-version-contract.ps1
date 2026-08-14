param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Text {
    param([Parameter(Mandatory = $true)][string]$RelativePath)
    $path = Join-Path $RepositoryRoot $RelativePath
    if (-not (Test-Path $path -PathType Leaf)) { throw "Version contract file was not found: $RelativePath" }
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

function Convert-ToSdkVersion {
    param([Parameter(Mandatory = $true)][string]$Value, [Parameter(Mandatory = $true)][string]$Source)
    try { return [version]$Value }
    catch { throw "$Source contains an invalid .NET SDK version: '$Value'." }
}

$contractPath = Join-Path $RepositoryRoot "bridge-contract.json"
if (-not (Test-Path $contractPath -PathType Leaf)) { throw "Bridge contract file was not found: bridge-contract.json" }
try { $contract = Get-Content $contractPath -Raw -Encoding UTF8 | ConvertFrom-Json }
catch { throw "bridge-contract.json is not valid JSON: $($_.Exception.Message)" }

$expectedVersion = [string]$contract.bridgeVersion
$expectedAbiVersion = [int]$contract.nativeAbi.current
$expectedOcctVersion = [string]$contract.occtVersion
$expectedPlatform = [string]$contract.platform
$supportedPlatforms = @($contract.supportedPlatforms | ForEach-Object { [string]$_ })
$expectedCmakeVersion = [string]$contract.cmakeMinimumVersion
$expectedAuthor = [string]$contract.author
$expectedTargetFramework = [string]$contract.dotnet.targetFramework
$expectedDesktopTargetFramework = [string]$contract.dotnet.desktopTargetFramework
$expectedSdkVersion = [string]$contract.dotnet.sdkVersion
$expectedLanguageVersion = [string]$contract.dotnet.languageVersion
$expectedNativeCount = [int]$contract.api.nativeExports
$expectedManagedCount = [int]$contract.api.managedPInvokes
$expectedPublicTypeCount = [int]$contract.api.publicNetTypes
$expectedViewerCount = [int]$contract.api.viewer
$expectedModelingCount = [int]$contract.api.modeling
$expectedMinimumAbiVersion = [int]$contract.nativeAbi.minimumSupported
$expectedCurrentAbiExports = [int]$contract.api.abi5Exports
$expectedLegacyAbiExports = [int]$contract.api.legacyAbi4Exports
$expectedCompatibilityExtensions = [int]$contract.api.compatibilityExtensions

foreach ($entry in ([ordered]@{
    bridgeVersion = $expectedVersion
    occtVersion = $expectedOcctVersion
    platform = $expectedPlatform
    cmakeMinimumVersion = $expectedCmakeVersion
    author = $expectedAuthor
    targetFramework = $expectedTargetFramework
    sdkVersion = $expectedSdkVersion
    desktopTargetFramework = $expectedDesktopTargetFramework
    languageVersion = $expectedLanguageVersion
}).GetEnumerator()) {
    if ([string]::IsNullOrWhiteSpace([string]$entry.Value)) { throw "Bridge contract value is missing: $($entry.Key)" }
}
foreach ($entry in ([ordered]@{
    nativeAbiVersion = $expectedAbiVersion
    nativeExports = $expectedNativeCount
    managedPInvokes = $expectedManagedCount
    publicNetTypes = $expectedPublicTypeCount
    viewer = $expectedViewerCount
    modeling = $expectedModelingCount
    minimumSupportedAbi = $expectedMinimumAbiVersion
    abi5Exports = $expectedCurrentAbiExports
    legacyAbi4Exports = $expectedLegacyAbiExports
    compatibilityExtensions = $expectedCompatibilityExtensions
}).GetEnumerator()) {
    if ([int]$entry.Value -le 0) { throw "Bridge contract numeric value must be positive: $($entry.Key)" }
}
if (($expectedViewerCount + $expectedModelingCount) -ne $expectedNativeCount) { throw "Viewer + Modeling API counts must equal nativeExports." }
if ($expectedNativeCount -ne $expectedManagedCount) { throw "Native export and managed P/Invoke counts must stay equal." }
$contractSdk = Convert-ToSdkVersion $expectedSdkVersion "bridge-contract.json"

if ([int]$contract.schemaVersion -ne 2) { throw "Bridge 3 contract must use schema version 2." }
if ($expectedPlatform -ne "cross-platform-x64") {
    throw "Source bridge contract platform must be cross-platform-x64; found '$expectedPlatform'."
}
$requiredPlatforms = @("windows-x64", "linux-x64")
if ($supportedPlatforms.Count -ne $requiredPlatforms.Count) {
    throw "Bridge contract supportedPlatforms must contain exactly windows-x64 and linux-x64."
}
foreach ($platform in $requiredPlatforms) {
    if ($platform -notin $supportedPlatforms) {
        throw "Bridge contract supportedPlatforms is missing '$platform'."
    }
}
if (@($supportedPlatforms | Group-Object | Where-Object Count -gt 1).Count -ne 0) {
    throw "Bridge contract supportedPlatforms must not contain duplicates."
}
if ($expectedMinimumAbiVersion -ne 4 -or @($contract.nativeAbi.legacy) -notcontains 4) {
    throw "ABI 4 must remain declared as the minimum supported legacy ABI."
}
if ([string]$contract.compatibility.abi4 -ne "deprecated" -or [string]$contract.compatibility.plannedRemoval -ne "4.0") {
    throw "ABI 4 deprecation and planned removal metadata is incomplete."
}
if (($expectedCurrentAbiExports + $expectedLegacyAbiExports + $expectedCompatibilityExtensions) -ne $expectedNativeCount) {
    throw "ABI export accounting must equal nativeExports."
}
$nativeEngine = Read-Text "src/OcctNative/core/OcctEngine.cpp"
if (-not $nativeEngine.Contains("return `"$expectedVersion`";")) { throw "Native bridge version differs from bridge-contract.json." }
if (-not $nativeEngine.Contains("return $expectedAbiVersion;")) { throw "Native ABI version differs from bridge-contract.json." }
$bridgeInfo = Read-Text "src/OcctNet/OcctBridgeInfo.cs"
if (-not $bridgeInfo.Contains("ExpectedAbiVersion = $expectedAbiVersion")) { throw "Managed ABI expectation differs from bridge-contract.json." }
if (-not $bridgeInfo.Contains("ManagedVersion = `"$expectedVersion`"")) { throw "Managed bridge version differs from bridge-contract.json." }

$projectFiles = [ordered]@{
    "src/OcctNet/OcctNet.csproj" = $expectedTargetFramework
    "src/OcctNet.Avalonia/OcctNet.Avalonia.csproj" = $expectedTargetFramework
    "tests/OcctNet.AvaloniaSmoke/OcctNet.AvaloniaSmoke.csproj" = $expectedTargetFramework
    "tests/OcctNet.ManagedTests/OcctNet.ManagedTests.csproj" = $expectedTargetFramework
    "src/OcctNet.WinForms/OcctNet.WinForms.csproj" = $expectedDesktopTargetFramework
    "src/OcctNet.Wpf/OcctNet.Wpf.csproj" = $expectedDesktopTargetFramework
    "tests/OcctNet.Smoke/OcctNet.Smoke.csproj" = $expectedDesktopTargetFramework
    "tests/compatibility/OcctNet.LegacyCompatibilityTests.csproj" = $expectedDesktopTargetFramework
}
foreach ($entry in $projectFiles.GetEnumerator()) {
    $relativePath = [string]$entry.Key
    $expectedProjectFramework = [string]$entry.Value
    [xml]$project = Read-Text $relativePath
    $targetFramework = Get-ProjectProperty $project "TargetFramework"
    $platformTarget = Get-ProjectProperty $project "PlatformTarget"
    if ($targetFramework -ne $expectedProjectFramework) { throw "$relativePath target framework is '$targetFramework'; expected '$expectedProjectFramework'." }
    if ($platformTarget -ne "x64") { throw "$relativePath PlatformTarget is '$platformTarget'; expected 'x64'." }
}

$globalJsonPath = Join-Path $RepositoryRoot "global.json"
try { $globalJson = Get-Content $globalJsonPath -Raw -Encoding UTF8 | ConvertFrom-Json }
catch { throw "global.json is not valid JSON: $($_.Exception.Message)" }
$globalSdkText = [string]$globalJson.sdk.version
$globalSdk = Convert-ToSdkVersion $globalSdkText "global.json"
if ($globalSdk.Major -ne $contractSdk.Major) {
    throw "global.json must select .NET SDK major $($contractSdk.Major); found $globalSdkText."
}
if ([string]$globalJson.sdk.rollForward -ne "latestMinor") {
    throw "global.json must use rollForward 'latestMinor' so any stable .NET $($contractSdk.Major).x SDK can be used without rolling to the next major."
}
if ([bool]$globalJson.sdk.allowPrerelease) { throw "global.json must not allow prerelease SDKs." }
if ([string]$globalJson.test.runner -ne "Microsoft.Testing.Platform") { throw "global.json must select Microsoft.Testing.Platform for .NET 10 tests." }

[xml]$directoryProps = Read-Text "Directory.Build.props"
$languageVersion = Get-ProjectProperty $directoryProps "LangVersion"
$author = Get-ProjectProperty $directoryProps "Authors"
$company = Get-ProjectProperty $directoryProps "Company"
if ($languageVersion -ne $expectedLanguageVersion) { throw "Directory.Build.props LangVersion differs from bridge-contract.json." }
if ($author -ne $expectedAuthor -or $company -ne $expectedAuthor) { throw "Directory.Build.props Authors/Company must match bridge-contract.json author '$expectedAuthor'." }

$nativeCmake = Read-Text "src/OcctNative/CMakeLists.txt"
foreach ($token in @(
    "cmake_minimum_required(VERSION $expectedCmakeVersion)",
    "bridge-contract.json",
    "string(JSON BRIDGE_VERSION",
    "string(JSON BRIDGE_ABI_VERSION",
    "string(JSON REQUIRED_OCCT_VERSION",
    "OcctCSharpBridge requires exactly OCCT `${REQUIRED_OCCT_VERSION}"
)) {
    if (-not $nativeCmake.Contains($token)) { throw "Native CMake version contract is missing: $token" }
}

$contractText = [System.IO.File]::ReadAllText($contractPath)
if ($contractText.Contains("compatibilityPublicNetTypes")) { throw "Compatibility API accounting must not be reintroduced into the new library contract." }

Write-Host ("[version] Bridge {0}, ABI {1}, OCCT {2}, platform {3}, author {4}, SDK major {5} (reference {6}), target {7}, C# {8}, API {9}/{10}, public types {11}, viewer/modeling {12}/{13}." -f
    $expectedVersion,
    $expectedAbiVersion,
    $expectedOcctVersion,
    $expectedPlatform,
    $expectedAuthor,
    $contractSdk.Major,
    $expectedSdkVersion,
    $expectedTargetFramework,
    $expectedLanguageVersion,
    $expectedNativeCount,
    $expectedManagedCount,
    $expectedPublicTypeCount,
    $expectedViewerCount,
    $expectedModelingCount) -ForegroundColor Green
