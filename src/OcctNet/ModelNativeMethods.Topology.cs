using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int occt_model_subshapes_copy(
        OcctModelingSafeHandle handle,
        long shapeId,
        int shapeType,
        [Out, MarshalUsing(CountElementName = nameof(capacity))] long[]? results,
        int capacity);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial long occt_model_outer_wire(OcctModelingSafeHandle handle, long faceId);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int occt_model_inner_wires_copy(
        OcctModelingSafeHandle handle,
        long faceId,
        [Out, MarshalUsing(CountElementName = nameof(capacity))] long[]? results,
        int capacity);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int occt_model_ancestors_copy(
        OcctModelingSafeHandle handle,
        long rootId,
        long childId,
        int ancestorType,
        [Out, MarshalUsing(CountElementName = nameof(capacity))] long[]? results,
        int capacity);
}
