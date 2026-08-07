namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public OcctModelProjectionResult ProjectPointOnEdge(OcctModelShape edge, OcctPoint3d point)
    {
        EnsureShape(edge);
        Check(ModelNativeMethods.occt_model_project_point_on_edge(_handle, edge.Id, point, out var result));
        return result;
    }

    public OcctModelProjectionResult ProjectPointOnFace(OcctModelShape face, OcctPoint3d point)
    {
        EnsureShape(face);
        Check(ModelNativeMethods.occt_model_project_point_on_face(_handle, face.Id, point, out var result));
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
        OcctGuard.NonNegative(tolerance, nameof(tolerance));
        if (!double.IsFinite(minimumParameter)) throw new ArgumentOutOfRangeException(nameof(minimumParameter));
        if (!double.IsFinite(maximumParameter) || maximumParameter < minimumParameter)
            throw new ArgumentOutOfRangeException(nameof(maximumParameter));

        var count = ModelNativeMethods.occt_model_ray_intersections(
            _handle,
            shape.Id,
            origin,
            direction,
            minimumParameter,
            maximumParameter,
            tolerance);
        if (count < 0) throw CreateException();

        var result = new OcctModelRayHit[count];
        for (var index = 0; index < count; index++)
        {
            Check(ModelNativeMethods.occt_model_ray_hit_at(_handle, index, out var native));
            result[index] = native.ToManaged(_ownerId);
        }
        return result;
    }

    public OcctModelState ClassifyPoint(OcctModelShape solid, OcctPoint3d point, double tolerance = 1e-7)
    {
        EnsureShape(solid);
        OcctGuard.NonNegative(tolerance, nameof(tolerance));
        return (OcctModelState)ModelNativeMethods.occt_model_classify_point(_handle, solid.Id, point, tolerance);
    }
}
