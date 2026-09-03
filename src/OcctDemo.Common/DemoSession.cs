using System.Drawing;
using System.Globalization;
using OcctNet;

namespace OcctDemo.Common;

public sealed class DemoValues
{
    private readonly IReadOnlyDictionary<string, string> _values;

    public DemoValues(IReadOnlyDictionary<string, string>? values = null)
    {
        _values = values ?? new Dictionary<string, string>();
    }

    public string Text(string key, string fallback = "") => _values.TryGetValue(key, out var value) ? value : fallback;

    public double Number(string key, double fallback = 0)
    {
        if (!_values.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value)) return fallback;
        if (double.TryParse(value, NumberStyles.Float, CultureInfo.CurrentCulture, out var current)) return current;
        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var invariant)) return invariant;
        throw new FormatException(DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified ? $"参数“{key}”不是有效数值：{value}" : $"Parameter '{key}' is not a valid number: {value}");
    }

    public int Integer(string key, int fallback = 0)
    {
        if (!_values.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value)) return fallback;
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.CurrentCulture, out var current)) return current;
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var invariant)) return invariant;
        throw new FormatException(DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified ? $"参数“{key}”不是有效整数：{value}" : $"Parameter '{key}' is not a valid integer: {value}");
    }

    public bool Boolean(string key, bool fallback = false)
    {
        if (!_values.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value)) return fallback;
        return value.Trim().ToLowerInvariant() switch
        {
            "true" or "1" or "yes" or "是" or "开启" => true,
            "false" or "0" or "no" or "否" or "关闭" => false,
            _ => throw new FormatException(DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified ? $"参数“{key}”不是有效布尔值：{value}" : $"Parameter '{key}' is not a valid Boolean value: {value}")
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
            if (components.Length is < 2 or > 3) throw new FormatException(DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified ? $"坐标点格式错误：{pointText}" : $"Invalid coordinate point: {pointText}");
            var x = ParseNumber(components[0]);
            var y = ParseNumber(components[1]);
            var z = components.Length == 3 ? ParseNumber(components[2]) : 0;
            result.Add(new OcctPoint3d(x, y, z));
        }
        if (result.Count == 0) throw new FormatException(DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified ? "至少需要一个坐标点。" : "At least one coordinate point is required.");
        return result;
    }

    private static double ParseNumber(string value)
    {
        if (double.TryParse(value, NumberStyles.Float, CultureInfo.CurrentCulture, out var current)) return current;
        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var invariant)) return invariant;
        throw new FormatException(DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified ? $"坐标值不是有效数值：{value}" : $"Coordinate value is not numeric: {value}");
    }
}

public enum DemoIsoView { NorthEast, NorthWest, SouthEast, SouthWest }

public sealed partial class DemoSession
{
    private static readonly Color[] CreatedShapePalette =
    [
        Color.FromArgb(186, 208, 228),
        Color.FromArgb(193, 218, 230),
        Color.FromArgb(192, 223, 218),
        Color.FromArgb(211, 220, 230),
        Color.FromArgb(208, 216, 235),
        Color.FromArgb(204, 225, 220),
        Color.FromArgb(220, 224, 228),
        Color.FromArgb(213, 226, 239)
    ];

    private int _nameSequence = 1;
    private int _shapeColorSequence;
    private readonly List<DemoHistoryEntry> _history = new();
    private int _historyPosition;
    private bool _restoringHistory;
    private bool _suppressNotifications;
    private bool _historyAvailable = true;
    private string? _historyBaselineFile;

    public DemoSession(OcctEngine engine)
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

    public void NewDocument()
    {
        Engine.Clear();
        ActiveObject = null;
        CurrentFilePath = null;
        _historyBaselineFile = null;
        _historyAvailable = true;
        IsModified = false;
        _nameSequence = 1;
        _shapeColorSequence = 0;
        ClearHistory();
        ModelChanged?.Invoke(this, EventArgs.Empty);
        StatusChanged?.Invoke(this, DemoLocalization.Text("Session.New"));
    }

    public OcctShape Open(string filePath)
    {
        Engine.Clear();
        ActiveObject = null;
        _nameSequence = 1;
        _shapeColorSequence = 0;
        CurrentFilePath = Path.GetFullPath(filePath);
        _historyBaselineFile = CurrentFilePath;
        _historyAvailable = true;
        ClearHistory();
        var shape = ImportCore(CurrentFilePath);
        IsModified = false;
        Engine.FitAll();
        ModelChanged?.Invoke(this, EventArgs.Empty);
        StatusChanged?.Invoke(this, DemoLocalization.Text("Session.Open", CurrentFilePath));
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
            _history.Add(DemoHistoryEntry.Import(fullPath,
                DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified
                    ? $"导入 {Path.GetFileName(fullPath)}"
                    : $"Import {Path.GetFileName(fullPath)}"));
            _historyPosition = _history.Count;
            HistoryChanged?.Invoke(this, EventArgs.Empty);
        }
        Engine.FitAll();
        if (!_suppressNotifications)
        {
            ModelChanged?.Invoke(this, EventArgs.Empty);
            StatusChanged?.Invoke(this, DemoLocalization.Text("Session.Import", fullPath));
        }
        return shape;
    }

    private OcctShape ImportCore(string filePath)
    {
        var shape = Engine.Import(filePath);
        var applicationTag = Engine.GetApplicationTag(shape);
        if (!applicationTag.StartsWith("step-path:", StringComparison.Ordinal))
        {
            SetGeneratedName(shape, Path.GetFileNameWithoutExtension(filePath));
        }
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
            default: throw new NotSupportedException(DemoLocalization.Text("Session.UnsupportedSave"));
        }
        CurrentFilePath = Path.GetFullPath(filePath);
        IsModified = false;
        StatusChanged?.Invoke(this, DemoLocalization.Text("Session.Save", CurrentFilePath));
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
            default: throw new NotSupportedException(DemoLocalization.Text("Session.UnsupportedExport"));
        }
        StatusChanged?.Invoke(this, DemoLocalization.Text("Session.Export", filePath));
    }

    public void SetIsoView(DemoIsoView view)
    {
        var center = GetSceneCenter();
        var diagonal = GetSceneDiagonal();
        var eyeVector = (view switch
        {
            DemoIsoView.NorthEast => new OcctVector3d(1, -1, 1),
            DemoIsoView.NorthWest => new OcctVector3d(-1, -1, 1),
            DemoIsoView.SouthEast => new OcctVector3d(1, 1, 1),
            DemoIsoView.SouthWest => new OcctVector3d(-1, 1, 1),
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

    public void SetObjectColor(IOcctObject value, Color color)
    {
        Engine.SetObjectColor(value, color);
        IsModified = true;
    }

    public IReadOnlyList<KeyValuePair<string, string>> DescribeObject(IOcctObject value) =>
        DescribeObjectLightweight(value);

    public string SafeName(IOcctObject value)
    {
        var name = Engine.GetObjectName(value);
        return string.IsNullOrWhiteSpace(name) ? $"{DemoLocalization.ObjectKind(value.Kind)} {value.Id}" : name;
    }

    private static string Local(string english, string chinese) =>
        DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified ? chinese : english;

    private DemoCommandResult CreateShape(string baseName, OcctShape shape)
    {
        SetGeneratedName(shape, baseName);
        SetObjectColor(shape, CreatedShapePalette[_shapeColorSequence++ % CreatedShapePalette.Length]);
        ActiveObject = shape;
        return DemoCommandResult.Created(DemoLocalization.Text("Session.Created", baseName), shape);
    }

    private T Name<T>(T value, string name) where T : struct, IOcctObject
    {
        SetGeneratedName(value, name);
        return value;
    }

    private void SetGeneratedName(IOcctObject value, string baseName)
    {
        var clean = string.IsNullOrWhiteSpace(baseName) ? value.Kind.ToString() : baseName.Trim();
        Engine.SetObjectName(value, $"{clean}_{_nameSequence++:000}");
    }

    private OcctShape RequireShape()
    {
        var shapes = SelectedShapes();
        if (shapes.Count > 0) return shapes[0];
        throw new InvalidOperationException(DemoLocalization.Text("Session.SelectOne"));
    }

    private IReadOnlyList<OcctShape> RequireShapes(int minimum)
    {
        var shapes = SelectedShapes();
        if (shapes.Count < minimum) throw new InvalidOperationException(DemoLocalization.Text("Session.SelectMany", minimum));
        return shapes;
    }

    private List<OcctShape> SelectedShapes() =>
        Engine.SelectedObjects
            .OfType<OcctShape>()
            .DistinctBy(item => item.Id)
            .ToList();

    private List<OcctShape> GetSceneShapes() =>
        Engine.GetObjects()
            .OfType<OcctShape>()
            .ToList();

    private OcctShape CopySelectedSubshape(int index)
    {
        try { return Engine.CopySelectedSubshape(index); }
        catch (OcctException exception) { throw new InvalidOperationException(DemoLocalization.Text("Session.SelectSubshape"), exception); }
    }

    private void ExportSingleOrCompound(string filePath, Action<OcctShape, string> exporter)
    {
        var shapes = GetSceneShapes();
        if (shapes.Count == 0) throw new InvalidOperationException(DemoLocalization.Text("Session.NoExportShape"));
        if (shapes.Count == 1) { exporter(shapes[0], filePath); return; }
        var compound = Engine.MakeCompound(shapes, false);
        try { exporter(compound, filePath); }
        finally { Engine.Delete(compound); }
    }

    private OcctPoint3d GetSceneCenter()
    {
        var shapes = GetSceneShapes();
        if (shapes.Count == 0) return OcctPoint3d.Origin;
        var bounds = shapes.Select(Engine.GetShapeBounds).ToArray();
        return new(
            (bounds.Min(item => item.MinX) + bounds.Max(item => item.MaxX)) / 2,
            (bounds.Min(item => item.MinY) + bounds.Max(item => item.MaxY)) / 2,
            (bounds.Min(item => item.MinZ) + bounds.Max(item => item.MaxZ)) / 2);
    }

    private double GetSceneDiagonal()
    {
        var shapes = GetSceneShapes();
        if (shapes.Count == 0) return 100;
        var bounds = shapes.Select(Engine.GetShapeBounds).ToArray();
        var dx = bounds.Max(item => item.MaxX) - bounds.Min(item => item.MinX);
        var dy = bounds.Max(item => item.MaxY) - bounds.Min(item => item.MinY);
        var dz = bounds.Max(item => item.MaxZ) - bounds.Min(item => item.MinZ);
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }
}
