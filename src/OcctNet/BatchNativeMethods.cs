using System.Runtime.InteropServices;

namespace OcctNet;

internal static class BatchNativeMethods
{
    private const string LibraryName = "OcctNative";

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occt_begin_update(IntPtr handle);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occt_end_update(IntPtr handle, int fitAll);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occt_is_updating(IntPtr handle);
}
