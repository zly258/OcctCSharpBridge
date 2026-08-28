using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_project_point_on_edge(
        OcctModelingSafeHandle handle,
        long edgeId,
        OcctPoint3d point,
        out OcctModelProjectionResult result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_project_point_on_face(
        OcctModelingSafeHandle handle,
        long faceId,
        OcctPoint3d point,
        out OcctModelProjectionResult result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_ray_intersections(
        OcctModelingSafeHandle handle,
        long shapeId,
        OcctPoint3d origin,
        OcctVector3d direction,
        double minimumParameter,
        double maximumParameter,
        double tolerance,
        out int resultCount);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_ray_hits_snapshot_get(
        OcctModelingSafeHandle handle,
        [Out, MarshalUsing(CountElementName = nameof(capacity))] NativeModelRayHit[]? results,
        int capacity,
        out int required);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_classify_point(
        OcctModelingSafeHandle handle,
        long solidId,
        OcctPoint3d point,
        double tolerance,
        out OcctModelState result);
}
