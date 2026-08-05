using System.Runtime.InteropServices;

namespace OcctNet;

internal static class DepthNativeMethods
{
    private const string LibraryName = "OcctNative";

    [StructLayout(LayoutKind.Sequential)]
    internal struct NativePolygonOffsetSettings
    {
        internal int Mode;
        internal double Factor;
        internal double Units;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeAutoZFitSettings
    {
        internal int Enabled;
        internal double ScaleFactor;
    }

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occt_set_auto_z_fit_mode(IntPtr handle, int enabled, double scaleFactor);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occt_get_auto_z_fit_mode(IntPtr handle, out NativeAutoZFitSettings result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occt_auto_z_fit(IntPtr handle);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occt_set_default_polygon_offsets(
        IntPtr handle,
        int mode,
        double factor,
        double units,
        int applyExisting);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occt_get_default_polygon_offsets(
        IntPtr handle,
        out NativePolygonOffsetSettings result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occt_set_object_polygon_offsets(
        IntPtr handle,
        long objectId,
        int mode,
        double factor,
        double units);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occt_get_object_polygon_offsets(
        IntPtr handle,
        long objectId,
        out NativePolygonOffsetSettings result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occt_reset_object_polygon_offsets(IntPtr handle, long objectId);
}
