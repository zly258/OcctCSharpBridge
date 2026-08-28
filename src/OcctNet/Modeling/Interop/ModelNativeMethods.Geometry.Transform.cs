using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_transform_translate(
        OcctModelingSafeHandle handle,
        long shapeId,
        OcctVector3d vector,
        out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_transform_rotate(
        OcctModelingSafeHandle handle,
        long shapeId,
        OcctPoint3d axisPoint,
        OcctVector3d axisDirection,
        double angleDegrees,
        out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_transform_scale(
        OcctModelingSafeHandle handle,
        long shapeId,
        OcctPoint3d center,
        double factor,
        out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_transform_affine(
        OcctModelingSafeHandle handle,
        long shapeId,
        OcctAffineTransform transform,
        out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_transform_mirror_plane(
        OcctModelingSafeHandle handle,
        long shapeId,
        OcctPoint3d planePoint,
        OcctVector3d planeNormal,
        out long result);
}
