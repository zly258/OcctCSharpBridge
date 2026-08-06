param(
    [Parameter(Position = 0)]
    [ValidateSet("validate", "native", "managed", "smoke", "all")]
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
$ManagedProject = Join-Path $RepoRoot "src\OcctNet\OcctNet.csproj"
$WinFormsProject = Join-Path $RepoRoot "src\OcctNet.WinForms\OcctNet.WinForms.csproj"
$WpfProject = Join-Path $RepoRoot "src\OcctNet.Wpf\OcctNet.Wpf.csproj"
$ManagedOutput = Join-Path $RepoRoot "src\OcctNet\bin\x64\$Configuration\net8.0-windows"
$WinFormsOutput = Join-Path $RepoRoot "src\OcctNet.WinForms\bin\x64\$Configuration\net8.0-windows"
$WpfOutput = Join-Path $RepoRoot "src\OcctNet.Wpf\bin\x64\$Configuration\net8.0-windows"
$SmokeProject = Join-Path $RepoRoot "tests\OcctNet.Smoke\OcctNet.Smoke.csproj"
$SmokeOutput = Join-Path $RepoRoot "tests\OcctNet.Smoke\bin\x64\$Configuration\net8.0-windows"
$ApiSurfaceCheck = Join-Path $RepoRoot "tests\check-api-surface.ps1"
$NativeBuildCheck = Join-Path $RepoRoot "tests\check-native-build-structure.ps1"
$SelectionContractCheck = Join-Path $RepoRoot "tests\check-selection-contract.ps1"
$ViewportApiCheck = Join-Path $RepoRoot "tests\check-viewport-api.ps1"
$UiHostsCheck = Join-Path $RepoRoot "tests\check-ui-hosts.ps1"
$AnalyticGeometryCheck = Join-Path $RepoRoot "tests\check-analytic-geometry-api.ps1"

$OcctIncludeDir = Join-Path $OcctRoot "inc"
$OcctLibDir = Join-Path $OcctRoot "win64\vc14\lib"
$OcctBinDir = Join-Path $OcctRoot "win64\vc14\bin"

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

function Test-ApiSurface {
    Assert-Path $ApiSurfaceCheck
    Assert-Path $NativeBuildCheck
    Assert-Path $SelectionContractCheck
    Assert-Path $ViewportApiCheck
    Assert-Path $UiHostsCheck
    Assert-Path $AnalyticGeometryCheck

    Write-Host "[analytic-geometry] Validating analytic curve and surface contracts..." -ForegroundColor Cyan
    & $AnalyticGeometryCheck -RepositoryRoot $RepoRoot
    if (-not $?) {
        throw "Analytic geometry API validation failed."
    }

    Write-Host "[ui-hosts] Validating WinForms and WPF viewport hosts..." -ForegroundColor Cyan
    & $UiHostsCheck -RepositoryRoot $RepoRoot
    if (-not $?) {
        throw "UI host validation failed."
    }

    Write-Host "[viewport] Validating extended viewport contracts..." -ForegroundColor Cyan
    & $ViewportApiCheck -RepositoryRoot $RepoRoot
    if (-not $?) {
        throw "Viewport API validation failed."
    }

    Write-Host "[selection] Validating point and rectangle selection behavior..." -ForegroundColor Cyan
    & $SelectionContractCheck -RepositoryRoot $RepoRoot
    if (-not $?) {
        throw "Selection contract validation failed."
    }

    Write-Host "[native-build] Validating CMake sources and toolkit boundaries..." -ForegroundColor Cyan
    & $NativeBuildCheck -RepositoryRoot $RepoRoot
    if (-not $?) {
        throw "Native build structure validation failed."
    }

    Write-Host "[api] Validating native declarations, implementations and P/Invoke..." -ForegroundColor Cyan
    & $ApiSurfaceCheck -RepositoryRoot $RepoRoot
    if (-not $?) {
        throw "API surface validation failed."
    }
}

function Build-Native {
    Assert-Command "cmake"
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

    Write-Host "[native] Configuring..." -ForegroundColor Cyan
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

function Build-Managed {
    Assert-Command "dotnet"

    foreach ($project in @($ManagedProject, $WinFormsProject, $WpfProject)) {
        Remove-Item (Join-Path (Split-Path -Parent $project) "bin") -Recurse -Force -ErrorAction SilentlyContinue
        Remove-Item (Join-Path (Split-Path -Parent $project) "obj") -Recurse -Force -ErrorAction SilentlyContinue
    }

    Write-Host "[managed] Building core wrapper ($Configuration)..." -ForegroundColor Cyan
    Invoke-Checked "dotnet" @(
        "build", $ManagedProject,
        "-c", $Configuration,
        "-p:Platform=x64",
        "--nologo"
    ) "OcctNet build failed."

    Write-Host "[managed] Building optional WinForms host ($Configuration)..." -ForegroundColor Cyan
    Invoke-Checked "dotnet" @(
        "build", $WinFormsProject,
        "-c", $Configuration,
        "-p:Platform=x64",
        "--nologo"
    ) "OcctNet.WinForms build failed."

    Write-Host "[managed] Building optional WPF host ($Configuration)..." -ForegroundColor Cyan
    Invoke-Checked "dotnet" @(
        "build", $WpfProject,
        "-c", $Configuration,
        "-p:Platform=x64",
        "--nologo"
    ) "OcctNet.Wpf build failed."

    Assert-Path (Join-Path $ManagedOutput "OcctNet.dll")
    Assert-Path (Join-Path $WinFormsOutput "OcctNet.WinForms.dll")
    Assert-Path (Join-Path $WpfOutput "OcctNet.Wpf.dll")

    if (Test-Path $NativeDll) {
        Copy-Item $NativeDll (Join-Path $ManagedOutput "OcctNative.dll") -Force
    }
    else {
        Write-Warning "OcctNative.dll was not found. Build target 'all' or 'native' before running a consumer application."
    }

    Write-Host "Managed core:     $ManagedOutput" -ForegroundColor Green
    Write-Host "Managed WinForms: $WinFormsOutput" -ForegroundColor Green
    Write-Host "Managed WPF:      $WpfOutput" -ForegroundColor Green
}

function Run-Smoke {
    Assert-Command "dotnet"
    Assert-Path $NativeDll

    Remove-Item (Join-Path (Split-Path -Parent $SmokeProject) "bin") -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item (Join-Path (Split-Path -Parent $SmokeProject) "obj") -Recurse -Force -ErrorAction SilentlyContinue

    Write-Host "[smoke] Building $Configuration..." -ForegroundColor Cyan
    Invoke-Checked "dotnet" @(
        "build", $SmokeProject,
        "-c", $Configuration,
        "-p:Platform=x64",
        "--nologo"
    ) "Smoke test build failed."

    Copy-Item $NativeDll (Join-Path $SmokeOutput "OcctNative.dll") -Force
    $previousNativeDirectory = $env:OCCT_BRIDGE_NATIVE_DIR
    try {
        $env:OCCT_BRIDGE_NATIVE_DIR = $SmokeOutput
        Write-Host "[smoke] Running..." -ForegroundColor Cyan
        Invoke-Checked "dotnet" @(
            "run",
            "--project", $SmokeProject,
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
    "native" {
        Build-Native
    }
    "managed" {
        Build-Managed
    }
    "smoke" {
        Build-Native
        Build-Managed
        Run-Smoke
    }
    "all" {
        Build-Native
        Build-Managed
    }
}

Write-Host "Build completed." -ForegroundColor Green
