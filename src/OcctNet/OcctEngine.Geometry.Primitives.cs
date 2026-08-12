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
        return CheckShape(NativeMethods.occt_make_box(_handle, x, y, z, dx, dy, dz));
    }

    public OcctShape MakeCylinder(OcctPoint3d origin, OcctVector3d axis, double radius, double height)
    {
        OcctGuard.Finite(origin, nameof(origin));
        OcctGuard.NonZero(axis, nameof(axis));
        OcctGuard.Positive(radius, nameof(radius));
        OcctGuard.Positive(height, nameof(height));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_cylinder(_handle, origin, axis, radius, height));
    }

    public OcctShape MakeCylinder(double radius, double height, double x = 0, double y = 0, double z = 0) =>
        MakeCylinder(new OcctPoint3d(x, y, z), OcctVector3d.UnitZ, radius, height);

    public OcctShape MakeSphere(double radius, double x = 0, double y = 0, double z = 0)
    {
        OcctGuard.Positive(radius, nameof(radius));
        var center = new OcctPoint3d(x, y, z);
        OcctGuard.Finite(center, nameof(center));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_sphere(_handle, center, radius));
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
        return CheckShape(NativeMethods.occt_make_cone(_handle, origin, axis, radius1, radius2, height));
    }

    public OcctShape MakeCone(double radius1, double radius2, double height, double x = 0, double y = 0, double z = 0) =>
        MakeCone(new OcctPoint3d(x, y, z), OcctVector3d.UnitZ, radius1, radius2, height);

    public OcctShape MakeTorus(double majorRadius, double minorRadius, OcctPoint3d? center = null, OcctVector3d? axis = null)
    {
        OcctGuard.Positive(majorRadius, nameof(majorRadius));
        OcctGuard.Positive(minorRadius, nameof(minorRadius));
        var actualCenter = center ?? OcctPoint3d.Origin;
        var actualAxis = axis ?? OcctVector3d.UnitZ;
        OcctGuard.Finite(actualCenter, nameof(center));
        OcctGuard.NonZero(actualAxis, nameof(axis));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_torus(_handle, actualCenter, actualAxis, majorRadius, minorRadius));
    }

    public OcctShape MakeWedge(double dx, double dy, double dz, double ltx)
    {
        OcctGuard.Positive(dx, nameof(dx));
        OcctGuard.Positive(dy, nameof(dy));
        OcctGuard.Positive(dz, nameof(dz));
        OcctGuard.Finite(ltx, nameof(ltx));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_wedge(_handle, dx, dy, dz, ltx));
    }
}
