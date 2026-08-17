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
$DistRoot = Join-Path $RepoRoot "dist\win-x64"
$ContractPath = Join-Path $DistRoot "bridge-contract.json"
$ManifestPath = Join-Path $DistRoot "bridge-manifest.json"
$GlobalJsonPath = Join-Path $RepoRoot "global.json"
$ConsumerCheckPath = Join-Path $RepoRoot "tests\check-sdk-consumer.ps1"

$globalJson = Get-Content -LiteralPath $GlobalJsonPath -Raw -Encoding UTF8 | ConvertFrom-Json
$SdkVersion = [string]$globalJson.sdk.version
$SdkRollForward = [string]$globalJson.sdk.rollForward
try { $SdkBaseline = [version]$SdkVersion }
catch { throw "global.json contains an invalid .NET SDK baseline: $SdkVersion" }
if ($SdkBaseline.Major -ne 10 -or $SdkBaseline.Minor -ne 0) { throw "global.json must use a .NET 10 SDK baseline." }
if ($SdkRollForward -ne "latestFeature") { throw "global.json must use latestFeature SDK roll-forward." }
if ([bool]$globalJson.sdk.allowPrerelease) { throw "global.json must not allow prerelease SDKs." }

$script:DotNetCommand = $null
$script:ResolvedSdkVersion = $null
$script:BridgeVersion = ""
$script:CoreTargetFramework = "net10.0"
$script:DesktopTargetFramework = "net10.0-windows"

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
    $diagnostics = [System.Collections.Generic.List[string]]::new()
    foreach ($candidate in @(Get-DotNetCandidates)) {
        if (-not (Test-Path -LiteralPath $candidate -PathType Leaf)) {
            $diagnostics.Add("$candidate => not found")
            continue
        }
        $sdkLines = @(& $candidate --list-sdks 2>&1)
        if ($LASTEXITCODE -ne 0) {
            $diagnostics.Add("$candidate => --list-sdks failed")
            continue
        }
        $installed = @($sdkLines | ForEach-Object {
            $line = [string]$_
            if ($line -match '^\s*([^\s]+)\s+\[') { $Matches[1] }
        })
        $diagnostics.Add("$candidate => " + $(if ($installed.Count -eq 0) { "no SDKs" } else { $installed -join ", " }))
        Push-Location $RepoRoot
        try {
            $resolved = @(& $candidate --version 2>&1)
            $exitCode = $LASTEXITCODE
        }
        finally { Pop-Location }
        if ($exitCode -ne 0 -or $resolved.Count -ne 1) { continue }
        $versionText = ([string]$resolved[0]).Trim()
        try { $version = [version]$versionText }
        catch { continue }
        if ($version.Major -ne $SdkBaseline.Major -or
            $version.Minor -ne $SdkBaseline.Minor -or
            $version -lt $SdkBaseline) { continue }
        $script:DotNetCommand = [System.IO.Path]::GetFullPath($candidate)
        $script:ResolvedSdkVersion = $versionText
        return
    }
    $detail = if ($diagnostics.Count -eq 0) { "No dotnet host candidates were found." } else { $diagnostics -join [Environment]::NewLine }
    throw "OcctCSharpBridge Demo requires a stable .NET 10 SDK at or above baseline $SdkVersion using '$SdkRollForward' roll-forward.`nChecked dotnet hosts:`n$detail"
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
    foreach ($name in @(
        "OcctNative.dll",
        "OcctNet.dll",
        "OcctNet.WinForms.dll",
        "OcctNet.Wpf.dll",
        "OcctNet.Avalonia.dll",
        "bridge-contract.json",
        "bridge-manifest.json")) {
        Assert-Path (Join-Path $DistRoot $name)
    }

    $contract = Get-Content -LiteralPath $ContractPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $manifest = Get-Content -LiteralPath $ManifestPath -Raw -Encoding UTF8 | ConvertFrom-Json

    if ([int]$contract.schemaVersion -ne 3) { throw "Demo requires Bridge contract schema 3." }
    if ([int]$contract.nativeAbi.current -ne 5 -or [int]$contract.nativeAbi.minimumSupported -ne 5) { throw "Demo requires ABI 5 only." }
    if ([string]$contract.api.policy -ne "abi5-only") { throw "Demo requires api.policy=abi5-only." }
    if ([string]$contract.platform -ne "win-x64") { throw "Expected win-x64 Binary SDK; received $($contract.platform)." }
    if ([string]$contract.dotnet.targetFramework -ne "net10.0") { throw "Unsupported Core target framework: $($contract.dotnet.targetFramework)" }
    if ([string]$contract.dotnet.desktopTargetFramework -ne "net10.0-windows") { throw "Unsupported Desktop target framework: $($contract.dotnet.desktopTargetFramework)" }
    if ([string]$contract.dotnet.sdkVersion -ne $SdkVersion) { throw "Binary SDK baseline $($contract.dotnet.sdkVersion) does not match Demo baseline $SdkVersion." }
    if ([string]$contract.dotnet.sdkRollForward -ne $SdkRollForward) { throw "Binary SDK roll-forward policy does not match the Demo policy." }
    if ([string]$contract.dotnet.languageVersion -ne "14.0") { throw "Demo requires C# 14.0." }

    if ([int]$manifest.schemaVersion -ne 2) { throw "Demo requires Binary SDK manifest schema 2." }
    if ([int]$manifest.nativeAbi.current -ne 5 -or [int]$manifest.nativeAbi.minimumSupported -ne 5) { throw "Binary SDK manifest is not ABI5-only." }
    if ([string]$manifest.author -ne [string]$contract.author -or
        [string]$manifest.bridgeVersion -ne [string]$contract.bridgeVersion -or
        [string]$manifest.occtVersion -ne [string]$contract.occtVersion -or
        [string]$manifest.platform -ne [string]$contract.platform -or
        [string]$manifest.targetFramework -ne [string]$contract.dotnet.targetFramework -or
        [string]$manifest.sdkVersion -ne [string]$contract.dotnet.sdkVersion -or
        [string]$manifest.sdkRollForward -ne [string]$contract.dotnet.sdkRollForward -or
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
    $script:CoreTargetFramework = [string]$contract.dotnet.targetFramework
    $script:DesktopTargetFramework = [string]$contract.dotnet.desktopTargetFramework
    return $contract
}

function Get-OutputDirectory {
    param([Parameter(Mandatory = $true)][string]$Name)
    $definition = $Projects[$Name]
    $project = Join-Path $RepoRoot $definition.Project
    $framework = if ([string]$definition.Framework -eq "desktop") { $script:DesktopTargetFramework } else { $script:CoreTargetFramework }
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
Write-Host "Bridge SDK:    $DistRoot" -ForegroundColor DarkGray

if ($Target -eq "clean") {
    Clean-Outputs
    exit 0
}

Resolve-DotNetSdk
Write-Host "dotnet:        $script:DotNetCommand" -ForegroundColor DarkGray
Write-Host "SDK resolved:  $script:ResolvedSdkVersion" -ForegroundColor Green

Assert-Path $ConsumerCheckPath
Write-Host "[consumer] Running SDK consumer boundary check..." -ForegroundColor Cyan
& $ConsumerCheckPath -RepositoryRoot $RepoRoot
if (-not $?) { throw "SDK consumer boundary validation failed." }
$contract = Test-BinarySdk
Write-Host "Bridge:        $($contract.bridgeVersion), ABI 5 only, OCCT $($contract.occtVersion)" -ForegroundColor Green

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
