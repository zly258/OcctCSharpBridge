namespace OcctNet;

internal static partial class NativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_box(OcctEngineSafeHandle handle, double x, double y, double z, double dx, double dy, double dz);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_cylinder(OcctEngineSafeHandle handle, OcctPoint3d origin, OcctVector3d axis, double radius, double height);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_sphere(OcctEngineSafeHandle handle, OcctPoint3d center, double radius);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_cone(OcctEngineSafeHandle handle, OcctPoint3d origin, OcctVector3d axis, double radius1, double radius2, double height);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_torus(OcctEngineSafeHandle handle, OcctPoint3d center, OcctVector3d axis, double majorRadius, double minorRadius);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_wedge(OcctEngineSafeHandle handle, double dx, double dy, double dz, double ltx);
}
