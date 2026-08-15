using System.Runtime.InteropServices;

namespace OcctNet;

[StructLayout(LayoutKind.Sequential)]
internal struct NativeAlgorithmSummary
{
    public uint StructSize;
    public uint ApiVersion;
    public long OperationId;
    public int HasWarnings;
    public int HasErrors;
}

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_model_algorithm_acquire(
        OcctModelingSafeHandle session,
        long operationId,
        out IntPtr result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void occt_algorithm_release(IntPtr handle);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_algorithm_get_summary(
        OcctAlgorithmSafeHandle handle,
        ref NativeAlgorithmSummary result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_algorithm_report_copy(
        OcctAlgorithmSafeHandle handle,
        [Out] byte[]? result,
        int capacity,
        out int written);
}
