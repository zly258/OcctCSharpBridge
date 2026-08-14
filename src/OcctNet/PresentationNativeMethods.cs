using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

[Flags]
internal enum NativeViewerPresentationStateUpdateMask : uint
{
    DisplayMode = 1u << 0,
    ResetDisplayMode = 1u << 1,
    AutoHighlight = 1u << 2,
    Infinite = 1u << 3
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeViewerPresentationStateOptions
{
    internal uint StructSize;
    internal uint ApiVersion;
    internal NativeViewerPresentationStateUpdateMask UpdateMask;
    internal int DisplayMode;
    internal int AutoHighlight;
    internal int Infinite;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeViewerPresentationState
{
    internal uint StructSize;
    internal uint ApiVersion;
    internal int HasDisplayModeOverride;
    internal int DisplayMode;
    internal int AutoHighlight;
    internal int Infinite;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativePresentationClipPlane
{
    internal OcctPoint3d Point;
    internal OcctVector3d Normal;
    internal int Enabled;
    internal int Capping;
    internal double CappingR;
    internal double CappingG;
    internal double CappingB;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeViewerClipPlanesOptions
{
    internal uint StructSize;
    internal uint ApiVersion;
    internal IntPtr Planes;
    internal int Count;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeViewerHighlightStyleOptions
{
    internal uint StructSize;
    internal uint ApiVersion;
    internal int Kind;
    internal int Dynamic;
    internal NativeOcctHighlightStyleSettings Settings;
}

internal static partial class PresentationNativeMethods
{
    private const string LibraryName = "OcctNative";

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_presentation_state_update(
        OcctEngineSafeHandle handle,
        long objectId,
        in NativeViewerPresentationStateOptions options);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_presentation_state_get(
        OcctEngineSafeHandle handle,
        long objectId,
        out NativeViewerPresentationState result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_presentation_clip_planes_set(
        OcctEngineSafeHandle handle,
        long objectId,
        in NativeViewerClipPlanesOptions options);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_highlight_style_global_set(
        OcctEngineSafeHandle handle,
        in NativeViewerHighlightStyleOptions options);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_highlight_style_object_set(
        OcctEngineSafeHandle handle,
        long objectId,
        in NativeViewerHighlightStyleOptions options);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_highlight_style_object_clear(
        OcctEngineSafeHandle handle,
        long objectId,
        int dynamic);
}
