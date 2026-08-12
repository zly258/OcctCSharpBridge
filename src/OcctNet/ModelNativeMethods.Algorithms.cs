using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern NativeModelAlgorithmResult occt_model_boolean(IntPtr handle, int operation, long leftId, long rightId, in OcctModelBooleanOptions options);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern NativeModelAlgorithmResult occt_model_split(IntPtr handle, [In] long[] objectIds, int objectCount, [In] long[] toolIds, int toolCount, in OcctModelBooleanOptions options);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern NativeModelAlgorithmResult occt_model_extrude(IntPtr handle, long profileId, OcctVector3d vector);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern NativeModelAlgorithmResult occt_model_revolve(IntPtr handle, long profileId, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern NativeModelAlgorithmResult occt_model_sweep(IntPtr handle, long spineWireId, long profileId);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern NativeModelAlgorithmResult occt_model_loft(IntPtr handle, [In] long[] wireIds, int count, int makeSolid, int ruled, double tolerance);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern NativeModelAlgorithmResult occt_model_fillet_edges(IntPtr handle, long shapeId, [In] int[] edgeIndices, int count, double radius);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern NativeModelAlgorithmResult occt_model_chamfer_edges(IntPtr handle, long shapeId, [In] int[] edgeIndices, int count, double distance);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern NativeModelAlgorithmResult occt_model_offset(IntPtr handle, long shapeId, double offset, double tolerance);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern NativeModelAlgorithmResult occt_model_thick_solid(IntPtr handle, long solidId, [In] int[] faceIndices, int count, double thickness, double tolerance);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern NativeModelAlgorithmResult occt_model_unify_same_domain(IntPtr handle, long shapeId, int unifyEdges, int unifyFaces, int concatBsplines);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern NativeModelAlgorithmResult occt_model_fix_shape(IntPtr handle, long shapeId, double precision, double minTolerance, double maxTolerance);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_history_generated_count(IntPtr handle, long operationId, long sourceShapeId);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_model_history_generated_at(IntPtr handle, long operationId, long sourceShapeId, int index);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_history_modified_count(IntPtr handle, long operationId, long sourceShapeId);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_model_history_modified_at(IntPtr handle, long operationId, long sourceShapeId, int index);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_history_is_removed(IntPtr handle, long operationId, long sourceShapeId);
}
