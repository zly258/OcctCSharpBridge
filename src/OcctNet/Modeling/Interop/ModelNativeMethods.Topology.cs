using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_subshapes_snapshot_get(
        OcctModelingSafeHandle handle,
        long shapeId,
        int shapeType,
        [Out, MarshalUsing(CountElementName = nameof(capacity))] long[]? results,
        int capacity,
        out int required);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_outer_wire_get(
        OcctModelingSafeHandle handle,
        long faceId,
        out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_inner_wires_snapshot_get(
        OcctModelingSafeHandle handle,
        long faceId,
        [Out, MarshalUsing(CountElementName = nameof(capacity))] long[]? results,
        int capacity,
        out int required);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_wire_edges_snapshot_get(
        OcctModelingSafeHandle handle,
        long wireId,
        [Out, MarshalUsing(CountElementName = nameof(capacity))] long[]? results,
        int capacity,
        out int required);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_ancestors_snapshot_get(
        OcctModelingSafeHandle handle,
        long rootId,
        long childId,
        int ancestorType,
        [Out, MarshalUsing(CountElementName = nameof(capacity))] long[]? results,
        int capacity,
        out int required);
}
