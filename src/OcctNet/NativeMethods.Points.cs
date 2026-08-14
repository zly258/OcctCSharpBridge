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

    // Frozen ABI 4 declarations retained only for compatibility-surface verification.
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern long occt_add_point(
        OcctEngineSafeHandle handle,
        OcctPoint3d position,
        int marker,
        double scale,
        double r,
        double g,
        double b);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_point_position(
        OcctEngineSafeHandle handle,
        long pointId,
        OcctPoint3d position);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_point_style(
        OcctEngineSafeHandle handle,
        long pointId,
        int marker,
        double scale,
        double r,
        double g,
        double b);
}
