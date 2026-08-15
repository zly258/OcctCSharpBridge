using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int occt_model_intersect_edges(
        OcctModelingSafeHandle handle,
        long firstEdgeId,
        long secondEdgeId,
        double tolerance);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int occt_model_edge_intersections_copy(
        OcctModelingSafeHandle handle,
        [Out, MarshalUsing(CountElementName = nameof(capacity))] NativeModelEdgeIntersection[]? results,
        int capacity);
}
