using System.Runtime.InteropServices;

namespace OcctNet;

internal static class SelectionStateNativeMethods
{
    private const string LibraryName = "OcctNative";

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_selected_hits(
        IntPtr handle,
        [Out] NativeOcctSelectionHit[]? items,
        int capacity,
        out int count);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_detected_hit(
        IntPtr handle,
        out NativeOcctSelectionHit result,
        out int hasHit);
}
