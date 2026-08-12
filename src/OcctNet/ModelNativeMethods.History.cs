using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_history_generated_copy(IntPtr handle, long operationId, long sourceShapeId, [Out] long[]? results, int capacity);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_history_modified_copy(IntPtr handle, long operationId, long sourceShapeId, [Out] long[]? results, int capacity);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_history_is_removed(IntPtr handle, long operationId, long sourceShapeId);
}
