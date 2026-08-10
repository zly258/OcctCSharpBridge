using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_create_topology_reference(
        IntPtr handle,
        long rootShapeId,
        long subshapeId,
        out NativeModelTopologyReference result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_resolve_topology_reference(
        IntPtr handle,
        long rootShapeId,
        in NativeModelTopologyReference reference,
        double matchingTolerance,
        out NativeModelTopologyReferenceResult result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_resolve_topology_reference_with_history(
        IntPtr handle,
        long rootShapeId,
        long operationId,
        long sourceShapeId,
        in NativeModelTopologyReference reference,
        double matchingTolerance,
        out NativeModelTopologyReferenceResult result);
}
