using System.Runtime.InteropServices;

namespace OcctNet;

internal static class SelectionNativeMethods
{
    private const string LibraryName = "OcctNative";

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occt_show_selection_rectangle(
        IntPtr handle,
        int x1,
        int y1,
        int x2,
        int y2,
        double lineR,
        double lineG,
        double lineB,
        double fillR,
        double fillG,
        double fillB,
        double fillTransparency,
        double lineWidth);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occt_hide_selection_rectangle(IntPtr handle);
}
