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
$script:TargetFramework = "net10.0-windows"

[xml]$props = Get-Content -LiteralPath $PropsPath -Raw
$propertyGroup = $props.Project.PropertyGroup
$globalJson = Get-Content -LiteralPath $GlobalJsonPath -Raw | ConvertFrom-Json
$ExpectedBridgeVersion = [string]$propertyGroup.Version
$ExpectedNativeAbi = [int]$propertyGroup.OcctBridgeNativeAbiVersion
$ExpectedOcctVersion = [string]$propertyGroup.OcctBridgeOcctVersion
$ExpectedLanguageVersion = [string]$propertyGroup.LangVersion
$ExpectedSdkVersion = [string]$globalJson.sdk.version

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
    avalonia = @{
        DisplayName = "OcctDemo.Avalonia"
        Project = "src\OcctDemo.Avalonia\OcctDemo.Avalonia.csproj"
        Executable = "CAD-Avalonia.exe"
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
    $required = @(
        "OcctNative.dll",
        "OcctNet.dll",
        "OcctNet.WinForms.dll",
        "OcctNet.Wpf.dll",
        "OcctNet.Avalonia.dll",
        "bridge-contract.json",
        "bridge-manifest.json"
    )
    foreach ($name in $required) {
        Assert-Path (Join-Path $DistRoot $name)
    }

    $contract = Get-Content -LiteralPath $ContractPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $manifest = Get-Content -LiteralPath $ManifestPath -Raw -Encoding UTF8 | ConvertFrom-Json

    if ([string]$contract.bridgeVersion -ne $ExpectedBridgeVersion) {
        throw "Unsupported Bridge version: $($contract.bridgeVersion). Demo expects $ExpectedBridgeVersion."
    }
    if ([int]$contract.nativeAbiVersion -ne $ExpectedNativeAbi) {
        throw "Unsupported Bridge native ABI: $($contract.nativeAbiVersion). Demo expects ABI $ExpectedNativeAbi."
    }
    if ([string]$contract.occtVersion -ne $ExpectedOcctVersion) {
        throw "Unsupported OCCT version: $($contract.occtVersion). Demo expects $ExpectedOcctVersion."
    }
    if ([string]$contract.platform -ne "windows-x64") {
        throw "Unsupported Bridge platform: $($contract.platform)"
    }
    if ([string]$contract.dotnet.targetFramework -ne "net10.0-windows") {
        throw "Unsupported Bridge target framework: $($contract.dotnet.targetFramework)"
    }
    if ([string]$contract.dotnet.sdkVersion -ne $ExpectedSdkVersion) {
        throw "Unsupported Bridge .NET SDK: $($contract.dotnet.sdkVersion). Demo expects $ExpectedSdkVersion."
    }
    if ([string]$contract.dotnet.languageVersion -ne $ExpectedLanguageVersion) {
        throw "Unsupported Bridge C# language version: $($contract.dotnet.languageVersion). Demo expects $ExpectedLanguageVersion."
    }
    $script:TargetFramework = [string]$contract.dotnet.targetFramework

    if ([int]$manifest.schemaVersion -ne 1) {
        throw "Unsupported Bridge binary manifest schema: $($manifest.schemaVersion)"
    }
    if ([string]$manifest.bridgeVersion -ne [string]$contract.bridgeVersion -or
        [int]$manifest.nativeAbiVersion -ne [int]$contract.nativeAbiVersion -or
        [string]$manifest.occtVersion -ne [string]$contract.occtVersion -or
        [string]$manifest.platform -ne [string]$contract.platform -or
        [string]$manifest.targetFramework -ne [string]$contract.dotnet.targetFramework -or
        [string]$manifest.sdkVersion -ne [string]$contract.dotnet.sdkVersion -or
        [string]$manifest.languageVersion -ne [string]$contract.dotnet.languageVersion) {
        throw "Bridge binary manifest does not match bridge-contract.json."
    }

    $expectedHashedFiles = @(
        "OcctNative.dll",
        "OcctNet.dll",
        "OcctNet.WinForms.dll",
        "OcctNet.Wpf.dll",
        "OcctNet.Avalonia.dll",
        "bridge-contract.json"
    )
    $manifestNames = @($manifest.files | ForEach-Object { [string]$_.name })
    foreach ($name in $expectedHashedFiles) {
        if ($name -notin $manifestNames) {
            throw "Bridge binary manifest does not hash required file: $name"
        }
    }

    foreach ($entry in @($manifest.files)) {
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

    Write-Host ("Bridge Binary SDK: {0}, ABI {1}, OCCT {2}, .NET SDK {3}, C# {4}" -f
        $contract.bridgeVersion,
        $contract.nativeAbiVersion,
        $contract.occtVersion,
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

    Write-Host ("[{0}] Building {1} / Bridge {2}..." -f $definition.DisplayName, $Configuration, $ExpectedBridgeVersion) -ForegroundColor Cyan
    Invoke-Checked "dotnet" @(
        "build", $project,
        "-c", $Configuration,
        "-p:Platform=x64",
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

if ($Target -eq "clean") {
    Clean-Outputs
    Write-Host "Build completed." -ForegroundColor Green
    exit 0
}

Test-BinarySdk

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
