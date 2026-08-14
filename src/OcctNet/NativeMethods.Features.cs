namespace OcctNet;

internal static partial class NativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_boolean(OcctEngineSafeHandle handle, int operation, long leftId, long rightId, int hideInputs);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_extrude(OcctEngineSafeHandle handle, long profileId, OcctVector3d vector, int hideInput);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_revolve(OcctEngineSafeHandle handle, long profileId, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees, int hideInput);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_sweep(OcctEngineSafeHandle handle, long spineWireId, long profileId, int hideInputs);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_loft(OcctEngineSafeHandle handle, [In] long[] wireIds, int count, int makeSolid, int ruled, double tolerance, int hideInputs);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_fillet_all_edges(OcctEngineSafeHandle handle, long shapeId, double radius, int hideInput);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_chamfer_all_edges(OcctEngineSafeHandle handle, long shapeId, double distance, int hideInput);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_offset_shape(OcctEngineSafeHandle handle, long shapeId, double offset, double tolerance, int hideInput);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_thick_solid(OcctEngineSafeHandle handle, long solidId, int faceIndexToRemove, double thickness, double tolerance, int hideInput);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_fillet_edges(OcctEngineSafeHandle handle, long shapeId, [In] int[] edgeIndices, int count, double radius, int hideInput);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_chamfer_edges(OcctEngineSafeHandle handle, long shapeId, [In] int[] edgeIndices, int count, double distance, int hideInput);
}
