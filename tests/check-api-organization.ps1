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
    "src/OcctNet/OcctEngine.ObjectIdentity.cs",
    "src/OcctNet/OcctEngine.ObjectAppearance.cs",
    "src/OcctNet/OcctEngine.ObjectInteraction.cs",
    "src/OcctNet/OcctEngine.ObjectTransform.cs",
    "src/OcctNet/OcctEngine.ShapeQueries.cs",
    "src/OcctNet/OcctEngine.ShapeTransform.cs",
    "src/OcctNet/OcctEngine.Geometry.Curves.cs",
    "src/OcctNet/OcctEngine.Geometry.Planar.cs",
    "src/OcctNet/OcctEngine.Geometry.Primitives.cs",
    "src/OcctNet/OcctEngine.Geometry.Assembly.cs",
    "src/OcctNet/OcctEngine.Features.cs",
    "src/OcctNet/OcctEngine.AnnotationShapes.cs",
    "src/OcctNet/OcctEngine.Annotations.cs",
    "src/OcctNet/OcctSafeHandles.cs",
    "src/OcctNet/OcctModelingSession.cs",
    "src/OcctNet/OcctModelingSession.ShapeQueries.cs",
    "src/OcctNet/OcctModelingSession.Topology.cs",
    "src/OcctNet/OcctModelingSession.GeometryQueries.cs",
    "src/OcctNet/OcctModelingSession.AnalyticGeometry.cs",
    "src/OcctNet/OcctModelingSession.DifferentialGeometry.cs",
    "src/OcctNet/OcctModelingSession.Geometry.Curves.cs",
    "src/OcctNet/OcctModelingSession.Geometry.Planar.cs",
    "src/OcctNet/OcctModelingSession.Geometry.Primitives.cs",
    "src/OcctNet/OcctModelingSession.Geometry.Assembly.cs",
    "src/OcctNet/OcctModelingSession.Geometry.Transform.cs",
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
    "src/OcctNet/NativeMethods.Geometry.Curves.cs",
    "src/OcctNet/NativeMethods.Geometry.Planar.cs",
    "src/OcctNet/NativeMethods.Geometry.Primitives.cs",
    "src/OcctNet/NativeMethods.Geometry.Assembly.cs",
    "src/OcctNet/NativeMethods.Features.cs",
    "src/OcctNet/NativeMethods.Annotations.cs",
    "src/OcctNet/NativeMethods.Exchange.cs",
    "src/OcctNet/ModelNativeMethods.Geometry.Curves.cs",
    "src/OcctNet/ModelNativeMethods.Geometry.Planar.cs",
    "src/OcctNet/ModelNativeMethods.Geometry.Primitives.cs",
    "src/OcctNet/ModelNativeMethods.Geometry.Assembly.cs",
    "src/OcctNet/ModelNativeMethods.Geometry.Transform.cs",
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

$forbiddenFiles = @(
    "src/OcctNet/OcctEngine.ApiAliases.cs",
    "src/OcctNet/OcctEngine.Geometry.cs",
    "src/OcctNet/NativeMethods.Modeling.cs",
    "src/OcctNet/OcctModelingSession.Geometry.cs",
    "src/OcctNet/ModelNativeMethods.Geometry.cs"
)
foreach ($relativePath in $forbiddenFiles) {
    if (Test-Path (Join-Path $RepositoryRoot $relativePath)) {
        throw "Legacy/compatibility aggregate file is not allowed in Bridge 2.6: $relativePath"
    }
}

$canonicalContracts = [ordered]@{
    "src/OcctNet/OcctEngine.View.cs" = @("public void Initialize(", "public void SetView(", "public void SetProjection(", "public void SetViewCubeLanguage(", "public OcctCameraState GetCamera(")
    "src/OcctNet/OcctEngine.Selection.cs" = @("public void Select(", "public void SelectRectangle(", "public IReadOnlyList<IOcctObject> SelectedObjects")
    "src/OcctNet/OcctEngine.Objects.cs" = @("public int ObjectCount", "public IReadOnlyList<IOcctObject> Objects", "public IOcctObject GetObject(", "public OcctShape GetShape(", "public void Delete(", "public void Clear(")
    "src/OcctNet/OcctEngine.ObjectIdentity.cs" = @("public string GetName(", "public void SetName(", "public void SetApplicationTag(", "public string GetApplicationTag(", "public bool TryGetObjectByApplicationTag(")
    "src/OcctNet/OcctEngine.ObjectAppearance.cs" = @("public void SetColor(", "public void SetTransparency(", "public void SetVisible(", "public void SetDisplayMode(IOcctObject", "public void SetMaterial(", "public void Redisplay(")
    "src/OcctNet/OcctEngine.ObjectInteraction.cs" = @("public void SetSelectable(", "public bool IsSelectable(", "public void Highlight(", "public void Unhighlight(")
    "src/OcctNet/OcctEngine.ShapeQueries.cs" = @("public bool IsShapeValid(", "public OcctBounds GetShapeBounds(", "public OcctDistanceResult GetShapeDistance(", "public OcctShape GetSubshapeAt(", "public OcctCurveType GetEdgeCurveType(", "public OcctSurfaceType GetFaceSurfaceType(", "public OcctUvBounds GetFaceUvBounds(")
    "src/OcctNet/OcctEngine.ShapeTransform.cs" = @("public OcctShape Copy(", "public OcctShape Translate(", "public OcctShape Rotate(", "public OcctShape Scale(", "public OcctShape MirrorPlane(")
    "src/OcctNet/OcctEngine.Geometry.Curves.cs" = @("public OcctShape MakeVertex(", "public OcctShape MakeLine(", "public OcctShape MakePolyline(", "public OcctShape MakeCircle(", "public OcctShape MakeArc(", "public OcctShape MakeEllipse(", "public OcctShape MakeBezier(", "public OcctShape MakeInterpolatedBSpline(")
    "src/OcctNet/OcctEngine.Geometry.Planar.cs" = @("public OcctShape MakeRegularPolygon(", "public OcctShape MakeRectangleWire(", "public OcctShape MakeFace(", "public OcctShape MakePlaneFace(")
    "src/OcctNet/OcctEngine.Geometry.Primitives.cs" = @("public OcctShape MakeBox(", "public OcctShape MakeCylinder(", "public OcctShape MakeSphere(", "public OcctShape MakeCone(", "public OcctShape MakeTorus(", "public OcctShape MakeWedge(")
    "src/OcctNet/OcctEngine.Geometry.Assembly.cs" = @("public OcctShape MakeCompound(", "public OcctShape MakeWire(", "public OcctShape Sew(", "public OcctShape MakeSolidFromShell(", "private long[] ShapeIds(")
    "src/OcctNet/OcctEngine.Features.cs" = @("public OcctShape Boolean(", "public OcctShape Extrude(", "public OcctShape FilletEdges(", "public OcctShape MakeThickSolid(", "public OcctShape DrillHole(")
    "src/OcctNet/OcctEngine.AnnotationShapes.cs" = @("public OcctShape MakeTextShape(", "public OcctShape MakeLengthAnnotationShape(", "public OcctShape MakeAngleAnnotationShape(", "public OcctShape MakeRadiusAnnotationShape(", "public OcctShape MakeDiameterAnnotationShape(")
    "src/OcctNet/OcctEngine.Annotations.cs" = @("public OcctText AddText(", "public void SetText(", "public void SetDimensionFlyout(", "public OcctDimension AddLengthDimension(", "public OcctDimension AddAngleDimension(", "public OcctDimension AddRadiusDimension(", "public OcctDimension AddDiameterDimension(")
    "src/OcctNet/NativeMethods.Geometry.Curves.cs" = @("occt_make_vertex", "occt_make_line", "occt_make_circle", "occt_make_arc_center", "occt_make_bspline_interpolated")
    "src/OcctNet/NativeMethods.Geometry.Planar.cs" = @("occt_make_regular_polygon", "occt_make_rectangle_wire", "occt_make_face_from_wire", "occt_make_plane_face")
    "src/OcctNet/NativeMethods.Geometry.Primitives.cs" = @("occt_make_box", "occt_make_cylinder", "occt_make_sphere", "occt_make_cone", "occt_make_torus", "occt_make_wedge")
    "src/OcctNet/NativeMethods.Geometry.Assembly.cs" = @("occt_make_compound", "occt_make_wire", "occt_sew_shapes", "occt_make_solid_from_shell")
    "src/OcctNet/NativeMethods.Features.cs" = @("occt_boolean", "occt_extrude", "occt_revolve", "occt_sweep", "occt_loft", "occt_fillet_edges", "occt_chamfer_edges")
    "src/OcctNet/OcctModelingSession.Geometry.Curves.cs" = @("public OcctModelShape MakeVertex(", "public OcctModelShape MakeLine(", "public OcctModelShape MakePolyline(", "public OcctModelShape MakeCircle(", "public OcctModelShape MakeArc(", "public OcctModelShape MakeEllipse(", "public OcctModelShape MakeBezier(", "public OcctModelShape MakeInterpolatedBSpline(")
    "src/OcctNet/OcctModelingSession.Geometry.Planar.cs" = @("public OcctModelShape MakeRegularPolygon(", "public OcctModelShape MakeRectangleWire(", "public OcctModelShape MakePlaneFace(", "public OcctModelShape MakeFace(")
    "src/OcctNet/OcctModelingSession.Geometry.Primitives.cs" = @("public OcctModelShape MakeBox(", "public OcctModelShape MakeCylinder(", "public OcctModelShape MakeCone(", "public OcctModelShape MakeSphere(", "public OcctModelShape MakeTorus(", "public OcctModelShape MakeWedge(")
    "src/OcctNet/OcctModelingSession.Geometry.Assembly.cs" = @("public OcctModelShape MakeCompound(", "public OcctModelShape MakeWire(", "public OcctModelShape Sew(", "public OcctModelShape MakeSolidFromShell(")
    "src/OcctNet/OcctModelingSession.Geometry.Transform.cs" = @("public OcctModelShape Translate(", "public OcctModelShape Rotate(", "public OcctModelShape Scale(", "public OcctModelShape MirrorPlane(")
    "src/OcctNet/ModelNativeMethods.Geometry.Curves.cs" = @("occt_model_make_vertex", "occt_model_make_line", "occt_model_make_circle", "occt_model_make_arc_center", "occt_model_make_bspline_interpolated")
    "src/OcctNet/ModelNativeMethods.Geometry.Planar.cs" = @("occt_model_make_regular_polygon", "occt_model_make_rectangle_wire", "occt_model_make_plane_face", "occt_model_make_face_from_wire")
    "src/OcctNet/ModelNativeMethods.Geometry.Primitives.cs" = @("occt_model_make_box", "occt_model_make_cylinder", "occt_model_make_cone", "occt_model_make_sphere", "occt_model_make_torus", "occt_model_make_wedge")
    "src/OcctNet/ModelNativeMethods.Geometry.Assembly.cs" = @("occt_model_make_compound", "occt_model_make_wire", "occt_model_sew", "occt_model_make_solid_from_shell")
    "src/OcctNet/ModelNativeMethods.Geometry.Transform.cs" = @("occt_model_translate", "occt_model_rotate", "occt_model_scale", "occt_model_mirror_plane")
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

function Assert-NoResponsibilityTokens {
    param(
        [string]$RelativePath,
        [string[]]$Tokens,
        [string]$Responsibility
    )

    $text = [System.IO.File]::ReadAllText((Join-Path $RepositoryRoot $RelativePath))
    foreach ($token in $Tokens) {
        if ($text.Contains($token)) {
            throw "$RelativePath contains $Responsibility responsibility: $token"
        }
    }
}

Assert-NoResponsibilityTokens "src/OcctNet/OcctEngine.Objects.cs" @("GetName(", "SetColor(", "Highlight(", "IsShapeValid(", "GetShapeBounds(", "GetSubshapeAt(", "public OcctShape Copy(") "non-registry"
Assert-NoResponsibilityTokens "src/OcctNet/OcctEngine.ObjectInteraction.cs" @("SetViewCubeLanguage(") "view"
Assert-NoResponsibilityTokens "src/OcctNet/OcctEngine.ShapeQueries.cs" @("public OcctShape Copy(", "public void SetColor(", "public void Delete(") "mutation/object"
Assert-NoResponsibilityTokens "src/OcctNet/OcctEngine.ShapeTransform.cs" @("IsShapeValid(", "GetShapeBounds(", "public void SetLocalTransformation(") "query/view-local transform"

Assert-NoResponsibilityTokens "src/OcctNet/OcctEngine.Geometry.Curves.cs" @("public OcctShape MakeBox(", "public OcctShape MakeFace(", "public OcctShape MakeCompound(", "public OcctShape Boolean(") "primitive/planar/assembly/feature"
Assert-NoResponsibilityTokens "src/OcctNet/OcctEngine.Geometry.Planar.cs" @("public OcctShape MakeBezier(", "public OcctShape MakeCylinder(", "public OcctShape MakeCompound(", "public OcctShape Boolean(") "curve/primitive/assembly/feature"
Assert-NoResponsibilityTokens "src/OcctNet/OcctEngine.Geometry.Primitives.cs" @("public OcctShape MakeArc(", "public OcctShape MakeFace(", "public OcctShape MakeWire(", "public OcctShape Boolean(") "curve/planar/assembly/feature"
Assert-NoResponsibilityTokens "src/OcctNet/OcctEngine.Geometry.Assembly.cs" @("public OcctShape MakeCircle(", "public OcctShape MakePlaneFace(", "public OcctShape MakeSphere(", "public OcctShape Boolean(") "curve/planar/primitive/feature"
Assert-NoResponsibilityTokens "src/OcctNet/NativeMethods.Geometry.Curves.cs" @("occt_make_box", "occt_make_face_from_wire", "occt_make_compound", "occt_boolean") "primitive/planar/assembly/feature PInvoke"
Assert-NoResponsibilityTokens "src/OcctNet/NativeMethods.Geometry.Planar.cs" @("occt_make_bezier", "occt_make_cylinder", "occt_make_compound", "occt_boolean") "curve/primitive/assembly/feature PInvoke"
Assert-NoResponsibilityTokens "src/OcctNet/NativeMethods.Geometry.Primitives.cs" @("occt_make_arc_center", "occt_make_face_from_wire", "occt_make_wire", "occt_boolean") "curve/planar/assembly/feature PInvoke"
Assert-NoResponsibilityTokens "src/OcctNet/NativeMethods.Geometry.Assembly.cs" @("occt_make_circle", "occt_make_plane_face", "occt_make_sphere", "occt_boolean") "curve/planar/primitive/feature PInvoke"
Assert-NoResponsibilityTokens "src/OcctNet/NativeMethods.Features.cs" @("occt_make_line", "occt_make_plane_face", "occt_make_box", "occt_make_compound") "geometry PInvoke"

Assert-NoResponsibilityTokens "src/OcctNet/OcctModelingSession.Geometry.Curves.cs" @("public OcctModelShape MakeBox(", "public OcctModelShape MakeFace(", "public OcctModelShape MakeCompound(", "public OcctModelShape Translate(") "primitive/planar/assembly/transform"
Assert-NoResponsibilityTokens "src/OcctNet/OcctModelingSession.Geometry.Planar.cs" @("public OcctModelShape MakeBezier(", "public OcctModelShape MakeCylinder(", "public OcctModelShape MakeCompound(", "public OcctModelShape Translate(") "curve/primitive/assembly/transform"
Assert-NoResponsibilityTokens "src/OcctNet/OcctModelingSession.Geometry.Primitives.cs" @("public OcctModelShape MakeArc(", "public OcctModelShape MakeFace(", "public OcctModelShape MakeWire(", "public OcctModelShape Translate(") "curve/planar/assembly/transform"
Assert-NoResponsibilityTokens "src/OcctNet/OcctModelingSession.Geometry.Assembly.cs" @("public OcctModelShape MakeCircle(", "public OcctModelShape MakePlaneFace(", "public OcctModelShape MakeSphere(", "public OcctModelShape Translate(") "curve/planar/primitive/transform"
Assert-NoResponsibilityTokens "src/OcctNet/OcctModelingSession.Geometry.Transform.cs" @("public OcctModelShape MakeLine(", "public OcctModelShape MakePlaneFace(", "public OcctModelShape MakeBox(", "public OcctModelShape MakeCompound(") "geometry construction"
Assert-NoResponsibilityTokens "src/OcctNet/ModelNativeMethods.Geometry.Curves.cs" @("occt_model_make_box", "occt_model_make_face_from_wire", "occt_model_make_compound", "occt_model_translate") "primitive/planar/assembly/transform PInvoke"
Assert-NoResponsibilityTokens "src/OcctNet/ModelNativeMethods.Geometry.Planar.cs" @("occt_model_make_bezier", "occt_model_make_cylinder", "occt_model_make_compound", "occt_model_translate") "curve/primitive/assembly/transform PInvoke"
Assert-NoResponsibilityTokens "src/OcctNet/ModelNativeMethods.Geometry.Primitives.cs" @("occt_model_make_arc_center", "occt_model_make_face_from_wire", "occt_model_make_wire", "occt_model_translate") "curve/planar/assembly/transform PInvoke"
Assert-NoResponsibilityTokens "src/OcctNet/ModelNativeMethods.Geometry.Assembly.cs" @("occt_model_make_circle", "occt_model_make_plane_face", "occt_model_make_sphere", "occt_model_translate") "curve/planar/primitive/transform PInvoke"
Assert-NoResponsibilityTokens "src/OcctNet/ModelNativeMethods.Geometry.Transform.cs" @("occt_model_make_line", "occt_model_make_plane_face", "occt_model_make_box", "occt_model_make_compound") "geometry construction PInvoke"

Assert-NoResponsibilityTokens "src/OcctNet/OcctEngine.Features.cs" @("MakeTextShape(", "MakeLengthAnnotationShape(", "AddText(", "SetText(", "AddLengthDimension(") "annotation"
Assert-NoResponsibilityTokens "src/OcctNet/OcctEngine.AnnotationShapes.cs" @("public OcctText AddText(", "public OcctDimension AddLengthDimension(") "interactive annotation"
Assert-NoResponsibilityTokens "src/OcctNet/OcctEngine.Annotations.cs" @("public OcctShape Boolean(", "public OcctShape MakeTextShape(") "modeling/BRep annotation"

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
