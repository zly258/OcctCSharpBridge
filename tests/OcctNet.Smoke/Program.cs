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

var xbfPath = Path.Combine(Path.GetTempPath(), $"occt-ocaf-{Guid.NewGuid():N}.xbf");
try
{
    using (var document = new OcafDocument())
    {
        if (OcafDocument.NativeVersion != "7.9.0")
            throw new InvalidOperationException($"Unexpected OCAF runtime: {OcafDocument.NativeVersion}");
        if (document.StorageFormatVersion != OcafStorageFormatVersion.Current)
            throw new InvalidOperationException("Unexpected OCAF storage format version.");

        document.UndoLimit = 10;
        OcafLabel dataLabel;
        OcafLabel widthVariable;
        OcafLabel heightVariable;
        OcafLabel expressionLabel;
        OcafLabel relationLabel;
        OcafLabel shapeLabel;
        OcafLabel subshapeLabel;
        OcafLabel colorDefinition;
        OcafLabel layer;
        OcafLabel materialDefinition;

        using (var command = document.BeginCommand())
        {
            dataLabel = document.NewChild(document.Main);
            document.SetName(dataLabel, "Smoke metadata");
            document.SetComment(dataLabel, "参数化数据");
            document.SetInteger(dataLabel, 42);
            document.SetRealArray(dataLabel, new[] { 1.25, 2.5, 5.0 }, lower: -1);

            widthVariable = document.NewChild(dataLabel);
            heightVariable = document.NewChild(dataLabel);
            expressionLabel = document.NewChild(dataLabel);
            relationLabel = document.NewChild(dataLabel);
            document.SetVariable(widthVariable, "Width", 100.0, "mm", isConstant: true);
            document.SetVariable(heightVariable, "Height", 80.0, "mm");
            document.SetExpression(expressionLabel, "Width + Height", new[] { widthVariable, heightVariable });
            document.SetRelation(relationLabel, "Height <= Width", new[] { heightVariable, widthVariable });
            document.AssignVariableExpression(heightVariable, "Width * 0.8", new[] { widthVariable });

            shapeLabel = document.AddShape(model, cut.Shape, makeAssembly: false);
            document.SetName(shapeLabel, "Cut body");
            subshapeLabel = document.AddSubshape(shapeLabel, model, firstFace);
            document.SetName(subshapeLabel, "First face");

            colorDefinition = document.AddColorDefinition(new OcafColor(0.2, 0.4, 0.8));
            document.SetColor(shapeLabel, OcafColorType.Surface, colorDefinition);

            layer = document.AddLayer("Machined");
            document.SetLayer(shapeLabel, layer, oneLayerOnly: true);

            materialDefinition = document.AddMaterialDefinition(
                "Steel", "Smoke-test material", 7850.0, "density", "kg/m3");
            document.SetMaterial(shapeLabel, materialDefinition);

            document.SetArea(shapeLabel, model.GetSurfaceProperties(cut.Shape).Mass);
            document.SetVolume(shapeLabel, model.GetVolumeProperties(cut.Shape).Mass);
            document.SetCentroid(shapeLabel, model.GetVolumeProperties(cut.Shape).CenterOfMass);
            document.MarkModified(shapeLabel);

            _ = command.Commit();
        }

        if (document.GetChildCount(dataLabel) != 4 || !document.IsDescendant(widthVariable, dataLabel))
            throw new InvalidOperationException("Extended TDF label queries failed.");
        if (document.GetAttributeCount(widthVariable) == 0 || document.GetTransaction(widthVariable) < 0)
            throw new InvalidOperationException("Extended TDF label state failed.");

        var width = document.GetVariable(widthVariable);
        var height = document.GetVariable(heightVariable);
        if (width.Value != 100.0 || width.Unit != "mm" || !width.IsConstant)
            throw new InvalidOperationException("TDataStd variable round trip failed.");
        if (!height.IsAssigned || document.GetExpressionVariables(expressionLabel).Count != 2)
            throw new InvalidOperationException("TDataStd expression variable references failed.");
        if (document.GetExpression(expressionLabel) != "Width + Height" ||
            document.GetRelation(relationLabel) != "Height <= Width" ||
            document.GetExpressionVariables(relationLabel, relation: true).Count != 2)
            throw new InvalidOperationException("TDataStd expression or relation round trip failed.");

        if (document.SearchShape(model, cut.Shape) != shapeLabel ||
            document.FindSubshape(shapeLabel, model, firstFace) != subshapeLabel ||
            document.GetSubshapes(shapeLabel).Count != 1)
            throw new InvalidOperationException("Extended XDE shape search or subshape storage failed.");
        if (!document.IsTopLevelShape(shapeLabel) || document.GetComponentCount(shapeLabel) != 0)
            throw new InvalidOperationException("Extended XDE shape classification failed.");

        if (!document.IsColorDefinition(colorDefinition) || !document.HasColor(shapeLabel, OcafColorType.Surface) ||
            document.GetColorDefinitionLabel(shapeLabel, OcafColorType.Surface) != colorDefinition ||
            document.FindColorDefinition(new OcafColor(0.2, 0.4, 0.8)) != colorDefinition)
            throw new InvalidOperationException("Extended XDE color definition workflow failed.");

        if (!document.IsLayerDefinition(layer) || !document.HasLayer(shapeLabel, layer) ||
            document.FindLayer("Machined") != layer || document.GetShapesOnLayer(layer).Count != 1)
            throw new InvalidOperationException("Extended XDE layer workflow failed.");

        if (!document.IsMaterialDefinition(materialDefinition) || document.GetMaterialLabel(shapeLabel) != materialDefinition)
            throw new InvalidOperationException("Extended XDE material workflow failed.");

        if (document.GetModifiedLabels().Count == 0)
            throw new InvalidOperationException("Modified-label tracking failed.");
        document.PurgeModified();
        if (document.GetModifiedLabels().Count != 0)
            throw new InvalidOperationException("Modified-label purge failed.");

        if (document.AvailableUndos <= 0 || !document.Undo() || !document.Redo())
            throw new InvalidOperationException("OCAF undo/redo failed.");

        document.SaveAs(xbfPath);
    }

    using var reopened = OcafDocument.Open(xbfPath);
    var reopenedShapes = reopened.GetShapes(freeOnly: true);
    if (reopenedShapes.Count != 1)
        throw new InvalidOperationException("XBF round trip lost the XDE shape.");
    if (reopened.GetName(reopenedShapes[0]) != "Cut body")
        throw new InvalidOperationException("XBF round trip lost the XDE name.");
    if (!reopened.TryGetColor(reopenedShapes[0], OcafColorType.Surface, out _))
        throw new InvalidOperationException("XBF round trip lost the XDE color.");
    if (reopened.GetLayers(reopenedShapes[0]).Count != 1)
        throw new InvalidOperationException("XBF round trip lost the XDE layer.");
    if (reopened.GetMaterialLabel(reopenedShapes[0]) is null)
        throw new InvalidOperationException("XBF round trip lost the XDE material.");
    if (reopened.GetSubshapes(reopenedShapes[0]).Count != 1)
        throw new InvalidOperationException("XBF round trip lost the XDE subshape label.");

    var extracted = reopened.GetShape(reopenedShapes[0], model);
    if (!model.IsValid(extracted))
        throw new InvalidOperationException("XDE-to-modeling shape interop failed.");
}
finally
{
    if (File.Exists(xbfPath)) File.Delete(xbfPath);
}

Console.WriteLine($"OCCT {OcctEngine.OcctVersion}");
Console.WriteLine($"Modeling capabilities: {OcctModelingSession.Capabilities}");
Console.WriteLine($"OCAF capabilities: {OcafDocument.Capabilities}");
Console.WriteLine($"Shapes: {model.ShapeCount}");
Console.WriteLine($"Faces: {faceCount}");
Console.WriteLine($"Mesh: {faceMesh.Nodes.Count} nodes, {faceMesh.Triangles.Count} triangles");
Console.WriteLine($"Ray hits: {rayHits.Count}");
Console.WriteLine($"Loft operation: {loft.OperationId}");
Console.WriteLine("Modeling and extended OCAF/XDE smoke tests passed.");
