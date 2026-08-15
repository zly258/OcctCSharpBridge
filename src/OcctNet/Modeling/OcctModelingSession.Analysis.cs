namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public OcctModelProjectionResult ProjectPointOnEdge(OcctModelShape edge, OcctPoint3d point)
    {
        EnsureShape(edge);
        CheckStatus(ModelNativeMethods.occt_model_project_point_on_edge(_handle, edge.Id, point, out var result));
        return result;
    }

    public OcctModelProjectionResult ProjectPointOnFace(OcctModelShape face, OcctPoint3d point)
    {
        EnsureShape(face);
        CheckStatus(ModelNativeMethods.occt_model_project_point_on_face(_handle, face.Id, point, out var result));
        return result;
    }

    public IReadOnlyList<OcctModelRayHit> IntersectRay(
        OcctModelShape shape,
        OcctPoint3d origin,
        OcctVector3d direction,
        double minimumParameter = 0,
        double maximumParameter = 1e12,
        double tolerance = 1e-7)
    {
        EnsureShape(shape);
        OcctGuard.NonZero(direction, nameof(direction));
        OcctGuard.Positive(tolerance, nameof(tolerance));
        if (!double.IsFinite(minimumParameter)) throw new ArgumentOutOfRangeException(nameof(minimumParameter));
        if (!double.IsFinite(maximumParameter) || maximumParameter < minimumParameter)
            throw new ArgumentOutOfRangeException(nameof(maximumParameter));

        var status = ModelNativeMethods.occt_model_ray_intersections(
            _handle,
            shape.Id,
            origin,
            direction,
            minimumParameter,
            maximumParameter,
            tolerance,
            out var count);
        CheckStatus(status);
        if (count == 0) return Array.Empty<OcctModelRayHit>();

        var native = new NativeModelRayHit[count];
        status = ModelNativeMethods.occt_model_ray_hits_snapshot_get(
            _handle,
            native,
            native.Length,
            out var required);
        CheckStatus(status);
        if (required != count)
            throw new InvalidOperationException("Native ray-hit count changed during snapshot copy.");

        var result = new OcctModelRayHit[count];
        for (var index = 0; index < count; index++)
            result[index] = native[index].ToManaged(_ownerId);
        return result;
    }

    public OcctModelState ClassifyPoint(OcctModelShape solid, OcctPoint3d point, double tolerance = 1e-7)
    {
        EnsureShape(solid);
        OcctGuard.Positive(tolerance, nameof(tolerance));
        CheckStatus(ModelNativeMethods.occt_model_classify_point(
            _handle,
            solid.Id,
            point,
            tolerance,
            out var result));
        return result;
    }
}
