using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern long occt_model_display_in_engine(
        IntPtr engineHandle,
        IntPtr modelHandle,
        long shapeId,
        int fit);
}
