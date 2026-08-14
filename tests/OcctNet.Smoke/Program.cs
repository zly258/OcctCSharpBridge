using OcctNet;

if (OcctBridgeInfo.NativeAbiVersion != OcctBridgeInfo.ExpectedAbiVersion)
    throw new InvalidOperationException("Native bridge ABI validation failed.");
if (!string.Equals(OcctBridgeInfo.NativeVersion, OcctBridgeInfo.ManagedVersion, StringComparison.Ordinal))
    throw new InvalidOperationException(
        $"Managed/native bridge version mismatch: {OcctBridgeInfo.ManagedVersion} / {OcctBridgeInfo.NativeVersion}.");
if (string.IsNullOrWhiteSpace(OcctBridgeInfo.BuildInfo))
    throw new InvalidOperationException("Native bridge build information is empty.");

using var model = new OcctModelingSession();

var box = model.MakeBox(100, 80, 60);
var cylinder = model.MakeCylinder(new OcctPoint3d(50, 40, -10), OcctVector3d.UnitZ, 12, 80);
var cut = model.Cut(box, cylinder);

if (!cut.Succeeded || !model.IsShapeValid(cut.Shape))
    throw new InvalidOperationException("Boolean result is invalid.");

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
// P0: full volume inertia properties.
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

// P2: persistent topology fingerprint/reference resolves on the current root.
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

// Keep the ray inside the box footprint but outside the through-hole centered at (50, 40).
var rayHits = model.IntersectRay(
    cut.Shape,
    new OcctPoint3d(20, 20, 100),
    new OcctVector3d(0, 0, -1));
if (rayHits.Count == 0)
    throw new InvalidOperationException("Expected at least one ray hit.");

// P1: structured edge/edge intersection with native curve parameters.
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

var lowerCircle = model.MakeCircle(new OcctPoint3d(0, 0, 0), OcctVector3d.UnitZ, 10);
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
Console.WriteLine($"Loft operation: {loft.OperationId}");
Console.WriteLine($"Bridge {OcctBridgeInfo.ManagedVersion} native smoke tests passed.");