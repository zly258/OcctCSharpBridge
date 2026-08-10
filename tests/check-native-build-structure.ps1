param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$nativeRoot = Join-Path $RepositoryRoot "src\OcctNative"
$cmakePath = Join-Path $nativeRoot "CMakeLists.txt"
if (-not (Test-Path $cmakePath -PathType Leaf)) { throw "Native CMakeLists.txt was not found: $cmakePath" }

$cmakeText = [System.IO.File]::ReadAllText($cmakePath)
$match = [regex]::Match($cmakeText, 'add_library\s*\(\s*OcctNative\s+SHARED(?<sources>.*?)\)', [System.Text.RegularExpressions.RegexOptions]::Singleline)
if (-not $match.Success) { throw "The OcctNative add_library source list is missing or not closed." }

$sourceTokens = @($match.Groups['sources'].Value -split '\s+' | ForEach-Object { $_.Trim() } | Where-Object { $_ })
$duplicates = @($sourceTokens | Group-Object | Where-Object Count -gt 1)
if ($duplicates.Count -gt 0) { throw "Duplicate native source entries were found: $(($duplicates.Name) -join ', ')" }
foreach ($source in $sourceTokens) {
    if (-not (Test-Path (Join-Path $nativeRoot $source) -PathType Leaf)) { throw "Native source entry does not exist: $source" }
}

function Assert-Module {
    param([string]$File, [string[]]$Symbols, [switch]$Narrow)
    if ($File -notin $sourceTokens) { throw "Native module is not listed in add_library: $File" }
    $path = Join-Path $nativeRoot $File
    $text = [System.IO.File]::ReadAllText($path)
    if ($Narrow -and $text.Contains('#include "OcctModelingInternal.hxx"')) { throw "$File depends on the retired OcctModelingInternal.hxx umbrella." }
    foreach ($symbol in $Symbols) {
        if (-not $text.Contains($symbol)) { throw "$File is missing expected responsibility symbol: $symbol" }
    }
}

foreach ($module in @(
    @{ File = "OcctEngine.cpp"; Symbols = @("occt_create", "occt_initialize", "occt_begin_update") },
    @{ File = "OcctEngineView.cpp"; Symbols = @("occt_fit_all", "occt_set_view", "occt_screen_to_world") },
    @{ File = "OcctEngineSelection.cpp"; Symbols = @("occt_select", "occt_select_rectangle_ex", "occt_clear_selection") },
    @{ File = "OcctEngineObjects.cpp"; Symbols = @("occt_object_count", "occt_delete_objects", "occt_clear") },
    @{ File = "OcctEngineShapes.cpp"; Symbols = @("occt_shape_bounds", "occt_get_subshape", "occt_translate") }
)) { Assert-Module -File $module.File -Symbols $module.Symbols }
if ((Get-Item (Join-Path $nativeRoot "OcctEngine.cpp")).Length -gt 26000) { throw "OcctEngine.cpp has grown beyond its lifecycle/shared-helper boundary." }

foreach ($module in @(
    @{ File = "OcctModelingCore.cpp"; Symbols = @("occt_model_create", "occt_model_shape_ids_copy", "occt_model_operation_report", "occt_model_copy_shape") },
    @{ File = "OcctModelingShapeQueries.cpp"; Symbols = @("occt_model_shape_hash", "occt_model_shape_bounds", "occt_model_shape_distance") },
    @{ File = "OcctModelingGeometryQueries.cpp"; Symbols = @("occt_model_vertex_point", "occt_model_edge_point_at", "occt_model_face_point_normal") },
    @{ File = "OcctModelingTopology.cpp"; Symbols = @("occt_model_subshapes_copy", "occt_model_outer_wire", "occt_model_ancestors_copy") },
    @{ File = "OcctModelingInterop.cpp"; Symbols = @("occt_model_display_in_engine") },
    @{ File = "OcctModelingBoolean.cpp"; Symbols = @("occt_model_boolean", "occt_model_split") },
    @{ File = "OcctModelingFeatures.cpp"; Symbols = @("occt_model_extrude", "occt_model_revolve", "occt_model_thick_solid") },
    @{ File = "OcctModelingHealing.cpp"; Symbols = @("occt_model_unify_same_domain", "occt_model_fix_shape") },
    @{ File = "OcctModelingHistory.cpp"; Symbols = @("occt_model_history_generated_copy", "occt_model_history_modified_copy", "occt_model_history_is_removed") },
    @{ File = "OcctModelingAnalysis.cpp"; Symbols = @("occt_model_project_point_on_edge", "occt_model_ray_intersections", "occt_model_ray_hits_copy", "occt_model_classify_point") },
    @{ File = "OcctModelingMesh.cpp"; Symbols = @("occt_model_mesh", "occt_model_face_mesh_nodes_copy", "occt_model_face_mesh_triangles_copy") },
    @{ File = "OcctModelingExchange.cpp"; Symbols = @("occt_model_import_step", "occt_model_import_file", "occt_model_export_step", "occt_model_export_stl") },
    @{ File = "OcctModelingAnalyticGeometry.cpp"; Symbols = @("occt_model_edge_line_geometry", "occt_model_face_cylinder_geometry") },
    @{ File = "OcctModelingDifferentialGeometry.cpp"; Symbols = @("occt_model_edge_differential", "occt_model_face_curvature") },
    @{ File = "OcctModelingExtensions.cpp"; Symbols = @("occt_model_shape_is_same", "occt_model_trim_edge", "occt_model_offset_wire") },
    @{ File = "OcctModelingBSpline.cpp"; Symbols = @("occt_model_edge_bspline_info", "occt_model_face_bspline_info") },
    @{ File = "OcctModelingTopologyAnalysis.cpp"; Symbols = @("occt_model_shape_free_bounds", "occt_model_shape_edge_adjacency") },
    @{ File = "OcctModelingFaceAnalysis.cpp"; Symbols = @("occt_model_shape_face_analysis") },
    @{ File = "OcctModelingInertia.cpp"; Symbols = @("occt_model_shape_linear_inertia", "occt_model_shape_surface_inertia", "occt_model_shape_volume_inertia") },
    @{ File = "OcctModelingIntersection.cpp"; Symbols = @("occt_model_intersect_edges", "occt_model_edge_intersections_copy") },
    @{ File = "OcctModelingTopologyReference.cpp"; Symbols = @("occt_model_create_topology_reference", "occt_model_resolve_topology_reference", "occt_model_resolve_topology_reference_with_history") }
)) { Assert-Module -File $module.File -Symbols $module.Symbols -Narrow }

if ((Get-Item (Join-Path $nativeRoot "OcctModelingCore.cpp")).Length -gt 9000) { throw "OcctModelingCore.cpp has grown beyond the session/registry boundary." }
if ((Get-Item (Join-Path $nativeRoot "OcctModelingAnalysis.cpp")).Length -gt 12000) { throw "OcctModelingAnalysis.cpp must remain limited to projection/ray/classification." }
if ((Get-Item (Join-Path $nativeRoot "OcctModelingTopologyReference.cpp")).Length -gt 22000) { throw "OcctModelingTopologyReference.cpp has grown beyond its fingerprint/resolution boundary." }
if (Test-Path (Join-Path $nativeRoot "OcctModelingAlgorithms.cpp")) { throw "Legacy mixed-responsibility OcctModelingAlgorithms.cpp must remain removed." }

$modelingInternalHeaders = @(
    @{ File = "OcctModelingSessionInternal.hxx"; Symbols = @("struct ModelSession", "modelOf", "executeShape", "requireOperation", "edgeIntersections") },
    @{ File = "OcctModelingShapeInternal.hxx"; Symbols = @("toDirection", "toShapeEnum", "indexedEdge", "maximumTolerance", "shapeList") },
    @{ File = "OcctModelingAlgorithmInternal.hxx"; Symbols = @("failedAlgorithmResult", "applyBooleanOptions", "finishBuilderAlgorithm", "historyCopy") },
    @{ File = "OcctModelingMeshInternal.hxx"; Symbols = @("faceTriangulation") },
    @{ File = "OcctModelingExchangeInternal.hxx"; Symbols = @("modelInputStream", "readModelStep", "readModelIges", "writeModelStep") }
)
foreach ($header in $modelingInternalHeaders) {
    Assert-Module -File $header.File -Symbols $header.Symbols
    $headerText = [System.IO.File]::ReadAllText((Join-Path $nativeRoot $header.File))
    if ($headerText.Contains('#include "OcctModelingInternal.hxx"')) { throw "$($header.File) depends on the retired modeling umbrella." }
}
if ([System.IO.File]::ReadAllText((Join-Path $nativeRoot "OcctModelingSessionInternal.hxx")).Contains('#include "OcctInternal.hxx"')) {
    throw "OcctModelingSessionInternal.hxx must remain headless and independent from viewer internals."
}
if (Test-Path (Join-Path $nativeRoot "OcctModelingInternal.hxx")) { throw "Retired OcctModelingInternal.hxx must not be reintroduced." }
if ($cmakeText.Contains("OcctModelingInternal.hxx")) { throw "CMake must not list the retired modeling umbrella." }

foreach ($header in @(
    @{ File = "OcctModelingInertia.h"; Symbols = @("OcctModelInertiaProperties", "occt_model_shape_volume_inertia") },
    @{ File = "OcctModelingIntersection.h"; Symbols = @("OcctModelEdgeIntersection", "occt_model_intersect_edges") },
    @{ File = "OcctModelingTopologyReference.h"; Symbols = @("OcctModelTopologyReference", "OcctModelTopologyReferenceResult") }
)) { Assert-Module -File $header.File -Symbols $header.Symbols }

foreach ($module in @(
    @{ File = "OcctModelingGeometry.Curves.cpp"; Symbols = @("occt_model_make_vertex", "occt_model_make_line", "occt_model_make_bspline_interpolated") },
    @{ File = "OcctModelingGeometry.Planar.cpp"; Symbols = @("occt_model_make_regular_polygon", "occt_model_make_rectangle_wire", "occt_model_make_plane_face") },
    @{ File = "OcctModelingGeometry.Primitives.cpp"; Symbols = @("occt_model_make_box", "occt_model_make_cylinder", "occt_model_make_torus") },
    @{ File = "OcctModelingGeometry.Assembly.cpp"; Symbols = @("occt_model_make_compound", "occt_model_make_wire", "occt_model_make_solid_from_shell") },
    @{ File = "OcctModelingGeometry.Transform.cpp"; Symbols = @("occt_model_translate", "occt_model_rotate", "occt_model_scale", "occt_model_mirror_plane") }
)) { Assert-Module -File $module.File -Symbols $module.Symbols -Narrow }
if (Test-Path (Join-Path $nativeRoot "OcctModelingGeometry.cpp")) { throw "Legacy OcctModelingGeometry.cpp must remain removed." }

foreach ($legacyToolkit in @("TKSTEPBase", "TKSTEPAttr", "TKSTEP209", "TKSTEP", "TKIGES")) {
    if ($cmakeText.Contains($legacyToolkit)) { throw "Legacy pre-7.9 data-exchange toolkit remains: $legacyToolkit" }
}
foreach ($requiredToolkit in @("TKDESTEP", "TKDEIGES", "TKDESTL")) {
    if (-not $cmakeText.Contains($requiredToolkit)) { throw "Required OCCT 7.9 toolkit is missing: $requiredToolkit" }
}

$buildScriptText = [System.IO.File]::ReadAllText((Join-Path $RepositoryRoot "build.ps1"))
if (-not $buildScriptText.Contains('D:\tools\occt-vc144-64')) { throw "build.ps1 must provide the conventional OCCT default root." }
if (-not ($buildScriptText.Contains('validate/managed/pack/ci do not require OCCT') -or $buildScriptText.Contains('validate/managed/ci do not require OCCT'))) {
    throw "build.ps1 must preserve OCCT-optional managed/validation targets."
}

if ($cmakeText -match 'OcctOcaf|occt_ocaf_|\b(?:TKCDF|TKLCAF|TKCAF|TKXCAF|TKBinL|TKXmlL|TKBinXCAF|TKXmlXCAF)\b') {
    throw "OCAF/XDE input remains in the reusable native build."
}

$unlistedCpp = @(Get-ChildItem $nativeRoot -Filter '*.cpp' -File | Where-Object { $_.Name -notin $sourceTokens } | Select-Object -ExpandProperty Name)
if ($unlistedCpp.Count -gt 0) { throw "Native C++ files are not listed in add_library: $($unlistedCpp -join ', ')" }

Write-Host "[native-build] P0 inertia, P1 structured intersection, P2 topology references, P3 bulk ABI, narrow internals and OCCT 7.9 toolkits validated." -ForegroundColor Green
