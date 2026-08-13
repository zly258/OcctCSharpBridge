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
    param([xml]$Project, [string]$Name)
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
try { $contract = Get-Content $contractPath -Raw -Encoding UTF8 | ConvertFrom-Json }
catch { throw "bridge-contract.json is not valid JSON: $($_.Exception.Message)" }

$expectedVersion = [string]$contract.bridgeVersion
$expectedAbiVersion = [int]$contract.nativeAbiVersion
$expectedOcctVersion = [string]$contract.occtVersion
$expectedCmakeVersion = [string]$contract.cmakeMinimumVersion
$expectedAuthor = [string]$contract.author
$expectedTargetFramework = [string]$contract.dotnet.targetFramework
$expectedSdkVersion = [string]$contract.dotnet.sdkVersion
$expectedLanguageVersion = [string]$contract.dotnet.languageVersion
$expectedNativeCount = [int]$contract.api.nativeExports
$expectedManagedCount = [int]$contract.api.managedPInvokes
$expectedPublicTypeCount = [int]$contract.api.publicNetTypes
$expectedViewerCount = [int]$contract.api.viewer
$expectedModelingCount = [int]$contract.api.modeling

if ([int]$contract.schemaVersion -ne 2) { throw "Avalonia branch requires bridge-contract schemaVersion 2." }
if ([string]$contract.platform -ne "cross-platform-x64") { throw "Avalonia branch platform must be cross-platform-x64." }
$platforms = @($contract.platforms | ForEach-Object { [string]$_ })
foreach ($requiredPlatform in @("windows-x64", "linux-x64")) {
    if ($requiredPlatform -notin $platforms) { throw "Avalonia branch must declare platform: $requiredPlatform" }
}
if ($platforms.Count -ne 2) { throw "Avalonia branch currently supports exactly windows-x64 and linux-x64." }

foreach ($entry in ([ordered]@{
    bridgeVersion = $expectedVersion; occtVersion = $expectedOcctVersion; cmakeMinimumVersion = $expectedCmakeVersion;
    author = $expectedAuthor; targetFramework = $expectedTargetFramework; sdkVersion = $expectedSdkVersion;
    languageVersion = $expectedLanguageVersion
}).GetEnumerator()) {
    if ([string]::IsNullOrWhiteSpace([string]$entry.Value)) { throw "Bridge contract value is missing: $($entry.Key)" }
}
foreach ($entry in ([ordered]@{
    nativeAbiVersion = $expectedAbiVersion; nativeExports = $expectedNativeCount; managedPInvokes = $expectedManagedCount;
    publicNetTypes = $expectedPublicTypeCount; viewer = $expectedViewerCount; modeling = $expectedModelingCount
}).GetEnumerator()) {
    if ([int]$entry.Value -le 0) { throw "Bridge contract numeric value must be positive: $($entry.Key)" }
}
if (($expectedViewerCount + $expectedModelingCount) -ne $expectedNativeCount) { throw "Viewer + Modeling API counts must equal nativeExports." }
if ($expectedNativeCount -ne $expectedManagedCount) { throw "Native export and managed P/Invoke counts must stay equal." }
if ($expectedTargetFramework -ne "net10.0") { throw "Avalonia Core and UI host must target net10.0." }
$contractSdk = Convert-ToSdkVersion $expectedSdkVersion "bridge-contract.json"

$nativeEngine = Read-Text "src/OcctNative/OcctEngine.cpp"
if (-not $nativeEngine.Contains("return `"$expectedVersion`";")) { throw "Native bridge version differs from bridge-contract.json." }
if (-not $nativeEngine.Contains("return $expectedAbiVersion;")) { throw "Native ABI version differs from bridge-contract.json." }
$bridgeInfo = Read-Text "src/OcctNet/OcctBridgeInfo.cs"
if (-not $bridgeInfo.Contains("ExpectedAbiVersion = $expectedAbiVersion")) { throw "Managed ABI expectation differs from bridge-contract.json." }
if (-not $bridgeInfo.Contains("ManagedVersion = `"$expectedVersion`"")) { throw "Managed bridge version differs from bridge-contract.json." }

$projectFiles = @(
    "src/OcctNet/OcctNet.csproj",
    "src/OcctNet.Avalonia/OcctNet.Avalonia.csproj",
    "tests/OcctNet.ManagedTests/OcctNet.ManagedTests.csproj",
    "tests/OcctNet.Smoke/OcctNet.Smoke.csproj",
    "tests/OcctNet.AvaloniaSmoke/OcctNet.AvaloniaSmoke.csproj"
)
foreach ($relativePath in $projectFiles) {
    [xml]$project = Read-Text $relativePath
    $targetFramework = Get-ProjectProperty $project "TargetFramework"
    $platformTarget = Get-ProjectProperty $project "PlatformTarget"
    if ($targetFramework -ne $expectedTargetFramework) { throw "$relativePath target framework is '$targetFramework'; expected '$expectedTargetFramework'." }
    if ($platformTarget -ne "x64") { throw "$relativePath PlatformTarget is '$platformTarget'; expected 'x64'." }
}

try { $globalJson = Get-Content (Join-Path $RepositoryRoot "global.json") -Raw -Encoding UTF8 | ConvertFrom-Json }
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
if ([string]$globalJson.test.runner -ne "Microsoft.Testing.Platform") { throw "global.json must select Microsoft.Testing.Platform." }

[xml]$directoryProps = Read-Text "Directory.Build.props"
if ((Get-ProjectProperty $directoryProps "LangVersion") -ne $expectedLanguageVersion) { throw "Directory.Build.props LangVersion differs from bridge-contract.json." }
if ((Get-ProjectProperty $directoryProps "Authors") -ne $expectedAuthor -or (Get-ProjectProperty $directoryProps "Company") -ne $expectedAuthor) {
    throw "Directory.Build.props Authors/Company must match bridge-contract.json author '$expectedAuthor'."
}

$nativeCmake = Read-Text "src/OcctNative/CMakeLists.txt"
foreach ($token in @(
    "cmake_minimum_required(VERSION $expectedCmakeVersion)", "bridge-contract.json", "string(JSON BRIDGE_VERSION",
    "string(JSON BRIDGE_ABI_VERSION", "string(JSON REQUIRED_OCCT_VERSION", "/usr/local/include/opencascade", "/usr/local/lib"
)) {
    if (-not $nativeCmake.Contains($token)) { throw "Native CMake version/platform contract is missing: $token" }
}

Write-Host ("[version] Avalonia Bridge {0}, ABI {1}, OCCT {2}, SDK major {3} (reference {4}), target {5}, platforms windows-x64/linux-x64, API {6}/{7}, public types {8}, viewer/modeling {9}/{10}." -f
    $expectedVersion, $expectedAbiVersion, $expectedOcctVersion, $contractSdk.Major, $expectedSdkVersion, $expectedTargetFramework,
    $expectedNativeCount, $expectedManagedCount, $expectedPublicTypeCount, $expectedViewerCount, $expectedModelingCount) -ForegroundColor Green
