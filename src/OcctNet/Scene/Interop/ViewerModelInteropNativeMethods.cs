using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ViewerModelInteropNativeMethods
{
    private const string LibraryName = "OcctNative";

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_object_shape_create_from_model(
        OcctEngineSafeHandle engineHandle,
        OcctModelingSafeHandle modelHandle,
        long modelShapeId,
        out long viewerObjectId);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_object_shape_update_from_model(
        OcctEngineSafeHandle engineHandle,
        OcctModelingSafeHandle modelHandle,
        long viewerObjectId,
        long modelShapeId,
        uint options);
}
