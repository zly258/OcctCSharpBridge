using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_edge_extrema_snapshot_get(
        OcctModelingSafeHandle handle,
        long firstEdgeId,
        long secondEdgeId,
        [Out, MarshalUsing(CountElementName = nameof(capacity))] NativeModelCurveCurveExtremum[]? results,
        int capacity,
        out int required);
}
