param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Text {
    param([string]$RelativePath)
    $path = Join-Path $RepositoryRoot $RelativePath
    if (-not (Test-Path $path -PathType Leaf)) { throw "Required API file is missing: $RelativePath" }
    return [System.IO.File]::ReadAllText($path)
}

$requiredFiles = @(
    "src/OcctNet/OcctEngine.cs",
    "src/OcctNet/OcctEngine.View.cs",
    "src/OcctNet/OcctEngine.Selection.cs",
    "src/OcctNet/OcctEngine.SelectionHits.cs",
    "src/OcctNet/OcctEngine.Objects.cs",
    "src/OcctNet/OcctEngine.ShapeQueries.cs",
    "src/OcctNet/OcctModelingSession.cs",
    "src/OcctNet/OcctModelingSession.ShapeQueries.cs",
    "src/OcctNet/OcctModelingSession.Topology.cs",
    "src/OcctNet/OcctModelingSession.Analysis.cs",
    "src/OcctNet/OcctModelingSession.Mesh.cs",
    "src/OcctNet/OcctModelingSession.History.cs",
    "src/OcctNet/OcctModelingSession.Inertia.cs",
    "src/OcctNet/OcctModelingSession.Intersection.cs",
    "src/OcctNet/OcctModelingSession.TopologyReference.cs",
    "src/OcctNet/OcctInertiaProperties.cs",
    "src/OcctNet/OcctIntersectionTypes.cs",
    "src/OcctNet/OcctTopologyReferenceTypes.cs",
    "src/OcctNet/ModelNativeMethods.Core.cs",
    "src/OcctNet/ModelNativeMethods.Topology.cs",
    "src/OcctNet/ModelNativeMethods.Analysis.cs",
    "src/OcctNet/ModelNativeMethods.Mesh.cs",
    "src/OcctNet/ModelNativeMethods.History.cs",
    "src/OcctNet/ModelNativeMethods.Inertia.cs",
    "src/OcctNet/ModelNativeMethods.Intersection.cs",
    "src/OcctNet/ModelNativeMethods.TopologyReference.cs",
    "src/OcctNet.WinForms/OcctViewportControl.cs",
    "src/OcctNet.Wpf/OcctWpfViewport.cs",
    "src/OcctNet.Avalonia/OcctAvaloniaViewport.cs"
)
foreach ($file in $requiredFiles) { [void](Read-Text $file) }

$forbiddenFiles = @(
    "src/OcctNet/OcctObject.Legacy.cs",
    "src/OcctNet/OcctGeometryExtensions.Compatibility.cs",
    "src/OcctNet/OcctEngine.ApiAliases.cs",
    "src/OcctNet/OcctEngine.Geometry.cs",
    "src/OcctNet/NativeMethods.Modeling.cs",
    "src/OcctNet/OcctModelingSession.Geometry.cs",
    "src/OcctNet/ModelNativeMethods.Geometry.cs",
    "src/OcctNet/OcctModelingSession.Algorithms.cs",
    "src/OcctNet/ModelNativeMethods.Algorithms.cs",
    "tests/OcctNet.ManagedTests/PublicApi.approved.txt",
    "tests/OcctNet.ManagedTests/PublicApiSnapshot.cs",
    "tests/OcctNet.ManagedTests/PublicApiSnapshotInitializer.cs"
)
foreach ($file in $forbiddenFiles) {
    if (Test-Path (Join-Path $RepositoryRoot $file)) { throw "Legacy/compatibility file is not allowed in the new library: $file" }
}

$contracts = [ordered]@{
    "src/OcctNet/OcctEngine.Selection.cs" = @("IReadOnlyList<IOcctObject> SelectedObjects", "IOcctObject? FirstSelectedObject")
    "src/OcctNet/OcctEngine.SelectionHits.cs" = @("GetSelectedHits", "TryGetDetectedHit")
    "src/OcctNet/OcctModelingSession.cs" = @("occt_model_shape_ids_copy", "public IReadOnlyList<OcctModelShape> Shapes")
    "src/OcctNet/OcctModelingSession.Topology.cs" = @("occt_model_subshapes_copy", "occt_model_inner_wires_copy", "occt_model_ancestors_copy")
    "src/OcctNet/OcctModelingSession.Mesh.cs" = @("occt_model_face_mesh_nodes_copy", "occt_model_face_mesh_triangles_copy")
    "src/OcctNet/OcctModelingSession.History.cs" = @("occt_model_history_generated_copy", "occt_model_history_modified_copy")
    "src/OcctNet/OcctModelingSession.Inertia.cs" = @("GetLinearInertiaProperties", "GetSurfaceInertiaProperties", "GetVolumeInertiaProperties")
    "src/OcctNet/OcctModelingSession.Intersection.cs" = @("IntersectEdges", "OcctEdgeIntersection")
    "src/OcctNet/OcctModelingSession.TopologyReference.cs" = @("CreateTopologyReference", "ResolveTopologyReference")
}
foreach ($contract in $contracts.GetEnumerator()) {
    $text = Read-Text $contract.Key
    foreach ($token in $contract.Value) {
        if (-not $text.Contains($token)) { throw "$($contract.Key) is missing canonical API token: $token" }
    }
}

$managedText = (Get-ChildItem (Join-Path $RepositoryRoot "src/OcctNet") -Filter '*.cs' -File |
    ForEach-Object { [System.IO.File]::ReadAllText($_.FullName) }) -join "`n"
foreach ($token in @(
    "public readonly record struct OcctObject",
    "SelectedObjectsOwned",
    "FirstSelectedObjectOwned",
    "Compatibility"
)) {
    if ($managedText.Contains($token)) { throw "Compatibility token remains in the public bridge source: $token" }
}

if ($managedText -match '\bDocumentManager\b|\bCommandBus\b|\bToolManager\b|\bOcafDocument\b') {
    throw "Application-layer CAD framework types must not enter the reusable bridge."
}

Write-Host "[organization] Clean owner-aware API, P0-P3 modules, UI hosts and no-compatibility boundary validated." -ForegroundColor Green
