namespace OcctNet;

public sealed partial class OcctEngine
{
    public OcctShape MakeBox(double dx, double dy, double dz, double x = 0, double y = 0, double z = 0)
    {
        OcctGuard.Positive(dx, nameof(dx));
        OcctGuard.Positive(dy, nameof(dy));
        OcctGuard.Positive(dz, nameof(dz));
        OcctGuard.Finite(x, nameof(x));
        OcctGuard.Finite(y, nameof(y));
        OcctGuard.Finite(z, nameof(z));
        EnsureInitialized();
        var status = ViewerGeometryCreationNativeMethods.occt_engine_shape_box_create(
            _handle,
            x,
            y,
            z,
            dx,
            dy,
            dz,
            out var result);
        return GeometryResult(status, result);
    }

    public OcctShape MakeCylinder(OcctPoint3d origin, OcctVector3d axis, double radius, double height)
    {
        OcctGuard.Finite(origin, nameof(origin));
        OcctGuard.NonZero(axis, nameof(axis));
        OcctGuard.Positive(radius, nameof(radius));
        OcctGuard.Positive(height, nameof(height));
        EnsureInitialized();
        var status = ViewerGeometryCreationNativeMethods.occt_engine_shape_cylinder_create(
            _handle,
            origin,
            axis,
            radius,
            height,
            out var result);
        return GeometryResult(status, result);
    }

    public OcctShape MakeCylinder(double radius, double height, double x = 0, double y = 0, double z = 0) =>
        MakeCylinder(new OcctPoint3d(x, y, z), OcctVector3d.UnitZ, radius, height);

    public OcctShape MakeSphere(double radius, double x = 0, double y = 0, double z = 0)
    {
        OcctGuard.Positive(radius, nameof(radius));
        var center = new OcctPoint3d(x, y, z);
        OcctGuard.Finite(center, nameof(center));
        EnsureInitialized();
        var status = ViewerGeometryCreationNativeMethods.occt_engine_shape_sphere_create(
            _handle,
            center,
            radius,
            out var result);
        return GeometryResult(status, result);
    }

    public OcctShape MakeCone(OcctPoint3d origin, OcctVector3d axis, double radius1, double radius2, double height)
    {
        OcctGuard.Finite(origin, nameof(origin));
        OcctGuard.NonZero(axis, nameof(axis));
        OcctGuard.NonNegative(radius1, nameof(radius1));
        OcctGuard.NonNegative(radius2, nameof(radius2));
        if (radius1 == 0 && radius2 == 0)
            throw new ArgumentException("At least one cone radius must be greater than zero.", nameof(radius1));
        OcctGuard.Positive(height, nameof(height));
        EnsureInitialized();
        var status = ViewerGeometryCreationNativeMethods.occt_engine_shape_cone_create(
            _handle,
            origin,
            axis,
            radius1,
            radius2,
            height,
            out var result);
        return GeometryResult(status, result);
    }

    public OcctShape MakeCone(double radius1, double radius2, double height, double x = 0, double y = 0, double z = 0) =>
        MakeCone(new OcctPoint3d(x, y, z), OcctVector3d.UnitZ, radius1, radius2, height);

    public OcctShape MakeTorus(
        double majorRadius,
        double minorRadius,
        OcctPoint3d? center = null,
        OcctVector3d? axis = null)
    {
        OcctGuard.Positive(majorRadius, nameof(majorRadius));
        OcctGuard.Positive(minorRadius, nameof(minorRadius));
        if (minorRadius >= majorRadius)
            throw new ArgumentException("minorRadius must be less than majorRadius.", nameof(minorRadius));
        var actualCenter = center ?? OcctPoint3d.Origin;
        var actualAxis = axis ?? OcctVector3d.UnitZ;
        OcctGuard.Finite(actualCenter, nameof(center));
        OcctGuard.NonZero(actualAxis, nameof(axis));
        EnsureInitialized();
        var status = ViewerGeometryCreationNativeMethods.occt_engine_shape_torus_create(
            _handle,
            actualCenter,
            actualAxis,
            majorRadius,
            minorRadius,
            out var result);
        return GeometryResult(status, result);
    }

    public OcctShape MakeWedge(double dx, double dy, double dz, double ltx)
    {
        OcctGuard.Positive(dx, nameof(dx));
        OcctGuard.Positive(dy, nameof(dy));
        OcctGuard.Positive(dz, nameof(dz));
        OcctGuard.Finite(ltx, nameof(ltx));
        EnsureInitialized();
        var status = ViewerGeometryCreationNativeMethods.occt_engine_shape_wedge_create(
            _handle,
            dx,
            dy,
            dz,
            ltx,
            out var result);
        return GeometryResult(status, result);
    }
}
