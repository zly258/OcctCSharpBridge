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
    public int Kind;
    public IntPtr Handle;
    public IntPtr Display;
}

internal static partial class NativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_initialize_surface(IntPtr engine, in NativeOcctSurface surface);
}
