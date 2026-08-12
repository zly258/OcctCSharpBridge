using OcctNet;

namespace OcctDemo.Common;

public sealed partial class DemoSession
{
    private DemoCommandResult AnalyzeBounds()
    {
        var bounds = Engine.GetShapeBounds(RequireShape());
        var text = DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified
            ? $"最小点：({bounds.MinX:G8}, {bounds.MinY:G8}, {bounds.MinZ:G8})\n最大点：({bounds.MaxX:G8}, {bounds.MaxY:G8}, {bounds.MaxZ:G8})\n尺寸：{bounds.SizeX:G8} × {bounds.SizeY:G8} × {bounds.SizeZ:G8}\n中心：{bounds.Center}"
            : $"Minimum: ({bounds.MinX:G8}, {bounds.MinY:G8}, {bounds.MinZ:G8})\nMaximum: ({bounds.MaxX:G8}, {bounds.MaxY:G8}, {bounds.MaxZ:G8})\nSize: {bounds.SizeX:G8} × {bounds.SizeY:G8} × {bounds.SizeZ:G8}\nCenter: {bounds.Center}";
        return new(Local("Extents analysis completed.", "包围盒分析完成。"), Array.Empty<IOcctObject>(), text);
    }

    private DemoCommandResult AnalyzeMass()
    {
        var shape = RequireShape();
        var linear = Engine.GetShapeLinearProperties(shape);
        var surface = Engine.GetShapeSurfaceProperties(shape);
        var volume = Engine.GetShapeVolumeProperties(shape);
        var text = DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified
            ? $"总长度：{linear.Mass:G10}\n总面积：{surface.Mass:G10}\n总体积：{volume.Mass:G10}\n体积重心：{volume.CenterOfMass}"
            : $"Total Length: {linear.Mass:G10}\nSurface Area: {surface.Mass:G10}\nVolume: {volume.Mass:G10}\nCentroid: {volume.CenterOfMass}";
        return new(Local("Mass properties completed.", "几何属性分析完成。"), Array.Empty<IOcctObject>(), text);
    }

    private DemoCommandResult AnalyzeTopology()
    {
        var shape = RequireShape();
        var types = new[] { OcctShapeType.Vertex, OcctShapeType.Edge, OcctShapeType.Wire, OcctShapeType.Face, OcctShapeType.Shell, OcctShapeType.Solid };
        var text = string.Join(Environment.NewLine, types.Select(type => $"{type}: {Engine.GetTopologyCount(shape, type)}"));
        return new(Local("Topology statistics completed.", "拓扑统计完成。"), Array.Empty<IOcctObject>(), text);
    }

    private DemoCommandResult AnalyzeDistance()
    {
        var shapes = RequireShapes(2);
        var result = Engine.GetShapeDistance(shapes[0], shapes[1]);
        var text = DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified
            ? $"最短距离：{result.Distance:G10}\n对象 1 最近点：{result.PointOnFirst}\n对象 2 最近点：{result.PointOnSecond}"
            : $"Minimum Distance: {result.Distance:G10}\nClosest Point on Object 1: {result.PointOnFirst}\nClosest Point on Object 2: {result.PointOnSecond}";
        return new(Local("Minimum distance calculation completed.", "最短距离计算完成。"), Array.Empty<IOcctObject>(), text);
    }

    private DemoCommandResult ValidateShape()
    {
        var valid = Engine.IsShapeValid(RequireShape());
        return new(valid ? Local("Shape validation passed.", "形体检查通过。") : Local("Shape validation failed.", "形体检查未通过。"), Array.Empty<IOcctObject>(), valid ? Local("The shape passed BRepCheck validation.", "当前形体通过 BRepCheck 检查。") : Local("The shape contains invalid topology or geometry.", "当前形体存在无效拓扑或几何。"));
    }

}
