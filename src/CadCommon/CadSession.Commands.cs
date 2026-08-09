using System.Drawing;
using OcctNet;

namespace CadCommon;

public sealed partial class CadSession
{
    public CadCommandResult Execute(CadCommandId commandId, IReadOnlyDictionary<string, string>? rawValues = null)
    {
        EnsureCommandAvailable(commandId);

        var storedValues = rawValues is null
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(rawValues, StringComparer.OrdinalIgnoreCase);
        var selectedObjectIds = Engine.SelectedObjects.Select(item => item.Id).Distinct().ToList();
        var values = new CadValues(storedValues);
        var isDemoCommand = IsDemoCommand(commandId);
        var displayBatch = isDemoCommand ? Engine.BeginDisplayBatch() : null;
        var demoInitialObjectIds = isDemoCommand
            ? Engine.Objects.Select(item => item.Id).ToHashSet()
            : null;
        CadCommandResult result;
        try
        {
            result = commandId switch
            {
            CadCommandId.Point => CreateShape(CadLocalization.CommandText(commandId), Engine.MakeVertex(values.Point())),
            CadCommandId.Line => CreateShape(CadLocalization.CommandText(commandId), Engine.MakeLine(new(values.Number("x1"), values.Number("y1"), values.Number("z1")), new(values.Number("x2"), values.Number("y2"), values.Number("z2")))),
            CadCommandId.Polyline => CreateShape(CadLocalization.CommandText(commandId), Engine.MakePolyline(values.Points("points"), values.Boolean("closed"))),
            CadCommandId.Circle => CreateShape(CadLocalization.CommandText(commandId), Engine.MakeCircle(values.Point(), OcctVector3d.UnitZ, values.Number("radius"))),
            CadCommandId.ArcThreePoints => CreateShape(CadLocalization.CommandText(commandId), Engine.MakeArc(new(values.Number("x1"), values.Number("y1"), values.Number("z")), new(values.Number("x2"), values.Number("y2"), values.Number("z")), new(values.Number("x3"), values.Number("y3"), values.Number("z")))),
            CadCommandId.ArcCenter => CreateShape(CadLocalization.CommandText(commandId), Engine.MakeArc(values.Point(), OcctVector3d.UnitZ, OcctVector3d.UnitX, values.Number("radius"), values.Number("start"), values.Number("end"))),
            CadCommandId.Ellipse => CreateShape(CadLocalization.CommandText(commandId), Engine.MakeEllipse(values.Point(), OcctVector3d.UnitZ, values.Number("major"), values.Number("minor"))),
            CadCommandId.Rectangle => CreateRectangle(values),
            CadCommandId.Polygon => CreateShape(CadLocalization.CommandText(commandId), Engine.MakeRegularPolygon(values.Number("radius"), values.Integer("sides"), values.Boolean("face"), values.Point())),
            CadCommandId.Bezier => CreateShape(CadLocalization.CommandText(commandId), Engine.MakeBezier(values.Points("points"))),
            CadCommandId.BSpline => CreateShape(CadLocalization.CommandText(commandId), Engine.MakeInterpolatedBSpline(values.Points("points"), values.Boolean("periodic"))),

            CadCommandId.Box => CreateShape(CadLocalization.CommandText(commandId), Engine.MakeBox(values.Number("dx"), values.Number("dy"), values.Number("dz"), values.Number("x"), values.Number("y"), values.Number("z"))),
            CadCommandId.Cylinder => CreateShape(CadLocalization.CommandText(commandId), Engine.MakeCylinder(values.Number("radius"), values.Number("height"), values.Number("x"), values.Number("y"), values.Number("z"))),
            CadCommandId.Frustum => CreateShape(CadLocalization.CommandText(commandId), Engine.MakeCone(values.Number("r1"), values.Number("r2"), values.Number("height"), values.Number("x"), values.Number("y"), values.Number("z"))),
            CadCommandId.Cone => CreateShape(CadLocalization.CommandText(commandId), Engine.MakeCone(values.Number("radius"), 0, values.Number("height"), values.Number("x"), values.Number("y"), values.Number("z"))),
            CadCommandId.Torus => CreateShape(CadLocalization.CommandText(commandId), Engine.MakeTorus(values.Number("major"), values.Number("minor"), values.Point())),
            CadCommandId.Sphere => CreateShape(CadLocalization.CommandText(commandId), Engine.MakeSphere(values.Number("radius"), values.Number("x"), values.Number("y"), values.Number("z"))),
            CadCommandId.Wedge => CreateShape(CadLocalization.CommandText(commandId), Engine.MakeWedge(values.Number("dx"), values.Number("dy"), values.Number("dz"), values.Number("ltx"))),
            CadCommandId.Pipe => CreatePipe(values),
            CadCommandId.Extrude => CreateShape(CadLocalization.CommandText(commandId), Engine.Extrude(RequireShape(), values.Vector("dx", "dy", "dz"), values.Boolean("hide", true))),
            CadCommandId.Revolve => CreateShape(CadLocalization.CommandText(commandId), Engine.Revolve(RequireShape(), values.Point("px", "py", "pz"), values.Vector("ax", "ay", "az"), values.Number("angle", 360), values.Boolean("hide", true))),
            CadCommandId.Sweep => Sweep(values),
            CadCommandId.Loft => Loft(values),
            CadCommandId.Fuse => Boolean(values, OcctBooleanOperation.Fuse, CadLocalization.CommandText(commandId)),
            CadCommandId.Cut => Boolean(values, OcctBooleanOperation.Cut, CadLocalization.CommandText(commandId)),
            CadCommandId.Common => Boolean(values, OcctBooleanOperation.Common, CadLocalization.CommandText(commandId)),
            CadCommandId.Section => Boolean(values, OcctBooleanOperation.Section, CadLocalization.CommandText(commandId)),
            CadCommandId.Fillet => CreateShape(CadLocalization.CommandText(commandId), Engine.FilletAllEdges(RequireShape(), values.Number("radius"), values.Boolean("hide", true))),
            CadCommandId.Chamfer => CreateShape(CadLocalization.CommandText(commandId), Engine.ChamferAllEdges(RequireShape(), values.Number("distance"), values.Boolean("hide", true))),
            CadCommandId.Offset => CreateShape(CadLocalization.CommandText(commandId), Engine.Offset(RequireShape(), values.Number("offset"), values.Number("tolerance", 0.0001), values.Boolean("hide", true))),
            CadCommandId.Shell => CreateShape(CadLocalization.CommandText(commandId), Engine.MakeThickSolid(RequireShape(), values.Integer("face"), values.Number("thickness"), 0.0001, values.Boolean("hide", true))),
            CadCommandId.Drill => CreateShape(CadLocalization.CommandText(commandId), Engine.DrillHole(RequireShape(), values.Point(), OcctVector3d.UnitZ, values.Number("radius"), values.Number("depth"), values.Boolean("hide", true))),

            CadCommandId.Translate => CreateShape(CadLocalization.CommandText(commandId), Engine.Translate(RequireShape(), values.Vector("dx", "dy", "dz"), values.Boolean("hide", true))),
            CadCommandId.Rotate => CreateShape(CadLocalization.CommandText(commandId), Engine.Rotate(RequireShape(), values.Point("px", "py", "pz"), values.Vector("ax", "ay", "az"), values.Number("angle"), values.Boolean("hide", true))),
            CadCommandId.Scale => CreateShape(CadLocalization.CommandText(commandId), Engine.Scale(RequireShape(), values.Point(), values.Number("factor"), values.Boolean("hide", true))),
            CadCommandId.Mirror => CreateShape(CadLocalization.CommandText(commandId), Engine.MirrorPlane(RequireShape(), values.Point(), values.Vector("nx", "ny", "nz"), values.Boolean("hide"))),
            CadCommandId.Copy => CreateShape(CadLocalization.CommandText(commandId), Engine.Copy(RequireShape(), values.Boolean("hide"))),
            CadCommandId.Delete => DeleteSelected(),

            CadCommandId.Text => CreateText(values),
            CadCommandId.LengthDimension => CreateLengthDimension(values),
            CadCommandId.AngleDimension => CreateAngleDimension(values),
            CadCommandId.RadiusDimension => CreateRadiusDimension(values, false),
            CadCommandId.DiameterDimension => CreateRadiusDimension(values, true),

            CadCommandId.AnalyzeBounds => AnalyzeBounds(),
            CadCommandId.AnalyzeMass => AnalyzeMass(),
            CadCommandId.AnalyzeTopology => AnalyzeTopology(),
            CadCommandId.AnalyzeDistance => AnalyzeDistance(),
            CadCommandId.ValidateShape => ValidateShape(),

            CadCommandId.DemoPrimitives => DemoPrimitives(),
            CadCommandId.DemoBracket => DemoBracket(),
            CadCommandId.DemoFlange => DemoFlange(),
            CadCommandId.DemoPipe => DemoPipe(),
            CadCommandId.DemoTee => DemoTee(),
            CadCommandId.DemoReducer => DemoReducer(),
            CadCommandId.DemoLoft => DemoLoft(),
            CadCommandId.DemoBoolean => DemoBoolean(),
            CadCommandId.DemoElements => DemoElements(),
            CadCommandId.DemoGear => DemoGear(),
            CadCommandId.DemoManifold => DemoManifold(),
            CadCommandId.DemoTwistedDuct => DemoTwistedDuct(),
            CadCommandId.DemoAnnotations => DemoAnnotations(),
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

        var changed = result.CreatedObjects.Count > 0 || commandId == CadCommandId.Delete;
        if (changed)
        {
            IsModified = true;
            if (!_restoringHistory)
            {
                if (IsUndoableCommand(commandId) && _historyAvailable)
                {
                    TruncateRedoHistory();
                    _history.Add(CadHistoryEntry.Command(commandId, storedValues, selectedObjectIds, CadLocalization.CommandText(commandId)));
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

    private CadCommandResult CreateRectangle(CadValues values)
    {
        var wire = Engine.MakeRectangleWire(values.Number("width"), values.Number("height"), values.Point());
        if (!values.Boolean("face")) return CreateShape(CadLocalization.CommandText(CadCommandId.Rectangle), wire);
        Engine.SetVisible(wire, false);
        return CreateShape(Local("Rectangle Face", "矩形面"), Engine.MakeFace(wire));
    }

    private CadCommandResult CreatePipe(CadValues values)
    {
        var outer = Engine.MakeCylinder(values.Number("outer"), values.Number("height"), values.Number("x"), values.Number("y"), values.Number("z"));
        var innerRadius = values.Number("outer") - values.Number("wall");
        if (innerRadius <= 0) throw new InvalidOperationException(Local("Wall thickness must be smaller than the outer radius.", "壁厚必须小于外半径。"));
        var inner = Engine.MakeCylinder(innerRadius, values.Number("height") + 2, values.Number("x"), values.Number("y"), values.Number("z") - 1);
        var pipe = Engine.Cut(outer, inner, true);
        return CreateShape(CadLocalization.CommandText(CadCommandId.Pipe), pipe);
    }

    private CadCommandResult Sweep(CadValues values)
    {
        var shapes = RequireShapes(2);
        return CreateShape(CadLocalization.CommandText(CadCommandId.Sweep), Engine.Sweep(shapes[0], shapes[1], values.Boolean("hide", true)));
    }

    private CadCommandResult Loft(CadValues values)
    {
        var shapes = RequireShapes(2);
        return CreateShape(CadLocalization.CommandText(CadCommandId.Loft), Engine.Loft(shapes, values.Boolean("solid", true), values.Boolean("ruled"), hideInputs: values.Boolean("hide", true)));
    }

    private CadCommandResult Boolean(CadValues values, OcctBooleanOperation operation, string name)
    {
        var shapes = RequireShapes(2);
        return CreateShape(name, Engine.Boolean(operation, shapes[0], shapes[1], values.Boolean("hide", operation != OcctBooleanOperation.Section)));
    }

    private CadCommandResult CreateText(CadValues values)
    {
        var text = Engine.MakeTextShape(
            values.Text("text", "OCCT CAD"),
            values.Point(),
            values.Number("height", 18),
            values.Number("depth", 0),
            values.Text("font", "Microsoft YaHei UI"),
            bold: values.Boolean("bold"),
            italic: values.Boolean("italic"));
        Engine.SetColor(text, Color.DarkSlateGray);
        SetGeneratedName(text, CadLocalization.CommandText(CadCommandId.Text));
        ActiveObject = text;
        return CadCommandResult.Created(CadLocalization.Text("Session.Created", CadLocalization.CommandText(CadCommandId.Text)), text);
    }

    private CadCommandResult CreateLengthDimension(CadValues values)
    {
        var edge = CopySelectedSubshape(0);
        try
        {
            var dimension = Engine.MakeLengthAnnotationShape(
                edge,
                values.Number("flyout", 20),
                values.Number("textHeight", 8),
                values.Number("arrowSize", 5),
                values.Text("font", "Microsoft YaHei UI"));
            SetGeneratedName(dimension, CadLocalization.CommandText(CadCommandId.LengthDimension));
            ActiveObject = dimension;
            return CadCommandResult.Created(CadLocalization.Text("Session.Created", CadLocalization.CommandText(CadCommandId.LengthDimension)), dimension);
        }
        finally
        {
            if (Engine.Exists(edge)) Engine.Delete(edge);
        }
    }

    private CadCommandResult CreateAngleDimension(CadValues values)
    {
        var first = CopySelectedSubshape(0);
        var second = CopySelectedSubshape(1);
        try
        {
            var dimension = Engine.MakeAngleAnnotationShape(
                first,
                second,
                values.Number("flyout", 30),
                values.Number("textHeight", 8),
                values.Number("arrowSize", 5),
                values.Text("font", "Microsoft YaHei UI"));
            SetGeneratedName(dimension, CadLocalization.CommandText(CadCommandId.AngleDimension));
            ActiveObject = dimension;
            return CadCommandResult.Created(CadLocalization.Text("Session.Created", CadLocalization.CommandText(CadCommandId.AngleDimension)), dimension);
        }
        finally
        {
            if (Engine.Exists(first)) Engine.Delete(first);
            if (Engine.Exists(second)) Engine.Delete(second);
        }
    }

    private CadCommandResult CreateRadiusDimension(CadValues values, bool diameter)
    {
        var edge = CopySelectedSubshape(0);
        try
        {
            var dimension = diameter
                ? Engine.MakeDiameterAnnotationShape(
                    edge,
                    values.Number("flyout", 20),
                    values.Number("textHeight", 8),
                    values.Number("arrowSize", 5),
                    values.Text("font", "Microsoft YaHei UI"))
                : Engine.MakeRadiusAnnotationShape(
                    edge,
                    values.Number("flyout", 20),
                    values.Number("textHeight", 8),
                    values.Number("arrowSize", 5),
                    values.Text("font", "Microsoft YaHei UI"));
            var commandId = diameter ? CadCommandId.DiameterDimension : CadCommandId.RadiusDimension;
            SetGeneratedName(dimension, CadLocalization.CommandText(commandId));
            ActiveObject = dimension;
            return CadCommandResult.Created(CadLocalization.Text("Session.Created", CadLocalization.CommandText(commandId)), dimension);
        }
        finally
        {
            if (Engine.Exists(edge)) Engine.Delete(edge);
        }
    }

    private CadCommandResult DeleteSelected()
    {
        var selected = Engine.SelectedObjects.ToList();

        var targets = selected
            .DistinctBy(item => item.Id)
            .Where(item => Engine.Exists(item))
            .Select(item => (IOcctObject)item)
            .ToArray();

        if (targets.Length == 0)
        {
            throw new InvalidOperationException(Local(
                "Select one or more objects to erase.",
                "请先选择要删除的对象。"));
        }

        // One managed call, one P/Invoke transition, one native validation pass and one redraw.
        Engine.Delete(targets);
        ActiveObject = null;
        return CadCommandResult.Empty(CadLocalization.Text("Session.Deleted", targets.Length));
    }

}
