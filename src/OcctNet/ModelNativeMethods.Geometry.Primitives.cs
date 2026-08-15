using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_primitive_box_create(
        OcctModelingSafeHandle handle,
        double x,
        double y,
        double z,
        double dx,
        double dy,
        double dz,
        out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_primitive_cylinder_create(
        OcctModelingSafeHandle handle,
        OcctPoint3d origin,
        OcctVector3d axis,
        double radius,
        double height,
        out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_primitive_cone_create(
        OcctModelingSafeHandle handle,
        OcctPoint3d origin,
        OcctVector3d axis,
        double radius1,
        double radius2,
        double height,
        out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_primitive_sphere_create(
        OcctModelingSafeHandle handle,
        OcctPoint3d center,
        double radius,
        out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_primitive_torus_create(
        OcctModelingSafeHandle handle,
        OcctPoint3d center,
        OcctVector3d axis,
        double majorRadius,
        double minorRadius,
        out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_primitive_wedge_create(
        OcctModelingSafeHandle handle,
        double dx,
        double dy,
        double dz,
        double ltx,
        out long result);
}
