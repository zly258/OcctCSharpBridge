namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public OcctModelShape MakeRegularPolygon(
        double radius,
        int sideCount,
        bool makeFace = false,
        OcctPoint3d? center = null,
        OcctVector3d? normal = null,
        OcctVector3d? xDirection = null)
    {
        OcctGuard.Positive(radius, nameof(radius));
        OcctGuard.AtLeast(sideCount, 3, nameof(sideCount));
        var actualCenter = center ?? OcctPoint3d.Origin;
        var actualNormal = normal ?? OcctVector3d.UnitZ;
        var actualXDirection = xDirection ?? OcctVector3d.UnitX;
        OcctGuard.Finite(actualCenter, nameof(center));
        OcctGuard.NonZero(actualNormal, nameof(normal));
        OcctGuard.NonZero(actualXDirection, nameof(xDirection));
        return CheckShape(ModelNativeMethods.occt_model_make_regular_polygon(
            NativeHandle, actualCenter, actualNormal, actualXDirection, radius, sideCount, makeFace ? 1 : 0));
    }

    public OcctModelShape MakeRectangleWire(
        double width,
        double height,
        OcctPoint3d? origin = null,
        OcctVector3d? xDirection = null,
        OcctVector3d? normal = null)
    {
        OcctGuard.Positive(width, nameof(width));
        OcctGuard.Positive(height, nameof(height));
        var actualOrigin = origin ?? OcctPoint3d.Origin;
        var actualXDirection = xDirection ?? OcctVector3d.UnitX;
        var actualNormal = normal ?? OcctVector3d.UnitZ;
        OcctGuard.Finite(actualOrigin, nameof(origin));
        OcctGuard.NonZero(actualXDirection, nameof(xDirection));
        OcctGuard.NonZero(actualNormal, nameof(normal));
        return CheckShape(ModelNativeMethods.occt_model_make_rectangle_wire(
            NativeHandle, actualOrigin, actualXDirection, actualNormal, width, height));
    }

    public OcctModelShape MakePlaneFace(
        double width,
        double height,
        OcctPoint3d? origin = null,
        OcctVector3d? xDirection = null,
        OcctVector3d? normal = null)
    {
        OcctGuard.Positive(width, nameof(width));
        OcctGuard.Positive(height, nameof(height));
        var actualOrigin = origin ?? OcctPoint3d.Origin;
        var actualXDirection = xDirection ?? OcctVector3d.UnitX;
        var actualNormal = normal ?? OcctVector3d.UnitZ;
        OcctGuard.Finite(actualOrigin, nameof(origin));
        OcctGuard.NonZero(actualXDirection, nameof(xDirection));
        OcctGuard.NonZero(actualNormal, nameof(normal));
        return CheckShape(ModelNativeMethods.occt_model_make_plane_face(
            NativeHandle, actualOrigin, actualXDirection, actualNormal, width, height));
    }

    public OcctModelShape MakeFace(OcctModelShape wire)
    {
        EnsureShape(wire);
        return CheckShape(ModelNativeMethods.occt_model_make_face_from_wire(_handle, wire.Id, 0));
    }
}
