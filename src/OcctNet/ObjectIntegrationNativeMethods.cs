using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class NativeMethods
{
    [LibraryImport(LibraryName, EntryPoint = "occt_engine_view_cube_language_set")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial OcctStatus occt_engine_view_cube_language_set_internal(
        OcctEngineSafeHandle handle,
        int language);

    internal static int occt_set_view_cube_language(OcctEngineSafeHandle handle, int language) =>
        occt_engine_view_cube_language_set_internal(handle, language) == OcctStatus.Ok ? 1 : 0;
}
