using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

[StructLayout(LayoutKind.Sequential)]
internal struct NativeManipulatorAttachOptionsV1
{
    internal uint StructSize;
    internal uint ApiVersion;
    internal int AdjustPosition;
    internal int AdjustSize;
    internal int EnableModes;
}

[Flags]
internal enum NativeManipulatorUpdateMask : uint
{
    Part = 1u << 0,
    ModeEnabled = 1u << 1,
    ModeActivationOnDetection = 1u << 2,
    Position = 1u << 3,
    Size = 1u << 4,
    Gap = 1u << 5,
    ZoomPersistence = 1u << 6,
    Skin = 1u << 7
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeManipulatorUpdateOptions
{
    internal uint StructSize;
    internal uint ApiVersion;
    internal NativeManipulatorUpdateMask UpdateMask;
    internal int AxisIndex;
    internal int Mode;
    internal int Enabled;
    internal OcctPoint3d Origin;
    internal OcctVector3d Normal;
    internal OcctVector3d XDirection;
    internal double Size;
    internal double Gap;
    internal int SkinMode;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeManipulatorStateV1
{
    internal uint StructSize;
    internal uint ApiVersion;
    internal int Attached;
    internal int ActiveMode;
    internal int ActiveAxisIndex;
    internal int HasActiveTransformation;
    internal int ModeActivationOnDetection;
    internal int ZoomPersistence;
    internal int SkinMode;
    internal OcctPoint3d Origin;
    internal OcctVector3d Normal;
    internal OcctVector3d XDirection;
    internal double Size;
}

internal enum NativeManipulatorTransformAction
{
    Start = 0,
    Update = 1,
    Stop = 2,
    DeactivateMode = 3
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeManipulatorTransformOptions
{
    internal uint StructSize;
    internal uint ApiVersion;
    internal NativeManipulatorTransformAction Action;
    internal int X;
    internal int Y;
    internal int Apply;
}

internal static partial class ManipulatorNativeMethods
{
    private const string LibraryName = "OcctNative";

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_manipulator_create(
        OcctEngineSafeHandle handle,
        out long manipulatorId);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_manipulator_attach(
        OcctEngineSafeHandle handle,
        long manipulatorId,
        IntPtr objectIds,
        int count,
        in NativeManipulatorAttachOptionsV1 options);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_manipulator_detach(
        OcctEngineSafeHandle handle,
        long manipulatorId);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_manipulator_update(
        OcctEngineSafeHandle handle,
        long manipulatorId,
        in NativeManipulatorUpdateOptions options);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_manipulator_state_get(
        OcctEngineSafeHandle handle,
        long manipulatorId,
        out NativeManipulatorStateV1 result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_manipulator_targets_get(
        OcctEngineSafeHandle handle,
        long manipulatorId,
        IntPtr objectIds,
        int capacity,
        out int count);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_manipulator_transform(
        OcctEngineSafeHandle handle,
        long manipulatorId,
        in NativeManipulatorTransformOptions options);
}
