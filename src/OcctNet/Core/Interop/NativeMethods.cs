using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class NativeMethods
{
    internal const string LibraryName = "OcctNative";

    static NativeMethods()
    {
        OcctRuntime.Configure();
        NativeLibrary.SetDllImportResolver(typeof(NativeMethods).Assembly, ResolveLibrary);
    }

    private static IntPtr ResolveLibrary(
        string libraryName,
        Assembly assembly,
        DllImportSearchPath? searchPath)
    {
        if (!string.Equals(libraryName, LibraryName, StringComparison.OrdinalIgnoreCase))
            return IntPtr.Zero;

        var failures = new List<string>();
        foreach (var candidate in OcctRuntime.GetNativeLibraryCandidates())
        {
            if (!File.Exists(candidate))
                continue;

            if (OperatingSystem.IsWindows())
            {
                var handle = LoadLibraryW(candidate);
                if (handle != IntPtr.Zero)
                    return handle;

                var errorCode = Marshal.GetLastWin32Error();
                failures.Add($"{candidate} -> Win32 {errorCode}: {new Win32Exception(errorCode).Message}");
                continue;
            }

            if (NativeLibrary.TryLoad(candidate, assembly, searchPath, out var nativeHandle))
                return nativeHandle;

            failures.Add(candidate);
        }

        var nativeFileName = OperatingSystem.IsWindows()
            ? "OcctNative.dll"
            : OperatingSystem.IsLinux()
                ? "libOcctNative.so"
                : LibraryName;
        var details = failures.Count == 0
            ? $"{nativeFileName} was not found in the configured native search paths."
            : string.Join(Environment.NewLine, failures);

        throw new DllNotFoundException(
            $"Unable to load {nativeFileName} or one of its dependencies." +
            Environment.NewLine + details);
    }

    [LibraryImport("kernel32.dll", EntryPoint = "LoadLibraryW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    private static partial IntPtr LoadLibraryW(string fileName);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr occt_engine_create();

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void occt_engine_destroy(IntPtr handle);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_last_error_code(OcctEngineSafeHandle handle);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_last_error_message(
        OcctEngineSafeHandle handle,
        [Out] byte[]? buffer,
        int capacity,
        out int required);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr occt_version();

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int occt_bridge_current_abi_version();

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr occt_bridge_version();

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr occt_bridge_build_info();
}
