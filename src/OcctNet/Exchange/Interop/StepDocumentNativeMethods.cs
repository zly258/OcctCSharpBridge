using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

[StructLayout(LayoutKind.Sequential)]
internal struct NativeStepTransform3d
{
    public double M00; public double M01; public double M02; public double M03;
    public double M10; public double M11; public double M12; public double M13;
    public double M20; public double M21; public double M22; public double M23;
}


internal static partial class StepDocumentNativeMethods
{
    [LibraryImport(NativeMethods.LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_step_document_json_get(
        OcctEngineSafeHandle handle,
        [Out] byte[]? utf8Buffer,
        int capacity,
        out int requiredBytes);

    [LibraryImport(NativeMethods.LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_step_node_name_set(
        OcctEngineSafeHandle handle,
        string nodeId,
        string utf8Name);

    [LibraryImport(NativeMethods.LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_step_node_visibility_set(
        OcctEngineSafeHandle handle,
        string nodeId,
        int visible);

    [LibraryImport(NativeMethods.LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_step_node_transform_set(
        OcctEngineSafeHandle handle,
        string nodeId,
        in NativeStepTransform3d transform);

    [LibraryImport(NativeMethods.LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_step_component_add(
        OcctEngineSafeHandle handle,
        string parentNodeId,
        string referenceNodeId,
        in NativeStepTransform3d transform,
        out long viewerObjectId);

    [LibraryImport(NativeMethods.LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_step_component_remove(
        OcctEngineSafeHandle handle,
        string componentNodeId);
}
