using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

[Flags]
internal enum NativeViewportResetMask : uint
{
    All = 1u << 0,
    Orientation = 1u << 1,
    Mapping = 1u << 2
}

internal static partial class ViewportStateNativeMethods
{
    private const string LibraryName = "OcctNative";

    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeViewportStateResult
    {
        internal uint StructSize;
        internal uint ApiVersion;
        internal OcctViewportState State;
    }

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_viewport_state_get(
        OcctEngineSafeHandle handle,
        out NativeViewportStateResult result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_viewport_reset(
        OcctEngineSafeHandle handle,
        NativeViewportResetMask resetMask);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_viewport_fit_selected(
        OcctEngineSafeHandle handle,
        double margin);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_viewport_gravity_point_get(
        OcctEngineSafeHandle handle,
        out OcctPoint3d result);
}
