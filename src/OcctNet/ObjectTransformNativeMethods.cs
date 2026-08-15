using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

[StructLayout(LayoutKind.Sequential)]
internal struct NativeViewerObjectTransformUpdate
{
    internal long ObjectId;
    internal OcctTransform3d Transformation;
}

internal static partial class ObjectTransformNativeMethods
{
    private const string LibraryName = "OcctNative";

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_object_transform_set(
        OcctEngineSafeHandle handle,
        long objectId,
        in OcctTransform3d transformation);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_object_transform_get(
        OcctEngineSafeHandle handle,
        long objectId,
        out OcctTransform3d transformation,
        out int hasTransformation);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_object_transform_reset(
        OcctEngineSafeHandle handle,
        long objectId);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_object_transforms_set(
        OcctEngineSafeHandle handle,
        IntPtr updates,
        int count);
}
