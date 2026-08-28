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

    public OcctModelShape Scale(
        OcctModelShape shape,
        OcctPoint3d center,
        double xFactor,
        double yFactor,
        double zFactor)
    {
        EnsureShape(shape);
        OcctGuard.Finite(center, nameof(center));
        OcctGuard.Positive(xFactor, nameof(xFactor));
        OcctGuard.Positive(yFactor, nameof(yFactor));
        OcctGuard.Positive(zFactor, nameof(zFactor));

        return Transform(shape, new OcctTransform3d(
            xFactor, 0, 0, center.X * (1 - xFactor),
            0, yFactor, 0, center.Y * (1 - yFactor),
            0, 0, zFactor, center.Z * (1 - zFactor)));
    }

    public OcctModelShape Transform(OcctModelShape shape, OcctTransform3d transform)
    {
        EnsureShape(shape);
        if (!transform.IsFinite)
            throw new ArgumentException("Transformation matrix must contain only finite values.", nameof(transform));

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
