namespace OcctNet;

public sealed partial class OcctEngine
{
    public OcctShape MakeRegularPolygon(
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
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_regular_polygon(
            _handle, actualCenter, actualNormal, actualXDirection, radius, sideCount, makeFace ? 1 : 0));
    }

    public OcctShape MakeRectangleWire(
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
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_rectangle_wire(
            _handle, actualOrigin, actualXDirection, actualNormal, width, height));
    }

    public OcctShape MakeFace(OcctShape wire, bool onlyPlane = true)
    {
        EnsureShape(wire);
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_face_from_wire(_handle, wire.Id, onlyPlane ? 1 : 0));
    }

    public OcctShape MakePlaneFace(
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
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_plane_face(
            _handle, actualOrigin, actualXDirection, actualNormal, width, height));
    }
}
