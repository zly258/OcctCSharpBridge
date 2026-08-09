param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$contractPath = Join-Path $RepositoryRoot "bridge-contract.json"
if (-not (Test-Path $contractPath -PathType Leaf)) { throw "Bridge contract file was not found: bridge-contract.json" }
try { $contract = Get-Content $contractPath -Raw -Encoding UTF8 | ConvertFrom-Json }
catch { throw "bridge-contract.json is not valid JSON: $($_.Exception.Message)" }

$expectedVersion = [string]$contract.bridgeVersion
$expectedAbiVersion = [int]$contract.nativeAbiVersion
$expectedOcctVersion = [string]$contract.occtVersion
$expectedCmakeVersion = [string]$contract.cmakeMinimumVersion
$expectedContactEmail = [string]$contract.contactEmail
$expectedTargetFramework = [string]$contract.dotnet.targetFramework
$expectedSdkVersion = [string]$contract.dotnet.sdkVersion
$expectedLanguageVersion = [string]$contract.dotnet.languageVersion
$expectedNativeCount = [int]$contract.api.nativeExports
$expectedManagedCount = [int]$contract.api.managedPInvokes
$expectedPublicTypeCount = [int]$contract.api.publicNetTypes
$expectedCompatibilityTypeCount = [int]$contract.api.compatibilityPublicNetTypes

foreach ($entry in ([ordered]@{
    bridgeVersion = $expectedVersion
    occtVersion = $expectedOcctVersion
    cmakeMinimumVersion = $expectedCmakeVersion
    contactEmail = $expectedContactEmail
    targetFramework = $expectedTargetFramework
    sdkVersion = $expectedSdkVersion
    languageVersion = $expectedLanguageVersion
}).GetEnumerator()) {
    if ([string]::IsNullOrWhiteSpace([string]$entry.Value)) { throw "Bridge contract value is missing: $($entry.Key)" }
}

foreach ($entry in ([ordered]@{
    nativeAbiVersion = $expectedAbiVersion
    nativeExports = $expectedNativeCount
    managedPInvokes = $expectedManagedCount
    publicNetTypes = $expectedPublicTypeCount
    compatibilityPublicNetTypes = $expectedCompatibilityTypeCount
}).GetEnumerator()) {
    if ([int]$entry.Value -le 0) { throw "Bridge contract numeric value must be positive: $($entry.Key)" }
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
    "src/OcctNet/OcctNet.csproj" = @(
        "<TargetFramework>$expectedTargetFramework</TargetFramework>",
        "<Platforms>x64</Platforms>"
    )
    "src/OcctNet.WinForms/OcctNet.WinForms.csproj" = @(
        "<TargetFramework>$expectedTargetFramework</TargetFramework>",
        "<PlatformTarget>x64</PlatformTarget>"
    )
    "src/OcctNet.Wpf/OcctNet.Wpf.csproj" = @(
        "<TargetFramework>$expectedTargetFramework</TargetFramework>",
        "<PlatformTarget>x64</PlatformTarget>"
    )
    "src/OcctNet.Avalonia/OcctNet.Avalonia.csproj" = @(
        "<TargetFramework>$expectedTargetFramework</TargetFramework>",
        "<AssemblyName>OcctNet.Avalonia</AssemblyName>"
    )
    "tests/OcctNet.ManagedTests/OcctNet.ManagedTests.csproj" = @(
        "<TargetFramework>$expectedTargetFramework</TargetFramework>",
        "<PlatformTarget>x64</PlatformTarget>"
    )
    "tests/OcctNet.Smoke/OcctNet.Smoke.csproj" = @(
        "<TargetFramework>$expectedTargetFramework</TargetFramework>",
        "<PlatformTarget>x64</PlatformTarget>"
    )
    "README.md" = @($expectedVersion, $expectedOcctVersion, $expectedSdkVersion, $expectedContactEmail)
    "README.zh-CN.md" = @($expectedVersion, $expectedOcctVersion, $expectedSdkVersion, $expectedContactEmail)
    "docs/API_COVERAGE.md" = @(
        "Native exports", [string]$expectedNativeCount,
        "Managed P/Invoke declarations", [string]$expectedManagedCount,
        "Public .NET types", [string]$expectedPublicTypeCount,
        "Compatibility .NET types", [string]$expectedCompatibilityTypeCount,
        "Native bridge version", $expectedVersion
    )
    "docs/API_COVERAGE.zh-CN.md" = @(
        "Native exports", [string]$expectedNativeCount,
        "Managed P/Invoke declarations", [string]$expectedManagedCount,
        "Public .NET types", [string]$expectedPublicTypeCount,
        "Compatibility .NET types", [string]$expectedCompatibilityTypeCount,
        "原生桥接版本", $expectedVersion
    )
    "src/OcctNative/CMakeLists.txt" = @(
        "cmake_minimum_required(VERSION $expectedCmakeVersion)",
        "bridge-contract.json",
        "string(JSON BRIDGE_VERSION",
        "string(JSON BRIDGE_ABI_VERSION",
        "string(JSON REQUIRED_OCCT_VERSION",
        'OcctCSharpBridge requires exactly OCCT ${REQUIRED_OCCT_VERSION}'
    )
    "global.json" = @($expectedSdkVersion)
    "Directory.Build.props" = @("<LangVersion>$expectedLanguageVersion</LangVersion>")
}

foreach ($contractEntry in $contracts.GetEnumerator()) {
    $path = Join-Path $RepositoryRoot $contractEntry.Key
    if (-not (Test-Path $path -PathType Leaf)) { throw "Version contract file was not found: $($contractEntry.Key)" }
    $content = [System.IO.File]::ReadAllText($path)
    foreach ($token in $contractEntry.Value) {
        if (-not $content.Contains([string]$token)) { throw "Version contract is stale in $($contractEntry.Key): $token" }
    }
}

foreach ($relativePath in @(
    "src/OcctDemo.Common/OcctDemo.Common.csproj",
    "src/OcctDemo.WinForms/OcctDemo.WinForms.csproj",
    "src/OcctDemo.Wpf/OcctDemo.Wpf.csproj",
    "src/OcctDemo.Avalonia/OcctDemo.Avalonia.csproj"
)) {
    $path = Join-Path $RepositoryRoot $relativePath
    if (-not (Test-Path $path -PathType Leaf)) { continue }
    $text = [System.IO.File]::ReadAllText($path)
    if (-not $text.Contains("<TargetFramework>$expectedTargetFramework</TargetFramework>")) {
        throw "Demo project target framework does not match bridge-contract.json: $relativePath"
    }
}

foreach ($relativePath in @(
    "src/OcctDemo.Common/DemoLocalization.cs",
    "src/CadCommon/CadLocalization.cs"
)) {
    $path = Join-Path $RepositoryRoot $relativePath
    if (-not (Test-Path $path -PathType Leaf)) { continue }
    $localization = [System.IO.File]::ReadAllText($path)
    if (-not $localization.Contains($expectedContactEmail)) {
        throw "Software About contact does not match bridge-contract.json: $relativePath"
    }
}

$publishPath = Join-Path $RepositoryRoot "publish.ps1"
if (Test-Path $publishPath -PathType Leaf) {
    $publishText = [System.IO.File]::ReadAllText($publishPath)
    if (-not $publishText.Contains($expectedContactEmail)) { throw "Published package contact does not match bridge-contract.json." }
}

$legacyContact = "zhangly1403" + "@" + "qq.com"
$contactScanFiles = [System.Collections.Generic.List[string]]::new()
foreach ($fileName in @("README.md", "README.zh-CN.md", "publish.ps1")) {
    $candidate = Join-Path $RepositoryRoot $fileName
    if (Test-Path $candidate -PathType Leaf) { $contactScanFiles.Add($candidate) }
}
foreach ($rootName in @("src", "docs")) {
    $root = Join-Path $RepositoryRoot $rootName
    if (-not (Test-Path $root -PathType Container)) { continue }
    Get-ChildItem $root -Recurse -File | Where-Object {
        $_.Extension -in @(".cs", ".cpp", ".h", ".hxx", ".md", ".txt", ".json", ".xml", ".xaml", ".csproj")
    } | ForEach-Object { $contactScanFiles.Add($_.FullName) }
}
foreach ($path in $contactScanFiles) {
    if ([System.IO.File]::ReadAllText($path).Contains($legacyContact)) { throw "Legacy contact email remains in $path." }
}

$machineSpecificFiles = @(
    (Join-Path $RepositoryRoot "build.ps1"),
    (Join-Path $RepositoryRoot "src\OcctNative\CMakeLists.txt")
) + @(Get-ChildItem (Join-Path $RepositoryRoot "src\OcctNet") -Filter "*.cs" -File | Select-Object -ExpandProperty FullName)

$allowedDefaultOcctPathPattern = '(?i)^D:[\\/]tools[\\/]occt-vc144-64$'
foreach ($path in $machineSpecificFiles) {
    $content = [System.IO.File]::ReadAllText($path)
    $matches = [regex]::Matches($content, '(?i)[A-Z]:[\\/]tools[\\/]occt[^"''\r\n ]*')
    foreach ($match in $matches) {
        if ($match.Value -notmatch $allowedDefaultOcctPathPattern) { throw "An unsupported machine-specific OCCT path remains in ${path}: $($match.Value)" }
        $isBuildScript = $path.EndsWith('build.ps1', [System.StringComparison]::OrdinalIgnoreCase)
        $isNativeCMake = $path.EndsWith('src\OcctNative\CMakeLists.txt', [System.StringComparison]::OrdinalIgnoreCase)
        if (-not ($isBuildScript -or $isNativeCMake)) { throw "The conventional OCCT default path may only appear in build.ps1 or native CMakeLists.txt; found in $path." }
    }
}

Write-Host ("[version] Bridge {0}, ABI {1}, OCCT {2}, build SDK {3}, target {4}, C# {5}, API {6}/{7}, primary types {8}, compatibility types {9}, contact {10}." -f
    $expectedVersion,
    $expectedAbiVersion,
    $expectedOcctVersion,
    $expectedSdkVersion,
    $expectedTargetFramework,
    $expectedLanguageVersion,
    $expectedNativeCount,
    $expectedManagedCount,
    $expectedPublicTypeCount,
    $expectedCompatibilityTypeCount,
    $expectedContactEmail) -ForegroundColor Green
