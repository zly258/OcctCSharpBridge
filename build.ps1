param(
    [Parameter(Position = 0)]
    [ValidateSet("validate", "native", "managed", "smoke", "ci", "all")]
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
    Core = "src\OcctNet\OcctNet.csproj"
    WinForms = "src\OcctNet.WinForms\OcctNet.WinForms.csproj"
    Wpf = "src\OcctNet.Wpf\OcctNet.Wpf.csproj"
    Smoke = "tests\OcctNet.Smoke\OcctNet.Smoke.csproj"
}

$Checks = [ordered]@{
    Version = "tests\check-version-contract.ps1"
    Organization = "tests\check-api-organization.ps1"
    Geometry = "tests\check-geometry-api.ps1"
    UiHosts = "tests\check-ui-hosts.ps1"
    Viewport = "tests\check-viewport-api.ps1"
    Selection = "tests\check-selection-contract.ps1"
    NativeBuild = "tests\check-native-build-structure.ps1"
    ApiSurface = "tests\check-api-surface.ps1"
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

    $script:OcctRoot = [System.IO.Path]::GetFullPath($OcctRoot)
    $script:OcctIncludeDir = Join-Path $script:OcctRoot "inc"
    $script:OcctLibDir = Join-Path $script:OcctRoot "win64\vc14\lib"
    $script:OcctBinDir = Join-Path $script:OcctRoot "win64\vc14\bin"

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
        "-DOCCT_ROOT=$script:OcctRoot",
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
    $relativePath = $Projects[$Name]
    if ([string]::IsNullOrWhiteSpace($relativePath)) {
        throw "Unknown project key: $Name"
    }

    $project = Join-Path $RepoRoot $relativePath
    Assert-Path $project
    $projectDirectory = Split-Path -Parent $project
    Remove-Item (Join-Path $projectDirectory "bin") -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item (Join-Path $projectDirectory "obj") -Recurse -Force -ErrorAction SilentlyContinue

    Write-Host "[$($Name.ToLowerInvariant())] Building $Configuration..." -ForegroundColor Cyan
    Invoke-Checked "dotnet" @(
        "build", $project,
        "-c", $Configuration,
        "-p:Platform=x64",
        "--nologo"
    ) "$Name build failed."
}

function Build-Managed {
    Build-Project "Core"
    Build-Project "WinForms"
    Build-Project "Wpf"
}

function Build-Ci {
    Build-Managed
    Build-Project "Smoke"
}

function Run-Smoke {
    Assert-Path $NativeDll
    Build-Project "Smoke"

    $smokeProject = Join-Path $RepoRoot $Projects.Smoke
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
if ([string]::IsNullOrWhiteSpace($OcctRoot)) {
    Write-Host "OCCT root:     not configured (valid for validate/managed/ci)" -ForegroundColor DarkGray
}
else {
    Write-Host "OCCT root:     $OcctRoot" -ForegroundColor DarkGray
}

Invoke-ContractChecks

switch ($Target) {
    "validate" { }
    "native" { Build-Native }
    "managed" { Build-Managed }
    "ci" { Build-Ci }
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