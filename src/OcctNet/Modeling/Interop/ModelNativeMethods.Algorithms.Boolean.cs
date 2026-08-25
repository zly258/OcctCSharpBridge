using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_boolean_execute(
        OcctModelingSafeHandle handle,
        int operation,
        long leftId,
        long rightId,
        in NativeModelBooleanOptions options,
        out NativeModelAlgorithmResult result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_boolean_general_fuse_execute(
        OcctModelingSafeHandle handle,
        [In] long[] shapeIds,
        int shapeCount,
        in NativeModelBooleanOptions options,
        out NativeModelAlgorithmResult result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_boolean_cells_execute(
        OcctModelingSafeHandle handle,
        [In] long[] argumentIds,
        int argumentCount,
        [In] long[] takeIds,
        int takeCount,
        [In] long[] avoidIds,
        int avoidCount,
        int material,
        int removeInternalBoundaries,
        in NativeModelBooleanOptions options,
        out NativeModelAlgorithmResult result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_boolean_split_execute(
        OcctModelingSafeHandle handle,
        [In] long[] objectIds,
        int objectCount,
        [In] long[] toolIds,
        int toolCount,
        in NativeModelBooleanOptions options,
        out NativeModelAlgorithmResult result);
}
