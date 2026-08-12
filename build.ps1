param(
    [Parameter(Position = 0)]
    [ValidateSet("validate", "native", "managed", "test", "smoke", "avalonia-smoke", "docs", "clean", "all")]
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
if (Test-Path "$env:SystemRoot\System32\chcp.com") { & "$env:SystemRoot\System32\chcp.com" 65001 | Out-Null }

$Target = $Target.ToLowerInvariant()
$RepoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$DefaultOcctRoot = "D:\tools\occt-vc144-64"
if ([string]::IsNullOrWhiteSpace($OcctRoot)) { $OcctRoot = $DefaultOcctRoot }

$NativeSource = Join-Path $RepoRoot "src\OcctNative"
$NativeBuild = Join-Path $RepoRoot "build\native"
$NativeDll = Join-Path $NativeBuild "bin\$Configuration\OcctNative.dll"
$ContractPath = Join-Path $RepoRoot "bridge-contract.json"
$ApiDocsGenerator = Join-Path $RepoRoot "tools\OcctApiDocsGenerator\OcctApiDocsGenerator.csproj"

if (-not (Test-Path $ContractPath -PathType Leaf)) { throw "Bridge contract file was not found: $ContractPath" }
$Contract = Get-Content $ContractPath -Raw -Encoding UTF8 | ConvertFrom-Json
$BridgeVersion = [string]$Contract.bridgeVersion
$Author = [string]$Contract.author
$RequiredOcctVersion = [string]$Contract.occtVersion
$TargetFramework = [string]$Contract.dotnet.targetFramework
$SdkVersion = [string]$Contract.dotnet.sdkVersion

$Projects = [ordered]@{
    Core = "src\OcctNet\OcctNet.csproj"
    Avalonia = "src\OcctNet.Avalonia\OcctNet.Avalonia.csproj"
    ManagedTests = "tests\OcctNet.ManagedTests\OcctNet.ManagedTests.csproj"
    Smoke = "tests\OcctNet.Smoke\OcctNet.Smoke.csproj"
    AvaloniaSmoke = "tests\OcctNet.AvaloniaSmoke\OcctNet.AvaloniaSmoke.csproj"
}

$Checks = [ordered]@{
    Version = "tests\check-version-contract.ps1"
    Architecture = "tests\check-architecture-boundaries.ps1"
    BulkAbi = "tests\check-bulk-abi.ps1"
    NativeBuild = "tests\check-native-build-structure.ps1"
    ApiSurface = "tests\check-api-surface.ps1"
}

function Assert-Path {
    param([string]$Path)
    if (-not (Test-Path $Path)) { throw "Required path was not found: $Path" }
}

function Assert-Command {
    param([string]$Name)
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) { throw "$Name was not found in PATH." }
}

function Invoke-Checked {
    param([string]$Command, [object[]]$Arguments, [string]$ErrorMessage)
    & $Command @Arguments
    if ($LASTEXITCODE -ne 0) { throw $ErrorMessage }
}

function Invoke-ContractChecks {
    foreach ($check in $Checks.GetEnumerator()) {
        $path = Join-Path $RepoRoot $check.Value
        Assert-Path $path
        Write-Host ("[{0}] Running {1}..." -f $check.Key.ToLowerInvariant(), $check.Value) -ForegroundColor Cyan
        & $path -RepositoryRoot $RepoRoot
        if (-not $?) { throw "$($check.Key) validation failed." }
    }
}

function Resolve-OcctConfiguration {
    $script:OcctRoot = [System.IO.Path]::GetFullPath($OcctRoot)
    if (-not (Test-Path $script:OcctRoot -PathType Container)) {
        throw "OCCT SDK root was not found: $script:OcctRoot. Set OCCT_ROOT, pass -OcctRoot <path>, or install OCCT at $DefaultOcctRoot."
    }
    $script:OcctIncludeDir = Join-Path $script:OcctRoot "inc"
    $script:OcctLibDir = Join-Path $script:OcctRoot "win64\vc14\lib"
    $script:OcctBinDir = Join-Path $script:OcctRoot "win64\vc14\bin"
    foreach ($path in @(
        $script:OcctIncludeDir, $script:OcctLibDir, $script:OcctBinDir,
        (Join-Path $script:OcctIncludeDir "Standard.hxx"),
        (Join-Path $script:OcctLibDir "TKernel.lib"),
        (Join-Path $script:OcctBinDir "TKernel.dll")
    )) { Assert-Path $path }
}

function Build-Native {
    if (-not $IsWindows) { throw "build.ps1 is the Windows build entry point. Use ./build.sh on Linux." }
    Assert-Command "cmake"
    Resolve-OcctConfiguration
    Write-Host "[native] Configuring Windows x64 / OCCT $RequiredOcctVersion..." -ForegroundColor Cyan
    Invoke-Checked "cmake" @(
        "-S", $NativeSource, "-B", $NativeBuild,
        "-G", "Visual Studio 17 2022", "-A", "x64",
        "-DOCCT_ROOT=$script:OcctRoot",
        "-DOCCT_INCLUDE_DIR=$script:OcctIncludeDir",
        "-DOCCT_LIB_DIR=$script:OcctLibDir",
        "-DOCCT_BIN_DIR=$script:OcctBinDir"
    ) "CMake configure failed."
    Invoke-Checked "cmake" @("--build", $NativeBuild, "--config", $Configuration, "--parallel") "Native build failed."
    Assert-Path $NativeDll
    Write-Host "Native: $NativeDll" -ForegroundColor Green
}

function Build-Project {
    param([string]$Name)
    Assert-Command "dotnet"
    $relativePath = $Projects[$Name]
    if ([string]::IsNullOrWhiteSpace($relativePath)) { throw "Unknown project key: $Name" }
    $project = Join-Path $RepoRoot $relativePath
    Assert-Path $project
    Write-Host "[$($Name.ToLowerInvariant())] Building $Configuration / $BridgeVersion..." -ForegroundColor Cyan
    Invoke-Checked "dotnet" @("build", $project, "-c", $Configuration, "-p:Platform=x64", "-p:Version=$BridgeVersion", "--nologo") "$Name build failed."
}

function Build-Managed {
    Build-Project "Core"
    Build-Project "Avalonia"
}

function Run-ManagedTests {
    Assert-Command "dotnet"
    $project = Join-Path $RepoRoot $Projects.ManagedTests
    Build-Project "ManagedTests"
    Write-Host "[managed-tests] Running cross-platform Core regression tests..." -ForegroundColor Cyan
    Invoke-Checked "dotnet" @("test", $project, "-c", $Configuration, "-p:Platform=x64", "-p:Version=$BridgeVersion", "--no-build") "Managed bridge regression tests failed."
}

function Invoke-WithNativeRuntime {
    param(
        [Parameter(Mandatory = $true)][string]$ProjectKey,
        [Parameter(Mandatory = $true)][string]$Label,
        [Parameter(Mandatory = $true)][string]$ErrorMessage
    )

    Assert-Path $NativeDll
    Resolve-OcctConfiguration
    Build-Project $ProjectKey
    $project = Join-Path $RepoRoot $Projects[$ProjectKey]
    $output = Join-Path (Split-Path -Parent $project) "bin\x64\$Configuration\$TargetFramework"
    Copy-Item $NativeDll (Join-Path $output "OcctNative.dll") -Force

    $previousNativeDirectory = $env:OCCT_BRIDGE_NATIVE_DIR
    $previousOcctRoot = $env:OCCT_ROOT
    try {
        $env:OCCT_BRIDGE_NATIVE_DIR = $output
        $env:OCCT_ROOT = $script:OcctRoot
        Write-Host "[$Label] Running..." -ForegroundColor Cyan
        Invoke-Checked "dotnet" @("run", "--project", $project, "-c", $Configuration, "-p:Platform=x64", "-p:Version=$BridgeVersion", "--no-build") $ErrorMessage
    }
    finally {
        $env:OCCT_BRIDGE_NATIVE_DIR = $previousNativeDirectory
        $env:OCCT_ROOT = $previousOcctRoot
    }
}

function Run-Smoke {
    Invoke-WithNativeRuntime -ProjectKey "Smoke" -Label "smoke" -ErrorMessage "Smoke test failed."
}

function Run-AvaloniaSmoke {
    Invoke-WithNativeRuntime -ProjectKey "AvaloniaSmoke" -Label "avalonia-smoke" -ErrorMessage "Avalonia viewer smoke failed."
}

function Generate-ApiDocumentation {
    Assert-Command "dotnet"
    Assert-Path $ApiDocsGenerator
    Build-Managed
    Invoke-Checked "dotnet" @("run", "--project", $ApiDocsGenerator, "-c", "Release", "--", "--repository-root", $RepoRoot, "--configuration", $Configuration) "API documentation generation failed."
}

function Clean-Outputs {
    Remove-Item (Join-Path $RepoRoot "build") -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item (Join-Path $RepoRoot "artifacts") -Recurse -Force -ErrorAction SilentlyContinue
    foreach ($relativePath in $Projects.Values) {
        $dir = Split-Path -Parent (Join-Path $RepoRoot $relativePath)
        Remove-Item (Join-Path $dir "bin") -Recurse -Force -ErrorAction SilentlyContinue
        Remove-Item (Join-Path $dir "obj") -Recurse -Force -ErrorAction SilentlyContinue
    }
    $toolDir = Split-Path -Parent $ApiDocsGenerator
    Remove-Item (Join-Path $toolDir "bin") -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item (Join-Path $toolDir "obj") -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "Generated build outputs removed." -ForegroundColor Green
}

Write-Host "Target:        $Target"
Write-Host "Configuration: $Configuration"
Write-Host "Bridge:        $BridgeVersion"
Write-Host "Author:        $Author"
Write-Host "SDK:           $SdkVersion" -ForegroundColor DarkGray
Write-Host "Platform:      Windows x64 / Avalonia" -ForegroundColor DarkGray
Write-Host "OCCT root:     $OcctRoot" -ForegroundColor DarkGray

if ($Target -eq "clean") { Clean-Outputs; exit 0 }
Invoke-ContractChecks

switch ($Target) {
    "validate" { }
    "native" { Build-Native }
    "managed" { Build-Managed }
    "test" { Run-ManagedTests }
    "smoke" { Build-Native; Build-Managed; Run-Smoke }
    "avalonia-smoke" { Build-Native; Build-Managed; Run-AvaloniaSmoke }
    "docs" { Generate-ApiDocumentation }
    "all" { Build-Native; Build-Managed; Run-ManagedTests; Run-Smoke }
}
Write-Host "Build completed." -ForegroundColor Green
