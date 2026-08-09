param(
    [Parameter(Position = 0)]
    [ValidateSet("validate", "native", "managed", "smoke", "winform", "wpf", "avalonia", "ci", "all")]
    [string]$Target = "all",

    [Parameter(Position = 1)]
    [ValidateSet("Debug", "Release", "RelWithDebInfo")]
    [string]$Configuration = "Release",

    [string]$OcctRoot = $env:OCCT_ROOT
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
$RepoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$DefaultOcctRoot = "D:\tools\occt-vc144-64"
if ([string]::IsNullOrWhiteSpace($OcctRoot)) {
    $OcctRoot = $DefaultOcctRoot
}
$NativeSource = Join-Path $RepoRoot "src\OcctNative"
$NativeBuild = Join-Path $RepoRoot "build\native"
$NativeDll = Join-Path $NativeBuild "bin\$Configuration\OcctNative.dll"
$ContractPath = Join-Path $RepoRoot "bridge-contract.json"

if (-not (Test-Path $ContractPath -PathType Leaf)) {
    throw "Bridge contract file was not found: $ContractPath"
}
$Contract = Get-Content $ContractPath -Raw -Encoding UTF8 | ConvertFrom-Json
$BridgeVersion = [string]$Contract.bridgeVersion
$RequiredOcctVersion = [string]$Contract.occtVersion
$TargetFramework = [string]$Contract.dotnet.targetFramework
$SdkVersion = [string]$Contract.dotnet.sdkVersion

$Projects = [ordered]@{
    Core = @{
        DisplayName = "OcctNet"
        Project = "src\OcctNet\OcctNet.csproj"
        Executable = $null
    }
    WinFormsHost = @{
        DisplayName = "OcctNet.WinForms"
        Project = "src\OcctNet.WinForms\OcctNet.WinForms.csproj"
        Executable = $null
    }
    WpfHost = @{
        DisplayName = "OcctNet.Wpf"
        Project = "src\OcctNet.Wpf\OcctNet.Wpf.csproj"
        Executable = $null
    }
    AvaloniaHost = @{
        DisplayName = "OcctNet.Avalonia"
        Project = "src\OcctNet.Avalonia\OcctNet.Avalonia.csproj"
        Executable = $null
    }
    DemoCommon = @{
        DisplayName = "OcctDemo.Common"
        Project = "src\OcctDemo.Common\OcctDemo.Common.csproj"
        Executable = $null
    }
    ManagedTests = @{
        DisplayName = "OcctNet.ManagedTests"
        Project = "tests\OcctNet.ManagedTests\OcctNet.ManagedTests.csproj"
        Executable = $null
    }
    WinFormsDemo = @{
        DisplayName = "CAD-Winform"
        Project = "src\OcctDemo.WinForms\OcctDemo.WinForms.csproj"
        Executable = "CAD-Winform.exe"
    }
    WpfDemo = @{
        DisplayName = "CAD-WPF"
        Project = "src\OcctDemo.Wpf\OcctDemo.Wpf.csproj"
        Executable = "CAD-WPF.exe"
    }
    AvaloniaDemo = @{
        DisplayName = "CAD-Avalonia"
        Project = "src\OcctDemo.Avalonia\OcctDemo.Avalonia.csproj"
        Executable = "CAD-Avalonia.exe"
    }
    Smoke = @{
        DisplayName = "OcctNet.Smoke"
        Project = "tests\OcctNet.Smoke\OcctNet.Smoke.csproj"
        Executable = "OcctNet.Smoke.exe"
    }
}

$Checks = [ordered]@{
    Version = "tests\check-version-contract.ps1"
    Organization = "tests\check-api-organization.ps1"
    Geometry = "tests\check-geometry-api.ps1"
    TopologyAnalysis = "tests\check-topology-analysis.ps1"
    RuntimeDiagnostics = "tests\check-runtime-diagnostics.ps1"
    UiHosts = "tests\check-ui-hosts.ps1"
    AvaloniaHost = "tests\check-avalonia-host.ps1"
    Viewport = "tests\check-viewport-api.ps1"
    Selection = "tests\check-selection-contract.ps1"
    NativeBuild = "tests\check-native-build-structure.ps1"
    ApiSurface = "tests\check-api-surface.ps1"
    DemoUi = "tests\check-demo-ui-structure.ps1"
    DemoPreconditions = "tests\check-demo-command-preconditions.ps1"
    Package = "tests\check-demo-package.ps1"
}

function Assert-Path {
    param([Parameter(Mandatory = $true)][string]$Path)
    if (-not (Test-Path $Path)) {
        throw "Required path was not found: $Path"
    }
}

function Assert-Command {
    param([Parameter(Mandatory = $true)][string]$Name)
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
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

function Invoke-ContractChecks {
    foreach ($check in $Checks.GetEnumerator()) {
        $path = Join-Path $RepoRoot $check.Value
        Assert-Path $path
        Write-Host ("[{0}] Running {1}..." -f $check.Key.ToLowerInvariant(), $check.Value) -ForegroundColor Cyan
        & $path -RepositoryRoot $RepoRoot
        if (-not $?) {
            throw "$($check.Key) validation failed."
        }
    }
}

function Resolve-OcctConfiguration {
    $script:ResolvedOcctRoot = [System.IO.Path]::GetFullPath($OcctRoot)
    if (-not (Test-Path $script:ResolvedOcctRoot -PathType Container)) {
        throw "OCCT SDK root was not found: $script:ResolvedOcctRoot. Set OCCT_ROOT, pass -OcctRoot <path>, or install OCCT at $DefaultOcctRoot. validate/managed/ci do not require OCCT."
    }
    $script:OcctIncludeDir = Join-Path $script:ResolvedOcctRoot "inc"
    $script:OcctLibDir = Join-Path $script:ResolvedOcctRoot "win64\vc14\lib"
    $script:OcctBinDir = Join-Path $script:ResolvedOcctRoot "win64\vc14\bin"
    $script:OcctThirdPartyDir = Join-Path $script:ResolvedOcctRoot "3rdparty-vc14-64"

    foreach ($path in @(
        $script:OcctIncludeDir,
        $script:OcctLibDir,
        $script:OcctBinDir,
        (Join-Path $script:OcctIncludeDir "Standard.hxx"),
        (Join-Path $script:OcctLibDir "TKernel.lib"),
        (Join-Path $script:OcctBinDir "TKernel.dll")
    )) {
        Assert-Path $path
    }
}

function Build-Native {
    Assert-Command "cmake"
    Resolve-OcctConfiguration

    Write-Host "[native] Configuring OCCT $RequiredOcctVersion bridge..." -ForegroundColor Cyan
    Invoke-Checked "cmake" @(
        "-S", $NativeSource,
        "-B", $NativeBuild,
        "-G", "Visual Studio 17 2022",
        "-A", "x64",
        "-DOCCT_ROOT=$script:ResolvedOcctRoot",
        "-DOCCT_INCLUDE_DIR=$script:OcctIncludeDir",
        "-DOCCT_LIB_DIR=$script:OcctLibDir",
        "-DOCCT_BIN_DIR=$script:OcctBinDir"
    ) "CMake configure failed."

    Write-Host "[native] Building $Configuration..." -ForegroundColor Cyan
    Invoke-Checked "cmake" @(
        "--build", $NativeBuild,
        "--config", $Configuration,
        "--parallel"
    ) "Native build failed."

    Assert-Path $NativeDll
    Write-Host "Native: $NativeDll" -ForegroundColor Green
}

function Copy-OcctRuntimeDependencies {
    param([Parameter(Mandatory = $true)][string]$OutputDirectory)

    Resolve-OcctConfiguration
    Assert-Path $OutputDirectory
    Assert-Path $NativeDll

    Write-Host "[runtime] Deploying OCCT runtime beside the application..." -ForegroundColor Cyan
    Copy-Item $NativeDll (Join-Path $OutputDirectory "OcctNative.dll") -Force

    Get-ChildItem $script:OcctBinDir -Filter "*.dll" -File | ForEach-Object {
        Copy-Item $_.FullName (Join-Path $OutputDirectory $_.Name) -Force
    }

    if (Test-Path $script:OcctThirdPartyDir -PathType Container) {
        Get-ChildItem $script:OcctThirdPartyDir -Filter "*.dll" -File -Recurse | Where-Object {
            $_.DirectoryName -match '[\\/]bin([\\/]|$)'
        } | ForEach-Object {
            Copy-Item $_.FullName (Join-Path $OutputDirectory $_.Name) -Force
        }
    }

    Assert-Path (Join-Path $OutputDirectory "OcctNative.dll")
    Assert-Path (Join-Path $OutputDirectory "TKernel.dll")
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
    $projectDirectory = Split-Path -Parent $project
    Remove-Item (Join-Path $projectDirectory "bin") -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item (Join-Path $projectDirectory "obj") -Recurse -Force -ErrorAction SilentlyContinue

    Write-Host ("[{0}] Building {1}..." -f $definition.DisplayName, $Configuration) -ForegroundColor Cyan
    Invoke-Checked "dotnet" @(
        "build", $project,
        "-c", $Configuration,
        "-p:Platform=x64",
        "--nologo"
    ) "$($definition.DisplayName) build failed."

    if ($null -ne $definition.Executable) {
        $output = Join-Path $projectDirectory "bin\x64\$Configuration\$TargetFramework"
        Assert-Path (Join-Path $output $definition.Executable)
        if (Test-Path $NativeDll) {
            Copy-Item $NativeDll (Join-Path $output "OcctNative.dll") -Force
            Assert-Path (Join-Path $output "OcctNative.dll")
        }
    }
}

function Run-ManagedTests {
    Assert-Command "dotnet"
    $definition = $Projects.ManagedTests
    $project = Join-Path $RepoRoot $definition.Project
    $output = Join-Path (Split-Path -Parent $project) "bin\x64\$Configuration\$TargetFramework"
    $assembly = Join-Path $output "OcctNet.ManagedTests.dll"
    Assert-Path $assembly
    Write-Host "[managed-tests] Running managed-only bridge regression tests..." -ForegroundColor Cyan
    Invoke-Checked "dotnet" @($assembly) "Managed bridge regression tests failed."
}

function Get-ProjectOutputDirectory {
    param([Parameter(Mandatory = $true)][string]$Name)

    $definition = $Projects[$Name]
    if ($null -eq $definition) {
        throw "Unknown project key: $Name"
    }

    $project = Join-Path $RepoRoot $definition.Project
    return Join-Path (Split-Path -Parent $project) "bin\x64\$Configuration\$TargetFramework"
}

function Build-Managed {
    Build-Project "Core"
    Build-Project "WinFormsHost"
    Build-Project "WpfHost"
    Build-Project "AvaloniaHost"
    Build-Project "DemoCommon"
}

function Build-Ci {
    Build-Managed
    Build-Project "ManagedTests"
    Run-ManagedTests
    Build-Project "WinFormsDemo"
    Build-Project "WpfDemo"
    Build-Project "AvaloniaDemo"
    Build-Project "Smoke"
}

function Run-Smoke {
    Assert-Path $NativeDll
    Build-Project "Smoke"

    $smokeProject = Join-Path $RepoRoot $Projects.Smoke.Project
    $smokeOutput = Join-Path (Split-Path -Parent $smokeProject) "bin\x64\$Configuration\$TargetFramework"
    Copy-Item $NativeDll (Join-Path $smokeOutput "OcctNative.dll") -Force

    $previousNativeDirectory = $env:OCCT_BRIDGE_NATIVE_DIR
    try {
        $env:OCCT_BRIDGE_NATIVE_DIR = $smokeOutput
        Write-Host "[smoke] Running native modeling scenarios..." -ForegroundColor Cyan
        Invoke-Checked "dotnet" @(
            "run",
            "--project", $smokeProject,
            "-c", $Configuration,
            "-p:Platform=x64",
            "--no-build"
        ) "Smoke test failed."
    }
    finally {
        $env:OCCT_BRIDGE_NATIVE_DIR = $previousNativeDirectory
    }
}

Write-Host "Target:        $Target"
Write-Host "Configuration: $Configuration"
Write-Host "Bridge:        $BridgeVersion"
Write-Host "SDK:           $SdkVersion" -ForegroundColor DarkGray
$occtRootSource = if ($env:OCCT_ROOT) { "environment" } elseif ($OcctRoot -eq $DefaultOcctRoot) { "default" } else { "argument" }
Write-Host "OCCT root:     $OcctRoot ($occtRootSource)" -ForegroundColor DarkGray

Invoke-ContractChecks

switch ($Target) {
    "validate" { }
    "managed" { Build-Managed }
    "ci" { Build-Ci }
    "native" { Build-Native }
    "winform" {
        Build-Native
        Build-Project "WinFormsDemo"
    }
    "wpf" {
        Build-Native
        Build-Project "WpfDemo"
    }
    "avalonia" {
        Build-Native
        Build-Project "AvaloniaDemo"
        Copy-OcctRuntimeDependencies (Get-ProjectOutputDirectory "AvaloniaDemo")
    }
    "smoke" {
        Build-Native
        Run-Smoke
    }
    "all" {
        Build-Native
        Build-Project "WinFormsDemo"
        Build-Project "WpfDemo"
        Build-Project "AvaloniaDemo"
        Copy-OcctRuntimeDependencies (Get-ProjectOutputDirectory "AvaloniaDemo")
        Build-Project "Smoke"
    }
}

Write-Host "Build completed." -ForegroundColor Green
