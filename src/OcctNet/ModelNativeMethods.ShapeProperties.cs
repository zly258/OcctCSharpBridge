using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int occt_model_shape_is_same(
        OcctModelingSafeHandle handle,
        long firstId,
        long secondId);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int occt_model_shape_is_partner(
        OcctModelingSafeHandle handle,
        long firstId,
        long secondId);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int occt_model_shape_oriented_bounds(
        OcctModelingSafeHandle handle,
        long shapeId,
        int optimal,
        out OcctOrientedBounds result);
}
