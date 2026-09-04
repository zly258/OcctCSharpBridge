param(
    [Parameter(Mandatory = $true)][string]$SdkRoot,
    [Parameter(Mandatory = $true)][string]$OcctRoot,
    [Parameter(Mandatory = $true)][string]$OutputDirectory,
    [switch]$Zip
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$RepoRoot = Split-Path -Parent (Split-Path -Parent $PSCommandPath)
$SdkRoot = [System.IO.Path]::GetFullPath($SdkRoot)
$OcctRoot = [System.IO.Path]::GetFullPath($OcctRoot)
$OutputDirectory = [System.IO.Path]::GetFullPath($OutputDirectory)
$ContractPath = Join-Path $SdkRoot "bridge-contract.json"
$ManifestPath = Join-Path $SdkRoot "bridge-manifest.json"
$OcctBinDir = Join-Path $OcctRoot "win64\vc14\bin"
$OcctThirdPartyDir = Join-Path $OcctRoot "3rdparty-vc14-64"

function Assert-Path {
    param([Parameter(Mandatory = $true)][string]$Path)
    if (-not (Test-Path -LiteralPath $Path)) { throw "Required path was not found: $Path" }
}

function Get-MsvcInstallations {
    $result = [System.Collections.Generic.List[string]]::new()
    $seen = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)

    if (-not [string]::IsNullOrWhiteSpace($env:VSINSTALLDIR) -and
        (Test-Path -LiteralPath $env:VSINSTALLDIR -PathType Container)) {
        $path = [System.IO.Path]::GetFullPath($env:VSINSTALLDIR)
        if ($seen.Add($path)) { $result.Add($path) }
    }

    $vswhereCandidates = [System.Collections.Generic.List[string]]::new()
    $vswhereCommand = Get-Command vswhere.exe -ErrorAction SilentlyContinue
    if ($null -ne $vswhereCommand -and -not [string]::IsNullOrWhiteSpace([string]$vswhereCommand.Source)) {
        $vswhereCandidates.Add([string]$vswhereCommand.Source)
    }
    if (-not [string]::IsNullOrWhiteSpace(${env:ProgramFiles(x86)})) {
        $defaultVswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
        if (Test-Path -LiteralPath $defaultVswhere -PathType Leaf) { $vswhereCandidates.Add($defaultVswhere) }
    }

    $seenVswhere = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($vswhere in $vswhereCandidates) {
        if (-not $seenVswhere.Add($vswhere)) { continue }
        foreach ($line in @(& $vswhere -all -products "*" -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath 2>$null)) {
            $path = ([string]$line).Trim()
            if ([string]::IsNullOrWhiteSpace($path) -or -not (Test-Path -LiteralPath $path -PathType Container)) { continue }
            $path = [System.IO.Path]::GetFullPath($path)
            if ($seen.Add($path)) { $result.Add($path) }
        }
    }

    foreach ($root in @($env:ProgramFiles, ${env:ProgramFiles(x86)}) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }) {
        $vsRoot = Join-Path $root "Microsoft Visual Studio"
        if (-not (Test-Path -LiteralPath $vsRoot -PathType Container)) { continue }
        foreach ($yearDirectory in @(Get-ChildItem -LiteralPath $vsRoot -Directory -ErrorAction SilentlyContinue | Sort-Object Name -Descending)) {
            foreach ($installation in @(Get-ChildItem -LiteralPath $yearDirectory.FullName -Directory -ErrorAction SilentlyContinue | Sort-Object Name)) {
                $path = $installation.FullName
                if (-not (Test-Path -LiteralPath (Join-Path $path "VC\Tools\MSVC") -PathType Container)) { continue }
                if ($seen.Add($path)) { $result.Add($path) }
            }
        }
    }

    return @($result)
}

function Resolve-Dumpbin {
    $command = Get-Command dumpbin.exe -ErrorAction SilentlyContinue
    if ($null -ne $command) { return $command.Source }

    if (-not [string]::IsNullOrWhiteSpace($env:VCToolsInstallDir)) {
        $candidate = Join-Path $env:VCToolsInstallDir "bin\Hostx64\x64\dumpbin.exe"
        if (Test-Path -LiteralPath $candidate -PathType Leaf) { return $candidate }
    }

    foreach ($installation in @(Get-MsvcInstallations)) {
        $toolsRoot = Join-Path $installation "VC\Tools\MSVC"
        if (-not (Test-Path -LiteralPath $toolsRoot -PathType Container)) { continue }
        foreach ($toolset in @(Get-ChildItem -LiteralPath $toolsRoot -Directory -ErrorAction SilentlyContinue | Sort-Object Name -Descending)) {
            $candidate = Join-Path $toolset.FullName "bin\Hostx64\x64\dumpbin.exe"
            if (Test-Path -LiteralPath $candidate -PathType Leaf) { return $candidate }
        }
    }

    throw "dumpbin.exe was not found. Install the Visual Studio C++ Build Tools (MSVC x64 tools) or use a Developer PowerShell."
}

function Get-ImportedDllNames {
    param([Parameter(Mandatory = $true)][string]$Path)
    $lines = & $script:Dumpbin /nologo /dependents $Path 2>$null
    if ($LASTEXITCODE -ne 0) { throw "dumpbin failed for $Path" }
    return @($lines | ForEach-Object { ([string]$_).Trim() } | Where-Object { $_ -match '(?i)^[A-Za-z0-9_.+-]+\.dll$' } | Sort-Object -Unique)
}

function Get-VcRuntimeDirectories {
    $result = [System.Collections.Generic.List[string]]::new()
    $seen = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)

    if (-not [string]::IsNullOrWhiteSpace($env:VCToolsRedistDir)) {
        Get-Item -Path (Join-Path $env:VCToolsRedistDir "x64\Microsoft.VC14*.CRT") -ErrorAction SilentlyContinue |
            Where-Object PSIsContainer | ForEach-Object {
                if ($seen.Add($_.FullName)) { $result.Add($_.FullName) }
            }
    }

    foreach ($installation in @(Get-MsvcInstallations)) {
        $redistRoot = Join-Path $installation "VC\Redist\MSVC"
        if (-not (Test-Path -LiteralPath $redistRoot -PathType Container)) { continue }
        foreach ($redistVersion in @(Get-ChildItem -LiteralPath $redistRoot -Directory -ErrorAction SilentlyContinue | Sort-Object Name -Descending)) {
            Get-Item -Path (Join-Path $redistVersion.FullName "x64\Microsoft.VC14*.CRT") -ErrorAction SilentlyContinue |
                Where-Object PSIsContainer | ForEach-Object {
                    if ($seen.Add($_.FullName)) { $result.Add($_.FullName) }
                }
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
    param([Parameter(Mandatory = $true)][string]$Name)

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
        for ($i = 0; $i -lt $old.Length; $i++) {
            if ($bytes[$offset + $i] -ne $old[$i]) { $same = $false; break }
        }
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
    param([Parameter(Mandatory = $true)][string]$Path)
    foreach ($dependency in @(Get-ImportedDllNames $Path)) {
        if ($dependency -notmatch '(?i)^tbb.*_debug\.dll$') { continue }
        $releaseName = [regex]::Replace($dependency, '(?i)_debug(?=\.dll$)', '')
        if ($null -eq (Resolve-Dependency $releaseName)) {
            throw "OCCT 7.9.0 imports $dependency but release counterpart $releaseName was not found."
        }
        Replace-AsciiImport $Path $dependency $releaseName
        Write-Host "[portable-runtime] corrected OCCT import: $dependency -> $releaseName" -ForegroundColor Yellow
    }
}

function Copy-NativeClosure {
    param([Parameter(Mandatory = $true)][string]$RuntimeDirectory)

    $sourceNative = Join-Path $SdkRoot "OcctNative.dll"
    Assert-Path $sourceNative
    New-Item -ItemType Directory -Path $RuntimeDirectory -Force | Out-Null
    Copy-Item -LiteralPath $sourceNative -Destination (Join-Path $RuntimeDirectory "OcctNative.dll") -Force

    $queue = [System.Collections.Generic.Queue[string]]::new()
    $queued = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    $processed = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    $queue.Enqueue((Join-Path $RuntimeDirectory "OcctNative.dll"))
    [void]$queued.Add("OcctNative.dll")

    while ($queue.Count -gt 0) {
        $current = $queue.Dequeue()
        $name = [System.IO.Path]::GetFileName($current)
        if (-not $processed.Add($name)) { continue }

        Repair-Occt790TbbImport $current
        foreach ($dependency in @(Get-ImportedDllNames $current)) {
            if (Test-SystemDependency $dependency) { continue }
            if (Test-DebugRuntime $dependency) {
                throw "Release portable SDK depends on a Microsoft debug runtime: $name -> $dependency"
            }

            $destinationPath = Join-Path $RuntimeDirectory $dependency
            if (-not (Test-Path -LiteralPath $destinationPath -PathType Leaf)) {
                $source = Resolve-Dependency $dependency
                if ($null -eq $source) {
                    throw "Portable runtime dependency was not found: $name -> $dependency"
                }
                Copy-Item -LiteralPath $source -Destination $destinationPath -Force
                Write-Host "[portable-runtime] $name -> $dependency" -ForegroundColor DarkGray
            }
            if ($queued.Add($dependency)) { $queue.Enqueue($destinationPath) }
        }
    }
}

function Copy-OcctResources {
    param([Parameter(Mandatory = $true)][string]$PackageRoot)

    $resourceRoot = Join-Path $PackageRoot "occt\resources"
    New-Item -ItemType Directory -Path $resourceRoot -Force | Out-Null
    $names = @("SHMessage", "XSMessage", "XSTEPResource", "XCAFResources", "StdResource", "Textures", "Shaders", "UnitsAPI")
    foreach ($name in $names) {
        $source = $null
        foreach ($candidate in @(
            (Join-Path $OcctRoot "src\$name"),
            (Join-Path $OcctRoot "share\opencascade\resources\$name"),
            (Join-Path $OcctRoot "share\opencascade\$name")
        )) {
            if (Test-Path -LiteralPath $candidate -PathType Container) { $source = $candidate; break }
        }
        if ($null -eq $source) { continue }
        Copy-Item -LiteralPath $source -Destination (Join-Path $resourceRoot $name) -Recurse -Force
        Write-Host "[portable-runtime] resource: $name" -ForegroundColor DarkGray
    }
}

function Copy-Notices {
    param([Parameter(Mandatory = $true)][string]$PackageRoot)
    foreach ($name in @("LICENSE", "LICENSE_LGPL_21.txt", "OcctCSharpBridge_LGPL_EXCEPTION.txt", "THIRD_PARTY_NOTICES.md", "COMMERCIAL.md")) {
        $source = Join-Path $RepoRoot $name
        if (Test-Path -LiteralPath $source -PathType Leaf) {
            Copy-Item -LiteralPath $source -Destination (Join-Path $PackageRoot $name) -Force
        }
    }
}

function Write-PortableReadme {
    param([Parameter(Mandatory = $true)][string]$PackageRoot)
    $text = @"
OcctCSharpBridge Portable SDK - Windows x64

This package contains the Bridge Binary SDK plus the OCCT runtime closure used to build it.
It does not bundle the .NET runtime.

Recommended application layout:
1. Copy the managed DLLs, runtime/ and occt/ directories beside the application executable.
2. Reference the required OcctNet*.dll assemblies from the application.
3. Call OcctRuntime.Configure() before creating the first OcctEngine or OcctModelingSession.

OcctRuntime automatically probes:
- <app>/runtime for OcctNative.dll and native dependencies
- <app>/occt for bundled OCCT resources

The target process must be Windows x64 and use a supported .NET runtime.
"@
    [System.IO.File]::WriteAllText(
        (Join-Path $PackageRoot "PORTABLE-SDK.txt"),
        $text.TrimStart() + [Environment]::NewLine,
        [System.Text.UTF8Encoding]::new($false))
}

function Write-PackageManifest {
    param(
        [Parameter(Mandatory = $true)][string]$PackageRoot,
        [Parameter(Mandatory = $true)]$Contract,
        [Parameter(Mandatory = $true)]$BridgeManifest
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

    $packageManifest = [ordered]@{
        schemaVersion = 1
        product = "OcctCSharpBridge Portable SDK"
        bridgeVersion = [string]$Contract.bridgeVersion
        bridgeSourceCommit = [string]$BridgeManifest.sourceCommit
        nativeAbi = [int]$Contract.nativeAbi.current
        occtVersion = [string]$Contract.occtVersion
        platform = "win-x64"
        configuration = "Release"
        portableRuntime = $true
        nativeDirectory = "runtime"
        occtRoot = "occt"
        dotnetRuntimeBundled = $false
        files = $files
    }

    [System.IO.File]::WriteAllText(
        $manifestPath,
        ($packageManifest | ConvertTo-Json -Depth 8) + [Environment]::NewLine,
        [System.Text.UTF8Encoding]::new($false))
}

Assert-Path $SdkRoot
Assert-Path $OcctRoot
Assert-Path $ContractPath
Assert-Path $ManifestPath
Assert-Path $OcctBinDir

$contract = Get-Content -LiteralPath $ContractPath -Raw -Encoding UTF8 | ConvertFrom-Json
$bridgeManifest = Get-Content -LiteralPath $ManifestPath -Raw -Encoding UTF8 | ConvertFrom-Json
if ([string]$contract.platform -ne "win-x64" -or [string]$bridgeManifest.configuration -ne "Release") {
    throw "Portable Windows SDK requires a validated Release win-x64 Binary SDK."
}

$script:Dumpbin = Resolve-Dumpbin
$script:VcRuntimeDirectories = @(Get-VcRuntimeDirectories)
$packageName = "OcctCSharpBridge-$($contract.bridgeVersion)-win-x64-portable"
$packageRoot = Join-Path $OutputDirectory $packageName
$runtimeDirectory = Join-Path $packageRoot "runtime"

Remove-Item -LiteralPath $packageRoot -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $packageRoot -Force | Out-Null

foreach ($name in @("OcctNet.dll", "OcctNet.WinForms.dll", "OcctNet.Wpf.dll", "OcctNet.Avalonia.dll", "bridge-contract.json", "bridge-manifest.json")) {
    $source = Join-Path $SdkRoot $name
    Assert-Path $source
    Copy-Item -LiteralPath $source -Destination (Join-Path $packageRoot $name) -Force
}

Copy-NativeClosure $runtimeDirectory
Copy-OcctResources $packageRoot
Copy-Notices $packageRoot
Write-PortableReadme $packageRoot
Write-PackageManifest $packageRoot $contract $bridgeManifest

if ($Zip.IsPresent) {
    $zipPath = "$packageRoot.zip"
    Remove-Item -LiteralPath $zipPath -Force -ErrorAction SilentlyContinue
    Compress-Archive -Path (Join-Path $packageRoot "*") -DestinationPath $zipPath -CompressionLevel Optimal
    Write-Host "Portable archive: $zipPath" -ForegroundColor Green
}

Write-Host "Portable SDK: $packageRoot" -ForegroundColor Green
