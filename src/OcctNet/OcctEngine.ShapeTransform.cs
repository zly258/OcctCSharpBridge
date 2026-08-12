namespace OcctNet;

public sealed partial class OcctEngine
{
    public OcctShape Copy(OcctShape shape, bool hideInput = false)
    {
        EnsureShape(shape);
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_copy_shape(_handle, shape.Id, hideInput ? 1 : 0));
    }

    public OcctShape Translate(OcctShape shape, OcctVector3d vector, bool hideInput = false)
    {
        EnsureShape(shape);
        OcctGuard.Finite(vector, nameof(vector));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_translate(_handle, shape.Id, vector, hideInput ? 1 : 0));
    }

    public OcctShape Rotate(
        OcctShape shape,
        OcctPoint3d axisPoint,
        OcctVector3d axisDirection,
        double angleDegrees,
        bool hideInput = false)
    {
        EnsureShape(shape);
        OcctGuard.Finite(axisPoint, nameof(axisPoint));
        OcctGuard.NonZero(axisDirection, nameof(axisDirection));
        OcctGuard.Finite(angleDegrees, nameof(angleDegrees));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_rotate(
            _handle,
            shape.Id,
            axisPoint,
            axisDirection,
            angleDegrees,
            hideInput ? 1 : 0));
    }

    public OcctShape Scale(OcctShape shape, OcctPoint3d center, double factor, bool hideInput = false)
    {
        EnsureShape(shape);
        OcctGuard.Finite(center, nameof(center));
        OcctGuard.Positive(factor, nameof(factor));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_scale(_handle, shape.Id, center, factor, hideInput ? 1 : 0));
    }

    public OcctShape MirrorPlane(
        OcctShape shape,
        OcctPoint3d planePoint,
        OcctVector3d planeNormal,
        bool hideInput = false)
    {
        EnsureShape(shape);
        OcctGuard.Finite(planePoint, nameof(planePoint));
        OcctGuard.NonZero(planeNormal, nameof(planeNormal));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_mirror_plane(
            _handle,
            shape.Id,
            planePoint,
            planeNormal,
            hideInput ? 1 : 0));
    }
}
