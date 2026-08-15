using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ViewerExchangeNativeMethods
{
    [LibraryImport(NativeMethods.LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_exchange_import_step(OcctEngineSafeHandle handle, string utf8Path, out long result);

    [LibraryImport(NativeMethods.LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_exchange_import_iges(OcctEngineSafeHandle handle, string utf8Path, out long result);

    [LibraryImport(NativeMethods.LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_exchange_import_brep(OcctEngineSafeHandle handle, string utf8Path, out long result);

    [LibraryImport(NativeMethods.LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_exchange_import_stl(OcctEngineSafeHandle handle, string utf8Path, out long result);

    [LibraryImport(NativeMethods.LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_exchange_import_file(OcctEngineSafeHandle handle, string utf8Path, out long result);

    [LibraryImport(NativeMethods.LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_exchange_export_step(OcctEngineSafeHandle handle, long objectId, string utf8Path);

    [LibraryImport(NativeMethods.LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_exchange_export_all_step(OcctEngineSafeHandle handle, string utf8Path);

    [LibraryImport(NativeMethods.LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_exchange_export_iges(OcctEngineSafeHandle handle, long objectId, string utf8Path);

    [LibraryImport(NativeMethods.LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_exchange_export_all_iges(OcctEngineSafeHandle handle, string utf8Path);

    [LibraryImport(NativeMethods.LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_exchange_export_brep(OcctEngineSafeHandle handle, long objectId, string utf8Path);

    [LibraryImport(NativeMethods.LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_exchange_export_stl(OcctEngineSafeHandle handle, long objectId, string utf8Path, double linearDeflection, double angularDeflection, int asciiMode);
}
