using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_curve_continuity_analyze(
        OcctModelingSafeHandle handle,
        long firstEdgeId,
        int firstAtEnd,
        long secondEdgeId,
        int secondAtStart,
        in NativeModelContinuityOptions options,
        out NativeModelCurveContinuityResult result);
}
