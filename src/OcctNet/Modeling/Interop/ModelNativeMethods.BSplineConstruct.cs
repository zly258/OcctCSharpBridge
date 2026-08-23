using System.Runtime.InteropServices;

namespace OcctNet;

[StructLayout(LayoutKind.Sequential)]
internal struct NativeBSplineCurveDefinition
{
    public uint StructSize;
    public uint ApiVersion;
    public int Degree;
    public int PoleCount;
    public int KnotCount;
    public int Rational;   // OcctBool
    public int Periodic;   // OcctBool
}

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_model_curve_bspline_explicit_create(
        OcctModelingSafeHandle handle,
        in NativeBSplineCurveDefinition def,
        [In] OcctPoint3d[] poles,
        [In] double[]? weights,
        [In] double[] knots,
        [In] int[] multiplicities,
        out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_model_face_bspline_explicit_create(
        OcctModelingSafeHandle handle,
        in NativeBSplineSurfaceDefinition def,
        [In] OcctPoint3d[] poles,
        [In] double[]? weights,
        [In] double[] uKnots, [In] int[] uMults,
        [In] double[] vKnots, [In] int[] vMults,
        out long result);
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeBSplineSurfaceDefinition
{
    public uint StructSize; public uint ApiVersion;
    public int UDegree; public int VDegree;
    public int UPoleCount; public int VPoleCount;
    public int UKnotCount; public int VKnotCount;
    public int URational; public int VRational;
    public int UPeriodic; public int VPeriodic;
}
