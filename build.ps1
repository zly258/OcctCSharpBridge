param(
    [Parameter(Position = 0)]
    [ValidateSet("native", "managed", "all")]
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
$ManagedOutput = Join-Path $RepoRoot "src\OcctNet\bin\x64\$Configuration\net8.0-windows"

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

    Remove-Item (Join-Path (Split-Path -Parent $ManagedProject) "bin") -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item (Join-Path (Split-Path -Parent $ManagedProject) "obj") -Recurse -Force -ErrorAction SilentlyContinue

    Write-Host "[managed] Building $Configuration..." -ForegroundColor Cyan
    Invoke-Checked "dotnet" @(
        "build", $ManagedProject,
        "-c", $Configuration,
        "-p:Platform=x64",
        "--nologo"
    ) "OcctNet build failed."

    Assert-Path (Join-Path $ManagedOutput "OcctNet.dll")

    if (Test-Path $NativeDll) {
        Copy-Item $NativeDll (Join-Path $ManagedOutput "OcctNative.dll") -Force
    }
    else {
        Write-Warning "OcctNative.dll was not found. Build target 'all' or 'native' before running a consumer application."
    }

    Write-Host "Managed: $ManagedOutput" -ForegroundColor Green
}

Write-Host "Target:        $Target"
Write-Host "Configuration: $Configuration"
Write-Host "OCCT root:     $OcctRoot" -ForegroundColor DarkGray

switch ($Target) {
    "native" {
        Build-Native
    }
    "managed" {
        Build-Managed
    }
    "all" {
        Build-Native
        Build-Managed
    }
}

Write-Host "Build completed." -ForegroundColor Green
