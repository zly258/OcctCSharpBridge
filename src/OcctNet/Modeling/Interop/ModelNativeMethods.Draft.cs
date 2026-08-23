using System.Runtime.InteropServices;

namespace OcctNet;

[StructLayout(LayoutKind.Sequential)]
internal struct NativeDraftOptions
{
    public uint StructSize;
    public uint ApiVersion;
    public double AngleDegrees;
    public OcctVector3d PullDirection;
    public OcctPoint3d  NeutralPlanePoint;
    public OcctVector3d NeutralPlaneNormal;
}

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_model_feature_draft_execute(
        OcctModelingSafeHandle handle,
        long solidId,
        [In] int[] faceIndices,
        int faceCount,
        in NativeDraftOptions options,
        out NativeModelAlgorithmResult result);
}
