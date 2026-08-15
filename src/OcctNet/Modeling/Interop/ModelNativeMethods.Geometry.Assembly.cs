using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_assembly_compound_create(
        OcctModelingSafeHandle handle,
        [In] long[] shapeIds,
        int count,
        out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_assembly_wire_create(
        OcctModelingSafeHandle handle,
        [In] long[] edgeIds,
        int count,
        out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_assembly_sew(
        OcctModelingSafeHandle handle,
        [In] long[] shapeIds,
        int count,
        double tolerance,
        out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_assembly_solid_from_shell_create(
        OcctModelingSafeHandle handle,
        long shellId,
        out long result);
}
