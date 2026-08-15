using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial long occt_model_shape_hash(OcctModelingSafeHandle handle, long shapeId);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int occt_model_shape_type(OcctModelingSafeHandle handle, long shapeId);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int occt_model_shape_orientation(OcctModelingSafeHandle handle, long shapeId);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int occt_model_shape_is_closed(OcctModelingSafeHandle handle, long shapeId);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int occt_model_shape_is_valid(OcctModelingSafeHandle handle, long shapeId);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial double occt_model_shape_tolerance(OcctModelingSafeHandle handle, long shapeId);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int occt_model_shape_bounds(OcctModelingSafeHandle handle, long shapeId, out OcctBounds result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int occt_model_shape_linear_properties(OcctModelingSafeHandle handle, long shapeId, out OcctMassProperties result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int occt_model_shape_surface_properties(OcctModelingSafeHandle handle, long shapeId, out OcctMassProperties result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int occt_model_shape_volume_properties(OcctModelingSafeHandle handle, long shapeId, out OcctMassProperties result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int occt_model_shape_distance(OcctModelingSafeHandle handle, long firstId, long secondId, out OcctDistanceResult result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr occt_model_check_report(OcctModelingSafeHandle handle, long shapeId);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int occt_model_get_location(OcctModelingSafeHandle handle, long shapeId, out OcctModelLocation result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial long occt_model_set_location(OcctModelingSafeHandle handle, long shapeId, in OcctModelLocation location, int copyShape);
}
