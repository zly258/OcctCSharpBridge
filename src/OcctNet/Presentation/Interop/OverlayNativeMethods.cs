using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

[Flags]
internal enum NativeOverlayLineUpdateMask : uint
{
    Geometry = 1u << 0,
    Style = 1u << 1
}

[Flags]
internal enum NativeOverlayMarkerUpdateMask : uint
{
    Position = 1u << 0,
    Style = 1u << 1
}

[Flags]
internal enum NativeOverlayTextUpdateMask : uint
{
    Content = 1u << 0,
    Position = 1u << 1,
    Style = 1u << 2
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeOverlayLineOptions
{
    internal uint StructSize;
    internal uint ApiVersion;
    internal NativeOverlayLineUpdateMask UpdateMask;
    internal int PrimitiveType;
    internal IntPtr Points;
    internal int PointCount;
    internal int Pattern;
    internal double Width;
    internal double Red;
    internal double Green;
    internal double Blue;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeOverlayMarkerOptions
{
    internal uint StructSize;
    internal uint ApiVersion;
    internal NativeOverlayMarkerUpdateMask UpdateMask;
    internal OcctPoint3d Position;
    internal int Marker;
    internal double Scale;
    internal double Red;
    internal double Green;
    internal double Blue;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeOverlayTextOptions
{
    internal uint StructSize;
    internal uint ApiVersion;
    internal NativeOverlayTextUpdateMask UpdateMask;
    internal IntPtr Text;
    internal OcctPoint3d Position;
    internal double Height;
    internal double Red;
    internal double Green;
    internal double Blue;
    internal int Zoomable;
    internal IntPtr FontName;
}

internal static partial class OverlayNativeMethods
{
    private const string LibraryName = "OcctNative";

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_overlay_line_create(
        OcctEngineSafeHandle handle,
        in NativeOverlayLineOptions options,
        out long resultOverlayId);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_overlay_line_update(
        OcctEngineSafeHandle handle,
        long overlayId,
        in NativeOverlayLineOptions options);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_overlay_marker_create(
        OcctEngineSafeHandle handle,
        in NativeOverlayMarkerOptions options,
        out long resultOverlayId);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_overlay_marker_update(
        OcctEngineSafeHandle handle,
        long overlayId,
        in NativeOverlayMarkerOptions options);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_overlay_text_create(
        OcctEngineSafeHandle handle,
        in NativeOverlayTextOptions options,
        out long resultOverlayId);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_overlay_text_update(
        OcctEngineSafeHandle handle,
        long overlayId,
        in NativeOverlayTextOptions options);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_overlay_primitive_type_get(
        OcctEngineSafeHandle handle,
        long overlayId,
        out int primitiveType);
}
