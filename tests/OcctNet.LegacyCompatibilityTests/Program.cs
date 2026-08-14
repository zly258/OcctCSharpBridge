using System.Runtime.InteropServices;

internal static class LegacyNativeMethods
{
    private const string LibraryName = "OcctNative";

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_bridge_abi_version();

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern IntPtr occt_create();

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void occt_destroy(IntPtr handle);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern IntPtr occt_model_create();

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void occt_model_destroy(IntPtr handle);
}

internal static class Program
{
    private static void Main()
    {
        if (LegacyNativeMethods.occt_bridge_abi_version() != 4)
            throw new InvalidOperationException("The frozen ABI 4 version query changed.");

        var engine = LegacyNativeMethods.occt_create();
        if (engine == IntPtr.Zero) throw new InvalidOperationException("ABI 4 engine creation failed.");
        LegacyNativeMethods.occt_destroy(engine);

        var modeling = LegacyNativeMethods.occt_model_create();
        if (modeling == IntPtr.Zero) throw new InvalidOperationException("ABI 4 modeling-session creation failed.");
        LegacyNativeMethods.occt_model_destroy(modeling);

        Console.WriteLine("Fixed ABI 4 consumer compatibility passed.");
    }
}
