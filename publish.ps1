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
    [switch]$FullResources,
    [switch]$Diagnostics,
    [switch]$Zip,
    [switch]$KeepExisting
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$utf8 = [System.Text.UTF8Encoding]::new($false)
$utf8Bom = [System.Text.UTF8Encoding]::new($true)
[Console]::InputEncoding = $utf8
[Console]::OutputEncoding = $utf8
$OutputEncoding = $utf8
$env:DOTNET_CLI_UI_LANGUAGE = "en-US"
$env:VSLANG = "1033"

if (Test-Path "$env:SystemRoot\System32\chcp.com") {
    & "$env:SystemRoot\System32\chcp.com" 65001 | Out-Null
}

$Target = $Target.ToLowerInvariant()
if ($SelfContained.IsPresent -and $FrameworkDependent.IsPresent) {
    throw "Use either -SelfContained or -FrameworkDependent, not both."
}
$UseSelfContained = -not $FrameworkDependent.IsPresent
if ($SelfContained.IsPresent) {
    $UseSelfContained = $true
}

$RepoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
if ([string]::IsNullOrWhiteSpace($OcctRoot)) {
    throw "OCCT_ROOT is not configured. Pass -OcctRoot <path> or set the OCCT_ROOT environment variable."
}
$OcctRoot = [System.IO.Path]::GetFullPath($OcctRoot)
if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path $RepoRoot "artifacts\publish"
}
$OutputDirectory = [System.IO.Path]::GetFullPath($OutputDirectory)

$OcctBinDir = Join-Path $OcctRoot "win64\vc14\bin"
$OcctThirdPartyDir = Join-Path $OcctRoot "3rdparty-vc14-64"
$NativeDll = Join-Path $RepoRoot "build\native\bin\$Configuration\OcctNative.dll"
$PackageName = "OcctCSharpBridge-Demo-$Target-win-x64"
$PackageRoot = Join-Path $OutputDirectory $PackageName
$AppsRoot = Join-Path $PackageRoot "apps"
$RuntimeRoot = Join-Path $PackageRoot "runtime"
$OcctPackageRoot = Join-Path $PackageRoot "occt"
$LicenseRoot = Join-Path $PackageRoot "licenses"
$TemporaryRoot = Join-Path $OutputDirectory ".publish-temp-$Target"

$Projects = [ordered]@{
    winform = @{
        Name = "WinForms"
        Project = "src\CadWinForms\CadWinForms.csproj"
        Folder = "winform"
        Executable = "CAD-Winform.exe"
    }
    wpf = @{
        Name = "WPF"
        Project = "src\CadWpf\CadWpf.csproj"
        Folder = "wpf"
        Executable = "CAD-WPF.exe"
    }
    avalonia = @{
        Name = "Avalonia"
        Project = "src\CadAvalonia\CadAvalonia.csproj"
        Folder = "avalonia"
        Executable = "CAD-Avalonia.exe"
    }
}

function Assert-Path {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [string]$Description = "Required path"
    )
    if (-not (Test-Path $Path)) {
        throw "$Description was not found: $Path"
    }
}

function Assert-Command {
    param([Parameter(Mandatory = $true)][string]$Name)
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "$Name was not found in PATH."
    }
}

function Invoke-Checked {
    param(
        [Parameter(Mandatory = $true)][string]$Command,
        [Parameter(Mandatory = $true)][object[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$ErrorMessage
    )
    & $Command @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw $ErrorMessage
    }
}

function Get-SelectedApplicationKeys {
    if ($Target -eq "all") {
        return @("winform", "wpf", "avalonia")
    }
    return @($Target)
}

function Get-FileHashValue {
    param([Parameter(Mandatory = $true)][string]$Path)
    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash
}

function Copy-RuntimeDll {
    param(
        [Parameter(Mandatory = $true)][System.IO.FileInfo]$Source,
        [Parameter(Mandatory = $true)][string]$Category
    )

    $destination = Join-Path $RuntimeRoot $Source.Name
    if (Test-Path $destination -PathType Leaf) {
        if ((Get-FileHashValue $Source.FullName) -eq (Get-FileHashValue $destination)) {
            return
        }
        throw "Conflicting runtime DLL '$($Source.Name)' was found while copying $Category."
    }

    Copy-Item -LiteralPath $Source.FullName -Destination $destination -Force
}

function Publish-Application {
    param([Parameter(Mandatory = $true)][string]$Key)

    $application = $Projects[$Key]
    if ($null -eq $application) {
        throw "Unknown publish target: $Key"
    }

    $projectPath = Join-Path $RepoRoot $application.Project
    $destination = Join-Path $AppsRoot $application.Folder
    $temporaryDestination = Join-Path $TemporaryRoot $application.Folder
    Assert-Path $projectPath "$($application.Name) project"

    Remove-Item $destination -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item $temporaryDestination -Recurse -Force -ErrorAction SilentlyContinue
    New-Item $destination -ItemType Directory -Force | Out-Null
    New-Item $temporaryDestination -ItemType Directory -Force | Out-Null

    $arguments = @(
        "publish", $projectPath,
        "-c", $Configuration,
        "-r", "win-x64",
        "-p:Platform=x64",
        "-p:PublishSingleFile=true",
        "-p:EnableCompressionInSingleFile=$($UseSelfContained.ToString().ToLowerInvariant())",
        "-p:IncludeNativeLibrariesForSelfExtract=true",
        "-p:PublishReadyToRun=false",
        "-p:DebugType=None",
        "-p:DebugSymbols=false",
        "--self-contained", $UseSelfContained.ToString().ToLowerInvariant(),
        "--nologo",
        "-o", $temporaryDestination
    )

    Write-Host "[publish] $($application.Name) single-file executable..." -ForegroundColor Cyan
    Invoke-Checked "dotnet" $arguments "$($application.Name) publish failed."

    $executablePath = Join-Path $temporaryDestination $application.Executable
    Assert-Path $executablePath "$($application.Name) executable"
    Copy-Item $executablePath (Join-Path $destination $application.Executable) -Force

    Get-ChildItem $temporaryDestination -File | Where-Object {
        $_.Name -ne $application.Executable -and
        $_.Extension -notin @(".pdb", ".xml")
    } | ForEach-Object {
        Copy-Item $_.FullName (Join-Path $destination $_.Name) -Force
    }
}

function Resolve-Dumpbin {
    $command = Get-Command "dumpbin.exe" -ErrorAction SilentlyContinue
    if ($null -ne $command -and -not [string]::IsNullOrWhiteSpace($command.Path)) {
        return $command.Path
    }

    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswhere -PathType Leaf) {
        $matches = @(& $vswhere `
            -latest `
            -products * `
            -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 `
            -find "VC\Tools\MSVC\**\bin\Hostx64\x64\dumpbin.exe" 2>$null)
        foreach ($match in $matches) {
            if (-not [string]::IsNullOrWhiteSpace($match) -and (Test-Path $match -PathType Leaf)) {
                return [System.IO.Path]::GetFullPath($match)
            }
        }
    }

    throw "dumpbin.exe was not found. Install the Visual Studio C++ x64 build tools."
}

function Get-PeDependencies {
    param(
        [Parameter(Mandatory = $true)][string]$DumpbinPath,
        [Parameter(Mandatory = $true)][string]$BinaryPath
    )

    $output = @(& $DumpbinPath /nologo /dependents $BinaryPath 2>&1)
    if ($LASTEXITCODE -ne 0) {
        throw "Unable to inspect native dependencies: $BinaryPath"
    }

    $collecting = $false
    $dependencies = [System.Collections.Generic.List[string]]::new()
    foreach ($line in $output) {
        $value = ([string]$line).Trim()
        if ($value -match "^Image has the following (?:delay load )?dependencies:") {
            $collecting = $true
            continue
        }
        if ($value -eq "Summary") {
            $collecting = $false
            continue
        }
        if ($collecting -and $value -match "^[A-Za-z0-9_.+\-]+\.dll$") {
            $dependencies.Add($value)
        }
    }

    return @($dependencies | Sort-Object -Unique)
}

function Test-VisualCppRuntimeDependency {
    param([Parameter(Mandatory = $true)][string]$Name)
    return $Name -match "^(?i:concrt140|msvcp140(?:_[A-Za-z0-9]+)*|vcruntime140(?:_[A-Za-z0-9]+)*|vcomp140)\.dll$"
}

function Test-SystemDependency {
    param([Parameter(Mandatory = $true)][string]$Name)

    if (Test-VisualCppRuntimeDependency $Name) {
        return $false
    }
    if ($Name -match "^(?i:api-ms-win-|ext-ms-win-)") {
        return $true
    }

    return Test-Path (Join-Path ([Environment]::SystemDirectory) $Name) -PathType Leaf
}

function Test-RuntimeCandidate {
    param([Parameter(Mandatory = $true)][System.IO.FileInfo]$File)

    $path = $File.FullName.ToLowerInvariant()
    if ($path -match "[\\/](?:lib-static(?:-ucrt)?|static(?:-ucrt)?)[\\/]") {
        return $false
    }
    if ($path -match "[\\/](?:x86|win32|arm|arm64)[\\/]") {
        return $false
    }
    if ($path -match "[\\/](?:onecore|uwp|store|debug_nonredist)[\\/]") {
        return $false
    }
    return $true
}

function Get-RuntimeCandidateVersion {
    param([Parameter(Mandatory = $true)][System.IO.FileInfo]$File)

    $pathMatch = [regex]::Match(
        $File.FullName,
        "(?i)[\\/]VC[\\/]Redist[\\/]MSVC[\\/](?<version>[0-9]+(?:\.[0-9]+){1,3})[\\/]")
    if ($pathMatch.Success) {
        try { return [version]$pathMatch.Groups["version"].Value } catch { }
    }

    $fileVersion = $File.VersionInfo.FileVersion
    if (-not [string]::IsNullOrWhiteSpace($fileVersion)) {
        $versionMatch = [regex]::Match($fileVersion, "[0-9]+(?:\.[0-9]+){1,3}")
        if ($versionMatch.Success) {
            try { return [version]$versionMatch.Value } catch { }
        }
    }

    return [version]"0.0"
}

function Get-RuntimeCandidateScore {
    param([Parameter(Mandatory = $true)][System.IO.FileInfo]$File)

    $path = $File.FullName.ToLowerInvariant()
    $score = 0
    if ([string]::Equals($File.DirectoryName, $OcctBinDir, [StringComparison]::OrdinalIgnoreCase)) {
        $score += 100000
    }
    if ($path -match "[\\/]vc[\\/]redist[\\/]msvc[\\/][^\\/]+[\\/]x64[\\/]") {
        $score += 20000
    }
    if ($path -match "[\\/](?:bin|bin64)[\\/]") { $score += 5000 }
    if ($path -match "[\\/](?:vc2022|vc143|vc14\.4)[\\/]") { $score += 4000 }
    elseif ($path -match "[\\/](?:vc2019|vc142)[\\/]") { $score += 3000 }
    elseif ($path -match "[\\/](?:vc2017|vc141)[\\/]") { $score += 2000 }
    if ($path -match "(?:x64|amd64|win64)") { $score += 300 }
    if ($Configuration -eq "Debug") {
        if ($path -match "[\\/]debug[\\/]" -or $File.Name -match "(?i)(?:_debug|debug|d)\.dll$") {
            $score += 500
        }
    }
    elseif ($path -notmatch "[\\/]debug[\\/]") {
        $score += 200
    }
    return $score
}

function Get-VisualCppRuntimeFiles {
    $result = [System.Collections.Generic.List[System.IO.FileInfo]]::new()
    $seen = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)

    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswhere -PathType Leaf) {
        $matches = @(& $vswhere `
            -latest `
            -products * `
            -requires Microsoft.VisualStudio.Component.VC.Redist.14.Latest `
            -find "VC\Redist\MSVC\**\x64\**\*.dll" 2>$null)
        foreach ($match in $matches) {
            if ([string]::IsNullOrWhiteSpace($match) -or -not (Test-Path $match -PathType Leaf)) { continue }
            $file = Get-Item -LiteralPath $match
            if ((Test-RuntimeCandidate $file) -and
                (Test-VisualCppRuntimeDependency $file.Name) -and
                $seen.Add($file.FullName)) {
                $result.Add($file)
            }
        }
    }

    foreach ($name in @(
        "concrt140.dll",
        "msvcp140.dll",
        "msvcp140_1.dll",
        "msvcp140_2.dll",
        "msvcp140_atomic_wait.dll",
        "msvcp140_codecvt_ids.dll",
        "vcruntime140.dll",
        "vcruntime140_1.dll",
        "vcruntime140_threads.dll",
        "vcomp140.dll"
    )) {
        $path = Join-Path ([Environment]::SystemDirectory) $name
        if (Test-Path $path -PathType Leaf) {
            $file = Get-Item -LiteralPath $path
            if ($seen.Add($file.FullName)) {
                $result.Add($file)
            }
        }
    }

    return @($result)
}

function New-RuntimeCandidateIndex {
    $index = @{}
    $files = [System.Collections.Generic.List[System.IO.FileInfo]]::new()

    Get-ChildItem $OcctBinDir -File -Filter "*.dll" | ForEach-Object { $files.Add($_) }
    Get-VisualCppRuntimeFiles | ForEach-Object { $files.Add($_) }

    if (Test-Path $OcctThirdPartyDir -PathType Container) {
        Get-ChildItem $OcctThirdPartyDir -Recurse -File -Filter "*.dll" | Where-Object {
            Test-RuntimeCandidate $_
        } | ForEach-Object { $files.Add($_) }
    }

    foreach ($file in $files) {
        $key = $file.Name.ToLowerInvariant()
        if (-not $index.ContainsKey($key)) {
            $index[$key] = [System.Collections.Generic.List[System.IO.FileInfo]]::new()
        }
        $index[$key].Add($file)
    }
    return $index
}

function Resolve-RuntimeDependency {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][hashtable]$CandidateIndex
    )

    $key = $Name.ToLowerInvariant()
    if (-not $CandidateIndex.ContainsKey($key)) {
        return $null
    }

    $ranked = @($CandidateIndex[$key] | Where-Object {
        Test-RuntimeCandidate $_
    } | ForEach-Object {
        [pscustomobject]@{
            File = $_
            Score = Get-RuntimeCandidateScore $_
            Version = Get-RuntimeCandidateVersion $_
        }
    } | Sort-Object -Property `
        @{ Expression = "Score"; Descending = $true }, `
        @{ Expression = "Version"; Descending = $true }, `
        @{ Expression = { $_.File.FullName }; Descending = $false })

    if ($ranked.Count -eq 0) {
        return $null
    }

    $topScore = $ranked[0].Score
    $topVersion = $ranked[0].Version
    $top = @($ranked | Where-Object {
        $_.Score -eq $topScore -and $_.Version -eq $topVersion
    })

    if ($top.Count -gt 1) {
        $hashGroups = @($top | Group-Object { Get-FileHashValue $_.File.FullName })
        if ($hashGroups.Count -gt 1) {
            $paths = ($top | ForEach-Object { "  $($_.File.FullName)" }) -join "`n"
            throw "Ambiguous required runtime DLL '$Name':`n$paths"
        }
    }

    return $ranked[0].File
}

function Copy-OcctRuntime {
    Assert-Path $OcctBinDir "OCCT runtime directory"
    $rootBinary = Join-Path $RuntimeRoot "OcctNative.dll"
    Assert-Path $rootBinary "Packaged OcctNative.dll"

    $dumpbin = Resolve-Dumpbin
    $candidateIndex = New-RuntimeCandidateIndex
    $queue = [System.Collections.Generic.Queue[string]]::new()
    $processed = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    $records = [System.Collections.Generic.List[string]]::new()
    $queue.Enqueue($rootBinary)

    Write-Host "[runtime] Resolving complete native dependency closure..." -ForegroundColor Cyan
    while ($queue.Count -gt 0) {
        $binary = [System.IO.Path]::GetFullPath($queue.Dequeue())
        if (-not $processed.Add($binary)) {
            continue
        }

        foreach ($dependency in Get-PeDependencies $dumpbin $binary) {
            if (Test-SystemDependency $dependency) {
                continue
            }

            $destination = Join-Path $RuntimeRoot $dependency
            if (-not (Test-Path $destination -PathType Leaf)) {
                $source = Resolve-RuntimeDependency $dependency $candidateIndex
                if ($null -eq $source) {
                    throw "Required native dependency '$dependency' imported by '$binary' was not found."
                }
                Copy-RuntimeDll $source "required native dependency"
                $records.Add("$([System.IO.Path]::GetFileName($binary)) -> $dependency <- $($source.FullName)")
            }
            else {
                $records.Add("$([System.IO.Path]::GetFileName($binary)) -> $dependency")
            }

            $queue.Enqueue($destination)
        }
    }

    if ($Diagnostics) {
        [System.IO.File]::WriteAllLines(
            (Join-Path $PackageRoot "native-resolution.txt"),
            @($records | Sort-Object -Unique),
            $utf8Bom)
    }

    Write-Host "[runtime] Resolved $($processed.Count) native binaries." -ForegroundColor Green
}

function Copy-VisualCppRuntime {
    $candidateIndex = New-RuntimeCandidateIndex
    $names = @(
        "concrt140.dll",
        "msvcp140.dll",
        "msvcp140_1.dll",
        "msvcp140_2.dll",
        "msvcp140_atomic_wait.dll",
        "msvcp140_codecvt_ids.dll",
        "vcruntime140.dll",
        "vcruntime140_1.dll",
        "vcruntime140_threads.dll",
        "vcomp140.dll"
    )

    foreach ($name in $names) {
        $source = Resolve-RuntimeDependency $name $candidateIndex
        if ($null -ne $source) {
            Copy-RuntimeDll $source "Visual C++ runtime"
        }
    }
}

function Resolve-OcctResourceRoot {
    foreach ($candidate in @(
        (Join-Path $OcctRoot "src"),
        (Join-Path $OcctRoot "share\opencascade\resources"),
        (Join-Path $OcctRoot "resources")
    )) {
        if (Test-Path $candidate -PathType Container) {
            return $candidate
        }
    }
    throw "OCCT resources were not found below the configured OCCT root: $OcctRoot"
}

function Copy-OcctResources {
    $sourceRoot = Resolve-OcctResourceRoot
    $resourceNames = [System.Collections.Generic.List[string]]::new()
    foreach ($name in @("Shaders", "StdResource", "UnitsAPI", "SHMessage", "XSMessage", "XSTEPResource")) {
        $resourceNames.Add($name)
    }
    if ($FullResources) {
        $resourceNames.Add("Textures")
    }

    $destinationRoot = Join-Path $OcctPackageRoot "src"
    New-Item $destinationRoot -ItemType Directory -Force | Out-Null

    foreach ($name in $resourceNames) {
        $source = Join-Path $sourceRoot $name
        if (-not (Test-Path $source -PathType Container)) {
            throw "Required OCCT resource directory was not found: $name"
        }
        Copy-Item $source (Join-Path $destinationRoot $name) -Recurse -Force
    }
}

function Test-PackagedNativeClosure {
    $dumpbin = Resolve-Dumpbin
    $runtimeFiles = @(Get-ChildItem $RuntimeRoot -File -Filter "*.dll" | Sort-Object Name)
    if ($runtimeFiles.Count -eq 0) {
        throw "The packaged runtime directory contains no DLL files."
    }

    $packagedNames = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    foreach ($file in $runtimeFiles) {
        [void]$packagedNames.Add($file.Name)
    }

    $rows = [System.Collections.Generic.List[string]]::new()
    $unresolved = [System.Collections.Generic.List[string]]::new()
    foreach ($file in $runtimeFiles) {
        $dependencies = @(Get-PeDependencies $dumpbin $file.FullName)
        $rows.Add("$($file.Name)`t$($dependencies -join ',')")
        foreach ($dependency in $dependencies) {
            if ($packagedNames.Contains($dependency)) { continue }
            if (Test-SystemDependency $dependency) { continue }
            $unresolved.Add("$($file.Name) -> $dependency")
        }
    }

    [System.IO.File]::WriteAllLines(
        (Join-Path $PackageRoot "native-dependencies.txt"),
        @("Binary`tDependencies") + $rows,
        $utf8Bom)

    if ($unresolved.Count -gt 0) {
        throw "The package has unresolved non-system native dependencies:`n$($unresolved -join "`n")"
    }
}

function Copy-NativeRuntimeToApplications {
    $runtimeFiles = @(Get-ChildItem $RuntimeRoot -File -Filter "*.dll")
    if ($runtimeFiles.Count -eq 0) {
        throw "No native runtime DLLs are available for application-local deployment."
    }

    foreach ($key in Get-SelectedApplicationKeys) {
        $application = $Projects[$key]
        $appDirectory = Join-Path $AppsRoot $application.Folder
        Assert-Path $appDirectory "$($application.Name) package directory"

        foreach ($file in $runtimeFiles) {
            $destination = Join-Path $appDirectory $file.Name
            if (Test-Path $destination -PathType Leaf) {
                if ((Get-FileHashValue $file.FullName) -ne (Get-FileHashValue $destination)) {
                    throw "Application-local DLL conflicts with the packaged runtime: $destination"
                }
                continue
            }
            Copy-Item $file.FullName $destination -Force
        }

        Assert-Path (Join-Path $appDirectory "OcctNative.dll") "$($application.Name) application-local native bridge"
    }

    Write-Host "[runtime] Native dependency closure copied beside every executable." -ForegroundColor Green
}

function Test-PackagedNativeLoad {
    $probePath = Join-Path $TemporaryRoot "native-load-probe.ps1"
    New-Item $TemporaryRoot -ItemType Directory -Force | Out-Null

    $probe = @'
param(
    [Parameter(Mandatory = $true)][string]$NativePath,
    [Parameter(Mandatory = $true)][string]$SearchDirectory
)
$ErrorActionPreference = "Stop"
Add-Type -TypeDefinition @"
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

public static class OcctPackageNativeProbe
{
    private const uint LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR = 0x00000100;
    private const uint LOAD_LIBRARY_SEARCH_USER_DIRS = 0x00000400;
    private const uint LOAD_LIBRARY_SEARCH_SYSTEM32 = 0x00000800;

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetDefaultDllDirectories(uint directoryFlags);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr AddDllDirectory(string newDirectory);

    [DllImport("kernel32.dll", EntryPoint = "LoadLibraryExW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr LoadLibraryEx(string fileName, IntPtr file, uint flags);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool FreeLibrary(IntPtr module);

    public static void Load(string nativePath, string searchDirectory)
    {
        uint defaults = LOAD_LIBRARY_SEARCH_USER_DIRS | LOAD_LIBRARY_SEARCH_SYSTEM32;
        if (!SetDefaultDllDirectories(defaults))
        {
            int code = Marshal.GetLastWin32Error();
            throw new Win32Exception(code, "SetDefaultDllDirectories failed.");
        }

        if (AddDllDirectory(searchDirectory) == IntPtr.Zero)
        {
            int code = Marshal.GetLastWin32Error();
            throw new Win32Exception(code, "AddDllDirectory failed.");
        }

        uint flags = LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR |
                     LOAD_LIBRARY_SEARCH_USER_DIRS |
                     LOAD_LIBRARY_SEARCH_SYSTEM32;
        IntPtr module = LoadLibraryEx(nativePath, IntPtr.Zero, flags);
        if (module == IntPtr.Zero)
        {
            int code = Marshal.GetLastWin32Error();
            throw new Win32Exception(code, "Unable to load packaged OcctNative.dll.");
        }

        FreeLibrary(module);
    }
}
"@
[OcctPackageNativeProbe]::Load(
    [System.IO.Path]::GetFullPath($NativePath),
    [System.IO.Path]::GetFullPath($SearchDirectory))
'@

    [System.IO.File]::WriteAllText($probePath, $probe, $utf8Bom)
    $powerShellExecutable = (Get-Process -Id $PID).Path
    Assert-Path $powerShellExecutable "Current PowerShell executable"

    foreach ($key in Get-SelectedApplicationKeys) {
        $application = $Projects[$key]
        $appDirectory = Join-Path $AppsRoot $application.Folder
        $nativePath = Join-Path $appDirectory "OcctNative.dll"

        Write-Host "[probe] Loading $($application.Name) app-local OcctNative.dll with restricted DLL search..." -ForegroundColor Cyan
        & $powerShellExecutable `
            -NoLogo `
            -NoProfile `
            -NonInteractive `
            -ExecutionPolicy Bypass `
            -File $probePath `
            -NativePath $nativePath `
            -SearchDirectory $appDirectory

        if ($LASTEXITCODE -ne 0) {
            throw "$($application.Name) native load probe failed. The package would not be portable to a clean Windows machine."
        }
    }

    Write-Host "[probe] Packaged native load probe passed for all selected applications." -ForegroundColor Green
}

function Add-LicenseSection {
    param(
        [Parameter(Mandatory = $true)][System.Text.StringBuilder]$Builder,
        [Parameter(Mandatory = $true)][string]$Title,
        [Parameter(Mandatory = $true)][string]$Path
    )

    if (-not (Test-Path $Path -PathType Leaf)) {
        return
    }

    [void]$Builder.AppendLine("================================================================================")
    [void]$Builder.AppendLine($Title)
    [void]$Builder.AppendLine("Source: $Path")
    [void]$Builder.AppendLine("================================================================================")
    [void]$Builder.AppendLine((Get-Content $Path -Raw -ErrorAction Stop))
    [void]$Builder.AppendLine()
}

function Write-LicenseFiles {
    New-Item $LicenseRoot -ItemType Directory -Force | Out-Null

    $projectLicense = Join-Path $RepoRoot "LICENSE"
    if (Test-Path $projectLicense -PathType Leaf) {
        Copy-Item $projectLicense (Join-Path $LicenseRoot "OcctCSharpBridge-LICENSE.txt") -Force
    }

    $occtBuilder = [System.Text.StringBuilder]::new()
    Get-ChildItem $OcctRoot -File -ErrorAction SilentlyContinue | Where-Object {
        $_.Name -match "(?i)license|copying|notice|exception"
    } | Sort-Object FullName | ForEach-Object {
        Add-LicenseSection $occtBuilder $_.Name $_.FullName
    }
    if ($occtBuilder.Length -gt 0) {
        [System.IO.File]::WriteAllText(
            (Join-Path $LicenseRoot "OCCT-LICENSES.txt"),
            $occtBuilder.ToString(),
            $utf8Bom)
    }

    if (Test-Path $OcctThirdPartyDir -PathType Container) {
        $thirdPartyBuilder = [System.Text.StringBuilder]::new()
        Get-ChildItem $OcctThirdPartyDir -Recurse -File -ErrorAction SilentlyContinue | Where-Object {
            $_.Name -match "(?i)^(license|copying|notice)(\.|$)"
        } | Sort-Object FullName | ForEach-Object {
            $relative = $_.FullName.Substring($OcctThirdPartyDir.Length).TrimStart('\', '/')
            Add-LicenseSection $thirdPartyBuilder $relative $_.FullName
        }
        if ($thirdPartyBuilder.Length -gt 0) {
            [System.IO.File]::WriteAllText(
                (Join-Path $LicenseRoot "THIRD-PARTY-NOTICES.txt"),
                $thirdPartyBuilder.ToString(),
                $utf8Bom)
        }
    }
}

function Write-PackageContract {
    $applications = [System.Collections.Generic.List[object]]::new()
    foreach ($key in Get-SelectedApplicationKeys) {
        $application = $Projects[$key]
        $relativeExecutable = Join-Path (Join-Path "apps" $application.Folder) $application.Executable
        $fullExecutable = Join-Path $PackageRoot $relativeExecutable
        Assert-Path $fullExecutable "$($application.Name) packaged executable"

        $applications.Add([ordered]@{
            name = $application.Name
            executable = $relativeExecutable.Replace('\', '/')
            selfContained = [bool]$UseSelfContained
            appLocalNativeRuntime = $true
        })
    }

    $resources = @(Get-ChildItem (Join-Path $OcctPackageRoot "src") -Directory |
        Sort-Object Name |
        Select-Object -ExpandProperty Name)

    $contract = [ordered]@{
        schemaVersion = 2
        packageName = $PackageName
        platform = "windows-x64"
        configuration = $Configuration
        selfContained = [bool]$UseSelfContained
        managedRuntime = if ($UseSelfContained) { "embedded-in-application" } else { "requires-.NET-8-Desktop-Runtime" }
        applications = $applications
        nativeRuntimeDirectory = "runtime"
        nativeRuntimeDeployment = "app-local-copy"
        occtRootDirectory = "occt"
        occtResourceDirectories = $resources
        dependencyManifest = "native-dependencies.txt"
        licenseDirectory = "licenses"
        contact = "zhangly1403@gmail.com"
    }

    $json = $contract | ConvertTo-Json -Depth 8
    [System.IO.File]::WriteAllText((Join-Path $PackageRoot "package-contract.json"), $json, $utf8Bom)
}

function Write-PackageReadme {
    $runtimeMode = if ($UseSelfContained) {
        "Self-contained single-file application."
    }
    else {
        "Framework-dependent single-file application; install the .NET 8 Desktop Runtime."
    }

    $selected = @(Get-SelectedApplicationKeys | ForEach-Object {
        "apps\$($Projects[$_].Folder)\$($Projects[$_].Executable)"
    })

    $lines = [System.Collections.Generic.List[string]]::new()
    foreach ($line in @(
        "OcctCSharpBridge Demo",
        "=====================",
        "",
        "Platform: Windows x64",
        "Target: $Target",
        "Runtime: $runtimeMode",
        "Contact: zhangly1403@gmail.com",
        "",
        "Runnable applications:"
    )) { $lines.Add($line) }
    foreach ($path in $selected) { $lines.Add("  $path") }
    foreach ($line in @(
        "",
        "Each application folder contains its own OcctNative.dll and complete native dependency closure.",
        "This app-local layout is intentional: a copied package can be started directly on a clean Windows machine",
        "without relying on OCCT or Visual C++ DLLs installed on the developer computer.",
        "",
        "Keep the apps and occt directories together. The top-level runtime directory is the canonical",
        "dependency manifest copy used for diagnostics and package validation.",
        "",
        "The default package is self-contained and does not require a separate .NET installation.",
        "Use -FrameworkDependent only when target machines already have the .NET 8 Desktop Runtime.",
        "Use -FullResources only when texture resources are needed.",
        "Use -Diagnostics to add native resolution and file manifests."
    )) { $lines.Add($line) }

    [System.IO.File]::WriteAllLines((Join-Path $PackageRoot "README.txt"), $lines, $utf8Bom)
}

function Write-Manifest {
    if (-not $Diagnostics) {
        return
    }

    $entries = Get-ChildItem $PackageRoot -Recurse -File | Sort-Object FullName | ForEach-Object {
        $relative = $_.FullName.Substring($PackageRoot.Length).TrimStart('\', '/')
        $hash = Get-FileHashValue $_.FullName
        "{0}`t{1}`t{2}" -f $relative, $_.Length, $hash
    }

    [System.IO.File]::WriteAllLines(
        (Join-Path $PackageRoot "runtime-manifest.txt"),
        @("RelativePath`tBytes`tSHA256") + $entries,
        $utf8Bom)
}

Assert-Command "dotnet"
Assert-Path (Join-Path $RepoRoot "build.ps1") "Build script"
Assert-Path $OcctRoot "OCCT root"
Assert-Path $OcctBinDir "OCCT runtime directory"

Write-Host "Target:             $Target"
Write-Host "Configuration:      $Configuration"
Write-Host "OCCT root:          $OcctRoot"
Write-Host "Output directory:   $OutputDirectory"
Write-Host "Self-contained:     $UseSelfContained"
Write-Host "Full resources:     $($FullResources.IsPresent)"

if ((Test-Path $PackageRoot) -and -not $KeepExisting) {
    Remove-Item $PackageRoot -Recurse -Force
}
Remove-Item $TemporaryRoot -Recurse -Force -ErrorAction SilentlyContinue
New-Item $AppsRoot -ItemType Directory -Force | Out-Null
New-Item $RuntimeRoot -ItemType Directory -Force | Out-Null
New-Item $OcctPackageRoot -ItemType Directory -Force | Out-Null
New-Item $TemporaryRoot -ItemType Directory -Force | Out-Null

try {
    Write-Host "[build] Building native bridge..." -ForegroundColor Cyan
    & (Join-Path $RepoRoot "build.ps1") native $Configuration -OcctRoot $OcctRoot
    if (-not $?) {
        throw "Native bridge build failed."
    }

    Assert-Path $NativeDll "OcctNative.dll"
    Copy-Item $NativeDll (Join-Path $RuntimeRoot "OcctNative.dll") -Force

    foreach ($key in Get-SelectedApplicationKeys) {
        Publish-Application $key
    }

    Copy-OcctRuntime
    Copy-VisualCppRuntime
    Copy-OcctResources
    Test-PackagedNativeClosure
    Copy-NativeRuntimeToApplications
    Test-PackagedNativeLoad
    Write-LicenseFiles
    Write-PackageReadme
    Write-PackageContract
    Write-Manifest

    if ($Zip) {
        $zipPath = Join-Path $OutputDirectory "$PackageName.zip"
        Remove-Item $zipPath -Force -ErrorAction SilentlyContinue
        Compress-Archive -Path (Join-Path $PackageRoot "*") -DestinationPath $zipPath -CompressionLevel Optimal
        Write-Host "ZIP: $zipPath" -ForegroundColor Green
    }

    $files = @(Get-ChildItem $PackageRoot -Recurse -File)
    $totalBytes = ($files | Measure-Object Length -Sum).Sum
    Write-Host "Package: $PackageRoot" -ForegroundColor Green
    Write-Host "Files:   $($files.Count)" -ForegroundColor Green
    Write-Host ("Size:    {0:N2} MB" -f ($totalBytes / 1MB)) -ForegroundColor Green
}
finally {
    Remove-Item $TemporaryRoot -Recurse -Force -ErrorAction SilentlyContinue
}
