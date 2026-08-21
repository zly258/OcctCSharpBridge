using System.Runtime.InteropServices;
using System.Threading;

namespace OcctNet;

/// <summary>
/// Identifies the managed wrapper and validates the exact loaded native bridge contract.
/// </summary>
public static class OcctBridgeInfo
{
    /// <summary>The ABI version this managed wrapper was compiled against.</summary>
    public const int ExpectedAbiVersion = 5;

    /// <summary>The managed OcctNet bridge version (matches the native bridge build).</summary>
    public const string ManagedVersion = "3.0.0";

    // Use int for Interlocked.CompareExchange: 0 = not validated, 1 = validated
    private static int _validated;

    /// <summary>Gets the ABI version reported by the currently loaded native bridge.</summary>
    public static int NativeAbiVersion => NativeMethods.occt_bridge_current_abi_version();

    /// <summary>Gets the version string reported by the currently loaded native bridge (e.g. "3.0.0").</summary>
    public static string NativeVersion => ReadUtf8(NativeMethods.occt_bridge_version());

    /// <summary>Gets the native bridge build info string (compiler, date, configuration).</summary>
    public static string BuildInfo => ReadUtf8(NativeMethods.occt_bridge_build_info());

    /// <summary>Gets the Open CASCADE Technology version string used to build the native bridge.</summary>
    public static string OcctVersion => ReadUtf8(NativeMethods.occt_version());

    /// <summary>
    /// Ensures the loaded native ABI and version exactly match the managed wrapper.
    /// Thread-safe: only validates once; subsequent calls are a cheap volatile read.
    /// </summary>
    internal static void EnsureCompatible()
    {
        // Fast-path: already validated
        if (Volatile.Read(ref _validated) != 0) return;

        // Perform validation — even if two threads race here, the result is identical
        // (both would throw the same exception or both would succeed), so correctness
        // is preserved. We use CAS after to guarantee at most one write.
        ValidateNativeBridge();

        // Mark as validated (idempotent)
        Interlocked.CompareExchange(ref _validated, 1, 0);
    }

    private static void ValidateNativeBridge()
    {
        int actualAbi;
        try
        {
            actualAbi = NativeAbiVersion;
        }
        catch (Exception ex)
        {
            throw new BadImageFormatException(
                "Failed to query OcctNative ABI version. " +
                "Ensure OcctNative.dll is present and not corrupted.", ex);
        }

        if (actualAbi != ExpectedAbiVersion)
        {
            throw new BadImageFormatException(
                $"OcctNative ABI mismatch. Managed wrapper requires ABI {ExpectedAbiVersion}, " +
                $"but the loaded native bridge reports ABI {actualAbi}. " +
                "Deploy OcctNet.dll and OcctNative.dll from the same build.");
        }

        string nativeVersionText;
        try
        {
            nativeVersionText = NativeVersion;
        }
        catch (Exception ex)
        {
            throw new BadImageFormatException(
                "Failed to query OcctNative version string. " +
                "Ensure OcctNative.dll is present and not corrupted.", ex);
        }

        if (!string.Equals(nativeVersionText, ManagedVersion, StringComparison.Ordinal))
        {
            throw new BadImageFormatException(
                $"OcctNative version mismatch. Managed wrapper requires native Bridge {ManagedVersion}, " +
                $"but the loaded native bridge reports '{nativeVersionText}'. " +
                "Deploy OcctNet.dll and OcctNative.dll from the same build.");
        }
    }

    private static string ReadUtf8(IntPtr pointer) =>
        pointer == IntPtr.Zero ? string.Empty : Marshal.PtrToStringUTF8(pointer) ?? string.Empty;
}
