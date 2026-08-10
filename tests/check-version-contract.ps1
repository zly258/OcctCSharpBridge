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
    if ([string]::IsNullOrWhiteSpace([string]$entry.Value)) { throw "Bridge contract value is missing: $($entry.Key)" }
}

foreach ($entry in ([ordered]@{
    nativeAbiVersion = $expectedAbiVersion
    nativeExports = $expectedNativeCount
    managedPInvokes = $expectedManagedCount
    publicNetTypes = $expectedPublicTypeCount
    viewer = $expectedViewerCount
    modeling = $expectedModelingCount
}).GetEnumerator()) {
    if ([int]$entry.Value -le 0) { throw "Bridge contract numeric value must be positive: $($entry.Key)" }
}

if (($expectedViewerCount + $expectedModelingCount) -ne $expectedNativeCount) {
    throw "Viewer + Modeling API counts must equal nativeExports."
}
if ($expectedNativeCount -ne $expectedManagedCount) {
    throw "Native export and managed P/Invoke counts must stay equal."
}

$contracts = [ordered]@{
    "src/OcctNative/OcctEngine.cpp" = @("occt_bridge_version()", "return `"$expectedVersion`";", "occt_bridge_abi_version()", "return $expectedAbiVersion;")
    "src/OcctNet/OcctBridgeInfo.cs" = @("ExpectedAbiVersion = $expectedAbiVersion", "ManagedVersion = `"$expectedVersion`"")
    "src/OcctNet/OcctNet.csproj" = @("<TargetFramework>$expectedTargetFramework</TargetFramework>", "<PlatformTarget>x64</PlatformTarget>")
    "src/OcctNet.WinForms/OcctNet.WinForms.csproj" = @("<TargetFramework>$expectedTargetFramework</TargetFramework>", "<PlatformTarget>x64</PlatformTarget>")
    "src/OcctNet.Wpf/OcctNet.Wpf.csproj" = @("<TargetFramework>$expectedTargetFramework</TargetFramework>", "<PlatformTarget>x64</PlatformTarget>")
    "src/OcctNet.Avalonia/OcctNet.Avalonia.csproj" = @("<TargetFramework>$expectedTargetFramework</TargetFramework>", "<PlatformTarget>x64</PlatformTarget>")
    "tests/OcctNet.ManagedTests/OcctNet.ManagedTests.csproj" = @("<TargetFramework>$expectedTargetFramework</TargetFramework>", "<PlatformTarget>x64</PlatformTarget>")
    "tests/OcctNet.Smoke/OcctNet.Smoke.csproj" = @("<TargetFramework>$expectedTargetFramework</TargetFramework>", "<PlatformTarget>x64</PlatformTarget>")
    "README.md" = @($expectedVersion, $expectedOcctVersion, $expectedSdkVersion, "## Installation", "## Usage Example", "## Contributing", "## License")
    "README.zh-CN.md" = @($expectedVersion, $expectedOcctVersion, $expectedSdkVersion, "## 安装指南", "## 使用示例", "## 贡献指南", "## 许可证")
    "docs/API_COVERAGE.md" = @("Native exports: ``$expectedNativeCount``", "Managed P/Invoke declarations: ``$expectedManagedCount``", "Public .NET types: ``$expectedPublicTypeCount``", "Viewer API: ``$expectedViewerCount``", "Modeling API: ``$expectedModelingCount``")
    "docs/API_COVERAGE.zh-CN.md" = @("Native exports：``$expectedNativeCount``", "Managed P/Invoke declarations：``$expectedManagedCount``", "Public .NET types：``$expectedPublicTypeCount``", "Viewer API：``$expectedViewerCount``", "Modeling API：``$expectedModelingCount``")
    "src/OcctNative/CMakeLists.txt" = @("cmake_minimum_required(VERSION $expectedCmakeVersion)", "bridge-contract.json", "string(JSON BRIDGE_VERSION", "string(JSON BRIDGE_ABI_VERSION", "string(JSON REQUIRED_OCCT_VERSION")
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

$contractText = [System.IO.File]::ReadAllText($contractPath)
if ($contractText.Contains("compatibilityPublicNetTypes")) {
    throw "Compatibility API accounting must not be reintroduced into the new library contract."
}

$buildScriptText = [System.IO.File]::ReadAllText((Join-Path $RepositoryRoot "build.ps1"))
if (-not $buildScriptText.Contains('D:\tools\occt-vc144-64')) { throw "build.ps1 must preserve the conventional OCCT root." }
if (-not $buildScriptText.Contains('-p:Version=$BridgeVersion')) { throw "Managed builds must use bridge-contract.json as the package version source." }

$nativeCmakeText = [System.IO.File]::ReadAllText((Join-Path $RepositoryRoot "src\OcctNative\CMakeLists.txt"))
if ($nativeCmakeText -notmatch 'OcctCSharpBridge requires exactly OCCT \$\{REQUIRED_OCCT_VERSION\}') {
    throw "Native CMake must enforce the exact contracted OCCT version."
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
