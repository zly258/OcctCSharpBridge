param(
    [Parameter(Position = 0)]
    [ValidateSet("validate", "common", "winform", "wpf", "clean", "all")]
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

if (Test-Path "$env:SystemRoot\System32\chcp.com") {
    & "$env:SystemRoot\System32\chcp.com" 65001 | Out-Null
}

$Target = $Target.ToLowerInvariant()
$RepoRoot = Split-Path -Parent $PSCommandPath
$DistRoot = Join-Path $RepoRoot "dist\win-x64"
$ContractPath = Join-Path $DistRoot "bridge-contract.json"
$ManifestPath = Join-Path $DistRoot "bridge-manifest.json"
$PropsPath = Join-Path $RepoRoot "Directory.Build.props"
$GlobalJsonPath = Join-Path $RepoRoot "global.json"
$ConsumerCheckPath = Join-Path $RepoRoot "tests\check-sdk-consumer.ps1"
$script:TargetFramework = "net10.0-windows"
$script:BridgeVersion = ""

[xml]$props = Get-Content -LiteralPath $PropsPath -Raw
$propertyGroup = $props.Project.PropertyGroup
$globalJson = Get-Content -LiteralPath $GlobalJsonPath -Raw | ConvertFrom-Json
$ExpectedAuthor = [string]$propertyGroup.Authors
$ExpectedLanguageVersion = [string]$propertyGroup.LangVersion
$ExpectedSdkVersion = [string]$globalJson.sdk.version
try { $ExpectedSdk = [version]$ExpectedSdkVersion }
catch { throw "global.json contains an invalid .NET SDK version: '$ExpectedSdkVersion'." }
$ExpectedSdkMajor = $ExpectedSdk.Major
if ([string]$globalJson.sdk.rollForward -ne "latestMinor") {
    throw "global.json must use rollForward 'latestMinor' so the demo accepts any stable .NET $ExpectedSdkMajor.x SDK."
}
if ([bool]$globalJson.sdk.allowPrerelease) { throw "global.json must not allow prerelease SDKs." }

$Projects = [ordered]@{
    common = @{
        DisplayName = "OcctDemo.Common"
        Project = "src\OcctDemo.Common\OcctDemo.Common.csproj"
        Executable = $null
    }
    winform = @{
        DisplayName = "OcctDemo.WinForms"
        Project = "src\OcctDemo.WinForms\OcctDemo.WinForms.csproj"
        Executable = "CAD-Winform.exe"
    }
    wpf = @{
        DisplayName = "OcctDemo.Wpf"
        Project = "src\OcctDemo.Wpf\OcctDemo.Wpf.csproj"
        Executable = "CAD-WPF.exe"
    }
}

function Assert-Path {
    param([Parameter(Mandatory = $true)][string]$Path)
    if (-not (Test-Path -LiteralPath $Path)) {
        throw "Required path was not found: $Path"
    }
}

function Assert-Command {
    param([Parameter(Mandatory = $true)][string]$Name)
    if ($null -eq (Get-Command -Name $Name -ErrorAction SilentlyContinue)) {
        throw "$Name was not found in PATH."
    }
}

function Invoke-Checked {
    param(
        [Parameter(Mandatory = $true)][string]$Command,
        [Parameter(Mandatory = $true)][object[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$ErrorMessage
    )

    & $Command @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw $ErrorMessage
    }
}

function Get-OutputDirectory {
    param([Parameter(Mandatory = $true)][string]$Name)

    $definition = $Projects[$Name]
    if ($null -eq $definition) {
        throw "Unknown project key: $Name"
    }
    $project = Join-Path $RepoRoot $definition.Project
    return Join-Path (Split-Path -Parent $project) "bin\x64\$Configuration\$script:TargetFramework"
}

function Test-BinarySdk {
    if (-not (Test-Path -LiteralPath $DistRoot -PathType Container) -or
        -not (Test-Path -LiteralPath $ContractPath -PathType Leaf) -or
        -not (Test-Path -LiteralPath $ManifestPath -PathType Leaf)) {
        throw "Bridge Binary SDK has not been published to demo/dist/win-x64. From the main branch run: .\publish.ps1 -OcctRoot <OCCT_ROOT>"
    }

    $required = @(
        "OcctNative.dll",
        "OcctNet.dll",
        "OcctNet.WinForms.dll",
        "OcctNet.Wpf.dll",
        "bridge-contract.json",
        "OcctNet.Avalonia.dll",
        "bridge-manifest.json"
    )
    foreach ($name in $required) {
        Assert-Path (Join-Path $DistRoot $name)
    }

    $contract = Get-Content -LiteralPath $ContractPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $manifest = Get-Content -LiteralPath $ManifestPath -Raw -Encoding UTF8 | ConvertFrom-Json

    if ([string]$contract.author -ne $ExpectedAuthor) {
        throw "Unsupported Bridge author metadata: $($contract.author). Demo expects $ExpectedAuthor."
    }
    if ([string]::IsNullOrWhiteSpace([string]$contract.bridgeVersion)) {
        throw "Bridge version is missing from bridge-contract.json."
    }
    if ([int]$contract.schemaVersion -ne 2 -or [int]$contract.nativeAbi.current -ne 5) {
        throw "Demo requires the Bridge schema 2 contract with current ABI 5."
    }
    if ([string]::IsNullOrWhiteSpace([string]$contract.occtVersion)) {
        throw "OCCT version is missing from bridge-contract.json."
    }
    if ([string]$contract.platform -ne "windows-x64") {
        throw "Unsupported Bridge platform: $($contract.platform)"
    }
    if ([string]$contract.dotnet.targetFramework -ne "net10.0") {
        throw "Unsupported Bridge core target framework: $($contract.dotnet.targetFramework)"
    }
    if ([string]$contract.dotnet.desktopTargetFramework -ne "net10.0-windows") {
        throw "Unsupported Bridge desktop target framework: $($contract.dotnet.desktopTargetFramework)"
    }
    $contractSdkText = [string]$contract.dotnet.sdkVersion
    try { $contractSdk = [version]$contractSdkText }
    catch { throw "Bridge contract contains an invalid .NET SDK version: '$contractSdkText'." }
    if ($contractSdk.Major -ne $ExpectedSdkMajor) {
        throw "Unsupported Bridge .NET SDK major: $contractSdkText. Demo requires .NET $ExpectedSdkMajor.x."
    }
    if ([string]$contract.dotnet.languageVersion -ne $ExpectedLanguageVersion) {
        throw "Unsupported Bridge C# language version: $($contract.dotnet.languageVersion). Demo expects $ExpectedLanguageVersion."
    }
    $script:BridgeVersion = [string]$contract.bridgeVersion
    $script:TargetFramework = [string]$contract.dotnet.desktopTargetFramework

    if ([int]$manifest.schemaVersion -ne 1) {
        throw "Unsupported Bridge binary manifest schema: $($manifest.schemaVersion)"
    }
    if ([string]$manifest.author -ne [string]$contract.author -or
        [string]$manifest.bridgeVersion -ne [string]$contract.bridgeVersion -or
        [int]$manifest.nativeAbiVersion -ne [int]$contract.nativeAbi.current -or
        [string]$manifest.occtVersion -ne [string]$contract.occtVersion -or
        [string]$manifest.platform -ne [string]$contract.platform -or
        [string]$manifest.targetFramework -ne [string]$contract.dotnet.targetFramework -or
        [string]$manifest.sdkVersion -ne [string]$contract.dotnet.sdkVersion -or
        [string]$manifest.languageVersion -ne [string]$contract.dotnet.languageVersion -or
        [string]$manifest.configuration -ne "Release") {
        throw "Bridge binary manifest does not match bridge-contract.json or is not a Release SDK."
    }
    if ([string]::IsNullOrWhiteSpace([string]$manifest.sourceCommit)) {
        throw "Bridge binary manifest sourceCommit is missing."
    }

    $expectedHashedFiles = @(
        "OcctNative.dll",
        "OcctNet.dll",
        "OcctNet.WinForms.dll",
        "OcctNet.Wpf.dll",
        "OcctNet.Avalonia.dll",
        "bridge-contract.json"
    )
    $entries = @($manifest.files)
    $manifestNames = @($entries | ForEach-Object { [string]$_.name })
    if ($manifestNames.Count -ne $expectedHashedFiles.Count) {
        throw "Bridge binary manifest contains an unexpected number of hashed files."
    }
    foreach ($name in $expectedHashedFiles) {
        if ($name -notin $manifestNames) {
            throw "Bridge binary manifest does not hash required file: $name"
        }
    }
    if (@($manifestNames | Group-Object | Where-Object Count -ne 1).Count -gt 0) {
        throw "Bridge binary manifest contains duplicate file entries."
    }

    foreach ($entry in $entries) {
        $name = [string]$entry.name
        if ([string]::IsNullOrWhiteSpace($name) -or $name.Contains('/') -or $name.Contains('\')) {
            throw "Invalid Bridge manifest file name: $name"
        }
        $path = Join-Path $DistRoot $name
        Assert-Path $path
        $actual = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
        if ($actual -ne ([string]$entry.sha256).ToLowerInvariant()) {
            throw "Bridge binary hash mismatch: $name"
        }
    }

    Write-Host ("Bridge Binary SDK: {0}, author {1}, ABI {2}, OCCT {3}, .NET SDK major {4} (reference {5}), C# {6}" -f
        $contract.bridgeVersion,
        $contract.author,
        $contract.nativeAbi.current,
        $contract.occtVersion,
        $contractSdk.Major,
        $contract.dotnet.sdkVersion,
        $contract.dotnet.languageVersion) -ForegroundColor Green
}

function Build-Project {
    param([Parameter(Mandatory = $true)][string]$Name)

    Assert-Command "dotnet"
    $definition = $Projects[$Name]
    if ($null -eq $definition) {
        throw "Unknown project key: $Name"
    }

    $project = Join-Path $RepoRoot $definition.Project
    Assert-Path $project

    Write-Host ("[{0}] Building {1} / Bridge {2}..." -f $definition.DisplayName, $Configuration, $script:BridgeVersion) -ForegroundColor Cyan
    Invoke-Checked "dotnet" @(
        "build", $project,
        "-c", $Configuration,
        "-p:Platform=x64",
        "-p:Version=$script:BridgeVersion",
        "--nologo"
    ) "$($definition.DisplayName) build failed."

    if ($null -ne $definition.Executable) {
        $output = Get-OutputDirectory $Name
        Assert-Path (Join-Path $output $definition.Executable)
        Assert-Path (Join-Path $output "OcctNative.dll")
    }
}

function Clean-Outputs {
    Write-Host "[clean] Removing generated demo outputs..." -ForegroundColor Cyan
    Remove-Item (Join-Path $RepoRoot "artifacts") -Recurse -Force -ErrorAction SilentlyContinue

    foreach ($definition in $Projects.Values) {
        $projectDirectory = Split-Path -Parent (Join-Path $RepoRoot $definition.Project)
        Remove-Item (Join-Path $projectDirectory "bin") -Recurse -Force -ErrorAction SilentlyContinue
        Remove-Item (Join-Path $projectDirectory "obj") -Recurse -Force -ErrorAction SilentlyContinue
    }

    Write-Host "Generated demo outputs removed." -ForegroundColor Green
}

Write-Host "Target:        $Target"
Write-Host "Configuration: $Configuration"
Write-Host "Bridge SDK:    $DistRoot" -ForegroundColor DarkGray
Write-Host "SDK policy:    stable .NET $ExpectedSdkMajor.x" -ForegroundColor DarkGray

if ($Target -eq "clean") {
    Clean-Outputs
    Write-Host "Build completed." -ForegroundColor Green
    exit 0
}

Assert-Path $ConsumerCheckPath
Write-Host "[consumer] Running SDK consumer boundary check..." -ForegroundColor Cyan
& $ConsumerCheckPath -RepositoryRoot $RepoRoot
if (-not $?) { throw "SDK consumer boundary validation failed." }
Test-BinarySdk

switch ($Target) {
    "validate" { }
    "common" { Build-Project "common" }
    "winform" { Build-Project "winform" }
    "wpf" { Build-Project "wpf" }
    "all" {
        Build-Project "common"
        Build-Project "winform"
        Build-Project "wpf"
    }
}

Write-Host "Build completed." -ForegroundColor Green
