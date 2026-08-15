using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ViewerFeatureNativeMethods
{
    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_boolean(
        OcctEngineSafeHandle handle,
        int operation,
        long leftId,
        long rightId,
        int hideInputs,
        out long result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_extrude(
        OcctEngineSafeHandle handle,
        long profileId,
        OcctVector3d value,
        int hideInput,
        out long result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_revolve(
        OcctEngineSafeHandle handle,
        long profileId,
        OcctPoint3d axisPoint,
        OcctVector3d axisDirection,
        double angleDegrees,
        int hideInput,
        out long result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_sweep(
        OcctEngineSafeHandle handle,
        long spineWireId,
        long profileId,
        int hideInputs,
        out long result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_loft(
        OcctEngineSafeHandle handle,
        IntPtr wireIds,
        int count,
        int makeSolid,
        int ruled,
        double tolerance,
        int hideInputs,
        out long result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_fillet_all_edges(
        OcctEngineSafeHandle handle,
        long shapeId,
        double radius,
        int hideInput,
        out long result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_fillet_edges(
        OcctEngineSafeHandle handle,
        long shapeId,
        IntPtr edgeIndices,
        int count,
        double radius,
        int hideInput,
        out long result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_chamfer_all_edges(
        OcctEngineSafeHandle handle,
        long shapeId,
        double distance,
        int hideInput,
        out long result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_chamfer_edges(
        OcctEngineSafeHandle handle,
        long shapeId,
        IntPtr edgeIndices,
        int count,
        double distance,
        int hideInput,
        out long result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_offset(
        OcctEngineSafeHandle handle,
        long shapeId,
        double offset,
        double tolerance,
        int hideInput,
        out long result);

    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_shape_thick_solid(
        OcctEngineSafeHandle handle,
        long solidId,
        int faceIndexToRemove,
        double thickness,
        double tolerance,
        int hideInput,
        out long result);
}
