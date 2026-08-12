using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_shape_face_analysis(
        IntPtr handle,
        long shapeId,
        [Out] NativeModelFaceAnalysis[]? items,
        int capacity,
        out int count);
}
