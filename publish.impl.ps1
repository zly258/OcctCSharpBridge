param(
    [Parameter(Position = 0)]
    [ValidateSet("all", "winform", "wpf", "avalonia")]
    [string]$Target = "all",

    [Parameter(Position = 1)]
    [ValidateSet("Debug", "Release", "RelWithDebInfo")]
    [string]$Configuration = "Release",

    [string]$OcctRoot = $env:OCCT_ROOT,
    [string]$OutputDirectory = "",
    [switch]$SelfContained,
    [switch]$FrameworkDependent,
    [switch]$Zip,
    [switch]$KeepExisting
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

if ($SelfContained.IsPresent -and $FrameworkDependent.IsPresent) { throw "Use either -SelfContained or -FrameworkDependent, not both." }
$UseSelfContained = -not $FrameworkDependent.IsPresent
if ($SelfContained.IsPresent) { $UseSelfContained = $true }

$RunningOnWindows = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)
if (-not $RunningOnWindows) { throw "publish.ps1 supports Windows x64 only. Use ./publish.sh on Linux." }

$RepoRoot = Split-Path -Parent $PSCommandPath
$GlobalJsonPath = Join-Path $RepoRoot "global.json"
$BuildScript = Join-Path $RepoRoot "build.ps1"
$DistRoot = Join-Path $RepoRoot "dist\win-x64"
$ContractPath = Join-Path $DistRoot "bridge-contract.json"
$ManifestPath = Join-Path $DistRoot "bridge-manifest.json"
$NativeDll = Join-Path $DistRoot "OcctNative.dll"
$DefaultOcctRoot = "D:\tools\occt-vc144-64"
if ([string]::IsNullOrWhiteSpace($OcctRoot)) { $OcctRoot = $DefaultOcctRoot }
$OcctRoot = [System.IO.Path]::GetFullPath($OcctRoot)
$OcctBinDir = Join-Path $OcctRoot "win64\vc14\bin"
$OcctThirdPartyDir = Join-Path $OcctRoot "3rdparty-vc14-64"
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
        if ($version.Major -eq $baseline.Major -and
            $version.Minor -eq $baseline.Minor -and
            $version -ge $baseline) {
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

function Resolve-Dumpbin {
    $command = Get-Command dumpbin.exe -ErrorAction SilentlyContinue
    if ($null -ne $command) { return $command.Source }
    if (-not [string]::IsNullOrWhiteSpace($env:VCToolsInstallDir)) {
        $candidate = Join-Path $env:VCToolsInstallDir "bin\Hostx64\x64\dumpbin.exe"
        if (Test-Path -LiteralPath $candidate) { return $candidate }
    }
    foreach ($root in @($env:ProgramFiles, ${env:ProgramFiles(x86)}) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }) {
        $pattern = Join-Path $root "Microsoft Visual Studio\2022\*\VC\Tools\MSVC\*\bin\Hostx64\x64\dumpbin.exe"
        $match = Get-Item -Path $pattern -ErrorAction SilentlyContinue | Sort-Object FullName -Descending | Select-Object -First 1
        if ($null -ne $match) { return $match.FullName }
    }
    throw "dumpbin.exe was not found. Install Visual Studio 2022 C++ build tools or use a Developer PowerShell."
}

function Get-ImportedDllNames {
    param([Parameter(Mandatory = $true)][string]$Path)
    $lines = & $script:Dumpbin /nologo /dependents $Path 2>$null
    if ($LASTEXITCODE -ne 0) { throw "dumpbin failed for $Path" }
    return @($lines | ForEach-Object { ([string]$_).Trim() } | Where-Object { $_ -match '(?i)^[A-Za-z0-9_.+-]+\.dll$' } | Sort-Object -Unique)
}

function Get-VcRuntimeDirectories {
    $result = [System.Collections.Generic.List[string]]::new()
    if (-not [string]::IsNullOrWhiteSpace($env:VCToolsRedistDir)) {
        Get-Item -Path (Join-Path $env:VCToolsRedistDir "x64\Microsoft.VC14*.CRT") -ErrorAction SilentlyContinue | Where-Object PSIsContainer | ForEach-Object { $result.Add($_.FullName) }
    }
    foreach ($root in @($env:ProgramFiles, ${env:ProgramFiles(x86)}) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }) {
        $pattern = Join-Path $root "Microsoft Visual Studio\2022\*\VC\Redist\MSVC\*\x64\Microsoft.VC14*.CRT"
        Get-Item -Path $pattern -ErrorAction SilentlyContinue | Sort-Object FullName -Descending | Where-Object PSIsContainer | ForEach-Object {
            if (-not $result.Contains($_.FullName)) { $result.Add($_.FullName) }
        }
    }
    return @($result)
}

function Test-DebugRuntime {
    param([string]$Name)
    return $Name -ieq "ucrtbased.dll" -or $Name -match '(?i)^(MSVCP|VCRUNTIME|CONCRT|VCCORLIB).*D\.dll$'
}

function Test-SystemDependency {
    param([string]$Name)
    if ($Name -match '(?i)^(api-ms-win-|ext-ms-win-)') { return $true }
    if ($Name -match '(?i)^(msvcp|vcruntime|concrt|vccorlib)') { return $false }
    return Test-Path -LiteralPath (Join-Path $env:SystemRoot "System32\$Name") -PathType Leaf
}

function Resolve-Dependency {
    param([string]$Name)
    $candidate = Join-Path $OcctBinDir $Name
    if (Test-Path -LiteralPath $candidate -PathType Leaf) { return $candidate }
    foreach ($directory in $script:VcRuntimeDirectories) {
        $candidate = Join-Path $directory $Name
        if (Test-Path -LiteralPath $candidate -PathType Leaf) { return $candidate }
    }
    if (Test-Path -LiteralPath $OcctThirdPartyDir -PathType Container) {
        $match = Get-ChildItem -LiteralPath $OcctThirdPartyDir -Filter $Name -File -Recurse -ErrorAction SilentlyContinue |
            Sort-Object @{ Expression = { if ($_.FullName -match '(?i)[\\/](debug|dbg)[\\/]') { 1 } else { 0 } } }, FullName |
            Select-Object -First 1
        if ($null -ne $match) { return $match.FullName }
    }
    return $null
}

function Replace-AsciiImport {
    param([string]$Path, [string]$OldName, [string]$NewName)
    if ($NewName.Length -gt $OldName.Length) { throw "Cannot replace a PE import with a longer name: $OldName -> $NewName" }
    $bytes = [System.IO.File]::ReadAllBytes($Path)
    $old = [System.Text.Encoding]::ASCII.GetBytes($OldName + [char]0)
    $new = [System.Text.Encoding]::ASCII.GetBytes($NewName + [char]0)
    $replaced = $false
    for ($offset = 0; $offset -le $bytes.Length - $old.Length; $offset++) {
        $same = $true
        for ($i = 0; $i -lt $old.Length; $i++) { if ($bytes[$offset + $i] -ne $old[$i]) { $same = $false; break } }
        if (-not $same) { continue }
        [Array]::Clear($bytes, $offset, $old.Length)
        [Array]::Copy($new, 0, $bytes, $offset, $new.Length)
        $replaced = $true
        $offset += $old.Length - 1
    }
    if (-not $replaced) { throw "Import string was not found in $Path`: $OldName" }
    [System.IO.File]::WriteAllBytes($Path, $bytes)
}

function Repair-Occt790TbbImport {
    param([string]$Path)
    if ($Configuration -ne "Release") { return }
    foreach ($dependency in @(Get-ImportedDllNames $Path)) {
        if ($dependency -notmatch '(?i)^tbb.*_debug\.dll$') { continue }
        $releaseName = [regex]::Replace($dependency, '(?i)_debug(?=\.dll$)', '')
        if ($null -eq (Resolve-Dependency $releaseName)) { throw "OCCT 7.9.0 imports $dependency but release counterpart $releaseName was not found." }
        Replace-AsciiImport $Path $dependency $releaseName
        Write-Host "[runtime] corrected OCCT 7.9.0 import: $dependency -> $releaseName" -ForegroundColor Yellow
    }
}

function Copy-NativeClosure {
    param([string]$Destination)
    Copy-Item -LiteralPath $NativeDll -Destination (Join-Path $Destination "OcctNative.dll") -Force
    $queue = [System.Collections.Generic.Queue[string]]::new()
    $queued = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    $processed = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    $queue.Enqueue((Join-Path $Destination "OcctNative.dll")); [void]$queued.Add("OcctNative.dll")
    while ($queue.Count -gt 0) {
        $current = $queue.Dequeue()
        $name = [IO.Path]::GetFileName($current)
        if (-not $processed.Add($name)) { continue }
        Repair-Occt790TbbImport $current
        foreach ($dependency in @(Get-ImportedDllNames $current)) {
            if (Test-SystemDependency $dependency) { continue }
            if ($Configuration -eq "Release" -and (Test-DebugRuntime $dependency)) { throw "Release package depends on Microsoft debug runtime: $name -> $dependency" }
            $destinationPath = Join-Path $Destination $dependency
            if (-not (Test-Path -LiteralPath $destinationPath -PathType Leaf)) {
                $source = Resolve-Dependency $dependency
                if ($null -eq $source) { throw "Portable runtime dependency was not found: $name -> $dependency" }
                Copy-Item -LiteralPath $source -Destination $destinationPath -Force
                Write-Host "[runtime] $name -> $dependency" -ForegroundColor DarkGray
            }
            if ($queued.Add($dependency)) { $queue.Enqueue($destinationPath) }
        }
    }
}

function Copy-OcctResource {
    param([string]$Name, [string]$PackageRoot)
    foreach ($candidate in @((Join-Path $OcctRoot "src\$Name"), (Join-Path $OcctRoot "share\opencascade\resources\$Name"))) {
        if (-not (Test-Path -LiteralPath $candidate -PathType Container)) { continue }
        $destination = Join-Path $PackageRoot "occt\resources\$Name"
        New-Item -ItemType Directory -Path (Split-Path -Parent $destination) -Force | Out-Null
        Copy-Item -LiteralPath $candidate -Destination $destination -Recurse -Force
        return
    }
}

function Publish-ProjectToStaging {
    param(
        [Parameter(Mandatory = $true)][string]$Key,
        [Parameter(Mandatory = $true)][string]$StagingRoot
    )
    $definition = $Projects[$Key]
    $project = Join-Path $RepoRoot $definition.Project
    Assert-Path $project

    & $BuildScript $Key $Configuration
    if (-not $?) { throw "$($definition.Name) validation/build failed." }

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
            $existing = Get-Item -LiteralPath $destinationPath
            if ($existing.Length -ne $file.Length) {
                throw "Conflicting publish output '$relative' has different lengths between Demo projects."
            }
            $sourceHash = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
            $destinationHash = (Get-FileHash -LiteralPath $destinationPath -Algorithm SHA256).Hash
            if ($sourceHash -ne $destinationHash) {
                throw "Conflicting publish output '$relative' differs between Demo projects."
            }
            continue
        }
        Copy-Item -LiteralPath $file.FullName -Destination $destinationPath -Force
    }
}

function Copy-SharedPackageContent {
    param([Parameter(Mandatory = $true)][string]$PackageRoot)
    Copy-NativeClosure $PackageRoot
    Copy-Item -LiteralPath $ContractPath -Destination (Join-Path $PackageRoot "bridge-contract.json") -Force
    Copy-Item -LiteralPath $ManifestPath -Destination (Join-Path $PackageRoot "bridge-manifest.json") -Force
    foreach ($notice in @("LICENSE", "LICENSE_LGPL_21.txt", "OcctCSharpBridge_LGPL_EXCEPTION.txt", "THIRD_PARTY_NOTICES.md", "COMMERCIAL.md")) {
        $path = Join-Path $RepoRoot $notice
        if (Test-Path -LiteralPath $path -PathType Leaf) { Copy-Item -LiteralPath $path -Destination (Join-Path $PackageRoot $notice) -Force }
    }
    foreach ($resource in @("SHMessage", "XSMessage", "XSTEPResource", "XCAFResources", "StdResource", "Textures")) { Copy-OcctResource $resource $PackageRoot }
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
set "CASROOT=%APP_DIR%occt"
set "OCCT_BRIDGE_NATIVE_DIR=%APP_DIR%"
set "PATH=%APP_DIR%;%PATH%"
set "RES=%APP_DIR%occt\resources"
if exist "%RES%\SHMessage" set "CSF_SHMessage=%RES%\SHMessage"
if exist "%RES%\XSMessage" set "CSF_XSMessage=%RES%\XSMessage"
if exist "%RES%\XSTEPResource" set "CSF_STEPDefaults=%RES%\XSTEPResource"
if exist "%RES%\XSTEPResource" set "CSF_IGESDefaults=%RES%\XSTEPResource"
if exist "%RES%\XCAFResources" set "CSF_XCAFDefaults=%RES%\XCAFResources"
if exist "%RES%\XCAFResources" set "CSF_PluginDefaults=%RES%\XCAFResources"
if exist "%RES%\Textures" set "CSF_MDTVTexturesDirectory=%RES%\Textures"
"%APP_DIR%$executable" %*
"@
    [System.IO.File]::WriteAllText((Join-Path $PackageRoot $FileName), $runCmd, [System.Text.Encoding]::ASCII)
}

function Write-PublishManifest {
    param(
        [Parameter(Mandatory = $true)][string]$PackageRoot,
        [Parameter(Mandatory = $true)][string[]]$Apps
    )
    $summary = @(
        "OcctCSharpBridge Demo",
        "Apps: $($Apps -join ', ')",
        "Bridge: $($script:Contract.bridgeVersion)",
        "ABI: $($script:Contract.nativeAbi.current) only",
        "OCCT: $($script:Contract.occtVersion)",
        "Platform: win-x64",
        "Configuration: $Configuration",
        "Self-contained: $UseSelfContained"
    )
    [System.IO.File]::WriteAllLines((Join-Path $PackageRoot "publish-manifest.txt"), $summary, [System.Text.UTF8Encoding]::new($false))
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
        Write-PublishManifest $packageRoot @([string]$definition.Name)
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
        Write-PublishManifest $packageRoot @("WinForms", "WPF", "Avalonia")
    }
    finally { Remove-Item -LiteralPath $stagingRoot -Recurse -Force -ErrorAction SilentlyContinue }

    Write-ZipPackage $packageRoot
    Write-Host "Unified package: $packageRoot" -ForegroundColor Green
}

Assert-Path $BuildScript
Assert-Path $ContractPath
Assert-Path $ManifestPath
Assert-Path $NativeDll
Assert-Path $OcctBinDir
$script:DotNet = Resolve-DotNet
$script:Dumpbin = Resolve-Dumpbin
$script:VcRuntimeDirectories = @(Get-VcRuntimeDirectories)
$script:Contract = Get-Content -LiteralPath $ContractPath -Raw -Encoding UTF8 | ConvertFrom-Json

& $BuildScript validate $Configuration
if (-not $?) { throw "Bridge SDK validation failed." }

if ($Target -eq "all") {
    Publish-Unified
}
else {
    Publish-Standalone $Target
}