using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

[Flags]
internal enum NativeViewerDepthUpdateMask : uint
{
    AutoZFitSettings = 1u << 0,
    AutoZFitNow = 1u << 1,
    DefaultPolygonOffsets = 1u << 2
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeViewerDepthUpdateOptions
{
    internal uint StructSize;
    internal uint ApiVersion;
    internal NativeViewerDepthUpdateMask UpdateMask;
    internal int AutoZFitEnabled;
    internal double AutoZFitScaleFactor;
    internal int PolygonOffsetMode;
    internal double PolygonOffsetFactor;
    internal double PolygonOffsetUnits;
    internal int ApplyPolygonOffsetsToExisting;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeViewerDepthState
{
    internal uint StructSize;
    internal uint ApiVersion;
    internal int AutoZFitEnabled;
    internal double AutoZFitScaleFactor;
    internal int PolygonOffsetMode;
    internal double PolygonOffsetFactor;
    internal double PolygonOffsetUnits;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeViewerObjectPolygonOffsetOptions
{
    internal uint StructSize;
    internal uint ApiVersion;
    internal int ResetToDefault;
    internal int Mode;
    internal double Factor;
    internal double Units;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeViewerObjectPolygonOffsetState
{
    internal uint StructSize;
    internal uint ApiVersion;
    internal int Mode;
    internal double Factor;
    internal double Units;
}

internal static partial class DepthNativeMethods
{
    private const string LibraryName = "OcctNative";

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_depth_update(
        OcctEngineSafeHandle handle,
        in NativeViewerDepthUpdateOptions options);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_depth_state_get(
        OcctEngineSafeHandle handle,
        out NativeViewerDepthState state);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_object_polygon_offset_update(
        OcctEngineSafeHandle handle,
        long objectId,
        in NativeViewerObjectPolygonOffsetOptions options);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_object_polygon_offset_get(
        OcctEngineSafeHandle handle,
        long objectId,
        out NativeViewerObjectPolygonOffsetState state);
}
