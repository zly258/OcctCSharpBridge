using System.Runtime.InteropServices;

namespace OcctNet;

public sealed partial class OcctWpfViewport
{
    private const int WsChild = 0x40000000;
    private const int WsVisible = 0x10000000;
    private const int WsClipSiblings = 0x04000000;
    private const int WsClipChildren = 0x02000000;
    private const int HtClient = 1;

    private const int WmSize = 0x0005;
    private const int WmSetFocus = 0x0007;
    private const int WmKillFocus = 0x0008;
    private const int WmPaint = 0x000F;
    private const int WmEraseBkgnd = 0x0014;
    private const int WmCancelMode = 0x001F;
    private const int WmNcHitTest = 0x0084;
    private const int WmKeyDown = 0x0100;
    private const int WmKeyUp = 0x0101;
    private const int WmSysKeyDown = 0x0104;
    private const int WmSysKeyUp = 0x0105;
    private const int WmMouseMove = 0x0200;
    private const int WmLButtonDown = 0x0201;
    private const int WmLButtonUp = 0x0202;
    private const int WmRButtonDown = 0x0204;
    private const int WmRButtonUp = 0x0205;
    private const int WmMButtonDown = 0x0207;
    private const int WmMButtonUp = 0x0208;
    private const int WmMouseWheel = 0x020A;
    private const int WmCaptureChanged = 0x0215;
    private const int WmDpiChanged = 0x02E0;

    private const int MkLButton = 0x0001;
    private const int MkRButton = 0x0002;
    private const int MkMButton = 0x0010;

    private const int VkBack = 0x08;
    private const int VkTab = 0x09;
    private const int VkReturn = 0x0D;
    private const int VkShift = 0x10;
    private const int VkControl = 0x11;
    private const int VkMenu = 0x12;
    private const int VkEscape = 0x1B;
    private const int VkSpace = 0x20;
    private const int VkPageUp = 0x21;
    private const int VkPageDown = 0x22;
    private const int VkEnd = 0x23;
    private const int VkHome = 0x24;
    private const int VkLeft = 0x25;
    private const int VkUp = 0x26;
    private const int VkRight = 0x27;
    private const int VkDown = 0x28;
    private const int VkInsert = 0x2D;
    private const int VkDelete = 0x2E;
    private const int VkLWin = 0x5B;
    private const int VkRWin = 0x5C;
    private const int VkF1 = 0x70;
    private const int VkF12 = 0x7B;

    private static bool IsKeyDown(int virtualKey) => (GetKeyState(virtualKey) & 0x8000) != 0;

    private static (int X, int Y) GetPoint(IntPtr lParam) =>
        (GetLowWordSigned(lParam), GetHighWordSigned(lParam));

    private static (int X, int Y) GetWheelPoint(IntPtr hwnd, IntPtr lParam)
    {
        var point = new NativePoint
        {
            X = GetLowWordSigned(lParam),
            Y = GetHighWordSigned(lParam)
        };
        return ScreenToClient(hwnd, ref point)
            ? (point.X, point.Y)
            : (point.X, point.Y);
    }

    private static int GetLowWordSigned(IntPtr value) => unchecked((short)(value.ToInt64() & 0xFFFF));

    private static int GetHighWordSigned(IntPtr value) => unchecked((short)((value.ToInt64() >> 16) & 0xFFFF));

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public int X;
        public int Y;
    }

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

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyWindow(IntPtr hwnd);

    [DllImport("user32.dll")]
    private static extern IntPtr SetFocus(IntPtr hwnd);

    [DllImport("user32.dll")]
    private static extern IntPtr SetCapture(IntPtr hwnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ReleaseCapture();

    [DllImport("user32.dll")]
    private static extern short GetKeyState(int virtualKey);

    [DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(IntPtr hwnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ScreenToClient(IntPtr hwnd, ref NativePoint point);
}
