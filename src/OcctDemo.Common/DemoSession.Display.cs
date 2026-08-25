using System.Globalization;
using OcctNet;

namespace OcctDemo.Common;

public sealed partial class DemoSession
{
    private const string StepPathPrefix = "step-path:";
    private const char StepPathSeparator = '\u001F';

    public IReadOnlyList<string> GetHierarchyPath(IOcctObject value)
    {
        if (value.Kind != OcctObjectKind.Shape) return Array.Empty<string>();
        var tag = Engine.GetApplicationTag(value);
        if (!tag.StartsWith(StepPathPrefix, StringComparison.Ordinal)) return Array.Empty<string>();
        return tag[StepPathPrefix.Length..]
            .Split(StepPathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    public IReadOnlyList<KeyValuePair<string, string>> DescribeObjectLightweight(IOcctObject value)
    {
        var rows = new List<KeyValuePair<string, string>>
        {
            new(DemoLocalization.Text("Object.Id"), value.Id.ToString(CultureInfo.InvariantCulture)),
            new(DemoLocalization.Text("Object.Name"), SafeName(value)),
            new(DemoLocalization.Text("Object.Kind"), DemoLocalization.ObjectKind(value.Kind))
        };

        var hierarchy = GetHierarchyPath(value);
        if (hierarchy.Count > 1)
        {
            rows.Add(new(
                Local("Assembly Path", "装配路径"),
                string.Join(" / ", hierarchy.Take(hierarchy.Count - 1))));
        }

        if (value.Kind == OcctObjectKind.Shape)
        {
            rows.Add(new(
                Local("Geometry Details", "几何详情"),
                Local("Click to query…", "点击查询…")));
        }

        return rows;
    }

    /// <summary>
    /// On-demand geometry summary (bounds / mass / topology) for the property grid.
    /// </summary>
    public IReadOnlyList<KeyValuePair<string, string>> QueryGeometryDetails(IOcctObject value)
    {
        if (value.Kind != OcctObjectKind.Shape)
            return Array.Empty<KeyValuePair<string, string>>();

        var shape = (OcctShape)value;
        EnsureAnalysisCacheTracking();
        var key = AnalysisKeyFor(shape);
        var rows = new List<KeyValuePair<string, string>>();

        try
        {
            if (!_boundsAnalysisCache.TryGetValue(key, out var bounds))
            {
                bounds = Engine.GetShapeBounds(shape);
                _boundsAnalysisCache[key] = bounds;
            }
            rows.Add(new(Local("Bounding Box Min", "包围盒最小点"),
                $"({bounds.MinX:G6}, {bounds.MinY:G6}, {bounds.MinZ:G6})"));
            rows.Add(new(Local("Bounding Box Max", "包围盒最大点"),
                $"({bounds.MaxX:G6}, {bounds.MaxY:G6}, {bounds.MaxZ:G6})"));
            rows.Add(new(Local("Size (X×Y×Z)", "尺寸 (X×Y×Z)"),
                $"{bounds.SizeX:G6} × {bounds.SizeY:G6} × {bounds.SizeZ:G6}"));
            rows.Add(new(Local("Center", "中心"), bounds.Center.ToString() ?? ""));
        }
        catch (Exception ex)
        {
            rows.Add(new(Local("Bounds", "包围盒"), ex.Message));
        }

        try
        {
            if (!_massAnalysisCache.TryGetValue(key, out var mass))
            {
                mass = new MassAnalysis(
                    Engine.GetShapeLinearProperties(shape),
                    Engine.GetShapeSurfaceProperties(shape),
                    Engine.GetShapeVolumeProperties(shape));
                _massAnalysisCache[key] = mass;
            }
            rows.Add(new(Local("Total Length", "总长度"), mass.Linear.Mass.ToString("G8", CultureInfo.InvariantCulture)));
            rows.Add(new(Local("Surface Area", "总面积"), mass.Surface.Mass.ToString("G8", CultureInfo.InvariantCulture)));
            rows.Add(new(Local("Volume", "总体积"), mass.Volume.Mass.ToString("G8", CultureInfo.InvariantCulture)));
            rows.Add(new(Local("Centroid", "重心"), mass.Volume.CenterOfMass.ToString() ?? ""));
        }
        catch (Exception ex)
        {
            rows.Add(new(Local("Mass Properties", "几何属性"), ex.Message));
        }

        try
        {
            if (!_topologyAnalysisCache.TryGetValue(key, out var topo))
            {
                topo = new TopologyAnalysis(
                    Engine.GetTopologyCount(shape, OcctShapeType.Vertex),
                    Engine.GetTopologyCount(shape, OcctShapeType.Edge),
                    Engine.GetTopologyCount(shape, OcctShapeType.Wire),
                    Engine.GetTopologyCount(shape, OcctShapeType.Face),
                    Engine.GetTopologyCount(shape, OcctShapeType.Shell),
                    Engine.GetTopologyCount(shape, OcctShapeType.Solid));
                _topologyAnalysisCache[key] = topo;
            }
            rows.Add(new(Local("Vertices / Edges / Faces", "顶点/边/面"),
                $"{topo.Vertices} / {topo.Edges} / {topo.Faces}"));
            rows.Add(new(Local("Wires / Shells / Solids", "线框/壳/实体"),
                $"{topo.Wires} / {topo.Shells} / {topo.Solids}"));
        }
        catch (Exception ex)
        {
            rows.Add(new(Local("Topology", "拓扑"), ex.Message));
        }

        return rows;
    }
}
