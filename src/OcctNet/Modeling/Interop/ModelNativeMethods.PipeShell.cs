using System.Runtime.InteropServices;

namespace OcctNet;

[StructLayout(LayoutKind.Sequential)]
internal struct NativePipeShellOptions
{
    public uint StructSize;
    public uint ApiVersion;
    public int Mode;
    public int ForceC1;
    public int MakeSolid;
    public OcctVector3d FixedNormal;
}

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_model_feature_pipe_shell_execute(
        OcctModelingSafeHandle handle,
        long spineWireId,
        [In] long[] profileIds,
        int profileCount,
        in NativePipeShellOptions options,
        out NativeModelAlgorithmResult result);
}
