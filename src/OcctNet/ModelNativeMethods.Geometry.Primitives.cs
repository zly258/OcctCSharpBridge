using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_model_make_box(IntPtr handle, double x, double y, double z, double dx, double dy, double dz);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_model_make_cylinder(IntPtr handle, OcctPoint3d origin, OcctVector3d axis, double radius, double height);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_model_make_cone(IntPtr handle, OcctPoint3d origin, OcctVector3d axis, double radius1, double radius2, double height);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_model_make_sphere(IntPtr handle, OcctPoint3d center, double radius);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_model_make_torus(IntPtr handle, OcctPoint3d center, OcctVector3d axis, double majorRadius, double minorRadius);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_model_make_wedge(IntPtr handle, double dx, double dy, double dz, double ltx);
}
