using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

[StructLayout(LayoutKind.Sequential)]
internal struct NativeViewerTriedronOptionsV1
{
    internal uint StructSize;
    internal uint ApiVersion;
    internal int Visible;
    internal int Position;
    internal double Scale;
    internal NativeViewColorRgb Color;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeViewerViewCubeOptionsV1
{
    internal uint StructSize;
    internal uint ApiVersion;
    internal int Visible;
    internal int Position;
    internal int SizePixels;
    internal int OffsetX;
    internal int OffsetY;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeViewerFaceBoundaryOptions
{
    internal uint StructSize;
    internal uint ApiVersion;
    internal int Visible;
    internal NativeViewColorRgb Color;
    internal double Width;
    internal int SetDefault;
    internal int ApplyExisting;
}

internal static partial class ViewerDecorationsNativeMethods
{
    private const string LibraryName = "OcctNative";

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_objects_z_layer_set(
        OcctEngineSafeHandle handle,
        IntPtr objectIds,
        int count,
        int layer);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_object_z_layer_get(
        OcctEngineSafeHandle handle,
        long objectId,
        out int layer);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_triedron_update(
        OcctEngineSafeHandle handle,
        in NativeViewerTriedronOptionsV1 options);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_view_cube_update(
        OcctEngineSafeHandle handle,
        in NativeViewerViewCubeOptionsV1 options);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_face_boundary_update(
        OcctEngineSafeHandle handle,
        IntPtr shapeIds,
        int count,
        in NativeViewerFaceBoundaryOptions options);
}
