using System.Drawing;
using OcctNet;

namespace OcctDemo.Common;

public sealed partial class DemoSession
{
    public DemoCommandResult Execute(DemoCommandId commandId, IReadOnlyDictionary<string, string>? rawValues = null)
    {
        EnsureCommandAvailable(commandId);

        var storedValues = rawValues is null
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(rawValues, StringComparer.OrdinalIgnoreCase);

        var selectedObjects = Engine.SelectedObjects;
        var selectedObjectIds = new List<long>(selectedObjects.Count);
        var seenIds = new HashSet<long>();
        foreach (var item in selectedObjects)
        {
            if (seenIds.Add(item.Id))
                selectedObjectIds.Add(item.Id);
        }

        var values = new DemoValues(storedValues);
        var isDemoCommand = IsDemoCommand(commandId);
        var displayBatch = isDemoCommand ? Engine.BeginDisplayBatch() : null;
        var demoInitialObjectIds = isDemoCommand
            ? new HashSet<long>(Engine.GetObjects().Select(item => item.Id))
            : null;
        DemoCommandResult result;
        try
        {
            result = commandId switch
            {
            DemoCommandId.Point => CreateShape(DemoLocalization.CommandText(commandId), Engine.MakeVertex(values.Point())),
            DemoCommandId.Line => CreateShape(DemoLocalization.CommandText(commandId), Engine.MakeLine(new(values.Number("x1"), values.Number("y1"), values.Number("z1")), new(values.Number("x2"), values.Number("y2"), values.Number("z2")))),
            DemoCommandId.Polyline => CreateShape(DemoLocalization.CommandText(commandId), Engine.MakePolyline(values.Points("points"), values.Boolean("closed"))),
            DemoCommandId.Circle => CreateShape(DemoLocalization.CommandText(commandId), Engine.MakeCircle(values.Point(), OcctVector3d.UnitZ, values.Number("radius"))),
            DemoCommandId.ArcThreePoints => CreateShape(DemoLocalization.CommandText(commandId), Engine.MakeArc(new(values.Number("x1"), values.Number("y1"), values.Number("z")), new(values.Number("x2"), values.Number("y2"), values.Number("z")), new(values.Number("x3"), values.Number("y3"), values.Number("z")))),
            DemoCommandId.ArcCenter => CreateShape(DemoLocalization.CommandText(commandId), Engine.MakeArc(values.Point(), OcctVector3d.UnitZ, OcctVector3d.UnitX, values.Number("radius"), values.Number("start"), values.Number("end"))),
            DemoCommandId.Ellipse => CreateShape(DemoLocalization.CommandText(commandId), Engine.MakeEllipse(values.Point(), OcctVector3d.UnitZ, values.Number("major"), values.Number("minor"))),
            DemoCommandId.Rectangle => CreateRectangle(values),
            DemoCommandId.Polygon => CreateShape(DemoLocalization.CommandText(commandId), Engine.MakeRegularPolygon(values.Number("radius"), values.Integer("sides"), values.Boolean("face"), values.Point())),
            DemoCommandId.Bezier => CreateShape(DemoLocalization.CommandText(commandId), Engine.MakeBezier(values.Points("points"))),
            DemoCommandId.BSpline => CreateShape(DemoLocalization.CommandText(commandId), Engine.MakeInterpolatedBSpline(values.Points("points"), values.Boolean("periodic"))),

            DemoCommandId.Box => CreateShape(DemoLocalization.CommandText(commandId), Engine.MakeBox(values.Number("dx"), values.Number("dy"), values.Number("dz"), values.Number("x"), values.Number("y"), values.Number("z"))),
            DemoCommandId.Cylinder => CreateShape(DemoLocalization.CommandText(commandId), Engine.MakeCylinder(values.Number("radius"), values.Number("height"), values.Number("x"), values.Number("y"), values.Number("z"))),
            DemoCommandId.Frustum => CreateShape(DemoLocalization.CommandText(commandId), Engine.MakeCone(values.Number("r1"), values.Number("r2"), values.Number("height"), values.Number("x"), values.Number("y"), values.Number("z"))),
            DemoCommandId.Cone => CreateShape(DemoLocalization.CommandText(commandId), Engine.MakeCone(values.Number("radius"), 0, values.Number("height"), values.Number("x"), values.Number("y"), values.Number("z"))),
            DemoCommandId.Torus => CreateShape(DemoLocalization.CommandText(commandId), Engine.MakeTorus(values.Number("major"), values.Number("minor"), values.Point())),
            DemoCommandId.Sphere => CreateShape(DemoLocalization.CommandText(commandId), Engine.MakeSphere(values.Number("radius"), values.Number("x"), values.Number("y"), values.Number("z"))),
            DemoCommandId.Wedge => CreateShape(DemoLocalization.CommandText(commandId), Engine.MakeWedge(values.Number("dx"), values.Number("dy"), values.Number("dz"), values.Number("ltx"))),
            DemoCommandId.Pipe => CreatePipe(values),
            DemoCommandId.Extrude => CreateShape(DemoLocalization.CommandText(commandId), Engine.Extrude(RequireShape(), values.Vector("dx", "dy", "dz"), values.Boolean("hide", true))),
            DemoCommandId.Revolve => CreateShape(DemoLocalization.CommandText(commandId), Engine.Revolve(RequireShape(), values.Point("px", "py", "pz"), values.Vector("ax", "ay", "az"), values.Number("angle", 360), values.Boolean("hide", true))),
            DemoCommandId.Sweep => Sweep(values),
            DemoCommandId.Loft => Loft(values),
            DemoCommandId.Fuse => Boolean(values, OcctBooleanOperation.Fuse, DemoLocalization.CommandText(commandId)),
            DemoCommandId.Cut => Boolean(values, OcctBooleanOperation.Cut, DemoLocalization.CommandText(commandId)),
            DemoCommandId.Common => Boolean(values, OcctBooleanOperation.Common, DemoLocalization.CommandText(commandId)),
            DemoCommandId.Section => Boolean(values, OcctBooleanOperation.Section, DemoLocalization.CommandText(commandId)),
            DemoCommandId.Fillet => CreateShape(DemoLocalization.CommandText(commandId), Engine.FilletAllEdges(RequireShape(), values.Number("radius"), values.Boolean("hide", true))),
            DemoCommandId.Chamfer => CreateShape(DemoLocalization.CommandText(commandId), Engine.ChamferAllEdges(RequireShape(), values.Number("distance"), values.Boolean("hide", true))),
            DemoCommandId.Offset => CreateShape(DemoLocalization.CommandText(commandId), Engine.Offset(RequireShape(), values.Number("offset"), values.Number("tolerance", 0.0001), values.Boolean("hide", true))),
            DemoCommandId.Shell => CreateShape(DemoLocalization.CommandText(commandId), Engine.MakeThickSolid(RequireShape(), values.Integer("face"), values.Number("thickness"), 0.0001, values.Boolean("hide", true))),
            DemoCommandId.Drill => CreateShape(DemoLocalization.CommandText(commandId), Engine.DrillHole(RequireShape(), values.Point(), OcctVector3d.UnitZ, values.Number("radius"), values.Number("depth"), values.Boolean("hide", true))),

            DemoCommandId.Translate => CreateShape(DemoLocalization.CommandText(commandId), Engine.Translate(RequireShape(), values.Vector("dx", "dy", "dz"), values.Boolean("hide", true))),
            DemoCommandId.Rotate => CreateShape(DemoLocalization.CommandText(commandId), Engine.Rotate(RequireShape(), values.Point("px", "py", "pz"), values.Vector("ax", "ay", "az"), values.Number("angle"), values.Boolean("hide", true))),
            DemoCommandId.Scale => CreateShape(DemoLocalization.CommandText(commandId), Engine.Scale(RequireShape(), values.Point(), values.Number("factor"), values.Boolean("hide", true))),
            DemoCommandId.Mirror => CreateShape(DemoLocalization.CommandText(commandId), Engine.MirrorPlane(RequireShape(), values.Point(), values.Vector("nx", "ny", "nz"), values.Boolean("hide"))),
            DemoCommandId.Copy => CreateShape(DemoLocalization.CommandText(commandId), Engine.Copy(RequireShape(), values.Boolean("hide"))),
            DemoCommandId.Delete => DeleteSelected(),

            // AutoCAD-style BRep Geometric Annotations & 3D Text (AIS_Shape)
            DemoCommandId.Text => CreateText(values),
            DemoCommandId.LengthDimension => CreateLengthDimension(values),
            DemoCommandId.AngleDimension => CreateAngleDimension(values),
            DemoCommandId.RadiusDimension => CreateRadiusDimension(values, false),
            DemoCommandId.DiameterDimension => CreateRadiusDimension(values, true),

            DemoCommandId.AnalyzeBounds => AnalyzeBounds(),
            DemoCommandId.AnalyzeMass => AnalyzeMass(),
            DemoCommandId.AnalyzeTopology => AnalyzeTopology(),
            DemoCommandId.AnalyzeDistance => AnalyzeDistance(),
            DemoCommandId.ValidateShape => ValidateShape(),

            DemoCommandId.DemoPrimitives => DemoPrimitives(),
            DemoCommandId.DemoBracket => DemoBracket(),
            DemoCommandId.DemoFlange => DemoFlange(),
            DemoCommandId.DemoPipe => DemoPipe(),
            DemoCommandId.DemoTee => DemoTee(),
            DemoCommandId.DemoReducer => DemoReducer(),
            DemoCommandId.DemoLoft => DemoLoft(),
            DemoCommandId.DemoBoolean => DemoBoolean(),
            DemoCommandId.DemoElements => DemoElements(),
            DemoCommandId.DemoGear => DemoGear(),
            DemoCommandId.DemoManifold => DemoManifold(),
            DemoCommandId.DemoTwistedDuct => DemoTwistedDuct(),
            DemoCommandId.DemoAnnotations => DemoAnnotations(),
            _ => throw new NotSupportedException(Local($"Command is not implemented: {commandId}", $"未实现命令：{commandId}"))
            };

            if (demoInitialObjectIds is not null)
            {
                RemoveDemoProcessObjects(demoInitialObjectIds, result.CreatedObjects);
            }
        }
        catch
        {
            if (demoInitialObjectIds is not null)
            {
                RemoveDemoProcessObjects(demoInitialObjectIds, Array.Empty<IOcctObject>());
            }
            throw;
        }
        finally
        {
            displayBatch?.Dispose();
        }

        var changed = result.CreatedObjects.Count > 0 || commandId == DemoCommandId.Delete;
        if (changed)
        {
            IsModified = true;
            if (!_restoringHistory)
            {
                if (IsUndoableCommand(commandId) && _historyAvailable)
                {
                    TruncateRedoHistory();
                    _history.Add(DemoHistoryEntry.Command(commandId, storedValues, selectedObjectIds, DemoLocalization.CommandText(commandId)));
                    _historyPosition = _history.Count;
                    HistoryChanged?.Invoke(this, EventArgs.Empty);
                }
                else if (!IsUndoableCommand(commandId))
                {
                    _historyAvailable = false;
                    ClearHistory();
                }
            }
            if (!_suppressNotifications) ModelChanged?.Invoke(this, EventArgs.Empty);
        }
        if (!_suppressNotifications) StatusChanged?.Invoke(this, result.Message);
        return result;
    }

    private DemoCommandResult CreateRectangle(DemoValues values)
    {
        var wire = Engine.MakeRectangleWire(values.Number("width"), values.Number("height"), values.Point());
        if (!values.Boolean("face")) return CreateShape(DemoLocalization.CommandText(DemoCommandId.Rectangle), wire);
        Engine.SetObjectVisible(wire, false);
        return CreateShape(Local("Rectangle Face", "矩形面"), Engine.MakeFace(wire));
    }

    private DemoCommandResult CreatePipe(DemoValues values)
    {
        var outer = Engine.MakeCylinder(values.Number("outer"), values.Number("height"), values.Number("x"), values.Number("y"), values.Number("z"));
        var innerRadius = values.Number("outer") - values.Number("wall");
        if (innerRadius <= 0) throw new InvalidOperationException(Local("Wall thickness must be smaller than the outer radius.", "壁厚必须小于外半径。"));
        var inner = Engine.MakeCylinder(innerRadius, values.Number("height") + 2, values.Number("x"), values.Number("y"), values.Number("z") - 1);
        var pipe = Engine.Cut(outer, inner, true);
        return CreateShape(DemoLocalization.CommandText(DemoCommandId.Pipe), pipe);
    }

    private DemoCommandResult Sweep(DemoValues values)
    {
        var shapes = RequireShapes(2);
        return CreateShape(DemoLocalization.CommandText(DemoCommandId.Sweep), Engine.Sweep(shapes[0], shapes[1], values.Boolean("hide", true)));
    }

    private DemoCommandResult Loft(DemoValues values)
    {
        var shapes = RequireShapes(2);
        return CreateShape(DemoLocalization.CommandText(DemoCommandId.Loft), Engine.Loft(shapes, values.Boolean("solid", true), values.Boolean("ruled"), hideInputs: values.Boolean("hide", true)));
    }

    private DemoCommandResult Boolean(DemoValues values, OcctBooleanOperation operation, string name)
    {
        var shapes = RequireShapes(2);
        return CreateShape(name, Engine.Boolean(operation, shapes[0], shapes[1], values.Boolean("hide", operation != OcctBooleanOperation.Section)));
    }

    private DemoCommandResult CreateText(DemoValues values)
    {
        using var model = new OcctModelingSession();
        var modelText = model.MakeBRepText(
            values.Text("text", "OCCT 3D CAD"),
            OcctBRepTextOptions.Default with
            {
                Position = values.Point(),
                Height = values.Number("height", 18),
                ExtrusionDepth = values.Number("depth", 2),
                FontName = DemoFonts.ResolveOcctFont(values.Text("font", "Microsoft YaHei")),
                Bold = values.Boolean("bold"),
                Italic = values.Boolean("italic")
            });
        var text = DisplayModelShape(model, modelText);
        Engine.SetObjectColor(text, Color.DarkSlateGray);
        SetGeneratedName(text, DemoLocalization.CommandText(DemoCommandId.Text));
        ActiveObject = text;
        return DemoCommandResult.Created(DemoLocalization.Text("Session.Created", DemoLocalization.CommandText(DemoCommandId.Text)), text);
    }

    private DemoCommandResult CreateLengthDimension(DemoValues values)
    {
        var edge = CopySelectedSubshape(0);
        Engine.SetObjectVisible(edge, false);
        Engine.SetObjectSelectable(edge, false);
        SetGeneratedName(edge, Local("Dimension Source Edge", "尺寸源边"));

        using var model = new OcctModelingSession();
        var p1 = Engine.EvaluateEdge(edge, 0.0).Point;
        var p2 = Engine.EvaluateEdge(edge, 1.0).Point;
        var modelEdge = model.MakeLine(p1, p2);
        var modelDim = model.MakeLengthAnnotation(
            modelEdge,
            OcctBRepAnnotationOptions.Default with
            {
                Offset = values.Number("offset", 20),
                TextHeight = values.Number("textHeight", 6),
                ArrowSize = values.Number("arrowSize", 4),
                FontName = DemoFonts.ResolveOcctFont(values.Text("font", "Microsoft YaHei"))
            });
        var dimShape = DisplayModelShape(model, modelDim);
        Engine.SetObjectColor(dimShape, Color.DarkBlue);
        SetGeneratedName(dimShape, DemoLocalization.CommandText(DemoCommandId.LengthDimension));
        ActiveObject = dimShape;
        return DemoCommandResult.Created(
            DemoLocalization.Text("Session.Created", DemoLocalization.CommandText(DemoCommandId.LengthDimension)),
            dimShape,
            edge);
    }

    private DemoCommandResult CreateAngleDimension(DemoValues values)
    {
        var first = CopySelectedSubshape(0);
        var second = CopySelectedSubshape(1);
        Engine.SetObjectVisible(first, false);
        Engine.SetObjectVisible(second, false);
        Engine.SetObjectSelectable(first, false);
        Engine.SetObjectSelectable(second, false);
        SetGeneratedName(first, Local("Angle Source A", "角度源边 A"));
        SetGeneratedName(second, Local("Angle Source B", "角度源边 B"));

        using var model = new OcctModelingSession();
        var p1Start = Engine.EvaluateEdge(first, 0.0).Point;
        var p1End = Engine.EvaluateEdge(first, 1.0).Point;
        var p2Start = Engine.EvaluateEdge(second, 0.0).Point;
        var p2End = Engine.EvaluateEdge(second, 1.0).Point;

        // Radiate outward from apex to guarantee correct angle orientation & readability
        var d00 = p1Start.DistanceTo(p2Start);
        var d01 = p1Start.DistanceTo(p2End);
        var d10 = p1End.DistanceTo(p2Start);
        var d11 = p1End.DistanceTo(p2End);
        var minD = Math.Min(Math.Min(d00, d01), Math.Min(d10, d11));
        OcctPoint3d apex, ray1End, ray2End;
        if (minD == d00) { apex = p1Start; ray1End = p1End; ray2End = p2End; }
        else if (minD == d01) { apex = p1Start; ray1End = p1End; ray2End = p2Start; }
        else if (minD == d10) { apex = p1End; ray1End = p1Start; ray2End = p2End; }
        else { apex = p1End; ray1End = p1Start; ray2End = p2Start; }

        var modelFirst = model.MakeLine(apex, ray1End);
        var modelSecond = model.MakeLine(apex, ray2End);
        var modelDim = model.MakeAngleAnnotation(
            modelFirst,
            modelSecond,
            OcctBRepAnnotationOptions.Default with
            {
                Offset = values.Number("offset", 35),
                TextHeight = values.Number("textHeight", 6),
                ArrowSize = values.Number("arrowSize", 4),
                FontName = DemoFonts.ResolveOcctFont(values.Text("font", "Microsoft YaHei"))
            });
        var dimShape = DisplayModelShape(model, modelDim);
        Engine.SetObjectColor(dimShape, Color.DarkGreen);
        SetGeneratedName(dimShape, DemoLocalization.CommandText(DemoCommandId.AngleDimension));
        ActiveObject = dimShape;
        return DemoCommandResult.Created(
            DemoLocalization.Text("Session.Created", DemoLocalization.CommandText(DemoCommandId.AngleDimension)),
            dimShape,
            first,
            second);
    }

    private DemoCommandResult CreateRadiusDimension(DemoValues values, bool diameter)
    {
        var edge = CopySelectedSubshape(0);
        Engine.SetObjectVisible(edge, false);
        Engine.SetObjectSelectable(edge, false);
        SetGeneratedName(edge, Local("Circular Source Edge", "圆尺寸源边"));

        using var model = new OcctModelingSession();
        var p0 = Engine.EvaluateEdge(edge, 0.0).Point;
        var p1 = Engine.EvaluateEdge(edge, 0.5).Point;
        var p2 = Engine.EvaluateEdge(edge, 1.0).Point;
        var modelCircle = (p0.DistanceTo(p2) < 1e-5)
            ? model.MakeCircle(new OcctPoint3d((p0.X + p1.X) / 2, (p0.Y + p1.Y) / 2, (p0.Z + p1.Z) / 2), OcctVector3d.UnitZ, p0.DistanceTo(p1) / 2.0)
            : model.MakeArc(p0, p1, p2);

        var modelDim = diameter
            ? model.MakeDiameterAnnotation(
                modelCircle,
                OcctBRepAnnotationOptions.Default with
                {
                    Offset = values.Number("offset", 20),
                    TextHeight = values.Number("textHeight", 6),
                    ArrowSize = values.Number("arrowSize", 4),
                    FontName = DemoFonts.ResolveOcctFont(values.Text("font", "Microsoft YaHei"))
                })
            : model.MakeRadiusAnnotation(
                modelCircle,
                OcctBRepAnnotationOptions.Default with
                {
                    Offset = values.Number("offset", 20),
                    TextHeight = values.Number("textHeight", 6),
                    ArrowSize = values.Number("arrowSize", 4),
                    FontName = DemoFonts.ResolveOcctFont(values.Text("font", "Microsoft YaHei"))
                });

        var dimShape = DisplayModelShape(model, modelDim);
        var commandId = diameter ? DemoCommandId.DiameterDimension : DemoCommandId.RadiusDimension;
        Engine.SetObjectColor(dimShape, diameter ? Color.Purple : Color.DarkRed);
        SetGeneratedName(dimShape, DemoLocalization.CommandText(commandId));
        ActiveObject = dimShape;
        return DemoCommandResult.Created(
            DemoLocalization.Text("Session.Created", DemoLocalization.CommandText(commandId)),
            dimShape,
            edge);
    }

    private OcctShape DisplayModelShape(OcctModelingSession model, OcctModelShape sourceShape)
    {
        ArgumentNullException.ThrowIfNull(model);
        if (!sourceShape.IsValid)
            throw new ArgumentException("Modeling shape is invalid.", nameof(sourceShape));

        var viewerShape = Engine.MakeVertex(OcctPoint3d.Origin);
        Engine.UpdateShape(viewerShape, model, sourceShape);
        return viewerShape;
    }

    private DemoCommandResult DeleteSelected()
    {
        var selected = Engine.SelectedObjects.ToList();

        var targets = selected
            .DistinctBy(item => item.Id)
            .Where(item => Engine.ContainsObject(item.Id))
            .Select(item => (IOcctObject)item)
            .ToArray();

        if (targets.Length == 0)
        {
            throw new InvalidOperationException(Local(
                "Select one or more objects to erase.",
                "请先选择要删除的对象。"));
        }

        Engine.Delete(targets);
        ActiveObject = null;
        return DemoCommandResult.Empty(DemoLocalization.Text("Session.Deleted", targets.Length));
    }
}
