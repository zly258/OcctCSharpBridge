using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

[StructLayout(LayoutKind.Sequential)]
internal struct NativeViewerDetectionOptions
{
    internal uint StructSize;
    internal uint ApiVersion;
    internal int X;
    internal int Y;
    internal int MaxHits;
    internal IntPtr OwnerIds;
    internal int OwnerCount;
    internal ulong ObjectKindMask;
    internal ulong ShapeTypeMask;
    internal int IncludeWholeObjects;
}

internal static partial class DetectionNativeMethods
{
    private const string LibraryName = "OcctNative";

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_selection_detect_filtered(
        OcctEngineSafeHandle handle,
        in NativeViewerDetectionOptions options,
        [Out] NativeOcctSelectionHitDetail[] items,
        int capacity,
        out int count);
}
