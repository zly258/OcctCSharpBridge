using OcctNet;

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
var firstFace = model.GetSubshape(cut.Shape, OcctShapeType.Face, 0);
var faceMesh = model.GetFaceMesh(firstFace);
if (faceMesh.Nodes.Count == 0 || faceMesh.Triangles.Count == 0)
    throw new InvalidOperationException("Face triangulation is empty.");

var rayHits = model.IntersectRay(
    cut.Shape,
    new OcctPoint3d(50, 40, 100),
    new OcctVector3d(0, 0, -1));
if (rayHits.Count == 0)
    throw new InvalidOperationException("Expected at least one ray hit.");

var lowerCircle = model.MakeCircle(new OcctPoint3d(0, 0, 0), OcctVector3d.UnitZ, 10);
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
    var imported = model.ImportBrep(brepPath);
    if (!model.IsValid(imported))
        throw new InvalidOperationException("Headless BREP round trip failed.");
}
finally
{
    if (File.Exists(brepPath)) File.Delete(brepPath);
}

var healed = model.FixShape(cut.Shape);
var unified = model.UnifySameDomain(healed.Shape);
if (!model.IsValid(unified.Shape))
    throw new InvalidOperationException("Healed and unified shape is invalid.");

Console.WriteLine($"OCCT {OcctEngine.OcctVersion}");
Console.WriteLine($"Capabilities: {OcctModelingSession.Capabilities}");
Console.WriteLine($"Shapes: {model.ShapeCount}");
Console.WriteLine($"Faces: {faceCount}");
Console.WriteLine($"Mesh: {faceMesh.Nodes.Count} nodes, {faceMesh.Triangles.Count} triangles");
Console.WriteLine($"Ray hits: {rayHits.Count}");
Console.WriteLine($"Loft operation: {loft.OperationId}");
Console.WriteLine("Smoke test passed.");
