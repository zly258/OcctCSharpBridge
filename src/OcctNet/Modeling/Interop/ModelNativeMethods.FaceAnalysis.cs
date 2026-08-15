using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_shape_face_analysis_snapshot_get(
        OcctModelingSafeHandle handle,
        long shapeId,
        [Out, MarshalUsing(CountElementName = nameof(capacity))] NativeModelFaceAnalysis[]? items,
        int capacity,
        out int required);
}
