using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

[StructLayout(LayoutKind.Sequential)]
internal struct NativeModelHlrResult
{
    internal long VisibleShapeId;
    internal long HiddenShapeId;
    internal long OutlineShapeId;
    internal long VisibleSharpShapeId;
    internal long HiddenSharpShapeId;
}

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_hlr_project(
        OcctModelingSafeHandle handle,
        long shapeId,
        OcctVector3d viewDirection,
        OcctVector3d upDirection,
        out NativeModelHlrResult result);
}
