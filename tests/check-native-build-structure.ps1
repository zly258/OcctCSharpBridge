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

$engineModules = @(
    @{ File = "OcctEngine.cpp"; Symbols = @("occt_create", "occt_initialize", "occt_begin_update") },
    @{ File = "OcctEngineView.cpp"; Symbols = @("occt_fit_all", "occt_set_view", "occt_screen_to_world") },
    @{ File = "OcctEngineSelection.cpp"; Symbols = @("occt_select", "occt_select_rectangle_ex", "occt_selected_at") },
    @{ File = "OcctEngineObjects.cpp"; Symbols = @("occt_object_count", "occt_delete_objects", "occt_clear") },
    @{ File = "OcctEngineShapes.cpp"; Symbols = @("occt_shape_bounds", "occt_get_subshape", "occt_translate") }
)

foreach ($module in $engineModules) {
    if ($module.File -notin $sourceTokens) {
        throw "Split native engine module is not listed in add_library: $($module.File)"
    }
    $moduleText = [System.IO.File]::ReadAllText((Join-Path $nativeRoot $module.File))
    foreach ($symbol in $module.Symbols) {
        if (-not $moduleText.Contains($symbol)) {
            throw "Split native engine module $($module.File) is missing expected responsibility symbol: $symbol"
        }
    }
}

$engineCorePath = Join-Path $nativeRoot "OcctEngine.cpp"
if ((Get-Item $engineCorePath).Length -gt 26000) {
    throw "OcctEngine.cpp has grown beyond the lifecycle/shared-helper boundary; keep view, selection, object, and shape responsibilities split."
}

$modelingCoreModules = @(
    @{ File = "OcctModelingCore.cpp"; Symbols = @("occt_model_create", "occt_model_shape_count", "occt_model_operation_report", "occt_model_copy_shape") },
    @{ File = "OcctModelingShapeQueries.cpp"; Symbols = @("occt_model_shape_hash", "occt_model_shape_bounds", "occt_model_shape_distance", "occt_model_check_report") },
    @{ File = "OcctModelingGeometryQueries.cpp"; Symbols = @("occt_model_vertex_point", "occt_model_edge_point_at", "occt_model_face_point_normal") },
    @{ File = "OcctModelingTopology.cpp"; Symbols = @("occt_model_topology_count", "occt_model_outer_wire", "occt_model_ancestor_at", "occt_model_sew") },
    @{ File = "OcctModelingInterop.cpp"; Symbols = @("occt_model_display_in_engine") }
)

foreach ($module in $modelingCoreModules) {
    if ($module.File -notin $sourceTokens) {
        throw "Split native modeling core module is not listed in add_library: $($module.File)"
    }
    $moduleText = [System.IO.File]::ReadAllText((Join-Path $nativeRoot $module.File))
    foreach ($symbol in $module.Symbols) {
        if (-not $moduleText.Contains($symbol)) {
            throw "Split native modeling core module $($module.File) is missing expected responsibility symbol: $symbol"
        }
    }
}

$modelingCorePath = Join-Path $nativeRoot "OcctModelingCore.cpp"
if ((Get-Item $modelingCorePath).Length -gt 9000) {
    throw "OcctModelingCore.cpp has grown beyond the session/registry boundary; keep shape queries, topology, geometry evaluation, and viewer interop split."
}
$modelingCoreText = [System.IO.File]::ReadAllText($modelingCorePath)
foreach ($forbiddenSymbol in @(
    "occt_model_shape_hash",
    "occt_model_shape_bounds",
    "occt_model_topology_count",
    "occt_model_vertex_point",
    "occt_model_display_in_engine"
)) {
    if ($modelingCoreText.Contains($forbiddenSymbol)) {
        throw "OcctModelingCore.cpp contains responsibility that belongs in a split module: $forbiddenSymbol"
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

Write-Host "[native-build] $($sourceTokens.Count) source entries, $($engineModules.Count) split engine modules, $($modelingCoreModules.Count) split modeling core modules, and $($modules.Count) dedicated modeling modules validated; no OCAF/XDE inputs remain." -ForegroundColor Green
