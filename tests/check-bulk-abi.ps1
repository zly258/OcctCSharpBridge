param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$nativeRoot = Join-Path $RepositoryRoot "src\OcctNative"
$managedRoot = Join-Path $RepositoryRoot "src\OcctNet"
foreach ($root in @($nativeRoot, $managedRoot)) {
    if (-not (Test-Path $root -PathType Container)) { throw "Bulk ABI source root was not found: $root" }
}

$nativeText = (Get-ChildItem $nativeRoot -File -Recurse | Where-Object {
    $_.Extension -in @('.h', '.hpp', '.hxx', '.cpp')
} | ForEach-Object { [System.IO.File]::ReadAllText($_.FullName) }) -join "`n"

$managedText = (Get-ChildItem $managedRoot -Filter '*.cs' -File -Recurse |
    ForEach-Object { [System.IO.File]::ReadAllText($_.FullName) }) -join "`n"
$sourceText = $nativeText + "`n" + $managedText

# Viewer high-cardinality state must use buffer/capacity APIs rather than N+1 accessors.
foreach ($api in @(
    "occt_engine_objects_snapshot_get",
    "occt_engine_selection_hits_get",
    "occt_engine_selection_detect_at"
)) {
    if (-not $sourceText.Contains($api)) { throw "Required ABI5 bulk API is missing: $api" }
}

# Modeling still owns several high-cardinality collections. During ABI5 migration their
# public spelling may evolve, but indexed count/at pairs are never allowed back in.
$bulkModelPatterns = @(
    'occt_model_shape_ids_copy',
    'occt_model_subshapes_copy',
    'occt_model_inner_wires_copy',
    'occt_model_ancestors_copy',
    'occt_model_ray_hits_copy',
    'occt_model_face_mesh_nodes_copy',
    'occt_model_face_mesh_triangles_copy',
    'occt_model_history_generated_copy',
    'occt_model_history_modified_copy'
)
foreach ($api in $bulkModelPatterns) {
    if (-not $sourceText.Contains($api)) { throw "Modeling bulk transfer API is missing: $api" }
}

$forbiddenIndexedApis = @(
    "occt_object_id_at",
    "occt_shape_id_at",
    "occt_shape_count",
    "occt_selected_count",
    "occt_selected_at",
    "occt_selected_hits",
    "occt_selected_hit_count",
    "occt_selected_hit_at",
    "occt_model_shape_count",
    "occt_model_shape_id_at",
    "occt_model_topology_count",
    "occt_model_get_subshape",
    "occt_model_inner_wire_count",
    "occt_model_inner_wire_at",
    "occt_model_ancestor_count",
    "occt_model_ancestor_at",
    "occt_model_ray_hit_count",
    "occt_model_ray_hit_at",
    "occt_model_face_mesh_counts",
    "occt_model_face_mesh_node(",
    "occt_model_face_mesh_triangle(",
    "occt_model_history_generated_count",
    "occt_model_history_generated_at",
    "occt_model_history_modified_count",
    "occt_model_history_modified_at"
)
foreach ($api in $forbiddenIndexedApis) {
    if ($sourceText.Contains($api)) { throw "Indexed/N+1 collection ABI must not be reintroduced: $api" }
}

Write-Host "[bulk-abi] ABI5 Viewer and Modeling high-cardinality collections use bulk transfer semantics." -ForegroundColor Green
