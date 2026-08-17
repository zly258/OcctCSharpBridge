param(
    [Parameter(Position = 0)]
    [ValidateSet("validate", "native", "managed", "test", "smoke", "viewport-smoke", "dist", "clean", "all")]
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
if ([string]::IsNullOrWhiteSpace($OcctRoot)) { $OcctRoot = $DefaultOcctRoot }

$NativeSource = Join-Path $RepoRoot "src\OcctNative"
$NativeBuild = Join-Path $RepoRoot "build\native"
$NativeDll = Join-Path $NativeBuild "bin\$Configuration\OcctNative.dll"
$ContractPath = Join-Path $RepoRoot "bridge-contract.json"
$DistParent = Join-Path $RepoRoot "dist"
$DistRoot = Join-Path $DistParent "win-x64"
$DistStaging = Join-Path $DistParent ".win-x64-staging"
$DistBackup = Join-Path $DistParent ".win-x64-backup"

if (-not (Test-Path $ContractPath -PathType Leaf)) { throw "Bridge contract file was not found: $ContractPath" }
$Contract = Get-Content $ContractPath -Raw -Encoding UTF8 | ConvertFrom-Json
$BridgeVersion = [string]$Contract.bridgeVersion
$Author = [string]$Contract.author
$RequiredOcctVersion = [string]$Contract.occtVersion
$TargetFramework = [string]$Contract.dotnet.targetFramework
$DesktopTargetFramework = [string]$Contract.dotnet.desktopTargetFramework
$SdkVersion = [string]$Contract.dotnet.sdkVersion
$SdkRollForward = [string]$Contract.dotnet.sdkRollForward
try { $SdkBaseline = [version]$SdkVersion }
catch { throw "Bridge contract contains an invalid .NET SDK baseline: $SdkVersion" }

$Projects = [ordered]@{
    Core = "src\OcctNet\OcctNet.csproj"
    WinForms = "src\OcctNet.WinForms\OcctNet.WinForms.csproj"
    Wpf = "src\OcctNet.Wpf\OcctNet.Wpf.csproj"
    Avalonia = "src\OcctNet.Avalonia\OcctNet.Avalonia.csproj"
    ManagedTests = "tests\OcctNet.ManagedTests\OcctNet.ManagedTests.csproj"
    Smoke = "tests\OcctNet.Smoke\OcctNet.Smoke.csproj"
    WinFormsSmoke = "tests\OcctNet.WinFormsSmoke\OcctNet.WinFormsSmoke.csproj"
    WpfSmoke = "tests\OcctNet.WpfSmoke\OcctNet.WpfSmoke.csproj"
    AvaloniaSmoke = "tests\OcctNet.AvaloniaSmoke\OcctNet.AvaloniaSmoke.csproj"
}

$Checks = [ordered]@{
    Version = "tests\check-version-contract.ps1"
    Architecture = "tests\check-architecture-boundaries.ps1"
    Abi5 = "tests\check-abi5-contract.ps1"
    BulkAbi = "tests\check-bulk-abi.ps1"
    NativeBuild = "tests\check-native-build-structure.ps1"
    ApiSurface = "tests\check-api-surface.ps1"
}

$script:DotNetCommand = $null
$script:ResolvedSdkVersion = $null

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

function Get-DotNetCandidates {
    $result = [System.Collections.Generic.List[string]]::new()
    $seen = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)

    foreach ($root in @($env:DOTNET_ROOT, $env:ProgramW6432, $env:ProgramFiles)) {
        if ([string]::IsNullOrWhiteSpace($root)) { continue }
        $candidate = if ((Split-Path -Leaf $root) -ieq "dotnet") {
            Join-Path $root "dotnet.exe"
        }
        else {
            Join-Path $root "dotnet\dotnet.exe"
        }
        if ($seen.Add($candidate)) { $result.Add($candidate) }
    }

    $command = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($null -ne $command -and -not [string]::IsNullOrWhiteSpace([string]$command.Source)) {
        $candidate = [string]$command.Source
        if ($seen.Add($candidate)) { $result.Add($candidate) }
    }

    return $result
}

function Resolve-DotNetSdk {
    if (-not [string]::IsNullOrWhiteSpace($script:DotNetCommand)) { return }

    $diagnostics = [System.Collections.Generic.List[string]]::new()
    foreach ($candidate in @(Get-DotNetCandidates)) {
        if (-not (Test-Path $candidate -PathType Leaf)) {
            $diagnostics.Add("$candidate => not found")
            continue
        }

        $sdkLines = @(& $candidate --list-sdks 2>&1)
        $listExitCode = $LASTEXITCODE
        if ($listExitCode -ne 0) {
            $diagnostics.Add("$candidate => --list-sdks failed with exit code $listExitCode")
            continue
        }

        $installed = @($sdkLines | ForEach-Object {
            $line = [string]$_
            if ($line -match '^\s*([^\s]+)\s+\[') { $Matches[1] }
        })
        $installedText = if ($installed.Count -eq 0) { "no SDKs" } else { $installed -join ", " }
        $diagnostics.Add("$candidate => $installedText")

        Push-Location $RepoRoot
        try {
            $resolvedOutput = @(& $candidate --version 2>&1)
            $resolvedExitCode = $LASTEXITCODE
        }
        finally {
            Pop-Location
        }

        if ($resolvedExitCode -ne 0 -or $resolvedOutput.Count -ne 1) { continue }
        $resolvedVersion = ([string]$resolvedOutput[0]).Trim()
        try { $resolvedSdk = [version]$resolvedVersion }
        catch { continue }

        if ($resolvedSdk.Major -ne $SdkBaseline.Major -or
            $resolvedSdk.Minor -ne $SdkBaseline.Minor -or
            $resolvedSdk -lt $SdkBaseline) {
            continue
        }

        $script:DotNetCommand = [System.IO.Path]::GetFullPath($candidate)
        $script:ResolvedSdkVersion = $resolvedVersion
        return
    }

    $detail = if ($diagnostics.Count -eq 0) { "No dotnet host candidates were found." } else { $diagnostics -join [Environment]::NewLine }
    throw "OcctCSharpBridge requires a stable .NET $($SdkBaseline.Major).$($SdkBaseline.Minor) SDK at or above baseline $SdkVersion using '$SdkRollForward' roll-forward, but no usable dotnet host could resolve one from this repository.`nChecked dotnet hosts:`n$detail`nInstall a compatible x64 .NET 10 SDK or fix DOTNET_ROOT/PATH so C:\Program Files\dotnet\dotnet.exe can see it."
}

function Invoke-DotNetChecked {
    param(
        [Parameter(Mandatory = $true)][object[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$ErrorMessage
    )
    Resolve-DotNetSdk
    Push-Location $RepoRoot
    try {
        Invoke-Checked $script:DotNetCommand $Arguments $ErrorMessage
    }
    finally {
        Pop-Location
    }
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
        throw "OCCT SDK root was not found: $script:OcctRoot. Set OCCT_ROOT, pass -OcctRoot <path>, or install OCCT at $DefaultOcctRoot. validate/managed/test do not require OCCT."
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
    Write-Host "[native] Configuring OCCT $RequiredOcctVersion / ABI $($Contract.nativeAbi.current)..." -ForegroundColor Cyan
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
    $relativePath = $Projects[$Name]
    if ([string]::IsNullOrWhiteSpace($relativePath)) { throw "Unknown project key: $Name" }
    $project = Join-Path $RepoRoot $relativePath
    Assert-Path $project

    Write-Host "[$($Name.ToLowerInvariant())] Building $Configuration / $BridgeVersion..." -ForegroundColor Cyan
    Invoke-DotNetChecked @(
        "build", $project,
        "-c", $Configuration,
        "-p:Platform=x64",
        "-p:Version=$BridgeVersion",
        "--nologo"
    ) "$Name build failed."
}

function Run-ManagedTests {
    $project = Join-Path $RepoRoot $Projects.ManagedTests
    Assert-Path $project
    Write-Host "[managed-tests] Running managed-only ABI5 regression tests..." -ForegroundColor Cyan
    Invoke-DotNetChecked @(
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

function Get-OcctRuntimeDirectories {
    Resolve-OcctConfiguration
    $runtimeDirectories = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    [void]$runtimeDirectories.Add($script:OcctBinDir)
    $thirdPartyRoot = Join-Path $script:OcctRoot "3rdparty-vc14-64"
    if (Test-Path $thirdPartyRoot -PathType Container) {
        foreach ($dll in @(Get-ChildItem -LiteralPath $thirdPartyRoot -Filter "*.dll" -File -Recurse -ErrorAction SilentlyContinue)) {
            [void]$runtimeDirectories.Add($dll.DirectoryName)
        }
    }
    return @($runtimeDirectories)
}

function Invoke-WithOcctRuntime {
    param(
        [Parameter(Mandatory = $true)][string]$NativeDirectory,
        [Parameter(Mandatory = $true)][scriptblock]$Action
    )

    Assert-Path $NativeDirectory
    $runtimeDirectories = @(Get-OcctRuntimeDirectories)
    $previousPath = $env:PATH
    $previousNativeDirectory = $env:OCCT_BRIDGE_NATIVE_DIR
    $previousOcctRoot = $env:OCCT_ROOT
    $previousCasRoot = $env:CASROOT
    try {
        $env:PATH = (@($NativeDirectory) + $runtimeDirectories + @($previousPath)) -join [System.IO.Path]::PathSeparator
        $env:OCCT_BRIDGE_NATIVE_DIR = $NativeDirectory
        $env:OCCT_ROOT = $script:OcctRoot
        $env:CASROOT = $script:OcctRoot
        & $Action
    }
    finally {
        $env:PATH = $previousPath
        $env:OCCT_BRIDGE_NATIVE_DIR = $previousNativeDirectory
        $env:OCCT_ROOT = $previousOcctRoot
        $env:CASROOT = $previousCasRoot
    }
}

function Prepare-SmokeOutput {
    param(
        [Parameter(Mandatory = $true)][string]$ProjectKey,
        [Parameter(Mandatory = $true)][string]$Framework
    )

    $project = Join-Path $RepoRoot $Projects[$ProjectKey]
    Assert-Path $project
    $output = Join-Path (Split-Path -Parent $project) "bin\x64\$Configuration\$Framework"
    Assert-Path $output
    Copy-Item $NativeDll (Join-Path $output "OcctNative.dll") -Force
    return $output
}

function Run-Smoke {
    Assert-Path $NativeDll
    Build-Project "Smoke"
    $smokeProject = Join-Path $RepoRoot $Projects.Smoke
    $smokeOutput = Prepare-SmokeOutput "Smoke" $TargetFramework

    Invoke-WithOcctRuntime $smokeOutput {
        Write-Host "[smoke] Running ABI5 native modeling scenarios..." -ForegroundColor Cyan
        Invoke-DotNetChecked @(
            "run",
            "--project", $smokeProject,
            "-c", $Configuration,
            "-p:Platform=x64",
            "-p:Version=$BridgeVersion",
            "--no-build"
        ) "Smoke test failed."
    }
}

function Run-ViewportSmokeProject {
    param(
        [Parameter(Mandatory = $true)][string]$ProjectKey,
        [Parameter(Mandatory = $true)][string]$Framework
    )

    Assert-Path $NativeDll
    Build-Project $ProjectKey
    $project = Join-Path $RepoRoot $Projects[$ProjectKey]
    $output = Prepare-SmokeOutput $ProjectKey $Framework

    Invoke-WithOcctRuntime $output {
        Write-Host "[$($ProjectKey.ToLowerInvariant())] Running native viewport lifecycle/render smoke..." -ForegroundColor Cyan
        Invoke-DotNetChecked @(
            "run",
            "--project", $project,
            "-c", $Configuration,
            "-p:Platform=x64",
            "-p:Version=$BridgeVersion",
            "--no-build"
        ) "$ProjectKey failed."
    }
}

function Run-ViewportSmokes {
    Run-ViewportSmokeProject "WinFormsSmoke" $DesktopTargetFramework
    Run-ViewportSmokeProject "WpfSmoke" $DesktopTargetFramework
    Run-ViewportSmokeProject "AvaloniaSmoke" $TargetFramework
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
    Write-Host "Generated build outputs removed." -ForegroundColor Green
}

function Assert-CleanSourceTree {
    Assert-Command "git"
    $changes = @(& git -C $RepoRoot status --porcelain --untracked-files=all)
    if ($LASTEXITCODE -ne 0) { throw "Failed to inspect the Git working tree." }
    if ($changes.Count -gt 0) { throw "The working tree is not clean. Commit or remove source/configuration changes before producing dist/win-x64." }
    $commit = (& git -C $RepoRoot rev-parse HEAD 2>$null)
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($commit)) { throw "Failed to resolve the source commit." }
    return $commit.Trim()
}

function Build-BinaryDistribution {
    if ($Configuration -ne "Release") { throw "Binary SDK distribution is Release-only. Run: .\build.ps1 dist Release" }
    $sourceCommit = Assert-CleanSourceTree
    Write-Host "[dist] Source commit: $sourceCommit" -ForegroundColor DarkGray

    Build-Native
    Build-Managed

    $files = [ordered]@{
        "OcctNative.dll" = Join-Path $RepoRoot "build\native\bin\Release\OcctNative.dll"
        "OcctNet.dll" = Join-Path $RepoRoot "src\OcctNet\bin\x64\Release\$TargetFramework\OcctNet.dll"
        "OcctNet.WinForms.dll" = Join-Path $RepoRoot "src\OcctNet.WinForms\bin\x64\Release\$DesktopTargetFramework\OcctNet.WinForms.dll"
        "OcctNet.Wpf.dll" = Join-Path $RepoRoot "src\OcctNet.Wpf\bin\x64\Release\$DesktopTargetFramework\OcctNet.Wpf.dll"
        "OcctNet.Avalonia.dll" = Join-Path $RepoRoot "src\OcctNet.Avalonia\bin\x64\Release\$TargetFramework\OcctNet.Avalonia.dll"
    }
    foreach ($source in $files.Values) { Assert-Path $source }

    Remove-Item $DistStaging -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item $DistBackup -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $DistStaging -Force | Out-Null

    try {
        foreach ($entry in $files.GetEnumerator()) { Copy-Item $entry.Value (Join-Path $DistStaging $entry.Key) -Force }

        $distributionContract = Get-Content $ContractPath -Raw -Encoding UTF8 | ConvertFrom-Json
        $distributionContract.platform = "win-x64"
        $distributionContractJson = $distributionContract | ConvertTo-Json -Depth 16
        [System.IO.File]::WriteAllText((Join-Path $DistStaging "bridge-contract.json"), $distributionContractJson + [Environment]::NewLine, $utf8)

        $manifestFiles = @()
        foreach ($name in @($files.Keys) + @("bridge-contract.json")) {
            $path = Join-Path $DistStaging $name
            $manifestFiles += [ordered]@{
                name = $name
                sha256 = (Get-FileHash $path -Algorithm SHA256).Hash.ToLowerInvariant()
            }
        }

        $manifest = [ordered]@{
            schemaVersion = 2
            author = $Author
            bridgeVersion = $BridgeVersion
            nativeAbi = [ordered]@{
                current = [int]$Contract.nativeAbi.current
                minimumSupported = [int]$Contract.nativeAbi.minimumSupported
            }
            occtVersion = $RequiredOcctVersion
            platform = "win-x64"
            targetFramework = $TargetFramework
            sdkVersion = $SdkVersion
            sdkRollForward = $SdkRollForward
            languageVersion = [string]$Contract.dotnet.languageVersion
            configuration = "Release"
            sourceCommit = $sourceCommit
            files = $manifestFiles
        }
        [System.IO.File]::WriteAllText(
            (Join-Path $DistStaging "bridge-manifest.json"),
            ($manifest | ConvertTo-Json -Depth 8) + [Environment]::NewLine,
            $utf8)

        $hadPrevious = Test-Path $DistRoot -PathType Container
        if ($hadPrevious) { Move-Item $DistRoot $DistBackup }
        try { Move-Item $DistStaging $DistRoot }
        catch {
            if ($hadPrevious -and (Test-Path $DistBackup -PathType Container)) { Move-Item $DistBackup $DistRoot }
            throw
        }
        Remove-Item $DistBackup -Recurse -Force -ErrorAction SilentlyContinue
    }
    finally {
        Remove-Item $DistStaging -Recurse -Force -ErrorAction SilentlyContinue
        if (-not (Test-Path $DistRoot -PathType Container) -and (Test-Path $DistBackup -PathType Container)) { Move-Item $DistBackup $DistRoot }
    }

    Write-Host "[dist] Binary SDK updated: $DistRoot" -ForegroundColor Green
}

Write-Host "Target:        $Target"
Write-Host "Configuration: $Configuration"
Write-Host "Bridge:        $BridgeVersion"
Write-Host "ABI:           $($Contract.nativeAbi.current) only"
Write-Host "Author:        $Author"
Write-Host "SDK contract:  $SdkVersion + $SdkRollForward" -ForegroundColor DarkGray
$occtRootSource = if ($env:OCCT_ROOT) { "environment" } elseif ($OcctRoot -eq $DefaultOcctRoot) { "default" } else { "argument" }
Write-Host "OCCT root:     $OcctRoot ($occtRootSource)" -ForegroundColor DarkGray

if ($Target -eq "clean") {
    Clean-Outputs
    Write-Host "Build completed." -ForegroundColor Green
    exit 0
}

if ($Target -in @("managed", "test", "smoke", "viewport-smoke", "dist", "all")) {
    Resolve-DotNetSdk
    Write-Host "dotnet:        $script:DotNetCommand" -ForegroundColor DarkGray
    Write-Host "SDK resolved:  $script:ResolvedSdkVersion" -ForegroundColor Green
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
    "dist" { Build-BinaryDistribution }
    "smoke" {
        Build-Native
        Build-Managed
        Run-Smoke
    }
    "viewport-smoke" {
        Build-Native
        Build-Managed
        Run-ViewportSmokes
    }
    "all" {
        Build-Native
        Build-Managed
        Build-Project "ManagedTests"
        Run-ManagedTests
        Run-Smoke
        Run-ViewportSmokes
    }
}

Write-Host "Build completed." -ForegroundColor Green
