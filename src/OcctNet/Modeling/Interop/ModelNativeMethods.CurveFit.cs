using System.Runtime.InteropServices;

namespace OcctNet;

[StructLayout(LayoutKind.Sequential)]
internal struct NativeFitBSplineOptions
{
    public uint StructSize;
    public uint ApiVersion;
    public int DegMin;
    public int DegMax;
    public int Continuity;
    public double Tolerance;
    public int Periodic;
}

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_model_curve_fit_bspline(
        OcctModelingSafeHandle handle,
        [In] OcctPoint3d[] points,
        int count,
        in NativeFitBSplineOptions options,
        out long result);
}
