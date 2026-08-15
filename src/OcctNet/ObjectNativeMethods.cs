using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

[Flags]
internal enum NativeViewerObjectUpdateMask : uint
{
    Name = 1u << 0,
    ApplicationTag = 1u << 1,
    Color = 1u << 2,
    Transparency = 1u << 3,
    Visibility = 1u << 4,
    LineWidth = 1u << 5,
    Material = 1u << 6,
    Selectable = 1u << 7
}

internal enum NativeViewerObjectPresentationAction
{
    Redisplay = 0,
    Highlight = 1,
    Unhighlight = 2
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeViewerObjectUpdateOptions
{
    internal uint StructSize;
    internal uint ApiVersion;
    internal NativeViewerObjectUpdateMask UpdateMask;
    internal IntPtr Name;
    internal IntPtr ApplicationTag;
    internal NativeViewColorRgb Color;
    internal double Transparency;
    internal int Visible;
    internal double LineWidth;
    internal int Material;
    internal int Selectable;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeViewerObjectState
{
    internal uint StructSize;
    internal uint ApiVersion;
    internal int Visible;
    internal int Selected;
    internal int Selectable;
}

internal static partial class ObjectNativeMethods
{
    private const string LibraryName = "OcctNative";

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_objects_snapshot_get(
        OcctEngineSafeHandle handle,
        IntPtr items,
        int capacity,
        out int objectCount,
        out int shapeCount);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_object_exists(
        OcctEngineSafeHandle handle,
        long objectId,
        out int exists);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_object_kind_get(
        OcctEngineSafeHandle handle,
        long objectId,
        out int kind);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_object_update(
        OcctEngineSafeHandle handle,
        long objectId,
        in NativeViewerObjectUpdateOptions options);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_objects_update(
        OcctEngineSafeHandle handle,
        IntPtr objectIds,
        int count,
        in NativeViewerObjectUpdateOptions options);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_object_state_get(
        OcctEngineSafeHandle handle,
        long objectId,
        out NativeViewerObjectState state);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_object_name_get(
        OcctEngineSafeHandle handle,
        long objectId,
        IntPtr utf8Buffer,
        int capacity,
        out int requiredBytes);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_object_application_tag_get(
        OcctEngineSafeHandle handle,
        long objectId,
        IntPtr utf8Buffer,
        int capacity,
        out int requiredBytes);

    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_object_find_by_application_tag(
        OcctEngineSafeHandle handle,
        string applicationTag,
        out long objectId,
        out int found);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_objects_delete(
        OcctEngineSafeHandle handle,
        IntPtr objectIds,
        int count);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_objects_clear(OcctEngineSafeHandle handle);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_objects_visibility_all_set(
        OcctEngineSafeHandle handle,
        int visible);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_object_presentation_action(
        OcctEngineSafeHandle handle,
        long objectId,
        NativeViewerObjectPresentationAction action);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_objects_presentation_action(
        OcctEngineSafeHandle handle,
        IntPtr objectIds,
        int count,
        NativeViewerObjectPresentationAction action);
}
