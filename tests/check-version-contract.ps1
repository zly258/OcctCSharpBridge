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
$expectedMinimumAbiVersion = [int]$contract.nativeAbi.minimumSupported
$expectedOcctVersion = [string]$contract.occtVersion
$expectedPlatform = [string]$contract.platform
$supportedPlatforms = @($contract.supportedPlatforms | ForEach-Object { [string]$_ })
$expectedCmakeVersion = [string]$contract.cmakeMinimumVersion
$expectedAuthor = [string]$contract.author
$expectedTargetFramework = [string]$contract.dotnet.targetFramework
$expectedDesktopTargetFramework = [string]$contract.dotnet.desktopTargetFramework
$supportedConsumerFrameworks = @($contract.dotnet.supportedConsumerFrameworks | ForEach-Object { [string]$_ })
$supportedDesktopConsumerFrameworks = @($contract.dotnet.supportedDesktopConsumerFrameworks | ForEach-Object { [string]$_ })
$expectedSdkVersion = [string]$contract.dotnet.sdkVersion
$expectedSdkRollForward = [string]$contract.dotnet.sdkRollForward
$expectedLanguageVersion = [string]$contract.dotnet.languageVersion

foreach ($entry in ([ordered]@{
    bridgeVersion = $expectedVersion
    occtVersion = $expectedOcctVersion
    platform = $expectedPlatform
    cmakeMinimumVersion = $expectedCmakeVersion
    author = $expectedAuthor
    targetFramework = $expectedTargetFramework
    desktopTargetFramework = $expectedDesktopTargetFramework
    sdkVersion = $expectedSdkVersion
    sdkRollForward = $expectedSdkRollForward
    languageVersion = $expectedLanguageVersion
}).GetEnumerator()) {
    if ([string]::IsNullOrWhiteSpace([string]$entry.Value)) { throw "Bridge contract value is missing: $($entry.Key)" }
}

if ([int]$contract.schemaVersion -ne 3) { throw "ABI5-only Bridge 3 contract must use schemaVersion 3." }
if ($expectedAbiVersion -ne 5 -or $expectedMinimumAbiVersion -ne 5) {
    throw "Bridge 3 must declare ABI 5 as both current and minimum supported ABI."
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
        throw "Retired ABI compatibility metadata must not be present: $retired"
    }
}

if ($expectedPlatform -ne "cross-platform-x64") {
    throw "Source bridge contract platform must be cross-platform-x64; found '$expectedPlatform'."
}
$requiredPlatforms = @("windows-x64", "linux-x64")
if ($supportedPlatforms.Count -ne $requiredPlatforms.Count) {
    throw "Bridge contract supportedPlatforms must contain exactly windows-x64 and linux-x64."
}
foreach ($platform in $requiredPlatforms) {
    if ($platform -notin $supportedPlatforms) { throw "Bridge contract supportedPlatforms is missing '$platform'." }
}
if (@($supportedPlatforms | Group-Object | Where-Object Count -gt 1).Count -gt 0) {
    throw "Bridge contract supportedPlatforms must not contain duplicates."
}

if ($expectedTargetFramework -ne "net8.0" -or $expectedDesktopTargetFramework -ne "net8.0-windows") {
    throw "Bridge Binary SDK must use the .NET 8 minimum target framework baseline."
}
$requiredConsumerFrameworks = @("net8.0", "net9.0", "net10.0")
if ($supportedConsumerFrameworks.Count -ne $requiredConsumerFrameworks.Count) {
    throw "supportedConsumerFrameworks must contain exactly net8.0, net9.0 and net10.0."
}
foreach ($framework in $requiredConsumerFrameworks) {
    if ($framework -notin $supportedConsumerFrameworks) { throw "supportedConsumerFrameworks is missing '$framework'." }
}
$requiredDesktopConsumerFrameworks = @("net8.0-windows", "net9.0-windows", "net10.0-windows")
if ($supportedDesktopConsumerFrameworks.Count -ne $requiredDesktopConsumerFrameworks.Count) {
    throw "supportedDesktopConsumerFrameworks must contain exactly the .NET 8/9/10 Windows TFMs."
}
foreach ($framework in $requiredDesktopConsumerFrameworks) {
    if ($framework -notin $supportedDesktopConsumerFrameworks) { throw "supportedDesktopConsumerFrameworks is missing '$framework'." }
}

$minimumSdkVersion = Convert-ToSdkVersion $expectedSdkVersion "bridge-contract.json"
if ($minimumSdkVersion.Major -ne 10 -or $minimumSdkVersion.Minor -ne 0) {
    throw "Bridge SDK baseline must remain on stable .NET 10.x."
}
if ($expectedSdkRollForward -ne "latestFeature") {
    throw "Bridge SDK roll-forward must be latestFeature so any compatible stable .NET 10 feature band/patch can build the repository."
}

$nativeEngine = Read-Text "src/OcctNative/core/OcctEngine.cpp"
if (-not $nativeEngine.Contains("return `"$expectedVersion`";")) { throw "Native bridge version differs from bridge-contract.json." }
if (-not $nativeEngine.Contains("return $expectedAbiVersion;")) { throw "Native ABI version differs from bridge-contract.json." }
$bridgeInfo = Read-Text "src/OcctNet/Core/OcctBridgeInfo.cs"
if (-not $bridgeInfo.Contains("ExpectedAbiVersion = $expectedAbiVersion")) { throw "Managed ABI expectation differs from bridge-contract.json." }
if (-not $bridgeInfo.Contains("ManagedVersion = `"$expectedVersion`"")) { throw "Managed bridge version differs from bridge-contract.json." }

$projectFiles = [ordered]@{
    "src/OcctNet/OcctNet.csproj" = $expectedTargetFramework
    "src/OcctNet.Avalonia/OcctNet.Avalonia.csproj" = $expectedTargetFramework
    "tests/OcctNet.AvaloniaSmoke/OcctNet.AvaloniaSmoke.csproj" = $expectedTargetFramework
    "tests/OcctNet.ManagedTests/OcctNet.ManagedTests.csproj" = $expectedTargetFramework
    "tests/OcctNet.Smoke/OcctNet.Smoke.csproj" = $expectedTargetFramework
    "src/OcctNet.WinForms/OcctNet.WinForms.csproj" = $expectedDesktopTargetFramework
    "src/OcctNet.Wpf/OcctNet.Wpf.csproj" = $expectedDesktopTargetFramework
    "tests/OcctNet.WinFormsSmoke/OcctNet.WinFormsSmoke.csproj" = $expectedDesktopTargetFramework
    "tests/OcctNet.WpfSmoke/OcctNet.WpfSmoke.csproj" = $expectedDesktopTargetFramework
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
[void](Convert-ToSdkVersion $globalSdkText "global.json")
if ($globalSdkText -ne $expectedSdkVersion) {
    throw "global.json SDK baseline must match bridge-contract.json ($expectedSdkVersion); found $globalSdkText."
}
if ([string]$globalJson.sdk.rollForward -ne $expectedSdkRollForward) {
    throw "global.json rollForward must match bridge-contract.json ($expectedSdkRollForward)."
}
if ([bool]$globalJson.sdk.allowPrerelease) { throw "global.json must not allow prerelease SDKs." }
if ([string]$globalJson.test.runner -ne "Microsoft.Testing.Platform") { throw "global.json must select Microsoft.Testing.Platform for repository tests." }

[xml]$directoryProps = Read-Text "Directory.Build.props"
$languageVersion = Get-ProjectProperty $directoryProps "LangVersion"
$packageVersion = Get-ProjectProperty $directoryProps "Version"
$author = Get-ProjectProperty $directoryProps "Authors"
$company = Get-ProjectProperty $directoryProps "Company"
if ($languageVersion -ne $expectedLanguageVersion) { throw "Directory.Build.props LangVersion differs from bridge-contract.json." }
if ($packageVersion -ne $expectedVersion) { throw "Directory.Build.props Version differs from bridge-contract.json." }
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

# Repository hygiene is part of the source contract. Generated SDKs, caches, runtime binaries,
# package archives and unexpectedly large tracked files belong in build/artifact storage, not Git.
$trackedFiles = @(& git -C $RepositoryRoot ls-files)
if ($LASTEXITCODE -ne 0) { throw "Unable to inspect tracked repository files." }
$forbiddenTrackedPathPatterns = @(
    '^(?:dist|artifacts|build|publish|\.cache)/',
    '(?:^|/)(?:bin|obj|TestResults|coverage)/'
)
$forbiddenTrackedExtensions = @(
    '.dll', '.exe', '.so', '.dylib', '.pdb', '.ilk', '.exp', '.idb', '.tlog',
    '.zip', '.tar', '.tgz', '.7z', '.rar', '.nupkg', '.snupkg'
)
$maxTrackedFileBytes = 2MB
foreach ($relativePath in $trackedFiles) {
    $normalized = ([string]$relativePath).Replace('\', '/')
    foreach ($pattern in $forbiddenTrackedPathPatterns) {
        if ($normalized -match $pattern) {
            throw "Generated/cache path must not be tracked by the Bridge source branch: $normalized"
        }
    }

    $extension = [System.IO.Path]::GetExtension($normalized).ToLowerInvariant()
    if ($extension -in $forbiddenTrackedExtensions -or $normalized.EndsWith('.tar.gz', [StringComparison]::OrdinalIgnoreCase)) {
        throw "Generated binary/archive must not be tracked by the Bridge source branch: $normalized"
    }

    $fullPath = Join-Path $RepositoryRoot $relativePath
    if (Test-Path -LiteralPath $fullPath -PathType Leaf) {
        $length = (Get-Item -LiteralPath $fullPath).Length
        if ($length -gt $maxTrackedFileBytes) {
            throw "Tracked file exceeds the 2 MiB repository hygiene limit: $normalized ($length bytes). Use Release/Artifacts for large generated payloads or explicitly redesign the source policy before tracking them."
        }
    }
}

Write-Host ("[version] Bridge {0}, ABI {1} only, OCCT {2}, platform {3}, target {4}, consumers .NET 8-10, SDK baseline {5} ({6}), C# {7}; repository hygiene validated." -f
    $expectedVersion,
    $expectedAbiVersion,
    $expectedOcctVersion,
    $expectedPlatform,
    $expectedTargetFramework,
    $expectedSdkVersion,
    $expectedSdkRollForward,
    $expectedLanguageVersion) -ForegroundColor Green
