using System.Runtime.InteropServices;

namespace OcctNet;

internal static class DetectionNativeMethods
{
    private const string LibraryName = "OcctNative";

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_detect_at_filtered(
        OcctEngineSafeHandle handle,
        int x,
        int y,
        int maxHits,
        [In] long[] ownerIds,
        int ownerCount,
        ulong objectKindMask,
        ulong shapeTypeMask,
        int includeWholeObjects,
        [Out] NativeOcctSelectionHitDetail[] items,
        int capacity,
        out int count);
}
