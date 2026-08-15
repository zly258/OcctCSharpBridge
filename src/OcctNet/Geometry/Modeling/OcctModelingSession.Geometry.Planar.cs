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

        var status = ModelNativeMethods.occt_model_planar_regular_polygon_create(
            NativeHandle,
            actualCenter,
            actualNormal,
            actualXDirection,
            radius,
            sideCount,
            makeFace ? 1 : 0,
            out var result);
        return CheckShape(status, result);
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

        var status = ModelNativeMethods.occt_model_planar_rectangle_wire_create(
            NativeHandle,
            actualOrigin,
            actualXDirection,
            actualNormal,
            width,
            height,
            out var result);
        return CheckShape(status, result);
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

        var status = ModelNativeMethods.occt_model_planar_face_create(
            NativeHandle,
            actualOrigin,
            actualXDirection,
            actualNormal,
            width,
            height,
            out var result);
        return CheckShape(status, result);
    }

    public OcctModelShape MakeFace(OcctModelShape wire)
    {
        EnsureShape(wire);
        var status = ModelNativeMethods.occt_model_planar_face_from_wire_create(
            _handle,
            wire.Id,
            0,
            out var result);
        return CheckShape(status, result);
    }
}
