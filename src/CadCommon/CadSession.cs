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

public sealed partial class CadSession
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
    public IOcctObject? ActiveObject { get; set; }
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
        ActiveObject = shape;
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

    public IReadOnlyList<KeyValuePair<string, string>> DescribeObject(IOcctObject value)
    {
        var rows = new List<KeyValuePair<string, string>>
        {
            new(CadLocalization.Text("Object.Id"), value.Id.ToString(CultureInfo.InvariantCulture)),
            new(CadLocalization.Text("Object.Name"), SafeName(value)),
            new(CadLocalization.Text("Object.Kind"), CadLocalization.ObjectKind(value.Kind))
        };
        if (value.Kind != OcctObjectKind.Shape) return rows;
        var shape = Engine.GetShape(value.Id);
        rows.Add(new(CadLocalization.Text("Object.Topology"), CadLocalization.ShapeType(Engine.GetShapeType(shape))));
        rows.Add(new(CadLocalization.Text("Object.Validity"), Engine.IsShapeValid(shape) ? CadLocalization.Text("Object.Valid") : CadLocalization.Text("Object.Invalid")));
        var bounds = Engine.GetShapeBounds(shape);
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

    private static string Local(string english, string chinese) =>
        CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? chinese : english;

    private CadCommandResult CreateShape(string baseName, OcctShape shape)
    {
        SetGeneratedName(shape, baseName);
        ActiveObject = shape;
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
        return Engine.SelectedObjects
            .Where(item => item.Kind == OcctObjectKind.Shape)
            .Select(item => Engine.GetShape(item.Id))
            .DistinctBy(item => item.Id)
            .ToList();
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
        var bounds = Engine.Shapes.Select(Engine.GetShapeBounds).ToArray();
        return new(
            (bounds.Min(item => item.MinX) + bounds.Max(item => item.MaxX)) / 2,
            (bounds.Min(item => item.MinY) + bounds.Max(item => item.MaxY)) / 2,
            (bounds.Min(item => item.MinZ) + bounds.Max(item => item.MaxZ)) / 2);
    }

    private double GetSceneDiagonal()
    {
        if (Engine.Shapes.Count == 0) return 100;
        var bounds = Engine.Shapes.Select(Engine.GetShapeBounds).ToArray();
        var dx = bounds.Max(item => item.MaxX) - bounds.Min(item => item.MinX);
        var dy = bounds.Max(item => item.MaxY) - bounds.Min(item => item.MinY);
        var dz = bounds.Max(item => item.MaxZ) - bounds.Min(item => item.MinZ);
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }
}
