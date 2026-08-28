param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$contractPath = Join-Path $RepositoryRoot "bridge-contract.json"
if (-not (Test-Path $contractPath -PathType Leaf)) {
    throw "bridge-contract.json was not found."
}

$contract = Get-Content $contractPath -Raw -Encoding UTF8 | ConvertFrom-Json
if ([string]::IsNullOrWhiteSpace([string]$contract.bridgeVersion)) {
    throw "Bridge version is missing."
}
if ([int]$contract.nativeAbi.current -ne 5 -or [int]$contract.nativeAbi.minimumSupported -ne 5) {
    throw "OcctCSharpBridge currently supports ABI 5 only."
}
if ([string]$contract.occtVersion -ne "7.9.0") {
    throw "OcctCSharpBridge currently targets OCCT 7.9.0."
}
if ([string]$contract.dotnet.targetFramework -ne "net10.0" -or
    [string]$contract.dotnet.desktopTargetFramework -ne "net10.0-windows") {
    throw "Binary SDK default target frameworks must be net10.0 / net10.0-windows."
}

$engine = Get-Content (Join-Path $RepositoryRoot "src\OcctNative\core\OcctEngine.cpp") -Raw -Encoding UTF8
$bridgeInfo = Get-Content (Join-Path $RepositoryRoot "src\OcctNet\Core\OcctBridgeInfo.cs") -Raw -Encoding UTF8
$props = [xml](Get-Content (Join-Path $RepositoryRoot "Directory.Build.props") -Raw -Encoding UTF8)
$global = Get-Content (Join-Path $RepositoryRoot "global.json") -Raw -Encoding UTF8 | ConvertFrom-Json

$version = [string]$contract.bridgeVersion
if (-not $engine.Contains("return `"$version`";")) {
    throw "Native bridge version differs from bridge-contract.json."
}
if (-not $engine.Contains("return 5;")) {
    throw "Native ABI version differs from bridge-contract.json."
}
if (-not $bridgeInfo.Contains("ExpectedAbiVersion = 5") -or
    -not $bridgeInfo.Contains("ManagedVersion = `"$version`"")) {
    throw "Managed bridge version/ABI differs from bridge-contract.json."
}

$projectVersion = [string]$props.Project.PropertyGroup.Version
if ($projectVersion -ne $version) {
    throw "Directory.Build.props version differs from bridge-contract.json."
}
if ([string]$global.sdk.version -ne [string]$contract.dotnet.sdkVersion -or
    [string]$global.sdk.rollForward -ne [string]$contract.dotnet.sdkRollForward) {
    throw "global.json SDK settings differ from bridge-contract.json."
}

Write-Host "[version] Bridge $version, ABI 5, OCCT $($contract.occtVersion), .NET 10 defaults." -ForegroundColor Green
