using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_intersect_edges(
        OcctModelingSafeHandle handle,
        long firstEdgeId,
        long secondEdgeId,
        double tolerance,
        out int resultCount);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_edge_intersections_snapshot_get(
        OcctModelingSafeHandle handle,
        [Out, MarshalUsing(CountElementName = nameof(capacity))] NativeModelEdgeIntersection[]? results,
        int capacity,
        out int required);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_intersect_edge_face_snapshot_get(
        OcctModelingSafeHandle handle,
        long edgeId,
        long faceId,
        double tolerance,
        [Out, MarshalUsing(CountElementName = nameof(capacity))] NativeModelEdgeFaceIntersection[]? results,
        int capacity,
        out int required);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_intersect_surfaces(
        OcctModelingSafeHandle handle,
        long firstFaceId,
        long secondFaceId,
        double tolerance,
        out long result);
}
