using System.Runtime.InteropServices;

namespace OcctNet;

[StructLayout(LayoutKind.Sequential)]
public struct OcctEdgeFilletSpec
{
    public int EdgeIndex;
    public double R1;
    public double R2;

    public OcctEdgeFilletSpec(int edgeIndex, double r1, double r2)
    { EdgeIndex = edgeIndex; R1 = r1; R2 = r2; }
}

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_model_feature_fillet_variable_execute(
        OcctModelingSafeHandle handle,
        long shapeId,
        [In] OcctEdgeFilletSpec[] specs,
        int count,
        out NativeModelAlgorithmResult result);
}
