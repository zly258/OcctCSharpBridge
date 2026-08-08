using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern long occt_model_shape_free_bounds(
        IntPtr handle,
        long shapeId,
        double tolerance,
        int boundaryKind,
        int splitClosed,
        int splitOpen);
}
