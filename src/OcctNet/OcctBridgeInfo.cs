using System.Runtime.InteropServices;
using System.Threading;

namespace OcctNet;

/// <summary>
/// Identifies the managed wrapper and validates the compatibility of the loaded native bridge.
/// </summary>
public static class OcctBridgeInfo
{
    public const int ExpectedAbiVersion = 5;
    public const string ManagedVersion = "3.0.0-preview.1";

    private static int _validated;

    public static int NativeAbiVersion => NativeMethods.occt_bridge_current_abi_version();
    public static string NativeVersion => ReadUtf8(NativeMethods.occt_bridge_version());
    public static string BuildInfo => ReadUtf8(NativeMethods.occt_bridge_build_info());
    public static string OcctVersion => ReadUtf8(NativeMethods.occt_version());

    internal static void EnsureCompatible()
    {
        if (Volatile.Read(ref _validated) != 0) return;

        var actualAbi = NativeAbiVersion;
        if (actualAbi != ExpectedAbiVersion)
        {
            throw new BadImageFormatException(
                $"OcctNative ABI mismatch. Managed wrapper requires ABI {ExpectedAbiVersion}, " +
                $"but the loaded native bridge reports ABI {actualAbi}. " +
                "Deploy OcctNet.dll and OcctNative.dll from the same build.");
        }

        var nativeVersionText = NativeVersion;
        if (!IsNativeVersionCompatible(nativeVersionText, ManagedVersion))
        {
            throw new BadImageFormatException(
                $"OcctNative version mismatch. Managed wrapper {ManagedVersion} requires native Bridge " +
                $"{ManagedVersion} or newer within ABI {ExpectedAbiVersion}, but the loaded native bridge reports " +
                $"'{nativeVersionText}'. Deploy OcctNet.dll and OcctNative.dll from the same build.");
        }

        Volatile.Write(ref _validated, 1);
    }

    internal static bool IsNativeVersionCompatible(string nativeVersion, string managedVersion)
    {
        if (!TryParseCoreVersion(nativeVersion, out var native)) return false;
        if (!TryParseCoreVersion(managedVersion, out var managed)) return false;
        return native >= managed;
    }

    private static bool TryParseCoreVersion(string value, out Version version)
    {
        var separator = value.IndexOf('-');
        var core = separator < 0 ? value : value[..separator];
        return Version.TryParse(core, out version!);
    }

    private static string ReadUtf8(IntPtr pointer) =>
        pointer == IntPtr.Zero ? string.Empty : Marshal.PtrToStringUTF8(pointer) ?? string.Empty;
}
