using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial long occt_model_shape_free_bounds(
        OcctModelingSafeHandle handle,
        long shapeId,
        double tolerance,
        int boundaryKind,
        int splitClosed,
        int splitOpen);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int occt_model_shape_edge_adjacency(
        OcctModelingSafeHandle handle,
        long shapeId,
        [Out, MarshalUsing(CountElementName = nameof(capacity))] NativeModelEdgeAdjacency[]? items,
        int capacity,
        out int count);
}
