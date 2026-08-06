using System.Drawing;
using System.Globalization;
using OcctNet;

namespace CadCommon;

public sealed class CadValues
{
    private readonly IReadOnlyDictionary<string, string> _values;

    public CadValues(IReadOnlyDictionary<string, string>? values = null)
    {
        _values = values ?? new Dictionary<string, string>();
    }

    public string Text(string key, string fallback = "") => _values.TryGetValue(key, out var value) ? value : fallback;

    public double Number(string key, double fallback = 0)
    {
        if (!_values.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value)) return fallback;
        if (double.TryParse(value, NumberStyles.Float, CultureInfo.CurrentCulture, out var current)) return current;
        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var invariant)) return invariant;
        throw new FormatException(CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? $"参数“{key}”不是有效数值：{value}" : $"Parameter '{key}' is not a valid number: {value}");
    }

    public int Integer(string key, int fallback = 0)
    {
        if (!_values.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value)) return fallback;
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.CurrentCulture, out var current)) return current;
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var invariant)) return invariant;
        throw new FormatException(CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? $"参数“{key}”不是有效整数：{value}" : $"Parameter '{key}' is not a valid integer: {value}");
    }

    public bool Boolean(string key, bool fallback = false)
    {
        if (!_values.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value)) return fallback;
        return value.Trim().ToLowerInvariant() switch
        {
            "true" or "1" or "yes" or "是" or "开启" => true,
            "false" or "0" or "no" or "否" or "关闭" => false,
            _ => throw new FormatException(CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? $"参数“{key}”不是有效布尔值：{value}" : $"Parameter '{key}' is not a valid Boolean value: {value}")
        };
    }

    public OcctPoint3d Point(string x = "x", string y = "y", string z = "z") => new(Number(x), Number(y), Number(z));
    public OcctVector3d Vector(string x, string y, string z) => new(Number(x), Number(y), Number(z));

    public IReadOnlyList<OcctPoint3d> Points(string key)
    {
        var text = Text(key);
        var result = new List<OcctPoint3d>();
        foreach (var pointText in text.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var components = pointText.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (components.Length is < 2 or > 3) throw new FormatException(CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? $"坐标点格式错误：{pointText}" : $"Invalid coordinate point: {pointText}");
            var x = ParseNumber(components[0]);
            var y = ParseNumber(components[1]);
            var z = components.Length == 3 ? ParseNumber(components[2]) : 0;
            result.Add(new OcctPoint3d(x, y, z));
        }
        if (result.Count == 0) throw new FormatException(CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? "至少需要一个坐标点。" : "At least one coordinate point is required.");
        return result;
    }

    private static double ParseNumber(string value)
    {
        if (double.TryParse(value, NumberStyles.Float, CultureInfo.CurrentCulture, out var current)) return current;
        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var invariant)) return invariant;
        throw new FormatException(CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? $"坐标值不是有效数值：{value}" : $"Coordinate value is not numeric: {value}");
    }
}

public enum CadIsoView { NorthEast, NorthWest, SouthEast, SouthWest }
public enum CadDepthBiasPreset { Forward, Backward, Default }

public sealed class CadSession
{
    private int _nameSequence = 1;
    private readonly List<CadHistoryEntry> _history = new();
    private int _historyPosition;
    private bool _restoringHistory;
    private bool _suppressNotifications;
    private bool _historyAvailable = true;
    private string? _historyBaselineFile;

    public CadSession(OcctEngine engine)
    {
        Engine = engine ?? throw new ArgumentNullException(nameof(engine));
    }

    public OcctEngine Engine { get; }
    public OcctObject? ActiveObject { get; set; }
    public string? CurrentFilePath { get; private set; }
    public bool IsModified { get; private set; }
    public bool CanUndo => _historyAvailable && _historyPosition > 0;
    public bool CanRedo => _historyAvailable && _historyPosition < _history.Count;
    public string? UndoDescription => CanUndo ? DescribeHistoryEntry(_history[_historyPosition - 1]) : null;
    public string? RedoDescription => CanRedo ? DescribeHistoryEntry(_history[_historyPosition]) : null;

    public event EventHandler? ModelChanged;
    public event EventHandler? HistoryChanged;
    public event EventHandler<string>? StatusChanged;

    public int ApplyDepthBiasToSelection(CadDepthBiasPreset preset)
    {
        var targets = Engine.SelectedObjects
            .Where(value => value.Kind == OcctObjectKind.Shape)
            .DistinctBy(value => value.Id)
            .ToList();

        if (targets.Count == 0
            && ActiveObject is { Kind: OcctObjectKind.Shape } active
            && Engine.Exists(active))
        {
            targets.Add(active);
        }

        if (targets.Count == 0) return 0;

        using (Engine.BeginDisplayBatch())
        {
            foreach (var target in targets)
            {
                switch (preset)
                {
                    case CadDepthBiasPreset.Forward:
                        Engine.SetPolygonOffsets(
                            target,
                            OcctPolygonOffsetMode.Fill,
                            factor: -1.0,
                            units: -1.0);
                        break;
                    case CadDepthBiasPreset.Backward:
                        Engine.SetPolygonOffsets(
                            target,
                            OcctPolygonOffsetMode.Fill,
                            factor: 3.0,
                            units: 3.0);
                        break;
                    default:
                        Engine.ResetPolygonOffsets(target);
                        break;
                }
            }
        }

        return targets.Count;
    }

    public CadCommandResult Execute(CadCommandId commandId, IReadOnlyDictionary<string, string>? rawValues = null)
    {
        var storedValues = rawValues is null
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(rawValues, StringComparer.OrdinalIgnoreCase);
        var selectedObjectIds = Engine.SelectedObjects.Select(item => item.Id).Distinct().ToList();
        if (selectedObjectIds.Count == 0 && ActiveObject is { } active && Engine.Exists(active))
        {
            selectedObjectIds.Add(active.Id);
        }
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

    public void Undo()
    {
        if (!CanUndo)
        {
            StatusChanged?.Invoke(this, CadLocalization.Text("History.NothingToUndo"));
            return;
        }
        var description = DescribeHistoryEntry(_history[_historyPosition - 1]);
        _historyPosition--;
        RebuildFromHistory();
        StatusChanged?.Invoke(this, CadLocalization.Text("History.Undone", description));
        HistoryChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Redo()
    {
        if (!CanRedo)
        {
            StatusChanged?.Invoke(this, CadLocalization.Text("History.NothingToRedo"));
            return;
        }
        var description = DescribeHistoryEntry(_history[_historyPosition]);
        _historyPosition++;
        RebuildFromHistory();
        StatusChanged?.Invoke(this, CadLocalization.Text("History.Redone", description));
        HistoryChanged?.Invoke(this, EventArgs.Empty);
    }

    private void RebuildFromHistory()
    {
        _restoringHistory = true;
        _suppressNotifications = true;
        try
        {
            Engine.Clear();
            ActiveObject = null;
            _nameSequence = 1;

            if (!string.IsNullOrWhiteSpace(_historyBaselineFile))
            {
                ImportCore(_historyBaselineFile);
            }

            for (var index = 0; index < _historyPosition; index++)
            {
                var entry = _history[index];
                if (entry.IsImport)
                {
                    ImportCore(entry.ImportFilePath!);
                    continue;
                }

                if (entry.CommandId is not { } commandId) continue;
                Engine.ClearSelection();
                var append = false;
                foreach (var objectId in entry.SelectedObjectIds)
                {
                    var value = new OcctObject(objectId, Engine.GetObjectKind(objectId));
                    if (!Engine.Exists(value)) continue;
                    Engine.SelectObject(value, append);
                    append = true;
                }
                Execute(commandId, entry.Values);
            }
            Engine.ClearSelection();
            if (Engine.ShapeCount > 0) Engine.FitAll();
            IsModified = _historyPosition > 0;
        }
        finally
        {
            _suppressNotifications = false;
            _restoringHistory = false;
        }
        ModelChanged?.Invoke(this, EventArgs.Empty);
    }

    private static string DescribeHistoryEntry(CadHistoryEntry entry)
    {
        if (entry.CommandId is { } commandId)
        {
            return CadLocalization.CommandText(commandId);
        }

        if (entry.IsImport)
        {
            var fileName = Path.GetFileName(entry.ImportFilePath);
            return CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified
                ? $"导入 {fileName}"
                : $"Import {fileName}";
        }

        return entry.Description;
    }

    private void TruncateRedoHistory()
    {
        if (_historyPosition < _history.Count)
        {
            _history.RemoveRange(_historyPosition, _history.Count - _historyPosition);
        }
    }

    private void ClearHistory()
    {
        _history.Clear();
        _historyPosition = 0;
        HistoryChanged?.Invoke(this, EventArgs.Empty);
    }

    private static bool IsUndoableCommand(CadCommandId commandId) => commandId is not
        (CadCommandId.AnalyzeBounds or CadCommandId.AnalyzeMass or CadCommandId.AnalyzeTopology or
         CadCommandId.AnalyzeDistance or CadCommandId.ValidateShape or
         CadCommandId.LengthDimension or CadCommandId.AngleDimension or
         CadCommandId.RadiusDimension or CadCommandId.DiameterDimension);

    public void NewDocument()
    {
        Engine.Clear();
        ActiveObject = null;
        CurrentFilePath = null;
        _historyBaselineFile = null;
        _historyAvailable = true;
        IsModified = false;
        _nameSequence = 1;
        ClearHistory();
        ModelChanged?.Invoke(this, EventArgs.Empty);
        StatusChanged?.Invoke(this, CadLocalization.Text("Session.New"));
    }

    public OcctShape Open(string filePath)
    {
        Engine.Clear();
        ActiveObject = null;
        _nameSequence = 1;
        CurrentFilePath = Path.GetFullPath(filePath);
        _historyBaselineFile = CurrentFilePath;
        _historyAvailable = true;
        ClearHistory();
        var shape = ImportCore(CurrentFilePath);
        IsModified = false;
        Engine.FitAll();
        ModelChanged?.Invoke(this, EventArgs.Empty);
        StatusChanged?.Invoke(this, CadLocalization.Text("Session.Open", CurrentFilePath));
        return shape;
    }

    public OcctShape Import(string filePath)
    {
        var fullPath = Path.GetFullPath(filePath);
        var shape = ImportCore(fullPath);
        IsModified = true;
        if (!_restoringHistory && _historyAvailable)
        {
            TruncateRedoHistory();
            _history.Add(CadHistoryEntry.Import(fullPath,
                CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified
                    ? $"导入 {Path.GetFileName(fullPath)}"
                    : $"Import {Path.GetFileName(fullPath)}"));
            _historyPosition = _history.Count;
            HistoryChanged?.Invoke(this, EventArgs.Empty);
        }
        Engine.FitAll();
        if (!_suppressNotifications)
        {
            ModelChanged?.Invoke(this, EventArgs.Empty);
            StatusChanged?.Invoke(this, CadLocalization.Text("Session.Import", fullPath));
        }
        return shape;
    }

    private OcctShape ImportCore(string filePath)
    {
        var shape = Engine.Import(filePath);
        SetGeneratedName(shape, Path.GetFileNameWithoutExtension(filePath));
        ActiveObject = new OcctObject(shape.Id, OcctObjectKind.Shape);
        return shape;
    }

    public void SaveAll(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        switch (extension)
        {
            case ".step": case ".stp": Engine.ExportAllStep(filePath); break;
            case ".iges": case ".igs": Engine.ExportAllIges(filePath); break;
            case ".brep": ExportSingleOrCompound(filePath, (shape, path) => Engine.ExportBrep(shape, path)); break;
            case ".stl": ExportSingleOrCompound(filePath, (shape, path) => Engine.ExportStl(shape, path)); break;
            default: throw new NotSupportedException(CadLocalization.Text("Session.UnsupportedSave"));
        }
        CurrentFilePath = Path.GetFullPath(filePath);
        IsModified = false;
        StatusChanged?.Invoke(this, CadLocalization.Text("Session.Save", CurrentFilePath));
    }

    public void ExportSelected(string filePath)
    {
        var shape = RequireShape();
        switch (Path.GetExtension(filePath).ToLowerInvariant())
        {
            case ".step": case ".stp": Engine.ExportStep(shape, filePath); break;
            case ".iges": case ".igs": Engine.ExportIges(shape, filePath); break;
            case ".brep": Engine.ExportBrep(shape, filePath); break;
            case ".stl": Engine.ExportStl(shape, filePath); break;
            default: throw new NotSupportedException(CadLocalization.Text("Session.UnsupportedExport"));
        }
        StatusChanged?.Invoke(this, CadLocalization.Text("Session.Export", filePath));
    }

    public void SetIsoView(CadIsoView view)
    {
        var center = GetSceneCenter();
        var diagonal = GetSceneDiagonal();
        var eyeVector = (view switch
        {
            CadIsoView.NorthEast => new OcctVector3d(1, -1, 1),
            CadIsoView.NorthWest => new OcctVector3d(-1, -1, 1),
            CadIsoView.SouthEast => new OcctVector3d(1, 1, 1),
            CadIsoView.SouthWest => new OcctVector3d(-1, 1, 1),
            _ => new OcctVector3d(1, -1, 1)
        }).Normalized();
        var distance = Math.Max(diagonal * 2.5, 100);
        var state = Engine.GetCamera();
        state.Center = center;
        state.Eye = center + eyeVector * distance;
        state.Up = OcctVector3d.UnitZ;
        state.Scale = Math.Max(state.Scale, 1);
        Engine.SetCamera(state);
        Engine.FitAll();
    }

    public IReadOnlyList<KeyValuePair<string, string>> DescribeObject(OcctObject value)
    {
        var rows = new List<KeyValuePair<string, string>>
        {
            new(CadLocalization.Text("Object.Id"), value.Id.ToString(CultureInfo.InvariantCulture)),
            new(CadLocalization.Text("Object.Name"), SafeName(value)),
            new(CadLocalization.Text("Object.Kind"), CadLocalization.ObjectKind(value.Kind))
        };
        if (value.Kind != OcctObjectKind.Shape) return rows;
        var shape = new OcctShape(value.Id);
        rows.Add(new(CadLocalization.Text("Object.Topology"), CadLocalization.ShapeType(Engine.GetShapeType(shape))));
        rows.Add(new(CadLocalization.Text("Object.Validity"), Engine.IsValid(shape) ? CadLocalization.Text("Object.Valid") : CadLocalization.Text("Object.Invalid")));
        var bounds = Engine.GetBounds(shape);
        rows.Add(new(CadLocalization.Text("Object.SizeX"), bounds.SizeX.ToString("G8")));
        rows.Add(new(CadLocalization.Text("Object.SizeY"), bounds.SizeY.ToString("G8")));
        rows.Add(new(CadLocalization.Text("Object.SizeZ"), bounds.SizeZ.ToString("G8")));
        rows.Add(new(CadLocalization.Text("Object.Center"), bounds.Center.ToString()));
        rows.Add(new(CadLocalization.Text("Object.Vertices"), Engine.GetTopologyCount(shape, OcctShapeType.Vertex).ToString()));
        rows.Add(new(CadLocalization.Text("Object.Edges"), Engine.GetTopologyCount(shape, OcctShapeType.Edge).ToString()));
        rows.Add(new(CadLocalization.Text("Object.Faces"), Engine.GetTopologyCount(shape, OcctShapeType.Face).ToString()));
        return rows;
    }

    public string SafeName(IOcctObject value)
    {
        var name = Engine.GetName(value);
        return string.IsNullOrWhiteSpace(name) ? $"{CadLocalization.ObjectKind(Engine.GetObjectKind(value.Id))} {value.Id}" : name;
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
        ActiveObject = new OcctObject(text.Id, OcctObjectKind.Shape);
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
            ActiveObject = new OcctObject(dimension.Id, OcctObjectKind.Shape);
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
            ActiveObject = new OcctObject(dimension.Id, OcctObjectKind.Shape);
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
            ActiveObject = new OcctObject(dimension.Id, OcctObjectKind.Shape);
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
        if (selected.Count == 0 && ActiveObject is { } active) selected.Add(active);

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

    private CadCommandResult AnalyzeBounds()
    {
        var bounds = Engine.GetBounds(RequireShape());
        var text = CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified
            ? $"最小点：({bounds.MinX:G8}, {bounds.MinY:G8}, {bounds.MinZ:G8})\n最大点：({bounds.MaxX:G8}, {bounds.MaxY:G8}, {bounds.MaxZ:G8})\n尺寸：{bounds.SizeX:G8} × {bounds.SizeY:G8} × {bounds.SizeZ:G8}\n中心：{bounds.Center}"
            : $"Minimum: ({bounds.MinX:G8}, {bounds.MinY:G8}, {bounds.MinZ:G8})\nMaximum: ({bounds.MaxX:G8}, {bounds.MaxY:G8}, {bounds.MaxZ:G8})\nSize: {bounds.SizeX:G8} × {bounds.SizeY:G8} × {bounds.SizeZ:G8}\nCenter: {bounds.Center}";
        return new(Local("Extents analysis completed.", "包围盒分析完成。"), Array.Empty<IOcctObject>(), text);
    }

    private CadCommandResult AnalyzeMass()
    {
        var shape = RequireShape();
        var linear = Engine.GetLinearProperties(shape);
        var surface = Engine.GetSurfaceProperties(shape);
        var volume = Engine.GetVolumeProperties(shape);
        var text = CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified
            ? $"总长度：{linear.Mass:G10}\n总面积：{surface.Mass:G10}\n总体积：{volume.Mass:G10}\n体积重心：{volume.CenterOfMass}"
            : $"Total Length: {linear.Mass:G10}\nSurface Area: {surface.Mass:G10}\nVolume: {volume.Mass:G10}\nCentroid: {volume.CenterOfMass}";
        return new(Local("Mass properties completed.", "几何属性分析完成。"), Array.Empty<IOcctObject>(), text);
    }

    private CadCommandResult AnalyzeTopology()
    {
        var shape = RequireShape();
        var types = new[] { OcctShapeType.Vertex, OcctShapeType.Edge, OcctShapeType.Wire, OcctShapeType.Face, OcctShapeType.Shell, OcctShapeType.Solid };
        var text = string.Join(Environment.NewLine, types.Select(type => $"{type}: {Engine.GetTopologyCount(shape, type)}"));
        return new(Local("Topology statistics completed.", "拓扑统计完成。"), Array.Empty<IOcctObject>(), text);
    }

    private CadCommandResult AnalyzeDistance()
    {
        var shapes = RequireShapes(2);
        var result = Engine.Distance(shapes[0], shapes[1]);
        var text = CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified
            ? $"最短距离：{result.Distance:G10}\n对象 1 最近点：{result.PointOnFirst}\n对象 2 最近点：{result.PointOnSecond}"
            : $"Minimum Distance: {result.Distance:G10}\nClosest Point on Object 1: {result.PointOnFirst}\nClosest Point on Object 2: {result.PointOnSecond}";
        return new(Local("Minimum distance calculation completed.", "最短距离计算完成。"), Array.Empty<IOcctObject>(), text);
    }

    private CadCommandResult ValidateShape()
    {
        var valid = Engine.IsValid(RequireShape());
        return new(valid ? Local("Shape validation passed.", "形体检查通过。") : Local("Shape validation failed.", "形体检查未通过。"), Array.Empty<IOcctObject>(), valid ? Local("The shape passed BRepCheck validation.", "当前形体通过 BRepCheck 检查。") : Local("The shape contains invalid topology or geometry.", "当前形体存在无效拓扑或几何。"));
    }

    private CadCommandResult DemoPrimitives()
    {
        var objects = new List<IOcctObject>();
        objects.Add(Name(Engine.MakeBox(80, 60, 45, -220, -80, 0), Local("Box", "长方体")));
        objects.Add(Name(Engine.MakeCylinder(35, 80, -90, -50, 0), Local("Cylinder", "圆柱")));
        objects.Add(Name(Engine.MakeCone(45, 22, 80, 20, -50, 0), Local("Conical Frustum", "圆台")));
        objects.Add(Name(Engine.MakeCone(45, 0, 80, 130, -50, 0), Local("Cone", "圆锥")));
        objects.Add(Name(Engine.MakeSphere(42, -160, 90, 42), Local("Sphere", "球")));
        objects.Add(Name(Engine.MakeTorus(45, 12, new(-30, 90, 35)), Local("Torus", "圆环")));
        var wedge = Engine.MakeWedge(90, 65, 55, 35);
        var movedWedge = Engine.Translate(wedge, new OcctVector3d(110, 70, 0), true);
        objects.Add(Name(movedWedge, Local("Wedge", "楔体")));
        Engine.FitAll();
        return new(Local("Primitive gallery created.", "已生成基本体陈列。"), objects);
    }

    private CadCommandResult DemoBracket()
    {
        var baseBody = Engine.MakeBox(180, 100, 20, -90, -50, 0);
        var upright = Engine.MakeBox(20, 100, 100, -90, -50, 20);
        var body = Engine.Fuse(baseBody, upright, true);
        body = Engine.DrillHole(body, new(-55, 0, -1), OcctVector3d.UnitZ, 12, 25, true);
        body = Engine.DrillHole(body, new(55, 0, -1), OcctVector3d.UnitZ, 12, 25, true);
        body = Engine.DrillHole(body, new(-91, 0, 70), OcctVector3d.UnitX, 18, 25, true);
        try { body = Engine.FilletAllEdges(body, 3, true); } catch (OcctException) { }
        SetGeneratedName(body, Local("Mechanical Bracket", "机械支架"));
        Engine.SetMaterial(body, OcctMaterial.Steel);
        Engine.FitAll();
        return CadCommandResult.Created(Local("Mechanical bracket sample created.", "已生成机械支架示例。"), body);
    }

    private CadCommandResult DemoFlange()
    {
        var body = Engine.MakeCylinder(80, 18, 0, 0, 0);
        body = Engine.DrillHole(body, new OcctPoint3d(0, 0, -1), OcctVector3d.UnitZ, 34, 20, true);
        const double pitchRadius = 58;
        for (var index = 0; index < 8; index++)
        {
            var angle = index * Math.PI / 4.0;
            var center = new OcctPoint3d(Math.Cos(angle) * pitchRadius, Math.Sin(angle) * pitchRadius, -1);
            body = Engine.DrillHole(body, center, OcctVector3d.UnitZ, 6.5, 20, true);
        }
        try { body = Engine.FilletAllEdges(body, 1.5, true); } catch (OcctException) { }
        SetGeneratedName(body, Local("Eight-Hole Flange", "八孔法兰"));
        Engine.SetMaterial(body, OcctMaterial.Steel);
        Engine.FitAll();
        return CadCommandResult.Created(Local("Eight-hole flange sample created.", "已生成八孔法兰示例。"), body);
    }

    private CadCommandResult DemoTee()
    {
        var outerMain = Engine.MakeCylinder(new OcctPoint3d(-100, 0, 0), OcctVector3d.UnitX, 30, 200);
        var outerBranch = Engine.MakeCylinder(new OcctPoint3d(0, 0, 0), OcctVector3d.UnitZ, 25, 110);
        var outer = Engine.Fuse(outerMain, outerBranch, true);

        var innerMain = Engine.MakeCylinder(new OcctPoint3d(-101, 0, 0), OcctVector3d.UnitX, 24, 202);
        var innerBranch = Engine.MakeCylinder(new OcctPoint3d(0, 0, -1), OcctVector3d.UnitZ, 19, 112);
        var inner = Engine.Fuse(innerMain, innerBranch, true);
        var tee = Engine.Cut(outer, inner, true);
        SetGeneratedName(tee, Local("Hollow Pipe Tee", "空心管道三通"));
        Engine.SetMaterial(tee, OcctMaterial.Steel);
        Engine.FitAll();
        return CadCommandResult.Created(Local("Hollow pipe tee sample created.", "已生成空心管道三通示例。"), tee);
    }

    private CadCommandResult DemoReducer()
    {
        static OcctShape CircleWire(OcctEngine engine, double radius, double z)
        {
            var edge = engine.MakeCircle(new OcctPoint3d(0, 0, z), OcctVector3d.UnitZ, radius);
            return engine.MakeWire(new[] { edge }, true);
        }

        var outer = Engine.Loft(new[] { CircleWire(Engine, 50, 0), CircleWire(Engine, 42, 60), CircleWire(Engine, 30, 130) }, true, false, hideInputs: true);
        var inner = Engine.Loft(new[] { CircleWire(Engine, 44, -1), CircleWire(Engine, 36, 60), CircleWire(Engine, 24, 131) }, true, false, hideInputs: true);
        var reducer = Engine.Cut(outer, inner, true);
        SetGeneratedName(reducer, Local("Hollow Reducer", "空心异径管"));
        Engine.SetMaterial(reducer, OcctMaterial.Copper);
        Engine.FitAll();
        return CadCommandResult.Created(Local("Hollow reducer sample created.", "已生成空心异径管示例。"), reducer);
    }

    private CadCommandResult DemoPipe()
    {
        var spineEdge = Engine.MakeInterpolatedBSpline(new[] { new OcctPoint3d(0,0,0), new OcctPoint3d(70,0,20), new OcctPoint3d(110,50,70), new OcctPoint3d(150,100,80) });
        var spine = Engine.MakeWire(new[] { spineEdge }, true);
        var profileEdge = Engine.MakeCircle(OcctPoint3d.Origin, OcctVector3d.UnitX, 12);
        var profile = Engine.MakeWire(new[] { profileEdge }, true);
        var pipe = Engine.Sweep(spine, profile, true);
        SetGeneratedName(pipe, Local("Swept Pipe", "扫掠弯管"));
        Engine.SetMaterial(pipe, OcctMaterial.Copper);
        Engine.FitAll();
        return CadCommandResult.Created(Local("Swept pipe sample created.", "已生成扫掠弯管示例。"), pipe);
    }

    private CadCommandResult DemoLoft()
    {
        var sections = new List<OcctShape>();
        foreach (var item in new[] { (Z: 0d, R: 55d), (Z: 70d, R: 38d), (Z: 140d, R: 48d) })
        {
            var edge = Engine.MakeCircle(new(0,0,item.Z), OcctVector3d.UnitZ, item.R);
            sections.Add(Engine.MakeWire(new[] { edge }, true));
        }
        var loft = Engine.Loft(sections, true, false, hideInputs: true);
        SetGeneratedName(loft, Local("Lofted Body", "放样壳体"));
        Engine.SetMaterial(loft, OcctMaterial.Aluminum);
        Engine.FitAll();
        return CadCommandResult.Created(Local("Lofted body sample created.", "已生成放样壳体示例。"), loft);
    }

    private CadCommandResult DemoBoolean()
    {
        var results = new List<IOcctObject>();
        results.Add(Name(BooleanPair(OcctBooleanOperation.Fuse, -180), Local("Boolean Union", "布尔并集")));
        results.Add(Name(BooleanPair(OcctBooleanOperation.Cut, -60), Local("Boolean Subtract", "布尔差集")));
        results.Add(Name(BooleanPair(OcctBooleanOperation.Common, 60), Local("Boolean Intersect", "布尔交集")));
        results.Add(Name(BooleanPair(OcctBooleanOperation.Section, 180), Local("Section Curves", "截交线")));
        Engine.FitAll();
        return new(Local("Boolean operation samples created.", "已生成布尔运算示例。"), results);
    }

    private OcctShape BooleanPair(OcctBooleanOperation operation, double x)
    {
        var box = Engine.MakeBox(80, 80, 70, x - 40, -40, 0);
        var sphere = Engine.MakeSphere(48, x + 20, 0, 45);
        return Engine.Boolean(operation, box, sphere, true);
    }

    private CadCommandResult DemoElements()
    {
        var results = new List<IOcctObject>();

        results.Add(Name(Engine.MakeVertex(new(-260, -120, 0)), Local("Vertex", "顶点")));
        results.Add(Name(Engine.MakeLine(new(-230, -120, 0), new(-150, -80, 0)), Local("Line", "直线")));
        results.Add(Name(Engine.MakePolyline(new[]
        {
            new OcctPoint3d(-120, -120, 0),
            new OcctPoint3d(-80, -80, 0),
            new OcctPoint3d(-40, -125, 0)
        }), Local("Polyline", "多段线")));
        results.Add(Name(Engine.MakeArc(new(-10, -120, 0), new(35, -70, 0), new(80, -120, 0)), Local("Arc", "圆弧")));
        results.Add(Name(Engine.MakeEllipse(new(140, -100, 0), OcctVector3d.UnitZ, 55, 28), Local("Ellipse", "椭圆")));
        results.Add(Name(Engine.MakeBezier(new[]
        {
            new OcctPoint3d(220, -125, 0),
            new OcctPoint3d(250, -55, 0),
            new OcctPoint3d(300, -150, 0),
            new OcctPoint3d(340, -85, 0)
        }), Local("Bezier Curve", "Bezier 曲线")));
        results.Add(Name(Engine.MakeInterpolatedBSpline(new[]
        {
            new OcctPoint3d(-260, 10, 0),
            new OcctPoint3d(-215, 55, 0),
            new OcctPoint3d(-170, -5, 0),
            new OcctPoint3d(-125, 65, 0),
            new OcctPoint3d(-80, 15, 0)
        }), Local("B-Spline Curve", "B 样条曲线")));

        var polygonFace = Engine.MakeRegularPolygon(48, 7, true, new(-10, 30, 0));
        results.Add(Name(polygonFace, Local("Planar Face", "平面")));

        var extrusionProfile = Engine.MakeRegularPolygon(38, 6, true, new(100, 10, 0));
        var extrusion = Engine.Extrude(extrusionProfile, new(0, 0, 70), true);
        results.Add(Name(extrusion, Local("Extruded Feature", "拉伸特征")));

        var box = Engine.MakeBox(85, 70, 65, 190, -10, 0);
        var cutter = Engine.MakeCylinder(new(232.5, 25, -1), OcctVector3d.UnitZ, 18, 67);
        var cutResult = Engine.Cut(box, cutter, true);
        results.Add(Name(cutResult, Local("Boolean Feature", "布尔特征")));

        results.Add(Name(Engine.MakeTorus(42, 11, new(330, 30, 35)), Local("Torus", "圆环体")));
        var vectorText = Engine.MakeTextShape(Local("VECTOR", "矢量"), new(-80, 120, 0), 28, 2, "Microsoft YaHei UI", bold: true);
        Engine.SetColor(vectorText, Color.DarkSlateBlue);
        results.Add(Name(vectorText, Local("BRep Vector Text", "BRep 矢量文字")));

        foreach (var shape in results.OfType<OcctShape>())
        {
            if (Engine.GetShapeType(shape) is OcctShapeType.Solid or OcctShapeType.CompSolid)
            {
                Engine.SetMaterial(shape, OcctMaterial.Satin);
            }
        }
        Engine.FitAll();
        return new(Local("Comprehensive element results created.", "已生成综合元素测试结果。"), results);
    }

    private CadCommandResult DemoGear()
    {
        const int toothCount = 24;
        const double rootRadius = 62;
        const double tipRadius = 78;
        const double thickness = 18;
        var outline = new List<OcctPoint3d>(toothCount * 4);
        for (var tooth = 0; tooth < toothCount; tooth++)
        {
            var baseAngle = tooth * Math.PI * 2.0 / toothCount;
            foreach (var item in new[]
                     {
                         (Offset: 0.02, Radius: rootRadius),
                         (Offset: 0.22, Radius: tipRadius),
                         (Offset: 0.78, Radius: tipRadius),
                         (Offset: 0.98, Radius: rootRadius)
                     })
            {
                var angle = baseAngle + item.Offset * Math.PI * 2.0 / toothCount;
                outline.Add(new OcctPoint3d(
                    Math.Cos(angle) * item.Radius,
                    Math.Sin(angle) * item.Radius,
                    0));
            }
        }

        var wire = Engine.MakePolyline(outline, true);
        var face = Engine.MakeFace(wire);
        var gear = Engine.Extrude(face, new(0, 0, thickness), true);
        gear = Engine.DrillHole(gear, new(0, 0, -1), OcctVector3d.UnitZ, 18, thickness + 2, true);
        for (var index = 0; index < 6; index++)
        {
            var angle = index * Math.PI / 3.0;
            gear = Engine.DrillHole(
                gear,
                new(Math.Cos(angle) * 40, Math.Sin(angle) * 40, -1),
                OcctVector3d.UnitZ,
                7,
                thickness + 2,
                true);
        }
        try { gear = Engine.ChamferAllEdges(gear, 0.8, true); } catch (OcctException) { }
        SetGeneratedName(gear, Local("Complex Gear", "复杂齿轮"));
        Engine.SetMaterial(gear, OcctMaterial.Steel);
        Engine.FitAll();
        return CadCommandResult.Created(Local("Complex gear result created.", "已生成复杂齿轮结果。"), gear);
    }

    private CadCommandResult DemoManifold()
    {
        var body = Engine.MakeBox(220, 130, 80, -110, -65, 0);
        var topBoss = Engine.MakeCylinder(new(0, 0, 80), OcctVector3d.UnitZ, 42, 50);
        body = Engine.Fuse(body, topBoss, true);
        var xBoss = Engine.MakeCylinder(new(-150, 0, 40), OcctVector3d.UnitX, 30, 300);
        body = Engine.Fuse(body, xBoss, true);
        var yBoss = Engine.MakeCylinder(new(0, -105, 42), OcctVector3d.UnitY, 26, 210);
        body = Engine.Fuse(body, yBoss, true);

        body = Engine.Cut(body, Engine.MakeCylinder(new(0, 0, -1), OcctVector3d.UnitZ, 25, 132), true);
        body = Engine.Cut(body, Engine.MakeCylinder(new(-151, 0, 40), OcctVector3d.UnitX, 16, 302), true);
        body = Engine.Cut(body, Engine.MakeCylinder(new(0, -106, 42), OcctVector3d.UnitY, 14, 212), true);
        foreach (var point in new[]
                 {
                     new OcctPoint3d(-82, -42, -1),
                     new OcctPoint3d(82, -42, -1),
                     new OcctPoint3d(-82, 42, -1),
                     new OcctPoint3d(82, 42, -1)
                 })
        {
            body = Engine.DrillHole(body, point, OcctVector3d.UnitZ, 6.5, 82, true);
        }
        try { body = Engine.FilletAllEdges(body, 2.0, true); } catch (OcctException) { }
        SetGeneratedName(body, Local("Multi-Port Manifold", "多通道阀体"));
        Engine.SetMaterial(body, OcctMaterial.Steel);
        Engine.FitAll();
        return CadCommandResult.Created(Local("Multi-port manifold result created.", "已生成多通道阀体结果。"), body);
    }

    private CadCommandResult DemoTwistedDuct()
    {
        static OcctShape RectangleSection(
            OcctEngine engine,
            double centerX,
            double centerY,
            double z,
            double width,
            double height,
            double angleDegrees)
        {
            var angle = angleDegrees * Math.PI / 180.0;
            var xDirection = new OcctVector3d(Math.Cos(angle), Math.Sin(angle), 0);
            var yDirection = new OcctVector3d(-Math.Sin(angle), Math.Cos(angle), 0);
            var origin = new OcctPoint3d(
                centerX - xDirection.X * width * 0.5 - yDirection.X * height * 0.5,
                centerY - xDirection.Y * width * 0.5 - yDirection.Y * height * 0.5,
                z);
            return engine.MakeRectangleWire(width, height, origin, xDirection, OcctVector3d.UnitZ);
        }

        var sections = new[]
        {
            (X: 0d, Y: 0d, Z: 0d, Width: 130d, Height: 82d, Angle: 0d),
            (X: 14d, Y: 0d, Z: 65d, Width: 108d, Height: 72d, Angle: 18d),
            (X: -10d, Y: 18d, Z: 135d, Width: 142d, Height: 62d, Angle: -14d),
            (X: 20d, Y: 28d, Z: 215d, Width: 96d, Height: 98d, Angle: 34d)
        };
        var outerSections = sections
            .Select(item => RectangleSection(Engine, item.X, item.Y, item.Z, item.Width, item.Height, item.Angle))
            .ToArray();
        var innerSections = sections
            .Select((item, index) => RectangleSection(
                Engine,
                item.X,
                item.Y,
                item.Z + (index == 0 ? -1 : index == sections.Length - 1 ? 1 : 0),
                item.Width - 10,
                item.Height - 10,
                item.Angle))
            .ToArray();
        var outer = Engine.Loft(outerSections, true, false, hideInputs: true);
        var inner = Engine.Loft(innerSections, true, false, hideInputs: true);
        var duct = Engine.Cut(outer, inner, true);
        SetGeneratedName(duct, Local("Twisted Transition Duct", "扭转过渡风管"));
        Engine.SetMaterial(duct, OcctMaterial.Aluminum);
        Engine.FitAll();
        return CadCommandResult.Created(Local("Twisted transition duct result created.", "已生成扭转过渡风管结果。"), duct);
    }

    private CadCommandResult DemoAnnotations()
    {
        var lengthSource = Engine.MakeLine(new(-190, -90, 0), new(-40, -90, 0));
        var angleFirst = Engine.MakeLine(new(-180, 15, 0), new(-70, 15, 0));
        var angleSecond = Engine.MakeLine(new(-180, 15, 0), new(-105, 82, 0));
        var radiusSource = Engine.MakeCircle(new(70, 55, 0), OcctVector3d.UnitZ, 38);
        var diameterSource = Engine.MakeCircle(new(185, -65, 0), OcctVector3d.UnitZ, 34);

        var length = Engine.MakeLengthAnnotationShape(lengthSource, 26, 9, 6, "Microsoft YaHei UI");
        var angle = Engine.MakeAngleAnnotationShape(angleFirst, angleSecond, 46, 9, 6, "Microsoft YaHei UI");
        var radius = Engine.MakeRadiusAnnotationShape(radiusSource, 28, 9, 6, "Microsoft YaHei UI");
        var diameter = Engine.MakeDiameterAnnotationShape(diameterSource, 24, 9, 6, "Microsoft YaHei UI");
        var text = Engine.MakeTextShape(
            Local("VECTOR ANNOTATIONS", "矢量注释标注"),
            new(-190, 135, 0),
            24,
            1.5,
            "Microsoft YaHei UI",
            bold: true);

        Name(length, Local("Vector Linear Dimension", "矢量线性尺寸"));
        Name(angle, Local("Vector Angular Dimension", "矢量角度尺寸"));
        Name(radius, Local("Vector Radius Dimension", "矢量半径尺寸"));
        Name(diameter, Local("Vector Diameter Dimension", "矢量直径尺寸"));
        Name(text, Local("BRep Note Text", "BRep 说明文字"));
        Engine.SetColor(length, Color.DarkBlue);
        Engine.SetColor(angle, Color.DarkGreen);
        Engine.SetColor(radius, Color.DarkRed);
        Engine.SetColor(diameter, Color.Purple);
        Engine.SetColor(text, Color.Black);
        Engine.FitAll();
        return new(
            Local("Vector annotation results created.", "已生成矢量注释标注结果。"),
            new IOcctObject[] { length, angle, radius, diameter, text });
    }

    private void RemoveDemoProcessObjects(
        IReadOnlySet<long> initialObjectIds,
        IEnumerable<IOcctObject> resultObjects)
    {
        var resultIds = resultObjects.Select(item => item.Id).ToHashSet();
        var processObjects = Engine.Objects
            .Where(item => !initialObjectIds.Contains(item.Id) && !resultIds.Contains(item.Id))
            .Select(item => (IOcctObject)item)
            .ToArray();
        if (processObjects.Length > 0) Engine.Delete(processObjects);
    }

    private static bool IsDemoCommand(CadCommandId commandId) =>
        commandId is CadCommandId.DemoPrimitives
            or CadCommandId.DemoBracket
            or CadCommandId.DemoFlange
            or CadCommandId.DemoPipe
            or CadCommandId.DemoTee
            or CadCommandId.DemoReducer
            or CadCommandId.DemoLoft
            or CadCommandId.DemoBoolean
            or CadCommandId.DemoElements
            or CadCommandId.DemoGear
            or CadCommandId.DemoManifold
            or CadCommandId.DemoTwistedDuct
            or CadCommandId.DemoAnnotations;

    private static string Local(string english, string chinese) =>
        CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? chinese : english;

    private CadCommandResult CreateShape(string baseName, OcctShape shape)
    {
        SetGeneratedName(shape, baseName);
        ActiveObject = new OcctObject(shape.Id, OcctObjectKind.Shape);
        return CadCommandResult.Created(CadLocalization.Text("Session.Created", baseName), shape);
    }

    private T Name<T>(T value, string name) where T : struct, IOcctObject
    {
        SetGeneratedName(value, name);
        return value;
    }

    private void SetGeneratedName(IOcctObject value, string baseName)
    {
        var clean = string.IsNullOrWhiteSpace(baseName) ? Engine.GetObjectKind(value.Id).ToString() : baseName.Trim();
        Engine.SetName(value, $"{clean}_{_nameSequence++:000}");
    }

    private OcctShape RequireShape()
    {
        var shapes = SelectedShapes();
        if (shapes.Count > 0) return shapes[0];
        throw new InvalidOperationException(CadLocalization.Text("Session.SelectOne"));
    }

    private IReadOnlyList<OcctShape> RequireShapes(int minimum)
    {
        var shapes = SelectedShapes();
        if (shapes.Count < minimum) throw new InvalidOperationException(CadLocalization.Text("Session.SelectMany", minimum));
        return shapes;
    }

    private List<OcctShape> SelectedShapes()
    {
        var shapes = Engine.SelectedObjects.Where(item => item.Kind == OcctObjectKind.Shape).Select(item => new OcctShape(item.Id)).DistinctBy(item => item.Id).ToList();
        if (shapes.Count == 0 && ActiveObject is { Kind: OcctObjectKind.Shape } active && Engine.Exists(active)) shapes.Add(new OcctShape(active.Id));
        return shapes;
    }

    private OcctShape CopySelectedSubshape(int index)
    {
        try { return Engine.CopySelectedSubshape(index); }
        catch (OcctException exception) { throw new InvalidOperationException(CadLocalization.Text("Session.SelectSubshape"), exception); }
    }

    private void ExportSingleOrCompound(string filePath, Action<OcctShape, string> exporter)
    {
        var shapes = Engine.Shapes;
        if (shapes.Count == 0) throw new InvalidOperationException(CadLocalization.Text("Session.NoExportShape"));
        if (shapes.Count == 1) { exporter(shapes[0], filePath); return; }
        var compound = Engine.MakeCompound(shapes, false);
        try { exporter(compound, filePath); }
        finally { Engine.Delete(compound); }
    }

    private OcctPoint3d GetSceneCenter()
    {
        if (Engine.Shapes.Count == 0) return OcctPoint3d.Origin;
        var bounds = Engine.Shapes.Select(Engine.GetBounds).ToArray();
        return new(
            (bounds.Min(item => item.MinX) + bounds.Max(item => item.MaxX)) / 2,
            (bounds.Min(item => item.MinY) + bounds.Max(item => item.MaxY)) / 2,
            (bounds.Min(item => item.MinZ) + bounds.Max(item => item.MaxZ)) / 2);
    }

    private double GetSceneDiagonal()
    {
        if (Engine.Shapes.Count == 0) return 100;
        var bounds = Engine.Shapes.Select(Engine.GetBounds).ToArray();
        var dx = bounds.Max(item => item.MaxX) - bounds.Min(item => item.MinX);
        var dy = bounds.Max(item => item.MaxY) - bounds.Min(item => item.MinY);
        var dz = bounds.Max(item => item.MaxZ) - bounds.Min(item => item.MinZ);
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }
}
