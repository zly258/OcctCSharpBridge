param(
    [Parameter(Position = 0)]
    [ValidateSet("Debug", "Release", "RelWithDebInfo")]
    [string]$Configuration = "Release",

    [string]$OcctRoot = $env:OCCT_ROOT,
    [string]$OutputDirectory = "",
    [switch]$FrameworkDependent,
    [switch]$NoZip
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$RepoRoot = Split-Path -Parent $PSCommandPath
$BuildScript = Join-Path $RepoRoot "build.ps1"
$Project = Join-Path $RepoRoot "src\OcctDemo.Avalonia\OcctDemo.Avalonia.csproj"
$ContractPath = Join-Path $RepoRoot "bridge-contract.json"
$NativeDll = Join-Path $RepoRoot "build\native\bin\$Configuration\OcctNative.dll"
$DefaultOcctRoot = "D:\tools\occt-vc144-64"
if ([string]::IsNullOrWhiteSpace($OcctRoot)) { $OcctRoot = $DefaultOcctRoot }
$OcctRoot = [System.IO.Path]::GetFullPath($OcctRoot)
$OcctBinDir = Join-Path $OcctRoot "win64\vc14\bin"
$OcctThirdPartyDir = Join-Path $OcctRoot "3rdparty-vc14-64"
if ([string]::IsNullOrWhiteSpace($OutputDirectory)) { $OutputDirectory = Join-Path $RepoRoot "artifacts\publish" }
$OutputDirectory = [System.IO.Path]::GetFullPath($OutputDirectory)
$PackageName = "CAD-Avalonia-win-x64"
$PackageRoot = Join-Path $OutputDirectory $PackageName
$StagingRoot = Join-Path $OutputDirectory (".$PackageName-staging-$PID")
$ZipPath = Join-Path $OutputDirectory "$PackageName.zip"
$UseSelfContained = -not $FrameworkDependent.IsPresent
$RunningOnWindows = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)

function Assert-Path([string]$Path) {
    if (-not (Test-Path -LiteralPath $Path)) { throw "Required path was not found: $Path" }
}

function Assert-Command([string]$Name) {
    if ($null -eq (Get-Command $Name -ErrorAction SilentlyContinue)) { throw "$Name was not found in PATH." }
}

function Invoke-Checked([string]$Command, [object[]]$Arguments, [string]$ErrorMessage) {
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
    $roots = @($env:ProgramFiles, ${env:ProgramFiles(x86)}) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    foreach ($root in $roots) {
        $pattern = Join-Path $root "Microsoft Visual Studio\2022\*\VC\Tools\MSVC\*\bin\Hostx64\x64\dumpbin.exe"
        $match = Get-Item -Path $pattern -ErrorAction SilentlyContinue | Sort-Object FullName -Descending | Select-Object -First 1
        if ($null -ne $match) { return $match.FullName }
    }
    throw "dumpbin.exe was not found. Install Visual Studio 2022 C++ build tools or run from a Developer PowerShell."
}

function Get-ImportedDllNames([string]$Path) {
    $lines = & $script:Dumpbin /nologo /dependents $Path 2>$null
    if ($LASTEXITCODE -ne 0) { throw "dumpbin failed for $Path" }
    return @($lines |
        ForEach-Object { ([string]$_).Trim() } |
        Where-Object { $_ -match '(?i)^[A-Za-z0-9_.+-]+\.dll$' } |
        Sort-Object -Unique)
}

function Get-VcRuntimeDirectories {
    $result = [System.Collections.Generic.List[string]]::new()
    if (-not [string]::IsNullOrWhiteSpace($env:VCToolsRedistDir)) {
        Get-Item -Path (Join-Path $env:VCToolsRedistDir "x64\Microsoft.VC14*.CRT") -ErrorAction SilentlyContinue |
            Where-Object PSIsContainer | ForEach-Object { $result.Add($_.FullName) }
    }
    $roots = @($env:ProgramFiles, ${env:ProgramFiles(x86)}) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    foreach ($root in $roots) {
        $pattern = Join-Path $root "Microsoft Visual Studio\2022\*\VC\Redist\MSVC\*\x64\Microsoft.VC14*.CRT"
        Get-Item -Path $pattern -ErrorAction SilentlyContinue | Sort-Object FullName -Descending |
            Where-Object PSIsContainer | ForEach-Object { if (-not $result.Contains($_.FullName)) { $result.Add($_.FullName) } }
    }
    return @($result)
}

function Test-DebugRuntime([string]$Name) {
    return $Name -ieq "ucrtbased.dll" -or $Name -match '(?i)^(MSVCP|VCRUNTIME|CONCRT|VCCORLIB).*D\.dll$'
}

function Test-SystemDependency([string]$Name) {
    if ($Name -match '(?i)^(api-ms-win-|ext-ms-win-)') { return $true }
    if ($Name -match '(?i)^(msvcp|vcruntime|concrt|vccorlib)') { return $false }
    return Test-Path -LiteralPath (Join-Path $env:SystemRoot "System32\$Name") -PathType Leaf
}

function Resolve-Dependency([string]$Name) {
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

function Replace-AsciiImport([string]$Path, [string]$OldName, [string]$NewName) {
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

function Repair-Occt790TbbImport([string]$Path) {
    if ($Configuration -ne "Release") { return }
    foreach ($dependency in @(Get-ImportedDllNames $Path)) {
        if ($dependency -notmatch '(?i)^tbb.*_debug\.dll$') { continue }
        $releaseName = [regex]::Replace($dependency, '(?i)_debug(?=\.dll$)', '')
        if ($null -eq (Resolve-Dependency $releaseName)) { throw "OCCT 7.9.0 imports $dependency but release counterpart $releaseName was not found." }
        Replace-AsciiImport $Path $dependency $releaseName
        Write-Host "[runtime] corrected OCCT 7.9.0 import: $dependency -> $releaseName" -ForegroundColor Yellow
    }
}

function Copy-NativeClosure([string]$Destination) {
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

function Copy-OcctResource([string]$Name) {
    $candidates = @(
        (Join-Path $OcctRoot "src\$Name"),
        (Join-Path $OcctRoot "share\opencascade\resources\$Name")
    )
    foreach ($candidate in $candidates) {
        if (-not (Test-Path -LiteralPath $candidate -PathType Container)) { continue }
        $destination = Join-Path $PackageRoot "occt\resources\$Name"
        New-Item -ItemType Directory -Path (Split-Path -Parent $destination) -Force | Out-Null
        Copy-Item -LiteralPath $candidate -Destination $destination -Recurse -Force
        return
    }
}

if (-not $RunningOnWindows) { throw "publish.ps1 must run on Windows. Use ./publish.sh on Linux." }
Assert-Command dotnet
Assert-Path $BuildScript
Assert-Path $Project
Assert-Path $ContractPath
$script:Dumpbin = Resolve-Dumpbin
$script:VcRuntimeDirectories = @(Get-VcRuntimeDirectories)

& $BuildScript $Configuration -OcctRoot $OcctRoot
Assert-Path $NativeDll
Assert-Path $OcctBinDir

Remove-Item -LiteralPath $PackageRoot -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath $StagingRoot -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $PackageRoot -Force | Out-Null
New-Item -ItemType Directory -Path $StagingRoot -Force | Out-Null

$publishArgs = @("publish", $Project, "-c", $Configuration, "-r", "win-x64", "-p:Platform=x64", "--nologo", "-o", $StagingRoot)
if ($UseSelfContained) { $publishArgs += @("--self-contained", "true") } else { $publishArgs += @("--self-contained", "false") }
Invoke-Checked "dotnet" $publishArgs "Avalonia Windows publish failed."
Copy-Item -Path (Join-Path $StagingRoot "*") -Destination $PackageRoot -Recurse -Force
Copy-NativeClosure $PackageRoot
Copy-Item -LiteralPath $ContractPath -Destination (Join-Path $PackageRoot "bridge-contract.json") -Force
foreach ($notice in @("LICENSE", "LICENSE_LGPL_21.txt", "OcctCSharpBridge_LGPL_EXCEPTION.txt", "THIRD_PARTY_NOTICES.md", "COMMERCIAL.md")) {
    $path = Join-Path $RepoRoot $notice
    if (Test-Path -LiteralPath $path -PathType Leaf) { Copy-Item -LiteralPath $path -Destination (Join-Path $PackageRoot $notice) -Force }
}
foreach ($resource in @("SHMessage", "XSMessage", "XSTEPResource", "XCAFResources", "StdResource", "Textures")) { Copy-OcctResource $resource }

$runCmd = @'
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
start "" "%APP_DIR%CAD-Avalonia.exe" %*
'@
[IO.File]::WriteAllText((Join-Path $PackageRoot "run.cmd"), $runCmd, [Text.UTF8Encoding]::new($false))

$manifest = @(
    "CAD-Avalonia Windows x64",
    "Configuration: $Configuration",
    "Self-contained: $UseSelfContained",
    "OCCT source root: $OcctRoot",
    "",
    "Files:"
)
$manifest += Get-ChildItem -LiteralPath $PackageRoot -File -Recurse | Sort-Object FullName | ForEach-Object {
    $relative = $_.FullName.Substring($PackageRoot.Length).TrimStart([IO.Path]::DirectorySeparatorChar, [IO.Path]::AltDirectorySeparatorChar)
    $hash = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash
    "$hash  $relative"
}
[IO.File]::WriteAllLines((Join-Path $PackageRoot "publish-manifest.txt"), $manifest, [Text.UTF8Encoding]::new($false))

Remove-Item -LiteralPath $StagingRoot -Recurse -Force -ErrorAction SilentlyContinue
if (-not $NoZip.IsPresent) {
    Remove-Item -LiteralPath $ZipPath -Force -ErrorAction SilentlyContinue
    Compress-Archive -Path $PackageRoot -DestinationPath $ZipPath -CompressionLevel Optimal
    Write-Host "Archive: $ZipPath" -ForegroundColor Green
}
Write-Host "Package: $PackageRoot" -ForegroundColor Green
