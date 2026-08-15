using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

[Flags]
internal enum NativeViewerPointUpdateMask : uint
{
    Position = 1u << 0,
    Style = 1u << 1
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeViewerPointOptions
{
    internal uint StructSize;
    internal uint ApiVersion;
    internal NativeViewerPointUpdateMask UpdateMask;
    internal OcctPoint3d Position;
    internal int Marker;
    internal double Scale;
    internal double Red;
    internal double Green;
    internal double Blue;
}

[Flags]
internal enum NativeViewerPointPixmapUpdateMask : uint
{
    Position = 1u << 0,
    Image = 1u << 1
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeViewerPointPixmapOptions
{
    internal uint StructSize;
    internal uint ApiVersion;
    internal NativeViewerPointPixmapUpdateMask UpdateMask;
    internal OcctPoint3d Position;
    internal int Width;
    internal int Height;
    internal IntPtr Pixels;
    internal int PixelCount;
    internal int PixelFormat;
}

internal static partial class NativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_point_create(
        OcctEngineSafeHandle handle,
        in NativeViewerPointOptions options,
        out long resultPointId);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_point_update(
        OcctEngineSafeHandle handle,
        long pointId,
        in NativeViewerPointOptions options);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_point_pixmap_create(
        OcctEngineSafeHandle handle,
        in NativeViewerPointPixmapOptions options,
        out long resultPointId);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_point_pixmap_update(
        OcctEngineSafeHandle handle,
        long pointId,
        in NativeViewerPointPixmapOptions options);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_points_update(
        OcctEngineSafeHandle handle,
        IntPtr updates,
        int count);
}
