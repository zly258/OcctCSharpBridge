using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_mesh(
        OcctModelingSafeHandle handle,
        long shapeId,
        in NativeModelMeshParameters parameters);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_clear_mesh(
        OcctModelingSafeHandle handle,
        long shapeId);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_face_mesh_nodes_snapshot_get(
        OcctModelingSafeHandle handle,
        long faceId,
        [Out, MarshalUsing(CountElementName = nameof(capacity))] NativeModelMeshNode[]? results,
        int capacity,
        out int required);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_face_mesh_triangles_snapshot_get(
        OcctModelingSafeHandle handle,
        long faceId,
        [Out, MarshalUsing(CountElementName = nameof(capacity))] OcctModelMeshTriangle[]? results,
        int capacity,
        out int required);

    [LibraryImport(LibraryName, EntryPoint = "occt_model_face_mesh_nodes_snapshot_get")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial OcctStatus FaceMeshVerticesCopyToPointer(
        OcctModelingSafeHandle handle,
        long faceId,
        OcctMeshVertex* results,
        int capacity,
        out int required);

    [LibraryImport(LibraryName, EntryPoint = "occt_model_face_mesh_triangles_snapshot_get")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial OcctStatus FaceMeshTrianglesCopyToPointer(
        OcctModelingSafeHandle handle,
        long faceId,
        OcctModelMeshTriangle* results,
        int capacity,
        out int required);
}
