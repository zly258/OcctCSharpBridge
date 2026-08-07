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

if (!cut.Succeeded || !model.IsValid(cut.Shape))
    throw new InvalidOperationException("Boolean result is invalid.");

var faceCount = model.GetTopologyCount(cut.Shape, OcctShapeType.Face);
if (faceCount <= 0)
    throw new InvalidOperationException("Boolean result contains no faces.");

model.Mesh(cut.Shape);
var firstFace = model.GetSubshapeAt(cut.Shape, OcctShapeType.Face, 0);
var faceMesh = model.GetFaceMesh(firstFace);
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

var rayHits = model.IntersectRay(
    cut.Shape,
    new OcctPoint3d(50, 40, 100),
    new OcctVector3d(0, 0, -1));
if (rayHits.Count == 0)
    throw new InvalidOperationException("Expected at least one ray hit.");

var lowerCircle = model.MakeCircle(new OcctPoint3d(0, 0, 0), OcctVector3d.UnitZ, 10);
var circleRange = model.GetEdgeParameterRange(lowerCircle);
var circleParameter = (circleRange.FirstParameter + circleRange.LastParameter) * 0.5;
var circleDifferential = model.EvaluateEdgeAtParameter(lowerCircle, circleParameter);
var circleCurvature = model.GetEdgeCurvature(lowerCircle, circleParameter);
if (!circleCurvature.HasTangent || Math.Abs(circleCurvature.Curvature - 0.1) > 1e-6)
    throw new InvalidOperationException("Circle differential geometry is invalid.");
if (circleDifferential.FirstDerivative.X == 0 && circleDifferential.FirstDerivative.Y == 0)
    throw new InvalidOperationException("Circle first derivative is invalid.");
var upperCircle = model.MakeCircle(new OcctPoint3d(0, 0, 25), OcctVector3d.UnitZ, 16);
var lowerWire = model.MakeWire(new[] { lowerCircle });
var upperWire = model.MakeWire(new[] { upperCircle });
var loft = model.Loft(new[] { lowerWire, upperWire });
if (!model.IsValid(loft.Shape))
    throw new InvalidOperationException("Loft result is invalid.");

var brepPath = Path.Combine(Path.GetTempPath(), $"occt-model-{Guid.NewGuid():N}.brep");
try
{
    model.ExportBrep(cut.Shape, brepPath);
    if (!File.Exists(brepPath) || new FileInfo(brepPath).Length == 0)
        throw new InvalidOperationException("BREP export produced no file content.");

    var imported = model.ImportBrep(brepPath);
    if (!model.IsValid(imported))
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
    if (!model.IsValid(imported))
        throw new InvalidOperationException("Headless STEP round trip failed.");
}
finally
{
    if (File.Exists(stepPath)) File.Delete(stepPath);
}

var healed = model.FixShape(cut.Shape);
var unified = model.UnifySameDomain(healed.Shape);
if (!model.IsValid(unified.Shape))
    throw new InvalidOperationException("Healed and unified shape is invalid.");

Console.WriteLine($"OCCT {OcctEngine.OcctVersion}");
Console.WriteLine($"Modeling capabilities: {OcctModelingSession.Capabilities}");
Console.WriteLine($"Shapes: {model.ShapeCount}");
Console.WriteLine($"Faces: {faceCount}");
Console.WriteLine($"Mesh: {faceMesh.Nodes.Count} nodes, {faceMesh.Triangles.Count} triangles");
Console.WriteLine($"Ray hits: {rayHits.Count}");
Console.WriteLine($"Loft operation: {loft.OperationId}");
Console.WriteLine("BREP and STEP exchange round trips passed.");
Console.WriteLine("Modeling smoke tests passed.");
