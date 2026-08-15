using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_shape_linear_inertia(
        OcctModelingSafeHandle handle,
        long shapeId,
        out NativeModelInertiaProperties result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_shape_surface_inertia(
        OcctModelingSafeHandle handle,
        long shapeId,
        out NativeModelInertiaProperties result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_shape_volume_inertia(
        OcctModelingSafeHandle handle,
        long shapeId,
        out NativeModelInertiaProperties result);
}
