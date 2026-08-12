namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public OcctModelShape Translate(OcctModelShape shape, OcctVector3d vector)
    {
        EnsureShape(shape);
        OcctGuard.Finite(vector, nameof(vector));
        return CheckShape(ModelNativeMethods.occt_model_translate(_handle, shape.Id, vector));
    }

    public OcctModelShape Rotate(OcctModelShape shape, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees)
    {
        EnsureShape(shape);
        OcctGuard.Finite(axisPoint, nameof(axisPoint));
        OcctGuard.NonZero(axisDirection, nameof(axisDirection));
        OcctGuard.Finite(angleDegrees, nameof(angleDegrees));
        return CheckShape(ModelNativeMethods.occt_model_rotate(_handle, shape.Id, axisPoint, axisDirection, angleDegrees));
    }

    public OcctModelShape Scale(OcctModelShape shape, OcctPoint3d center, double factor)
    {
        EnsureShape(shape);
        OcctGuard.Finite(center, nameof(center));
        OcctGuard.Positive(factor, nameof(factor));
        return CheckShape(ModelNativeMethods.occt_model_scale(_handle, shape.Id, center, factor));
    }

    public OcctModelShape MirrorPlane(OcctModelShape shape, OcctPoint3d planePoint, OcctVector3d planeNormal)
    {
        EnsureShape(shape);
        OcctGuard.Finite(planePoint, nameof(planePoint));
        OcctGuard.NonZero(planeNormal, nameof(planeNormal));
        return CheckShape(ModelNativeMethods.occt_model_mirror_plane(_handle, shape.Id, planePoint, planeNormal));
    }
}
