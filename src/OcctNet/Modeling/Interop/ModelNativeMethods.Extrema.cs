using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_edge_extrema_snapshot_get(
        OcctModelingSafeHandle handle,
        long firstEdgeId,
        long secondEdgeId,
        [Out, MarshalUsing(CountElementName = nameof(capacity))] NativeModelCurveCurveExtremum[]? results,
        int capacity,
        out int required);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_edge_face_extrema_snapshot_get(
        OcctModelingSafeHandle handle,
        long edgeId,
        long faceId,
        [Out, MarshalUsing(CountElementName = nameof(capacity))] NativeModelCurveSurfaceExtremum[]? results,
        int capacity,
        out int required);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_face_extrema_snapshot_get(
        OcctModelingSafeHandle handle,
        long firstFaceId,
        long secondFaceId,
        [Out, MarshalUsing(CountElementName = nameof(capacity))] NativeModelSurfaceSurfaceExtremum[]? results,
        int capacity,
        out int required);
}
