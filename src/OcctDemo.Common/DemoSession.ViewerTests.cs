using OcctNet;

namespace OcctDemo.Common;

public sealed partial class DemoSession
{
    public DemoCommandResult RunViewerProjectionTest()
    {
        var temporary = new List<IOcctObject>();
        string details;

        using (Engine.BeginDisplayBatch())
        {
            try
            {
                var edge = Engine.MakeLine(OcctPoint3d.Origin, new OcctPoint3d(100, 0, 0));
                temporary.Add(edge);

                var edgeProjection = Engine.ProjectPointToEdge(edge, new OcctPoint3d(40, 25, 0));
                var edgeEvaluation = Engine.EvaluateEdge(edge, edgeProjection.NormalizedParameter);
                if (edgeProjection.Point.DistanceTo(new OcctPoint3d(40, 0, 0)) > 1e-6
                    || Math.Abs(edgeProjection.Tangent.X - 1.0) > 1e-8
                    || Math.Abs(edgeProjection.Tangent.Y) > 1e-8
                    || Math.Abs(edgeProjection.Tangent.Z) > 1e-8
                    || Math.Abs(edgeProjection.NormalizedParameter - 0.4) > 1e-8
                    || Math.Abs(edgeProjection.Distance - 25.0) > 1e-6
                    || edgeEvaluation.Point.DistanceTo(edgeProjection.Point) > 1e-6
                    || Math.Abs(edgeEvaluation.Tangent.X - edgeProjection.Tangent.X) > 1e-8
                    || Math.Abs(edgeEvaluation.Tangent.Y - edgeProjection.Tangent.Y) > 1e-8
                    || Math.Abs(edgeEvaluation.Tangent.Z - edgeProjection.Tangent.Z) > 1e-8)
                {
                    throw new InvalidOperationException(Local(
                        "Point-to-edge projection/tangent validation failed.",
                        "点到边投影/切线验证失败。"));
                }

                var endpointProjection = Engine.ProjectPointToEdge(edge, new OcctPoint3d(150, 10, 0));
                if (endpointProjection.Point.DistanceTo(new OcctPoint3d(100, 0, 0)) > 1e-6
                    || Math.Abs(endpointProjection.NormalizedParameter - 1.0) > 1e-8)
                {
                    throw new InvalidOperationException(Local(
                        "Point-to-edge projection did not respect the trimmed endpoint.",
                        "点到边投影没有正确遵守裁剪边端点。"));
                }

                var box = Engine.MakeBox(80, 60, 45, -40, -30, 20);
                temporary.Add(box);
                var face = Engine.GetSubshapeAt(box, OcctShapeType.Face, 0);
                temporary.Add(face);

                var uv = Engine.GetFaceUvBounds(face);
                var u = (uv.UMin + uv.UMax) * 0.5;
                var v = (uv.VMin + uv.VMax) * 0.5;
                var faceEvaluation = Engine.EvaluateFace(face, u, v);
                var faceSource = faceEvaluation.Point + faceEvaluation.Normal * 20.0;
                var faceProjection = Engine.ProjectPointToFace(face, faceSource);
                var projectedEvaluation = Engine.EvaluateFace(face, faceProjection.U, faceProjection.V);

                if (faceProjection.Point.DistanceTo(faceEvaluation.Point) > 1e-5
                    || Math.Abs(faceProjection.Normal.X - faceEvaluation.Normal.X) > 1e-8
                    || Math.Abs(faceProjection.Normal.Y - faceEvaluation.Normal.Y) > 1e-8
                    || Math.Abs(faceProjection.Normal.Z - faceEvaluation.Normal.Z) > 1e-8
                    || Math.Abs(faceProjection.Distance - 20.0) > 1e-5
                    || projectedEvaluation.Point.DistanceTo(faceProjection.Point) > 1e-5
                    || Math.Abs(projectedEvaluation.Normal.X - faceProjection.Normal.X) > 1e-8
                    || Math.Abs(projectedEvaluation.Normal.Y - faceProjection.Normal.Y) > 1e-8
                    || Math.Abs(projectedEvaluation.Normal.Z - faceProjection.Normal.Z) > 1e-8)
                {
                    throw new InvalidOperationException(Local(
                        "Point-to-face projection/normal validation failed.",
                        "点到面投影/法向验证失败。"));
                }

                details = Local(
                    $"Viewer projection test passed. Edge: point ({edgeProjection.Point.X:F3}, {edgeProjection.Point.Y:F3}, {edgeProjection.Point.Z:F3}), tangent ({edgeProjection.Tangent.X:F3}, {edgeProjection.Tangent.Y:F3}, {edgeProjection.Tangent.Z:F3}), normalized parameter {edgeProjection.NormalizedParameter:F3}, distance {edgeProjection.Distance:F3}; trimmed endpoint parameter {endpointProjection.NormalizedParameter:F3}. Face: UV ({faceProjection.U:F3}, {faceProjection.V:F3}), normal ({faceProjection.Normal.X:F3}, {faceProjection.Normal.Y:F3}, {faceProjection.Normal.Z:F3}), distance {faceProjection.Distance:F3}. Point/direction and parameter/UV round trips passed. Temporary test geometry was removed after validation.",
                    $"Viewer 投影测试通过。边：投影点 ({edgeProjection.Point.X:F3}, {edgeProjection.Point.Y:F3}, {edgeProjection.Point.Z:F3})，切线 ({edgeProjection.Tangent.X:F3}, {edgeProjection.Tangent.Y:F3}, {edgeProjection.Tangent.Z:F3})，归一化参数 {edgeProjection.NormalizedParameter:F3}，距离 {edgeProjection.Distance:F3}；裁剪端点参数 {endpointProjection.NormalizedParameter:F3}。面：UV ({faceProjection.U:F3}, {faceProjection.V:F3})，法向 ({faceProjection.Normal.X:F3}, {faceProjection.Normal.Y:F3}, {faceProjection.Normal.Z:F3})，距离 {faceProjection.Distance:F3}。点/方向和 Parameter/UV 回代均通过，测试临时几何已清理。" );
            }
            finally
            {
                for (var index = temporary.Count - 1; index >= 0; index--)
                {
                    var value = temporary[index];
                    if (Engine.ContainsObject(value.Id)) Engine.Delete(value);
                }
            }
        }

        var result = new DemoCommandResult(
            Local("Viewer projection test completed.", "Viewer 投影测试完成。"),
            Array.Empty<IOcctObject>(),
            details);
        StatusChanged?.Invoke(this, result.Message);
        return result;
    }
}
