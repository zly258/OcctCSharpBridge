using OcctNet;

namespace OcctDemo.Common;

public sealed partial class DemoSession
{
    private readonly Dictionary<AnalysisKey, OcctBounds> _boundsAnalysisCache = new();
    private readonly Dictionary<AnalysisKey, MassAnalysis> _massAnalysisCache = new();
    private readonly Dictionary<AnalysisKey, TopologyAnalysis> _topologyAnalysisCache = new();
    private readonly Dictionary<AnalysisKey, bool> _validationAnalysisCache = new();
    private readonly Dictionary<DistanceAnalysisKey, OcctDistanceResult> _distanceAnalysisCache = new();
    private long _analysisRevision;
    private bool _analysisCacheTrackingEnabled;

    private DemoCommandResult AnalyzeBounds()
    {
        var shape = RequireShape();
        var key = AnalysisKeyFor(shape);
        if (!_boundsAnalysisCache.TryGetValue(key, out var bounds))
        {
            bounds = Engine.GetShapeBounds(shape);
            _boundsAnalysisCache[key] = bounds;
        }

        var text = DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified
            ? $"最小点：({bounds.MinX:G8}, {bounds.MinY:G8}, {bounds.MinZ:G8})\n最大点：({bounds.MaxX:G8}, {bounds.MaxY:G8}, {bounds.MaxZ:G8})\n尺寸：{bounds.SizeX:G8} × {bounds.SizeY:G8} × {bounds.SizeZ:G8}\n中心：{bounds.Center}"
            : $"Minimum: ({bounds.MinX:G8}, {bounds.MinY:G8}, {bounds.MinZ:G8})\nMaximum: ({bounds.MaxX:G8}, {bounds.MaxY:G8}, {bounds.MaxZ:G8})\nSize: {bounds.SizeX:G8} × {bounds.SizeY:G8} × {bounds.SizeZ:G8}\nCenter: {bounds.Center}";
        return new(Local("Extents analysis completed.", "包围盒分析完成。"), Array.Empty<IOcctObject>(), text);
    }

    private DemoCommandResult AnalyzeMass()
    {
        var shape = RequireShape();
        var key = AnalysisKeyFor(shape);
        if (!_massAnalysisCache.TryGetValue(key, out var analysis))
        {
            analysis = new MassAnalysis(
                Engine.GetShapeLinearProperties(shape),
                Engine.GetShapeSurfaceProperties(shape),
                Engine.GetShapeVolumeProperties(shape));
            _massAnalysisCache[key] = analysis;
        }

        var text = DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified
            ? $"总长度：{analysis.Linear.Mass:G10}\n总面积：{analysis.Surface.Mass:G10}\n总体积：{analysis.Volume.Mass:G10}\n体积重心：{analysis.Volume.CenterOfMass}"
            : $"Total Length: {analysis.Linear.Mass:G10}\nSurface Area: {analysis.Surface.Mass:G10}\nVolume: {analysis.Volume.Mass:G10}\nCentroid: {analysis.Volume.CenterOfMass}";
        return new(Local("Mass properties completed.", "几何属性分析完成。"), Array.Empty<IOcctObject>(), text);
    }

    private DemoCommandResult AnalyzeTopology()
    {
        var shape = RequireShape();
        var key = AnalysisKeyFor(shape);
        if (!_topologyAnalysisCache.TryGetValue(key, out var analysis))
        {
            analysis = new TopologyAnalysis(
                Engine.GetTopologyCount(shape, OcctShapeType.Vertex),
                Engine.GetTopologyCount(shape, OcctShapeType.Edge),
                Engine.GetTopologyCount(shape, OcctShapeType.Wire),
                Engine.GetTopologyCount(shape, OcctShapeType.Face),
                Engine.GetTopologyCount(shape, OcctShapeType.Shell),
                Engine.GetTopologyCount(shape, OcctShapeType.Solid));
            _topologyAnalysisCache[key] = analysis;
        }

        var text = string.Join(Environment.NewLine,
            $"{OcctShapeType.Vertex}: {analysis.Vertices}",
            $"{OcctShapeType.Edge}: {analysis.Edges}",
            $"{OcctShapeType.Wire}: {analysis.Wires}",
            $"{OcctShapeType.Face}: {analysis.Faces}",
            $"{OcctShapeType.Shell}: {analysis.Shells}",
            $"{OcctShapeType.Solid}: {analysis.Solids}");
        return new(Local("Topology statistics completed.", "拓扑统计完成。"), Array.Empty<IOcctObject>(), text);
    }

    private DemoCommandResult AnalyzeDistance()
    {
        var shapes = RequireShapes(2);
        EnsureAnalysisCacheTracking();
        var key = new DistanceAnalysisKey(shapes[0].Id, shapes[1].Id, _analysisRevision);
        if (!_distanceAnalysisCache.TryGetValue(key, out var result))
        {
            result = Engine.GetShapeDistance(shapes[0], shapes[1]);
            _distanceAnalysisCache[key] = result;
        }

        var text = DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified
            ? $"最短距离：{result.Distance:G10}\n对象 1 最近点：{result.PointOnFirst}\n对象 2 最近点：{result.PointOnSecond}"
            : $"Minimum Distance: {result.Distance:G10}\nClosest Point on Object 1: {result.PointOnFirst}\nClosest Point on Object 2: {result.PointOnSecond}";
        return new(Local("Minimum distance calculation completed.", "最短距离计算完成。"), Array.Empty<IOcctObject>(), text);
    }

    private DemoCommandResult ValidateShape()
    {
        var shape = RequireShape();
        var key = AnalysisKeyFor(shape);
        if (!_validationAnalysisCache.TryGetValue(key, out var valid))
        {
            valid = Engine.IsShapeValid(shape);
            _validationAnalysisCache[key] = valid;
        }

        return new(
            valid ? Local("Shape validation passed.", "形体检查通过。") : Local("Shape validation failed.", "形体检查未通过。"),
            Array.Empty<IOcctObject>(),
            valid
                ? Local("The shape passed BRepCheck validation.", "当前形体通过 BRepCheck 检查。")
                : Local("The shape contains invalid topology or geometry.", "当前形体存在无效拓扑或几何。"));
    }

    private AnalysisKey AnalysisKeyFor(OcctShape shape)
    {
        EnsureAnalysisCacheTracking();
        return new AnalysisKey(shape.Id, _analysisRevision);
    }

    private void EnsureAnalysisCacheTracking()
    {
        if (_analysisCacheTrackingEnabled) return;
        ModelChanged += OnAnalysisModelChanged;
        _analysisCacheTrackingEnabled = true;
    }

    private void OnAnalysisModelChanged(object? sender, EventArgs e)
    {
        ++_analysisRevision;
        _boundsAnalysisCache.Clear();
        _massAnalysisCache.Clear();
        _topologyAnalysisCache.Clear();
        _validationAnalysisCache.Clear();
        _distanceAnalysisCache.Clear();
    }

    private readonly record struct AnalysisKey(long ObjectId, long GeometryRevision);
    private readonly record struct DistanceAnalysisKey(long FirstObjectId, long SecondObjectId, long GeometryRevision);
    private readonly record struct MassAnalysis(OcctMassProperties Linear, OcctMassProperties Surface, OcctMassProperties Volume);
    private readonly record struct TopologyAnalysis(int Vertices, int Edges, int Wires, int Faces, int Shells, int Solids);
}
