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
        var selectedObjectIds = Engine.SelectedObjects.Select(item => item.Id).Distinct().ToList();
        var values = new DemoValues(storedValues);
        var isDemoCommand = IsDemoCommand(commandId);
        var displayBatch = isDemoCommand ? Engine.BeginDisplayBatch() : null;
        var demoInitialObjectIds = isDemoCommand
            ? Engine.GetObjects().Select(item => item.Id).ToHashSet()
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
            DemoCommandId.DemoLinearCopies => DemoLinearCopies(),
            DemoCommandId.DemoRadialCopies => DemoRadialCopies(),
            DemoCommandId.DemoMirrorCopies => DemoMirrorCopies(),
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
            values.Text("text", "OCCT CAD"),
            OcctBRepTextOptions.Default with
            {
                Position = values.Point(),
                Height = values.Number("height", 18),
                ExtrusionDepth = values.Number("depth", 0),
                FontName = DemoFonts.ResolveOcctFont(values.Text("font", DemoFonts.OcctSansSerif)),
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
        SetGeneratedName(edge, Local("Linear Dimension Source", "线性尺寸源边"));

        var dimension = Engine.AddLengthDimension(edge, values.Number("flyout", 20));
        SetGeneratedName(dimension, DemoLocalization.CommandText(DemoCommandId.LengthDimension));
        ActiveObject = dimension;
        return DemoCommandResult.Created(
            DemoLocalization.Text("Session.Created", DemoLocalization.CommandText(DemoCommandId.LengthDimension)),
            dimension,
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
        SetGeneratedName(first, Local("Angle Dimension Source A", "角度尺寸源边 A"));
        SetGeneratedName(second, Local("Angle Dimension Source B", "角度尺寸源边 B"));

        var dimension = Engine.AddAngleDimension(first, second, values.Number("flyout", 30));
        SetGeneratedName(dimension, DemoLocalization.CommandText(DemoCommandId.AngleDimension));
        ActiveObject = dimension;
        return DemoCommandResult.Created(
            DemoLocalization.Text("Session.Created", DemoLocalization.CommandText(DemoCommandId.AngleDimension)),
            dimension,
            first,
            second);
    }

    private DemoCommandResult CreateRadiusDimension(DemoValues values, bool diameter)
    {
        var edge = CopySelectedSubshape(0);
        Engine.SetObjectVisible(edge, false);
        Engine.SetObjectSelectable(edge, false);
        SetGeneratedName(edge, Local("Circular Dimension Source", "圆尺寸源边"));

        var dimension = diameter
            ? Engine.AddDiameterDimension(edge, values.Number("flyout", 20))
            : Engine.AddRadiusDimension(edge, values.Number("flyout", 20));
        var commandId = diameter ? DemoCommandId.DiameterDimension : DemoCommandId.RadiusDimension;
        SetGeneratedName(dimension, DemoLocalization.CommandText(commandId));
        ActiveObject = dimension;
        return DemoCommandResult.Created(
            DemoLocalization.Text("Session.Created", DemoLocalization.CommandText(commandId)),
            dimension,
            edge);
    }

    private OcctShape DisplayModelShape(OcctModelingSession model, OcctModelShape sourceShape)
    {
        ArgumentNullException.ThrowIfNull(model);
        if (!sourceShape.IsValid)
            throw new ArgumentException("Modeling shape is invalid.", nameof(sourceShape));

        return Engine.CreateShapeFromModel(model, sourceShape);
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
