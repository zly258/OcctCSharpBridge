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
            Engine.SetObjectColor(shape, Color.SteelBlue);
            objects.Add(shape);
        }
        if (split.Negative is { } negative)
        {
            var shape = DisplayModelShape(model, negative);
            SetGeneratedName(shape, Local("Section Negative", "截面负侧"));
            Engine.SetObjectColor(shape, Color.SandyBrown);
            objects.Add(shape);
        }
        if (split.Section is { } section)
        {
            var shape = DisplayModelShape(model, section);
            SetGeneratedName(shape, Local("Section Curve", "截交线"));
            Engine.SetObjectColor(shape, Color.DarkRed);
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
        var source = model.MakeBox(100, 70, 55, -50, -35, 0);
        var objects = new List<IOcctObject>();

        AddHlrProjection(model, source, OcctHlrProjection.Front, new OcctVector3d(-140, 90, 0), "Front", objects);
        AddHlrProjection(model, source, OcctHlrProjection.Top, new OcctVector3d(20, 90, 0), "Top", objects);
        AddHlrProjection(model, source, OcctHlrProjection.Right, new OcctVector3d(-140, -70, 0), "Right", objects);
        AddHlrProjection(model, source, OcctHlrProjection.Isometric, new OcctVector3d(20, -70, 0), "Isometric", objects);

        Engine.FitAll();
        ActiveObject = objects.LastOrDefault();
        return new DemoCommandResult(
            Local("Four HLR engineering projections created.", "已生成四个 HLR 工程投影。"),
            objects,
            Local("Front / Top / Right / Isometric; visible lines are dark, hidden lines are gray, outlines are emphasized.",
                  "前视 / 俯视 / 右视 / 轴测；可见线为深色，隐藏线为灰色，轮廓线加粗显示。"));
    }

    private DemoCommandResult DemoDrawingProjection()
    {
        using var model = new OcctModelingSession();
        var source = model.MakeBox(100, 70, 55, -50, -35, 0);
        var objects = new List<IOcctObject>();

        AddHlrProjection(model, source, OcctHlrProjection.Front, new OcctVector3d(-140, 90, 0), "Front", objects);
        AddHlrProjection(model, source, OcctHlrProjection.Top, new OcctVector3d(20, 90, 0), "Top", objects);
        AddHlrProjection(model, source, OcctHlrProjection.Right, new OcctVector3d(-140, -70, 0), "Right", objects);
        AddHlrProjection(model, source, OcctHlrProjection.Isometric, new OcctVector3d(20, -70, 0), "Isometric", objects);

        Engine.FitAll();
        ActiveObject = objects.LastOrDefault();
        return new DemoCommandResult(
            Local("Four HLR engineering projections created.", "已生成四个 HLR 工程投影。"),
            objects,
            Local("Front / Top / Right / Isometric; visible lines are dark, hidden lines are gray, outlines are emphasized.",
                  "前视 / 俯视 / 右视 / 轴测；可见线为深色，隐藏线为灰色，轮廓线加粗显示。"));
    }

    private void AddHlrProjection(
        OcctModelingSession model,
        OcctModelShape source,
        OcctHlrProjection projection,
        OcctVector3d offset,
        string name,
        ICollection<IOcctObject> objects)
    {
        var result = model.ProjectHlr(source, projection);
        Add(result.VisibleLines, Color.Black, 1.4, "Visible");
        Add(result.HiddenLines, Color.Gray, 1.0, "Hidden");
        Add(result.Outlines, Color.DarkBlue, 2.2, "Outline");

        void Add(OcctModelShape? value, Color color, double width, string suffix)
        {
            if (value is not { } shape) return;
            var moved = model.Translate(shape, offset);
            var displayed = DisplayModelShape(model, moved);
            SetGeneratedName(displayed, $"{name} {suffix}");
            Engine.SetObjectColor(displayed, color);
            Engine.SetObjectLineWidth(displayed, width);
            objects.Add(displayed);
        }
    }

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
        Engine.SetObjectColor(firstView, Color.DarkBlue);
        Engine.SetObjectColor(secondView, Color.DarkGreen);

        var objects = new List<IOcctObject> { firstView, secondView };
        foreach (var item in extrema.Take(8))
        {
            if ((item.PointOnSecond - item.PointOnFirst).LengthSquared <= 1e-18) continue;
            var connector = Engine.MakeLine(item.PointOnFirst, item.PointOnSecond);
            SetGeneratedName(connector, Local("Extremum Distance", "极值距离"));
            Engine.SetObjectColor(connector, Color.DarkRed);
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
        Engine.SetObjectColor(beforeView, Color.IndianRed);
        Engine.SetObjectColor(afterView, Color.SeaGreen);

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
