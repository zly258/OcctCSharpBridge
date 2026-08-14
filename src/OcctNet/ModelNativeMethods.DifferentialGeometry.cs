using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_edge_parameter_range(OcctModelingSafeHandle handle, long edgeId, out NativeModelParameterRange result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_edge_differential(OcctModelingSafeHandle handle, long edgeId, double parameter, out NativeModelCurveDifferential result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_edge_curvature(OcctModelingSafeHandle handle, long edgeId, double parameter, double resolution, out NativeModelCurveCurvature result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_face_periodicity(OcctModelingSafeHandle handle, long faceId, out NativeModelSurfacePeriodicity result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_face_differential(OcctModelingSafeHandle handle, long faceId, double u, double v, double resolution, out NativeModelSurfaceDifferential result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_face_curvature(OcctModelingSafeHandle handle, long faceId, double u, double v, double resolution, out NativeModelSurfaceCurvature result);
}
