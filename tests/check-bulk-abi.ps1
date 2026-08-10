param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$sourceRoots = @(
    Join-Path $RepositoryRoot "src\OcctNative",
    Join-Path $RepositoryRoot "src\OcctNet"
)
foreach ($root in $sourceRoots) {
    if (-not (Test-Path $root -PathType Container)) {
        throw "Bulk ABI source root was not found: $root"
    }
}

$sourceText = ($sourceRoots | ForEach-Object {
    Get-ChildItem $_ -File -Recurse | Where-Object {
        $_.Extension -in @('.h', '.hpp', '.hxx', '.cpp', '.cs')
    } | ForEach-Object {
        [System.IO.File]::ReadAllText($_.FullName)
    }
}) -join "`n"

$requiredBulkApis = @(
    "occt_model_shape_ids_copy",
    "occt_model_subshapes_copy",
    "occt_model_inner_wires_copy",
    "occt_model_ancestors_copy",
    "occt_model_ray_hits_copy",
    "occt_model_face_mesh_nodes_copy",
    "occt_model_face_mesh_triangles_copy",
    "occt_model_history_generated_copy",
    "occt_model_history_modified_copy",
    "occt_selected_hits"
)
foreach ($api in $requiredBulkApis) {
    if (-not $sourceText.Contains($api)) {
        throw "Required bulk ABI is missing: $api"
    }
}

$forbiddenIndexedApis = @(
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
    "occt_model_history_modified_at",
    "occt_selected_hit_count",
    "occt_selected_hit_at"
)
foreach ($api in $forbiddenIndexedApis) {
    if ($sourceText.Contains($api)) {
        throw "Indexed/N+1 collection ABI must not be reintroduced: $api"
    }
}

Write-Host "[bulk-abi] Modeling high-cardinality collections and selected hits use bulk transfer; retired indexed ABI is absent." -ForegroundColor Green
