using System.Runtime.InteropServices;
using System.Threading;

namespace OcctNet;

/// <summary>
/// Identifies the managed wrapper and validates the ABI of the loaded native bridge.
/// </summary>
public static class OcctBridgeInfo
{
    public const int ExpectedAbiVersion = 2;
    public const string ManagedVersion = "2.4.0";

    private static int _validated;

    public static int NativeAbiVersion => NativeMethods.occt_bridge_abi_version();
    public static string NativeVersion => ReadUtf8(NativeMethods.occt_bridge_version());
    public static string BuildInfo => ReadUtf8(NativeMethods.occt_bridge_build_info());
    public static string OcctVersion => ReadUtf8(NativeMethods.occt_version());

    internal static void EnsureCompatible()
    {
        if (Volatile.Read(ref _validated) != 0) return;

        var actual = NativeAbiVersion;
        if (actual != ExpectedAbiVersion)
        {
            throw new BadImageFormatException(
                $"OcctNative ABI mismatch. Managed wrapper requires ABI {ExpectedAbiVersion}, " +
                $"but the loaded native bridge reports ABI {actual}. " +
                "Deploy OcctNet.dll and OcctNative.dll from the same build.");
        }

        Volatile.Write(ref _validated, 1);
    }

    private static string ReadUtf8(IntPtr pointer) =>
        pointer == IntPtr.Zero ? string.Empty : Marshal.PtrToStringUTF8(pointer) ?? string.Empty;
}
