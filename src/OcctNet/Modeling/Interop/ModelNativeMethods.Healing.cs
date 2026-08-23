using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_model_healing_fix_tolerance_execute(
        OcctModelingSafeHandle handle, long shapeId, double tolerance,
        out NativeModelAlgorithmResult result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_model_healing_fix_gaps_execute(
        OcctModelingSafeHandle handle, long shapeId, double gapTolerance,
        out NativeModelAlgorithmResult result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_model_healing_reshape_remove_execute(
        OcctModelingSafeHandle handle, long shapeId,
        [In] int[] subShapeIndices, int count,
        out NativeModelAlgorithmResult result);
}
