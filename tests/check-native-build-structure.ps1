param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$nativeRoot = Join-Path $RepositoryRoot "src\OcctNative"
$cmakePath = Join-Path $nativeRoot "CMakeLists.txt"
if (-not (Test-Path $cmakePath -PathType Leaf)) { throw "Native CMakeLists.txt was not found: $cmakePath" }

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
    if (-not (Test-Path (Join-Path $nativeRoot $source) -PathType Leaf)) {
        throw "Native CMake source entry does not exist: $source"
    }
}

$cppFiles = @(Get-ChildItem $nativeRoot -Filter '*.cpp' -File -Recurse | ForEach-Object {
    [System.IO.Path]::GetRelativePath($nativeRoot, $_.FullName).Replace('\', '/')
})
$unlistedCpp = @($cppFiles | Where-Object { $_ -notin $sourceTokens })
if ($unlistedCpp.Count -gt 0) { throw "Native C++ files are not listed in add_library: $($unlistedCpp -join ', ')" }

foreach ($legacyToolkit in @("TKSTEPBase", "TKSTEPAttr", "TKSTEP209", "TKSTEP", "TKIGES")) {
    if ($cmakeText.Contains($legacyToolkit)) { throw "Legacy pre-7.9 data-exchange toolkit remains: $legacyToolkit" }
}
foreach ($requiredToolkit in @("TKDESTEP", "TKDEIGES", "TKDESTL", "TKLCAF", "TKCAF", "TKXCAF")) {
    if (-not $cmakeText.Contains($requiredToolkit)) { throw "Required OCCT 7.9 data-exchange/XDE toolkit is missing: $requiredToolkit" }
}

foreach ($requiredDomain in @("core", "exchange", "geometry", "mesh", "modeling", "platform", "presentation", "scene", "selection", "topology", "viewer")) {
    if (-not (Test-Path (Join-Path $nativeRoot $requiredDomain) -PathType Container)) {
        throw "Native domain directory is missing: $requiredDomain"
    }
}

$nativeFiles = @(Get-ChildItem $nativeRoot -File -Recurse | Where-Object {
    $_.Extension -in @('.h', '.hpp', '.hxx', '.cpp')
})
$badNames = @($nativeFiles | Where-Object {
    $_.Name -match '(?i)(Extension|Extensions|Helper|Helpers|Utils|Utilities|Misc)\.(cpp|cxx|h|hpp|hxx)$'
})
if ($badNames.Count -gt 0) {
    throw "Native generic utility modules are forbidden: $((@($badNames | ForEach-Object { [System.IO.Path]::GetRelativePath($nativeRoot, $_.FullName) })) -join ', ')"
}

$headlessModelingFiles = @(
    foreach ($domain in @("modeling", "geometry", "topology", "mesh", "exchange")) {
        $domainRoot = Join-Path $nativeRoot $domain
        Get-ChildItem $domainRoot -File -Recurse | Where-Object {
            $_.Extension -in @(".cpp", ".hxx") -and ($domain -eq "modeling" -or $_.Name -like "OcctModeling*")
        }
    }
)
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

foreach ($relativePath in @(
    "include/occt_bridge/api.h",
    "include/occt_bridge/status.h",
    "include/occt_bridge/types.h",
    "include/occt_bridge/viewer.h",
    "include/occt_bridge/modeling.h",
    "include/occt_bridge/occt_bridge.h"
)) {
    if (-not (Test-Path (Join-Path $nativeRoot $relativePath) -PathType Leaf)) {
        throw "Bridge 3 modular native header is missing: $relativePath"
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

Write-Host "[native-build] ABI5 CMake inventory, domain boundaries, platform isolation and OCCT 7.9 toolkits validated." -ForegroundColor Green
