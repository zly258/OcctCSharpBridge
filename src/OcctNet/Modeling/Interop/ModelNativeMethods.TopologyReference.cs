using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_create_topology_reference(
        OcctModelingSafeHandle handle,
        long rootShapeId,
        long subshapeId,
        out NativeModelTopologyReference result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_resolve_topology_reference(
        OcctModelingSafeHandle handle,
        long rootShapeId,
        in NativeModelTopologyReference reference,
        double matchingTolerance,
        out NativeModelTopologyReferenceResult result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_resolve_topology_reference_with_history(
        OcctModelingSafeHandle handle,
        long rootShapeId,
        long operationId,
        long sourceShapeId,
        in NativeModelTopologyReference reference,
        double matchingTolerance,
        out NativeModelTopologyReferenceResult result);
}
