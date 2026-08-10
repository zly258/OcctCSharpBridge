param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-RepositoryText {
    param([Parameter(Mandatory = $true)][string]$RelativePath)
    $path = Join-Path $RepositoryRoot $RelativePath
    if (-not (Test-Path $path -PathType Leaf)) {
        throw "Bulk ABI contract file was not found: $RelativePath"
    }
    return [System.IO.File]::ReadAllText($path)
}

function Assert-Contains {
    param([string]$Text, [string[]]$Tokens, [string]$Area)
    foreach ($token in $Tokens) {
        if (-not $Text.Contains($token)) { throw "$Area contract is missing: $token" }
    }
}

function Assert-DoesNotContain {
    param([string]$Text, [string[]]$Tokens, [string]$Area)
    foreach ($token in $Tokens) {
        if ($Text.Contains($token)) { throw "$Area still contains obsolete indexed ABI: $token" }
    }
}

$analysisSession = Read-RepositoryText "src\OcctNet\OcctModelingSession.Analysis.cs"
Assert-Contains $analysisSession @(
    "occt_model_ray_hits_copy",
    "new NativeModelRayHit[count]",
    "Native ray-hit count changed during bulk copy"
) "Managed ray-hit bulk transfer"
Assert-DoesNotContain $analysisSession @("occt_model_ray_hit_at(_handle", "occt_model_ray_hit_count(_handle") "Managed ray-hit collection"

$analysisNativeMethods = Read-RepositoryText "src\OcctNet\ModelNativeMethods.Analysis.cs"
Assert-Contains $analysisNativeMethods @("occt_model_ray_hits_copy", "[Out] NativeModelRayHit[]") "Ray-hit P/Invoke"
Assert-DoesNotContain $analysisNativeMethods @("occt_model_ray_hit_at", "occt_model_ray_hit_count") "Ray-hit P/Invoke"

$analysisNative = Read-RepositoryText "src\OcctNative\OcctModelingAnalysis.cpp"
Assert-Contains $analysisNative @("occt_model_ray_hits_copy", "model->rayHits") "Ray-hit native"
Assert-DoesNotContain $analysisNative @("occt_model_ray_hit_at", "occt_model_ray_hit_count") "Ray-hit native"

$historySession = Read-RepositoryText "src\OcctNet\OcctModelingSession.History.cs"
Assert-Contains $historySession @(
    "GetHistoryShapes",
    "occt_model_history_generated_copy",
    "occt_model_history_modified_copy",
    "Native topology-history count changed during bulk copy"
) "Managed topology-history bulk transfer"
Assert-DoesNotContain $historySession @("occt_model_history_generated_at", "occt_model_history_modified_at", "occt_model_history_generated_count", "occt_model_history_modified_count") "Managed topology-history collection"

$historyNativeMethods = Read-RepositoryText "src\OcctNet\ModelNativeMethods.History.cs"
Assert-Contains $historyNativeMethods @("occt_model_history_generated_copy", "occt_model_history_modified_copy", "[Out] long[]") "Topology-history P/Invoke"
Assert-DoesNotContain $historyNativeMethods @("occt_model_history_generated_at", "occt_model_history_modified_at", "occt_model_history_generated_count", "occt_model_history_modified_count") "Topology-history P/Invoke"

$historyNative = Read-RepositoryText "src\OcctNative\OcctModelingHistory.cpp"
Assert-Contains $historyNative @("occt_model_history_generated_copy", "occt_model_history_modified_copy", "historyCopy") "Topology-history native"
Assert-DoesNotContain $historyNative @("occt_model_history_generated_at", "occt_model_history_modified_at", "occt_model_history_generated_count", "occt_model_history_modified_count") "Topology-history native"

$shapeSession = Read-RepositoryText "src\OcctNet\OcctModelingSession.cs"
Assert-Contains $shapeSession @("occt_model_shape_ids_copy", "Native shape count changed during bulk copy") "Managed shape enumeration"
Assert-DoesNotContain $shapeSession @("occt_model_shape_id_at", "occt_model_shape_count") "Managed shape enumeration"

$topologySession = Read-RepositoryText "src\OcctNet\OcctModelingSession.Topology.cs"
Assert-Contains $topologySession @("occt_model_subshapes_copy", "occt_model_inner_wires_copy", "occt_model_ancestors_copy", "ReadShapeCollection") "Managed topology collection"
Assert-DoesNotContain $topologySession @("occt_model_get_subshape", "occt_model_inner_wire_at", "occt_model_ancestor_at") "Managed topology collection"

$meshSession = Read-RepositoryText "src\OcctNet\OcctModelingSession.Mesh.cs"
Assert-Contains $meshSession @("occt_model_face_mesh_nodes_copy", "occt_model_face_mesh_triangles_copy") "Managed mesh collection"
Assert-DoesNotContain $meshSession @("occt_model_face_mesh_node(", "occt_model_face_mesh_triangle(", "occt_model_face_mesh_counts(") "Managed mesh collection"

$nativeHeader = Read-RepositoryText "src\OcctNative\OcctModeling.h"
Assert-Contains $nativeHeader @(
    "occt_model_shape_ids_copy",
    "occt_model_subshapes_copy",
    "occt_model_inner_wires_copy",
    "occt_model_ancestors_copy",
    "occt_model_ray_hits_copy",
    "occt_model_face_mesh_nodes_copy",
    "occt_model_face_mesh_triangles_copy",
    "occt_model_history_generated_copy",
    "occt_model_history_modified_copy"
) "Modeling bulk ABI"
Assert-DoesNotContain $nativeHeader @(
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
) "Modeling bulk ABI"

Write-Host "[modeling-bulk-abi] Shape, topology, ray-hit, mesh, and history collections are bulk-only." -ForegroundColor Green
