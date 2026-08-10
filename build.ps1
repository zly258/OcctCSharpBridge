param(
    [Parameter(Position = 0)]
    [ValidateSet("validate", "native", "managed", "test", "pack", "smoke", "docs", "dist", "ci", "clean", "all")]
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
$PackageOutput = Join-Path $RepoRoot "artifacts\packages"
$ContractPath = Join-Path $RepoRoot "bridge-contract.json"
$DistParent = Join-Path $RepoRoot "dist"
$DistRoot = Join-Path $DistParent "win-x64"
$DistStaging = Join-Path $DistParent ".win-x64-staging"
$DistBackup = Join-Path $DistParent ".win-x64-backup"
$ApiDocsGenerator = Join-Path $RepoRoot "tools\OcctApiDocsGenerator\OcctApiDocsGenerator.csproj"

if (-not (Test-Path $ContractPath -PathType Leaf)) {
    throw "Bridge contract file was not found: $ContractPath"
}
$Contract = Get-Content $ContractPath -Raw -Encoding UTF8 | ConvertFrom-Json
$BridgeVersion = [string]$Contract.bridgeVersion
$Author = [string]$Contract.author
$RequiredOcctVersion = [string]$Contract.occtVersion
$TargetFramework = [string]$Contract.dotnet.targetFramework
$SdkVersion = [string]$Contract.dotnet.sdkVersion

$Projects = [ordered]@{
    Core = "src\OcctNet\OcctNet.csproj"
    WinForms = "src\OcctNet.WinForms\OcctNet.WinForms.csproj"
    Wpf = "src\OcctNet.Wpf\OcctNet.Wpf.csproj"
    Avalonia = "src\OcctNet.Avalonia\OcctNet.Avalonia.csproj"
    ManagedTests = "tests\OcctNet.ManagedTests\OcctNet.ManagedTests.csproj"
    Smoke = "tests\OcctNet.Smoke\OcctNet.Smoke.csproj"
}

$PackageProjects = @("Core", "WinForms", "Wpf", "Avalonia")

# Static checks are intentionally limited to repository-level invariants that
# cannot be expressed more reliably by compilation, managed tests, or native smoke tests.
$Checks = [ordered]@{
    Version = "tests\check-version-contract.ps1"
    Architecture = "tests\check-architecture-boundaries.ps1"
    BulkAbi = "tests\check-bulk-abi.ps1"
    NativeBuild = "tests\check-native-build-structure.ps1"
    ApiSurface = "tests\check-api-surface.ps1"
    SdkPackage = "tests\check-sdk-package.ps1"
}

function Assert-Path {
    param([Parameter(Mandatory = $true)][string]$Path)
    if (-not (Test-Path $Path)) { throw "Required path was not found: $Path" }
}

function Assert-Command {
    param([Parameter(Mandatory = $true)][string]$Name)
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) { throw "$Name was not found in PATH." }
}

function Invoke-Checked {
    param(
        [Parameter(Mandatory = $true)][string]$Command,
        [Parameter(Mandatory = $true)][object[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$ErrorMessage
    )
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
        throw "OCCT SDK root was not found: $script:OcctRoot. Set OCCT_ROOT, pass -OcctRoot <path>, or install OCCT at $DefaultOcctRoot. validate/managed/test/pack/docs/ci do not require OCCT."
    }
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
    )) { Assert-Path $path }
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
    Invoke-Checked "cmake" @("--build", $NativeBuild, "--config", $Configuration, "--parallel") "Native build failed."
    Assert-Path $NativeDll
    Write-Host "Native: $NativeDll" -ForegroundColor Green
}

function Build-Project {
    param([Parameter(Mandatory = $true)][string]$Name)

    Assert-Command "dotnet"
    $relativePath = $Projects[$Name]
    if ([string]::IsNullOrWhiteSpace($relativePath)) { throw "Unknown project key: $Name" }

    $project = Join-Path $RepoRoot $relativePath
    Assert-Path $project

    Write-Host "[$($Name.ToLowerInvariant())] Building $Configuration / $BridgeVersion..." -ForegroundColor Cyan
    Invoke-Checked "dotnet" @(
        "build", $project,
        "-c", $Configuration,
        "-p:Platform=x64",
        "-p:Version=$BridgeVersion",
        "-p:PackageVersion=$BridgeVersion",
        "--nologo"
    ) "$Name build failed."
}

function Run-ManagedTests {
    Assert-Command "dotnet"
    $project = Join-Path $RepoRoot $Projects.ManagedTests
    Assert-Path $project

    Write-Host "[managed-tests] Running managed-only bridge regression tests..." -ForegroundColor Cyan
    Invoke-Checked "dotnet" @(
        "test", $project,
        "-c", $Configuration,
        "-p:Platform=x64",
        "-p:Version=$BridgeVersion",
        "--no-build"
    ) "Managed bridge regression tests failed."
}

function Build-Managed {
    Build-Project "Core"
    Build-Project "WinForms"
    Build-Project "Wpf"
    Build-Project "Avalonia"
}

function Clean-Outputs {
    Write-Host "[clean] Removing generated build outputs..." -ForegroundColor Cyan

    Remove-Item (Join-Path $RepoRoot "build") -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item (Join-Path $RepoRoot "artifacts") -Recurse -Force -ErrorAction SilentlyContinue

    foreach ($relativePath in $Projects.Values) {
        $project = Join-Path $RepoRoot $relativePath
        $projectDirectory = Split-Path -Parent $project
        Remove-Item (Join-Path $projectDirectory "bin") -Recurse -Force -ErrorAction SilentlyContinue
        Remove-Item (Join-Path $projectDirectory "obj") -Recurse -Force -ErrorAction SilentlyContinue
    }

    if (Test-Path $ApiDocsGenerator -PathType Leaf) {
        $toolDirectory = Split-Path -Parent $ApiDocsGenerator
        Remove-Item (Join-Path $toolDirectory "bin") -Recurse -Force -ErrorAction SilentlyContinue
        Remove-Item (Join-Path $toolDirectory "obj") -Recurse -Force -ErrorAction SilentlyContinue
    }

    Write-Host "Generated build outputs removed." -ForegroundColor Green
}

function Test-ManagedPackage {
    param(
        [Parameter(Mandatory = $true)][string]$PackagePath,
        [Parameter(Mandatory = $true)][string]$AssemblyName
    )

    $archive = [System.IO.Compression.ZipFile]::OpenRead($PackagePath)
    try {
        $entries = @($archive.Entries | ForEach-Object { $_.FullName.Replace('\', '/') })
        $nativeLeak = @($entries | Where-Object {
            $_ -match '(^|/)OcctNative\.dll$' -or
            $_ -match '(^|/)TK[^/]*\.dll$' -or
            $_.StartsWith('runtimes/', [StringComparison]::OrdinalIgnoreCase)
        })
        if ($nativeLeak.Count -gt 0) { throw "Managed package contains native runtime content: $($nativeLeak -join ', ')" }

        $managedDll = @($entries | Where-Object { $_ -match "^lib/.+/$([regex]::Escape($AssemblyName))\.dll$" })
        $xmlDocs = @($entries | Where-Object { $_ -match "^lib/.+/$([regex]::Escape($AssemblyName))\.xml$" })
        if ($managedDll.Count -ne 1) { throw "Managed package does not contain exactly one $AssemblyName.dll under lib/." }
        if ($xmlDocs.Count -ne 1) { throw "Managed package does not contain exactly one $AssemblyName.xml IntelliSense document under lib/." }
        if ('README.md' -notin $entries -or 'LICENSE' -notin $entries) { throw "Managed package must include README.md and LICENSE." }
    }
    finally { $archive.Dispose() }
}

function Pack-ManagedSdk {
    param([switch]$SkipBuild)

    Assert-Command "dotnet"
    if (-not $SkipBuild) { Build-Managed }

    Remove-Item $PackageOutput -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $PackageOutput -Force | Out-Null

    foreach ($name in $PackageProjects) {
        $project = Join-Path $RepoRoot $Projects[$name]
        Write-Host "[pack] Packing $name $BridgeVersion..." -ForegroundColor Cyan
        Invoke-Checked "dotnet" @(
            "pack", $project,
            "-c", $Configuration,
            "-p:Platform=x64",
            "-p:Version=$BridgeVersion",
            "-p:PackageVersion=$BridgeVersion",
            "--no-build",
            "--nologo",
            "-o", $PackageOutput
        ) "$name package creation failed."
    }

    $packages = [ordered]@{
        "OcctNet" = "OcctNet"
        "OcctNet.WinForms" = "OcctNet.WinForms"
        "OcctNet.Wpf" = "OcctNet.Wpf"
        "OcctNet.Avalonia" = "OcctNet.Avalonia"
    }
    foreach ($entry in $packages.GetEnumerator()) {
        $packagePath = Join-Path $PackageOutput "$($entry.Key).$BridgeVersion.nupkg"
        Assert-Path $packagePath
        Assert-Path (Join-Path $PackageOutput "$($entry.Key).$BridgeVersion.snupkg")
        Test-ManagedPackage -PackagePath $packagePath -AssemblyName $entry.Value
    }

    Write-Host "Packages: $PackageOutput" -ForegroundColor Green
    Write-Host "Managed packages validated: assemblies/XML docs only; no OCCT or OcctNative runtime bundled." -ForegroundColor Green
}

function Build-Ci {
    Build-Managed
    Build-Project "ManagedTests"
    Run-ManagedTests
    Build-Project "Smoke"
    Pack-ManagedSdk -SkipBuild
}

function Run-Smoke {
    Assert-Path $NativeDll
    Resolve-OcctConfiguration
    Build-Project "Smoke"

    $smokeProject = Join-Path $RepoRoot $Projects.Smoke
    $smokeOutput = Join-Path (Split-Path -Parent $smokeProject) "bin\x64\$Configuration\$TargetFramework"
    Copy-Item $NativeDll (Join-Path $smokeOutput "OcctNative.dll") -Force

    $previousNativeDirectory = $env:OCCT_BRIDGE_NATIVE_DIR
    $previousOcctRoot = $env:OCCT_ROOT
    try {
        $env:OCCT_BRIDGE_NATIVE_DIR = $smokeOutput
        $env:OCCT_ROOT = $script:OcctRoot
        Write-Host "[smoke] Running native modeling scenarios..." -ForegroundColor Cyan
        Invoke-Checked "dotnet" @(
            "run",
            "--project", $smokeProject,
            "-c", $Configuration,
            "-p:Platform=x64",
            "-p:Version=$BridgeVersion",
            "--no-build"
        ) "Smoke test failed."
    }
    finally {
        $env:OCCT_BRIDGE_NATIVE_DIR = $previousNativeDirectory
        $env:OCCT_ROOT = $previousOcctRoot
    }
}

function Generate-ApiDocumentation {
    Assert-Command "dotnet"
    Assert-Path $ApiDocsGenerator
    Build-Managed

    Write-Host "[docs] Generating bilingual public API reference..." -ForegroundColor Cyan
    Invoke-Checked "dotnet" @(
        "run",
        "--project", $ApiDocsGenerator,
        "-c", "Release",
        "--",
        "--repository-root", $RepoRoot,
        "--configuration", $Configuration
    ) "API documentation generation failed."
}

function Assert-CleanSourceTree {
    Assert-Command "git"
    $changes = @(& git -C $RepoRoot status --porcelain --untracked-files=all)
    if ($LASTEXITCODE -ne 0) { throw "Failed to inspect the Git working tree." }
    if ($changes.Count -gt 0) {
        throw "The working tree is not clean. Commit or remove source/configuration changes before producing dist/win-x64."
    }

    $commit = (& git -C $RepoRoot rev-parse HEAD 2>$null)
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($commit)) { throw "Failed to resolve the source commit." }
    return $commit.Trim()
}

function Build-BinaryDistribution {
    if ($Configuration -ne "Release") {
        throw "Binary SDK distribution is Release-only. Run: .\build.ps1 dist Release"
    }

    $sourceCommit = Assert-CleanSourceTree
    Write-Host "[dist] Source commit: $sourceCommit" -ForegroundColor DarkGray

    Build-Native
    Build-Managed
    Run-Smoke

    $files = [ordered]@{
        "OcctNative.dll" = Join-Path $RepoRoot "build\native\bin\Release\OcctNative.dll"
        "OcctNet.dll" = Join-Path $RepoRoot "src\OcctNet\bin\x64\Release\$TargetFramework\OcctNet.dll"
        "OcctNet.WinForms.dll" = Join-Path $RepoRoot "src\OcctNet.WinForms\bin\x64\Release\$TargetFramework\OcctNet.WinForms.dll"
        "OcctNet.Wpf.dll" = Join-Path $RepoRoot "src\OcctNet.Wpf\bin\x64\Release\$TargetFramework\OcctNet.Wpf.dll"
        "OcctNet.Avalonia.dll" = Join-Path $RepoRoot "src\OcctNet.Avalonia\bin\x64\Release\$TargetFramework\OcctNet.Avalonia.dll"
    }
    foreach ($source in $files.Values) { Assert-Path $source }

    Remove-Item $DistStaging -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item $DistBackup -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $DistStaging -Force | Out-Null

    try {
        foreach ($entry in $files.GetEnumerator()) {
            Copy-Item $entry.Value (Join-Path $DistStaging $entry.Key) -Force
        }
        Copy-Item $ContractPath (Join-Path $DistStaging "bridge-contract.json") -Force

        $manifestFiles = @()
        foreach ($name in @($files.Keys) + @("bridge-contract.json")) {
            $path = Join-Path $DistStaging $name
            $manifestFiles += [ordered]@{
                name = $name
                sha256 = (Get-FileHash $path -Algorithm SHA256).Hash.ToLowerInvariant()
            }
        }

        $manifest = [ordered]@{
            schemaVersion = 1
            author = $Author
            bridgeVersion = $BridgeVersion
            nativeAbiVersion = [int]$Contract.nativeAbiVersion
            occtVersion = $RequiredOcctVersion
            platform = [string]$Contract.platform
            targetFramework = $TargetFramework
            sdkVersion = $SdkVersion
            languageVersion = [string]$Contract.dotnet.languageVersion
            configuration = "Release"
            sourceCommit = $sourceCommit
            files = $manifestFiles
        }
        $manifestJson = $manifest | ConvertTo-Json -Depth 8
        [System.IO.File]::WriteAllText((Join-Path $DistStaging "bridge-manifest.json"), $manifestJson + [Environment]::NewLine, $utf8)

        $hadPrevious = Test-Path $DistRoot -PathType Container
        if ($hadPrevious) { Move-Item $DistRoot $DistBackup }
        try {
            Move-Item $DistStaging $DistRoot
        }
        catch {
            if ($hadPrevious -and (Test-Path $DistBackup -PathType Container)) { Move-Item $DistBackup $DistRoot }
            throw
        }
        Remove-Item $DistBackup -Recurse -Force -ErrorAction SilentlyContinue
    }
    finally {
        Remove-Item $DistStaging -Recurse -Force -ErrorAction SilentlyContinue
        if (-not (Test-Path $DistRoot -PathType Container) -and (Test-Path $DistBackup -PathType Container)) {
            Move-Item $DistBackup $DistRoot
        }
    }

    Write-Host "[dist] Binary SDK updated: $DistRoot" -ForegroundColor Green
}

Write-Host "Target:        $Target"
Write-Host "Configuration: $Configuration"
Write-Host "Bridge:        $BridgeVersion"
Write-Host "Author:        $Author"
Write-Host "SDK:           $SdkVersion" -ForegroundColor DarkGray
$occtRootSource = if ($env:OCCT_ROOT) { "environment" } elseif ($OcctRoot -eq $DefaultOcctRoot) { "default" } else { "argument" }
Write-Host "OCCT root:     $OcctRoot ($occtRootSource)" -ForegroundColor DarkGray

if ($Target -eq "clean") {
    Clean-Outputs
    Write-Host "Build completed." -ForegroundColor Green
    exit 0
}

Invoke-ContractChecks

switch ($Target) {
    "validate" { }
    "native" { Build-Native }
    "managed" { Build-Managed }
    "test" {
        Build-Project "ManagedTests"
        Run-ManagedTests
    }
    "pack" { Pack-ManagedSdk }
    "docs" { Generate-ApiDocumentation }
    "dist" { Build-BinaryDistribution }
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
