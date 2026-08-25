using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_surface_continuity_analyze(
        OcctModelingSafeHandle handle,
        long firstFaceId,
        long secondFaceId,
        long sharedEdgeId,
        int sampleCount,
        in NativeModelContinuityOptions options,
        out NativeModelSurfaceContinuityResult result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_curvature_comb_copy(
        OcctModelingSafeHandle handle,
        long edgeId,
        int sampleCount,
        double scale,
        double resolution,
        [Out] NativeModelCurvatureCombSample[]? samples,
        int capacity,
        out int required);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_surface_quality_copy(
        OcctModelingSafeHandle handle,
        long faceId,
        in NativeModelSurfaceQualityOptions options,
        [Out] NativeModelSurfaceQualitySample[]? samples,
        int capacity,
        out int required,
        out NativeModelSurfaceQualitySummary summary);
}
