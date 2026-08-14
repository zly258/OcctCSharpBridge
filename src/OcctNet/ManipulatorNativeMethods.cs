using System.Runtime.InteropServices;

namespace OcctNet;

internal static class ManipulatorNativeMethods
{
    private const string LibraryName = "OcctNative";

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern long occt_add_manipulator(OcctEngineSafeHandle handle);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_attach_manipulator(
        OcctEngineSafeHandle handle,
        long manipulatorId,
        [In] long[] objectIds,
        int count,
        in NativeOcctManipulatorAttachOptions options);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_detach_manipulator(OcctEngineSafeHandle handle, long manipulatorId);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_manipulator_part(
        OcctEngineSafeHandle handle,
        long manipulatorId,
        int axisIndex,
        int mode,
        int enabled);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_manipulator_mode_enabled(
        OcctEngineSafeHandle handle,
        long manipulatorId,
        int mode,
        int enabled);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_manipulator_mode_activation_on_detection(
        OcctEngineSafeHandle handle,
        long manipulatorId,
        int enabled);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_manipulator_position(
        OcctEngineSafeHandle handle,
        long manipulatorId,
        OcctPoint3d origin,
        OcctVector3d normal,
        OcctVector3d xDirection);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_manipulator_size(OcctEngineSafeHandle handle, long manipulatorId, double size);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_manipulator_gap(OcctEngineSafeHandle handle, long manipulatorId, double gap);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_manipulator_zoom_persistence(OcctEngineSafeHandle handle, long manipulatorId, int enabled);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_manipulator_skin(OcctEngineSafeHandle handle, long manipulatorId, int skinMode);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_get_manipulator_state(
        OcctEngineSafeHandle handle,
        long manipulatorId,
        out NativeOcctManipulatorState result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_get_manipulator_objects(
        OcctEngineSafeHandle handle,
        long manipulatorId,
        [Out] long[]? objectIds,
        int capacity,
        out int count);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_start_manipulator_transform(OcctEngineSafeHandle handle, long manipulatorId, int x, int y);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_update_manipulator_transform(OcctEngineSafeHandle handle, long manipulatorId, int x, int y);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_stop_manipulator_transform(OcctEngineSafeHandle handle, long manipulatorId, int apply);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_deactivate_manipulator_mode(OcctEngineSafeHandle handle, long manipulatorId);
}
