using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_history_generated_copy(OcctModelingSafeHandle handle, long operationId, long sourceShapeId, [Out] long[]? results, int capacity);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_history_modified_copy(OcctModelingSafeHandle handle, long operationId, long sourceShapeId, [Out] long[]? results, int capacity);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_model_history_is_removed(OcctModelingSafeHandle handle, long operationId, long sourceShapeId);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern OcctStatus occt_model_history_summary(OcctModelingSafeHandle handle, long operationId, long sourceShapeId, out NativeModelTopologyHistorySummary result);
}
