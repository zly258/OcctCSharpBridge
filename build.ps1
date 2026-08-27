param(
    [Parameter(Position = 0)]
    [ValidateSet("validate", "common", "winform", "wpf", "avalonia", "clean", "all")]
    [string]$Target = "all",

    [Parameter(Position = 1)]
    [ValidateSet("Debug", "Release", "RelWithDebInfo")]
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$utf8 = [System.Text.UTF8Encoding]::new($false)
[Console]::InputEncoding = $utf8
[Console]::OutputEncoding = $utf8
$OutputEncoding = $utf8
$env:DOTNET_CLI_UI_LANGUAGE = "en-US"
$env:VSLANG = "1033"

$RunningOnWindows = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)
if (-not $RunningOnWindows) { throw "build.ps1 supports Windows x64 only. Use ./build.sh on Linux." }

$Target = $Target.ToLowerInvariant()
$RepoRoot = Split-Path -Parent $PSCommandPath
$DistRoot = Join-Path $RepoRoot "external\OcctCSharpBridge\win-x64"
$ContractPath = Join-Path $DistRoot "bridge-contract.json"
$ManifestPath = Join-Path $DistRoot "bridge-manifest.json"
$GlobalJsonPath = Join-Path $RepoRoot "global.json"
$ConsumerCheckPath = Join-Path $RepoRoot "tests\check-sdk-consumer.ps1"
$NoReflectionCheckPath = Join-Path $RepoRoot "tests\check-no-reflection-dispatch.ps1"
$SdkFileNames = @(
    "OcctNative.dll",
    "OcctNet.dll",
    "OcctNet.WinForms.dll",
    "OcctNet.Wpf.dll",
    "OcctNet.Avalonia.dll",
    "bridge-contract.json",
    "bridge-manifest.json"
)

$globalJson = Get-Content -LiteralPath $GlobalJsonPath -Raw -Encoding UTF8 | ConvertFrom-Json
$SdkVersion = [string]$globalJson.sdk.version
$SdkRollForward = [string]$globalJson.sdk.rollForward
if ($SdkRollForward -ne "latestFeature") { throw "global.json must use rollForward=latestFeature for the .NET 10 SDK baseline." }
if ([bool]$globalJson.sdk.allowPrerelease) { throw "global.json must not allow prerelease SDKs." }

$script:DotNetCommand = $null
$script:ResolvedSdkVersion = $null
$script:BridgeVersion = ""
$script:BridgeCoreTargetFramework = ""
$script:BridgeDesktopTargetFramework = ""
$script:DemoCoreTargetFramework = "net10.0"
$script:DemoDesktopTargetFramework = "net10.0-windows"

$Projects = [ordered]@{
    common = @{
        DisplayName = "OcctDemo.Common"
        Project = "src\OcctDemo.Common\OcctDemo.Common.csproj"
        Executable = $null
        Framework = "core"
    }
    winform = @{
        DisplayName = "OcctDemo.WinForms"
        Project = "src\OcctDemo.WinForms\OcctDemo.WinForms.csproj"
        Executable = "CAD-Winform.exe"
        Framework = "desktop"
    }
    wpf = @{
        DisplayName = "OcctDemo.Wpf"
        Project = "src\OcctDemo.Wpf\OcctDemo.Wpf.csproj"
        Executable = "CAD-WPF.exe"
        Framework = "desktop"
    }
    avalonia = @{
        DisplayName = "OcctDemo.Avalonia"
        Project = "src\OcctDemo.Avalonia\OcctDemo.Avalonia.csproj"
        Executable = "CAD-Avalonia.exe"
        Framework = "core"
    }
}

function Assert-Path {
    param([Parameter(Mandatory = $true)][string]$Path)
    if (-not (Test-Path -LiteralPath $Path)) { throw "Required path was not found: $Path" }
}

function Get-DotNetCandidates {
    $result = [System.Collections.Generic.List[string]]::new()
    $seen = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($root in @($env:DOTNET_ROOT, $env:ProgramW6432, $env:ProgramFiles)) {
        if ([string]::IsNullOrWhiteSpace($root)) { continue }
        $candidate = if ((Split-Path -Leaf $root) -ieq "dotnet") { Join-Path $root "dotnet.exe" } else { Join-Path $root "dotnet\dotnet.exe" }
        if ($seen.Add($candidate)) { $result.Add($candidate) }
    }
    $command = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($null -ne $command -and -not [string]::IsNullOrWhiteSpace([string]$command.Source)) {
        $candidate = [string]$command.Source
        if ($seen.Add($candidate)) { $result.Add($candidate) }
    }
    return $result
}

function Resolve-DotNetSdk {
    if (-not [string]::IsNullOrWhiteSpace($script:DotNetCommand)) { return }

    try { $minimumSdkVersion = [version]$SdkVersion }
    catch { throw "global.json contains an invalid SDK baseline: $SdkVersion" }

    $diagnostics = [System.Collections.Generic.List[string]]::new()
    foreach ($candidate in @(Get-DotNetCandidates)) {
        if (-not (Test-Path -LiteralPath $candidate -PathType Leaf)) {
            $diagnostics.Add("$candidate => not found")
            continue
        }
        Push-Location $RepoRoot
        try {
            $resolved = @(& $candidate --version 2>&1)
            $exitCode = $LASTEXITCODE
        }
        finally { Pop-Location }
        if ($exitCode -ne 0 -or $resolved.Count -ne 1) {
            $diagnostics.Add("$candidate => SDK resolution failed")
            continue
        }
        $version = ([string]$resolved[0]).Trim()
        try { $resolvedSdkVersion = [version]$version }
        catch {
            $diagnostics.Add("$candidate => $version (not a stable SDK version)")
            continue
        }
        $diagnostics.Add("$candidate => $version")
        if ($resolvedSdkVersion.Major -ne $minimumSdkVersion.Major -or
            $resolvedSdkVersion.Minor -ne $minimumSdkVersion.Minor -or
            $resolvedSdkVersion -lt $minimumSdkVersion) {
            continue
        }
        $script:DotNetCommand = [System.IO.Path]::GetFullPath($candidate)
        $script:ResolvedSdkVersion = $version
        return
    }
    $detail = if ($diagnostics.Count -eq 0) { "No dotnet host candidates were found." } else { $diagnostics -join [Environment]::NewLine }
    throw "OcctCSharpBridge Demo requires a stable .NET 10 SDK compatible with baseline $SdkVersion and roll-forward '$SdkRollForward'.`nChecked dotnet hosts:`n$detail"
}

function Invoke-DotNetChecked {
    param(
        [Parameter(Mandatory = $true)][object[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$ErrorMessage
    )
    Resolve-DotNetSdk
    Push-Location $RepoRoot
    try {
        & $script:DotNetCommand @Arguments
        if ($LASTEXITCODE -ne 0) { throw $ErrorMessage }
    }
    finally { Pop-Location }
}

function Test-BinarySdk {
    foreach ($name in $SdkFileNames) { Assert-Path (Join-Path $DistRoot $name) }

    $unexpectedEntries = @(
        Get-ChildItem -LiteralPath $DistRoot -Force | Where-Object { $_.Name -notin $SdkFileNames }
    )
    if ($unexpectedEntries.Count -gt 0) {
        throw "Demo external/OcctCSharpBridge/win-x64 contains files or directories outside the validated Binary SDK payload: $((@($unexpectedEntries.Name | Sort-Object)) -join ', '). Run .\sync.ps1 to refresh it."
    }

    $contract = Get-Content -LiteralPath $ContractPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $manifest = Get-Content -LiteralPath $ManifestPath -Raw -Encoding UTF8 | ConvertFrom-Json

    if ([int]$contract.schemaVersion -ne 3) { throw "Demo requires Bridge contract schema 3." }
    if ([int]$contract.nativeAbi.current -ne 5 -or [int]$contract.nativeAbi.minimumSupported -ne 5) { throw "Demo requires ABI 5 only." }
    if ([string]$contract.api.policy -ne "abi5-only") { throw "Demo requires api.policy=abi5-only." }
    if ([string]$contract.platform -ne "win-x64") { throw "Expected win-x64 Binary SDK; received $($contract.platform)." }

    $bridgeCoreFramework = [string]$contract.dotnet.targetFramework
    $bridgeDesktopFramework = [string]$contract.dotnet.desktopTargetFramework
    $supportedCoreFrameworks = @($contract.dotnet.supportedConsumerFrameworks | ForEach-Object { [string]$_ })
    $supportedDesktopFrameworks = @($contract.dotnet.supportedDesktopConsumerFrameworks | ForEach-Object { [string]$_ })
    if ($bridgeCoreFramework -notin $supportedCoreFrameworks) {
        throw "Bridge Core target framework '$bridgeCoreFramework' is not declared in supportedConsumerFrameworks."
    }
    if ($bridgeDesktopFramework -notin $supportedDesktopFrameworks) {
        throw "Bridge Desktop target framework '$bridgeDesktopFramework' is not declared in supportedDesktopConsumerFrameworks."
    }
    if ($script:DemoCoreTargetFramework -notin $supportedCoreFrameworks) {
        throw "The synchronized Bridge SDK does not support Demo target $script:DemoCoreTargetFramework. Supported: $($supportedCoreFrameworks -join ', ')."
    }
    if ($script:DemoDesktopTargetFramework -notin $supportedDesktopFrameworks) {
        throw "The synchronized Bridge SDK does not support Demo desktop target $script:DemoDesktopTargetFramework. Supported: $($supportedDesktopFrameworks -join ', ')."
    }

    $binarySdkBaseline = [string]$contract.dotnet.sdkVersion
    if ([string]::IsNullOrWhiteSpace($binarySdkBaseline)) { throw "Binary SDK .NET SDK baseline is missing." }
    try { [void][version]$binarySdkBaseline }
    catch { throw "Binary SDK contains an invalid SDK baseline: $binarySdkBaseline" }
    if ([string]$contract.dotnet.languageVersion -ne "14.0") { throw "Demo requires a Bridge built with C# 14.0 contract metadata." }

    if ([int]$manifest.schemaVersion -ne 2) { throw "Demo requires Binary SDK manifest schema 2." }
    if ([int]$manifest.nativeAbi.current -ne 5 -or [int]$manifest.nativeAbi.minimumSupported -ne 5) { throw "Binary SDK manifest is not ABI5-only." }
    if ([string]$manifest.author -ne [string]$contract.author -or
        [string]$manifest.bridgeVersion -ne [string]$contract.bridgeVersion -or
        [string]$manifest.occtVersion -ne [string]$contract.occtVersion -or
        [string]$manifest.platform -ne [string]$contract.platform -or
        [string]$manifest.targetFramework -ne $bridgeCoreFramework -or
        [string]$manifest.sdkVersion -ne $binarySdkBaseline -or
        [string]$manifest.languageVersion -ne [string]$contract.dotnet.languageVersion -or
        [string]$manifest.configuration -ne "Release") {
        throw "Binary SDK manifest does not match bridge-contract.json."
    }
    if ([string]::IsNullOrWhiteSpace([string]$manifest.sourceCommit)) { throw "Binary SDK manifest sourceCommit is missing." }

    $requiredHashes = @("OcctNative.dll", "OcctNet.dll", "OcctNet.WinForms.dll", "OcctNet.Wpf.dll", "OcctNet.Avalonia.dll", "bridge-contract.json")
    $entries = @($manifest.files)
    $names = @($entries | ForEach-Object { [string]$_.name })
    if ($names.Count -ne $requiredHashes.Count) { throw "Binary SDK manifest contains an unexpected number of files." }
    foreach ($name in $requiredHashes) {
        if ($name -notin $names) { throw "Binary SDK manifest does not hash required file: $name" }
    }
    foreach ($entry in $entries) {
        $path = Join-Path $DistRoot ([string]$entry.name)
        Assert-Path $path
        $actual = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
        if ($actual -ne ([string]$entry.sha256).ToLowerInvariant()) { throw "Binary SDK hash mismatch: $($entry.name)" }
    }

    $script:BridgeVersion = [string]$contract.bridgeVersion
    $script:BridgeCoreTargetFramework = $bridgeCoreFramework
    $script:BridgeDesktopTargetFramework = $bridgeDesktopFramework
    return $contract
}

function Get-OutputDirectory {
    param([Parameter(Mandatory = $true)][string]$Name)
    $definition = $Projects[$Name]
    $project = Join-Path $RepoRoot $definition.Project
    $framework = if ([string]$definition.Framework -eq "desktop") { $script:DemoDesktopTargetFramework } else { $script:DemoCoreTargetFramework }
    return Join-Path (Split-Path -Parent $project) "bin\x64\$Configuration\$framework"
}

function Build-Project {
    param([Parameter(Mandatory = $true)][string]$Name)
    $definition = $Projects[$Name]
    if ($null -eq $definition) { throw "Unknown project key: $Name" }
    $project = Join-Path $RepoRoot $definition.Project
    Assert-Path $project
    Write-Host "[$Name] Building $($definition.DisplayName) / $Configuration / Bridge $script:BridgeVersion..." -ForegroundColor Cyan
    Invoke-DotNetChecked @("build", $project, "-c", $Configuration, "-p:Platform=x64", "-p:Version=$script:BridgeVersion", "--nologo") "$($definition.DisplayName) build failed."
    if ($null -ne $definition.Executable) {
        $output = Get-OutputDirectory $Name
        Assert-Path (Join-Path $output $definition.Executable)
        Assert-Path (Join-Path $output "OcctNative.dll")
    }
}

function Clean-Outputs {
    Remove-Item (Join-Path $RepoRoot "artifacts") -Recurse -Force -ErrorAction SilentlyContinue
    foreach ($definition in $Projects.Values) {
        $directory = Split-Path -Parent (Join-Path $RepoRoot $definition.Project)
        Remove-Item (Join-Path $directory "bin") -Recurse -Force -ErrorAction SilentlyContinue
        Remove-Item (Join-Path $directory "obj") -Recurse -Force -ErrorAction SilentlyContinue
    }
    Write-Host "Generated demo outputs removed." -ForegroundColor Green
}

Write-Host "Target:        $Target"
Write-Host "Configuration: $Configuration"
Write-Host "SDK contract:  $SdkVersion + $SdkRollForward" -ForegroundColor DarkGray
Write-Host "Demo TFM:      $script:DemoCoreTargetFramework / $script:DemoDesktopTargetFramework" -ForegroundColor DarkGray
Write-Host "Bridge SDK:    $DistRoot" -ForegroundColor DarkGray

if ($Target -eq "clean") {
    Clean-Outputs
    Write-Host "Build completed." -ForegroundColor Green
    exit 0
}

Assert-Path $NoReflectionCheckPath
& $NoReflectionCheckPath -RepositoryRoot $RepoRoot
if (-not $?) { throw "No-reflection dispatch validation failed." }

Resolve-DotNetSdk
Write-Host "dotnet:        $script:DotNetCommand" -ForegroundColor DarkGray
Write-Host "SDK resolved:  $script:ResolvedSdkVersion" -ForegroundColor Green

Assert-Path $ConsumerCheckPath
Write-Host "[consumer] Running SDK consumer boundary check..." -ForegroundColor Cyan
& $ConsumerCheckPath -RepositoryRoot $RepoRoot
if (-not $?) { throw "SDK consumer boundary validation failed." }
$contract = Test-BinarySdk
Write-Host "Bridge:        $($contract.bridgeVersion), ABI 5 only, OCCT $($contract.occtVersion), target $script:BridgeCoreTargetFramework" -ForegroundColor Green

switch ($Target) {
    "validate" { }
    "common" { Build-Project "common" }
    "winform" { Build-Project "winform" }
    "wpf" { Build-Project "wpf" }
    "avalonia" { Build-Project "avalonia" }
    "all" {
        Build-Project "common"
        Build-Project "winform"
        Build-Project "wpf"
        Build-Project "avalonia"
    }
}

Write-Host "Build completed." -ForegroundColor Green
