param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Get-TrackedNativeFiles {
    param([string[]]$Extensions = @())

    $tracked = @(& git -C $RepositoryRoot ls-files -- "src/OcctNative/**" 2>$null)
    if ($LASTEXITCODE -ne 0) { throw "Unable to inspect tracked native files with git ls-files." }

    $files = @()
    foreach ($relativePath in $tracked) {
        if ($Extensions.Count -gt 0) {
            $extension = [System.IO.Path]::GetExtension($relativePath)
            if ($extension -notin $Extensions) { continue }
        }
        $fullPath = Join-Path $RepositoryRoot $relativePath
        if (-not (Test-Path $fullPath -PathType Leaf)) {
            throw "Tracked native source file is missing from the working tree: $relativePath"
        }
        $files += Get-Item $fullPath
    }
    return @($files)
}

$nativeRoot = Join-Path $RepositoryRoot "src\OcctNative"
$cmakePath = Join-Path $nativeRoot "CMakeLists.txt"
if (-not (Test-Path $cmakePath -PathType Leaf)) { throw "Native CMakeLists.txt was not found: $cmakePath" }

$trackedNativeFiles = @(Get-TrackedNativeFiles)
$trackedNativePaths = @($trackedNativeFiles | ForEach-Object {
    [System.IO.Path]::GetRelativePath($nativeRoot, $_.FullName).Replace('\', '/')
})

$cmakeText = [System.IO.File]::ReadAllText($cmakePath)
$match = [regex]::Match(
    $cmakeText,
    'add_library\s*\(\s*OcctNative\s+SHARED(?<sources>.*?)\)',
    [System.Text.RegularExpressions.RegexOptions]::Singleline)
if (-not $match.Success) { throw "The OcctNative add_library source list is missing or malformed." }

$sourceTokens = @($match.Groups['sources'].Value -split '\s+' |
    ForEach-Object { $_.Trim() } |
    Where-Object { $_ -and -not $_.StartsWith('#') })
$duplicates = @($sourceTokens | Group-Object | Where-Object Count -gt 1)
if ($duplicates.Count -gt 0) { throw "Duplicate native source entries were found: $(($duplicates.Name) -join ', ')" }

foreach ($source in $sourceTokens) {
    if ($source -notin $trackedNativePaths) {
        throw "Native CMake source entry is not tracked by git: $source"
    }
}

$cppFiles = @($trackedNativeFiles | Where-Object { $_.Extension -eq '.cpp' } | ForEach-Object {
    [System.IO.Path]::GetRelativePath($nativeRoot, $_.FullName).Replace('\', '/')
})
$unlistedCpp = @($cppFiles | Where-Object { $_ -notin $sourceTokens })
if ($unlistedCpp.Count -gt 0) { throw "Tracked native C++ files are not listed in add_library: $($unlistedCpp -join ', ')" }

foreach ($legacyToolkit in @("TKSTEPBase", "TKSTEPAttr", "TKSTEP209", "TKSTEP", "TKIGES")) {
    if ($cmakeText.Contains($legacyToolkit)) { throw "Legacy pre-7.9 data-exchange toolkit remains: $legacyToolkit" }
}
foreach ($requiredToolkit in @("TKDESTEP", "TKDEIGES", "TKDESTL", "TKLCAF", "TKCAF", "TKXCAF")) {
    if (-not $cmakeText.Contains($requiredToolkit)) { throw "Required OCCT 7.9 data-exchange/XDE toolkit is missing: $requiredToolkit" }
}

foreach ($requiredDomain in @("core", "exchange", "geometry", "mesh", "modeling", "platform", "presentation", "scene", "selection", "topology", "viewer")) {
    $domainPrefix = "$requiredDomain/"
    if (@($trackedNativePaths | Where-Object { $_.StartsWith($domainPrefix, [System.StringComparison]::Ordinal) }).Count -eq 0) {
        throw "Tracked native domain is missing: $requiredDomain"
    }
}

$nativeFiles = @($trackedNativeFiles | Where-Object {
    $_.Extension -in @('.h', '.hpp', '.hxx', '.cpp', '.cxx')
})
$badNames = @($nativeFiles | Where-Object {
    $_.Name -match '(?i)(Extension|Extensions|Helper|Helpers|Utils|Utilities|Misc)\.(cpp|cxx|h|hpp|hxx)$'
})
if ($badNames.Count -gt 0) {
    throw "Native generic utility modules are forbidden: $((@($badNames | ForEach-Object { [System.IO.Path]::GetRelativePath($nativeRoot, $_.FullName) })) -join ', ')"
}

$headlessModelingFiles = @($nativeFiles | Where-Object {
    $relativePath = [System.IO.Path]::GetRelativePath($nativeRoot, $_.FullName).Replace('\', '/')
    $domain = $relativePath.Split('/')[0]
    $_.Extension -in @('.cpp', '.hxx') -and
        $domain -in @('modeling', 'geometry', 'topology', 'mesh', 'exchange') -and
        ($domain -eq 'modeling' -or $_.Name -like 'OcctModeling*')
})
$modelingViewerPattern = 'core/OcctInternal\.hxx|\b(?:AIS_|V3d_|Aspect_Window|ViewerContext)\b'
$modelingViewerViolations = @($headlessModelingFiles | Where-Object {
    [System.IO.File]::ReadAllText($_.FullName) -match $modelingViewerPattern
} | ForEach-Object { [System.IO.Path]::GetRelativePath($nativeRoot, $_.FullName) })
if ($modelingViewerViolations.Count -gt 0) {
    throw "Headless Modeling implementation depends on Viewer/Core state: $($modelingViewerViolations -join ', ')"
}

$platformRoot = Join-Path $nativeRoot "platform"
$platformPrefix = [System.IO.Path]::GetFullPath($platformRoot).TrimEnd(
    [System.IO.Path]::DirectorySeparatorChar,
    [System.IO.Path]::AltDirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
$platformTypePattern = '\b(?:WNT_Window|Xw_Window|HWND|XOpenDisplay|XCreateSimpleWindow)\b|<X11/'
$platformBoundaryViolations = @()
foreach ($file in $nativeFiles) {
    $fullPath = [System.IO.Path]::GetFullPath($file.FullName)
    if ($fullPath.StartsWith($platformPrefix, [System.StringComparison]::OrdinalIgnoreCase)) { continue }
    if ([System.IO.File]::ReadAllText($fullPath) -match $platformTypePattern) {
        $platformBoundaryViolations += [System.IO.Path]::GetRelativePath($nativeRoot, $fullPath)
    }
}
if ($platformBoundaryViolations.Count -gt 0) {
    throw "OS window-system types escaped the native platform adapter: $($platformBoundaryViolations -join ', ')"
}

$nativeText = ($nativeFiles | ForEach-Object { [System.IO.File]::ReadAllText($_.FullName) }) -join "`n"
if ($nativeText -match '\bSTEPCAFControl_(?:Reader|Writer)\b') {
    foreach ($requiredXdeType in @("XCAFDoc_ShapeTool", "XCAFDoc_ColorTool")) {
        if ($nativeText -notmatch "\b$requiredXdeType\b") {
            throw "Structured STEP exchange uses STEPCAF but is missing required XDE support: $requiredXdeType"
        }
    }
}

$modularHeaders = @(
    "include/occt_bridge/api.h",
    "include/occt_bridge/status.h",
    "include/occt_bridge/types.h",
    "include/occt_bridge/viewer.h",
    "include/occt_bridge/modeling.h",
    "include/occt_bridge/occt_bridge.h"
)
foreach ($relativePath in $modularHeaders) {
    if ($relativePath -notin $trackedNativePaths) {
        throw "Bridge 3 modular native header is not tracked: $relativePath"
    }
}

# Public umbrella headers must not reference retired/untracked migration files.
foreach ($relativePath in @("include/occt_bridge/viewer.h", "include/occt_bridge/modeling.h", "include/occt_bridge/occt_bridge.h")) {
    $headerPath = Join-Path $nativeRoot $relativePath
    $headerText = [System.IO.File]::ReadAllText($headerPath)
    $includes = @([regex]::Matches($headerText, '(?m)^\s*#include\s+"([^"]+)"') | ForEach-Object { $_.Groups[1].Value })
    foreach ($include in $includes) {
        $candidate = if ($include.StartsWith('occt_bridge/', [System.StringComparison]::Ordinal)) {
            "include/$include"
        }
        else {
            $include
        }
        if ($candidate -notin $trackedNativePaths) {
            throw "Public ABI5 umbrella header references an untracked or retired header: $relativePath -> $include"
        }
    }
}

foreach ($requiredCmakeToken in @("CXX_VISIBILITY_PRESET hidden", "VISIBILITY_INLINES_HIDDEN YES", "include/occt_bridge")) {
    if (-not $cmakeText.Contains($requiredCmakeToken, [System.StringComparison]::Ordinal)) {
        throw "Bridge 3 native build contract is missing: $requiredCmakeToken"
    }
}

$rootHeader = [System.IO.File]::ReadAllText((Join-Path $nativeRoot "OcctNative.h"))
foreach ($requiredApi in @(
    "occt_engine_create",
    "occt_engine_destroy",
    "occt_engine_last_error_code",
    "occt_engine_last_error_message",
    "occt_bridge_current_abi_version"
)) {
    if (-not $rootHeader.Contains($requiredApi, [System.StringComparison]::Ordinal)) {
        throw "ABI5 core contract is missing: $requiredApi"
    }
}
foreach ($retiredApi in @("occt_create(", "occt_destroy(", "occt_last_error(", "occt_bridge_abi_version(")) {
    if ($rootHeader.Contains($retiredApi, [System.StringComparison]::Ordinal)) {
        throw "Retired ABI4 core entry point returned to OcctNative.h: $retiredApi"
    }
}

$surfaceHeader = [System.IO.File]::ReadAllText((Join-Path $nativeRoot "platform/OcctNativeSurface.h"))
foreach ($requiredToken in @("structSize", "apiVersion", "occt_engine_initialize_surface", "occt_engine_surface_resize", "occt_engine_surface_redraw")) {
    if (-not $surfaceHeader.Contains($requiredToken, [System.StringComparison]::Ordinal)) {
        throw "Versioned cross-platform surface contract is missing: $requiredToken"
    }
}

Write-Host "[native-build] Tracked ABI5 CMake inventory, public headers, domain boundaries, platform isolation and OCCT 7.9 toolkits validated." -ForegroundColor Green
