using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

[Flags]
internal enum NativeViewerSelectionSettingsUpdateMask : uint
{
    Mode = 1u << 0,
    Tolerance = 1u << 1
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeViewerSelectionSettingsOptions
{
    internal uint StructSize;
    internal uint ApiVersion;
    internal NativeViewerSelectionSettingsUpdateMask UpdateMask;
    internal int SelectionMode;
    internal int PixelTolerance;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeViewerRectangleSelectionOptions
{
    internal uint StructSize;
    internal uint ApiVersion;
    internal int X1;
    internal int Y1;
    internal int X2;
    internal int Y2;
    internal int Append;
    internal int AllowOverlap;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeViewerObjectSelectionOptions
{
    internal uint StructSize;
    internal uint ApiVersion;
    internal IntPtr ObjectIds;
    internal int Count;
    internal int Operation;
}

internal static partial class SelectionNativeMethods
{
    private const string LibraryName = "OcctNative";

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_selection_settings_update(
        OcctEngineSafeHandle handle,
        in NativeViewerSelectionSettingsOptions options);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_selection_move_to(OcctEngineSafeHandle handle, int x, int y);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_selection_point_select(OcctEngineSafeHandle handle, int x, int y, int append);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_selection_rectangle_select(
        OcctEngineSafeHandle handle,
        in NativeViewerRectangleSelectionOptions options);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_selection_object_select(OcctEngineSafeHandle handle, long objectId, int append);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_selection_objects_update(
        OcctEngineSafeHandle handle,
        in NativeViewerObjectSelectionOptions options);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_selection_clear(OcctEngineSafeHandle handle);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_selection_subshape_copy(
        OcctEngineSafeHandle handle,
        int index,
        out long resultShapeId);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_selection_all_visible(OcctEngineSafeHandle handle);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_selection_invert(OcctEngineSafeHandle handle);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_selection_hide_selected(OcctEngineSafeHandle handle);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_selection_automatic_highlight_set(
        OcctEngineSafeHandle handle,
        int enabled);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_selection_object_mode_set_active(
        OcctEngineSafeHandle handle,
        long objectId,
        int mode,
        int active,
        int concurrency,
        int force);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_selection_object_sensitivity_set(
        OcctEngineSafeHandle handle,
        long objectId,
        int mode,
        int sensitivity);
}
