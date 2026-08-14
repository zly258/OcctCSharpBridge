param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$nativeRoot = Join-Path $RepositoryRoot "src\OcctNative"
$cmakePath = Join-Path $nativeRoot "CMakeLists.txt"
if (-not (Test-Path $cmakePath -PathType Leaf)) {
    throw "Native CMakeLists.txt was not found: $cmakePath"
}

$cmakeText = [System.IO.File]::ReadAllText($cmakePath)
$match = [regex]::Match(
    $cmakeText,
    'add_library\s*\(\s*OcctNative\s+SHARED(?<sources>.*?)\)',
    [System.Text.RegularExpressions.RegexOptions]::Singleline)
if (-not $match.Success) {
    throw "The OcctNative add_library source list is missing or malformed."
}

$sourceTokens = @($match.Groups['sources'].Value -split '\s+' |
    ForEach-Object { $_.Trim() } |
    Where-Object { $_ -and -not $_.StartsWith('#') })

$duplicates = @($sourceTokens | Group-Object | Where-Object Count -gt 1)
if ($duplicates.Count -gt 0) {
    throw "Duplicate native source entries were found: $(($duplicates.Name) -join ', ')"
}

foreach ($source in $sourceTokens) {
    $sourcePath = Join-Path $nativeRoot $source
    if (-not (Test-Path $sourcePath -PathType Leaf)) {
        throw "Native CMake source entry does not exist: $source"
    }
}

$cppFiles = @(Get-ChildItem $nativeRoot -Filter '*.cpp' -File -Recurse | ForEach-Object {
    [System.IO.Path]::GetRelativePath($nativeRoot, $_.FullName).Replace('\', '/')
})
$unlistedCpp = @($cppFiles | Where-Object { $_ -notin $sourceTokens })
if ($unlistedCpp.Count -gt 0) {
    throw "Native C++ files are not listed in add_library: $($unlistedCpp -join ', ')"
}

foreach ($legacyToolkit in @("TKSTEPBase", "TKSTEPAttr", "TKSTEP209", "TKSTEP", "TKIGES")) {
    if ($cmakeText.Contains($legacyToolkit)) {
        throw "Legacy pre-7.9 data-exchange toolkit remains: $legacyToolkit"
    }
}

foreach ($requiredToolkit in @("TKDESTEP", "TKDEIGES", "TKDESTL", "TKLCAF", "TKCAF", "TKXCAF")) {
    if (-not $cmakeText.Contains($requiredToolkit)) {
        throw "Required OCCT 7.9 data-exchange/XDE toolkit is missing: $requiredToolkit"
    }
}

$nativeFiles = @(Get-ChildItem $nativeRoot -File -Recurse | Where-Object {
    $_.Extension -in @('.h', '.hpp', '.hxx', '.cpp')
})
foreach ($requiredDomain in @("exchange", "mesh", "platform", "presentation", "scene", "selection", "viewer")) {
    $domainRoot = Join-Path $nativeRoot $requiredDomain
    if (-not (Test-Path $domainRoot -PathType Container)) {
        throw "Native domain directory is missing: $requiredDomain"
    }
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
    if (-not (Test-Path (Join-Path $nativeRoot $relativePath) -PathType Leaf)) {
        throw "Bridge 3 modular native header is missing: $relativePath"
    }
}

foreach ($requiredCmakeToken in @("CXX_VISIBILITY_PRESET hidden", "VISIBILITY_INLINES_HIDDEN YES", "include/occt_bridge")) {
    if (-not $cmakeText.Contains($requiredCmakeToken, [System.StringComparison]::Ordinal)) {
        throw "Bridge 3 native build contract is missing: $requiredCmakeToken"
    }
}

$viewerHeader = [System.IO.File]::ReadAllText((Join-Path $nativeRoot "OcctNative.h"))
$modelingHeader = [System.IO.File]::ReadAllText((Join-Path $nativeRoot "OcctModeling.h"))
$surfaceHeader = [System.IO.File]::ReadAllText((Join-Path $nativeRoot "platform/OcctNativeSurface.h"))
foreach ($requiredApi in @("occt_create", "occt_engine_create", "occt_engine_last_error_code")) {
    if (-not $viewerHeader.Contains($requiredApi, [System.StringComparison]::Ordinal)) {
        throw "Viewer compatibility/typed-handle contract is missing: $requiredApi"
    }
}
foreach ($requiredApi in @("occt_model_create", "occt_model_session_create", "occt_model_history_summary")) {
    if (-not $modelingHeader.Contains($requiredApi, [System.StringComparison]::Ordinal)) {
        throw "Modeling compatibility/history contract is missing: $requiredApi"
    }
}
foreach ($requiredToken in @("structSize", "apiVersion", "occt_engine_initialize_surface")) {
    if (-not $surfaceHeader.Contains($requiredToken, [System.StringComparison]::Ordinal)) {
        throw "Versioned cross-platform surface contract is missing: $requiredToken"
    }
}


Write-Host "[native-build] CMake source inventory and OCCT 7.9 exchange/XDE toolkits validated." -ForegroundColor Green
