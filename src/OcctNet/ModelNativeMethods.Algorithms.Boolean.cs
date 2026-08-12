using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern NativeModelAlgorithmResult occt_model_boolean(IntPtr handle, int operation, long leftId, long rightId, in NativeModelBooleanOptions options);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern NativeModelAlgorithmResult occt_model_split(IntPtr handle, [In] long[] objectIds, int objectCount, [In] long[] toolIds, int toolCount, in NativeModelBooleanOptions options);
}
