namespace OcctNet;

public sealed partial class OcctModelingSession
{

    public OcctModelShape MakePlaneFace(OcctPoint3d origin, OcctVector3d normal, OcctVector3d xDirection, double uMin, double uMax, double vMin, double vMax)
    {
        ValidateSurfaceBounds(uMin, uMax, vMin, vMax);
        OcctGuard.Finite(origin, nameof(origin));
        OcctGuard.NonZero(normal, nameof(normal));
        OcctGuard.NonZero(xDirection, nameof(xDirection));
        var status = ModelNativeMethods.occt_model_surface_plane_face_create(NativeHandle, origin, normal, xDirection, uMin, uMax, vMin, vMax, out var result);
        return CheckShape(status, result);
    }

    public OcctModelShape MakeCylinderFace(double radius, double uMin, double uMax, double vMin, double vMax, OcctPoint3d? origin = null, OcctVector3d? axis = null, OcctVector3d? xDirection = null)
    {
        OcctGuard.Positive(radius, nameof(radius));
        ValidateSurfaceBounds(uMin, uMax, vMin, vMax);
        var actualOrigin = origin ?? OcctPoint3d.Origin;
        var actualAxis = axis ?? OcctVector3d.UnitZ;
        var actualXDirection = xDirection ?? OcctVector3d.UnitX;
        OcctGuard.Finite(actualOrigin, nameof(origin));
        OcctGuard.NonZero(actualAxis, nameof(axis));
        OcctGuard.NonZero(actualXDirection, nameof(xDirection));
        var status = ModelNativeMethods.occt_model_surface_cylinder_face_create(NativeHandle, actualOrigin, actualAxis, actualXDirection, radius, uMin, uMax, vMin, vMax, out var result);
        return CheckShape(status, result);
    }

    public OcctModelShape MakeConeFace(double referenceRadius, double semiAngleRadians, double uMin, double uMax, double vMin, double vMax, OcctPoint3d? referenceOrigin = null, OcctVector3d? axis = null, OcctVector3d? xDirection = null)
    {
        OcctGuard.Positive(referenceRadius, nameof(referenceRadius));
        OcctGuard.Finite(semiAngleRadians, nameof(semiAngleRadians));
        if (Math.Abs(semiAngleRadians) <= 1e-12 || Math.Abs(semiAngleRadians) >= Math.PI / 2.0) throw new ArgumentOutOfRangeException(nameof(semiAngleRadians));
        ValidateSurfaceBounds(uMin, uMax, vMin, vMax);
        var actualOrigin = referenceOrigin ?? OcctPoint3d.Origin;
        var actualAxis = axis ?? OcctVector3d.UnitZ;
        var actualXDirection = xDirection ?? OcctVector3d.UnitX;
        OcctGuard.Finite(actualOrigin, nameof(referenceOrigin));
        OcctGuard.NonZero(actualAxis, nameof(axis));
        OcctGuard.NonZero(actualXDirection, nameof(xDirection));
        var status = ModelNativeMethods.occt_model_surface_cone_face_create(NativeHandle, actualOrigin, actualAxis, actualXDirection, referenceRadius, semiAngleRadians, uMin, uMax, vMin, vMax, out var result);
        return CheckShape(status, result);
    }

    public OcctModelShape MakeSphereFace(double radius, double uMin, double uMax, double vMin, double vMax, OcctPoint3d? center = null, OcctVector3d? axis = null, OcctVector3d? xDirection = null)
    {
        OcctGuard.Positive(radius, nameof(radius));
        ValidateSurfaceBounds(uMin, uMax, vMin, vMax);
        var actualCenter = center ?? OcctPoint3d.Origin;
        var actualAxis = axis ?? OcctVector3d.UnitZ;
        var actualXDirection = xDirection ?? OcctVector3d.UnitX;
        OcctGuard.Finite(actualCenter, nameof(center));
        OcctGuard.NonZero(actualAxis, nameof(axis));
        OcctGuard.NonZero(actualXDirection, nameof(xDirection));
        var status = ModelNativeMethods.occt_model_surface_sphere_face_create(NativeHandle, actualCenter, actualAxis, actualXDirection, radius, uMin, uMax, vMin, vMax, out var result);
        return CheckShape(status, result);
    }

    public OcctModelShape MakeTorusFace(double majorRadius, double minorRadius, double uMin, double uMax, double vMin, double vMax, OcctPoint3d? center = null, OcctVector3d? axis = null, OcctVector3d? xDirection = null)
    {
        OcctGuard.Positive(majorRadius, nameof(majorRadius));
        OcctGuard.Positive(minorRadius, nameof(minorRadius));
        if (minorRadius >= majorRadius) throw new ArgumentOutOfRangeException(nameof(minorRadius));
        ValidateSurfaceBounds(uMin, uMax, vMin, vMax);
        var actualCenter = center ?? OcctPoint3d.Origin;
        var actualAxis = axis ?? OcctVector3d.UnitZ;
        var actualXDirection = xDirection ?? OcctVector3d.UnitX;
        OcctGuard.Finite(actualCenter, nameof(center));
        OcctGuard.NonZero(actualAxis, nameof(axis));
        OcctGuard.NonZero(actualXDirection, nameof(xDirection));
        var status = ModelNativeMethods.occt_model_surface_torus_face_create(NativeHandle, actualCenter, actualAxis, actualXDirection, majorRadius, minorRadius, uMin, uMax, vMin, vMax, out var result);
        return CheckShape(status, result);
    }

    private static void ValidateSurfaceBounds(double uMin, double uMax, double vMin, double vMax)
    {
        if (!double.IsFinite(uMin) || !double.IsFinite(uMax) || uMax <= uMin) throw new ArgumentOutOfRangeException(nameof(uMax));
        if (!double.IsFinite(vMin) || !double.IsFinite(vMax) || vMax <= vMin) throw new ArgumentOutOfRangeException(nameof(vMax));
    }

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
