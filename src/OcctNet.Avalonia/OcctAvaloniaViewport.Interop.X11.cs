using System.Runtime.InteropServices;

namespace OcctNet;

public sealed partial class OcctAvaloniaViewport
{
    private const long X11KeyPressMask = 1L << 0;
    private const long X11KeyReleaseMask = 1L << 1;
    private const long X11ButtonPressMask = 1L << 2;
    private const long X11ButtonReleaseMask = 1L << 3;
    private const long X11PointerMotionMask = 1L << 6;
    private const long X11ExposureMask = 1L << 15;
    private const long X11StructureNotifyMask = 1L << 17;

    private const int X11KeyPress = 2;
    private const int X11KeyRelease = 3;
    private const int X11ButtonPress = 4;
    private const int X11ButtonRelease = 5;
    private const int X11MotionNotify = 6;
    private const int X11Expose = 12;
    private const int X11ConfigureNotify = 22;

    private const uint X11ShiftMask = 1U << 0;
    private const uint X11ControlMask = 1U << 2;
    private const uint X11Mod1Mask = 1U << 3;
    private const uint X11Mod4Mask = 1U << 6;
    private const uint X11Button1Mask = 1U << 8;
    private const uint X11Button2Mask = 1U << 9;
    private const uint X11Button3Mask = 1U << 10;

    private const uint X11Button1 = 1;
    private const uint X11Button2 = 2;
    private const uint X11Button3 = 3;
    private const uint X11Button4 = 4;
    private const uint X11Button5 = 5;
    private const int X11MaxEventsPerTick = 128;
    private const int X11RevertToParent = 2;
    private static readonly nuint X11CurrentTime = 0;

    private const ulong XkBackSpace = 0xFF08;
    private const ulong XkTab = 0xFF09;
    private const ulong XkReturn = 0xFF0D;
    private const ulong XkEscape = 0xFF1B;
    private const ulong XkHome = 0xFF50;
    private const ulong XkLeft = 0xFF51;
    private const ulong XkUp = 0xFF52;
    private const ulong XkRight = 0xFF53;
    private const ulong XkDown = 0xFF54;
    private const ulong XkPageUp = 0xFF55;
    private const ulong XkPageDown = 0xFF56;
    private const ulong XkEnd = 0xFF57;
    private const ulong XkInsert = 0xFF63;
    private const ulong XkF1 = 0xFFBE;
    private const ulong XkF12 = 0xFFC9;
    private const ulong XkShiftL = 0xFFE1;
    private const ulong XkShiftR = 0xFFE2;
    private const ulong XkControlL = 0xFFE3;
    private const ulong XkControlR = 0xFFE4;
    private const ulong XkMetaL = 0xFFE7;
    private const ulong XkMetaR = 0xFFE8;
    private const ulong XkAltL = 0xFFE9;
    private const ulong XkAltR = 0xFFEA;
    private const ulong XkDelete = 0xFFFF;

    [StructLayout(LayoutKind.Explicit, Size = 192)]
    private struct X11Event
    {
        [FieldOffset(0)] public int Type;
        [FieldOffset(0)] public X11KeyEvent Key;
        [FieldOffset(0)] public X11ButtonEvent Button;
        [FieldOffset(0)] public X11MotionEvent Motion;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct X11KeyEvent
    {
        public int Type;
        public nuint Serial;
        public int SendEvent;
        public IntPtr Display;
        public nuint Window;
        public nuint Root;
        public nuint Subwindow;
        public nuint Time;
        public int X;
        public int Y;
        public int XRoot;
        public int YRoot;
        public uint State;
        public uint Keycode;
        public int SameScreen;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct X11ButtonEvent
    {
        public int Type;
        public nuint Serial;
        public int SendEvent;
        public IntPtr Display;
        public nuint Window;
        public nuint Root;
        public nuint Subwindow;
        public nuint Time;
        public int X;
        public int Y;
        public int XRoot;
        public int YRoot;
        public uint State;
        public uint Button;
        public int SameScreen;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct X11MotionEvent
    {
        public int Type;
        public nuint Serial;
        public int SendEvent;
        public IntPtr Display;
        public nuint Window;
        public nuint Root;
        public nuint Subwindow;
        public nuint Time;
        public int X;
        public int Y;
        public int XRoot;
        public int YRoot;
        public uint State;
        public byte IsHint;
        public int SameScreen;
    }

    [DllImport("libX11.so.6")]
    private static extern IntPtr XOpenDisplay(IntPtr displayName);

    [DllImport("libX11.so.6")]
    private static extern int XDefaultScreen(IntPtr display);

    [DllImport("libX11.so.6")]
    private static extern nuint XBlackPixel(IntPtr display, int screenNumber);

    [DllImport("libX11.so.6")]
    private static extern nuint XCreateSimpleWindow(
        IntPtr display,
        nuint parent,
        int x,
        int y,
        uint width,
        uint height,
        uint borderWidth,
        nuint border,
        nuint background);

    [DllImport("libX11.so.6")]
    private static extern int XMapWindow(IntPtr display, nuint window);

    [DllImport("libX11.so.6")]
    private static extern int XSelectInput(IntPtr display, nuint window, nint eventMask);

    [DllImport("libX11.so.6")]
    private static extern int XPending(IntPtr display);

    [DllImport("libX11.so.6")]
    private static extern int XNextEvent(IntPtr display, out X11Event nativeEvent);

    [DllImport("libX11.so.6")]
    private static extern nuint XLookupKeysym(ref X11KeyEvent keyEvent, int index);

    [DllImport("libX11.so.6")]
    private static extern int XSetInputFocus(IntPtr display, nuint focus, int revertTo, nuint time);

    [DllImport("libX11.so.6")]
    private static extern int XDestroyWindow(IntPtr display, nuint window);

    [DllImport("libX11.so.6")]
    private static extern int XFlush(IntPtr display);

    [DllImport("libX11.so.6")]
    private static extern int XCloseDisplay(IntPtr display);
}
