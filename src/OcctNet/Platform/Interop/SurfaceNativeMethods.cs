using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

internal enum OcctNativeSurfaceKind
{
    Auto = 0,
    Win32Window = 1,
    X11Window = 2,
    WaylandSurface = 3
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeOcctSurface
{
    public uint StructSize;
    public uint ApiVersion;
    public OcctNativeSurfaceKind Kind;
    public IntPtr Handle;
    public IntPtr Display;
}

internal static partial class SurfaceNativeMethods
{
    private const string LibraryName = "OcctNative";

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_initialize_surface(
        OcctEngineSafeHandle engine,
        in NativeOcctSurface surface);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_surface_resize(
        OcctEngineSafeHandle engine,
        int redraw);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_surface_redraw(
        OcctEngineSafeHandle engine);
}
