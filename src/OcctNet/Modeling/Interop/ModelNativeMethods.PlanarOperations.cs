using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_make_face_with_holes(
        OcctModelingSafeHandle handle,
        long outerWireId,
        [MarshalUsing(CountElementName = nameof(innerWireCount))] long[] innerWireIds,
        int innerWireCount,
        out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_trim_edge(
        OcctModelingSafeHandle handle,
        long edgeId,
        double firstParameter,
        double lastParameter,
        out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_offset_wire(
        OcctModelingSafeHandle handle,
        long wireId,
        double offset,
        double altitude,
        int joinType,
        int openResult,
        out long result);
}
