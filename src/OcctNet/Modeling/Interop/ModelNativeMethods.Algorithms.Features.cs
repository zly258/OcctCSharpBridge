using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_feature_extrude_execute(
        OcctModelingSafeHandle handle,
        long profileId,
        OcctVector3d vector,
        out NativeModelAlgorithmResult result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_feature_revolve_execute(
        OcctModelingSafeHandle handle,
        long profileId,
        OcctPoint3d axisPoint,
        OcctVector3d axisDirection,
        double angleDegrees,
        out NativeModelAlgorithmResult result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_feature_sweep_execute(
        OcctModelingSafeHandle handle,
        long spineWireId,
        long profileId,
        out NativeModelAlgorithmResult result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_feature_loft_execute(
        OcctModelingSafeHandle handle,
        [In] long[] wireIds,
        int count,
        int makeSolid,
        int ruled,
        double tolerance,
        out NativeModelAlgorithmResult result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_feature_fillet_edges_execute(
        OcctModelingSafeHandle handle,
        long shapeId,
        [In] int[] edgeIndices,
        int count,
        double radius,
        out NativeModelAlgorithmResult result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_feature_chamfer_edges_execute(
        OcctModelingSafeHandle handle,
        long shapeId,
        [In] int[] edgeIndices,
        int count,
        double distance,
        out NativeModelAlgorithmResult result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_feature_offset_execute(
        OcctModelingSafeHandle handle,
        long shapeId,
        double offset,
        double tolerance,
        out NativeModelAlgorithmResult result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_feature_thick_solid_execute(
        OcctModelingSafeHandle handle,
        long solidId,
        [In] int[] faceIndices,
        int count,
        double thickness,
        double tolerance,
        out NativeModelAlgorithmResult result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_model_feature_loft_guided_execute(
        OcctModelingSafeHandle handle,
        [In] long[] sectionWireIds, int sectionCount,
        [In] long[]? guideWireIds, int guideCount,
        int makeSolid,
        double tolerance,
        out NativeModelAlgorithmResult result);
}
