using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_shape_free_bounds(
        OcctModelingSafeHandle handle,
        long shapeId,
        double tolerance,
        int boundaryKind,
        int splitClosed,
        int splitOpen,
        out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_shape_edge_adjacency_snapshot_get(
        OcctModelingSafeHandle handle,
        long shapeId,
        [Out, MarshalUsing(CountElementName = nameof(capacity))] NativeModelEdgeAdjacency[]? items,
        int capacity,
        out int required);
}
