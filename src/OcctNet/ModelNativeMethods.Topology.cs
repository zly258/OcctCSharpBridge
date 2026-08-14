using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_subshapes_copy(OcctModelingSafeHandle handle, long shapeId, int shapeType, [Out] long[]? results, int capacity);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern long occt_model_outer_wire(OcctModelingSafeHandle handle, long faceId);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_inner_wires_copy(OcctModelingSafeHandle handle, long faceId, [Out] long[]? results, int capacity);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_ancestors_copy(OcctModelingSafeHandle handle, long rootId, long childId, int ancestorType, [Out] long[]? results, int capacity);
}
