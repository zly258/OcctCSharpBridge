namespace OcctNet;

internal static partial class NativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_compound(IntPtr handle, [In] long[] shapeIds, int count, int hideInputs);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_wire(IntPtr handle, [In] long[] edgeIds, int count, int hideInputs);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_sew_shapes(IntPtr handle, [In] long[] shapeIds, int count, double tolerance, int hideInputs);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_solid_from_shell(IntPtr handle, long shellId, int hideInput);
}
