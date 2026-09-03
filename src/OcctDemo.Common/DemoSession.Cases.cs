using System.Drawing;
using OcctNet;

namespace OcctDemo.Common;

public sealed partial class DemoSession
{
    private DemoCommandResult DemoSectionAnalysis(DemoValues values)
    {
        using var model = new OcctModelingSession();
        var source = model.MakeBox(140, 100, 80, -70, -50, 0);
        var offset = values.Number("offset", 30);
        var split = model.SplitByPlane(source, new OcctPlane3d(new OcctPoint3d(0, 0, offset), OcctVector3d.UnitZ));

        var objects = new List<IOcctObject>();
        if (split.Positive is { } positive)
        {
            var shape = DisplayModelShape(model, positive);
            SetGeneratedName(shape, Local("Section Positive", "截面正侧"));
            SetObjectColor(shape, Color.SteelBlue);
            objects.Add(shape);
        }
        if (split.Negative is { } negative)
        {
            var shape = DisplayModelShape(model, negative);
            SetGeneratedName(shape, Local("Section Negative", "截面负侧"));
            SetObjectColor(shape, Color.SandyBrown);
            objects.Add(shape);
        }
        if (split.Section is { } section)
        {
            var shape = DisplayModelShape(model, section);
            SetGeneratedName(shape, Local("Section Curve", "截交线"));
            SetObjectColor(shape, Color.DarkRed);
            Engine.SetObjectLineWidth(shape, 3.0);
            objects.Add(shape);
        }

        Engine.FitAll();
        ActiveObject = objects.LastOrDefault();
        return new DemoCommandResult(
            Local("Section analysis completed.", "截面分析完成。"),
            objects,
            Local($"Plane Z = {offset:G6}; positive={split.Positive.HasValue}, negative={split.Negative.HasValue}, section={split.Section.HasValue}.",
                  $"截面 Z = {offset:G6}；正侧={split.Positive.HasValue}，负侧={split.Negative.HasValue}，截交={split.Section.HasValue}。"));
    }

    private DemoCommandResult DemoDrawingProjection()
    {
        using var model = new OcctModelingSession();

        var plate = model.MakeBox(120, 80, 24, -60, -40, 0);
        var boss = model.MakeCylinder(new OcctPoint3d(24, 8, 24), OcctVector3d.UnitZ, 18, 28);
        var body = model.Fuse(plate, boss).Shape;

        var verticalHole = model.MakeCylinder(new OcctPoint3d(24, 8, -4), OcctVector3d.UnitZ, 7, 64);
        body = model.Cut(body, verticalHole).Shape;

        var crossHole = model.MakeCylinder(new OcctPoint3d(-70, -18, 12), OcctVector3d.UnitX, 6, 140);
        body = model.Cut(body, crossHole).Shape;

        if (!model.IsShapeValid(body))
            throw new InvalidOperationException(Local(
                "The engineering projection source model is invalid.",
                "工程投影源模型无效。"));

        var objects = new List<IOcctObject>();
        var front = AddHlrProjection(model, body, OcctHlrProjection.Front, new OcctVector3d(-165, 105, 0), "Front", objects);
        var top = AddHlrProjection(model, body, OcctHlrProjection.Top, new OcctVector3d(35, 105, 0), "Top", objects);
        var right = AddHlrProjection(model, body, OcctHlrProjection.Right, new OcctVector3d(-165, -85, 0), "Right", objects);
        var iso = AddHlrProjection(model, body, OcctHlrProjection.Isometric, new OcctVector3d(35, -85, 0), "Isometric", objects);

        Engine.FitAll();
        ActiveObject = objects.LastOrDefault();

        return new DemoCommandResult(
            Local("Four HLR engineering projections created.", "已生成四个 HLR 工程投影。"),
            objects,
            Local(
                $"Front V/H/O/VS/HS={front.Visible}/{front.Hidden}/{front.Outline}/{front.VisibleSharp}/{front.HiddenSharp}; " +
                $"Top={top.Visible}/{top.Hidden}/{top.Outline}/{top.VisibleSharp}/{top.HiddenSharp}; " +
                $"Right={right.Visible}/{right.Hidden}/{right.Outline}/{right.VisibleSharp}/{right.HiddenSharp}; " +
                $"Isometric={iso.Visible}/{iso.Hidden}/{iso.Outline}/{iso.VisibleSharp}/{iso.HiddenSharp}.",
                $"前视 V/H/O/VS/HS={front.Visible}/{front.Hidden}/{front.Outline}/{front.VisibleSharp}/{front.HiddenSharp}；" +
                $"俯视={top.Visible}/{top.Hidden}/{top.Outline}/{top.VisibleSharp}/{top.HiddenSharp}；" +
                $"右视={right.Visible}/{right.Hidden}/{right.Outline}/{right.VisibleSharp}/{right.HiddenSharp}；" +
                $"轴测={iso.Visible}/{iso.Hidden}/{iso.Outline}/{iso.VisibleSharp}/{iso.HiddenSharp}。"));
    }

    private HlrViewStats AddHlrProjection(
        OcctModelingSession model,
        OcctModelShape source,
        OcctHlrProjection projection,
        OcctVector3d offset,
        string name,
        ICollection<IOcctObject> objects)
    {
        var result = model.ProjectHlr(source, projection);
        var stats = new HlrViewStats(
            EdgeCount(result.VisibleLines),
            EdgeCount(result.HiddenLines),
            EdgeCount(result.Outlines),
            EdgeCount(result.VisibleSharpLines),
            EdgeCount(result.HiddenSharpLines));

        if (stats.Visible + stats.Outline + stats.VisibleSharp == 0)
            throw new InvalidOperationException(Local(
                $"HLR projection '{name}' returned no visible linework.",
                $"HLR 投影“{name}”没有返回任何可见线结果。"));

        Add(result.HiddenLines, stats.Hidden, Color.Gray, 0.9, "Hidden");
        Add(result.HiddenSharpLines, stats.HiddenSharp, Color.DimGray, 1.1, "Hidden Sharp");
        Add(result.VisibleLines, stats.Visible, Color.Black, 1.4, "Visible");
        Add(result.VisibleSharpLines, stats.VisibleSharp, Color.DarkGreen, 1.7, "Visible Sharp");
        Add(result.Outlines, stats.Outline, Color.DarkBlue, 2.2, "Outline");
        return stats;

        int EdgeCount(OcctModelShape? shape) =>
            shape is { } value ? model.GetTopologyCount(value, OcctShapeType.Edge) : 0;

        void Add(OcctModelShape? value, int edgeCount, Color color, double width, string suffix)
        {
            if (value is not { } shape || edgeCount == 0) return;
            // Keep the HLR TopoDS_Shape unchanged. OCCTBIM-Source displays the
            // HLR result directly as AIS_Shape; applying a BRep-level transform
            // before presentation can lose the special projected edge geometry.
            var displayed = DisplayModelShape(model, shape);
            Engine.SetLocalTransformation(
                displayed,
                OcctTransform3d.Translation(offset.X, offset.Y, offset.Z));
            SetGeneratedName(displayed, $"{name} {suffix}");

            Engine.SetObjectDisplayMode(displayed, OcctDisplayMode.Wireframe);
            SetObjectColor(displayed, color);
            Engine.SetObjectLineWidth(displayed, width);
            Engine.SetObjectLineStyle(
                displayed,
                suffix.StartsWith("Hidden", StringComparison.Ordinal)
                    ? OcctLineStyle.Dash
                    : OcctLineStyle.Solid);
            objects.Add(displayed);
        }
    }

    private readonly record struct HlrViewStats(
        int Visible,
        int Hidden,
        int Outline,
        int VisibleSharp,
        int HiddenSharp);

    private DemoCommandResult DemoDistanceExtrema()
    {
        using var model = new OcctModelingSession();
        var first = model.MakeLine(new OcctPoint3d(0, -70, 0), new OcctPoint3d(0, 70, 0));
        var second = model.MakeLine(new OcctPoint3d(-80, 0, 30), new OcctPoint3d(80, 0, 30));
        var extrema = model.GetEdgeExtrema(first, second).OrderBy(item => item.Distance).ToArray();

        var firstView = DisplayModelShape(model, first);
        var secondView = DisplayModelShape(model, second);
        SetGeneratedName(firstView, Local("Extrema Edge A", "极值边 A"));
        SetGeneratedName(secondView, Local("Extrema Edge B", "极值边 B"));
        SetObjectColor(firstView, Color.DarkBlue);
        SetObjectColor(secondView, Color.DarkGreen);

        var objects = new List<IOcctObject> { firstView, secondView };
        foreach (var item in extrema.Take(8))
        {
            if ((item.PointOnSecond - item.PointOnFirst).LengthSquared <= 1e-18) continue;
            var connector = Engine.MakeLine(item.PointOnFirst, item.PointOnSecond);
            SetGeneratedName(connector, Local("Extremum Distance", "极值距离"));
            SetObjectColor(connector, Color.DarkRed);
            objects.Add(connector);
        }

        Engine.FitAll();
        ActiveObject = firstView;
        var closest = extrema.FirstOrDefault();
        return new DemoCommandResult(
            Local("Distance and extrema analysis completed.", "距离与极值分析完成。"),
            objects,
            extrema.Length == 0
                ? Local("No extrema were returned.", "未返回极值结果。")
                : Local($"Extrema: {extrema.Length}; minimum distance: {closest.Distance:G8}; parameters: {closest.FirstParameter:G6}, {closest.SecondParameter:G6}.",
                        $"极值数量：{extrema.Length}；最小距离：{closest.Distance:G8}；参数：{closest.FirstParameter:G6}, {closest.SecondParameter:G6}。"));
    }

    private DemoCommandResult DemoModelRepair(DemoValues values)
    {
        using var model = new OcctModelingSession();
        var source = model.MakeBox(110, 75, 55, -55, -37.5, 0);
        var before = model.InspectShape(source);
        var repaired = model.FixShape(
            source,
            values.Number("precision", 1e-7),
            1e-7,
            values.Number("maxTolerance", 1.0));
        var after = model.InspectShape(repaired.Shape);

        var beforeView = DisplayModelShape(model, model.Translate(source, new OcctVector3d(-80, 0, 0)));
        var afterView = DisplayModelShape(model, model.Translate(repaired.Shape, new OcctVector3d(80, 0, 0)));
        SetGeneratedName(beforeView, Local("Before Repair", "修复前"));
        SetGeneratedName(afterView, Local("After Repair", "修复后"));
        SetObjectColor(beforeView, Color.IndianRed);
        SetObjectColor(afterView, Color.SeaGreen);

        Engine.FitAll();
        ActiveObject = afterView;
        return new DemoCommandResult(
            Local("Model repair pipeline completed.", "模型修复流程完成。"),
            new IOcctObject[] { beforeView, afterView },
            Local(
                $"Before: valid={before.IsValid}, max tolerance={before.MaximumTolerance:G6}, faces={before.FaceAnalysis.FaceCount}.\nAfter: valid={after.IsValid}, max tolerance={after.MaximumTolerance:G6}, faces={after.FaceAnalysis.FaceCount}.\nFixShape: succeeded={repaired.Succeeded}, warnings={repaired.HasWarnings}, errors={repaired.HasErrors}.\n{repaired.Report}",
                $"修复前：有效={before.IsValid}，最大容差={before.MaximumTolerance:G6}，面={before.FaceAnalysis.FaceCount}。\n修复后：有效={after.IsValid}，最大容差={after.MaximumTolerance:G6}，面={after.FaceAnalysis.FaceCount}。\nFixShape：成功={repaired.Succeeded}，警告={repaired.HasWarnings}，错误={repaired.HasErrors}。\n{repaired.Report}"));
    }
}
