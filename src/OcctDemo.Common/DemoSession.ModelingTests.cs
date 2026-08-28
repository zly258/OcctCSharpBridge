using System.Drawing;
using OcctNet;

namespace OcctDemo.Common;

public sealed partial class DemoSession
{
    private const string GeometryInspectionTestId = "geometry-inspection";
    private const string GeometryAlgorithmsTestId = "geometry-algorithms";
    private const string BSplineSurfaceTestId = "bspline-surface";
    private const string MeshGenerationTestId = "mesh-generation";
    private const string PipeShellTestId = "pipe-shell";
    private const string TransformCopyTestId = "transform-copy";
    private const string ShapeValidityTestId = "shape-validity";

    public DemoCommandResult RunGeometryInspectionTest() => ExecuteModelingTest(GeometryInspectionTestId);

    public DemoCommandResult RunGeometryAlgorithmsTest() => ExecuteModelingTest(GeometryAlgorithmsTestId);

    public DemoCommandResult RunBSplineSurfaceTest() => ExecuteModelingTest(BSplineSurfaceTestId);

    public DemoCommandResult RunMeshGenerationTest() => ExecuteModelingTest(MeshGenerationTestId);

    public DemoCommandResult RunPipeShellTest() => ExecuteModelingTest(PipeShellTestId);
    public DemoCommandResult RunTransformCopyTest() => ExecuteModelingTest(TransformCopyTestId);
    public DemoCommandResult RunShapeValidityTest() => ExecuteModelingTest(ShapeValidityTestId);

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
                    GeometryInspectionTestId => CreateGeometryInspectionTest(),
                    GeometryAlgorithmsTestId => CreateGeometryAlgorithmsTest(),
                    BSplineSurfaceTestId => CreateBSplineSurfaceTest(),
                    MeshGenerationTestId => CreateMeshGenerationTest(),
                    PipeShellTestId => CreatePipeShellTest(),
                    TransformCopyTestId => CreateTransformCopyTest(),
                    ShapeValidityTestId => CreateShapeValidityTest(),
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

    private DemoCommandResult CreateGeometryInspectionTest()
    {
        using var model = new OcctModelingSession();

        var line = model.MakeLine(new OcctPoint3d(-40, 0, 0), new OcctPoint3d(40, 0, 0));
        var circle = model.MakeCircle(OcctPoint3d.Origin, OcctVector3d.UnitZ, 25);
        var ellipse = model.MakeEllipse(new OcctPoint3d(0, 70, 0), OcctVector3d.UnitZ, 35, 18);
        var bezier = model.MakeBezier(new[]
        {
            new OcctPoint3d(-40, -60, 0),
            new OcctPoint3d(-10, -20, 25),
            new OcctPoint3d(20, -90, -15),
            new OcctPoint3d(55, -45, 0)
        });
        var bspline = model.MakeInterpolatedBSpline(new[]
        {
            new OcctPoint3d(-60, 110, 0),
            new OcctPoint3d(-20, 135, 15),
            new OcctPoint3d(20, 95, -10),
            new OcctPoint3d(60, 125, 0)
        });

        var cylinder = model.MakeCylinder(new OcctPoint3d(110, 0, 0), OcctVector3d.UnitZ, 25, 60);
        var sphere = model.MakeSphere(new OcctPoint3d(110, 90, 30), 30);

        _ = model.GetLineGeometry(line);
        _ = model.GetCircleGeometry(circle);
        _ = model.GetEllipseGeometry(ellipse);

        var bezierData = model.GetBezierCurveData(bezier);
        var bsplineData = model.GetBSplineCurveData(bspline);
        if (bezierData.Poles.Count < 2 || bsplineData.Poles.Count < 2 || bsplineData.Knots.Count < 2)
            throw new InvalidOperationException(Local(
                "Free-form curve inspection returned incomplete data.",
                "自由曲线读取返回的数据不完整。"));

        var cylinderFace = model.GetFaces(cylinder)
            .First(face => model.GetFaceSurfaceType(face) == OcctSurfaceType.Cylinder);
        var sphereFace = model.GetFaces(sphere)
            .First(face => model.GetFaceSurfaceType(face) == OcctSurfaceType.Sphere);
        _ = model.GetCylinderGeometry(cylinderFace);
        _ = model.GetSphereGeometry(sphereFace);

        var displayedBezier = DisplayModelShape(model, bezier);
        var displayedBSpline = DisplayModelShape(model, bspline);
        var displayedCylinder = DisplayModelShape(model, cylinderFace);
        var displayedSphere = DisplayModelShape(model, sphereFace);
        SetGeneratedName(displayedBezier, Local("Bezier Inspection", "Bezier 读取"));
        SetGeneratedName(displayedBSpline, Local("B-Spline Inspection", "B-Spline 读取"));
        SetGeneratedName(displayedCylinder, Local("Cylinder Inspection", "圆柱面读取"));
        SetGeneratedName(displayedSphere, Local("Sphere Inspection", "球面读取"));

        return new DemoCommandResult(
            Local("Geometry inspection test completed.", "几何读取测试完成。"),
            new IOcctObject[] { displayedBezier, displayedBSpline, displayedCylinder, displayedSphere },
            Local(
                $"Geometry inspection passed: analytic curves, Bezier ({bezierData.Poles.Count} poles), B-Spline ({bsplineData.Poles.Count} poles / {bsplineData.Knots.Count} knots), cylinder and sphere.",
                $"几何读取测试通过：解析曲线、Bezier（{bezierData.Poles.Count} 个控制点）、B-Spline（{bsplineData.Poles.Count} 个控制点 / {bsplineData.Knots.Count} 个节点）、圆柱面与球面。"));
    }

    private DemoCommandResult CreateGeometryAlgorithmsTest()
    {
        using var model = new OcctModelingSession();

        var firstLine = model.MakeLine(new OcctPoint3d(-60, 0, 0), new OcctPoint3d(60, 0, 0));
        var secondLine = model.MakeLine(new OcctPoint3d(0, -60, 10), new OcctPoint3d(0, 60, 10));
        var edgeExtrema = model.GetEdgeExtrema(firstLine, secondLine);
        if (edgeExtrema.Count == 0)
            throw new InvalidOperationException(Local(
                "Curve/curve extrema returned no result.",
                "Curve/Curve Extrema 未返回结果。"));

        var firstSphere = model.MakeSphere(new OcctPoint3d(0, 0, 0), 50);
        var secondSphere = model.MakeSphere(new OcctPoint3d(55, 0, 0), 45);
        var firstFace = model.GetFaces(firstSphere)
            .First(face => model.GetFaceSurfaceType(face) == OcctSurfaceType.Sphere);
        var secondFace = model.GetFaces(secondSphere)
            .First(face => model.GetFaceSurfaceType(face) == OcctSurfaceType.Sphere);

        var lineThroughSphere = model.MakeLine(new OcctPoint3d(-80, 0, 0), new OcctPoint3d(80, 0, 0));
        var edgeFaceExtrema = model.GetEdgeFaceExtrema(lineThroughSphere, firstFace);
        var faceExtrema = model.GetFaceExtrema(firstFace, secondFace);
        var edgeFaceIntersections = model.IntersectEdgeFace(lineThroughSphere, firstFace);
        var surfaceIntersection = model.IntersectSurfaces(firstFace, secondFace);

        var equator = model.MakeCircle(OcctPoint3d.Origin, OcctVector3d.UnitZ, 50);
        var tangential = model.IntersectEdgeFace(equator, firstFace);
        var overlapCount = tangential.Count(item => item.Kind == OcctIntersectionKind.Overlap);

        if (edgeFaceExtrema.Count == 0 ||
            faceExtrema.Count == 0 ||
            edgeFaceIntersections.Count < 2 ||
            !surfaceIntersection.IsValid ||
            overlapCount == 0)
        {
            throw new InvalidOperationException(Local(
                "Geometry algorithm validation returned incomplete extrema/intersection results.",
                "Geometry Algorithm 验证返回的 Extrema/Intersection 结果不完整。"));
        }

        var displayedIntersection = DisplayModelShape(model, surfaceIntersection);
        SetGeneratedName(displayedIntersection, Local("Surface Intersection", "曲面交线"));

        return new DemoCommandResult(
            Local("Geometry algorithms test completed.", "几何算法测试完成。"),
            new IOcctObject[] { displayedIntersection },
            Local(
                $"Geometry algorithms passed: curve/curve extrema {edgeExtrema.Count}, curve/surface extrema {edgeFaceExtrema.Count}, surface/surface extrema {faceExtrema.Count}, edge/face points {edgeFaceIntersections.Count}, tangential overlaps {overlapCount}, surface intersection valid.",
                $"几何算法测试通过：Curve/Curve Extrema {edgeExtrema.Count}，Curve/Surface Extrema {edgeFaceExtrema.Count}，Surface/Surface Extrema {faceExtrema.Count}，Edge/Face 交点 {edgeFaceIntersections.Count}，切向 Overlap {overlapCount}，Surface Intersection 有效。"));
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
        var parameters = new OcctModelMeshParameters
        {
            LinearDeflection = 0.5,
            AngularDeflection = 0.5,
            MinimumSize = 0.01,
            Relative = false,
            Parallel = false,
            InternalVertices = true,
            ControlSurfaceDeflection = true
        };
        var mesh = model.GetShapeMeshData(source, parameters);

        ValidateMeshData(mesh);

        using var ownedMesh = model.CreateMeshResource(source, parameters);
        var directVertices = new OcctMeshVertex[ownedMesh.NodeCount];
        var directTriangles = new OcctModelMeshTriangle[ownedMesh.TriangleCount];
        if (ownedMesh.CopyVertices(directVertices) != directVertices.Length ||
            ownedMesh.CopyTriangles(directTriangles) != directTriangles.Length ||
            directVertices.Length == 0 ||
            !directVertices[0].Point.IsFinite)
        {
            throw new InvalidOperationException(Local(
                "Direct mesh buffer copy returned inconsistent data.",
                "网格直接缓冲区复制返回了不一致的数据。"));
        }

        var firstFace = model.GetFaces(source).First();
        var (faceVertexCount, faceTriangleCount) = model.GetFaceMeshCounts(firstFace);
        var faceVertices = new OcctMeshVertex[faceVertexCount];
        var faceTriangles = new OcctModelMeshTriangle[faceTriangleCount];
        var written = model.CopyFaceMesh(firstFace, faceVertices, faceTriangles);
        if (written.VerticesWritten != faceVertexCount ||
            written.TrianglesWritten != faceTriangleCount)
        {
            throw new InvalidOperationException(Local(
                "Direct face mesh buffer copy returned inconsistent data.",
                "面网格直接缓冲区复制返回了不一致的数据。"));
        }

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
            $"Mesh generation test passed: faces {mesh.FaceCount}, nodes {mesh.NodeCount}, triangles {mesh.TriangleCount}, provenance ranges {mesh.FaceRanges.Count}; owned mesh and direct Span copies also passed. The viewport displays the actual triangle connectivity returned by GetShapeMeshData.",
            $"网格生成测试通过：面 {mesh.FaceCount}，节点 {mesh.NodeCount}，三角形 {mesh.TriangleCount}，面来源区间 {mesh.FaceRanges.Count}；独立网格资源和 Span 直接复制也已通过。视口显示的是 GetShapeMeshData 实际返回的三角形连接关系。" );

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

    private DemoCommandResult CreateTransformCopyTest()
    {
        var source = Engine.MakeBox(40, 30, 20, -20, -15, 0);
        var sourceBounds = Engine.GetShapeBounds(source);
        var copy = Engine.Copy(source, false);
        var moved = Engine.Translate(copy, new OcctVector3d(100, 25, 12), true);
        var movedBounds = Engine.GetShapeBounds(moved);

        const double tolerance = 1e-7;
        if (Math.Abs(sourceBounds.SizeX - movedBounds.SizeX) > tolerance ||
            Math.Abs(sourceBounds.SizeY - movedBounds.SizeY) > tolerance ||
            Math.Abs(sourceBounds.SizeZ - movedBounds.SizeZ) > tolerance)
            throw new InvalidOperationException(Local("Copy/translate changed the source extents.", "复制/平移后外形尺寸发生变化。"));

        if (!Engine.IsShapeValid(source) || !Engine.IsShapeValid(moved))
            throw new InvalidOperationException(Local("Copy/transform produced an invalid shape.", "复制/变换生成了无效形体。"));

        SetGeneratedName(source, Local("Transform Source", "变换源"));
        SetGeneratedName(moved, Local("Translated Copy", "平移副本"));
        return new(Local("Copy and transform validation completed.", "复制与变换验证完成。"),
            new IOcctObject[] { source, moved },
            Local("Copy preserves geometry and translated extents remain identical.", "复制保持几何一致，平移后的包围盒尺寸保持不变。"));
    }

    private DemoCommandResult CreateShapeValidityTest()
    {
        var body = Engine.MakeBox(120, 80, 40, -60, -40, 0);
        var boss = Engine.MakeCylinder(new OcctPoint3d(0, 0, 40), OcctVector3d.UnitZ, 24, 35);
        body = Engine.Fuse(body, boss, true);
        body = Engine.DrillHole(body, new OcctPoint3d(0, 0, -1), OcctVector3d.UnitZ, 12, 78, true);

        if (!Engine.IsShapeValid(body))
            throw new InvalidOperationException(Local("Boolean/drill result failed BRep validation.", "布尔/钻孔结果未通过 BRep 有效性检查。"));

        var solids = Engine.GetTopologyCount(body, OcctShapeType.Solid);
        var faces = Engine.GetTopologyCount(body, OcctShapeType.Face);
        if (solids < 1 || faces < 6)
            throw new InvalidOperationException(Local("Validated shape has unexpected topology.", "验证后的形体拓扑数量异常。"));

        SetGeneratedName(body, Local("Validated Boolean Body", "有效布尔实体"));
        return new(Local("BRep validity test completed.", "BRep 有效性测试完成。"),
            new IOcctObject[] { body },
            Local($"Valid shape: solids {solids}, faces {faces}.", $"形体有效：实体 {solids}，面 {faces}。"));
    }

    private static string GetModelingTestDescription(string testId) => testId switch
    {
        GeometryInspectionTestId => Local("Geometry Inspection Test", "几何读取测试"),
        GeometryAlgorithmsTestId => Local("Geometry Algorithms Test", "几何算法测试"),
        BSplineSurfaceTestId => Local("B-Spline Surface Test", "B 样条曲面测试"),
        MeshGenerationTestId => Local("Mesh Generation Test", "网格生成测试"),
        PipeShellTestId => Local("PipeShell Sweep Test", "PipeShell 高级扫掠测试"),
        TransformCopyTestId => Local("Copy / Transform Test", "复制 / 变换测试"),
        ShapeValidityTestId => Local("BRep Validity Test", "BRep 有效性测试"),
        _ => testId
    };
}
