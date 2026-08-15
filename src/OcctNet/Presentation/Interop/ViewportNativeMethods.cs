using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

[Flags]
internal enum NativeViewportRenderingUpdateMask : uint
{
    MsaaSamples = 1u << 0,
    ResolutionScale = 1u << 1,
    ResolutionDpi = 1u << 2,
    Method = 1u << 3,
    Shadows = 1u << 4,
    ImmediateUpdate = 1u << 5,
    FrustumCulling = 1u << 6,
    FaceBoundaries = 1u << 7
}

internal static partial class ViewportNativeMethods
{
    private const string LibraryName = "OcctNative";

    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeViewportRenderingOptions
    {
        internal uint StructSize;
        internal uint ApiVersion;
        internal NativeViewportRenderingUpdateMask UpdateMask;
        internal int MsaaSamples;
        internal double ResolutionScale;
        internal double ResolutionDpi;
        internal int RenderingMethod;
        internal int ShadowsEnabled;
        internal int ImmediateUpdate;
        internal int FrustumCullingEnabled;
        internal int FaceBoundariesVisible;
        internal int ApplyFaceBoundariesToExisting;
    }

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_viewport_fit_objects(
        OcctEngineSafeHandle handle,
        IntPtr objectIds,
        int count,
        double margin);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_viewport_zup_set(
        OcctEngineSafeHandle handle,
        int orientation,
        int fitAll);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_viewport_screen_to_ray(
        OcctEngineSafeHandle handle,
        int x,
        int y,
        out OcctProjectionRay result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_viewport_zoom_at_point(
        OcctEngineSafeHandle handle,
        int x,
        int y,
        double delta);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_viewport_rendering_update(
        OcctEngineSafeHandle handle,
        in NativeViewportRenderingOptions options);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_viewport_clip_planes_set(
        OcctEngineSafeHandle handle,
        IntPtr planes,
        int count);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_viewport_clip_plane_limit_get(
        OcctEngineSafeHandle handle,
        out int limit);
}
