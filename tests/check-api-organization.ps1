param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$requiredFiles = @(
    "src/OcctNet/OcctEngine.cs",
    "src/OcctNet/OcctEngine.View.cs",
    "src/OcctNet/OcctEngine.Selection.cs",
    "src/OcctNet/OcctEngine.Objects.cs",
    "src/OcctNet/OcctSafeHandles.cs",
    "src/OcctNet/OcctModelingSession.cs",
    "src/OcctNet/OcctModelingSession.ShapeQueries.cs",
    "src/OcctNet/OcctModelingSession.Topology.cs",
    "src/OcctNet/OcctModelingSession.GeometryQueries.cs",
    "src/OcctNet/OcctModelingSession.AnalyticGeometry.cs",
    "src/OcctNet/OcctModelingSession.DifferentialGeometry.cs",
    "src/OcctNet/OcctModelingSession.Geometry.cs",
    "src/OcctNet/OcctModelingSession.Algorithms.cs",
    "src/OcctNet/OcctModelingSession.Analysis.cs",
    "src/OcctNet/OcctModelingSession.Mesh.cs",
    "src/OcctNet/OcctModelingSession.Exchange.cs",
    "src/OcctNet/OcctModelingSession.History.cs",
    "src/OcctNet/NativeMethods.cs",
    "src/OcctNet/NativeMethods.View.cs",
    "src/OcctNet/NativeMethods.Objects.cs",
    "src/OcctNet/NativeMethods.Modeling.cs",
    "src/OcctNet/NativeMethods.Annotations.cs",
    "src/OcctNet/NativeMethods.Exchange.cs",
    "src/OcctNet/ModelNativeMethods.Analysis.cs",
    "src/OcctNet/ModelNativeMethods.Mesh.cs",
    "src/OcctNet/ModelNativeMethods.Exchange.cs",
    "src/OcctNet/ModelNativeMethods.Interop.cs"
)
foreach ($relativePath in $requiredFiles) {
    if (-not (Test-Path (Join-Path $RepositoryRoot $relativePath) -PathType Leaf)) {
        throw "Managed API category file is missing: $relativePath"
    }
}

$engineBaseText = [System.IO.File]::ReadAllText((Join-Path $RepositoryRoot "src/OcctNet/OcctEngine.cs"))
foreach ($forbidden in @(
    "public void Initialize(",
    "public void SetView(",
    "public void Select(",
    "public int ObjectCount",
    "public IReadOnlyList<OcctShape> Shapes",
    "public OcctBounds GetBounds("
)) {
    if ($engineBaseText.Contains($forbidden)) {
        throw "OcctEngine.cs contains a categorized viewer API: $forbidden"
    }
}

$baseText = [System.IO.File]::ReadAllText((Join-Path $RepositoryRoot "src/OcctNet/OcctModelingSession.cs"))
foreach ($forbidden in @(
    "GetShapeHash(",
    "GetTopologyCount(",
    "GetVertexPoint(",
    "GetEdgeCurveType(",
    "ImportCall",
    "ExportCall",
    "ValidateExchangePath"
)) {
    if ($baseText.Contains($forbidden)) {
        throw "OcctModelingSession.cs contains a categorized API/helper: $forbidden"
    }
}

$canonicalContracts = [ordered]@{
    "src/OcctNet/OcctEngine.View.cs" = @(
        "public void Initialize(",
        "public void SetView(",
        "public void SetProjection(",
        "public OcctCameraState GetCamera("
    )
    "src/OcctNet/OcctEngine.Selection.cs" = @(
        "public void Select(",
        "public void SelectRectangle(",
        "public IReadOnlyList<OcctObject> SelectedObjects"
    )
    "src/OcctNet/OcctEngine.Objects.cs" = @(
        "public int ObjectCount",
        "public IReadOnlyList<OcctShape> Shapes",
        "public bool Owns(",
        "public OcctBounds GetBounds("
    )
    "src/OcctNet/OcctModelingSession.ShapeQueries.cs" = @(
        "GetShapeOrientation",
        "GetShapeBounds",
        "GetShapeDistance",
        "GetShapeLocation",
        "SetShapeLocation"
    )
    "src/OcctNet/OcctModelingSession.Topology.cs" = @("GetSubshapeAt")
    "src/OcctNet/OcctModelingSession.GeometryQueries.cs" = @(
        "EvaluateEdgeNormalized",
        "GetEdgeCurveType",
        "GetFaceSurfaceType",
        "GetFaceUvBounds",
        "EvaluateFaceAtParameters"
    )
    "src/OcctNet/OcctModelingSession.Analysis.cs" = @(
        "ProjectPointOnEdge",
        "ProjectPointOnFace",
        "IntersectRay",
        "ClassifyPoint"
    )
    "src/OcctNet/OcctModelingSession.Mesh.cs" = @(
        "public void Mesh(",
        "ClearMesh",
        "GetFaceMesh"
    )
    "src/OcctNet/OcctModelingSession.Exchange.cs" = @(
        "ImportStep",
        "ImportIges",
        "ImportBrep",
        "ImportStl",
        "ExportStep",
        "ExportIges",
        "ExportBrep",
        "ExportStl"
    )
}
foreach ($contract in $canonicalContracts.GetEnumerator()) {
    $text = [System.IO.File]::ReadAllText((Join-Path $RepositoryRoot $contract.Key))
    foreach ($token in $contract.Value) {
        if (-not $text.Contains($token)) {
            throw "Canonical managed API is missing from $($contract.Key): $token"
        }
    }
}

$analysisText = [System.IO.File]::ReadAllText((Join-Path $RepositoryRoot "src/OcctNet/OcctModelingSession.Analysis.cs"))
foreach ($forbidden in @("public void Mesh(", "GetFaceMesh", "ImportStep", "ExportStep", "ImportStl", "ExportStl")) {
    if ($analysisText.Contains($forbidden)) {
        throw "Analysis API file contains mesh/exchange responsibility: $forbidden"
    }
}

$nativeAnalysisText = [System.IO.File]::ReadAllText((Join-Path $RepositoryRoot "src/OcctNet/ModelNativeMethods.Analysis.cs"))
foreach ($forbidden in @("occt_model_mesh", "occt_model_import_", "occt_model_export_", "occt_model_display_in_engine")) {
    if ($nativeAnalysisText.Contains($forbidden)) {
        throw "Native analysis declaration file contains another responsibility: $forbidden"
    }
}

$nativeBootstrapText = [System.IO.File]::ReadAllText((Join-Path $RepositoryRoot "src/OcctNet/NativeMethods.cs"))
foreach ($required in @("ResolveLibrary", "occt_create", "occt_destroy", "occt_last_error", "occt_bridge_version")) {
    if (-not $nativeBootstrapText.Contains($required)) {
        throw "NativeMethods.cs is missing bootstrap/core declaration: $required"
    }
}
foreach ($forbidden in @(
    "occt_initialize(",
    "occt_select(",
    "occt_object_count(",
    "occt_make_box(",
    "occt_add_text(",
    "occt_import_step("
)) {
    if ($nativeBootstrapText.Contains($forbidden)) {
        throw "NativeMethods.cs contains a categorized declaration: $forbidden"
    }
}

$nativeCategoryContracts = [ordered]@{
    "src/OcctNet/NativeMethods.View.cs" = @("occt_initialize", "occt_select", "occt_get_camera")
    "src/OcctNet/NativeMethods.Objects.cs" = @("occt_object_count", "occt_shape_bounds", "occt_translate")
    "src/OcctNet/NativeMethods.Modeling.cs" = @("occt_make_box", "occt_boolean", "occt_fillet_edges")
    "src/OcctNet/NativeMethods.Annotations.cs" = @("occt_add_text", "occt_add_length_dimension")
    "src/OcctNet/NativeMethods.Exchange.cs" = @("occt_import_step", "occt_export_step")
}
foreach ($contract in $nativeCategoryContracts.GetEnumerator()) {
    $text = [System.IO.File]::ReadAllText((Join-Path $RepositoryRoot $contract.Key))
    foreach ($token in $contract.Value) {
        if (-not $text.Contains($token)) {
            throw "Native declaration category is missing from $($contract.Key): $token"
        }
    }
}

$nativeMethodFiles = Get-ChildItem (Join-Path $RepositoryRoot "src/OcctNet") -Filter "*NativeMethods*.cs" -File
foreach ($file in $nativeMethodFiles) {
    $text = [System.IO.File]::ReadAllText($file.FullName)
    $attributes = [regex]::Matches($text, '\[DllImport\(LibraryName(?<body>.*?)\)\]', 'Singleline')
    foreach ($attribute in $attributes) {
        $body = $attribute.Groups['body'].Value
        if ($body -notmatch 'CallingConvention\s*=\s*CallingConvention\.Cdecl') {
            throw "Bridge P/Invoke does not declare Cdecl: $($file.Name)"
        }
        if ($body -notmatch 'ExactSpelling\s*=\s*true') {
            throw "Bridge P/Invoke does not use exact symbol spelling: $($file.Name)"
        }
    }
}

$docs = @(Get-ChildItem (Join-Path $RepositoryRoot "docs") -File | Select-Object -ExpandProperty Name | Sort-Object)
$expectedDocs = @("API_COVERAGE.md", "API_COVERAGE.zh-CN.md")
if (Compare-Object $expectedDocs $docs) {
    throw "The docs directory must contain only API_COVERAGE.md and API_COVERAGE.zh-CN.md."
}

Write-Host "[organization] Viewer/modeling categories, native responsibility boundaries, P/Invoke attributes and documentation layout validated." -ForegroundColor Green
