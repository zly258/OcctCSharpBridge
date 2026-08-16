using System.Drawing;
using OcctNet;

namespace OcctDemo.Common;

public sealed partial class DemoSession
{
    public DemoCommandResult RunViewerProjectionTest()
    {
        var created = new List<IOcctObject>();
        string details;

        using (Engine.BeginDisplayBatch(fitAllOnDispose: true))
        {
            var edge = Engine.MakeLine(OcctPoint3d.Origin, new OcctPoint3d(100, 0, 0));
            SetGeneratedName(edge, Local("Projection Edge", "投影测试边"));
            Engine.SetObjectColor(edge, Color.SteelBlue);
            Engine.SetObjectLineWidth(edge, 2.0);
            created.Add(edge);

            var edgeSource = new OcctPoint3d(40, 25, 0);
            var edgeProjection = Engine.ProjectPointToEdge(edge, edgeSource);
            var edgeEvaluation = Engine.EvaluateEdge(edge, edgeProjection.NormalizedParameter);
            if (edgeProjection.Point.DistanceTo(new OcctPoint3d(40, 0, 0)) > 1e-6
                || Math.Abs(edgeProjection.NormalizedParameter - 0.4) > 1e-8
                || Math.Abs(edgeProjection.Distance - 25.0) > 1e-6
                || edgeEvaluation.Point.DistanceTo(edgeProjection.Point) > 1e-6)
            {
                throw new InvalidOperationException(Local(
                    "Point-to-edge projection validation failed.",
                    "点到边投影验证失败。"));
            }

            var edgePoint = Engine.MakeVertex(edgeProjection.Point);
            SetGeneratedName(edgePoint, Local("Projected Edge Point", "边投影点"));
            Engine.SetObjectColor(edgePoint, Color.OrangeRed);
            created.Add(edgePoint);

            var box = Engine.MakeBox(80, 60, 45, -40, -30, 20);
            SetGeneratedName(box, Local("Projection Box", "投影测试盒体"));
            Engine.SetObjectColor(box, Color.LightSteelBlue);
            Engine.SetObjectTransparency(box, 0.35);
            created.Add(box);

            var face = Engine.GetSubshapeAt(box, OcctShapeType.Face, 0);
            try
            {
                var uv = Engine.GetFaceUvBounds(face);
                var u = (uv.UMin + uv.UMax) * 0.5;
                var v = (uv.VMin + uv.VMax) * 0.5;
                var faceEvaluation = Engine.EvaluateFace(face, u, v);
                var faceSource = faceEvaluation.Point + faceEvaluation.Normal * 20.0;
                var faceProjection = Engine.ProjectPointToFace(face, faceSource);
                var projectedEvaluation = Engine.EvaluateFace(face, faceProjection.U, faceProjection.V);

                if (faceProjection.Point.DistanceTo(faceEvaluation.Point) > 1e-5
                    || Math.Abs(faceProjection.Distance - 20.0) > 1e-5
                    || projectedEvaluation.Point.DistanceTo(faceProjection.Point) > 1e-5)
                {
                    throw new InvalidOperationException(Local(
                        "Point-to-face projection validation failed.",
                        "点到面投影验证失败。"));
                }

                var facePoint = Engine.MakeVertex(faceProjection.Point);
                SetGeneratedName(facePoint, Local("Projected Face Point", "面投影点"));
                Engine.SetObjectColor(facePoint, Color.DarkOrange);
                created.Add(facePoint);

                details = Local(
                    $"Viewer projection test passed. Edge: point ({edgeProjection.Point.X:F3}, {edgeProjection.Point.Y:F3}, {edgeProjection.Point.Z:F3}), normalized parameter {edgeProjection.NormalizedParameter:F3}, distance {edgeProjection.Distance:F3}. Face: UV ({faceProjection.U:F3}, {faceProjection.V:F3}), distance {faceProjection.Distance:F3}. Both projected parameters evaluate back to the projected point.",
                    $"Viewer 投影测试通过。边：投影点 ({edgeProjection.Point.X:F3}, {edgeProjection.Point.Y:F3}, {edgeProjection.Point.Z:F3})，归一化参数 {edgeProjection.NormalizedParameter:F3}，距离 {edgeProjection.Distance:F3}。面：UV ({faceProjection.U:F3}, {faceProjection.V:F3})，距离 {faceProjection.Distance:F3}。边和面的投影参数均可回代到对应投影点。" );
            }
            finally
            {
                Engine.Delete(face);
            }
        }

        ActiveObject = created.LastOrDefault();
        IsModified = true;
        var result = new DemoCommandResult(
            Local("Viewer projection test completed.", "Viewer 投影测试完成。"),
            created,
            details);
        ModelChanged?.Invoke(this, EventArgs.Empty);
        StatusChanged?.Invoke(this, result.Message);
        return result;
    }
}
