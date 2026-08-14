using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern NativeModelAlgorithmResult occt_model_extrude(OcctModelingSafeHandle handle, long profileId, OcctVector3d vector);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern NativeModelAlgorithmResult occt_model_revolve(OcctModelingSafeHandle handle, long profileId, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern NativeModelAlgorithmResult occt_model_sweep(OcctModelingSafeHandle handle, long spineWireId, long profileId);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern NativeModelAlgorithmResult occt_model_loft(OcctModelingSafeHandle handle, [In] long[] wireIds, int count, int makeSolid, int ruled, double tolerance);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern NativeModelAlgorithmResult occt_model_fillet_edges(OcctModelingSafeHandle handle, long shapeId, [In] int[] edgeIndices, int count, double radius);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern NativeModelAlgorithmResult occt_model_chamfer_edges(OcctModelingSafeHandle handle, long shapeId, [In] int[] edgeIndices, int count, double distance);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern NativeModelAlgorithmResult occt_model_offset(OcctModelingSafeHandle handle, long shapeId, double offset, double tolerance);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern NativeModelAlgorithmResult occt_model_thick_solid(OcctModelingSafeHandle handle, long solidId, [In] int[] faceIndices, int count, double thickness, double tolerance);
}
