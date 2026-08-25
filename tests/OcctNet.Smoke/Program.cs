using OcctNet;

if (OcctBridgeInfo.NativeAbiVersion != OcctBridgeInfo.ExpectedAbiVersion)
    throw new InvalidOperationException("Native bridge ABI validation failed.");
if (!string.Equals(OcctBridgeInfo.NativeVersion, OcctBridgeInfo.ManagedVersion, StringComparison.Ordinal))
    throw new InvalidOperationException(
        $"Managed/native bridge version mismatch: {OcctBridgeInfo.ManagedVersion} / {OcctBridgeInfo.NativeVersion}.");
if (string.IsNullOrWhiteSpace(OcctBridgeInfo.BuildInfo))
    throw new InvalidOperationException("Native bridge build information is empty.");

ConcurrencyStress.Run();

using var model = new OcctModelingSession();

var snapshotSource = model.MakeBox(3, 4, 5);
using var snapshot = model.AcquireShape(snapshotSource);
model.Delete(snapshotSource);
if (snapshot.ShapeType != OcctShapeType.Solid)
{
    throw new InvalidOperationException(
        "Owned shape snapshot did not survive deletion of its source registry entry.");
}

var meshSource = model.MakeBox(6, 7, 8);
using var ownedMesh = model.CreateMeshResource(meshSource);
model.Delete(meshSource);
var ownedMeshData = ownedMesh.GetMesh();
if (ownedMesh.NodeCount <= 0 ||
    ownedMesh.TriangleCount <= 0 ||
    ownedMeshData.Nodes.Count != ownedMesh.NodeCount ||
    ownedMeshData.Triangles.Count != ownedMesh.TriangleCount)
{
    throw new InvalidOperationException(
        "Owned mesh snapshot did not survive deletion of its source registry entry.");
}

var directVertices = new OcctMeshVertex[ownedMesh.NodeCount];
var directTriangles = new OcctModelMeshTriangle[ownedMesh.TriangleCount];
if (ownedMesh.CopyVertices(directVertices) != directVertices.Length ||
    ownedMesh.CopyTriangles(directTriangles) != directTriangles.Length)
{
    throw new InvalidOperationException("Direct mesh Span copy returned an unexpected element count.");
}
if (directVertices.Length == 0 || !directVertices[0].Point.IsFinite)
    throw new InvalidOperationException("Direct mesh Span copy returned an invalid vertex.");


var box = model.MakeBox(100, 80, 60);
var cylinder = model.MakeCylinder(new OcctPoint3d(50, 40, -10), OcctVector3d.UnitZ, 12, 80);
var cut = model.Cut(box, cylinder);

if (!cut.Succeeded || !model.IsShapeValid(cut.Shape))
    throw new InvalidOperationException("Boolean result is invalid.");

using var cutAlgorithm = model.AcquireAlgorithm(cut);
if (cutAlgorithm.OperationId != cut.OperationId ||
    cutAlgorithm.HasWarnings != cut.HasWarnings ||
    cutAlgorithm.HasErrors != cut.HasErrors ||
    !string.Equals(cutAlgorithm.Report, cut.Report, StringComparison.Ordinal))
{
    throw new InvalidOperationException("Owned algorithm diagnostics differ from the source operation result.");
}

var cutHistory = model.GetTopologyHistorySummary(cut.OperationId, box);
if (cutHistory.GeneratedCount < 0 || cutHistory.ModifiedCount < 0)
    throw new InvalidOperationException("Topology-history summary contains invalid counts.");

var lineageShapeCount = model.ShapeCount;
var generatedLineage = model.GetGeneratedShapes(cut.OperationId, box);
var modifiedLineage = model.GetModifiedShapes(cut.OperationId, box);
var generatedLineageAgain = model.GetGeneratedShapes(cut.OperationId, box);
var modifiedLineageAgain = model.GetModifiedShapes(cut.OperationId, box);
if (!generatedLineage.SequenceEqual(generatedLineageAgain) ||
    !modifiedLineage.SequenceEqual(modifiedLineageAgain) ||
    model.ShapeCount != lineageShapeCount)
{
    throw new InvalidOperationException("Topology-history lineage IDs are not stable across repeated queries.");
}

var inertia = model.GetVolumeInertiaProperties(cut.Shape);
if (!double.IsFinite(inertia.Mass) || inertia.Mass <= 0 || !inertia.CenterOfMass.IsFinite)
    throw new InvalidOperationException("Volume inertia properties are invalid.");
if (!inertia.PrincipalAxis1.IsFinite || !inertia.PrincipalAxis2.IsFinite || !inertia.PrincipalAxis3.IsFinite)
    throw new InvalidOperationException("Principal inertia axes are invalid.");

var bounds = model.GetShapeOrientedBounds(cut.Shape, optimal: true);
if (!bounds.IsFinite || bounds.SizeX <= 0 || bounds.SizeY <= 0 || bounds.SizeZ <= 0)
    throw new InvalidOperationException("Oriented bounding box is invalid.");

var faceCount = model.GetTopologyCount(cut.Shape, OcctShapeType.Face);
var faces = model.GetSubshapes(cut.Shape, OcctShapeType.Face);
if (faceCount <= 0 || faces.Count != faceCount)
    throw new InvalidOperationException("Boolean result face collection is invalid.");

var firstFace = faces[0];

var topologyReference = model.CreateTopologyReference(cut.Shape, firstFace);
var topologyResolution = model.ResolveTopologyReference(cut.Shape, topologyReference);
if (topologyResolution.Status != OcctTopologyReferenceStatus.Resolved ||
    !topologyResolution.Shape.HasValue ||
    !model.IsSameShape(firstFace, topologyResolution.Shape.Value))
{
    throw new InvalidOperationException("Topology reference did not resolve to the source face.");
}

model.Triangulate(cut.Shape);
var faceMesh = model.GetFaceMesh(firstFace);
var (faceVertexCount, faceTriangleCount) = model.GetFaceMeshCounts(firstFace);
var directFaceVertices = new OcctMeshVertex[faceVertexCount];
var directFaceTriangles = new OcctModelMeshTriangle[faceTriangleCount];
var directFaceWritten = model.CopyFaceMesh(firstFace, directFaceVertices, directFaceTriangles);
if (directFaceWritten.VerticesWritten != faceVertexCount ||
    directFaceWritten.TrianglesWritten != faceTriangleCount)
{
    throw new InvalidOperationException("Direct face mesh Span copy returned an unexpected element count.");
}
var shapeMesh = model.GetShapeMesh(cut.Shape);
var faceUv = model.GetFaceUvBounds(firstFace);
var faceU = (faceUv.UMin + faceUv.UMax) * 0.5;
var faceV = (faceUv.VMin + faceUv.VMax) * 0.5;
var facePeriodicity = model.GetFacePeriodicity(firstFace);
var faceDifferential = model.EvaluateFaceDifferential(firstFace, faceU, faceV);
var faceCurvature = model.GetFaceCurvature(firstFace, faceU, faceV);
if (!faceDifferential.HasNormal || !faceCurvature.HasNormal)
    throw new InvalidOperationException("Face differential geometry has no normal.");
_ = facePeriodicity;
if (faceMesh.Nodes.Count == 0 || faceMesh.Triangles.Count == 0)
    throw new InvalidOperationException("Face triangulation is empty.");
if (shapeMesh.Nodes.Count == 0 || shapeMesh.Triangles.Count == 0)
    throw new InvalidOperationException("Whole-shape triangulation is empty.");

var rayHits = model.IntersectRay(
    cut.Shape,
    new OcctPoint3d(20, 20, 100),
    new OcctVector3d(0, 0, -1));
if (rayHits.Count == 0)
    throw new InvalidOperationException("Expected at least one ray hit.");

var horizontal = model.MakeLine(
    new OcctPoint3d(0, 0, 0),
    new OcctPoint3d(100, 0, 0));
var vertical = model.MakeLine(
    new OcctPoint3d(50, -20, 0),
    new OcctPoint3d(50, 20, 0));
var edgeIntersections = model.IntersectEdges(horizontal, vertical);
if (edgeIntersections.Count == 0 || edgeIntersections[0].Kind != OcctIntersectionKind.Point)
    throw new InvalidOperationException("Structured edge intersection did not return the expected point hit.");
var crossing = edgeIntersections[0].StartPoint;
if (crossing.DistanceTo(new OcctPoint3d(50, 0, 0)) > 1e-6)
    throw new InvalidOperationException("Structured edge intersection returned the wrong point.");

var annotationX = model.MakeLine(OcctPoint3d.Origin, new OcctPoint3d(40, 0, 0));
var annotationY = model.MakeLine(OcctPoint3d.Origin, new OcctPoint3d(0, 30, 0));
var brepText = model.MakeBRepText(
    "OCCT",
    OcctBRepTextOptions.Default with
    {
        Position = new OcctPoint3d(0, 45, 0),
        Height = 6,
        ExtrusionDepth = 1
    });
var lengthAnnotation = model.MakeLengthAnnotation(
    annotationX,
    OcctBRepAnnotationOptions.Default with { Offset = 10, TextHeight = 4, ArrowSize = 2 });
var angleAnnotation = model.MakeAngleAnnotation(
    annotationX,
    annotationY,
    OcctBRepAnnotationOptions.Default with { Offset = 12, TextHeight = 4, ArrowSize = 2 });

foreach (var generated in new[] { brepText, lengthAnnotation, angleAnnotation })
{
    if (!model.IsShapeValid(generated) || model.GetTopologyCount(generated, OcctShapeType.Edge) <= 0)
        throw new InvalidOperationException("Headless BRep text/annotation generation produced invalid geometry.");
}

var lowerCircle = model.MakeCircle(new OcctPoint3d(0, 0, 0), OcctVector3d.UnitZ, 10);
var radiusAnnotation = model.MakeRadiusAnnotation(
    lowerCircle,
    OcctBRepAnnotationOptions.Default with { Offset = 8, TextHeight = 4, ArrowSize = 2 });
var diameterAnnotation = model.MakeDiameterAnnotation(
    lowerCircle,
    OcctBRepAnnotationOptions.Default with { Offset = 8, TextHeight = 4, ArrowSize = 2 });
foreach (var generated in new[] { radiusAnnotation, diameterAnnotation })
{
    if (!model.IsShapeValid(generated) || model.GetTopologyCount(generated, OcctShapeType.Edge) <= 0)
        throw new InvalidOperationException("Headless circular BRep annotation generation produced invalid geometry.");
}

var circleRange = model.GetEdgeParameterRange(lowerCircle);
var circleParameter = (circleRange.FirstParameter + circleRange.LastParameter) * 0.5;
var circleDifferential = model.EvaluateEdgeAtParameter(lowerCircle, circleParameter);
var circleCurvature = model.GetEdgeCurvature(lowerCircle, circleParameter);
if (!circleCurvature.HasTangent || Math.Abs(circleCurvature.Curvature - 0.1) > 1e-6)
    throw new InvalidOperationException("Circle differential geometry is invalid.");
if (circleDifferential.FirstDerivative.X == 0 && circleDifferential.FirstDerivative.Y == 0)
    throw new InvalidOperationException("Circle first derivative is invalid.");

var circleSpan = circleRange.LastParameter - circleRange.FirstParameter;
var trimmedCircle = model.TrimEdge(
    lowerCircle,
    circleRange.FirstParameter + circleSpan * 0.1,
    circleRange.FirstParameter + circleSpan * 0.6);
if (!model.IsShapeValid(trimmedCircle))
    throw new InvalidOperationException("Trimmed edge is invalid.");

var outerWire = model.MakeRectangleWire(40, 30);
var innerWire = model.MakeRectangleWire(
    10,
    8,
    new OcctPoint3d(12, 10, 0));
var faceWithHole = model.MakePlanarFace(outerWire, new[] { innerWire });
if (!model.IsShapeValid(faceWithHole) || model.GetInnerWires(faceWithHole).Count != 1)
    throw new InvalidOperationException("Planar face with hole is invalid.");

var offsetWire = model.OffsetWire(outerWire, 2.0);
if (!model.IsShapeValid(offsetWire))
    throw new InvalidOperationException("Planar wire offset is invalid.");

var upperCircle = model.MakeCircle(new OcctPoint3d(0, 0, 25), OcctVector3d.UnitZ, 16);
var lowerWire = model.MakeWire(new[] { lowerCircle });
var upperWire = model.MakeWire(new[] { upperCircle });
var loft = model.Loft(new[] { lowerWire, upperWire });
if (!model.IsShapeValid(loft.Shape))
    throw new InvalidOperationException("Loft result is invalid.");

var brepPath = Path.Combine(Path.GetTempPath(), $"occt-model-{Guid.NewGuid():N}.brep");
try
{
    model.ExportBrep(cut.Shape, brepPath);
    if (!File.Exists(brepPath) || new FileInfo(brepPath).Length == 0)
        throw new InvalidOperationException("BREP export produced no file content.");

    var imported = model.ImportBrep(brepPath);
    if (!model.IsShapeValid(imported))
        throw new InvalidOperationException("Headless BREP round trip failed.");
}
finally
{
    if (File.Exists(brepPath)) File.Delete(brepPath);
}

var stepPath = Path.Combine(Path.GetTempPath(), $"occt-model-{Guid.NewGuid():N}.step");
try
{
    model.ExportStep(cut.Shape, stepPath);
    if (!File.Exists(stepPath) || new FileInfo(stepPath).Length == 0)
        throw new InvalidOperationException("STEP export produced no file content.");

    var imported = model.ImportStep(stepPath);
    if (!model.IsShapeValid(imported))
        throw new InvalidOperationException("Headless STEP round trip failed.");
}
finally
{
    if (File.Exists(stepPath)) File.Delete(stepPath);
}

var healed = model.FixShape(cut.Shape);
var unified = model.UnifySameDomain(healed.Shape);
if (!model.IsShapeValid(unified.Shape))
    throw new InvalidOperationException("Healed and unified shape is invalid.");

Console.WriteLine($"OCCT {OcctEngine.OcctVersion}");
Console.WriteLine($"Bridge {OcctBridgeInfo.ManagedVersion} / ABI {OcctBridgeInfo.ExpectedAbiVersion}");
Console.WriteLine($"Modeling capabilities: {OcctModelingSession.Capabilities}");
Console.WriteLine($"Shapes: {model.ShapeCount}");
Console.WriteLine($"Faces: {faceCount}");
Console.WriteLine($"Volume mass: {inertia.Mass:G6}");
Console.WriteLine($"Face mesh: {faceMesh.Nodes.Count} nodes, {faceMesh.Triangles.Count} triangles");
Console.WriteLine($"Shape mesh: {shapeMesh.Nodes.Count} nodes, {shapeMesh.Triangles.Count} triangles");
Console.WriteLine($"Ray hits: {rayHits.Count}");
Console.WriteLine($"Edge intersections: {edgeIntersections.Count}");
Console.WriteLine($"Topology reference score: {topologyResolution.Score:G4}");
Console.WriteLine($"OBB: {bounds.SizeX:G4} x {bounds.SizeY:G4} x {bounds.SizeZ:G4}");
Console.WriteLine($"Cut operation resource: {cutAlgorithm.OperationId}");
Console.WriteLine($"Loft operation: {loft.OperationId}");
Console.WriteLine("BRep text and four annotation kinds: validated");

model.Dispose();
if (cutAlgorithm.OperationId != cut.OperationId ||
    cutAlgorithm.HasWarnings != cut.HasWarnings ||
    cutAlgorithm.HasErrors != cut.HasErrors ||
    !string.Equals(cutAlgorithm.Report, cut.Report, StringComparison.Ordinal))
{
    throw new InvalidOperationException(
        "Owned algorithm diagnostics did not survive disposal of the source modeling session.");
}
Console.WriteLine("Owned algorithm diagnostics after session disposal: validated");
Console.WriteLine($"Bridge {OcctBridgeInfo.ManagedVersion} native smoke tests passed.");
