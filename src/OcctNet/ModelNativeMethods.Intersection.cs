using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_intersect_edges(
        OcctModelingSafeHandle handle,
        long firstEdgeId,
        long secondEdgeId,
        double tolerance);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_edge_intersections_copy(
        OcctModelingSafeHandle handle,
        [Out] NativeModelEdgeIntersection[]? results,
        int capacity);
}
