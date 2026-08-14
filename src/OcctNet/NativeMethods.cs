using System.ComponentModel;
using System.Reflection;

namespace OcctNet;

internal static partial class NativeMethods
{
    private const string LibraryName = "OcctNative";

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
                var handle = LoadLibrary(candidate);
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

        var details = failures.Count == 0
            ? "OcctNative.dll was not found in the application directory."
            : string.Join(Environment.NewLine, failures);

        throw new DllNotFoundException(
            "Unable to load OcctNative.dll or one of its dependencies." +
            Environment.NewLine + details);
    }

    [DllImport("kernel32.dll", EntryPoint = "LoadLibraryW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr LoadLibrary(string fileName);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern IntPtr occt_create();
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern void occt_destroy(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern IntPtr occt_last_error(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern OcctEngineSafeHandle occt_engine_create();
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern void occt_engine_destroy(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern OcctStatus occt_engine_last_error_code(OcctEngineSafeHandle handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern OcctStatus occt_engine_last_error_message(OcctEngineSafeHandle handle, [Out] byte[]? buffer, int capacity, out int required);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern IntPtr occt_version();
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_bridge_abi_version();
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_bridge_current_abi_version();
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern IntPtr occt_bridge_version();
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern IntPtr occt_bridge_build_info();
}
