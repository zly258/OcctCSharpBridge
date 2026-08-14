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
internal struct LegacyNativeSurface
{
    public int Kind;
    public IntPtr Handle;
    public IntPtr Display;
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



internal static partial class NativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_initialize_surface(OcctEngineSafeHandle engine, in LegacyNativeSurface surface);
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_initialize_surface(OcctEngineSafeHandle engine, in NativeOcctSurface surface);

}
