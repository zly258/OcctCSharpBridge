using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_shape_is_same(OcctModelingSafeHandle handle, long firstId, long secondId);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_shape_is_partner(OcctModelingSafeHandle handle, long firstId, long secondId);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_shape_oriented_bounds(
        OcctModelingSafeHandle handle,
        long shapeId,
        int optimal,
        out OcctOrientedBounds result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern long occt_model_make_face_with_holes(
        OcctModelingSafeHandle handle,
        long outerWireId,
        [In] long[] innerWireIds,
        int innerWireCount);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern long occt_model_trim_edge(
        OcctModelingSafeHandle handle,
        long edgeId,
        double firstParameter,
        double lastParameter);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern long occt_model_offset_wire(
        OcctModelingSafeHandle handle,
        long wireId,
        double offset,
        double altitude,
        int joinType,
        int openResult);
}
