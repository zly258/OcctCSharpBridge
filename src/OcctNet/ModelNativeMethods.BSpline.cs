using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_edge_bspline_info(
        OcctModelingSafeHandle handle,
        long edgeId,
        out OcctModelBSplineCurveInfoNative result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_edge_bspline_pole_at(
        OcctModelingSafeHandle handle,
        long edgeId,
        int index,
        out OcctPoint3d pole,
        out double weight);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_edge_bspline_knot_at(
        OcctModelingSafeHandle handle,
        long edgeId,
        int index,
        out double knot,
        out int multiplicity);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_face_bspline_info(
        OcctModelingSafeHandle handle,
        long faceId,
        out OcctModelBSplineSurfaceInfoNative result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_face_bspline_pole_at(
        OcctModelingSafeHandle handle,
        long faceId,
        int uIndex,
        int vIndex,
        out OcctPoint3d pole,
        out double weight);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_face_bspline_u_knot_at(
        OcctModelingSafeHandle handle,
        long faceId,
        int index,
        out double knot,
        out int multiplicity);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_face_bspline_v_knot_at(
        OcctModelingSafeHandle handle,
        long faceId,
        int index,
        out double knot,
        out int multiplicity);
}
