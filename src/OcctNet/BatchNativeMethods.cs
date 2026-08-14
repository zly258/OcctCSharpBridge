using System.Runtime.InteropServices;

namespace OcctNet;

internal static class BatchNativeMethods
{
    private const string LibraryName = "OcctNative";

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_begin_update(OcctEngineSafeHandle handle);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_end_update(OcctEngineSafeHandle handle, int fitAll);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_is_updating(OcctEngineSafeHandle handle);
}
