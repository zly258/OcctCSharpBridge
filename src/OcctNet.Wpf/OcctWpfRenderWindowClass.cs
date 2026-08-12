using System.Runtime.InteropServices;

namespace OcctNet;

internal static class OcctWpfRenderWindowClass
{
    internal const string Name = "OcctNet.Wpf.RenderHost";

    private const uint CsOwnDc = 0x0020;
    private const uint WmEraseBkgnd = 0x0014;
    private const int ErrorClassAlreadyExists = 1410;
    private const int IdcArrow = 32512;

    private static readonly object SyncRoot = new();
    private static readonly WindowProcedure WindowProc = DispatchWindowMessage;
    private static bool _registered;
    private static IntPtr _moduleHandle;

    internal static IntPtr ModuleHandle
    {
        get
        {
            EnsureRegistered();
            return _moduleHandle;
        }
    }

    internal static void EnsureRegistered()
    {
        if (_registered) return;
        lock (SyncRoot)
        {
            if (_registered) return;

            _moduleHandle = GetModuleHandleW(null);
            if (_moduleHandle == IntPtr.Zero)
                throw new InvalidOperationException($"Unable to resolve the current Win32 module. Error: {Marshal.GetLastWin32Error()}.");

            var windowClass = new WndClassEx
            {
                Size = (uint)Marshal.SizeOf<WndClassEx>(),
                Style = CsOwnDc,
                WindowProcedure = Marshal.GetFunctionPointerForDelegate(WindowProc),
                ClassExtraBytes = 0,
                WindowExtraBytes = 0,
                Instance = _moduleHandle,
                Icon = IntPtr.Zero,
                Cursor = LoadCursorW(IntPtr.Zero, new IntPtr(IdcArrow)),
                BackgroundBrush = IntPtr.Zero,
                MenuName = null,
                ClassName = Name,
                SmallIcon = IntPtr.Zero
            };

            if (RegisterClassExW(ref windowClass) == 0)
            {
                var error = Marshal.GetLastWin32Error();
                if (error != ErrorClassAlreadyExists)
                    throw new InvalidOperationException($"Unable to register the WPF OCCT render window class. Win32 error: {error}.");
            }

            _registered = true;
        }
    }

    private static IntPtr DispatchWindowMessage(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam)
    {
        // OpenGL owns the complete client area. There is deliberately no class
        // background brush and no STATIC-control text/paint behavior.
        if (message == WmEraseBkgnd) return new IntPtr(1);
        return DefWindowProcW(hwnd, message, wParam, lParam);
    }

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate IntPtr WindowProcedure(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WndClassEx
    {
        internal uint Size;
        internal uint Style;
        internal IntPtr WindowProcedure;
        internal int ClassExtraBytes;
        internal int WindowExtraBytes;
        internal IntPtr Instance;
        internal IntPtr Icon;
        internal IntPtr Cursor;
        internal IntPtr BackgroundBrush;
        [MarshalAs(UnmanagedType.LPWStr)] internal string? MenuName;
        [MarshalAs(UnmanagedType.LPWStr)] internal string ClassName;
        internal IntPtr SmallIcon;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr GetModuleHandleW(string? moduleName);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern ushort RegisterClassExW(ref WndClassEx windowClass);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr DefWindowProcW(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr LoadCursorW(IntPtr instance, IntPtr cursorName);
}
