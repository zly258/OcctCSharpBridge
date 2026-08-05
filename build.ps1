param(
    [Parameter(Position = 0)]
    [ValidateSet("validate", "native", "managed", "smoke", "winform", "wpf", "all")]
    [string]$Target = "all",

    [Parameter(Position = 1)]
    [ValidateSet("Debug", "Release", "RelWithDebInfo")]
    [string]$Configuration = "Release",

    [string]$OcctRoot = $(if ($env:OCCT_ROOT) { $env:OCCT_ROOT } else { "D:\tools\occt-vc144-64" })
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
$ApiSurfaceCheck = Join-Path $RepoRoot "tests\check-api-surface.ps1"

$OcctIncludeDir = Join-Path $OcctRoot "inc"
$OcctLibDir = Join-Path $OcctRoot "win64\vc14\lib"
$OcctBinDir = Join-Path $OcctRoot "win64\vc14\bin"

$Projects = [ordered]@{
    wrapper = @{
        Name = "OcctNet"
        Project = "src\OcctNet\OcctNet.csproj"
        Executable = $null
    }
    common = @{
        Name = "CadCommon"
        Project = "src\CadCommon\CadCommon.csproj"
        Executable = $null
    }
    winform = @{
        Name = "CAD-Winform"
        Project = "src\CadWinForms\CadWinForms.csproj"
        Executable = "CAD-Winform.exe"
    }
    wpf = @{
        Name = "CAD-WPF"
        Project = "src\CadWpf\CadWpf.csproj"
        Executable = "CAD-WPF.exe"
    }
    smoke = @{
        Name = "OcctNet.Smoke"
        Project = "tests\OcctNet.Smoke\OcctNet.Smoke.csproj"
        Executable = "OcctNet.Smoke.exe"
    }
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

function Clean-ProjectOutput {
    param([Parameter(Mandatory = $true)][string]$ProjectDirectory)
    Remove-Item (Join-Path $ProjectDirectory "bin") -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item (Join-Path $ProjectDirectory "obj") -Recurse -Force -ErrorAction SilentlyContinue
}

function Test-ApiSurface {
    Assert-Path $ApiSurfaceCheck
    Write-Host "[api] Validating C declarations, C++ definitions and C# P/Invoke..." -ForegroundColor Cyan
    & $ApiSurfaceCheck -RepositoryRoot $RepoRoot
    if (-not $?) {
        throw "API surface validation failed."
    }
}

function Assert-OcctSdk {
    foreach ($path in @(
        $OcctIncludeDir,
        $OcctLibDir,
        $OcctBinDir,
        (Join-Path $OcctIncludeDir "Standard.hxx"),
        (Join-Path $OcctLibDir "TKernel.lib"),
        (Join-Path $OcctBinDir "TKernel.dll")
    )) {
        Assert-Path $path
    }
}

function Build-Native {
    Assert-Command "cmake"
    Assert-OcctSdk

    Write-Host "[native] Configuring OCCT 7.9.0 bridge..." -ForegroundColor Cyan
    Invoke-Checked "cmake" @(
        "-S", $NativeSource,
        "-B", $NativeBuild,
        "-G", "Visual Studio 17 2022",
        "-A", "x64",
        "-DOCCT_ROOT=$OcctRoot",
        "-DOCCT_INCLUDE_DIR=$OcctIncludeDir",
        "-DOCCT_LIB_DIR=$OcctLibDir",
        "-DOCCT_BIN_DIR=$OcctBinDir"
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

function Build-ManagedProject {
    param([Parameter(Mandatory = $true)][string]$Key)

    Assert-Command "dotnet"
    $project = $Projects[$Key]
    if ($null -eq $project) {
        throw "Unknown managed project key: $Key"
    }

    $projectFile = Join-Path $RepoRoot $project.Project
    Assert-Path $projectFile
    Clean-ProjectOutput (Split-Path -Parent $projectFile)

    Write-Host ("[{0}] Building {1}..." -f $project.Name, $Configuration) -ForegroundColor Cyan
    Invoke-Checked "dotnet" @(
        "build", $projectFile,
        "-c", $Configuration,
        "-p:Platform=x64",
        "--nologo"
    ) "$($project.Name) build failed."

    if ($null -ne $project.Executable) {
        $output = Join-Path (Split-Path -Parent $projectFile) "bin\x64\$Configuration\net8.0-windows"
        Assert-Path (Join-Path $output $project.Executable)
        if (Test-Path $NativeDll) {
            Assert-Path (Join-Path $output "OcctNative.dll")
        }
    }
}

function Run-Smoke {
    Build-ManagedProject "smoke"
    $smokeProject = Join-Path $RepoRoot $Projects.smoke.Project
    $smokeOutput = Join-Path (Split-Path -Parent $smokeProject) "bin\x64\$Configuration\net8.0-windows"
    Copy-Item $NativeDll (Join-Path $smokeOutput "OcctNative.dll") -Force

    $previousNativeDirectory = $env:OCCT_BRIDGE_NATIVE_DIR
    try {
        $env:OCCT_BRIDGE_NATIVE_DIR = $smokeOutput
        Write-Host "[smoke] Running native, modeling and OCAF/XDE scenarios..." -ForegroundColor Cyan
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
Write-Host "OCCT root:     $OcctRoot" -ForegroundColor DarkGray

Test-ApiSurface

switch ($Target) {
    "validate" {
    }
    "managed" {
        Build-ManagedProject "wrapper"
        Build-ManagedProject "common"
    }
    "native" {
        Build-Native
    }
    "winform" {
        Build-Native
        Build-ManagedProject "winform"
    }
    "wpf" {
        Build-Native
        Build-ManagedProject "wpf"
    }
    "smoke" {
        Build-Native
        Run-Smoke
    }
    "all" {
        Build-Native
        Build-ManagedProject "winform"
        Build-ManagedProject "wpf"
        Build-ManagedProject "smoke"
    }
}

Write-Host "Build completed." -ForegroundColor Green
