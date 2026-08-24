using System.Drawing;
using OcctNet;

namespace OcctDemo.Common;

public sealed partial class DemoSession
{
    private const string BSplineSurfaceTestId = "bspline-surface";
    private const string MeshGenerationTestId = "mesh-generation";
    private const string CurveFitTestId = "curve-fit";
    private const string PipeShellTestId = "pipe-shell";
    private const string EdgeIntersectionTestId = "edge-intersection";
    private const string ObjGltfExchangeTestId = "exchange-gltf-obj";

    public DemoCommandResult RunBSplineSurfaceTest() => ExecuteModelingTest(BSplineSurfaceTestId);

    public DemoCommandResult RunMeshGenerationTest() => ExecuteModelingTest(MeshGenerationTestId);

    public DemoCommandResult RunCurveFitTest() => ExecuteModelingTest(CurveFitTestId);

    public DemoCommandResult RunPipeShellTest() => ExecuteModelingTest(PipeShellTestId);

    public DemoCommandResult RunEdgeIntersectionTest() => ExecuteModelingTest(EdgeIntersectionTestId);

    public DemoCommandResult RunObjGltfExchangeTest() => ExecuteModelingTest(ObjGltfExchangeTestId);

    private DemoCommandResult ExecuteModelingTest(string testId)
    {
        var initialObjectIds = Engine.GetObjects().Select(item => item.Id).ToHashSet();
        DemoCommandResult result;

        using (Engine.BeginDisplayBatch(fitAllOnDispose: true))
        {
            try
            {
                result = testId switch
                {
                    BSplineSurfaceTestId => CreateBSplineSurfaceTest(),
                    MeshGenerationTestId => CreateMeshGenerationTest(),
                    CurveFitTestId => CreateCurveFitTest(),
                    PipeShellTestId => CreatePipeShellTest(),
                    EdgeIntersectionTestId => CreateEdgeIntersectionTest(),
                    ObjGltfExchangeTestId => CreateObjGltfExchangeTest(),
                    _ => throw new ArgumentOutOfRangeException(nameof(testId), testId, "Unknown modeling test.")
                };

                RemoveDemoProcessObjects(initialObjectIds, result.CreatedObjects);
            }
            catch
            {
                RemoveDemoProcessObjects(initialObjectIds, Array.Empty<IOcctObject>());
                throw;
            }
        }

        IsModified = true;
        if (!_restoringHistory && _historyAvailable)
        {
            TruncateRedoHistory();
            _history.Add(DemoHistoryEntry.ModelingTest(testId, GetModelingTestDescription(testId)));
            _historyPosition = _history.Count;
            HistoryChanged?.Invoke(this, EventArgs.Empty);
        }

        if (!_suppressNotifications)
        {
            ModelChanged?.Invoke(this, EventArgs.Empty);
            StatusChanged?.Invoke(this, result.Message);
        }

        return result;
    }

    private DemoCommandResult CreateBSplineSurfaceTest()
    {
        using var model = new OcctModelingSession();
        var sections = new[]
        {
            model.MakeRectangleWire(80, 50, new OcctPoint3d(0, 0, 0)),
            model.MakeRectangleWire(105, 65, new OcctPoint3d(-10, 8, 35)),
            model.MakeRectangleWire(70, 90, new OcctPoint3d(12, -6, 75)),
            model.MakeRectangleWire(95, 55, new OcctPoint3d(-5, 10, 115))
        };

        var loft = model.Loft(sections, makeSolid: false, ruled: false, tolerance: 1e-6).Shape;
        var bsplineFace = model.GetFaces(loft)
            .FirstOrDefault(face => model.GetFaceSurfaceType(face) == OcctSurfaceType.BSpline);
        if (!bsplineFace.IsValid)
        {
            throw new InvalidOperationException(Local(
                "The loft did not produce a B-Spline surface.",
                "放样结果没有生成 B 样条曲面。"));
        }

        var data = model.GetBSplineSurfaceData(bsplineFace);
        ValidateBSplineSurfaceData(data);

        var surface = DisplayModelShape(model, bsplineFace);
        SetGeneratedName(surface, Local("B-Spline Surface", "B 样条曲面"));
        Engine.SetObjectColor(surface, Color.SteelBlue);
        Engine.SetObjectTransparency(surface, 0.28);

        var controlCurves = new List<OcctShape>(data.UPoleCount + data.VPoleCount);
        for (var uIndex = 0; uIndex < data.UPoleCount; uIndex++)
        {
            var points = Enumerable.Range(0, data.VPoleCount)
                .Select(vIndex => data.GetPole(uIndex, vIndex))
                .ToArray();
            controlCurves.Add(Engine.MakePolyline(points));
        }
        for (var vIndex = 0; vIndex < data.VPoleCount; vIndex++)
        {
            var points = Enumerable.Range(0, data.UPoleCount)
                .Select(uIndex => data.GetPole(uIndex, vIndex))
                .ToArray();
            controlCurves.Add(Engine.MakePolyline(points));
        }

        var controlNet = Engine.MakeCompound(controlCurves, hideInputs: true);
        SetGeneratedName(controlNet, Local("B-Spline Control Net", "B 样条控制网"));
        Engine.SetObjectColor(controlNet, Color.DarkOrange);
        Engine.SetObjectLineWidth(controlNet, 1.8);

        ActiveObject = surface;

        var details = Local(
            $"B-Spline surface test passed: U degree {data.UDegree}, V degree {data.VDegree}, " +
            $"poles {data.UPoleCount} x {data.VPoleCount} ({data.PoleCount}), " +
            $"knots U/V {data.UKnotCount}/{data.VKnotCount}. The tested B-Spline face and its U/V control net are displayed.",
            $"B 样条曲面测试通过：U 次数 {data.UDegree}，V 次数 {data.VDegree}，" +
            $"控制点 {data.UPoleCount} x {data.VPoleCount}（共 {data.PoleCount} 个），" +
            $"U/V 节点数 {data.UKnotCount}/{data.VKnotCount}。视口显示的是实际被测试的 B 样条面及其 U/V 控制网。" );

        return new DemoCommandResult(
            Local("B-Spline surface test completed.", "B 样条曲面测试完成。"),
            new IOcctObject[] { surface, controlNet },
            details);
    }

    private DemoCommandResult CreateMeshGenerationTest()
    {
        using var model = new OcctModelingSession();
        var source = model.MakeBox(80, 60, 45, -40, -30, 0);
        var mesh = model.GetShapeMeshData(source, new OcctModelMeshParameters
        {
            LinearDeflection = 0.5,
            AngularDeflection = 0.5,
            MinimumSize = 0.01,
            Relative = false,
            Parallel = false,
            InternalVertices = true,
            ControlSurfaceDeflection = true
        });

        ValidateMeshData(mesh);

        var triangleEdges = new List<OcctShape>(mesh.TriangleCount);
        foreach (var triangle in mesh.Mesh.Triangles)
        {
            var points = new[]
            {
                mesh.Mesh.Nodes[triangle.Node1].Point,
                mesh.Mesh.Nodes[triangle.Node2].Point,
                mesh.Mesh.Nodes[triangle.Node3].Point
            };
            triangleEdges.Add(Engine.MakePolyline(points, closed: true));
        }

        var meshWireframe = Engine.MakeCompound(triangleEdges, hideInputs: true);
        SetGeneratedName(meshWireframe, Local("Triangulated Box Mesh", "盒体三角网格"));
        Engine.SetObjectColor(meshWireframe, Color.DarkSlateGray);
        Engine.SetObjectLineWidth(meshWireframe, 1.4);
        ActiveObject = meshWireframe;

        var details = Local(
            $"Mesh generation test passed: faces {mesh.FaceCount}, nodes {mesh.NodeCount}, triangles {mesh.TriangleCount}, provenance ranges {mesh.FaceRanges.Count}. The viewport displays the actual triangle connectivity returned by GetShapeMeshData.",
            $"网格生成测试通过：面 {mesh.FaceCount}，节点 {mesh.NodeCount}，三角形 {mesh.TriangleCount}，面来源区间 {mesh.FaceRanges.Count}。视口显示的是 GetShapeMeshData 实际返回的三角形连接关系。" );

        return new DemoCommandResult(
            Local("Mesh generation test completed.", "网格生成测试完成。"),
            new IOcctObject[] { meshWireframe },
            details);
    }

    private static void ValidateBSplineSurfaceData(OcctBSplineSurfaceData data)
    {
        if (data.UDegree < 1 || data.VDegree < 1 || data.UPoleCount < 2 || data.VPoleCount < 2)
        {
            throw new InvalidOperationException(Local(
                "The B-Spline surface metadata is invalid.",
                "B 样条曲面的次数或控制点数据无效。"));
        }
        if (data.PoleCount != data.Poles.Count || data.Poles.Count != data.Weights.Count)
        {
            throw new InvalidOperationException(Local(
                "The B-Spline surface pole grid is inconsistent.",
                "B 样条曲面的控制点网格数据不一致。"));
        }
        if (data.UKnots.Count != data.UMultiplicities.Count || data.VKnots.Count != data.VMultiplicities.Count)
        {
            throw new InvalidOperationException(Local(
                "The B-Spline knot and multiplicity data is inconsistent.",
                "B 样条曲面的节点与重复度数据不一致。"));
        }
        if (data.Poles.Any(point => !point.IsFinite))
        {
            throw new InvalidOperationException(Local(
                "The B-Spline surface contains a non-finite control point.",
                "B 样条曲面包含非有限控制点。"));
        }
        if (data.Weights.Any(weight => !double.IsFinite(weight) || weight <= 0))
        {
            throw new InvalidOperationException(Local(
                "The B-Spline surface contains an invalid weight.",
                "B 样条曲面包含无效权重。"));
        }
        for (var index = 1; index < data.UKnots.Count; index++)
        {
            if (data.UKnots[index] <= data.UKnots[index - 1])
            {
                throw new InvalidOperationException(Local(
                    "The B-Spline U knots are not strictly increasing.",
                    "B 样条曲面的 U 向节点不是严格递增。"));
            }
        }
        for (var index = 1; index < data.VKnots.Count; index++)
        {
            if (data.VKnots[index] <= data.VKnots[index - 1])
            {
                throw new InvalidOperationException(Local(
                    "The B-Spline V knots are not strictly increasing.",
                    "B 样条曲面的 V 向节点不是严格递增。"));
            }
        }

        var firstPole = data.GetPole(0, 0);
        var firstWeight = data.GetWeight(0, 0);
        if (!firstPole.IsFinite || !double.IsFinite(firstWeight) || firstWeight <= 0)
        {
            throw new InvalidOperationException(Local(
                "Indexed B-Spline pole access failed.",
                "B 样条曲面控制点索引访问失败。"));
        }
    }

    private static void ValidateMeshData(OcctShapeMeshData mesh)
    {
        if (mesh.FaceCount != 6 || mesh.NodeCount <= 0 || mesh.TriangleCount <= 0)
        {
            throw new InvalidOperationException(Local(
                "Mesh generation returned incomplete data.",
                "网格生成返回的数据不完整。"));
        }
        if (mesh.Mesh.Nodes.Any(node => !node.Point.IsFinite))
        {
            throw new InvalidOperationException(Local(
                "Mesh generation returned a non-finite node.",
                "网格生成返回了非有限节点。"));
        }
        foreach (var triangle in mesh.Mesh.Triangles)
        {
            if ((uint)triangle.Node1 >= (uint)mesh.NodeCount ||
                (uint)triangle.Node2 >= (uint)mesh.NodeCount ||
                (uint)triangle.Node3 >= (uint)mesh.NodeCount)
            {
                throw new InvalidOperationException(Local(
                    "Mesh generation returned an invalid triangle index.",
                    "网格生成返回了无效的三角形节点索引。"));
            }
        }

        var expectedNodeStart = 0;
        var expectedTriangleStart = 0;
        foreach (var range in mesh.FaceRanges)
        {
            if (!range.Face.IsValid || range.NodeCount <= 0 || range.TriangleCount <= 0)
            {
                throw new InvalidOperationException(Local(
                    "A mesh face range is invalid.",
                    "网格中存在无效的面来源区间。"));
            }
            if (range.NodeStart != expectedNodeStart || range.TriangleStart != expectedTriangleStart)
            {
                throw new InvalidOperationException(Local(
                    "Mesh provenance ranges are not contiguous.",
                    "网格面来源区间不连续。"));
            }
            expectedNodeStart = range.NodeEndExclusive;
            expectedTriangleStart = range.TriangleEndExclusive;
        }

        if (expectedNodeStart != mesh.NodeCount || expectedTriangleStart != mesh.TriangleCount)
        {
            throw new InvalidOperationException(Local(
                "Mesh provenance does not cover the complete mesh.",
                "网格面来源没有覆盖完整网格。"));
        }
    }

    private DemoCommandResult CreateCurveFitTest()
    {
        using var model = new OcctModelingSession();
        var points = new List<OcctPoint3d>();
        const int pointCount = 25;
        for (var i = 0; i < pointCount; i++)
        {
            var t = i / (double)(pointCount - 1);
            var angle = t * 2.5 * Math.PI;
            var r = 30 + 20 * t;
            var x = r * Math.Cos(angle);
            var y = r * Math.Sin(angle);
            var z = 80 * t;
            points.Add(new OcctPoint3d(x, y, z));
        }

        var fitEdge = model.FitBSplineCurve(points, degMin: 3, degMax: 6, continuity: OcctContinuity.C2, tolerance: 0.1);
        if (!fitEdge.IsValid)
        {
            throw new InvalidOperationException(Local("Curve fitting returned an invalid shape.", "曲线拟合返回了无效形体。"));
        }

        var fitShape = DisplayModelShape(model, fitEdge);
        SetGeneratedName(fitShape, Local("Fitted B-Spline Curve", "拟合 B 样条曲线"));
        Engine.SetObjectColor(fitShape, Color.Cyan);
        Engine.SetObjectLineWidth(fitShape, 2.5);

        var samplePoints = points.Select(p => Engine.MakeVertex(p)).Cast<OcctShape>().ToList();
        var pointsCompound = Engine.MakeCompound(samplePoints, hideInputs: true);
        SetGeneratedName(pointsCompound, Local("Curve Fit Sample Points", "曲线拟合采样点"));
        Engine.SetObjectColor(pointsCompound, Color.Yellow);

        ActiveObject = fitShape;

        var details = Local(
            $"B-Spline curve fit test passed: {points.Count} points fitted with C2 continuity and max degree 6. The fitted curve and sample points are displayed in viewport.",
            $"B 样条曲线拟合测试通过：{points.Count} 个采样点以 C2 连续度、最大 6 次完成拟合。视口已显示拟合曲线与采样点。");

        return new DemoCommandResult(
            Local("B-Spline curve fit test completed.", "B 样条曲线拟合测试完成。"),
            new IOcctObject[] { fitShape, pointsCompound },
            details);
    }

    private DemoCommandResult CreatePipeShellTest()
    {
        using var model = new OcctModelingSession();
        var spinePoints = new[]
        {
            new OcctPoint3d(0, 0, 0),
            new OcctPoint3d(0, 50, 40),
            new OcctPoint3d(60, 80, 80),
            new OcctPoint3d(120, 50, 120),
            new OcctPoint3d(120, 0, 160)
        };
        var spineEdge = model.MakeInterpolatedBSpline(spinePoints);
        var spineWire = model.MakeWire(new[] { spineEdge });

        var profileEdge = model.MakeCircle(OcctPoint3d.Origin, OcctVector3d.UnitY, 15);
        var profileWire = model.MakeWire(new[] { profileEdge });

        var sweepResult = model.SweepPipeShell(spineWire, new[] { profileWire }, OcctPipeShellMode.CorrectedFrenet, solid: true);
        if (!sweepResult.Shape.IsValid)
        {
            throw new InvalidOperationException(Local("PipeShell sweep failed to produce a valid shape.", "高级管道扫掠未能生成有效形体。"));
        }

        var sweptShape = DisplayModelShape(model, sweepResult.Shape);
        SetGeneratedName(sweptShape, Local("PipeShell Sweep Solid", "PipeShell 扫掠实体"));
        Engine.SetObjectColor(sweptShape, Color.MediumSeaGreen);
        Engine.SetObjectMaterial(sweptShape, OcctMaterial.Copper);

        ActiveObject = sweptShape;

        var details = Local(
            "PipeShell sweep test passed: constructed solid sweep using CorrectedFrenet trihedron along a 3D spline spine. The swept solid is displayed.",
            "PipeShell 扫掠测试通过：沿三维样条脊线使用 CorrectedFrenet 标架扫掠生成中实实体。视口已显示该扫掠模型。");

        return new DemoCommandResult(
            Local("PipeShell sweep test completed.", "PipeShell 扫掠测试完成。"),
            new IOcctObject[] { sweptShape },
            details);
    }

    private DemoCommandResult CreateEdgeIntersectionTest()
    {
        using var model = new OcctModelingSession();
        var edge1 = model.MakeLine(new OcctPoint3d(-60, 0, 10), new OcctPoint3d(60, 0, 10));
        var edge2 = model.MakeLine(new OcctPoint3d(0, -60, 10), new OcctPoint3d(0, 60, 10));

        var intersections = model.IntersectEdges(edge1, edge2, tolerance: 1e-6);
        if (intersections.Count == 0)
        {
            throw new InvalidOperationException(Local("Edge intersection test found no intersection point.", "边求交测试未检测到交点。"));
        }

        var intersectPoint = intersections[0].StartPoint;
        if (intersectPoint.DistanceTo(new OcctPoint3d(0, 0, 10)) > 1e-4)
        {
            throw new InvalidOperationException(Local("Intersection point coordinate verification failed.", "交点坐标验证失败。"));
        }

        var line1 = DisplayModelShape(model, edge1);
        var line2 = DisplayModelShape(model, edge2);
        SetGeneratedName(line1, Local("Intersection Line 1", "相交直线 1"));
        SetGeneratedName(line2, Local("Intersection Line 2", "相交直线 2"));
        Engine.SetObjectColor(line1, Color.OrangeRed);
        Engine.SetObjectColor(line2, Color.DeepSkyBlue);
        Engine.SetObjectLineWidth(line1, 2.0);
        Engine.SetObjectLineWidth(line2, 2.0);

        var pointMarker = Engine.MakeVertex(intersectPoint);
        SetGeneratedName(pointMarker, Local("Intersection Point", "求交交点"));
        Engine.SetObjectColor(pointMarker, Color.LimeGreen);

        var details = Local(
            $"Edge intersection test passed: found {intersections.Count} intersection at ({intersectPoint.X:F3}, {intersectPoint.Y:F3}, {intersectPoint.Z:F3}), parameters ({intersections[0].FirstParameterStart:F3}, {intersections[0].SecondParameterStart:F3}).",
            $"边求交测试通过：在 ({intersectPoint.X:F3}, {intersectPoint.Y:F3}, {intersectPoint.Z:F3}) 处检测到 {intersections.Count} 个交点，参数分别为 ({intersections[0].FirstParameterStart:F3}, {intersections[0].SecondParameterStart:F3})。");

        return new DemoCommandResult(
            Local("Edge intersection test completed.", "边求交测试完成。"),
            new IOcctObject[] { line1, line2, pointMarker },
            details);
    }

    private DemoCommandResult CreateObjGltfExchangeTest()
    {
        using var model = new OcctModelingSession();
        var box = model.MakeBox(50, 40, 30, -25, -20, 0);
        var cyl = model.MakeCylinder(new OcctPoint3d(0, 0, -5), OcctVector3d.UnitZ, 15, 40);
        var part = model.Cut(box, cyl).Shape;

        var tempDir = Path.Combine(Path.GetTempPath(), "OcctDemo_ExchangeTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        var objPath = Path.Combine(tempDir, "test_model.obj");
        var gltfPath = Path.Combine(tempDir, "test_model.glb");

        try
        {
            model.ExportObj(part, objPath);
            if (!File.Exists(objPath) || new FileInfo(objPath).Length == 0)
                throw new InvalidOperationException(Local("OBJ export produced an empty file.", "OBJ 导出生成了空文件。"));
            var importedObj = model.ImportObj(objPath);
            if (!importedObj.IsValid)
                throw new InvalidOperationException(Local("OBJ import produced an invalid shape.", "OBJ 导入生成了无效形体。"));

            model.ExportGltf(part, gltfPath, new OcctGltfExportOptions { WriteBinary = true, TransformToGltfCs = true });
            if (!File.Exists(gltfPath) || new FileInfo(gltfPath).Length == 0)
                throw new InvalidOperationException(Local("glTF export produced an empty file.", "glTF 导出生成了空文件。"));
            var importedGltf = model.ImportGltf(gltfPath);
            if (!importedGltf.IsValid)
                throw new InvalidOperationException(Local("glTF import produced an invalid shape.", "glTF 导入生成了无效形体。"));

            var displayed = DisplayModelShape(model, importedGltf);
            SetGeneratedName(displayed, Local("Imported glTF/OBJ Model", "导入的 glTF/OBJ 模型"));
            Engine.SetObjectColor(displayed, Color.CadetBlue);
            ActiveObject = displayed;

            var details = Local(
                $"OBJ and glTF exchange test passed: exported model to OBJ ({new FileInfo(objPath).Length} bytes) and glTF binary ({new FileInfo(gltfPath).Length} bytes), then successfully re-imported and validated shapes.",
                $"OBJ 与 glTF 数据交换测试通过：成功导出 OBJ（{new FileInfo(objPath).Length} 字节）与 glTF 格式（{new FileInfo(gltfPath).Length} 字节），并成功重新导入且完成形体完整性校验。");

            return new DemoCommandResult(
                Local("glTF and OBJ exchange test completed.", "glTF 与 OBJ 数据交换测试完成。"),
                new IOcctObject[] { displayed },
                details);
        }
        finally
        {
            try { Directory.Delete(tempDir, true); } catch { }
        }
    }

    private static string GetModelingTestDescription(string testId) => testId switch
    {
        BSplineSurfaceTestId => Local("B-Spline Surface Test", "B 样条曲面测试"),
        MeshGenerationTestId => Local("Mesh Generation Test", "网格生成测试"),
        CurveFitTestId => Local("B-Spline Curve Fit Test", "B 样条曲线拟合测试"),
        PipeShellTestId => Local("PipeShell Sweep Test", "PipeShell 高级扫掠测试"),
        EdgeIntersectionTestId => Local("Edge Intersection Test", "几何边求交测试"),
        ObjGltfExchangeTestId => Local("glTF and OBJ Exchange Test", "glTF 与 OBJ 交换测试"),
        _ => testId
    };
}
