namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public OcctModelShape MakeBox(double dx, double dy, double dz, double x = 0, double y = 0, double z = 0)
    {
        OcctGuard.Positive(dx, nameof(dx));
        OcctGuard.Positive(dy, nameof(dy));
        OcctGuard.Positive(dz, nameof(dz));
        OcctGuard.Finite(x, nameof(x));
        OcctGuard.Finite(y, nameof(y));
        OcctGuard.Finite(z, nameof(z));
        return CheckShape(ModelNativeMethods.occt_model_make_box(NativeHandle, x, y, z, dx, dy, dz));
    }

    public OcctModelShape MakeCylinder(OcctPoint3d origin, OcctVector3d axis, double radius, double height)
    {
        OcctGuard.Finite(origin, nameof(origin));
        OcctGuard.NonZero(axis, nameof(axis));
        OcctGuard.Positive(radius, nameof(radius));
        OcctGuard.Positive(height, nameof(height));
        return CheckShape(ModelNativeMethods.occt_model_make_cylinder(NativeHandle, origin, axis, radius, height));
    }

    public OcctModelShape MakeCone(OcctPoint3d origin, OcctVector3d axis, double radius1, double radius2, double height)
    {
        OcctGuard.Finite(origin, nameof(origin));
        OcctGuard.NonZero(axis, nameof(axis));
        OcctGuard.NonNegative(radius1, nameof(radius1));
        OcctGuard.NonNegative(radius2, nameof(radius2));
        if (radius1 == 0 && radius2 == 0)
            throw new ArgumentException("At least one cone radius must be greater than zero.", nameof(radius1));
        OcctGuard.Positive(height, nameof(height));
        return CheckShape(ModelNativeMethods.occt_model_make_cone(NativeHandle, origin, axis, radius1, radius2, height));
    }

    public OcctModelShape MakeSphere(OcctPoint3d center, double radius)
    {
        OcctGuard.Finite(center, nameof(center));
        OcctGuard.Positive(radius, nameof(radius));
        return CheckShape(ModelNativeMethods.occt_model_make_sphere(NativeHandle, center, radius));
    }

    public OcctModelShape MakeTorus(OcctPoint3d center, OcctVector3d axis, double majorRadius, double minorRadius)
    {
        OcctGuard.Finite(center, nameof(center));
        OcctGuard.NonZero(axis, nameof(axis));
        OcctGuard.Positive(majorRadius, nameof(majorRadius));
        OcctGuard.Positive(minorRadius, nameof(minorRadius));
        if (minorRadius >= majorRadius)
            throw new ArgumentException("minorRadius must be less than majorRadius.", nameof(minorRadius));
        return CheckShape(ModelNativeMethods.occt_model_make_torus(NativeHandle, center, axis, majorRadius, minorRadius));
    }

    public OcctModelShape MakeWedge(double dx, double dy, double dz, double ltx)
    {
        OcctGuard.Positive(dx, nameof(dx));
        OcctGuard.Positive(dy, nameof(dy));
        OcctGuard.Positive(dz, nameof(dz));
        OcctGuard.Finite(ltx, nameof(ltx));
        return CheckShape(ModelNativeMethods.occt_model_make_wedge(NativeHandle, dx, dy, dz, ltx));
    }
}
