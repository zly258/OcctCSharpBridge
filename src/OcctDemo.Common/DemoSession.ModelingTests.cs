using OcctNet;

namespace OcctDemo.Common;

public sealed partial class DemoSession
{
    public DemoCommandResult RunBSplineSurfaceTest()
    {
        // Keep the viewer sample history-aware while validating the same non-ruled loft
        // capability through the headless modeling API used by SDK consumers.
        var visibleResult = Execute(DemoCommandId.DemoLoft);

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

        var details = Local(
            $"B-Spline surface test passed: U degree {data.UDegree}, V degree {data.VDegree}, " +
            $"poles {data.UPoleCount} x {data.VPoleCount} ({data.PoleCount}), " +
            $"knots U/V {data.UKnotCount}/{data.VKnotCount}.",
            $"B 样条曲面测试通过：U 次数 {data.UDegree}，V 次数 {data.VDegree}，" +
            $"控制点 {data.UPoleCount} x {data.VPoleCount}（共 {data.PoleCount} 个），" +
            $"U/V 节点数 {data.UKnotCount}/{data.VKnotCount}。" );

        return visibleResult with
        {
            Message = Local("B-Spline surface test completed.", "B 样条曲面测试完成。"),
            AnalysisText = details
        };
    }

    public DemoCommandResult RunMeshGenerationTest()
    {
        // The first visible primitive is an 80 x 60 x 45 box. Validate that same
        // representative geometry through the modeling mesh/provenance API.
        var visibleResult = Execute(DemoCommandId.DemoPrimitives);

        using var model = new OcctModelingSession();
        var source = model.MakeBox(80, 60, 45, -220, -80, 0);
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

        if (mesh.FaceCount != 6 || mesh.NodeCount <= 0 || mesh.TriangleCount <= 0)
        {
            throw new InvalidOperationException(Local(
                "Mesh generation returned incomplete data.",
                "网格生成返回的数据不完整。"));
        }
        if (mesh.Nodes.Any(node => !node.Point.IsFinite))
        {
            throw new InvalidOperationException(Local(
                "Mesh generation returned a non-finite node.",
                "网格生成返回了非有限节点。"));
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

        var details = Local(
            $"Mesh generation test passed: faces {mesh.FaceCount}, nodes {mesh.NodeCount}, triangles {mesh.TriangleCount}, provenance ranges {mesh.FaceRanges.Count}.",
            $"网格生成测试通过：面 {mesh.FaceCount}，节点 {mesh.NodeCount}，三角形 {mesh.TriangleCount}，面来源区间 {mesh.FaceRanges.Count}。" );

        return visibleResult with
        {
            Message = Local("Mesh generation test completed.", "网格生成测试完成。"),
            AnalysisText = details
        };
    }
}