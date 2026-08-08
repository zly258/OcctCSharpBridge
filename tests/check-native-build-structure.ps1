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

$text = [System.IO.File]::ReadAllText($cmakePath)
$match = [regex]::Match(
    $text,
    'add_library\s*\(\s*OcctNative\s+SHARED(?<sources>.*?)\)',
    [System.Text.RegularExpressions.RegexOptions]::Singleline)
if (-not $match.Success) {
    throw "The OcctNative add_library source list is missing or not closed."
}

$sourceTokens = @(
    $match.Groups['sources'].Value -split '\s+' |
        ForEach-Object { $_.Trim() } |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) -and -not $_.StartsWith('#') }
)
if ($sourceTokens.Count -eq 0) {
    throw "The OcctNative source list is empty."
}

$duplicates = @($sourceTokens | Group-Object | Where-Object Count -gt 1)
if ($duplicates.Count -gt 0) {
    $names = ($duplicates | Select-Object -ExpandProperty Name) -join ', '
    throw "Duplicate native source entries were found: $names"
}

foreach ($source in $sourceTokens) {
    $path = Join-Path $nativeRoot $source
    if (-not (Test-Path $path -PathType Leaf)) {
        throw "Native source entry does not exist: $source"
    }
}

$modules = @(
    @{
        Name = "Extensions"
        Files = @("OcctModelingExtensions.cpp", "OcctModelingExtensions.h")
        Header = "OcctModelingExtensions.h"
        Symbols = @("occt_model_shape_is_same", "occt_model_shape_oriented_bounds", "occt_model_make_face_with_holes", "occt_model_trim_edge", "occt_model_offset_wire")
    },
    @{
        Name = "B-Spline"
        Files = @("OcctModelingBSpline.cpp", "OcctModelingBSpline.h")
        Header = "OcctModelingBSpline.h"
        Symbols = @("occt_model_edge_bspline_info", "occt_model_face_bspline_info", "occt_model_face_bspline_pole_at")
    },
    @{
        Name = "Topology analysis"
        Files = @("OcctModelingTopologyAnalysis.cpp", "OcctModelingTopologyAnalysis.h")
        Header = "OcctModelingTopologyAnalysis.h"
        Symbols = @("occt_model_shape_free_bounds", "occt_model_shape_edge_adjacency")
    },
    @{
        Name = "Face analysis"
        Files = @("OcctModelingFaceAnalysis.cpp", "OcctModelingFaceAnalysis.h")
        Header = "OcctModelingFaceAnalysis.h"
        Symbols = @("OcctModelFaceAnalysis", "occt_model_shape_face_analysis")
    }
)

foreach ($module in $modules) {
    foreach ($required in $module.Files) {
        if ($required -notin $sourceTokens) {
            throw "$($module.Name) native module file is not listed in add_library: $required"
        }
    }

    $header = [System.IO.File]::ReadAllText((Join-Path $nativeRoot $module.Header))
    foreach ($symbol in $module.Symbols) {
        if (-not $header.Contains($symbol)) {
            throw "$($module.Name) native declaration is missing: $symbol"
        }
    }
}

$forbiddenPatterns = [ordered]@{
    'OCAF/XDE source' = 'OcctOcaf|occt_ocaf_'
    'OCAF/XDE toolkit' = '\b(?:TKCDF|TKLCAF|TKCAF|TKXCAF|TKBinL|TKXmlL|TKBinXCAF|TKXmlXCAF)\b'
}
foreach ($item in $forbiddenPatterns.GetEnumerator()) {
    if ($text -match $item.Value) {
        throw "$($item.Key) remains in the reusable native build."
    }
}

$unlistedCpp = @(
    Get-ChildItem $nativeRoot -Filter '*.cpp' -File |
        Where-Object { $_.Name -notin $sourceTokens } |
        Select-Object -ExpandProperty Name
)
if ($unlistedCpp.Count -gt 0) {
    throw "Native C++ files are not listed in add_library: $($unlistedCpp -join ', ')"
}

Write-Host "[native-build] $($sourceTokens.Count) source entries and $($modules.Count) dedicated modules validated; no OCAF/XDE inputs remain." -ForegroundColor Green
