using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_brep_serialize(
        OcctModelingSafeHandle handle,
        long shapeId,
        [Out] byte[]? buffer,
        int capacity,
        out int required);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_brep_deserialize(
        OcctModelingSafeHandle handle,
        [In] byte[] buffer,
        int length,
        out long result);
}
