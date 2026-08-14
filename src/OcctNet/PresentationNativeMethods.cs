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

    // Frozen ABI 4 declarations retained only for compatibility-surface verification.
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_object_clip_planes(
        OcctEngineSafeHandle handle,
        long objectId,
        [In] NativeOcctViewClipPlane[] planes,
        int count);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_global_highlight_style(
        OcctEngineSafeHandle handle,
        int kind,
        in NativeOcctHighlightStyleSettings settings);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_object_highlight_style(
        OcctEngineSafeHandle handle,
        long objectId,
        int dynamic,
        in NativeOcctHighlightStyleSettings settings);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_clear_object_highlight_style(
        OcctEngineSafeHandle handle,
        long objectId,
        int dynamic);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_reset_object_display_mode(OcctEngineSafeHandle handle, long objectId);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_get_object_display_mode(
        OcctEngineSafeHandle handle,
        long objectId,
        out int hasOverride,
        out int displayMode);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_object_auto_highlight(OcctEngineSafeHandle handle, long objectId, int enabled);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_get_object_auto_highlight(OcctEngineSafeHandle handle, long objectId, out int enabled);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_object_infinite_state(OcctEngineSafeHandle handle, long objectId, int infinite);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_get_object_infinite_state(OcctEngineSafeHandle handle, long objectId, out int infinite);
}
