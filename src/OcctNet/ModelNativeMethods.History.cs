using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_history_generated_count(IntPtr handle, long operationId, long sourceShapeId);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_model_history_generated_at(IntPtr handle, long operationId, long sourceShapeId, int index);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_history_generated_copy(IntPtr handle, long operationId, long sourceShapeId, [Out] long[] results, int capacity);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_history_modified_count(IntPtr handle, long operationId, long sourceShapeId);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_model_history_modified_at(IntPtr handle, long operationId, long sourceShapeId, int index);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_history_modified_copy(IntPtr handle, long operationId, long sourceShapeId, [Out] long[] results, int capacity);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_history_is_removed(IntPtr handle, long operationId, long sourceShapeId);
}
