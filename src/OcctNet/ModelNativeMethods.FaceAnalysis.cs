using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int occt_model_shape_face_analysis(
        OcctModelingSafeHandle handle,
        long shapeId,
        [Out, MarshalUsing(CountElementName = nameof(capacity))] NativeModelFaceAnalysis[]? items,
        int capacity,
        out int count);
}
