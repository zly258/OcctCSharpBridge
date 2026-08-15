using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_history_generated_snapshot_get(
        OcctModelingSafeHandle handle,
        long operationId,
        long sourceShapeId,
        [Out, MarshalUsing(CountElementName = nameof(capacity))] long[]? results,
        int capacity,
        out int required);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_history_modified_snapshot_get(
        OcctModelingSafeHandle handle,
        long operationId,
        long sourceShapeId,
        [Out, MarshalUsing(CountElementName = nameof(capacity))] long[]? results,
        int capacity,
        out int required);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_history_is_removed_get(
        OcctModelingSafeHandle handle,
        long operationId,
        long sourceShapeId,
        out int result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_history_summary(
        OcctModelingSafeHandle handle,
        long operationId,
        long sourceShapeId,
        out NativeModelTopologyHistorySummary result);
}
