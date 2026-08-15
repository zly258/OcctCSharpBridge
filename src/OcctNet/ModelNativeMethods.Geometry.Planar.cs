using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_planar_regular_polygon_create(
        OcctModelingSafeHandle handle,
        OcctPoint3d center,
        OcctVector3d normal,
        OcctVector3d xDirection,
        double radius,
        int sideCount,
        int makeFace,
        out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_planar_rectangle_wire_create(
        OcctModelingSafeHandle handle,
        OcctPoint3d origin,
        OcctVector3d xDirection,
        OcctVector3d normal,
        double width,
        double height,
        out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_planar_face_create(
        OcctModelingSafeHandle handle,
        OcctPoint3d origin,
        OcctVector3d xDirection,
        OcctVector3d normal,
        double width,
        double height,
        out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_planar_face_from_wire_create(
        OcctModelingSafeHandle handle,
        long wireId,
        int onlyPlane,
        out long result);
}
