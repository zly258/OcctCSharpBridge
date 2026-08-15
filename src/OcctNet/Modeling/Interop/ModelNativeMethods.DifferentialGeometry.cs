using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_edge_parameter_range(OcctModelingSafeHandle handle, long edgeId, out NativeModelParameterRange result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_edge_differential(OcctModelingSafeHandle handle, long edgeId, double parameter, out NativeModelCurveDifferential result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_edge_curvature(OcctModelingSafeHandle handle, long edgeId, double parameter, double resolution, out NativeModelCurveCurvature result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_face_periodicity(OcctModelingSafeHandle handle, long faceId, out NativeModelSurfacePeriodicity result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_face_differential(OcctModelingSafeHandle handle, long faceId, double u, double v, double resolution, out NativeModelSurfaceDifferential result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_face_curvature(OcctModelingSafeHandle handle, long faceId, double u, double v, double resolution, out NativeModelSurfaceCurvature result);
}
