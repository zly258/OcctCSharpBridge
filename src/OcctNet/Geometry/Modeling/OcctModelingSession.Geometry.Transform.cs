namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public OcctModelShape Translate(OcctModelShape shape, OcctVector3d vector)
    {
        EnsureShape(shape);
        OcctGuard.Finite(vector, nameof(vector));
        var status = ModelNativeMethods.occt_model_transform_translate(
            _handle, shape.Id, vector, out var result);
        return CheckShape(status, result);
    }

    public OcctModelShape Rotate(
        OcctModelShape shape,
        OcctPoint3d axisPoint,
        OcctVector3d axisDirection,
        double angleDegrees)
    {
        EnsureShape(shape);
        OcctGuard.Finite(axisPoint, nameof(axisPoint));
        OcctGuard.NonZero(axisDirection, nameof(axisDirection));
        OcctGuard.Finite(angleDegrees, nameof(angleDegrees));
        var status = ModelNativeMethods.occt_model_transform_rotate(
            _handle, shape.Id, axisPoint, axisDirection, angleDegrees, out var result);
        return CheckShape(status, result);
    }

    public OcctModelShape Scale(OcctModelShape shape, OcctPoint3d center, double factor)
    {
        EnsureShape(shape);
        OcctGuard.Finite(center, nameof(center));
        OcctGuard.Positive(factor, nameof(factor));
        var status = ModelNativeMethods.occt_model_transform_scale(
            _handle, shape.Id, center, factor, out var result);
        return CheckShape(status, result);
    }

    public OcctModelShape Transform(OcctModelShape shape, OcctAffineTransform transform)
    {
        EnsureShape(shape);
        OcctGuard.Finite(transform.M00, nameof(transform));
        OcctGuard.Finite(transform.M01, nameof(transform));
        OcctGuard.Finite(transform.M02, nameof(transform));
        OcctGuard.Finite(transform.M03, nameof(transform));
        OcctGuard.Finite(transform.M10, nameof(transform));
        OcctGuard.Finite(transform.M11, nameof(transform));
        OcctGuard.Finite(transform.M12, nameof(transform));
        OcctGuard.Finite(transform.M13, nameof(transform));
        OcctGuard.Finite(transform.M20, nameof(transform));
        OcctGuard.Finite(transform.M21, nameof(transform));
        OcctGuard.Finite(transform.M22, nameof(transform));
        OcctGuard.Finite(transform.M23, nameof(transform));

        var status = ModelNativeMethods.occt_model_transform_affine(
            _handle, shape.Id, transform, out var result);
        return CheckShape(status, result);
    }

    public OcctModelShape MirrorPlane(
        OcctModelShape shape,
        OcctPoint3d planePoint,
        OcctVector3d planeNormal)
    {
        EnsureShape(shape);
        OcctGuard.Finite(planePoint, nameof(planePoint));
        OcctGuard.NonZero(planeNormal, nameof(planeNormal));
        var status = ModelNativeMethods.occt_model_transform_mirror_plane(
            _handle, shape.Id, planePoint, planeNormal, out var result);
        return CheckShape(status, result);
    }
}
