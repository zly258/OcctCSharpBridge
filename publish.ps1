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
if ($Target -eq "all" -and $SelfContained.IsPresent) {
    throw "Unified 'all' publish cannot be self-contained because WinForms/WPF/Avalonia Windows Desktop publish closures contain conflicting framework DLLs. Publish a single target with -SelfContained instead."
}

$UseSelfContained = $Target -ne "all" -and -not $FrameworkDependent.IsPresent
if ($SelfContained.IsPresent) { $UseSelfContained = $true }
if ($Target -eq "all") {
    Write-Host "[publish] Unified Windows package uses framework-dependent .NET 10 Desktop Runtime to avoid duplicate/conflicting framework DLLs." -ForegroundColor DarkGray
}

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
        if ($actual -ne ([string]$entry.sha256).ToLowerInvariant()) { throw "Bridge portable runtime hash mismatch: $($entry.name)" }
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
    Write-Host "[publish] $($definition.Name) / $Configuration..." -ForegroundColor Cyan
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
        [Parameter(Mandatory = $true)][string]$Destination
    )
    $sourceRoot = [System.IO.Path]::GetFullPath($Source).TrimEnd('\') + '\'
    foreach ($file in @(Get-ChildItem -LiteralPath $Source -File -Recurse)) {
        $relative = $file.FullName.Substring($sourceRoot.Length)
        $destinationPath = Join-Path $Destination $relative
        $destinationDirectory = Split-Path -Parent $destinationPath
        if (-not (Test-Path -LiteralPath $destinationDirectory -PathType Container)) {
            New-Item -ItemType Directory -Path $destinationDirectory -Force | Out-Null
        }
        if (Test-Path -LiteralPath $destinationPath -PathType Leaf) {
            $sourceHash = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
            $destinationHash = (Get-FileHash -LiteralPath $destinationPath -Algorithm SHA256).Hash
            if ($sourceHash -ne $destinationHash) { throw "Conflicting publish output '$relative' differs between Demo projects." }
            continue
        }
        Copy-Item -LiteralPath $file.FullName -Destination $destinationPath -Force
    }
}

function Assert-BridgeAssembliesMatch {
    param([Parameter(Mandatory = $true)][string]$PackageRoot)
    foreach ($name in @("OcctNet.dll", "OcctNet.WinForms.dll", "OcctNet.Wpf.dll", "OcctNet.Avalonia.dll")) {
        $packagePath = Join-Path $PackageRoot $name
        if (-not (Test-Path -LiteralPath $packagePath -PathType Leaf)) { continue }
        $bridgePath = Join-Path $PortableRoot $name
        Assert-Path $bridgePath
        $packageHash = (Get-FileHash -LiteralPath $packagePath -Algorithm SHA256).Hash
        $bridgeHash = (Get-FileHash -LiteralPath $bridgePath -Algorithm SHA256).Hash
        if ($packageHash -ne $bridgeHash) { throw "Published Demo assembly does not match synchronized Bridge portable SDK: $name" }
    }
}

function Copy-PortableDirectory {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string]$PackageRoot
    )
    $source = Join-Path $PortableRoot $Name
    Assert-Path $source
    $destination = Join-Path $PackageRoot $Name
    Remove-Item -LiteralPath $destination -Recurse -Force -ErrorAction SilentlyContinue
    Copy-Item -LiteralPath $source -Destination $destination -Recurse -Force
}

function Copy-SharedPackageContent {
    param([Parameter(Mandatory = $true)][string]$PackageRoot)

    # dotnet publish may copy the minimal OcctNative.dll beside the executable. Remove it so
    # OcctRuntime selects the validated <app>/runtime closure instead of an incomplete app-local native module.
    Remove-Item -LiteralPath (Join-Path $PackageRoot "OcctNative.dll") -Force -ErrorAction SilentlyContinue

    Assert-BridgeAssembliesMatch $PackageRoot
    Copy-PortableDirectory "runtime" $PackageRoot
    Copy-PortableDirectory "occt" $PackageRoot

    Copy-Item -LiteralPath $ContractPath -Destination (Join-Path $PackageRoot "bridge-contract.json") -Force
    Copy-Item -LiteralPath $ManifestPath -Destination (Join-Path $PackageRoot "bridge-manifest.json") -Force
    Copy-Item -LiteralPath $PortableManifestPath -Destination (Join-Path $PackageRoot "bridge-portable-manifest.json") -Force

    foreach ($name in @("LICENSE", "LICENSE_LGPL_21.txt", "OcctCSharpBridge_LGPL_EXCEPTION.txt", "THIRD_PARTY_NOTICES.md", "COMMERCIAL.md", "PORTABLE-SDK.txt")) {
        $source = Join-Path $PortableRoot $name
        if (Test-Path -LiteralPath $source -PathType Leaf) {
            $destinationName = if ($name -eq "PORTABLE-SDK.txt") { "BRIDGE-PORTABLE-SDK.txt" } else { $name }
            Copy-Item -LiteralPath $source -Destination (Join-Path $PackageRoot $destinationName) -Force
        }
    }
}

function Write-RunCommand {
    param(
        [Parameter(Mandatory = $true)][string]$Key,
        [Parameter(Mandatory = $true)][string]$PackageRoot,
        [Parameter(Mandatory = $true)][string]$FileName
    )
    $executable = [string]$Projects[$Key].Executable
    $runCmd = @"
@echo off
setlocal
set "APP_DIR=%~dp0"
set "NATIVE=%APP_DIR%runtime"
set "CASROOT=%APP_DIR%occt"
set "OCCT_ROOT=%APP_DIR%occt"
set "OCCT_BRIDGE_NATIVE_DIR=%NATIVE%"
set "PATH=%NATIVE%;%APP_DIR%;%PATH%"
set "RES=%APP_DIR%occt\resources"
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
        [Parameter(Mandatory = $true)][string[]]$Apps
    )

    $manifestPath = Join-Path $PackageRoot "package-manifest.json"
    Remove-Item -LiteralPath $manifestPath -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath (Join-Path $PackageRoot "publish-manifest.txt") -Force -ErrorAction SilentlyContinue

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
        schemaVersion = 2
        product = "OcctCSharpBridge Demo"
        apps = $Apps
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

    [System.IO.File]::WriteAllText($manifestPath, ($packageManifest | ConvertTo-Json -Depth 8) + [Environment]::NewLine, [System.Text.UTF8Encoding]::new($false))
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

    if ((Test-Path -LiteralPath $packageRoot) -and -not $KeepExisting.IsPresent) { Remove-Item -LiteralPath $packageRoot -Recurse -Force }
    New-Item -ItemType Directory -Path $packageRoot -Force | Out-Null
    try {
        Publish-ProjectToStaging $Key $stagingRoot
        Merge-PublishTree $stagingRoot $packageRoot
        Copy-SharedPackageContent $packageRoot
        Write-RunCommand $Key $packageRoot "run.cmd"
        Write-PackageManifest $packageRoot @([string]$definition.Name)
    }
    finally { Remove-Item -LiteralPath $stagingRoot -Recurse -Force -ErrorAction SilentlyContinue }

    Write-ZipPackage $packageRoot
    Write-Host "Package: $packageRoot" -ForegroundColor Green
}

function Publish-Unified {
    $packageRoot = Join-Path $OutputDirectory $UnifiedPackage
    $stagingRoot = Join-Path $OutputDirectory (".$UnifiedPackage-staging-$PID")

    if (-not $KeepExisting.IsPresent) {
        if (Test-Path -LiteralPath $packageRoot) { Remove-Item -LiteralPath $packageRoot -Recurse -Force }
        foreach ($definition in $Projects.Values) {
            $legacyRoot = Join-Path $OutputDirectory ([string]$definition.Package)
            if (Test-Path -LiteralPath $legacyRoot) { Remove-Item -LiteralPath $legacyRoot -Recurse -Force }
            Remove-Item -LiteralPath "$legacyRoot.zip" -Force -ErrorAction SilentlyContinue
        }
    }
    Remove-Item -LiteralPath $stagingRoot -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $packageRoot -Force | Out-Null
    New-Item -ItemType Directory -Path $stagingRoot -Force | Out-Null

    try {
        foreach ($key in @("winform", "wpf", "avalonia")) {
            $appStaging = Join-Path $stagingRoot $key
            Publish-ProjectToStaging $key $appStaging
            Merge-PublishTree $appStaging $packageRoot
        }
        Copy-SharedPackageContent $packageRoot
        Write-RunCommand "winform" $packageRoot "run-winform.cmd"
        Write-RunCommand "wpf" $packageRoot "run-wpf.cmd"
        Write-RunCommand "avalonia" $packageRoot "run-avalonia.cmd"
        Write-PackageManifest $packageRoot @("WinForms", "WPF", "Avalonia")
    }
    finally { Remove-Item -LiteralPath $stagingRoot -Recurse -Force -ErrorAction SilentlyContinue }

    Write-ZipPackage $packageRoot
    Write-Host "Unified package: $packageRoot" -ForegroundColor Green
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

Write-Host "[publish] Reusing Bridge portable runtime from source $($script:Manifest.sourceCommit)." -ForegroundColor DarkGray
if ($Target -eq "all") { Publish-Unified } else { Publish-Standalone $Target }
