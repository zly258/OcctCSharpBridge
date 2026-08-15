using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class SelectionStateNativeMethods
{
    private const string LibraryName = "OcctNative";

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_selection_hits_get(
        OcctEngineSafeHandle handle,
        [Out] NativeOcctSelectionHit[]? items,
        int capacity,
        out int count);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_selection_detected_hit_get(
        OcctEngineSafeHandle handle,
        out NativeOcctSelectionHit result,
        out int hasHit);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_selection_detected_hit_detail_get(
        OcctEngineSafeHandle handle,
        out NativeOcctSelectionHitDetail result,
        out int hasHit);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_selection_detect_at(
        OcctEngineSafeHandle handle,
        int x,
        int y,
        int maxHits,
        [Out] NativeOcctSelectionHitDetail[] items,
        int capacity,
        out int count);
}
