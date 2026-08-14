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
    internal static extern int occt_initialize(IntPtr handle, IntPtr windowHandle);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern long occt_make_box(IntPtr handle, double x, double y, double z, double dx, double dy, double dz);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_fit_all(IntPtr handle);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_redraw(IntPtr handle);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern IntPtr occt_model_create();

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void occt_model_destroy(IntPtr handle);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern long occt_model_make_box(IntPtr handle, double x, double y, double z, double dx, double dy, double dz);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_shape_exists(IntPtr handle, long shapeId);
}

internal static class LegacyWindow
{
    private const int WsOverlappedWindow = 0x00CF0000;

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateWindowExW(
        int extendedStyle,
        string className,
        string windowName,
        int style,
        int x,
        int y,
        int width,
        int height,
        IntPtr parent,
        IntPtr menu,
        IntPtr instance,
        IntPtr parameter);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyWindow(IntPtr handle);

    internal static IntPtr Create()
    {
        var handle = CreateWindowExW(0, "STATIC", "OcctBridge ABI 4 compatibility", WsOverlappedWindow, 0, 0, 320, 240, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        return handle != IntPtr.Zero
            ? handle
            : throw new InvalidOperationException($"Unable to create the ABI 4 compatibility window. Win32 error: {Marshal.GetLastWin32Error()}.");
    }

    internal static void Destroy(IntPtr handle)
    {
        if (handle != IntPtr.Zero && !DestroyWindow(handle))
            throw new InvalidOperationException($"Unable to destroy the ABI 4 compatibility window. Win32 error: {Marshal.GetLastWin32Error()}.");
    }
}

internal static class Program
{
    private static void Main()
    {
        if (LegacyNativeMethods.occt_bridge_abi_version() != 4)
            throw new InvalidOperationException("The frozen ABI 4 version query changed.");
        Console.WriteLine("[compatibility] Frozen ABI query passed.");

        var window = LegacyWindow.Create();
        Console.WriteLine("[compatibility] Native test window created.");
        try
        {
            var engine = LegacyNativeMethods.occt_create();
            if (engine == IntPtr.Zero) throw new InvalidOperationException("ABI 4 engine creation failed.");
            try
            {
                Console.WriteLine("[compatibility] Initializing legacy viewer.");
                if (LegacyNativeMethods.occt_initialize(engine, window) == 0) throw new InvalidOperationException("ABI 4 viewer initialization failed.");
                Console.WriteLine("[compatibility] Creating and rendering legacy viewer shape.");
                if (LegacyNativeMethods.occt_make_box(engine, 0, 0, 0, 10, 20, 30) <= 0) throw new InvalidOperationException("ABI 4 viewer modeling failed.");
                if (LegacyNativeMethods.occt_fit_all(engine) == 0) throw new InvalidOperationException("ABI 4 fit-all failed.");
                if (LegacyNativeMethods.occt_redraw(engine) == 0) throw new InvalidOperationException("ABI 4 redraw failed.");
            }
            finally
            {
                LegacyNativeMethods.occt_destroy(engine);
            }

            var modeling = LegacyNativeMethods.occt_model_create();
            if (modeling == IntPtr.Zero) throw new InvalidOperationException("ABI 4 modeling-session creation failed.");
            try
            {
                Console.WriteLine("[compatibility] Running legacy headless modeling.");
                var shape = LegacyNativeMethods.occt_model_make_box(modeling, 0, 0, 0, 4, 5, 6);
                if (shape <= 0 || LegacyNativeMethods.occt_model_shape_exists(modeling, shape) == 0)
                    throw new InvalidOperationException("ABI 4 headless modeling failed.");
            }
            finally
            {
                LegacyNativeMethods.occt_model_destroy(modeling);
            }
        }
        finally
        {
            LegacyWindow.Destroy(window);
        }

        Console.WriteLine("Fixed ABI 4 create/initialize/model/render/dispose compatibility passed.");
    }
}
