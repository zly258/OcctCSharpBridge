using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_healing_unify_same_domain_execute(
        OcctModelingSafeHandle handle,
        long shapeId,
        int unifyEdges,
        int unifyFaces,
        int concatBsplines,
        out NativeModelAlgorithmResult result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_healing_fix_shape_execute(
        OcctModelingSafeHandle handle,
        long shapeId,
        double precision,
        double minTolerance,
        double maxTolerance,
        out NativeModelAlgorithmResult result);
}
