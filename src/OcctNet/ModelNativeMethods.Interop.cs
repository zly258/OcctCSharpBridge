using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial long occt_model_display_in_engine(
        OcctEngineSafeHandle engineHandle,
        OcctModelingSafeHandle modelHandle,
        long shapeId,
        int fit);
}
