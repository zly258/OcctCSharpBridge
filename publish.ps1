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
    throw "-KeepExisting is not supported for unified 'all' publishing because the package layout must be rebuilt cleanly."
}

# Unified Windows publishing is portable by default without carrying three duplicate .NET runtimes.
# A single private .NET Desktop Runtime is stored at packageRoot/dotnet and the three apphosts
# resolve it through AppHostDotNetSearch=AppRelative. Explicit -SelfContained preserves the old
# per-application self-contained layout; explicit -FrameworkDependent requires a machine runtime.
$UseSharedDotNet = $Target -eq "all" -and -not $SelfContained.IsPresent -and -not $FrameworkDependent.IsPresent
$UseSelfContained = -not $FrameworkDependent.IsPresent -and -not $UseSharedDotNet
if ($SelfContained.IsPresent) { $UseSelfContained = $true; $UseSharedDotNet = $false }
$SharedDotNetRelativeFromApp = "../../dotnet"

$RunningOnWindows = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)
if (-not $RunningOnWindows) { throw "publish.ps1 supports Windows x64 only. Use ./publish.sh on Linux." }

$RepoRoot = Split-Path -Parent $PSCommandPath
$GlobalJsonPath = Join-Path $RepoRoot "global.json"
$BuildScript = Join-Path $RepoRoot "build.ps1"
$DistRoot = Join-Path $RepoRoot "external\OcctCSharpBridge\win-x64"
$PortableRoot = Join-Path $RepoRoot "external\OcctCSharpBridge\portable\win-x64"
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

function Get-DirectorySizeBytes {
    param([Parameter(Mandatory = $true)][string]$Path)
    if (-not (Test-Path -LiteralPath $Path -PathType Container)) { return [int64]0 }
    $sum = (Get-ChildItem -LiteralPath $Path -File -Recurse -Force | Measure-Object -Property Length -Sum).Sum
    if ($null -eq $sum) { return [int64]0 }
    return [int64]$sum
}

function Format-Size {
    param([int64]$Bytes)
    if ($Bytes -ge 1GB) { return "{0:N2} GB" -f ($Bytes / 1GB) }
    return "{0:N1} MB" -f ($Bytes / 1MB)
}

function Remove-EmptyDirectories {
    param([Parameter(Mandatory = $true)][string]$Root)
    if (-not (Test-Path -LiteralPath $Root -PathType Container)) { return }
    @(Get-ChildItem -LiteralPath $Root -Directory -Recurse -Force | Sort-Object { $_.FullName.Length } -Descending) |
        ForEach-Object {
            if (@(Get-ChildItem -LiteralPath $_.FullName -Force).Count -eq 0) {
                Remove-Item -LiteralPath $_.FullName -Force
            }
        }
}

function Optimize-ManagedPayload {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [switch]$PrivateDotNetRuntime
    )
    if (-not (Test-Path -LiteralPath $Root -PathType Container)) { return }

    $before = Get-DirectorySizeBytes $Root
    $removedFiles = 0

    # Satellite assemblies contain localized framework/package strings only. The Demo ships a
    # neutral UI and deliberately falls back to neutral resources instead of carrying every locale.
    foreach ($file in @(Get-ChildItem -LiteralPath $Root -File -Recurse -Force -Filter "*.resources.dll")) {
        Remove-Item -LiteralPath $file.FullName -Force
        $removedFiles++
    }

    # Symbols and runtimeconfig.dev.json are development-only artifacts.
    foreach ($file in @(Get-ChildItem -LiteralPath $Root -File -Recurse -Force | Where-Object {
        $_.Extension -ieq ".pdb" -or $_.Name -like "*.runtimeconfig.dev.json"
    })) {
        Remove-Item -LiteralPath $file.FullName -Force
        $removedFiles++
    }

    # Remove assembly XML documentation while leaving arbitrary application XML untouched.
    foreach ($file in @(Get-ChildItem -LiteralPath $Root -File -Recurse -Force -Filter "*.xml")) {
        $base = [System.IO.Path]::Combine($file.DirectoryName, [System.IO.Path]::GetFileNameWithoutExtension($file.Name))
        if ((Test-Path -LiteralPath "$base.dll" -PathType Leaf) -or (Test-Path -LiteralPath "$base.exe" -PathType Leaf)) {
            Remove-Item -LiteralPath $file.FullName -Force
            $removedFiles++
        }
    }

    if ($PrivateDotNetRuntime.IsPresent) {
        # These files support dump/debug/SOS workflows, not normal application execution.
        $diagnosticNames = @(
            "createdump.exe",
            "mscordaccore.dll",
            "mscordbi.dll",
            "sos.dll",
            "SOS.NETCore.dll"
        )
        foreach ($name in $diagnosticNames) {
            foreach ($file in @(Get-ChildItem -LiteralPath $Root -File -Recurse -Force -Filter $name)) {
                Remove-Item -LiteralPath $file.FullName -Force
                $removedFiles++
            }
        }
    }

    Remove-EmptyDirectories $Root
    $after = Get-DirectorySizeBytes $Root
    $saved = $before - $after
    Write-Host "[publish] Slimmed '$Root': removed $removedFiles files, saved $(Format-Size $saved)." -ForegroundColor DarkGray
}

function Copy-DirectoryContent {
    param(
        [Parameter(Mandatory = $true)][string]$Source,
        [Parameter(Mandatory = $true)][string]$Destination
    )
    Assert-Path $Source
    New-Item -ItemType Directory -Path $Destination -Force | Out-Null
    foreach ($item in @(Get-ChildItem -LiteralPath $Source -Force)) {
        Copy-Item -LiteralPath $item.FullName -Destination $Destination -Recurse -Force
    }
}

function Resolve-PrivateDotNetRuntime {
    param([Parameter(Mandatory = $true)][string]$DotNetRoot)

    $coreRoot = Join-Path $DotNetRoot "shared\Microsoft.NETCore.App"
    $desktopRoot = Join-Path $DotNetRoot "shared\Microsoft.WindowsDesktop.App"
    $fxrRoot = Join-Path $DotNetRoot "host\fxr"
    Assert-Path $coreRoot
    Assert-Path $desktopRoot
    Assert-Path $fxrRoot

    $coreNames = @(Get-ChildItem -LiteralPath $coreRoot -Directory | Where-Object { $_.Name -notmatch '-' } | ForEach-Object { $_.Name })
    $desktopNames = @(Get-ChildItem -LiteralPath $desktopRoot -Directory | Where-Object { $_.Name -notmatch '-' } | ForEach-Object { $_.Name })
    $commonNames = @($coreNames | Where-Object { $_ -in $desktopNames -and $_ -match '^10\.' } | Sort-Object { [version]$_ } -Descending)
    if ($commonNames.Count -eq 0) {
        throw "A matching stable Microsoft.NETCore.App + Microsoft.WindowsDesktop.App 10.x runtime was not found under $DotNetRoot. Install the .NET 10 Desktop Runtime/SDK."
    }

    $frameworkVersion = [string]$commonNames[0]
    $fxrNames = @(Get-ChildItem -LiteralPath $fxrRoot -Directory | Where-Object { $_.Name -match '^10\.' -and $_.Name -notmatch '-' } | ForEach-Object { $_.Name } | Sort-Object { [version]$_ } -Descending)
    if ($fxrNames.Count -eq 0) { throw "A stable .NET 10 hostfxr was not found under $DotNetRoot." }

    return [pscustomobject]@{
        FrameworkVersion = $frameworkVersion
        HostFxrVersion = [string]$fxrNames[0]
        CoreRoot = Join-Path $coreRoot $frameworkVersion
        DesktopRoot = Join-Path $desktopRoot $frameworkVersion
        HostFxrRoot = Join-Path $fxrRoot ([string]$fxrNames[0])
    }
}

function Build-PrivateDotNetRuntime {
    param([Parameter(Mandatory = $true)][string]$PackageRoot)

    $dotnetRoot = Join-Path $PackageRoot "dotnet"
    Remove-Item -LiteralPath $dotnetRoot -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $dotnetRoot -Force | Out-Null

    $sourceRoot = Split-Path -Parent $script:DotNet
    $resolved = Resolve-PrivateDotNetRuntime $sourceRoot

    $hostDestination = Join-Path $dotnetRoot ("host\fxr\" + $resolved.HostFxrVersion)
    $coreDestination = Join-Path $dotnetRoot ("shared\Microsoft.NETCore.App\" + $resolved.FrameworkVersion)
    $desktopDestination = Join-Path $dotnetRoot ("shared\Microsoft.WindowsDesktop.App\" + $resolved.FrameworkVersion)

    Copy-DirectoryContent $resolved.HostFxrRoot $hostDestination
    Copy-DirectoryContent $resolved.CoreRoot $coreDestination
    Copy-DirectoryContent $resolved.DesktopRoot $desktopDestination

    foreach ($name in @("dotnet.exe", "LICENSE.txt", "ThirdPartyNotices.txt")) {
        $source = Join-Path $sourceRoot $name
        if (Test-Path -LiteralPath $source -PathType Leaf) {
            Copy-Item -LiteralPath $source -Destination (Join-Path $dotnetRoot $name) -Force
        }
    }

    Optimize-ManagedPayload $dotnetRoot -PrivateDotNetRuntime

    Assert-Path (Join-Path $hostDestination "hostfxr.dll")
    Assert-Path (Join-Path $coreDestination "hostpolicy.dll")
    Assert-Path (Join-Path $coreDestination "coreclr.dll")
    Assert-Path (Join-Path $coreDestination "System.Private.CoreLib.dll")
    Assert-Path (Join-Path $desktopDestination "PresentationFramework.dll")

    $script:PrivateDotNetInfo = [pscustomobject]@{
        FrameworkVersion = $resolved.FrameworkVersion
        HostFxrVersion = $resolved.HostFxrVersion
        Root = $dotnetRoot
    }
    Write-Host "[publish] Shared private .NET Desktop Runtime $($resolved.FrameworkVersion) prepared: $(Format-Size (Get-DirectorySizeBytes $dotnetRoot))." -ForegroundColor Green
}

function Test-BinaryContainsAscii {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Text
    )
    $haystack = [System.IO.File]::ReadAllBytes($Path)
    $needle = [System.Text.Encoding]::UTF8.GetBytes($Text)
    if ($needle.Length -eq 0 -or $haystack.Length -lt $needle.Length) { return $false }
    for ($i = 0; $i -le $haystack.Length - $needle.Length; $i++) {
        $match = $true
        for ($j = 0; $j -lt $needle.Length; $j++) {
            if ($haystack[$i + $j] -ne $needle[$j]) { $match = $false; break }
        }
        if ($match) { return $true }
    }
    return $false
}

function Assert-SharedRuntimeAppHost {
    param(
        [Parameter(Mandatory = $true)][string]$Key,
        [Parameter(Mandatory = $true)][string]$AppRoot
    )
    $definition = $Projects[$Key]
    $exe = Join-Path $AppRoot ([string]$definition.Executable)
    $assemblyName = [System.IO.Path]::GetFileNameWithoutExtension([string]$definition.Executable)
    $runtimeConfigPath = Join-Path $AppRoot "$assemblyName.runtimeconfig.json"
    Assert-Path $exe
    Assert-Path $runtimeConfigPath

    foreach ($runtimeFile in @("hostfxr.dll", "hostpolicy.dll", "coreclr.dll", "System.Private.CoreLib.dll")) {
        if (Test-Path -LiteralPath (Join-Path $AppRoot $runtimeFile) -PathType Leaf) {
            throw "Shared-runtime publish gate failed: $runtimeFile was duplicated into $AppRoot."
        }
    }

    $runtimeConfig = Get-Content -LiteralPath $runtimeConfigPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $runtimeOptions = $runtimeConfig.PSObject.Properties["runtimeOptions"].Value
    $framework = $runtimeOptions.PSObject.Properties["framework"]
    $frameworks = $runtimeOptions.PSObject.Properties["frameworks"]
    if ($null -eq $framework -and $null -eq $frameworks) {
        throw "Shared-runtime publish gate failed: $assemblyName.runtimeconfig.json is not framework-dependent."
    }

    if (-not (Test-BinaryContainsAscii $exe $SharedDotNetRelativeFromApp)) {
        throw "Shared-runtime publish gate failed: $($definition.Executable) does not contain the AppRelative .NET path '$SharedDotNetRelativeFromApp'."
    }
    Write-Host "[shared-runtime-gate] $($definition.Executable): AppRelative private .NET path verified." -ForegroundColor Green
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
        throw "Synchronized Bridge portable runtime does not match external/OcctCSharpBridge/win-x64. Run .\sync.ps1 first."
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

    $mode = if ($UseSharedDotNet) { "shared-private-runtime" } elseif ($UseSelfContained) { "self-contained" } else { "framework-dependent" }
    Write-Host "[publish] $($definition.Name) / $Configuration / $mode..." -ForegroundColor Cyan

    $arguments = @(
        "publish", $project,
        "-c", $Configuration,
        "-r", "win-x64",
        "-p:Platform=x64",
        "-p:Version=$($script:Contract.bridgeVersion)",
        "-p:DebugType=None",
        "-p:DebugSymbols=false",
        "-p:UseAppHost=true",
        "-p:SatelliteResourceLanguages=en-US",
        "--self-contained", $UseSelfContained.ToString().ToLowerInvariant(),
        "--nologo",
        "-o", $StagingRoot
    )
    if ($UseSharedDotNet) {
        $arguments += "-p:AppHostDotNetSearch=AppRelative"
        $arguments += "-p:AppHostRelativeDotNet=$SharedDotNetRelativeFromApp"
    }

    Invoke-Checked $script:DotNet $arguments "$($definition.Name) publish failed."
    Assert-Path (Join-Path $StagingRoot $definition.Executable)
    Optimize-ManagedPayload $StagingRoot
    if ($UseSharedDotNet) { Assert-SharedRuntimeAppHost $Key $StagingRoot }
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

    # The Bridge Native DLL must come from the shared validated Portable Runtime.
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
    $dotnetLines = if ($UseSharedDotNet) {
        "set `"DOTNET_ROOT=%ROOT%dotnet`"`r`nset `"DOTNET_ROOT_X64=%ROOT%dotnet`""
    }
    else { "" }

    $runCmd = @"
@echo off
setlocal
set "ROOT=%~dp0"
$appDirectoryLine
$dotnetLines
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
    $requiredRuntime = if ($UseSharedDotNet -or $UseSelfContained) { $null } elseif ($requiresDesktopRuntime) { "Microsoft.WindowsDesktop.App 10.x x64" } else { "Microsoft.NETCore.App 10.x x64" }
    $deploymentMode = if ($UseSharedDotNet) { "shared-private-dotnet" } elseif ($UseSelfContained) { "self-contained" } else { "framework-dependent" }
    $privateFrameworkVersion = if ($UseSharedDotNet -and $null -ne $script:PrivateDotNetInfo) { [string]$script:PrivateDotNetInfo.FrameworkVersion } else { $null }
    $privateHostFxrVersion = if ($UseSharedDotNet -and $null -ne $script:PrivateDotNetInfo) { [string]$script:PrivateDotNetInfo.HostFxrVersion } else { $null }

    $packageManifest = [ordered]@{
        schemaVersion = 4
        product = "OcctCSharpBridge Demo"
        apps = $Apps
        applicationLayout = $ApplicationLayout
        deploymentMode = $deploymentMode
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
        privateDotNetRuntime = [bool]$UseSharedDotNet
        privateDotNetRoot = if ($UseSharedDotNet) { "dotnet" } else { $null }
        privateDotNetFrameworkVersion = $privateFrameworkVersion
        privateDotNetHostFxrVersion = $privateHostFxrVersion
        satelliteResources = "neutral-only"
        diagnosticsPayload = "removed"
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

function Write-PackageSizeSummary {
    param([Parameter(Mandatory = $true)][string]$PackageRoot)
    Write-Host "[publish] Package size summary:" -ForegroundColor Cyan
    foreach ($relative in @("apps\winform", "apps\wpf", "apps\avalonia", "dotnet", "runtime", "occt")) {
        $path = Join-Path $PackageRoot $relative
        if (Test-Path -LiteralPath $path -PathType Container) {
            Write-Host ("  {0,-18} {1,12}" -f $relative, (Format-Size (Get-DirectorySizeBytes $path)))
        }
    }
    Write-Host ("  {0,-18} {1,12}" -f "TOTAL", (Format-Size (Get-DirectorySizeBytes $PackageRoot))) -ForegroundColor Green
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

    Remove-Item -LiteralPath $packageRoot -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $appsRoot -Force | Out-Null

    foreach ($key in @("winform", "wpf", "avalonia")) {
        $appRoot = Join-Path $appsRoot $key
        Publish-ProjectToStaging $key $appRoot
        Prepare-AppDirectory $appRoot
    }

    if ($UseSharedDotNet) {
        Build-PrivateDotNetRuntime $packageRoot
    }

    Merge-BridgePortablePayload $packageRoot
    Write-RunCommand "winform" $packageRoot "run-winform.cmd" "apps\winform"
    Write-RunCommand "wpf" $packageRoot "run-wpf.cmd" "apps\wpf"
    Write-RunCommand "avalonia" $packageRoot "run-avalonia.cmd" "apps\avalonia"
    Write-PackageManifest $packageRoot @("WinForms", "WPF", "Avalonia") "apps/<frontend>"
    Write-PackageSizeSummary $packageRoot

    Write-ZipPackage $packageRoot
    Write-Host "Unified package: $packageRoot" -ForegroundColor Green
    if ($UseSharedDotNet) {
        Write-Host "The unified package carries one shared private .NET Desktop Runtime. The three EXEs use AppRelative host lookup and can be started directly without a machine-wide .NET installation." -ForegroundColor Green
    }
    elseif ($UseSelfContained) {
        Write-Host "Explicit self-contained mode carries a separate .NET runtime inside each app directory." -ForegroundColor Yellow
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
$script:PrivateDotNetInfo = $null
Test-PortableRuntime

$validationTarget = if ($Target -eq "all") { "all" } else { $Target }
& $BuildScript $validationTarget $Configuration
if (-not $?) { throw "Demo validation/build failed before publish." }

Write-Host "[publish] Reusing exact Bridge portable payload from source $($script:Manifest.sourceCommit)." -ForegroundColor DarkGray
if ($UseSharedDotNet) {
    Write-Host "[publish] Unified default: one shared private .NET runtime + neutral resources only." -ForegroundColor DarkGray
}
if ($Target -eq "all") { Publish-Unified } else { Publish-Standalone $Target }
