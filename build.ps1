param(
    [Parameter(Position = 0)]
    [ValidateSet("common", "winform", "wpf", "avalonia", "clean", "all")]
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
$SyncScriptPath = Join-Path $RepoRoot "sync.ps1"
$DemoCoreTargetFramework = "net10.0"
$DemoDesktopTargetFramework = "net10.0-windows"
$RequiredSdkFiles = @(
    "OcctNative.dll",
    "OcctNet.dll",
    "OcctNet.WinForms.dll",
    "OcctNet.Wpf.dll",
    "OcctNet.Avalonia.dll",
    "bridge-contract.json"
)

$Projects = [ordered]@{
    common = @{ DisplayName = "OcctDemo.Common"; Project = "src\OcctDemo.Common\OcctDemo.Common.csproj"; Executable = $null; Framework = $DemoCoreTargetFramework }
    winform = @{ DisplayName = "OcctDemo.WinForms"; Project = "src\OcctDemo.WinForms\OcctDemo.WinForms.csproj"; Executable = "CAD-Winform.exe"; Framework = $DemoDesktopTargetFramework }
    wpf = @{ DisplayName = "OcctDemo.Wpf"; Project = "src\OcctDemo.Wpf\OcctDemo.Wpf.csproj"; Executable = "CAD-WPF.exe"; Framework = $DemoDesktopTargetFramework }
    avalonia = @{ DisplayName = "OcctDemo.Avalonia"; Project = "src\OcctDemo.Avalonia\OcctDemo.Avalonia.csproj"; Executable = "CAD-Avalonia.exe"; Framework = $DemoCoreTargetFramework }
}

function Assert-Path {
    param([Parameter(Mandatory = $true)][string]$Path)
    if (-not (Test-Path -LiteralPath $Path)) { throw "Required path was not found: $Path" }
}

function Ensure-BinarySdk {
    $missing = @($RequiredSdkFiles | Where-Object { -not (Test-Path -LiteralPath (Join-Path $DistRoot $_) -PathType Leaf) })
    if ($missing.Count -eq 0) { return }

    Assert-Path $SyncScriptPath
    Write-Host "[bridge] Binary SDK is missing; synchronizing Bridge main..." -ForegroundColor Cyan
    & $SyncScriptPath -BridgeBranch "main"
    if ($LASTEXITCODE -ne 0) { throw "Bridge Binary SDK synchronization failed." }

    $missing = @($RequiredSdkFiles | Where-Object { -not (Test-Path -LiteralPath (Join-Path $DistRoot $_) -PathType Leaf) })
    if ($missing.Count -gt 0) { throw "Bridge Binary SDK is incomplete: $($missing -join ', ')." }
}

function Build-Project {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string]$BridgeVersion
    )
    $definition = $Projects[$Name]
    if ($null -eq $definition) { throw "Unknown project key: $Name" }
    $project = Join-Path $RepoRoot $definition.Project
    Assert-Path $project

    Write-Host "[$Name] Building $($definition.DisplayName) / $Configuration / Bridge $BridgeVersion..." -ForegroundColor Cyan
    & dotnet build $project -c $Configuration -p:Platform=x64 -p:Version=$BridgeVersion --nologo
    if ($LASTEXITCODE -ne 0) { throw "$($definition.DisplayName) build failed with exit code $LASTEXITCODE." }

    if ($null -ne $definition.Executable) {
        $output = Join-Path (Split-Path -Parent $project) "bin\x64\$Configuration\$($definition.Framework)"
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
Write-Host "Bridge SDK:    $DistRoot" -ForegroundColor DarkGray

if ($Target -eq "clean") {
    Clean-Outputs
    Write-Host "Build completed." -ForegroundColor Green
    exit 0
}

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { throw "dotnet was not found in PATH." }
Ensure-BinarySdk
$contract = Get-Content -LiteralPath $ContractPath -Raw -Encoding UTF8 | ConvertFrom-Json
$bridgeVersion = [string]$contract.bridgeVersion
if ([string]::IsNullOrWhiteSpace($bridgeVersion)) { throw "Bridge version is missing from bridge-contract.json." }
Write-Host "Bridge:        $bridgeVersion" -ForegroundColor Green

switch ($Target) {
    "common" { Build-Project "common" $bridgeVersion }
    "winform" { Build-Project "winform" $bridgeVersion }
    "wpf" { Build-Project "wpf" $bridgeVersion }
    "avalonia" { Build-Project "avalonia" $bridgeVersion }
    "all" {
        Build-Project "common" $bridgeVersion
        Build-Project "winform" $bridgeVersion
        Build-Project "wpf" $bridgeVersion
        Build-Project "avalonia" $bridgeVersion
    }
}

Write-Host "Build completed." -ForegroundColor Green
