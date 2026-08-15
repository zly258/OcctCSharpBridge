using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class StepDocumentNativeMethods
{
    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_step_document_json_get(
        OcctEngineSafeHandle handle,
        [Out] byte[]? utf8Buffer,
        int capacity,
        out int requiredBytes);
}
