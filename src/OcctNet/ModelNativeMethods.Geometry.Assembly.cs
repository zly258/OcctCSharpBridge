using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_model_make_compound(OcctModelingSafeHandle handle, [In] long[] shapeIds, int count);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_model_make_wire(OcctModelingSafeHandle handle, [In] long[] edgeIds, int count);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_model_sew(OcctModelingSafeHandle handle, [In] long[] shapeIds, int count, double tolerance);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_model_make_solid_from_shell(OcctModelingSafeHandle handle, long shellId);
}
