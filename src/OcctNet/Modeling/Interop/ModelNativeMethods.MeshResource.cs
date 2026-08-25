using System.Runtime.InteropServices;

namespace OcctNet;

[StructLayout(LayoutKind.Sequential)]
internal struct NativeMeshBuildOptions
{
    public uint StructSize;
    public uint ApiVersion;
    public double LinearDeflection;
    public double AngularDeflection;
    public double MinSize;
    public int Relative;
    public int Parallel;
    public int InternalVertices;
    public int ControlSurfaceDeflection;
}

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_model_mesh_create(
        OcctModelingSafeHandle session,
        long shapeId,
        in NativeMeshBuildOptions options,
        out IntPtr result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void occt_mesh_release(IntPtr handle);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_mesh_get_counts(
        OcctMeshSafeHandle handle,
        out int nodeCount,
        out int triangleCount);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_mesh_nodes_copy(
        OcctMeshSafeHandle handle,
        [Out] NativeModelMeshNode[] results,
        int capacity,
        out int written);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_mesh_triangles_copy(
        OcctMeshSafeHandle handle,
        [Out] OcctModelMeshTriangle[] results,
        int capacity,
        out int written);

    [LibraryImport(LibraryName, EntryPoint = "occt_mesh_nodes_copy")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static unsafe partial OcctStatus MeshVerticesCopyToPointer(
        OcctMeshSafeHandle handle,
        OcctMeshVertex* results,
        int capacity,
        out int written);

    [LibraryImport(LibraryName, EntryPoint = "occt_mesh_triangles_copy")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static unsafe partial OcctStatus MeshTrianglesCopyToPointer(
        OcctMeshSafeHandle handle,
        OcctModelMeshTriangle* results,
        int capacity,
        out int written);
}
