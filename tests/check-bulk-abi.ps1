param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Get-TrackedSourceText {
    param(
        [Parameter(Mandatory = $true)][string]$RelativeRoot,
        [Parameter(Mandatory = $true)][string[]]$Extensions
    )

    $normalizedRoot = $RelativeRoot.Replace('\', '/').TrimEnd('/')
    $tracked = @(& git -C $RepositoryRoot ls-files -- "$normalizedRoot/**" 2>$null)
    if ($LASTEXITCODE -ne 0) {
        throw "Unable to inspect tracked repository files with git ls-files: $RelativeRoot"
    }

    $parts = @()
    foreach ($relativePath in $tracked) {
        if ([System.IO.Path]::GetExtension($relativePath) -notin $Extensions) { continue }
        $fullPath = Join-Path $RepositoryRoot $relativePath
        if (-not (Test-Path $fullPath -PathType Leaf)) {
            throw "Tracked source file is missing from the working tree: $relativePath"
        }
        $parts += [System.IO.File]::ReadAllText($fullPath)
    }
    return $parts -join "`n"
}

$nativeText = Get-TrackedSourceText "src/OcctNative" @('.h', '.hpp', '.hxx', '.cpp')
$managedText = Get-TrackedSourceText "src/OcctNet" @('.cs')
$sourceText = $nativeText + "`n" + $managedText

# High-cardinality ABI5 state must use one-shot snapshots/buffers rather than indexed N+1 accessors.
$requiredBulkApis = @(
    "occt_engine_objects_snapshot_get",
    "occt_engine_selection_hits_get",
    "occt_model_shapes_snapshot_get",
    "occt_model_subshapes_snapshot_get",
    "occt_model_inner_wires_snapshot_get",
    "occt_model_ancestors_snapshot_get",
    "occt_model_ray_hits_snapshot_get",
    "occt_model_project_points_on_edge",
    "occt_model_project_points_on_face",
    "occt_model_shape_distances",
    "occt_model_face_mesh_nodes_snapshot_get",
    "occt_model_face_mesh_triangles_snapshot_get",
    "occt_model_history_generated_snapshot_get",
    "occt_model_history_modified_snapshot_get",
    "occt_model_edge_intersections_snapshot_get",
    "occt_model_shape_edge_adjacency_snapshot_get",
    "occt_model_shape_face_analysis_snapshot_get"
)
foreach ($api in $requiredBulkApis) {
    if (-not $sourceText.Contains($api)) {
        throw "Required ABI5 snapshot/buffer API is missing: $api"
    }
}

# ABI4/transition-era bulk spellings are retired. Their presence means compatibility code leaked back in.
$retiredBulkApis = @(
    "occt_model_shape_ids_copy",
    "occt_model_subshapes_copy",
    "occt_model_inner_wires_copy",
    "occt_model_ancestors_copy",
    "occt_model_ray_hits_copy",
    "occt_model_face_mesh_nodes_copy",
    "occt_model_face_mesh_triangles_copy",
    "occt_model_history_generated_copy",
    "occt_model_history_modified_copy",
    "occt_model_edge_intersections_copy",
    "occt_model_shape_edge_adjacency(",
    "occt_model_shape_face_analysis("
)
foreach ($api in $retiredBulkApis) {
    if ($sourceText.Contains($api)) {
        throw "Retired pre-ABI5 bulk API must not be reintroduced: $api"
    }
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
    if ($sourceText.Contains($api)) {
        throw "Indexed/N+1 collection ABI must not be reintroduced: $api"
    }
}

Write-Host "[bulk-abi] ABI5 Viewer and Modeling collections use tracked snapshot/buffer APIs only." -ForegroundColor Green
