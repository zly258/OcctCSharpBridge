using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern NativeModelAlgorithmResult occt_model_unify_same_domain(OcctModelingSafeHandle handle, long shapeId, int unifyEdges, int unifyFaces, int concatBsplines);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern NativeModelAlgorithmResult occt_model_fix_shape(OcctModelingSafeHandle handle, long shapeId, double precision, double minTolerance, double maxTolerance);
}
