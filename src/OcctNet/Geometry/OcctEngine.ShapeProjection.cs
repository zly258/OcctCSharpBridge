namespace OcctNet;

public sealed partial class OcctEngine
{
    public OcctEdgeProjectionResult ProjectPointToEdge(OcctShape edge, OcctPoint3d sourcePoint)
    {
        EnsureShape(edge);
        if (!sourcePoint.IsFinite)
            throw new ArgumentOutOfRangeException(nameof(sourcePoint), "Projection source point must be finite.");
        EnsureInitialized();

        var status = ViewerShapeNativeMethods.occt_engine_shape_edge_project_point(
            _handle,
            edge.Id,
            sourcePoint,
            out var result);
        if (status != OcctStatus.Ok) throw CreateException();
        ValidateEdgeProjectionResult(result);
        return result;
    }

    public OcctFaceProjectionResult ProjectPointToFace(OcctShape face, OcctPoint3d sourcePoint)
    {
        EnsureShape(face);
        if (!sourcePoint.IsFinite)
            throw new ArgumentOutOfRangeException(nameof(sourcePoint), "Projection source point must be finite.");
        EnsureInitialized();

        var status = ViewerShapeNativeMethods.occt_engine_shape_face_project_point(
            _handle,
            face.Id,
            sourcePoint,
            out var result);
        if (status != OcctStatus.Ok) throw CreateException();
        ValidateFaceProjectionResult(result);
        return result;
    }

    private static void ValidateEdgeProjectionResult(OcctEdgeProjectionResult result)
    {
        if (!result.Point.IsFinite
            || !result.Tangent.IsFinite
            || result.Tangent.LengthSquared <= 1e-30
            || !double.IsFinite(result.NormalizedParameter)
            || !double.IsFinite(result.Distance)
            || result.Distance < 0.0
            || result.NormalizedParameter < 0.0
            || result.NormalizedParameter > 1.0)
        {
            throw new InvalidOperationException("Native edge projection returned invalid geometry.");
        }
    }

    private static void ValidateFaceProjectionResult(OcctFaceProjectionResult result)
    {
        if (!result.Point.IsFinite
            || !result.Normal.IsFinite
            || result.Normal.LengthSquared <= 1e-30
            || !double.IsFinite(result.U)
            || !double.IsFinite(result.V)
            || !double.IsFinite(result.Distance)
            || result.Distance < 0.0)
        {
            throw new InvalidOperationException("Native face projection returned invalid geometry.");
        }
    }
}
