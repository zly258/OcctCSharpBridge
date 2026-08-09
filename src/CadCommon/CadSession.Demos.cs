using System.Drawing;
using OcctNet;

namespace CadCommon;

public sealed partial class CadSession
{
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

}
