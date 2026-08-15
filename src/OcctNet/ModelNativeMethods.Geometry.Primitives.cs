using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_make_box(OcctModelingSafeHandle handle, double x, double y, double z, double dx, double dy, double dz, out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_make_cylinder(OcctModelingSafeHandle handle, OcctPoint3d origin, OcctVector3d axis, double radius, double height, out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_make_cone(OcctModelingSafeHandle handle, OcctPoint3d origin, OcctVector3d axis, double radius1, double radius2, double height, out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_make_sphere(OcctModelingSafeHandle handle, OcctPoint3d center, double radius, out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_make_torus(OcctModelingSafeHandle handle, OcctPoint3d center, OcctVector3d axis, double majorRadius, double minorRadius, out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_make_wedge(OcctModelingSafeHandle handle, double dx, double dy, double dz, double ltx, out long result);
}
