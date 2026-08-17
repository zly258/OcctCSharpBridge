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
    $baseline = [version]"10.0.100"
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
        if ($exitCode -ne 0) { continue }
        try { $version = [version]$versionText }
        catch { continue }
        if ($version.Major -eq 10 -and $version.Minor -eq 0 -and $version -ge $baseline) {
            return [System.IO.Path]::GetFullPath($candidate)
        }
    }
    throw "A dotnet host resolving a stable .NET 10 SDK at or above baseline 10.0.100 was not found."
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

function Publish-One {
    param([Parameter(Mandatory = $true)][string]$Key)
    $definition = $Projects[$Key]
    $project = Join-Path $RepoRoot $definition.Project
    $packageRoot = Join-Path $OutputDirectory $definition.Package
    $stagingRoot = Join-Path $OutputDirectory (".$($definition.Package)-staging-$PID")

    Assert-Path $project
    & $BuildScript $Key $Configuration
    if (-not $?) { throw "$($definition.Name) validation/build failed." }

    if ((Test-Path -LiteralPath $packageRoot) -and -not $KeepExisting.IsPresent) { Remove-Item -LiteralPath $packageRoot -Recurse -Force }
    Remove-Item -LiteralPath $stagingRoot -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $packageRoot -Force | Out-Null
    New-Item -ItemType Directory -Path $stagingRoot -Force | Out-Null

    try {
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
            "-o", $stagingRoot
        ) "$($definition.Name) publish failed."
        Copy-Item -Path (Join-Path $stagingRoot "*") -Destination $packageRoot -Recurse -Force
        Assert-Path (Join-Path $packageRoot $definition.Executable)

        Copy-NativeClosure $packageRoot
        Copy-Item -LiteralPath $ContractPath -Destination (Join-Path $packageRoot "bridge-contract.json") -Force
        Copy-Item -LiteralPath $ManifestPath -Destination (Join-Path $packageRoot "bridge-manifest.json") -Force
        foreach ($notice in @("LICENSE", "LICENSE_LGPL_21.txt", "OcctCSharpBridge_LGPL_EXCEPTION.txt", "THIRD_PARTY_NOTICES.md", "COMMERCIAL.md")) {
            $path = Join-Path $RepoRoot $notice
            if (Test-Path -LiteralPath $path -PathType Leaf) { Copy-Item -LiteralPath $path -Destination (Join-Path $packageRoot $notice) -Force }
        }
        foreach ($resource in @("SHMessage", "XSMessage", "XSTEPResource", "XCAFResources", "StdResource", "Textures")) { Copy-OcctResource $resource $packageRoot }

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
"%APP_DIR%$($definition.Executable)" %*
"@
        [System.IO.File]::WriteAllText((Join-Path $packageRoot "run.cmd"), $runCmd, [System.Text.Encoding]::ASCII)

        $summary = @(
            "$($definition.Name) Demo",
            "Bridge: $($script:Contract.bridgeVersion)",
            "ABI: $($script:Contract.nativeAbi.current) only",
            "OCCT: $($script:Contract.occtVersion)",
            "Platform: win-x64",
            "Configuration: $Configuration",
            "Self-contained: $UseSelfContained"
        )
        [System.IO.File]::WriteAllLines((Join-Path $packageRoot "publish-manifest.txt"), $summary, [System.Text.UTF8Encoding]::new($false))
    }
    finally { Remove-Item -LiteralPath $stagingRoot -Recurse -Force -ErrorAction SilentlyContinue }

    if ($Zip.IsPresent) {
        $zipPath = "$packageRoot.zip"
        Remove-Item -LiteralPath $zipPath -Force -ErrorAction SilentlyContinue
        Compress-Archive -Path (Join-Path $packageRoot "*") -DestinationPath $zipPath -CompressionLevel Optimal
        Write-Host "Archive: $zipPath" -ForegroundColor Green
    }
    Write-Host "Package: $packageRoot" -ForegroundColor Green
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

$selected = if ($Target -eq "all") { @("winform", "wpf", "avalonia") } else { @($Target) }
foreach ($key in $selected) { Publish-One $key }
