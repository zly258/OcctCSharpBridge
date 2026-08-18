param(
    [Parameter(Position = 0)]
    [ValidateSet("all", "winform", "wpf", "avalonia")]
    [string]$Target = "all",

    [Parameter(Position = 1)]
    [ValidateSet("Debug", "Release", "RelWithDebInfo")]
    [string]$Configuration = "Release",

    [string]$OutputDirectory = "",
    [switch]$SelfContained,
    [switch]$FrameworkDependent,
    [switch]$Zip,
    [switch]$KeepExisting
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

if ($SelfContained.IsPresent -and $FrameworkDependent.IsPresent) {
    throw "Use either -SelfContained or -FrameworkDependent, not both."
}
if ($Target -eq "all" -and $KeepExisting.IsPresent) {
    throw "-KeepExisting is not supported for unified 'all' publishing because the self-contained app layout must be rebuilt cleanly."
}

# Distribution packages are portable by default. Framework-dependent output is now an explicit opt-in.
$UseSelfContained = -not $FrameworkDependent.IsPresent
if ($SelfContained.IsPresent) { $UseSelfContained = $true }

$RunningOnWindows = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)
if (-not $RunningOnWindows) { throw "publish.ps1 supports Windows x64 only. Use ./publish.sh on Linux." }

$RepoRoot = Split-Path -Parent $PSCommandPath
$GlobalJsonPath = Join-Path $RepoRoot "global.json"
$BuildScript = Join-Path $RepoRoot "build.ps1"
$DistRoot = Join-Path $RepoRoot "dist\win-x64"
$PortableRoot = Join-Path $RepoRoot "dist\portable\win-x64"
$ContractPath = Join-Path $DistRoot "bridge-contract.json"
$ManifestPath = Join-Path $DistRoot "bridge-manifest.json"
$PortableManifestPath = Join-Path $PortableRoot "package-manifest.json"
if ([string]::IsNullOrWhiteSpace($OutputDirectory)) { $OutputDirectory = Join-Path $RepoRoot "artifacts\publish" }
$OutputDirectory = [System.IO.Path]::GetFullPath($OutputDirectory)
$UnifiedPackage = "CAD-Demo-win-x64"

$Projects = [ordered]@{
    winform = @{
        Name = "WinForms"
        Project = "src\OcctDemo.WinForms\OcctDemo.WinForms.csproj"
        Executable = "CAD-Winform.exe"
        Package = "CAD-Winform-win-x64"
    }
    wpf = @{
        Name = "WPF"
        Project = "src\OcctDemo.Wpf\OcctDemo.Wpf.csproj"
        Executable = "CAD-WPF.exe"
        Package = "CAD-WPF-win-x64"
    }
    avalonia = @{
        Name = "Avalonia"
        Project = "src\OcctDemo.Avalonia\OcctDemo.Avalonia.csproj"
        Executable = "CAD-Avalonia.exe"
        Package = "CAD-Avalonia-win-x64"
    }
}

function Assert-Path {
    param([Parameter(Mandatory = $true)][string]$Path)
    if (-not (Test-Path -LiteralPath $Path)) { throw "Required path was not found: $Path" }
}

function Resolve-DotNet {
    Assert-Path $GlobalJsonPath
    $globalJson = Get-Content -LiteralPath $GlobalJsonPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $baselineText = [string]$globalJson.sdk.version
    $rollForward = [string]$globalJson.sdk.rollForward
    try { $baseline = [version]$baselineText }
    catch { throw "global.json contains an invalid .NET SDK baseline: $baselineText" }
    if ($rollForward -ne "latestFeature") { throw "global.json must use rollForward=latestFeature." }
    if ([bool]$globalJson.sdk.allowPrerelease) { throw "global.json must not allow prerelease SDKs." }

    $candidates = @()
    foreach ($root in @($env:DOTNET_ROOT, $env:ProgramW6432, $env:ProgramFiles)) {
        if ([string]::IsNullOrWhiteSpace($root)) { continue }
        $candidate = if ((Split-Path -Leaf $root) -ieq "dotnet") { Join-Path $root "dotnet.exe" } else { Join-Path $root "dotnet\dotnet.exe" }
        if ($candidate -notin $candidates) { $candidates += $candidate }
    }
    $command = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($null -ne $command -and [string]$command.Source -notin $candidates) { $candidates += [string]$command.Source }
    foreach ($candidate in $candidates) {
        if (-not (Test-Path -LiteralPath $candidate -PathType Leaf)) { continue }
        Push-Location $RepoRoot
        try {
            $versionText = ([string](& $candidate --version 2>$null)).Trim()
            $exitCode = $LASTEXITCODE
        }
        finally { Pop-Location }
        if ($exitCode -ne 0 -or [string]::IsNullOrWhiteSpace($versionText) -or $versionText.Contains("-")) { continue }
        try { $version = [version]$versionText }
        catch { continue }
        if ($version.Major -eq $baseline.Major -and $version.Minor -eq $baseline.Minor -and $version -ge $baseline) {
            return [System.IO.Path]::GetFullPath($candidate)
        }
    }
    throw "A stable .NET 10 SDK compatible with baseline $baselineText / $rollForward was not found."
}

function Invoke-Checked {
    param([string]$Command, [object[]]$Arguments, [string]$ErrorMessage)
    & $Command @Arguments
    if ($LASTEXITCODE -ne 0) { throw $ErrorMessage }
}

function Test-PortableRuntime {
    Assert-Path $PortableManifestPath
    Assert-Path (Join-Path $PortableRoot "runtime\OcctNative.dll")
    Assert-Path (Join-Path $PortableRoot "occt\resources")

    $package = Get-Content -LiteralPath $PortableManifestPath -Raw -Encoding UTF8 | ConvertFrom-Json
    if ([string]$package.product -ne "OcctCSharpBridge Portable SDK" -or
        [string]$package.platform -ne "win-x64" -or
        -not [bool]$package.portableRuntime -or
        [string]$package.bridgeSourceCommit -ne [string]$script:Manifest.sourceCommit -or
        [string]$package.bridgeVersion -ne [string]$script:Contract.bridgeVersion) {
        throw "Synchronized Bridge portable runtime does not match dist/win-x64. Run .\sync.ps1 first."
    }

    foreach ($entry in @($package.files)) {
        $relative = ([string]$entry.name).Replace('/', [System.IO.Path]::DirectorySeparatorChar)
        $path = Join-Path $PortableRoot $relative
        Assert-Path $path
        $actual = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
        if ($actual -ne ([string]$entry.sha256).ToLowerInvariant()) {
            throw "Bridge portable runtime hash mismatch: $($entry.name)"
        }
    }
    $script:PortableManifest = $package
}

function Publish-ProjectToStaging {
    param(
        [Parameter(Mandatory = $true)][string]$Key,
        [Parameter(Mandatory = $true)][string]$StagingRoot
    )
    $definition = $Projects[$Key]
    $project = Join-Path $RepoRoot $definition.Project
    Assert-Path $project

    Remove-Item -LiteralPath $StagingRoot -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $StagingRoot -Force | Out-Null
    $mode = if ($UseSelfContained) { "self-contained" } else { "framework-dependent" }
    Write-Host "[publish] $($definition.Name) / $Configuration / $mode..." -ForegroundColor Cyan
    Invoke-Checked $script:DotNet @(
        "publish", $project,
        "-c", $Configuration,
        "-r", "win-x64",
        "-p:Platform=x64",
        "-p:Version=$($script:Contract.bridgeVersion)",
        "-p:DebugType=None",
        "-p:DebugSymbols=false",
        "--self-contained", $UseSelfContained.ToString().ToLowerInvariant(),
        "--nologo",
        "-o", $StagingRoot
    ) "$($definition.Name) publish failed."
    Assert-Path (Join-Path $StagingRoot $definition.Executable)
}

function Merge-PublishTree {
    param(
        [Parameter(Mandatory = $true)][string]$Source,
        [Parameter(Mandatory = $true)][string]$Destination,
        [string]$ManifestRename = ""
    )

    $sourceRoot = [System.IO.Path]::GetFullPath($Source).TrimEnd('\') + '\'
    foreach ($file in @(Get-ChildItem -LiteralPath $Source -File -Recurse)) {
        $relative = $file.FullName.Substring($sourceRoot.Length)
        if (-not [string]::IsNullOrWhiteSpace($ManifestRename) -and $relative -ieq "package-manifest.json") {
            $relative = $ManifestRename
        }
        $destinationPath = Join-Path $Destination $relative
        $destinationDirectory = Split-Path -Parent $destinationPath
        if (-not (Test-Path -LiteralPath $destinationDirectory -PathType Container)) {
            New-Item -ItemType Directory -Path $destinationDirectory -Force | Out-Null
        }
        if (Test-Path -LiteralPath $destinationPath -PathType Leaf) {
            $sourceHash = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
            $destinationHash = (Get-FileHash -LiteralPath $destinationPath -Algorithm SHA256).Hash
            if ($sourceHash -ne $destinationHash) {
                throw "Conflicting publish payload '$relative' differs from the validated source."
            }
            continue
        }
        Copy-Item -LiteralPath $file.FullName -Destination $destinationPath -Force
    }
}

function Assert-AppBridgeAssembliesMatch {
    param([Parameter(Mandatory = $true)][string]$AppRoot)

    foreach ($name in @("OcctNet.dll", "OcctNet.WinForms.dll", "OcctNet.Wpf.dll", "OcctNet.Avalonia.dll")) {
        $appPath = Join-Path $AppRoot $name
        if (-not (Test-Path -LiteralPath $appPath -PathType Leaf)) { continue }
        $portablePath = Join-Path $PortableRoot $name
        Assert-Path $portablePath
        $appHash = (Get-FileHash -LiteralPath $appPath -Algorithm SHA256).Hash
        $portableHash = (Get-FileHash -LiteralPath $portablePath -Algorithm SHA256).Hash
        if ($appHash -ne $portableHash) {
            throw "Published application contains a Bridge assembly that differs from the validated Portable SDK: $name"
        }
    }
}

function Prepare-AppDirectory {
    param([Parameter(Mandatory = $true)][string]$AppRoot)

    # The Bridge Native DLL must come from the shared validated Portable Runtime. Removing
    # app-local copies also prevents the resolver from choosing an incomplete native closure first.
    Remove-Item -LiteralPath (Join-Path $AppRoot "OcctNative.dll") -Force -ErrorAction SilentlyContinue
    Assert-AppBridgeAssembliesMatch $AppRoot
}

function Merge-BridgePortablePayload {
    param([Parameter(Mandatory = $true)][string]$PackageRoot)

    Merge-PublishTree -Source $PortableRoot -Destination $PackageRoot -ManifestRename "bridge-portable-manifest.json"
    Assert-Path (Join-Path $PackageRoot "runtime\OcctNative.dll")
    Assert-Path (Join-Path $PackageRoot "bridge-portable-manifest.json")
}

function Write-RunCommand {
    param(
        [Parameter(Mandatory = $true)][string]$Key,
        [Parameter(Mandatory = $true)][string]$PackageRoot,
        [Parameter(Mandatory = $true)][string]$FileName,
        [string]$AppRelativeDirectory = ""
    )

    $executable = [string]$Projects[$Key].Executable
    $appDirectoryLine = if ([string]::IsNullOrWhiteSpace($AppRelativeDirectory)) {
        'set "APP_DIR=%ROOT%"'
    }
    else {
        'set "APP_DIR=%ROOT%' + $AppRelativeDirectory.TrimEnd('\') + '\"'
    }

    $runCmd = @"
@echo off
setlocal
set "ROOT=%~dp0"
$appDirectoryLine
set "NATIVE=%ROOT%runtime"
set "CASROOT=%ROOT%occt"
set "OCCT_ROOT=%ROOT%occt"
set "OCCT_BRIDGE_NATIVE_DIR=%NATIVE%"
set "PATH=%NATIVE%;%APP_DIR%;%PATH%"
set "RES=%ROOT%occt\resources"
if exist "%RES%\SHMessage" set "CSF_SHMessage=%RES%\SHMessage"
if exist "%RES%\XSMessage" set "CSF_XSMessage=%RES%\XSMessage"
if exist "%RES%\StdResource" set "CSF_StandardDefaults=%RES%\StdResource"
if exist "%RES%\XSTEPResource" set "CSF_STEPDefaults=%RES%\XSTEPResource"
if exist "%RES%\XSTEPResource" set "CSF_IGESDefaults=%RES%\XSTEPResource"
if exist "%RES%\XCAFResources" set "CSF_XCAFDefaults=%RES%\XCAFResources"
if exist "%RES%\XCAFResources" set "CSF_PluginDefaults=%RES%\XCAFResources"
if exist "%RES%\Shaders" set "CSF_ShadersDirectory=%RES%\Shaders"
if exist "%RES%\Textures" set "CSF_MDTVTexturesDirectory=%RES%\Textures"
"%APP_DIR%$executable" %*
"@
    [System.IO.File]::WriteAllText((Join-Path $PackageRoot $FileName), $runCmd, [System.Text.Encoding]::ASCII)
}

function Write-PackageManifest {
    param(
        [Parameter(Mandatory = $true)][string]$PackageRoot,
        [Parameter(Mandatory = $true)][string[]]$Apps,
        [string]$ApplicationLayout = "flat"
    )

    $manifestPath = Join-Path $PackageRoot "package-manifest.json"
    Remove-Item -LiteralPath $manifestPath -Force -ErrorAction SilentlyContinue

    $root = [System.IO.Path]::GetFullPath($PackageRoot).TrimEnd('\') + '\'
    $files = @(
        Get-ChildItem -LiteralPath $PackageRoot -File -Recurse |
            Sort-Object FullName |
            ForEach-Object {
                [ordered]@{
                    name = $_.FullName.Substring($root.Length).Replace('\', '/')
                    size = $_.Length
                    sha256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
                }
            }
    )

    $requiresDesktopRuntime = @($Apps | Where-Object { $_ -in @("WinForms", "WPF") }).Count -gt 0
    $requiredRuntime = if ($UseSelfContained) { $null } elseif ($requiresDesktopRuntime) { "Microsoft.WindowsDesktop.App 10.x x64" } else { "Microsoft.NETCore.App 10.x x64" }

    $packageManifest = [ordered]@{
        schemaVersion = 3
        product = "OcctCSharpBridge Demo"
        apps = $Apps
        applicationLayout = $ApplicationLayout
        bridgeVersion = [string]$script:Contract.bridgeVersion
        bridgeSourceCommit = [string]$script:Manifest.sourceCommit
        bridgeTargetFramework = [string]$script:Contract.dotnet.targetFramework
        bridgePortableRuntime = $true
        bridgePortableManifest = "bridge-portable-manifest.json"
        nativeDirectory = "runtime"
        occtRoot = "occt"
        nativeAbi = [int]$script:Contract.nativeAbi.current
        occtVersion = [string]$script:Contract.occtVersion
        platform = "win-x64"
        configuration = $Configuration
        selfContained = [bool]$UseSelfContained
        requiredRuntime = $requiredRuntime
        files = $files
    }

    [System.IO.File]::WriteAllText(
        $manifestPath,
        ($packageManifest | ConvertTo-Json -Depth 8) + [Environment]::NewLine,
        [System.Text.UTF8Encoding]::new($false))
}

function Write-ZipPackage {
    param([Parameter(Mandatory = $true)][string]$PackageRoot)
    if (-not $Zip.IsPresent) { return }
    $zipPath = "$PackageRoot.zip"
    Remove-Item -LiteralPath $zipPath -Force -ErrorAction SilentlyContinue
    Compress-Archive -Path (Join-Path $PackageRoot "*") -DestinationPath $zipPath -CompressionLevel Optimal
    Write-Host "Archive: $zipPath" -ForegroundColor Green
}

function Publish-Standalone {
    param([Parameter(Mandatory = $true)][string]$Key)
    $definition = $Projects[$Key]
    $packageRoot = Join-Path $OutputDirectory $definition.Package
    $stagingRoot = Join-Path $OutputDirectory (".$($definition.Package)-staging-$PID")

    if ((Test-Path -LiteralPath $packageRoot) -and -not $KeepExisting.IsPresent) {
        Remove-Item -LiteralPath $packageRoot -Recurse -Force
    }
    New-Item -ItemType Directory -Path $packageRoot -Force | Out-Null

    try {
        Publish-ProjectToStaging $Key $stagingRoot
        Merge-PublishTree $stagingRoot $packageRoot
        Prepare-AppDirectory $packageRoot
        Merge-BridgePortablePayload $packageRoot
        Write-RunCommand $Key $packageRoot "run.cmd"
        Write-PackageManifest $packageRoot @([string]$definition.Name)
    }
    finally { Remove-Item -LiteralPath $stagingRoot -Recurse -Force -ErrorAction SilentlyContinue }

    Write-ZipPackage $packageRoot
    Write-Host "Package: $packageRoot" -ForegroundColor Green
}

function Publish-Unified {
    $packageRoot = Join-Path $OutputDirectory $UnifiedPackage
    $appsRoot = Join-Path $packageRoot "apps"

    # The old flat unified layout was framework-dependent because three desktop publish closures
    # cannot safely share one set of framework files. Keep each application's .NET closure isolated.
    Remove-Item -LiteralPath $packageRoot -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $appsRoot -Force | Out-Null

    foreach ($key in @("winform", "wpf", "avalonia")) {
        $appRoot = Join-Path $appsRoot $key
        Publish-ProjectToStaging $key $appRoot
        Prepare-AppDirectory $appRoot
    }

    Merge-BridgePortablePayload $packageRoot
    Write-RunCommand "winform" $packageRoot "run-winform.cmd" "apps\winform"
    Write-RunCommand "wpf" $packageRoot "run-wpf.cmd" "apps\wpf"
    Write-RunCommand "avalonia" $packageRoot "run-avalonia.cmd" "apps\avalonia"
    Write-PackageManifest $packageRoot @("WinForms", "WPF", "Avalonia") "apps/<frontend>"

    Write-ZipPackage $packageRoot
    Write-Host "Unified package: $packageRoot" -ForegroundColor Green
    if ($UseSelfContained) {
        Write-Host "The unified package is self-contained: target machines do not need the .NET 10 Desktop Runtime." -ForegroundColor Green
    }
    else {
        Write-Host "Framework-dependent mode was explicitly requested; target machines need the .NET 10 Desktop Runtime x64." -ForegroundColor Yellow
    }
}

Assert-Path $BuildScript
Assert-Path $ContractPath
Assert-Path $ManifestPath
Assert-Path $PortableRoot
$script:DotNet = Resolve-DotNet
$script:Contract = Get-Content -LiteralPath $ContractPath -Raw -Encoding UTF8 | ConvertFrom-Json
$script:Manifest = Get-Content -LiteralPath $ManifestPath -Raw -Encoding UTF8 | ConvertFrom-Json
Test-PortableRuntime

$validationTarget = if ($Target -eq "all") { "all" } else { $Target }
& $BuildScript $validationTarget $Configuration
if (-not $?) { throw "Demo validation/build failed before publish." }

Write-Host "[publish] Reusing exact Bridge portable payload from source $($script:Manifest.sourceCommit)." -ForegroundColor DarkGray
if ($Target -eq "all") { Publish-Unified } else { Publish-Standalone $Target }
