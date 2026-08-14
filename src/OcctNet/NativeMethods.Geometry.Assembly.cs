namespace OcctNet;

internal static partial class NativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_compound(OcctEngineSafeHandle handle, [In] long[] shapeIds, int count, int hideInputs);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_wire(OcctEngineSafeHandle handle, [In] long[] edgeIds, int count, int hideInputs);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_sew_shapes(OcctEngineSafeHandle handle, [In] long[] shapeIds, int count, double tolerance, int hideInputs);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_solid_from_shell(OcctEngineSafeHandle handle, long shellId, int hideInput);
}
