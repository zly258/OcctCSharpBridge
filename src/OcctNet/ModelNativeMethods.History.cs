using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int occt_model_history_generated_copy(
        OcctModelingSafeHandle handle,
        long operationId,
        long sourceShapeId,
        [Out, MarshalUsing(CountElementName = nameof(capacity))] long[]? results,
        int capacity);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int occt_model_history_modified_copy(
        OcctModelingSafeHandle handle,
        long operationId,
        long sourceShapeId,
        [Out, MarshalUsing(CountElementName = nameof(capacity))] long[]? results,
        int capacity);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int occt_model_history_is_removed(
        OcctModelingSafeHandle handle,
        long operationId,
        long sourceShapeId);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_history_summary(
        OcctModelingSafeHandle handle,
        long operationId,
        long sourceShapeId,
        out NativeModelTopologyHistorySummary result);
}
