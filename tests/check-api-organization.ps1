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
    "src/OcctNet/OcctEngine.Geometry.cs",
    "src/OcctNet/OcctEngine.Features.cs",
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
    "src/OcctNet/OcctModelingSession.Extensions.cs",
    "src/OcctNet/OcctModelingSession.History.cs",
    "src/OcctNet/OcctModelingExtensionTypes.cs",
    "src/OcctNet/NativeMethods.cs",
    "src/OcctNet/NativeMethods.View.cs",
    "src/OcctNet/NativeMethods.Objects.cs",
    "src/OcctNet/NativeMethods.Modeling.cs",
    "src/OcctNet/NativeMethods.Annotations.cs",
    "src/OcctNet/NativeMethods.Exchange.cs",
    "src/OcctNet/ModelNativeMethods.Analysis.cs",
    "src/OcctNet/ModelNativeMethods.Mesh.cs",
    "src/OcctNet/ModelNativeMethods.Exchange.cs",
    "src/OcctNet/ModelNativeMethods.Extensions.cs",
    "src/OcctNet/ModelNativeMethods.Interop.cs"
)
foreach ($relativePath in $requiredFiles) {
    if (-not (Test-Path (Join-Path $RepositoryRoot $relativePath) -PathType Leaf)) {
        throw "Managed API category file is missing: $relativePath"
    }
}

$forbiddenFiles = @("src/OcctNet/OcctEngine.ApiAliases.cs")
foreach ($relativePath in $forbiddenFiles) {
    if (Test-Path (Join-Path $RepositoryRoot $relativePath)) {
        throw "Compatibility alias file is not allowed in Bridge 2.6: $relativePath"
    }
}

$canonicalContracts = [ordered]@{
    "src/OcctNet/OcctEngine.View.cs" = @("public void Initialize(", "public void SetView(", "public void SetProjection(", "public OcctCameraState GetCamera(")
    "src/OcctNet/OcctEngine.Selection.cs" = @("public void Select(", "public void SelectRectangle(", "public IReadOnlyList<IOcctObject> SelectedObjects")
    "src/OcctNet/OcctEngine.Objects.cs" = @("public IOcctObject GetObject(", "public OcctShape GetShape(", "public bool IsShapeValid(", "public OcctBounds GetShapeBounds(", "public OcctDistanceResult GetShapeDistance(", "public OcctShape GetSubshapeAt(", "public OcctCurveType GetEdgeCurveType(", "public OcctSurfaceType GetFaceSurfaceType(", "public OcctUvBounds GetFaceUvBounds(")
    "src/OcctNet/OcctModelingSession.ShapeQueries.cs" = @("GetShapeOrientation", "IsShapeClosed", "IsShapeValid", "GetShapeMaximumTolerance", "GetShapeCheckReport", "GetShapeBounds", "GetShapeDistance", "GetShapeLocation", "SetShapeLocation")
    "src/OcctNet/OcctModelingSession.Topology.cs" = @("GetSubshapeAt", "GetSubshapes", "GetOuterWire", "GetInnerWires", "GetAncestors")
    "src/OcctNet/OcctModelingSession.GeometryQueries.cs" = @("EvaluateEdge(", "GetEdgeCurveType", "GetFaceSurfaceType", "GetFaceUvBounds", "EvaluateFace(")
    "src/OcctNet/OcctModelingSession.Mesh.cs" = @("public void Triangulate(", "ClearTriangulation", "GetFaceMesh", "GetShapeMesh")
    "src/OcctNet/OcctModelingSession.Extensions.cs" = @("IsSameShape", "IsPartnerShape", "GetShapeOrientedBounds", "MakePlanarFace", "TrimEdge", "OffsetWire")
    "src/OcctNet/OcctModelingSession.Exchange.cs" = @("ImportStep", "ImportIges", "ImportBrep", "ImportStl", "ExportStep", "ExportIges", "ExportBrep", "ExportStl")
    "src/OcctNet/ModelNativeMethods.Extensions.cs" = @("occt_model_shape_is_same", "occt_model_shape_is_partner", "occt_model_shape_oriented_bounds", "occt_model_make_face_with_holes", "occt_model_trim_edge", "occt_model_offset_wire")
}
foreach ($contract in $canonicalContracts.GetEnumerator()) {
    $text = [System.IO.File]::ReadAllText((Join-Path $RepositoryRoot $contract.Key))
    foreach ($token in $contract.Value) {
        if (-not $text.Contains($token)) {
            throw "Canonical managed API is missing from $($contract.Key): $token"
        }
    }
}

$sourceRoot = Join-Path $RepositoryRoot "src/OcctNet"
$sourceText = (Get-ChildItem $sourceRoot -Filter "*.cs" -File | ForEach-Object { [System.IO.File]::ReadAllText($_.FullName) }) -join "`n"

$forbiddenManagedTokens = @(
    "public OcctBounds GetBounds(", "public OcctDistanceResult Distance(", "public OcctShape GetSubshape(",
    "EvaluateEdgeNormalized", "EvaluateFaceAtParameters", "public OcctCurveType GetCurveType(",
    "public OcctSurfaceType GetSurfaceType(", "public OcctUvBounds GetUvBounds(", "public void Mesh(",
    "public void ClearMesh(", "UseParallelProcessing", "NonDestructiveMode", "RelativeDeflection",
    "UseParallelMeshing", "NativeHasUv", "NativeHasNormal", "NativeState", "public readonly record struct OcctObject :"
)
foreach ($token in $forbiddenManagedTokens) {
    if ($sourceText.Contains($token)) {
        throw "Bridge 2.6 compatibility/native-leak token remains in public wrapper: $token"
    }
}

# IsBound was an older leaked OCCT-style member name. Match it as an identifier/call instead of
# using substring search so valid APIs such as IsBoundaryCandidate are not rejected.
if ($sourceText -match '(?<![A-Za-z0-9_])IsBound\s*(?:\(|=>|\{|;)') {
    throw "Bridge 2.6 compatibility/native-leak identifier remains in public wrapper: IsBound"
}

$engineBaseText = [System.IO.File]::ReadAllText((Join-Path $RepositoryRoot "src/OcctNet/OcctEngine.cs"))
foreach ($forbidden in @("public void Initialize(", "public void SetView(", "public void Select(", "public int ObjectCount")) {
    if ($engineBaseText.Contains($forbidden)) {
        throw "OcctEngine.cs contains a categorized viewer API: $forbidden"
    }
}

$baseText = [System.IO.File]::ReadAllText((Join-Path $RepositoryRoot "src/OcctNet/OcctModelingSession.cs"))
foreach ($forbidden in @("GetShapeHash(", "GetTopologyCount(", "GetVertexPoint(", "ImportCall", "ExportCall")) {
    if ($baseText.Contains($forbidden)) {
        throw "OcctModelingSession.cs contains a categorized API/helper: $forbidden"
    }
}

$nativeBootstrapText = [System.IO.File]::ReadAllText((Join-Path $RepositoryRoot "src/OcctNet/NativeMethods.cs"))
foreach ($required in @("ResolveLibrary", "occt_create", "occt_destroy", "occt_last_error", "occt_bridge_version")) {
    if (-not $nativeBootstrapText.Contains($required)) {
        throw "NativeMethods.cs is missing bootstrap/core declaration: $required"
    }
}
foreach ($forbidden in @("occt_initialize(", "occt_select(", "occt_object_count(", "occt_make_box(", "occt_add_text(", "occt_import_step(")) {
    if ($nativeBootstrapText.Contains($forbidden)) {
        throw "NativeMethods.cs contains a categorized declaration: $forbidden"
    }
}

$nativeMethodFiles = Get-ChildItem $sourceRoot -Filter "*NativeMethods*.cs" -File
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

foreach ($fileName in @("API_COVERAGE.md", "API_COVERAGE.zh-CN.md")) {
    if (-not (Test-Path (Join-Path $RepositoryRoot "docs\$fileName") -PathType Leaf)) {
        throw "Required API coverage documentation is missing: docs/$fileName"
    }
}

$unexpectedDocs = @(Get-ChildItem (Join-Path $RepositoryRoot "docs") -File | Where-Object { $_.Extension -ne ".md" })
if ($unexpectedDocs.Count -gt 0) {
    throw "The docs directory must contain Markdown documentation only: $($unexpectedDocs.Name -join ', ')"
}

Write-Host "[organization] Bridge 2.6 canonical naming, strict ownership, typed DTO boundaries and responsibility layout validated." -ForegroundColor Green
