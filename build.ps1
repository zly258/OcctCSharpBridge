param(
    [Parameter(Position = 0)]
    [ValidateSet("validate", "native", "managed", "smoke", "winform", "wpf", "all")]
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
$NativeSource = Join-Path $RepoRoot "src\OcctNative"
$NativeBuild = Join-Path $RepoRoot "build\native"
$NativeDll = Join-Path $NativeBuild "bin\$Configuration\OcctNative.dll"

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
    DemoCommon = @{
        DisplayName = "CadCommon"
        Project = "src\CadCommon\CadCommon.csproj"
        Executable = $null
    }
    WinFormsDemo = @{
        DisplayName = "CAD-Winform"
        Project = "src\CadWinForms\CadWinForms.csproj"
        Executable = "CAD-Winform.exe"
    }
    WpfDemo = @{
        DisplayName = "CAD-WPF"
        Project = "src\CadWpf\CadWpf.csproj"
        Executable = "CAD-WPF.exe"
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
    AnalyticGeometry = "tests\check-analytic-geometry-api.ps1"
    DifferentialGeometry = "tests\check-differential-geometry-api.ps1"
    UiHosts = "tests\check-ui-hosts.ps1"
    Viewport = "tests\check-viewport-api.ps1"
    Selection = "tests\check-selection-contract.ps1"
    NativeBuild = "tests\check-native-build-structure.ps1"
    ApiSurface = "tests\check-api-surface.ps1"
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
    if ([string]::IsNullOrWhiteSpace($OcctRoot)) {
        throw "OCCT_ROOT is not configured. Pass -OcctRoot <path> or set the OCCT_ROOT environment variable."
    }

    $script:ResolvedOcctRoot = [System.IO.Path]::GetFullPath($OcctRoot)
    $script:OcctIncludeDir = Join-Path $script:ResolvedOcctRoot "inc"
    $script:OcctLibDir = Join-Path $script:ResolvedOcctRoot "win64\vc14\lib"
    $script:OcctBinDir = Join-Path $script:ResolvedOcctRoot "win64\vc14\bin"

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

    Write-Host "[native] Configuring OCCT 7.9.0 bridge..." -ForegroundColor Cyan
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
        $output = Join-Path $projectDirectory "bin\x64\$Configuration\net8.0-windows"
        Assert-Path (Join-Path $output $definition.Executable)
        if (Test-Path $NativeDll) {
            Assert-Path (Join-Path $output "OcctNative.dll")
        }
    }
}

function Build-Managed {
    Build-Project "Core"
    Build-Project "WinFormsHost"
    Build-Project "WpfHost"
    Build-Project "DemoCommon"
}

function Run-Smoke {
    Assert-Path $NativeDll
    Build-Project "Smoke"

    $smokeProject = Join-Path $RepoRoot $Projects.Smoke.Project
    $smokeOutput = Join-Path (Split-Path -Parent $smokeProject) "bin\x64\$Configuration\net8.0-windows"
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
if ([string]::IsNullOrWhiteSpace($OcctRoot)) {
    Write-Host "OCCT root:     not configured (valid for validate/managed)" -ForegroundColor DarkGray
}
else {
    Write-Host "OCCT root:     $OcctRoot" -ForegroundColor DarkGray
}

Invoke-ContractChecks

switch ($Target) {
    "validate" { }
    "managed" { Build-Managed }
    "native" { Build-Native }
    "winform" {
        Build-Native
        Build-Project "WinFormsDemo"
    }
    "wpf" {
        Build-Native
        Build-Project "WpfDemo"
    }
    "smoke" {
        Build-Native
        Run-Smoke
    }
    "all" {
        Build-Native
        Build-Project "WinFormsDemo"
        Build-Project "WpfDemo"
        Build-Project "Smoke"
    }
}

Write-Host "Build completed." -ForegroundColor Green
