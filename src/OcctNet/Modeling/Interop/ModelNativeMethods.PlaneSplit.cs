using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

[StructLayout(LayoutKind.Sequential)]
internal struct NativeModelPlaneSplitResult
{
    internal long PositiveShapeId;
    internal long NegativeShapeId;
    internal long SectionShapeId;
}

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_split_by_plane(
        OcctModelingSafeHandle handle,
        long shapeId,
        OcctPoint3d origin,
        OcctVector3d normal,
        out NativeModelPlaneSplitResult result);
}
