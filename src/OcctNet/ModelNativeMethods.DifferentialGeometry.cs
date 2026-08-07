using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_edge_parameter_range(IntPtr handle, long edgeId, out NativeModelParameterRange result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_edge_differential(IntPtr handle, long edgeId, double parameter, out NativeModelCurveDifferential result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_edge_curvature(IntPtr handle, long edgeId, double parameter, double resolution, out NativeModelCurveCurvature result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_face_periodicity(IntPtr handle, long faceId, out NativeModelSurfacePeriodicity result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_face_differential(IntPtr handle, long faceId, double u, double v, double resolution, out NativeModelSurfaceDifferential result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_face_curvature(IntPtr handle, long faceId, double u, double v, double resolution, out NativeModelSurfaceCurvature result);
}
